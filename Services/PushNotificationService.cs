using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using vodongha.Data;
using vodongha.Data.Models;
using WebPush;
using PushSubscriptionModel = vodongha.Data.Models.PushSubscription;

namespace vodongha.Services;

public class PushNotificationService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly WebPushClient _client;
    private readonly VapidDetails _vapid;

    public string PublicKey { get; }

    public PushNotificationService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<PushNotificationService> logger,
        IConfiguration config)
    {
        _dbFactory = dbFactory;
        _logger = logger;

        PublicKey = config["Push:VapidPublicKey"] ?? "";
        string privateKey = config["Push:VapidPrivateKey"] ?? "";
        string subject = config["Push:VapidSubject"] ?? "mailto:admin@example.com";

        _vapid = new VapidDetails(subject, PublicKey, privateKey);
        _client = new WebPushClient();
    }

    // ── Subscription management ───────────────────────────────────────────────

    public async Task SaveSubscriptionAsync(string endpoint, string p256dh, string auth,
        int? chatSessionId, bool isAdmin)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

        // Upsert by endpoint — same browser re-subscribing updates keys
        PushSubscriptionModel? existing = await db.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == endpoint);

        if (existing is not null)
        {
            existing.P256DH = p256dh;
            existing.Auth = auth;
            existing.ChatSessionId = chatSessionId;
            existing.IsAdmin = isAdmin;
        }
        else
        {
            db.PushSubscriptions.Add(new PushSubscriptionModel
            {
                Endpoint = endpoint,
                P256DH = p256dh,
                Auth = auth,
                ChatSessionId = chatSessionId,
                IsAdmin = isAdmin,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RemoveSubscriptionAsync(string endpoint)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        await db.PushSubscriptions.Where(s => s.Endpoint == endpoint).ExecuteDeleteAsync();
    }

    // ── Send helpers ──────────────────────────────────────────────────────────

    /// <summary>Send push to all admin subscriptions.</summary>
    public async Task SendToAdminsAsync(string title, string body, string url)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        List<PushSubscriptionModel> subs = await db.PushSubscriptions
            .Where(s => s.IsAdmin)
            .ToListAsync();

        await SendBatchAsync(subs, title, body, url);
    }

    /// <summary>Send push to the visitor subscription linked to a chat session.</summary>
    public async Task SendToSessionAsync(int sessionId, string title, string body, string url)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        List<PushSubscriptionModel> subs = await db.PushSubscriptions
            .Where(s => s.ChatSessionId == sessionId && !s.IsAdmin)
            .ToListAsync();

        await SendBatchAsync(subs, title, body, url);
    }

    private async Task SendBatchAsync(List<PushSubscriptionModel> subs, string title, string body, string url)
    {
        if (subs.Count == 0)
        {
            return;
        }

        string payload = JsonSerializer.Serialize(new { title, body, url });

        List<string> staleEndpoints = [];

        foreach (PushSubscriptionModel sub in subs)
        {
            try
            {
                WebPush.PushSubscription pushSub = new(sub.Endpoint, sub.P256DH, sub.Auth);
                await _client.SendNotificationAsync(pushSub, payload, _vapid);
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone
                                            || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Subscription expired or revoked — clean up
                staleEndpoints.Add(sub.Endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send push to {Endpoint}", sub.Endpoint);
            }
        }

        if (staleEndpoints.Count > 0)
        {
            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
            await db.PushSubscriptions
                .Where(s => staleEndpoints.Contains(s.Endpoint))
                .ExecuteDeleteAsync();
        }
    }
}
