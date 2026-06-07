using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using vodongha.Data;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AppSecretsService Secrets { get; set; } = default!;

    private int _unreadChatCount;
    private int _unreadMessagesCount;

    private string _menuOrder = "[]";
    private DotNetObjectReference<Dashboard>? _dotNetRef;

    private const string MenuPrefKey = "_pref.dashboard.menu";

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _dotNetRef = DotNetObjectReference.Create(this);
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        _unreadMessagesCount = await db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _menuOrder = await Secrets.GetValueAsync(MenuPrefKey) ?? "[]";
            await InvokeAsync(StateHasChanged);   // update data-saved-order in DOM before JS reads it
            await JS.InvokeVoidAsync("initSortableCards", "admin-dash-cards", _dotNetRef, MenuPrefKey);
        }
    }

    /// <summary>Called by JS when user finishes dragging a card.</summary>
    [JSInvokable]
    public async Task SaveCardOrder(string prefKey, string[] ids)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(ids);
        await Secrets.SaveAsync(prefKey, json);
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public async ValueTask DisposeAsync()
    {
        Loc.OnChanged -= OnLangChanged;
        _dotNetRef?.Dispose();
        await ValueTask.CompletedTask;
    }
}
