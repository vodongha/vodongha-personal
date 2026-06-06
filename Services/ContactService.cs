using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class ContactService(AppDbContext db)
{
    public async Task SendAsync(ContactMessage message)
    {
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();
    }
}
