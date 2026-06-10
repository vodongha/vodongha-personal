using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class SettingsApiClient(HttpClient http)
{
    public async Task<Dictionary<string, string>> GetAllAsync()
        => await http.GetFromJsonAsync<Dictionary<string, string>>("/api/admin/settings") ?? [];

    public async Task SaveAllAsync(Dictionary<string, string> values)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync("/api/admin/settings", new SettingsSaveRequest(values));
        resp.EnsureSuccessStatusCode();
    }

    public async Task<string?> UploadAvatarAsync(Stream fileStream, string fileName, string contentType)
    {
        using MultipartFormDataContent content = new();
        content.Add(new StreamContent(fileStream), "file", fileName);
        HttpResponseMessage resp = await http.PostAsync("/api/admin/settings/avatar", content);
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<AvatarUploadResult>();
        return result?.Url;
    }

    private record AvatarUploadResult(string Url);
}
