using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using vodongha.Data.Models;

namespace vodongha.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VisitorLog>()
            .HasIndex(v => v.IpAddress).IsUnique();

        modelBuilder.Entity<SiteSetting>()
            .HasIndex(s => s.Key).IsUnique();

        modelBuilder.Entity<SiteSetting>()
            .HasData(
                new SiteSetting { Id = 1,  Key = "Name",       Value = "Võ Đông Hà" },
                new SiteSetting { Id = 2,  Key = "Title",      Value = "Full-Stack Developer" },
                new SiteSetting { Id = 3,  Key = "Tagline",    Value = "Building modern web experiences" },
                new SiteSetting { Id = 4,  Key = "Bio",        Value = "Tôi xây dựng các ứng dụng web hiện đại với .NET, Blazor và PostgreSQL. Đam mê tạo ra những sản phẩm sạch, hiệu quả và đẹp mắt." },
                new SiteSetting { Id = 5,  Key = "Email",      Value = "REDACTED_EMAIL" },
                new SiteSetting { Id = 6,  Key = "Phone",      Value = "REDACTED_PHONE" },
                new SiteSetting { Id = 7,  Key = "Location",   Value = "Ho Chi Minh City, Vietnam" },
                new SiteSetting { Id = 8,  Key = "GitHub",     Value = "https://github.com/vodongha" },
                new SiteSetting { Id = 9,  Key = "LinkedIn",   Value = "https://linkedin.com/in/vodongha" },
                new SiteSetting { Id = 10, Key = "AvatarUrl",  Value = "/images/avatar.png" },
                new SiteSetting { Id = 11, Key = "BioEn",      Value = "I build modern web applications with .NET, Blazor, and PostgreSQL. Passionate about creating clean, efficient, and beautiful products." },
                new SiteSetting { Id = 12, Key = "Facebook",   Value = "" }
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

        modelBuilder.Entity<BlogPost>()
            .HasData(
                new BlogPost
                {
                    Id = 1,
                    Title = "Lập trình với AI: Trải nghiệm thực tế khi làm việc cùng Claude Code",
                    TitleEn = "Building with AI: A Developer's Real Experience with Claude Code",
                    Slug = "building-with-ai-experience-with-claude-code",
                    Summary = "Chia sẻ trải nghiệm thực tế sau nhiều tháng sử dụng Claude Code trong công việc hàng ngày — từ debug lỗi phức tạp, viết test, đến xây dựng tính năng hoàn chỉnh trên codebase lớn.",
                    SummaryEn = "Sharing real-world experience after months of using Claude Code daily — from debugging complex issues and writing tests to building complete features on a large enterprise codebase.",
                    Tags = "AI,Claude,Developer Experience,.NET,Blazor",
                    IsPublished = true,
                    CreatedAt = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc),
                    Content = @"<p>Trong hơn một năm qua, AI đã trở thành một phần không thể thiếu trong quy trình làm việc của tôi. Và công cụ tôi dùng nhiều nhất là <strong>Claude Code</strong> — CLI của Anthropic chạy trực tiếp trong terminal.</p>

<h2>Bắt đầu từ đâu?</h2>
<p>Tôi bắt đầu thử nghiệm Claude Code khi đang phát triển <a href=""https://portal.konfipay.de"" target=""_blank"">konfipay</a> — một nền tảng ngân hàng trực tuyến doanh nghiệp chuẩn EBICS với hàng trăm tenant, mỗi tenant có database riêng. Codebase lớn, logic phức tạp, và áp lực deadline thực tế.</p>

<p>Điều đầu tiên tôi nhận ra: Claude không chỉ ""gợi ý code"" — nó <em>đọc hiểu ngữ cảnh</em>. Khi tôi hỏi về một bug trong luồng xử lý payment, Claude đọc qua controller, service, repo rồi chỉ ra đúng chỗ có vấn đề, không phải chỉ đoán mò.</p>

<h2>Những việc Claude làm tốt nhất</h2>

<h3>1. Debug và trace logic phức tạp</h3>
<p>Khi một background job Hangfire chạy sai trong production nhưng pass tests, Claude giúp tôi phân tích sự khác biệt giữa môi trường DEBUG và Release — phát hiện một service thiếu đăng ký trong <code>Windata.Server.Jobs</code> nhưng lại có trong <code>Windata.Server</code>.</p>

<h3>2. Viết test nhanh và đúng pattern</h3>
<p>Tôi chỉ cần mô tả: ""viết NUnit test cho repo method này, dùng SQLite in-memory, FakeItEasy mock service, Shouldly assertion"" — Claude tạo ra test đúng convention của project ngay lập tức. Tiết kiệm rất nhiều thời gian boilerplate.</p>

<h3>3. Refactor an toàn</h3>
<p>Dự án đang trong quá trình chuyển từ sync sang async. Claude giúp tôi convert từng method một cách chính xác: <code>GetPaged</code> → <code>GetPagedAsync</code>, thay <code>.ToList()</code> bằng <code>await .ToListAsync()</code>, và <code>using var db</code> thành <code>await using var db</code>. Không bỏ sót một chỗ nào.</p>

<h3>4. Hiểu kiến trúc của project</h3>
<p>Claude đọc <code>CLAUDE.md</code> — file tài liệu kiến trúc của project — và tuân thủ đúng convention: controller thin, business logic trong service, DB access trong repo. Không bao giờ vi phạm nguyên tắc layered architecture dù tôi không nhắc lại.</p>

<h2>Những giới hạn cần lưu ý</h2>
<p>Claude Code không phải không có điểm yếu:</p>
<ul>
    <li><strong>Không tự kiểm tra runtime:</strong> Nó có thể suggest code compile được nhưng fail khi chạy thực tế — đặc biệt với async deadlock hoặc DI container issues.</li>
    <li><strong>Cần context rõ ràng:</strong> Nếu bạn không mô tả đủ ngữ cảnh, Claude sẽ đưa ra giải pháp generic thay vì phù hợp với project của bạn.</li>
    <li><strong>Không thay thế được review:</strong> Code do AI tạo vẫn cần review kỹ — đặc biệt với logic nghiệp vụ quan trọng như payment processing.</li>
</ul>

<h2>Kết luận</h2>
<p>Sau hơn một năm, tôi không còn nghĩ AI là ""công cụ hỗ trợ"" nữa — nó đã trở thành một phần của quy trình làm việc. Không phải vì nó thay thế được tư duy của developer, mà vì nó <strong>khuếch đại</strong> tốc độ và độ chính xác của từng quyết định kỹ thuật.</p>

<p>Nếu bạn chưa thử Claude Code trong môi trường làm việc thực tế, tôi nghĩ đây là thời điểm tốt để bắt đầu.</p>",
                    ContentEn = @"<p>Over the past year, AI has become an indispensable part of my workflow. The tool I use most is <strong>Claude Code</strong> — Anthropic's CLI that runs directly in the terminal.</p>

<h2>Where It Started</h2>
<p>I started experimenting with Claude Code while developing <a href=""https://portal.konfipay.de"" target=""_blank"">konfipay</a> — an enterprise online banking platform implementing the EBICS standard, with hundreds of tenants each having their own dedicated database. Large codebase, complex business logic, real deadlines.</p>

<p>The first thing I noticed: Claude doesn't just ""suggest code"" — it <em>understands context</em>. When I asked about a bug in the payment processing flow, Claude read through the controller, service, and repo layers and pinpointed the exact issue rather than guessing.</p>

<h2>Where Claude Excels</h2>

<h3>1. Debugging and tracing complex logic</h3>
<p>When a Hangfire background job was misbehaving in production but passing all tests, Claude helped me analyse the difference between DEBUG and Release environments — catching a service that was registered in <code>Windata.Server</code> but missing from <code>Windata.Server.Jobs</code>.</p>

<h3>3. Writing tests quickly and correctly</h3>
<p>I just describe: ""write an NUnit test for this repo method, SQLite in-memory, FakeItEasy for service mocks, Shouldly assertions"" — and Claude produces a test that matches the project's conventions immediately. It saves an enormous amount of boilerplate time.</p>

<h3>3. Safe refactoring</h3>
<p>The project is mid-migration from sync to async. Claude helped me convert methods accurately one by one: <code>GetPaged</code> → <code>GetPagedAsync</code>, replacing <code>.ToList()</code> with <code>await .ToListAsync()</code>, and <code>using var db</code> with <code>await using var db</code> — without missing a single callsite.</p>

<h3>4. Respecting the project architecture</h3>
<p>Claude reads <code>CLAUDE.md</code> — the project's architecture documentation — and consistently follows its conventions: thin controllers, business logic in services, DB access in repos. It never violates the layered architecture even without being reminded.</p>

<h2>Limitations to Keep in Mind</h2>
<p>Claude Code isn't without weaknesses:</p>
<ul>
    <li><strong>No runtime verification:</strong> It can suggest code that compiles but fails at runtime — especially with async deadlocks or DI container issues.</li>
    <li><strong>Context matters a lot:</strong> Without a clear description of your environment, Claude may offer a generic solution rather than one tailored to your project.</li>
    <li><strong>Still needs review:</strong> AI-generated code still requires careful review — especially for critical business logic like payment processing.</li>
</ul>

<h2>Conclusion</h2>
<p>After more than a year, I no longer think of AI as a ""support tool"" — it has become part of my workflow. Not because it replaces developer thinking, but because it <strong>amplifies</strong> the speed and accuracy of every technical decision.</p>

<p>If you haven't tried Claude Code in a real work environment yet, I think now is a great time to start.</p>"
                }
            );

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
                    Description = "Hệ thống quản lý cho nền tảng đô thị không rác. Quản lý khách hàng, nhân viên, hợp đồng, tích hợp thanh toán MoMo và Payoo. Xây dựng bằng Laravel + MySQL.",
                    DescriptionEn = "Management system for an eco-city waste platform. Handles customers, employees, contracts, with MoMo and Payoo payment gateway integration. Built with Laravel and MySQL.",
                    Technologies = "Laravel,PHP,JavaScript,jQuery,AJAX,MySQL,MoMo API,Payoo",
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
                    Technologies = "ASP.NET Core 3.1,C#,JavaScript,jQuery,CSS,Dropbox API,FTP,SQL Server",
                    IsFeatured = false,
                    Order = 4,
                    CreatedAt = new DateTime(2020, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Project
                {
                    Id = 6,
                    Title = "Hangry",
                    Description = "Hệ thống đặt đồ ăn phục vụ phòng khách sạn. Khách xác thực bằng số phòng + PIN hoặc quét mã QR, xem thực đơn tuần và đặt order tính vào hóa đơn phòng. Theo dõi đơn hàng real-time qua SignalR. Kitchen staff quản lý orders và thực đơn theo thời gian thực từ dashboard riêng.",
                    DescriptionEn = "Hotel room service ordering system. Guests authenticate via room number + PIN or QR code scan, browse the weekly menu, and place orders charged directly to their room bill. Real-time order tracking via SignalR. Kitchen staff manage live orders and menu from a dedicated dashboard.",
                    Technologies = "Blazor WASM,.NET 10,ASP.NET Core,SignalR,PostgreSQL,EF Core,.NET Aspire,SCSS,JWT,Bootstrap 5",
                    IsFeatured = true,
                    Order = 6,
                    CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
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
