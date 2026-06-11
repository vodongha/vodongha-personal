using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class BlogService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
{
    private const string PublishedCacheKey = "blog_published";
    private const string SlugsCacheKey = "blog_slugs";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<List<BlogPost>> GetPublishedAsync()
    {
        if (cache.TryGetValue(PublishedCacheKey, out List<BlogPost>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<BlogPost> result = await db.BlogPosts
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        cache.Set(PublishedCacheKey, result, CacheTtl);
        return result;
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
        if (post.Id == Guid.Empty)
        {
            post.Id = Guid.NewGuid();
            db.BlogPosts.Add(post);
        }
        else
        {
            post.UpdatedAt = DateTime.UtcNow;
            db.BlogPosts.Update(post);
        }

        await db.SaveChangesAsync();
        InvalidateCache();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        BlogPost? post = await db.BlogPosts.FindAsync(id);
        if (post is not null)
        {
            db.BlogPosts.Remove(post);
            await db.SaveChangesAsync();
            InvalidateCache();
        }
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        await db.BlogPosts
            .Where(b => b.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.ViewCount, b => b.ViewCount + 1));
    }

    public async Task<List<BlogPost>> GetRelatedAsync(Guid postId, string tags, int count = 3)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        string[] tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToArray();

        if (tagList.Length == 0)
        {
            return await db.BlogPosts
                .Where(b => b.IsPublished && b.Id != postId)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .Select(b => new BlogPost
                {
                    Id = b.Id,
                    Title = b.Title,
                    TitleEn = b.TitleEn,
                    Slug = b.Slug,
                    Summary = b.Summary,
                    SummaryEn = b.SummaryEn,
                    Tags = b.Tags,
                    CoverImageUrl = b.CoverImageUrl,
                    CreatedAt = b.CreatedAt,
                    IsPublished = b.IsPublished
                })
                .ToListAsync();
        }

        // Project only the fields needed for scoring — avoids loading full Content HTML into memory
        List<BlogPost> all = await db.BlogPosts
            .Where(b => b.IsPublished && b.Id != postId)
            .Select(b => new BlogPost
            {
                Id = b.Id,
                Title = b.Title,
                TitleEn = b.TitleEn,
                Slug = b.Slug,
                Summary = b.Summary,
                SummaryEn = b.SummaryEn,
                Tags = b.Tags,
                CoverImageUrl = b.CoverImageUrl,
                CreatedAt = b.CreatedAt,
                IsPublished = b.IsPublished
            })
            .ToListAsync();

        return all
            .Select(b => new
            {
                Post = b,
                Score = b.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Count(t => tagList.Contains(t, StringComparer.OrdinalIgnoreCase))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Post.CreatedAt)
            .Take(count)
            .Select(x => x.Post)
            .ToList();
    }

    public async Task<List<BlogPost>> GetAllSlugsForSitemapAsync()
    {
        if (cache.TryGetValue(SlugsCacheKey, out List<BlogPost>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<BlogPost> result = await db.BlogPosts
            .Where(b => b.IsPublished)
            .Select(b => new BlogPost { Slug = b.Slug, UpdatedAt = b.UpdatedAt, CreatedAt = b.CreatedAt })
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        cache.Set(SlugsCacheKey, result, CacheTtl);
        return result;
    }

    public void InvalidateCache()
    {
        cache.Remove(PublishedCacheKey);
        cache.Remove(SlugsCacheKey);
    }
}
