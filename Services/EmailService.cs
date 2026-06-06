using Resend;

namespace vodongha.Services;

public class EmailService(IResend resend, IConfiguration config, ILogger<EmailService> logger)
{
    public async Task SendContactNotificationAsync(string senderName, string senderEmail, string subject, string message)
    {
        string? apiKey = config["Email:ResendApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("Resend API key missing — skipping email notification.");
            return;
        }

        try
        {
            string notifyTo = config["Email:NotifyTo"] ?? "REDACTED_EMAIL";

            EmailMessage email = new()
            {
                From    = "vodongha.id.vn <onboarding@resend.dev>",
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
