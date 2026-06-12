using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Components.Shared;

public partial class AdminFooter : ComponentBase
{
    [Inject] private SettingsApiClient SettingsApi { get; set; } = default!;
    [Inject] private GitHubVersionService GithubVersion { get; set; } = default!;

    private string _name = "";
    private string _version = "—";

    protected override async Task OnInitializedAsync()
    {
        Dictionary<string, string> settings = await SettingsApi.GetAllAsync();
        _name = settings.GetValueOrDefault("Name", "");
        _version = await GithubVersion.GetLatestVersionAsync();
    }
}
