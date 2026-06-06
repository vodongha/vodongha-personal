using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class ContactService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task SendAsync(ContactMessage message)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();
    }
}
