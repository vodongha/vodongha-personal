using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Data.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class ContactSection : ComponentBase, IDisposable
{
    [Inject] private ContactService ContactSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private ContactMessage _model = new();
    private bool _sending;
    private bool _sent;

    private bool CanSend =>
        !string.IsNullOrWhiteSpace(_model.Name) &&
        !string.IsNullOrWhiteSpace(_model.Subject) &&
        !string.IsNullOrWhiteSpace(_model.Message) &&
        IsValidEmail(_model.Email);

    // Per-field error messages (null = no error shown yet)
    private string? NameError;
    private string? EmailError;
    private string? SubjectError;
    private string? MessageError;

    protected override void OnInitialized() => Lang.OnChange += StateHasChanged;
    public void Dispose() => Lang.OnChange -= StateHasChanged;

    private void ValidateName()
    {
        NameError = string.IsNullOrWhiteSpace(_model.Name)
            ? (Lang.IsVi ? "Vui lòng nhập họ tên." : "Name is required.")
            : null;
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(_model.Email))
        {
            EmailError = Lang.IsVi ? "Vui lòng nhập email." : "Email is required.";
        }
        else if (!IsValidEmail(_model.Email))
        {
            EmailError = Lang.IsVi ? "Email không hợp lệ." : "Invalid email address.";
        }
        else
        {
            EmailError = null;
        }
    }

    private void ValidateSubject()
    {
        SubjectError = string.IsNullOrWhiteSpace(_model.Subject)
            ? (Lang.IsVi ? "Vui lòng nhập tiêu đề." : "Subject is required.")
            : null;
    }

    private void ValidateMessage()
    {
        MessageError = string.IsNullOrWhiteSpace(_model.Message)
            ? (Lang.IsVi ? "Vui lòng nhập tin nhắn." : "Message is required.")
            : null;
    }

    private bool ValidateAll()
    {
        ValidateName();
        ValidateEmail();
        ValidateSubject();
        ValidateMessage();
        return NameError == null && EmailError == null && SubjectError == null && MessageError == null;
    }

    private static bool IsValidEmail(string email)
    {
        try { return new System.Net.Mail.MailAddress(email).Address == email.Trim(); }
        catch { return false; }
    }

    private async Task HandleSubmit()
    {
        if (!ValidateAll() || _sending)
        {
            return;
        }

        _sending = true;
        try
        {
            await ContactSvc.SendAsync(_model);
            _sent = true;
        }
        finally
        {
            _sending = false;
        }
    }
}
