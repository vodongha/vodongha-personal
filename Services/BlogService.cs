using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class BlogService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<BlogPost>> GetPublishedAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.BlogPosts
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.BlogPosts.FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);
    }

    public async Task<List<BlogPost>> GetAllAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.BlogPosts.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task SaveAsync(BlogPost post)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        if (post.Id == 0)
        {
            db.BlogPosts.Add(post);
        }
        else
        {
            post.UpdatedAt = DateTime.UtcNow;
            db.BlogPosts.Update(post);
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        BlogPost? post = await db.BlogPosts.FindAsync(id);
        if (post is not null)
        {
            db.BlogPosts.Remove(post);
            await db.SaveChangesAsync();
        }
    }
}
