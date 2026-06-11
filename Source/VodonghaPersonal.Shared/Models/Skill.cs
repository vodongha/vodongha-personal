namespace VodonghaPersonal.Shared.Models;

public class Skill
{
    public int Id { get; set; }
    public Guid Rid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Proficiency { get; set; }
    public int Order { get; set; }
}
