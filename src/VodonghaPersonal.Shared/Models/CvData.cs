namespace VodonghaPersonal.Shared.Models;

public record CvData(
    string Name,
    string Title,
    string Email,
    string Phone,
    string Location,
    string GitHub,
    string LinkedIn,
    string Bio,
    string AvatarUrl,
    List<Skill> Skills,
    List<Experience> Experiences,
    List<Education> Educations,
    List<Project> Projects
);
