namespace VodonghaPersonal.Shared.Models;

public class ChatSession
{
    public int Id { get; set; }
    public Guid Rid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool HasUnread { get; set; } = false;
    public long? TelegramTopicId { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
