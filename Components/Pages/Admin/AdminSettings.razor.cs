using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.JSInterop;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminSettings : ComponentBase, IDisposable
{
    [Inject] private SiteSettingService SettingsSvc { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private IWebHostEnvironment Env { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ChatService ChatSvc { get; set; } = default!;

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
            string ext = Path.GetExtension(file.Name).ToLowerInvariant();
            string fileName = $"avatar{ext}";
            string uploadsDir = Path.Combine(Env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);
            string filePath = Path.Combine(uploadsDir, fileName);

            await using FileStream fs = new(filePath, FileMode.Create);
            await file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024).CopyToAsync(fs);

            Val["AvatarUrl"] = $"/uploads/{fileName}";
            Toast.Show("Đã tải ảnh lên thành công");
        }
        catch (Exception ex)
        {
            Toast.Show($"Lỗi: {ex.Message}");
        }
        finally
        {
            _uploading = false;
        }
    }

    private Dictionary<string, string> Val = new()
    {
        ["Name"] = "", ["Title"] = "", ["Tagline"] = "", ["Bio"] = "", ["BioEn"] = "",
        ["Email"] = "", ["Phone"] = "", ["Location"] = "",
        ["GitHub"] = "", ["LinkedIn"] = "", ["Facebook"] = "", ["AvatarUrl"] = ""
    };

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        Dictionary<string, string> settings = await SettingsSvc.GetAllAsync();
        foreach (KeyValuePair<string, string> s in settings)
        {
            Val[s.Key] = s.Value;
        }
    }

    private async Task SaveAll()
    {
        await SettingsSvc.SaveAllAsync(Val);
        Toast.Show("Đã lưu cài đặt thành công");
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
