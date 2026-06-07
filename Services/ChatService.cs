using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;
using vodongha.Hubs;

namespace vodongha.Services;

public class ChatService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly TelegramService _telegram;
    private readonly IHubContext<ChatHub> _hub;

    public ChatService(IDbContextFactory<AppDbContext> dbFactory, TelegramService telegram, IHubContext<ChatHub> hub)
    {
        _dbFactory = dbFactory;
        _telegram = telegram;
        _hub = hub;
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
                _ = _telegram.SendMessageAsync(info, topicId.Value);
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

        // Forward to Telegram in the background — failure is non-critical
        if (session.TelegramTopicId != null)
        {
            long topicId = session.TelegramTopicId.Value;
            _ = _telegram.SendMessageAsync($"💬 {content}", topicId);
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

        // Forward to Telegram in the background — failure is non-critical
        if (session.TelegramTopicId != null)
        {
            long topicId = session.TelegramTopicId.Value;
            _ = _telegram.SendMessageAsync($"🔵 [Admin] {content}", topicId);
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
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        ChatSession? session = await db.ChatSessions.FindAsync(sessionId);
        if (session?.TelegramTopicId != null)
        {
            await _telegram.SendTypingAsync(session.TelegramTopicId.Value);
        }
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
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
