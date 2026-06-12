using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class ExperienceApiClient(HttpClient http) : BaseCrudApiClient<Experience>(http, "/api/admin/experience")
{
    public Task<Experience?> SaveAsync(Experience item) => SaveAsync(item, item.Id == 0 ? Guid.Empty : item.Rid);
}
