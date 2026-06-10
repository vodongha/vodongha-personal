using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Data.Models;
using VodonghaPersonal.Hubs;

namespace VodonghaPersonal.Services;

public class ChatService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly TelegramService _telegram;
    private readonly IHubContext<ChatHub> _hub;
    private readonly PushNotificationService _push;
    private readonly ILogger<ChatService> _logger;

    // Short-lived cache: sessionId → TelegramTopicId, avoids a DB query on every typing event.
    // ChatService is scoped, so this cache lives for the duration of a single SignalR hub invocation.
    // A static dictionary is intentional: typing events come from different scopes; we want to share the cache.
    private static readonly ConcurrentDictionary<int, long?> s_topicIdCache = new();

    public ChatService(IDbContextFactory<AppDbContext> dbFactory, TelegramService telegram,
        IHubContext<ChatHub> hub, PushNotificationService push, ILogger<ChatService> logger)
    {
        _dbFactory = dbFactory;
        _telegram = telegram;
        _hub = hub;
        _push = push;
        _logger = logger;
    }

    /// <summary>
    /// Runs a task in the background, logging any exception rather than silently swallowing it.
    /// </summary>
    private void FireAndForget(Task task)
    {
        _ = task.ContinueWith(
            t => _logger.LogWarning(t.Exception, "Background task failed"),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    public async Task<ChatSession> CreateSessionAsync(string name, string phone, string email)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

        ChatSession session = new()
        {
            Name = name,
            Phone = phone,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };

        db.ChatSessions.Add(session);
        await db.SaveChangesAsync();

        // Automated welcome message — saved to DB so it loads when widget connects
        string firstName = name.Trim().Split(' ').Last(); // extract first name (last word)
        ChatMessage welcome = new()
        {
            ChatSessionId = session.Id,
            Content = $"Xin chào {firstName}! 👋 Mình là Hà — cứ nhắn bất cứ điều gì bạn cần, mình sẽ trả lời sớm nhất có thể.",
            IsFromUser = false,
            SentAt = DateTime.UtcNow,
        };
        db.ChatMessages.Add(welcome);
        await db.SaveChangesAsync();

        return session;
    }

    public async Task<ChatMessage> SendUserMessageAsync(int sessionId, string content)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

        ChatSession? session = await db.ChatSessions.FindAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found.");
        }

        // Create Telegram topic on first message — must await to get the topic ID
        if (session.TelegramTopicId == null)
        {
            string topicTitle = $"{session.Name} | {session.Email}";
            long? topicId = await _telegram.CreateTopicAsync(topicTitle);
            if (topicId != null)
            {
                session.TelegramTopicId = topicId;
                // Fire-and-forget the contact info pin — no need to block user
                string info = $"👤 {session.Name}\n📞 {session.Phone}\n📧 {session.Email}";
                FireAndForget(_telegram.SendMessageAsync(info, topicId.Value));
            }
        }

        ChatMessage message = new()
        {
            ChatSessionId = sessionId,
            Content = content,
            IsFromUser = true,
            SentAt = DateTime.UtcNow
        };

        // Save to DB first — fast path, no Telegram latency
        session.LastMessageAt = DateTime.UtcNow;
        session.HasUnread = true;
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        // Notify admin group immediately after DB save
        await _hub.Clients.Group("admin").SendAsync("SessionUpdated", session.Id);

        // Web push to admin devices (fire-and-forget — non-critical)
        _ = _push.SendToAdminsAsync("💬 Tin nhắn mới", $"{session.Name}: {content}", $"/admin/chats?session={session.Id}");

        // Forward to Telegram — if topic was deleted, recreate it
        if (session.TelegramTopicId != null)
        {
            _ = ForwardUserMessageToTelegramAsync(session, message.Content);
        }

        return message;
    }

    public async Task<ChatMessage> SendAdminReplyAsync(int sessionId, string content)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

        ChatSession? session = await db.ChatSessions.FindAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found.");
        }

        ChatMessage message = new()
        {
            ChatSessionId = sessionId,
            Content = content,
            IsFromUser = false,
            SentAt = DateTime.UtcNow
        };

        // Save to DB first — fast path
        session.LastMessageAt = DateTime.UtcNow;
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        // Push to chat widget immediately after DB save
        await _hub.Clients.Group($"session_{sessionId}")
            .SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                content = message.Content,
                isFromUser = message.IsFromUser,
                sentAt = message.SentAt
            });

        // Web push to visitor device (fire-and-forget — non-critical)
        _ = _push.SendToSessionAsync(sessionId, "💬 Bạn có tin nhắn mới", content, "/");

        // Forward to Telegram in the background — failure is non-critical
        if (session.TelegramTopicId != null)
        {
            long topicId = session.TelegramTopicId.Value;
            FireAndForget(_telegram.SendMessageAsync($"🔵 [Admin] {content}", topicId));
        }

        return message;
    }

    public async Task HandleTelegramWebhookAsync(TelegramUpdate update)
    {
        if (update.Message == null)
        {
            return;
        }

        long? threadId = update.Message.MessageThreadId;
        string? text = update.Message.Text;

        if (threadId == null || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // Ignore messages sent by the bot itself (admin replies from panel already saved)
        if (update.Message.From?.IsBot == true)
        {
            return;
        }

        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.TelegramTopicId == threadId);
        if (session == null)
        {
            return;
        }

        ChatMessage message = new()
        {
            ChatSessionId = session.Id,
            Content = text,
            IsFromUser = false,
            SentAt = DateTime.UtcNow,
            TelegramMessageId = update.Message.MessageId
        };

        session.LastMessageAt = DateTime.UtcNow;
        // Admin replied from Telegram — they clearly read the conversation, so mark as read
        session.HasUnread = false;
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        // Notify admin group so session list refreshes
        await _hub.Clients.Group("admin").SendAsync("SessionUpdated", session.Id);

        // Push to chat widget
        await _hub.Clients.Group($"session_{session.Id}")
            .SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                content = message.Content,
                isFromUser = message.IsFromUser,
                sentAt = message.SentAt
            });

        // Web push to visitor device (fire-and-forget)
        _ = _push.SendToSessionAsync(session.Id, "💬 Bạn có tin nhắn mới", "Nhấn để xem tin nhắn", "/");
    }

    /// <summary>
    /// Sends a user message to the Telegram topic.
    /// If the topic no longer exists (deleted on Telegram side), recreates it and retries once.
    /// </summary>
    private async Task ForwardUserMessageToTelegramAsync(ChatSession session, string content)
    {
        if (session.TelegramTopicId == null)
        {
            return;
        }

        (long? _, bool topicDeleted) = await _telegram.SendMessageAsync($"💬 {content}", session.TelegramTopicId.Value);

        if (!topicDeleted)
        {
            return;
        }

        // Topic was deleted on Telegram — recreate it and update DB
        string topicTitle = $"{session.Name} | {session.Email}";
        long? newTopicId = await _telegram.CreateTopicAsync(topicTitle);
        if (newTopicId == null)
        {
            return;
        }

        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        ChatSession? dbSession = await db.ChatSessions.FindAsync(session.Id);
        if (dbSession == null)
        {
            return;
        }

        dbSession.TelegramTopicId = newTopicId;
        await db.SaveChangesAsync();

        // Invalidate typing cache so it picks up the new topic ID
        s_topicIdCache[session.Id] = newTopicId;

        // Re-pin contact info in the new topic
        string info = $"👤 {session.Name}\n📞 {session.Phone}\n📧 {session.Email}\n⚠️ Topic was recreated — previous history is in the old topic.";
        await _telegram.SendMessageAsync(info, newTopicId.Value);

        // Retry sending the actual message
        await _telegram.SendMessageAsync($"💬 {content}", newTopicId.Value);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(int sessionId)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        return await db.ChatMessages
            .Where(m => m.ChatSessionId == sessionId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<ChatSession>> GetSessionsAsync()
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        return await db.ChatSessions
            .OrderByDescending(s => s.LastMessageAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetSessionAsync(int sessionId)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        return await db.ChatSessions.FindAsync(sessionId);
    }

    public async Task MarkSessionReadAsync(int sessionId)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        ChatSession? session = await db.ChatSessions.FindAsync(sessionId);
        if (session != null && session.HasUnread)
        {
            session.HasUnread = false;
            await db.SaveChangesAsync();
        }

        // Broadcast read receipt to widget: admin has read all user messages up to this ID
        int lastUserMsgId = await db.ChatMessages
            .Where(m => m.ChatSessionId == sessionId && m.IsFromUser)
            .Select(m => (int?)m.Id)
            .MaxAsync() ?? 0;

        if (lastUserMsgId > 0)
        {
            await _hub.Clients.Group($"session_{sessionId}").SendAsync("AdminRead", lastUserMsgId);
        }
    }

    public async Task SendTypingToTelegramAsync(int sessionId)
    {
        // Use the static cache to avoid a DB query on every keystroke.
        // Cache is invalidated when the topic is recreated in ForwardUserMessageToTelegramAsync.
        if (!s_topicIdCache.TryGetValue(sessionId, out long? topicId))
        {
            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
            topicId = (await db.ChatSessions.FindAsync(sessionId))?.TelegramTopicId;
            s_topicIdCache[sessionId] = topicId;
        }

        if (topicId.HasValue)
        {
            await _telegram.SendTypingAsync(topicId.Value);
        }
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

        // Delete the Telegram topic before wiping DB so we still have the TelegramTopicId
        ChatSession? session = await db.ChatSessions.FindAsync(sessionId);
        if (session?.TelegramTopicId != null)
        {
            await _telegram.DeleteTopicAsync(session.TelegramTopicId.Value);
        }

        await db.ChatMessages.Where(m => m.ChatSessionId == sessionId).ExecuteDeleteAsync();
        await db.ChatSessions.Where(s => s.Id == sessionId).ExecuteDeleteAsync();

        // Notify admin group so the session card disappears in real-time for all admin tabs
        await _hub.Clients.Group("admin").SendAsync("SessionDeleted", sessionId);
    }

    public async Task<int> GetUnreadCountAsync()
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        return await db.ChatSessions.CountAsync(s => s.HasUnread);
    }
}

// Telegram update DTOs
public class TelegramUpdate
{
    [System.Text.Json.Serialization.JsonPropertyName("update_id")]
    public long UpdateId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public TelegramMessage? Message { get; set; }
}

public class TelegramMessage
{
    [System.Text.Json.Serialization.JsonPropertyName("message_id")]
    public long MessageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("message_thread_id")]
    public long? MessageThreadId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("text")]
    public string? Text { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("from")]
    public TelegramUser? From { get; set; }
}

public class TelegramUser
{
    [System.Text.Json.Serialization.JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }
}
