using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Server.Tests;

[TestFixture]
public class BlogServiceTests
{
    private IDbContextFactory<AppDbContext> _factory = null!;
    private IMemoryCache _cache = null!;

    [SetUp]
    public async Task SetUp()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        // SQLite in-memory requires a single persistent connection; use the pool factory
        _factory = new SingletonDbContextFactory(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        await using AppDbContext db = await _factory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public void TearDown() => _cache.Dispose();

    private BlogService CreateService() => new(_factory, _cache);

    // ── GetPublishedAsync ──────────────────────────────────────────────────

    [Test]
    public async Task GetPublishedAsync_ReturnsOnlyPublishedPosts()
    {
        await using AppDbContext db = await _factory.CreateDbContextAsync();
        db.BlogPosts.AddRange(
            new BlogPost { Title = "A", Slug = "a", IsPublished = true },
            new BlogPost { Title = "B", Slug = "b", IsPublished = false });
        await db.SaveChangesAsync();

        List<BlogPost> result = await CreateService().GetPublishedAsync();

        result.Count.ShouldBe(1);
        result[0].Slug.ShouldBe("a");
    }

    [Test]
    public async Task GetPublishedAsync_OrdersByCreatedAtDescending()
    {
        await using AppDbContext db = await _factory.CreateDbContextAsync();
        db.BlogPosts.AddRange(
            new BlogPost { Title = "Old", Slug = "old", IsPublished = true, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new BlogPost { Title = "New", Slug = "new", IsPublished = true, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        List<BlogPost> result = await CreateService().GetPublishedAsync();

        result[0].Slug.ShouldBe("new");
        result[1].Slug.ShouldBe("old");
    }

    // ── GetBySlugAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetBySlugAsync_ReturnsMatchingPublishedPost()
    {
        await using AppDbContext db = await _factory.CreateDbContextAsync();
        db.BlogPosts.Add(new BlogPost { Title = "Post", Slug = "my-post", IsPublished = true });
        await db.SaveChangesAsync();

        BlogPost? result = await CreateService().GetBySlugAsync("my-post");

        result.ShouldNotBeNull();
        result!.Slug.ShouldBe("my-post");
    }

    [Test]
    public async Task GetBySlugAsync_ReturnsNullForUnpublishedPost()
    {
        await using AppDbContext db = await _factory.CreateDbContextAsync();
        db.BlogPosts.Add(new BlogPost { Title = "Draft", Slug = "draft", IsPublished = false });
        await db.SaveChangesAsync();

        BlogPost? result = await CreateService().GetBySlugAsync("draft");

        result.ShouldBeNull();
    }

    // ── SaveAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task SaveAsync_NewPost_InsertsRecord()
    {
        BlogPost post = new() { Title = "New", Slug = "new-post" };

        await CreateService().SaveAsync(post);

        await using AppDbContext db = await _factory.CreateDbContextAsync();
        (await db.BlogPosts.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task SaveAsync_ExistingPost_UpdatesRecord()
    {
        await using AppDbContext seed = await _factory.CreateDbContextAsync();
        BlogPost post = new() { Title = "Original", Slug = "post" };
        seed.BlogPosts.Add(post);
        await seed.SaveChangesAsync();

        post.Title = "Updated";
        await CreateService().SaveAsync(post);

        await using AppDbContext db = await _factory.CreateDbContextAsync();
        BlogPost? saved = await db.BlogPosts.FindAsync(post.Id);
        saved!.Title.ShouldBe("Updated");
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_RemovesExistingPost()
    {
        await using AppDbContext seed = await _factory.CreateDbContextAsync();
        BlogPost post = new() { Title = "ToDelete", Slug = "del" };
        seed.BlogPosts.Add(post);
        await seed.SaveChangesAsync();

        await CreateService().DeleteAsync(post.Rid);

        await using AppDbContext db = await _factory.CreateDbContextAsync();
        (await db.BlogPosts.CountAsync()).ShouldBe(0);
    }

    [Test]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        Should.NotThrow(() => CreateService().DeleteAsync(Guid.NewGuid()).GetAwaiter().GetResult());
    }

    // ── GetRelatedAsync ────────────────────────────────────────────────────

    [Test]
    public async Task GetRelatedAsync_WithTags_ReturnsByTagScore()
    {
        await using AppDbContext seed = await _factory.CreateDbContextAsync();
        Guid targetRid = Guid.NewGuid();
        seed.BlogPosts.AddRange(
            new BlogPost { Id = 1, Rid = targetRid, Title = "Target", Slug = "t", IsPublished = true, Tags = "dotnet,blazor" },
            new BlogPost { Id = 2, Rid = Guid.NewGuid(), Title = "Both tags", Slug = "b", IsPublished = true, Tags = "dotnet,blazor" },
            new BlogPost { Id = 3, Rid = Guid.NewGuid(), Title = "One tag", Slug = "o", IsPublished = true, Tags = "dotnet" },
            new BlogPost { Id = 4, Rid = Guid.NewGuid(), Title = "No tags", Slug = "n", IsPublished = true, Tags = "" });
        await seed.SaveChangesAsync();

        List<BlogPost> related = await CreateService().GetRelatedAsync(targetRid, "dotnet,blazor", count: 3);

        related.ShouldNotBeEmpty();
        related[0].Slug.ShouldBe("b");
    }

    [Test]
    public async Task GetRelatedAsync_NoTags_ReturnsMostRecentByDate()
    {
        await using AppDbContext seed = await _factory.CreateDbContextAsync();
        Guid targetRid = Guid.NewGuid();
        seed.BlogPosts.AddRange(
            new BlogPost { Id = 1, Rid = targetRid, Title = "Target", Slug = "t", IsPublished = true, Tags = "", CreatedAt = DateTime.UtcNow },
            new BlogPost { Id = 2, Rid = Guid.NewGuid(), Title = "Newer", Slug = "newer", IsPublished = true, Tags = "", CreatedAt = DateTime.UtcNow.AddMinutes(1) },
            new BlogPost { Id = 3, Rid = Guid.NewGuid(), Title = "Older", Slug = "older", IsPublished = true, Tags = "", CreatedAt = DateTime.UtcNow.AddDays(-1) });
        await seed.SaveChangesAsync();

        List<BlogPost> related = await CreateService().GetRelatedAsync(targetRid, "", count: 2);

        related.Count.ShouldBe(2);
        related[0].Slug.ShouldBe("newer");
    }

    // ── Helper: factory that keeps a single in-memory connection alive ─────

    private sealed class SingletonDbContextFactory : IDbContextFactory<AppDbContext>, IAsyncDisposable
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private Microsoft.Data.Sqlite.SqliteConnection? _conn;

        public SingletonDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext()
        {
            if (_conn is null)
            {
                _conn = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
                _conn.Open();
            }

            DbContextOptions<AppDbContext> connOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_conn)
                .Options;
            return new AppDbContext(connOptions);
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());

        public async ValueTask DisposeAsync()
        {
            if (_conn is not null)
            {
                await _conn.DisposeAsync();
            }
        }
    }
}
