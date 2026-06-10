using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminSettings : ComponentBase, IDisposable
{
    [Inject] private SettingsApiClient SettingsClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private bool _uploading;

    private async Task TriggerFilePicker()
    {
        await JS.InvokeVoidAsync("clickFileInput", "avatarFileInput");
    }

    private async Task OnAvatarFileChange(InputFileChangeEventArgs e)
    {
        IBrowserFile file = e.File;
        if (file is null) { return; }

        _uploading = true;
        try
        {
            await using Stream stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            string? url = await SettingsClient.UploadAvatarAsync(stream, file.Name, file.ContentType);
            if (url != null) { Val["AvatarUrl"] = url; }
            Toast.Show("Đã tải ảnh lên thành công");
        }
        catch (Exception ex)
        {
            Toast.Show($"Lỗi: {ex.Message}", success: false);
        }
        finally
        {
            _uploading = false;
        }
    }

    private Dictionary<string, string> Val = new()
    {
        ["Name"] = "", ["Title"] = "", ["Tagline"] = "", ["Bio"] = "", ["BioEn"] = "",
        ["Email"] = "", ["Phone"] = "", ["Location"] = "", ["GitHub"] = "",
        ["LinkedIn"] = "", ["Facebook"] = "", ["AvatarUrl"] = ""
    };

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        Dictionary<string, string> settings = await SettingsClient.GetAllAsync();
        foreach (KeyValuePair<string, string> s in settings) { Val[s.Key] = s.Value; }
    }

    private async Task SaveAll()
    {
        await SettingsClient.SaveAllAsync(Val);
        Toast.Show("Đã lưu cài đặt thành công");
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
