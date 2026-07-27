namespace VodonghaPersonal.Shared.Models;

public class AppSecret
{
    public int Id { get; set; }
    public Guid Rid { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Description { get; set; }
    public string Category { get; set; } = "";
    public bool IsSensitive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
