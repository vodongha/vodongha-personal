using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AppSecret> AppSecrets => Set<AppSecret>();
    public DbSet<PageView> PageViews => Set<PageView>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PageView>()
            .HasIndex(p => p.CreatedAt);

        modelBuilder.Entity<VisitorLog>()
            .HasIndex(v => v.IpAddress).IsUnique();

        modelBuilder.Entity<SiteSetting>()
            .HasIndex(s => s.Key).IsUnique();

        modelBuilder.Entity<AppSecret>()
            .HasIndex(a => a.Key).IsUnique();

        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Username).IsUnique();

        // Long free-text columns -> NCLOB on Oracle (default NVARCHAR2(2000) would truncate).
        modelBuilder.Entity<BlogPost>(e =>
        {
            e.Property(b => b.Summary).HasColumnType("NCLOB");
            e.Property(b => b.SummaryEn).HasColumnType("NCLOB");
            e.Property(b => b.Content).HasColumnType("NCLOB");
            e.Property(b => b.ContentEn).HasColumnType("NCLOB");
        });
        modelBuilder.Entity<Project>(e =>
        {
            e.Property(p => p.Description).HasColumnType("NCLOB");
            e.Property(p => p.DescriptionEn).HasColumnType("NCLOB");
        });
        modelBuilder.Entity<Experience>(e =>
        {
            e.Property(x => x.Description).HasColumnType("NCLOB");
            e.Property(x => x.DescriptionEn).HasColumnType("NCLOB");
        });
        modelBuilder.Entity<Education>(e =>
        {
            e.Property(x => x.Description).HasColumnType("NCLOB");
            e.Property(x => x.DescriptionEn).HasColumnType("NCLOB");
        });
        modelBuilder.Entity<ContactMessage>().Property(c => c.Message).HasColumnType("NCLOB");
        modelBuilder.Entity<ChatMessage>().Property(c => c.Content).HasColumnType("NCLOB");
        modelBuilder.Entity<SiteSetting>().Property(s => s.Value).HasColumnType("NCLOB");
        modelBuilder.Entity<AppSecret>().Property(a => a.Value).HasColumnType("NCLOB");
        modelBuilder.Entity<DataProtectionKey>().Property(d => d.Xml).HasColumnType("NCLOB");
    }
}
