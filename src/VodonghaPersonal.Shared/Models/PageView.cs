namespace VodonghaPersonal.Shared.Models;

public class PageView
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Referrer { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
