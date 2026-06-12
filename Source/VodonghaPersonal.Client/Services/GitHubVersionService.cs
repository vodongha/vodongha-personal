using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace VodonghaPersonal.Client.Services;

public class GitHubVersionService
{
    private readonly HttpClient _http;
    private string? _cached;

    public GitHubVersionService()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent", "vodongha-personal");
    }

    public async Task<string> GetLatestVersionAsync()
    {
        if (_cached != null)
        {
            return _cached;
        }

        try
        {
            GitHubRelease? release = await _http.GetFromJsonAsync<GitHubRelease>(
                "https://api.github.com/repos/vodongha/vodongha-personal/releases/latest");
            _cached = release?.TagName ?? "—";
        }
        catch
        {
            _cached = "—";
        }

        return _cached;
    }

    private record GitHubRelease([property: JsonPropertyName("tag_name")] string? TagName);
}
