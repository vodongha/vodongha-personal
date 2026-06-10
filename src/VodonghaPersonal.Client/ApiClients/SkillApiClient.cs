using System.Net.Http.Json;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class SkillApiClient(HttpClient http)
{
    public async Task<List<Skill>> GetAllAsync()
        => await http.GetFromJsonAsync<List<Skill>>("/api/admin/skills") ?? [];

    public async Task<Skill?> SaveAsync(Skill skill)
    {
        HttpResponseMessage resp = skill.Id == 0
            ? await http.PostAsJsonAsync("/api/admin/skills", skill)
            : await http.PutAsJsonAsync($"/api/admin/skills/{skill.Id}", skill);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Skill>();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/skills/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
