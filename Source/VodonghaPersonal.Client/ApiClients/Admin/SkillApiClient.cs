using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class SkillApiClient(HttpClient http) : BaseCrudApiClient<Skill>(http, "/api/admin/skills")
{
    public Task<Skill?> SaveAsync(Skill skill) => SaveAsync(skill, skill.Rid);
}
