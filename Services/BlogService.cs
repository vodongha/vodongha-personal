using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class BlogService(AppDbContext db)
{
    public async Task<List<BlogPost>> GetPublishedAsync() =>
        await db.BlogPosts
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<BlogPost?> GetBySlugAsync(string slug) =>
        await db.BlogPosts.FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

    public async Task<List<BlogPost>> GetAllAsync() =>
        await db.BlogPosts.OrderByDescending(b => b.CreatedAt).ToListAsync();

    public async Task SaveAsync(BlogPost post)
    {
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
        BlogPost? post = await db.BlogPosts.FindAsync(id);
        if (post is not null)
        {
            db.BlogPosts.Remove(post);
            await db.SaveChangesAsync();
        }
    }
}
