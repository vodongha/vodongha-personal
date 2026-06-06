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

        // Create Telegram topic on first message
        if (session.TelegramTopicId == null)
        {
            string topicTitle = $"{session.Name} | {session.Email}";
            long? topicId = await _telegram.CreateTopicAsync(topicTitle);
            if (topicId != null)
            {
                session.TelegramTopicId = topicId;
                // Send contact info as first message in topic
                string info = $"👤 {session.Name}\n📞 {session.Phone}\n📧 {session.Email}";
                await _telegram.SendMessageAsync(info, topicId.Value);
            }
        }

        ChatMessage message = new()
        {
            ChatSessionId = sessionId,
            Content = content,
            IsFromUser = true,
            SentAt = DateTime.UtcNow
        };

        // Send to Telegram
        if (session.TelegramTopicId != null)
        {
            long? msgId = await _telegram.SendMessageAsync($"💬 {content}", session.TelegramTopicId.Value);
            message.TelegramMessageId = msgId;
        }

        session.LastMessageAt = DateTime.UtcNow;
        session.HasUnread = true;
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

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

        // Also send to Telegram topic so admin sees it there too
        if (session.TelegramTopicId != null)
        {
            long? msgId = await _telegram.SendMessageAsync($"🔵 [Admin] {content}", session.TelegramTopicId.Value);
            message.TelegramMessageId = msgId;
        }

        session.LastMessageAt = DateTime.UtcNow;
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        // Push to chat widget in real-time
        await _hub.Clients.Group($"session_{sessionId}")
            .SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                content = message.Content,
                isFromUser = message.IsFromUser,
                sentAt = message.SentAt
            });

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
        session.HasUnread = true;
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

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
