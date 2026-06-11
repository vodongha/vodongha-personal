namespace VodonghaPersonal.Shared.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public Guid Rid { get; set; } = Guid.NewGuid();
    public int ChatSessionId { get; set; }
    public string Content { get; set; } = "";
    public bool IsFromUser { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public long? TelegramMessageId { get; set; }

    public ChatSession Session { get; set; } = null!;
}
