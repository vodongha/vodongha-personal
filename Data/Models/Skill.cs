namespace VodonghaPersonal.Data.Models;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Proficiency { get; set; } // 0-100
    public int Order { get; set; }
}
