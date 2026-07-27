namespace VodonghaPersonal.Shared.Models;

public class Education
{
    public int Id { get; set; }
    public Guid Rid { get; set; } = Guid.NewGuid();
    public string School { get; set; } = string.Empty;
    public string? Degree { get; set; }
    public string? Field { get; set; }
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public int Order { get; set; }
}
