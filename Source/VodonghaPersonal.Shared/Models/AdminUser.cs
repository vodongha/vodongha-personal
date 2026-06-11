namespace VodonghaPersonal.Shared.Models;

public class AdminUser
{
    public int Id { get; set; }
    public Guid Rid { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
