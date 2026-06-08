namespace vodongha.Data.Models;

public class PushSubscription
{
    public int Id { get; set; }

    /// <summary>Browser-assigned push endpoint URL (unique per browser/device).</summary>
    public string Endpoint { get; set; } = "";

    public string P256DH { get; set; } = "";
    public string Auth { get; set; } = "";

    /// <summary>
    /// Linked chat session. Null = admin subscription (device belonging to the site owner).
    /// Non-null = visitor subscription for a specific chat session.
    /// </summary>
    public int? ChatSessionId { get; set; }

    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
