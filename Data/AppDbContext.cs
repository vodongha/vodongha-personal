using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using vodongha.Data.Models;

namespace vodongha.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SiteSetting>()
            .HasIndex(s => s.Key).IsUnique();

        modelBuilder.Entity<SiteSetting>()
            .HasData(
                new SiteSetting { Id = 1,  Key = "Name",       Value = "Võ Đông Hà" },
                new SiteSetting { Id = 2,  Key = "Title",      Value = "Full-Stack Developer" },
                new SiteSetting { Id = 3,  Key = "Tagline",    Value = "Building modern web experiences" },
                new SiteSetting { Id = 4,  Key = "Bio",        Value = "Tôi xây dựng các ứng dụng web hiện đại với .NET, Blazor và PostgreSQL. Đam mê tạo ra những sản phẩm sạch, hiệu quả và đẹp mắt." },
                new SiteSetting { Id = 5,  Key = "Email",      Value = "vodongha@hotmail.com" },
                new SiteSetting { Id = 6,  Key = "Phone",      Value = "0929758757" },
                new SiteSetting { Id = 7,  Key = "Location",   Value = "Ho Chi Minh City, Vietnam" },
                new SiteSetting { Id = 8,  Key = "GitHub",     Value = "https://github.com/vodongha" },
                new SiteSetting { Id = 9,  Key = "LinkedIn",   Value = "https://linkedin.com/in/vodongha" },
                new SiteSetting { Id = 10, Key = "AvatarUrl",  Value = "/images/avatar.jpg" }
            );

        modelBuilder.Entity<Education>()
            .HasData(
                new Education { Id = 1, School = "Nguyen Tat Thanh University", Degree = "Bachelor's degree", Field = "Computer Software Engineering", StartYear = 2016, EndYear = 2020, WebsiteUrl = "https://ntt.edu.vn", Order = 1 },
                new Education { Id = 2, School = "Vien Dong College", Degree = "Associate's degree", Field = "Automotive Engineering Technology", StartYear = 2013, EndYear = 2016, WebsiteUrl = "https://www.viendong.edu.vn", Order = 2 }
            );

        modelBuilder.Entity<Experience>()
            .HasData(
                new Experience { Id = 1, Company = "cisbox GmbH", Role = "Web Developer (Onsite)", Location = "Ho Chi Minh City", StartYear = 2021, StartMonth = 10, IsCurrent = true, WebsiteUrl = "https://cisbox.com", Description = "Phát triển konfipay — nền tảng ngân hàng trực tuyến doanh nghiệp chuẩn EBICS. Full-stack với ASP.NET Core, Blazor WASM, SQL Server.", DescriptionEn = "Developing konfipay — an enterprise online banking platform implementing the EBICS standard. Full-stack development with ASP.NET Core, Blazor WASM, and SQL Server.", Order = 1 },
                new Experience { Id = 2, Company = "BSP Software Services Corporation", Role = "Web Developer", Location = "Ho Chi Minh City", StartYear = 2020, StartMonth = 10, IsCurrent = true, WebsiteUrl = "https://bsp.vn", Description = "Công ty outsourcing phần mềm hàng đầu Việt Nam, chuyên cung cấp giải pháp cho khách hàng toàn cầu. Phát triển Order — hệ thống quản lý đơn hàng nội bộ doanh nghiệp. Full-stack với Ruby on Rails 7, MySQL, Elasticsearch và Sidekiq.", DescriptionEn = "A leading Vietnamese software outsourcing company delivering solutions for global clients. Developing Order — an internal order management system for enterprise use. Full-stack with Ruby on Rails 7, MySQL, Elasticsearch, and Sidekiq.", Order = 2 }
            );

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
                new Skill { Id = 13, Name = "Azure",          Category = "DevOps",   Icon = "devicon-azure-plain",        Proficiency = 65, Order = 13 },
                // AI
                new Skill { Id = 14, Name = "Claude (Anthropic)", Category = "AI", Icon = "bi bi-stars",              Proficiency = 90, Order = 14 },
                new Skill { Id = 15, Name = "GitHub Copilot",     Category = "AI", Icon = "devicon-github-plain",       Proficiency = 85, Order = 15 },
                new Skill { Id = 16, Name = "Prompt Engineering",  Category = "AI", Icon = "devicon-tensorflow-plain",  Proficiency = 80, Order = 16 },
                new Skill { Id = 17, Name = "AI-Assisted Dev",     Category = "AI", Icon = "devicon-vscode-plain",      Proficiency = 85, Order = 17 },
                // Frontend extras from LinkedIn
                new Skill { Id = 18, Name = "jQuery",            Category = "Frontend", Icon = "devicon-jquery-plain",        Proficiency = 75, Order = 18 },
                new Skill { Id = 19, Name = "AJAX",              Category = "Frontend", Icon = "bi bi-arrow-repeat",          Proficiency = 72, Order = 19 },
                new Skill { Id = 20, Name = "JSON",              Category = "Backend",  Icon = "devicon-json-plain",          Proficiency = 85, Order = 20 },
                new Skill { Id = 21, Name = "CoffeeScript",      Category = "Frontend", Icon = "devicon-coffeescript-plain",  Proficiency = 60, Order = 21 }
            );

        modelBuilder.Entity<Project>()
            .HasData(
                new Project
                {
                    Id = 1,
                    Title = "konfipay",
                    Description = "Nền tảng ngân hàng trực tuyến doanh nghiệp quy mô lớn theo chuẩn EBICS. Quản lý hàng trăm tài khoản ngân hàng trên 13+ ngân hàng, cung cấp REST API cho tích hợp ERP/tự động hoá, kiến trúc multi-tenant với database riêng mỗi khách hàng, tự động ký và nộp lệnh thanh toán SEPA, xử lý file camt/MT940/pain. Tích hợp PayPal, Atlassian/Jira. Hỗ trợ FinTS/XS2A (Open Banking). Deploy trên Azure (SaaS) và Swisscom Docker (konfipay.ch) cho khách hàng Thụy Sĩ yêu cầu data sovereignty.",
                    DescriptionEn = "Enterprise-scale online banking platform implementing the EBICS standard. Manages hundreds of bank accounts across 13+ banks with a REST API for ERP integration and automation, per-tenant SQL Server database architecture, automated SEPA payment signing and submission, and camt/MT940/pain file processing. Integrates PayPal and Atlassian/Jira. Supports FinTS/XS2A (Open Banking). Deployed on Azure (SaaS) and Swisscom Docker (konfipay.ch) for Swiss customers requiring data sovereignty.",
                    Technologies = "C#,.NET 9,ASP.NET Core,Blazor WASM,SQL Server,Hangfire,Serilog,EBICS,FinTS/XS2A,Azure,Docker,PayPal API,Jira API",
                    LiveUrl = "https://portal.konfipay.de",
                    IsFeatured = true,
                    Order = 1,
                    CreatedAt = new DateTime(2021, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Project
                {
                    Id = 5,
                    Title = "Order",
                    Description = "Hệ thống quản lý đơn hàng nội bộ dành cho doanh nghiệp. Xây dựng với Ruby on Rails 7, tích hợp Elasticsearch để tìm kiếm và lọc đơn hàng theo thời gian thực. Hỗ trợ xuất báo cáo PDF/Excel, xử lý background job với Sidekiq, phân quyền người dùng với Devise + Pundit, và kết nối SFTP để đồng bộ dữ liệu.",
                    DescriptionEn = "Internal order management system for enterprise use. Built with Ruby on Rails 7, featuring real-time search and filtering via Elasticsearch, PDF/Excel report export, background job processing with Sidekiq, role-based access control with Devise and Pundit, and SFTP integration for data synchronisation.",
                    Technologies = "Ruby on Rails 7,MySQL,Elasticsearch,Sidekiq,jQuery,SCSS,Docker,SFTP,PDF/Excel export",
                    IsFeatured = false,
                    Order = 5,
                    CreatedAt = new DateTime(2021, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Project
                {
                    Id = 3,
                    Title = "Grac",
                    Description = "Hệ thống quản lý cho nền tảng đô thị không rác. Quản lý khách hàng, nhân viên, hợp đồng, thanh toán tích hợp MoMo Payment Gateway.",
                    DescriptionEn = "Management system for an eco-city waste platform. Handles customers, employees, contracts, and payment with MoMo Payment Gateway integration.",
                    Technologies = "Ruby on Rails,JavaScript,jQuery,AJAX,PostgreSQL,MoMo API",
                    LiveUrl = "https://e.grac.vn",
                    IsFeatured = false,
                    Order = 3,
                    CreatedAt = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Project
                {
                    Id = 4,
                    Title = "Foto Solution",
                    Description = "Website quản lý studio ảnh: khách hàng, sale, editor, admin. Khách hàng upload ảnh qua Dropbox/FTP, editor chỉnh sửa và ghi chú, quản lý doanh thu và thanh toán.",
                    DescriptionEn = "Photo studio management platform: client portal, sales, editor workflow, and admin dashboard. Clients upload images via Dropbox/FTP, editors annotate and design, with sales reporting and payment management.",
                    Technologies = "Ruby on Rails,JavaScript,jQuery,CoffeeScript,CSS,Dropbox API,FTP",
                    IsFeatured = false,
                    Order = 4,
                    CreatedAt = new DateTime(2020, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Project
                {
                    Id = 2,
                    Title = "Personal Website",
                    Description = "Web cá nhân xây dựng với Blazor Web App .NET 10 và PostgreSQL. Deploy tự động lên Fly.io qua GitHub Actions, SCSS dark theme.",
                    DescriptionEn = "Personal website built with Blazor Web App .NET 10 and PostgreSQL. Auto-deployed to Fly.io via GitHub Actions with SCSS dark theme.",
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
