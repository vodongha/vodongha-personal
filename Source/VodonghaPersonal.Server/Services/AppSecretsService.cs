using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

/// <summary>
/// Manages application API keys/secrets stored in the database.
/// Values are encrypted with ASP.NET Data Protection before persisting.
/// At runtime, services call GetValue(key) which returns the DB override first,
/// then falls back to IConfiguration (environment variables / .env on the host).
/// </summary>
public class AppSecretsService
{
    // ─── Well-known keys ─────────────────────────────────────────────────────

    public static readonly IReadOnlyList<AppSecretDefinition> Definitions =
    [
        new("Neon:ApiKey",        "Neon API Key",         "API key for Neon PostgreSQL management API",        "Neon",     Sensitive: true),
        new("Neon:ProjectId",     "Neon Project ID",      "Project ID from Neon console (e.g. red-fire-...)", "Neon",     Sensitive: false),
        new("Telegram:BotToken",  "Telegram Bot Token",   "Bot token from @BotFather for chat notifications",  "Telegram", Sensitive: true),
        new("Telegram:ChatId",    "Telegram Chat ID",     "Chat / channel ID where notifications are sent",    "Telegram", Sensitive: false),
        new("Email:ResendApiKey",   "Resend API Key",        "API key for Resend transactional email service",                          "Email",    Sensitive: true),
        new("Email:NotifyTo",       "Notification Email",    "Email address that receives contact notifications",                       "Email",    Sensitive: false),
        new("Push:VapidPublicKey",  "VAPID Public Key",      "Web Push public key (base64url). Generate once — changing breaks all existing subscriptions.", "Web Push", Sensitive: false),
        new("Push:VapidPrivateKey", "VAPID Private Key",     "Web Push private key (base64url). Keep secret.",                         "Web Push", Sensitive: true),
        new("Push:VapidSubject",         "VAPID Subject",         "Contact email for Web Push (e.g. mailto:you@example.com)",               "Web Push", Sensitive: false),
        new("Gemini:ApiKey",        "Gemini API Key",        "Google Gemini API key for AI feature. Free at https://aistudio.google.com/apikey", "Gemini", Sensitive: true),
        new("Gemini:Model",         "Gemini Model",          "Model name (default: gemini-2.0-flash). Leave blank to use default.",              "Gemini", Sensitive: false),
    ];

    // ─── Private fields ───────────────────────────────────────────────────────

    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDataProtector _protector;
    private readonly IConfiguration _config;
    private readonly ILogger<AppSecretsService> _logger;

    // In-memory cache: key → plaintext value (from DB)
    private readonly Dictionary<string, string> _cache = [];
    // volatile ensures cross-thread visibility — reads outside the semaphore
    // see a consistent value once the load completes inside the lock.
    private volatile bool _loaded = false;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    private const string ProtectorPurpose = "AppSecrets.v1";

    public AppSecretsService(
        IDbContextFactory<AppDbContext> dbFactory,
        IDataProtectionProvider dataProtection,
        IConfiguration config,
        ILogger<AppSecretsService> logger)
    {
        _dbFactory = dbFactory;
        _protector = dataProtection.CreateProtector(ProtectorPurpose);
        _config = config;
        _logger = logger;
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>Returns the value for the given key: DB override first, then IConfiguration.</summary>
    public async Task<string?> GetValueAsync(string key)
    {
        await EnsureLoadedAsync();
        if (_cache.TryGetValue(key, out string? dbValue))
        {
            return dbValue;
        }

        return _config[key];
    }

    /// <summary>Sync version — safe to call only after EnsureLoadedAsync has run once (e.g. in constructors after startup).</summary>
    public string? GetValue(string key)
    {
        if (_cache.TryGetValue(key, out string? dbValue))
        {
            return dbValue;
        }

        return _config[key];
    }

    /// <summary>Returns all DB-stored secrets (decrypted) for display in the admin UI.</summary>
    public async Task<List<AppSecret>> GetAllAsync()
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        List<AppSecret> rows = await db.AppSecrets.OrderBy(a => a.Category).ThenBy(a => a.Key).ToListAsync();

        // Decrypt values for the admin layer
        foreach (AppSecret row in rows)
        {
            try
            {
                row.Value = _protector.Unprotect(row.Value);
            }
            catch
            {
                row.Value = ""; // decryption failed — treat as empty
            }
        }

        return rows;
    }

    /// <summary>Saves or updates a secret. Empty value removes the DB override (falls back to env var).</summary>
    public async Task<bool> SaveAsync(string key, string plaintextValue)
    {
        try
        {
            AppSecretDefinition? def = Definitions.FirstOrDefault(d => d.Key == key);

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
            AppSecret? existing = await db.AppSecrets.FirstOrDefaultAsync(a => a.Key == key);

            if (string.IsNullOrWhiteSpace(plaintextValue))
            {
                // Empty → remove DB override
                if (existing != null)
                {
                    db.AppSecrets.Remove(existing);
                    await db.SaveChangesAsync();
                }

                lock (_cache) { _cache.Remove(key); }
                return true;
            }

            string encrypted = _protector.Protect(plaintextValue);

            if (existing == null)
            {
                db.AppSecrets.Add(new AppSecret
                {
                    Key = key,
                    Value = encrypted,
                    DisplayName = def?.DisplayName ?? key,
                    Description = def?.Description ?? "",
                    Category = def?.Category ?? "Other",
                    IsSensitive = def?.Sensitive ?? true,
                    UpdatedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.Value = encrypted;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            // Update in-memory cache immediately — no restart needed
            lock (_cache) { _cache[key] = plaintextValue; }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save secret {Key}", key);
            return false;
        }
    }

    /// <summary>Returns true if a DB override exists for this key.</summary>
    public async Task<bool> HasOverrideAsync(string key)
    {
        await EnsureLoadedAsync();
        return _cache.ContainsKey(key);
    }

    /// <summary>Force reload from DB (e.g. after external change).</summary>
    public void InvalidateCache()
    {
        lock (_cache) { _loaded = false; }
    }

    // ─── Private ─────────────────────────────────────────────────────────────

    private async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        await _loadLock.WaitAsync();
        try
        {
            if (_loaded)
            {
                return;
            }

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
            List<AppSecret> rows = await db.AppSecrets.ToListAsync();

            lock (_cache)
            {
                _cache.Clear();
                foreach (AppSecret row in rows)
                {
                    try
                    {
                        _cache[row.Key] = _protector.Unprotect(row.Value);
                    }
                    catch
                    {
                        // Ignore rows with corrupt encrypted values
                    }
                }

                _loaded = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load app secrets from database");
        }
        finally
        {
            _loadLock.Release();
        }
    }
}

/// <summary>Static definition of a known secret key.</summary>
public record AppSecretDefinition(
    string Key,
    string DisplayName,
    string Description,
    string Category,
    bool Sensitive
);
