using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class EducationApiClient(HttpClient http) : BaseCrudApiClient<Education>(http, "/api/admin/education")
{
    public Task<Education?> SaveAsync(Education item) => SaveAsync(item, item.Id == 0 ? Guid.Empty : item.Rid);
}
