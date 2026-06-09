using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

/// <summary>
/// Calls Google Gemini API with a context snapshot of the portfolio owner's data.
/// Context is cached for 30 minutes to avoid repeated DB reads.
/// </summary>
public class AiService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AppSecretsService _secrets;
    private readonly HttpClient _http;
    private readonly ILogger<AiService> _logger;

    private string? _cachedContext;
    private DateTime _contextBuiltAt = DateTime.MinValue;
    private readonly SemaphoreSlim _contextLock = new(1, 1);
    private static readonly TimeSpan ContextTtl = TimeSpan.FromMinutes(30);

    public AiService(
        IDbContextFactory<AppDbContext> dbFactory,
        AppSecretsService secrets,
        HttpClient http,
        ILogger<AiService> logger)
    {
        _dbFactory = dbFactory;
        _secrets   = secrets;
        _http      = http;
        _logger    = logger;
    }

    public record AiMessage(string Role, string Text);

    /// <summary>
    /// Sends the conversation history to Gemini and returns the model reply.
    /// The last item in <paramref name="history"/> must be the user message to answer.
    /// Returns null when the API key is missing or the call fails.
    /// </summary>
    public async Task<string?> AskAsync(IReadOnlyList<AiMessage> history, CancellationToken ct = default)
    {
        string? apiKey = await _secrets.GetValueAsync("Gemini:ApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API key not configured");
            return null;
        }

        string model   = (await _secrets.GetValueAsync("Gemini:Model")) ?? "gemini-2.0-flash";
        string context = await GetContextAsync();

        List<object> contents = history
            .Select(m => (object)new
            {
                role  = m.Role,
                parts = new[] { new { text = m.Text } }
            })
            .ToList();

        object body = new
        {
            system_instruction = new { parts = new[] { new { text = context } } },
            contents,
            generationConfig = new
            {
                maxOutputTokens = 600,
                temperature     = 0.75,
                topK            = 40,
                topP            = 0.95
            }
        };

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        using StringContent payload = new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage res = await _http.PostAsync(url, payload, ct);
            string raw = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini {Status}: {Body}", res.StatusCode, raw[..Math.Min(raw.Length, 300)]);
                return null;
            }

            using JsonDocument doc = JsonDocument.Parse(raw);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed");
            return null;
        }
    }

    /// <summary>Invalidates the context cache (call after admin updates skills/projects/etc.).</summary>
    public void InvalidateContext() => _cachedContext = null;

    // ─── Context builder ──────────────────────────────────────────────────────

    private async Task<string> GetContextAsync()
    {
        if (_cachedContext != null && DateTime.UtcNow - _contextBuiltAt < ContextTtl)
        {
            return _cachedContext;
        }

        await _contextLock.WaitAsync();
        try
        {
            if (_cachedContext != null && DateTime.UtcNow - _contextBuiltAt < ContextTtl)
            {
                return _cachedContext;
            }

            _cachedContext   = await BuildContextAsync();
            _contextBuiltAt  = DateTime.UtcNow;
            return _cachedContext;
        }
        finally
        {
            _contextLock.Release();
        }
    }

    private async Task<string> BuildContextAsync()
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

        Dictionary<string, string> settings = await db.SiteSettings
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        List<Skill> skills = await db.Skills
            .OrderBy(s => s.Category).ThenBy(s => s.Order)
            .ToListAsync();

        List<Experience> experiences = await db.Experiences
            .OrderBy(e => e.Order)
            .ToListAsync();

        List<Education> educations = await db.Educations
            .OrderBy(e => e.Order)
            .ToListAsync();

        List<Project> projects = await db.Projects
            .Where(p => p.IsFeatured)
            .OrderBy(p => p.Order)
            .Take(10)
            .ToListAsync();

        StringBuilder sb = new();

        sb.AppendLine("You are an AI assistant on Võ Đông Hà's personal portfolio website.");
        sb.AppendLine("Answer visitors' questions about Võ Đông Hà using ONLY the context below.");
        sb.AppendLine("Be friendly, concise, and professional. Keep answers under 150 words unless more detail is requested.");
        sb.AppendLine("Respond in the SAME language the visitor uses — Vietnamese if they write in Vietnamese, English otherwise.");
        sb.AppendLine("If asked something outside this context, say honestly that you don't have that information.");
        sb.AppendLine("Never fabricate information not present in the context.");
        sb.AppendLine();

        sb.AppendLine("=== ABOUT ===");
        Setting(sb, settings, "Name");
        Setting(sb, settings, "Title");
        Setting(sb, settings, "Tagline");
        Setting(sb, settings, "Location");
        Setting(sb, settings, "Email");
        Setting(sb, settings, "GitHub");
        Setting(sb, settings, "LinkedIn");
        Setting(sb, settings, "Facebook");
        if (settings.TryGetValue("Bio",   out string? bioVi))  { sb.AppendLine($"Bio (Vietnamese): {bioVi}"); }
        if (settings.TryGetValue("BioEn", out string? bioEn))  { sb.AppendLine($"Bio (English): {bioEn}"); }
        sb.AppendLine();

        if (skills.Count > 0)
        {
            sb.AppendLine("=== SKILLS ===");
            foreach (IGrouping<string, Skill> group in skills.GroupBy(s => s.Category))
            {
                sb.AppendLine($"{group.Key}: {string.Join(", ", group.Select(s => s.Name))}");
            }
            sb.AppendLine();
        }

        if (experiences.Count > 0)
        {
            sb.AppendLine("=== WORK EXPERIENCE ===");
            foreach (Experience exp in experiences)
            {
                string start = $"{exp.StartMonth:D2}/{exp.StartYear}";
                string end   = exp.IsCurrent ? "Present" : (exp.EndMonth.HasValue && exp.EndYear.HasValue
                    ? $"{exp.EndMonth:D2}/{exp.EndYear}"
                    : exp.EndYear?.ToString() ?? "Present");
                sb.AppendLine($"{exp.Company} | {exp.Role} | {start} – {end}");
                string? desc = exp.DescriptionEn ?? exp.Description;
                if (!string.IsNullOrWhiteSpace(desc)) { sb.AppendLine(desc); }
                sb.AppendLine();
            }
        }

        if (educations.Count > 0)
        {
            sb.AppendLine("=== EDUCATION ===");
            foreach (Education edu in educations)
            {
                string end = edu.EndYear?.ToString() ?? "Present";
                sb.AppendLine($"{edu.School} | {edu.Degree} in {edu.Field} | {edu.StartYear} – {end}");
                string? desc = edu.DescriptionEn ?? edu.Description;
                if (!string.IsNullOrWhiteSpace(desc)) { sb.AppendLine(desc); }
            }
            sb.AppendLine();
        }

        if (projects.Count > 0)
        {
            sb.AppendLine("=== PROJECTS ===");
            foreach (Project proj in projects)
            {
                sb.AppendLine($"• {proj.Title}");
                string? desc = proj.DescriptionEn ?? proj.Description;
                if (!string.IsNullOrWhiteSpace(desc)) { sb.AppendLine($"  {desc}"); }
                if (!string.IsNullOrWhiteSpace(proj.Technologies))
                { sb.AppendLine($"  Tech: {proj.Technologies}"); }
                if (!string.IsNullOrWhiteSpace(proj.LiveUrl))
                { sb.AppendLine($"  Live: {proj.LiveUrl}"); }
            }
        }

        return sb.ToString();
    }

    private static void Setting(StringBuilder sb, Dictionary<string, string> settings, string key)
    {
        if (settings.TryGetValue(key, out string? val) && !string.IsNullOrWhiteSpace(val))
        {
            sb.AppendLine($"{key}: {val}");
        }
    }
}
