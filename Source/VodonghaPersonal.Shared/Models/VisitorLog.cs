namespace VodonghaPersonal.Shared.Models;

public class VisitorLog
{
    public int Id { get; set; }
    public Guid Rid { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    public string? UserAgent { get; set; }
}
