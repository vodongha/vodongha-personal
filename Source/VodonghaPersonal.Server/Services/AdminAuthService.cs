using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class AdminAuthService(IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly PasswordHasher<AdminUser> _hasher = new();

    public async Task<bool> ValidateAsync(string username, string password)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        AdminUser? user = await db.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            return false;
        }

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return false;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        AdminUser? user = await db.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            return false;
        }

        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
        {
            return false;
        }

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task SeedFromConfigAsync(string username, string password)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        if (await db.AdminUsers.AnyAsync())
        {
            return;
        }

        AdminUser user = new() { Id = Guid.NewGuid(), Username = username, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = _hasher.HashPassword(user, password);
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
    }
}
