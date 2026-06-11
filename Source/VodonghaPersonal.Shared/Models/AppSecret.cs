namespace VodonghaPersonal.Shared.Models;

public class AppSecret
{
    public Guid Id { get; set; }
    public string Key { get; set; } = "";   // e.g. "Fly:ApiToken"
    public string Value { get; set; } = "";   // AES-encrypted via IDataProtector
    public string DisplayName { get; set; } = "";   // e.g. "Fly.io API Token"
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";   // e.g. "Fly.io"
    public bool IsSensitive { get; set; } = true; // mask value in UI
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
