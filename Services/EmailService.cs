using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace vodongha.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    public async Task SendContactNotificationAsync(string senderName, string senderEmail, string subject, string message)
    {
        string? host     = config["Email:SmtpHost"];
        string? portStr  = config["Email:SmtpPort"];
        string? user     = config["Email:SmtpUser"];
        string? pass     = config["Email:SmtpPass"];
        string? notifyTo = config["Email:NotifyTo"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            logger.LogWarning("Email config missing — skipping notification.");
            return;
        }

        try
        {
            MimeMessage email = new();
            email.From.Add(new MailboxAddress("vodongha.id.vn", user));
            email.To.Add(MailboxAddress.Parse(notifyTo ?? user));
            email.Subject = $"[Contact] {subject}";
            email.Body = new TextPart("plain")
            {
                Text = $"""
                    Tin nhắn mới từ vodongha.id.vn

                    Người gửi : {senderName}
                    Email     : {senderEmail}
                    Tiêu đề   : {subject}

                    Nội dung:
                    {message}
                    """
            };

            using SmtpClient smtp = new();
            int port = int.TryParse(portStr, out int p) ? p : 587;
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(user, pass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            logger.LogInformation("Contact notification sent to {To}", notifyTo ?? user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contact notification email.");
        }
    }
}
