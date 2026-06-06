namespace vodongha.Data.Models;

public class Experience
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int StartYear { get; set; }
    public int StartMonth { get; set; }
    public int? EndYear { get; set; }
    public int? EndMonth { get; set; }
    public bool IsCurrent { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public int Order { get; set; }
}
