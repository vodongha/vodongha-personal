using Microsoft.EntityFrameworkCore;
using vodongha.Data.Models;

namespace vodongha.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        modelBuilder.Entity<Skill>()
            .HasData(
                new Skill { Id = 1, Name = "C# / .NET", Category = "Backend", Icon = "devicon-csharp-plain", Proficiency = 90, Order = 1 },
                new Skill { Id = 2, Name = "ASP.NET Core", Category = "Backend", Icon = "devicon-dotnetcore-plain", Proficiency = 85, Order = 2 },
                new Skill { Id = 3, Name = "Blazor", Category = "Frontend", Icon = "devicon-blazor-plain", Proficiency = 80, Order = 3 },
                new Skill { Id = 4, Name = "PostgreSQL", Category = "Database", Icon = "devicon-postgresql-plain", Proficiency = 75, Order = 4 },
                new Skill { Id = 5, Name = "Docker", Category = "DevOps", Icon = "devicon-docker-plain", Proficiency = 70, Order = 5 },
                new Skill { Id = 6, Name = "Git", Category = "DevOps", Icon = "devicon-git-plain", Proficiency = 85, Order = 6 }
            );

        modelBuilder.Entity<Project>()
            .HasData(
                new Project
                {
                    Id = 1,
                    Title = "Personal Website",
                    Description = "Web cá nhân được xây dựng với Blazor Web App .NET 10 và PostgreSQL.",
                    Technologies = "Blazor,.NET 10,PostgreSQL,SCSS",
                    GitHubUrl = "https://github.com/vodongha/vodongha.id.vn",
                    LiveUrl = "https://vodongha.id.vn",
                    IsFeatured = true,
                    Order = 1,
                    CreatedAt = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc)
                }
            );
    }
}
