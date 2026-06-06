using Microsoft.AspNetCore.Components;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class Dashboard : ComponentBase
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;

    private int _unreadChatCount;

    protected override async Task OnInitializedAsync()
    {
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
    }
}
