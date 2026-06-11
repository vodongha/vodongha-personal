namespace VodonghaPersonal.Shared.Models;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ChatSessionId { get; set; }
    public string Content { get; set; } = "";
    public bool IsFromUser { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public long? TelegramMessageId { get; set; }

    public ChatSession Session { get; set; } = null!;
}
