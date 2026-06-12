using System.Net.Http.Json;

namespace VodonghaPersonal.Client.Services;

public class GitHubVersionService(HttpClient http)
{
    private string? _cached;

    public async Task<string> GetLatestVersionAsync()
    {
        if (_cached != null)
        {
            return _cached;
        }

        try
        {
            VersionResponse? response = await http.GetFromJsonAsync<VersionResponse>("/api/admin/version");
            _cached = response?.Version ?? "—";
        }
        catch
        {
            _cached = "—";
        }

        return _cached;
    }

    private record VersionResponse(string Version);
}
