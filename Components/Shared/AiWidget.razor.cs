using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class AiWidget : ComponentBase, IDisposable
{
    [Inject] private AiService AiSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private AppSecretsService Secrets { get; set; } = default!;

    private readonly record struct AiDisplayMessage(string Text, bool IsUser);

    private readonly List<AiDisplayMessage>    _messages = [];
    private readonly List<AiService.AiMessage> _history  = [];

    private string _input        = "";
    private bool   _open         = false;
    private bool   _thinking     = false;
    private int    _questionCount = 0;
    private string _modelDisplay = "Gemini";

    protected override void OnInitialized() => Lang.OnChange += OnLangChanged;

    protected override async Task OnInitializedAsync()
    {
        string? model = await Secrets.GetValueAsync("Gemini:Model");
        _modelDisplay = FormatModelName(model ?? "gemini-2.0-flash");
    }

    private static string FormatModelName(string model) => model switch
    {
        "gemini-2.5-flash"         => "Gemini 2.5 Flash",
        "gemini-2.5-pro"           => "Gemini 2.5 Pro",
        "gemini-2.0-flash"         => "Gemini 2.0 Flash",
        "gemini-2.0-flash-lite"    => "Gemini 2.0 Flash Lite",
        "gemini-1.5-flash"         => "Gemini 1.5 Flash",
        "gemini-1.5-pro"           => "Gemini 1.5 Pro",
        _                          => model
    };

    private void ToggleOpen() => _open = !_open;
    private void Close()      => _open = false;

    private async Task SendAsync()
    {
        string question = _input.Trim();
        if (string.IsNullOrEmpty(question) || _thinking)
        {
            return;
        }

        _input = "";
        _thinking = true;
        _questionCount++;
        _messages.Add(new AiDisplayMessage(question, IsUser: true));
        _history.Add(new AiService.AiMessage("user", question));
        await InvokeAsync(StateHasChanged);

        string? answer = await AiSvc.AskAsync(_history);

        _thinking = false;

        if (answer != null)
        {
            _messages.Add(new AiDisplayMessage(answer, IsUser: false));
            _history.Add(new AiService.AiMessage("model", answer));
        }
        else
        {
            string errMsg = Lang.T("ai.error");
            _messages.Add(new AiDisplayMessage(errMsg, IsUser: false));
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task AskSuggestionAsync(string suggestion)
    {
        _input = suggestion;
        await SendAsync();
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendAsync();
        }
    }

    private IEnumerable<string> GetSuggestions() => Lang.IsVi
        ?
        [
            "Kỹ năng chính của bạn là gì?",
            "Kể về kinh nghiệm làm việc",
            "Bạn đã làm dự án nào nổi bật?",
            "Làm sao liên hệ với bạn?"
        ]
        :
        [
            "What are your main skills?",
            "Tell me about your work experience",
            "What are your featured projects?",
            "How can I contact you?"
        ];

    private void OnLangChanged() => InvokeAsync(StateHasChanged);
    public void Dispose()        => Lang.OnChange -= OnLangChanged;
}
