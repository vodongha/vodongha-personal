using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class ContactService(IDbContextFactory<AppDbContext> dbFactory, EmailService emailSvc)
{
    public async Task SendAsync(ContactMessage message)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();

        // Send email notification (non-blocking — errors are logged, not thrown)
        await emailSvc.SendContactNotificationAsync(message.Name, message.Email, message.Subject, message.Message);
    }
}
