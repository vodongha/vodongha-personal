using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminChats : ComponentBase
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;

    private List<ChatSession> _sessions = [];
    private List<ChatMessage> _messages = [];
    private int? _selectedSessionId;
    private ChatSession? _selectedSession;
    private string _replyText = "";
    private bool _sending;

    protected override async Task OnInitializedAsync()
    {
        _sessions = await ChatSvc.GetSessionsAsync();
    }

    private async Task SelectSession(int sessionId)
    {
        _selectedSessionId = sessionId;
        _selectedSession = _sessions.FirstOrDefault(s => s.Id == sessionId);
        _messages = await ChatSvc.GetMessagesAsync(sessionId);
        _replyText = "";
    }

    private async Task SendReply()
    {
        if (string.IsNullOrWhiteSpace(_replyText) || _sending || !_selectedSessionId.HasValue)
        {
            return;
        }

        string content = _replyText.Trim();
        _replyText = "";
        _sending = true;

        try
        {
            ChatMessage msg = await ChatSvc.SendAdminReplyAsync(_selectedSessionId.Value, content);
            _messages.Add(msg);
            _sessions = await ChatSvc.GetSessionsAsync();
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task OnReplyKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendReply();
        }
    }
}
