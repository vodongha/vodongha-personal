using Resend;

namespace vodongha.Services;

public class EmailService(IResend resend, AppSecretsService secrets, ILogger<EmailService> logger)
{
    public async Task SendContactNotificationAsync(string senderName, string senderEmail, string subject, string message)
    {
        string? apiKey = await secrets.GetValueAsync("Email:ResendApiKey");
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("Resend API key missing — skipping email notification.");
            return;
        }

        try
        {
            string notifyTo = await secrets.GetValueAsync("Email:NotifyTo") ?? "vodongha@hotmail.com";

            EmailMessage email = new()
            {
                From    = "vodongha.id.vn <no-reply@vodongha.id.vn>",
                To      = [notifyTo],
                Subject = $"[Contact] {subject}",
                TextBody = $"""
                    Tin nhắn mới từ vodongha.id.vn

                    Người gửi : {senderName}
                    Email     : {senderEmail}
                    Tiêu đề   : {subject}

                    Nội dung:
                    {message}
                    """
            };

            ResendResponse<Guid> response = await resend.EmailSendAsync(email);
            logger.LogInformation("Contact notification sent. Id={Id}", response.Content);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contact notification via Resend.");
        }
    }
}
