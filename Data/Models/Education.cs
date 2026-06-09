namespace VodonghaPersonal.Data.Models;

public class Education
{
    public int Id { get; set; }
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public int Order { get; set; }
}
