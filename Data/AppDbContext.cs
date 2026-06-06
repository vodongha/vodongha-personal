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
                // Backend
                new Skill { Id = 1,  Name = "C# / .NET",      Category = "Backend",  Icon = "devicon-csharp-plain",       Proficiency = 90, Order = 1 },
                new Skill { Id = 2,  Name = "ASP.NET Core",   Category = "Backend",  Icon = "devicon-dotnetcore-plain",   Proficiency = 88, Order = 2 },
                new Skill { Id = 3,  Name = "Ruby on Rails",  Category = "Backend",  Icon = "devicon-rails-plain",        Proficiency = 80, Order = 3 },
                new Skill { Id = 4,  Name = "Laravel",        Category = "Backend",  Icon = "devicon-laravel-plain",      Proficiency = 75, Order = 4 },
                // Frontend
                new Skill { Id = 5,  Name = "Blazor",         Category = "Frontend", Icon = "devicon-blazor-plain",       Proficiency = 85, Order = 5 },
                new Skill { Id = 6,  Name = "JavaScript",     Category = "Frontend", Icon = "devicon-javascript-plain",   Proficiency = 75, Order = 6 },
                new Skill { Id = 7,  Name = "HTML / CSS",     Category = "Frontend", Icon = "devicon-html5-plain",        Proficiency = 85, Order = 7 },
                // Database
                new Skill { Id = 8,  Name = "PostgreSQL",     Category = "Database", Icon = "devicon-postgresql-plain",   Proficiency = 80, Order = 8 },
                new Skill { Id = 9,  Name = "MySQL",          Category = "Database", Icon = "devicon-mysql-plain",        Proficiency = 75, Order = 9 },
                new Skill { Id = 10, Name = "SQL Server",     Category = "Database", Icon = "devicon-microsoftsqlserver-plain", Proficiency = 80, Order = 10 },
                // DevOps
                new Skill { Id = 11, Name = "Docker",         Category = "DevOps",   Icon = "devicon-docker-plain",       Proficiency = 70, Order = 11 },
                new Skill { Id = 12, Name = "Git",            Category = "DevOps",   Icon = "devicon-git-plain",          Proficiency = 85, Order = 12 },
                new Skill { Id = 13, Name = "Azure",          Category = "DevOps",   Icon = "devicon-azure-plain",        Proficiency = 65, Order = 13 }
            );

        modelBuilder.Entity<Project>()
            .HasData(
                new Project
                {
                    Id = 1,
                    Title = "konfipay",
                    Description = "Nền tảng ngân hàng trực tuyến doanh nghiệp hỗ trợ chuẩn EBICS. Quản lý hàng trăm tài khoản ngân hàng, tích hợp REST API cho ERP, multi-tenant, tự động hoá thanh toán.",
                    Technologies = "C#,ASP.NET Core,Blazor,SQL Server,Hangfire,Azure",
                    LiveUrl = "https://konfipay.de",
                    IsFeatured = true,
                    Order = 1,
                    CreatedAt = new DateTime(2021, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Project
                {
                    Id = 2,
                    Title = "Personal Website",
                    Description = "Web cá nhân xây dựng với Blazor Web App .NET 10 và PostgreSQL. Deploy tự động lên Fly.io qua GitHub Actions, SCSS dark theme.",
                    Technologies = "Blazor,.NET 10,PostgreSQL,SCSS,Fly.io,Docker",
                    GitHubUrl = "https://github.com/vodongha/vodongha-personal",
                    LiveUrl = "https://vodongha.id.vn",
                    IsFeatured = true,
                    Order = 2,
                    CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
    }
}
