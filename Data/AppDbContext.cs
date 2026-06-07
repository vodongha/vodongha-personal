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
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
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
                new SiteSetting { Id = 9,  Key = "LinkedIn",   Value = "https://www.linkedin.com/in/vodongha" },
                new SiteSetting { Id = 10, Key = "AvatarUrl",  Value = "/images/avatar.png" },
                new SiteSetting { Id = 11, Key = "BioEn",      Value = "I build modern web applications with .NET, Blazor, and PostgreSQL. Passionate about creating clean, efficient, and beautiful products." },
                new SiteSetting { Id = 12, Key = "Facebook",   Value = "https://www.facebook.com/vodongha.fb" }
            );

        modelBuilder.Entity<Education>()
            .HasData(
                new Education { Id = 1, School = "Nguyen Tat Thanh University", Degree = "Bachelor's degree", Field = "Computer Software Engineering", StartYear = 2016, EndYear = 2020, WebsiteUrl = "https://ntt.edu.vn", Order = 1 },
                new Education { Id = 2, School = "Vien Dong College", Degree = "Associate's degree", Field = "Automotive Engineering Technology", StartYear = 2013, EndYear = 2016, WebsiteUrl = "https://www.viendong.edu.vn", Order = 2 }
            );

        modelBuilder.Entity<Experience>()
            .HasData(
                new Experience { Id = 1, Company = "cisbox GmbH", Role = "Web Developer (Onsite)", Location = "Ho Chi Minh City", StartYear = 2021, StartMonth = 10, IsCurrent = true, WebsiteUrl = "https://cisbox.com", Description = "Phát triển konfipay — nền tảng ngân hàng trực tuyến doanh nghiệp chuẩn EBICS. Full-stack với ASP.NET Core, Blazor WASM, SQL Server.", DescriptionEn = "Developing konfipay — an enterprise online banking platform implementing the EBICS standard. Full-stack development with ASP.NET Core, Blazor WASM, and SQL Server.", Order = 1 },
                new Experience { Id = 2, Company = "BSP Software Services Corporation", Role = "Web Developer", Location = "Ho Chi Minh City", StartYear = 2020, StartMonth = 10, IsCurrent = true, WebsiteUrl = "https://bsp.vn", Description = "Công ty outsourcing phần mềm hàng đầu Việt Nam, chuyên cung cấp giải pháp cho khách hàng toàn cầu. Phát triển Order — hệ thống quản lý đơn hàng nội bộ doanh nghiệp. Full-stack với Ruby on Rails 7, MySQL, Elasticsearch và Sidekiq.", DescriptionEn = "A leading Vietnamese software outsourcing company delivering solutions for global clients. Developing Order — an internal order management system for enterprise use. Full-stack with Ruby on Rails 7, MySQL, Elasticsearch, and Sidekiq.", Order = 2 },
                new Experience { Id = 3, Company = "Trung tâm dạy nghề lái xe Sài Gòn", Role = "Driving Instructor", Location = "Dong Nai", StartYear = 2020, StartMonth = 5, EndYear = 2022, EndMonth = 12, IsCurrent = false, Description = "Huấn luyện viên dạy lái xe ô tô.", DescriptionEn = "Car driving instructor.", Order = 3 },
                new Experience { Id = 4, Company = "Be Group", Role = "Car Driver", Location = "Ho Chi Minh City", StartYear = 2020, StartMonth = 3, EndYear = 2020, EndMonth = 10, IsCurrent = false, Description = "Tài xế xe hơi công nghệ.", DescriptionEn = "Ride-hailing car driver.", Order = 4 },
                new Experience { Id = 5, Company = "Gojek", Role = "Motorbike Driver", Location = "Ho Chi Minh City", StartYear = 2018, StartMonth = 4, EndYear = 2020, EndMonth = 3, IsCurrent = false, Description = "Tài xế xe máy công nghệ.", DescriptionEn = "Ride-hailing motorbike driver.", Order = 5 },
                new Experience { Id = 6, Company = "Uber", Role = "Motorbike Driver", Location = "Ho Chi Minh City", StartYear = 2016, StartMonth = 8, EndYear = 2018, EndMonth = 4, IsCurrent = false, Description = "Tài xế xe máy công nghệ.", DescriptionEn = "Ride-hailing motorbike driver.", Order = 6 },
                new Experience { Id = 7, Company = "Bến Thành Ford", Role = "Automotive Technician Internship", Location = "Ho Chi Minh City", StartYear = 2016, StartMonth = 2, EndYear = 2016, EndMonth = 4, IsCurrent = false, Description = "Thực tập kỹ thuật viên ô tô.", DescriptionEn = "Automotive technician internship.", Order = 7 },
                new Experience { Id = 8, Company = "Grab", Role = "Motorbike Driver", Location = "Ho Chi Minh City", StartYear = 2015, StartMonth = 5, EndYear = 2016, EndMonth = 1, IsCurrent = false, Description = "Tài xế xe máy công nghệ.", DescriptionEn = "Ride-hailing motorbike driver.", Order = 8 }
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
                    Summary = "Hơn một năm dùng Claude Code hàng ngày trên codebase enterprise .NET — đây là những gì tôi học được về debug, viết test, refactor và giới hạn thực sự của AI coding assistant.",
                    SummaryEn = "Over a year using Claude Code daily on an enterprise .NET codebase — here is what I learned about debugging, writing tests, refactoring, and the real limits of AI coding assistants.",
                    CoverImageUrl = "https://images.unsplash.com/photo-1620712943543-bcc4688e7485?w=1200&auto=format&fit=crop&q=80",
                    Tags = "AI,Claude Code,Developer Experience,.NET,Blazor,Productivity",
                    IsPublished = true,
                    CreatedAt = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc),
                    Content = @"<p><img src=""https://images.unsplash.com/photo-1620712943543-bcc4688e7485?w=1200&auto=format&fit=crop&q=80"" alt=""Lập trình với Claude Code AI assistant"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Hơn một năm qua, <strong>Claude Code</strong> — CLI của Anthropic chạy trực tiếp trong terminal — đã thay đổi hoàn toàn cách tôi làm việc với code. Không phải theo kiểu ""công cụ hỗ trợ thêm"", mà là thay đổi cả quy trình: cách tôi debug, cách tôi viết test, cách tôi tiếp cận refactor trên codebase lớn.</p>

<p>Bài viết này chia sẻ trải nghiệm thực tế — không phải demo, không phải tutorial — từ môi trường làm việc thực sự với deadline thật và codebase enterprise phức tạp.</p>

<h2>Bối cảnh: Codebase như thế nào?</h2>
<p>Tôi dùng Claude Code chủ yếu khi phát triển <a href=""https://portal.konfipay.de"" target=""_blank"" rel=""noopener"">konfipay</a> — nền tảng ngân hàng trực tuyến doanh nghiệp theo chuẩn EBICS, với kiến trúc multi-tenant: mỗi khách hàng có một database SQL Server riêng. Codebase gồm nhiều project: API server, Blazor WASM client, background job processor, migration tool. Hàng trăm nghìn dòng code, logic nghiệp vụ phức tạp, và yêu cầu bảo mật cao vì xử lý giao dịch tài chính thực.</p>

<p>Đây không phải môi trường lý tưởng để ""thử nghiệm AI"" — đây là nơi AI phải chứng minh được giá trị thực sự.</p>

<h2>Những việc Claude Code làm tốt nhất</h2>

<h3>1. Debug và trace logic phức tạp xuyên nhiều layer</h3>
<p>Đây là điểm Claude Code vượt trội nhất. Khi một background job Hangfire chạy sai trong production nhưng pass hết tests, tôi mô tả triệu chứng và paste stack trace — Claude đọc qua service registrations, tìm ngay vấn đề: một service được đăng ký trong <code>Windata.Server</code> (debug host) nhưng thiếu trong <code>Windata.Server.Jobs</code> (production Hangfire host). Loại bug này thường mất vài tiếng để tự tìm; với Claude mất dưới 10 phút.</p>

<p>Bí quyết: cung cấp đủ ngữ cảnh — kiến trúc project, flow của feature, và triệu chứng cụ thể. Claude không đoán mò khi có đủ thông tin.</p>

<h3>2. Viết test nhanh và đúng convention của project</h3>
<p>Mô tả yêu cầu: <em>""viết NUnit test cho <code>GetPagedAsync</code>, dùng SQLite in-memory, FakeItEasy mock service, Shouldly assertion, theo pattern của các test hiện có""</em>. Claude đọc các test file hiện có, hiểu pattern, rồi tạo test mới hoàn toàn phù hợp convention — không phải code generic. Thời gian viết boilerplate giảm từ 30 phút xuống còn 3 phút.</p>

<h3>3. Refactor an toàn trên diện rộng</h3>
<p>Project đang trong quá trình chuyển toàn bộ codebase từ synchronous sang async — hàng trăm method cần đổi. Claude giúp convert chính xác: <code>GetPaged</code> → <code>GetPagedAsync</code>, <code>.ToList()</code> → <code>await .ToListAsync()</code>, <code>using var db</code> → <code>await using var db</code>, và tìm hết các callsite cần cập nhật. Quan trọng hơn, nó cảnh báo khi phát hiện nguy cơ <code>.Result</code> hay <code>.Wait()</code> tiềm ẩn deadlock.</p>

<h3>4. Tôn trọng kiến trúc và convention của project</h3>
<p>Claude Code đọc file <code>CLAUDE.md</code> — tài liệu mô tả kiến trúc, convention và quyết định kỹ thuật của project — và tuân thủ nhất quán: controller mỏng, business logic trong service, DB access trong repo. Không bao giờ tự ý vi phạm layered architecture hay thêm logic vào sai layer, ngay cả khi tôi không nhắc lại trong câu hỏi.</p>

<h3>5. Giải thích code xa lạ</h3>
<p>Khi cần làm việc với phần codebase cũ không có tài liệu, Claude đọc và giải thích logic theo ngữ cảnh nghiệp vụ — không phải giải thích từng dòng code như syntax, mà giải thích <em>tại sao</em> code làm vậy. Rút ngắn đáng kể thời gian onboard vào module mới.</p>

<h2>Giới hạn thực sự cần biết</h2>
<p>Claude Code không phải không có điểm yếu. Sau hơn một năm dùng thực tế, đây là những giới hạn quan trọng nhất:</p>

<ul>
    <li><strong>Không tự verify runtime:</strong> Code compile được không có nghĩa chạy đúng. Đặc biệt cẩn thận với async deadlock, DI scope mismatch, và EF Core tracking behavior — những lỗi này không bị bắt ở compile time.</li>
    <li><strong>Hallucinate API không tồn tại:</strong> Đôi khi Claude suggest method hoặc overload trông hợp lý nhưng thực ra không tồn tại trong version bạn đang dùng. Luôn verify với IntelliSense hoặc docs.</li>
    <li><strong>Context window có giới hạn:</strong> Trên codebase rất lớn, Claude có thể bỏ sót một số file liên quan. Chỉ rõ file nào cần đọc sẽ cho kết quả tốt hơn là để Claude tự tìm.</li>
    <li><strong>Logic nghiệp vụ phức tạp cần review kỹ:</strong> Code liên quan đến payment, authentication, hay database migration — không bao giờ merge mà không đọc từng dòng, dù AI tạo ra.</li>
</ul>

<h2>Cách dùng Claude Code hiệu quả nhất</h2>
<p>Sau nhiều tháng thử và sai, đây là những nguyên tắc tôi áp dụng:</p>

<ol>
    <li><strong>Duy trì file CLAUDE.md:</strong> Mô tả kiến trúc, convention, và quyết định kỹ thuật quan trọng. Claude đọc file này trước mọi thứ khác — đây là ""bộ nhớ dài hạn"" của AI về project của bạn.</li>
    <li><strong>Đặt câu hỏi có ngữ cảnh:</strong> Thay vì ""fix bug này"", hãy nói ""đây là service X, method Y đang throw Z khi input là W, đây là stack trace, đây là code liên quan"".</li>
    <li><strong>Làm việc theo iteration nhỏ:</strong> Đừng yêu cầu implement toàn bộ tính năng một lần. Xác nhận approach trước, implement từng phần, review trước khi tiếp tục.</li>
    <li><strong>Dùng cho việc khám phá:</strong> Claude giỏi trả lời ""có cách nào tốt hơn không?"" hay ""pattern này có vấn đề gì?"" — dùng như một senior dev để pair với, không chỉ để generate code.</li>
</ol>

<h2>Kết luận</h2>
<p>Claude Code không thay thế tư duy của developer — nó <strong>khuếch đại</strong> tốc độ và độ chính xác của từng quyết định kỹ thuật. Developer biết kiến trúc tốt, biết đặt câu hỏi đúng, biết review kết quả nghiêm túc sẽ hưởng lợi nhiều nhất.</p>

<p>Nếu bạn muốn tìm hiểu thêm về cách AI đang thay đổi quy trình lập trình theo hướng rộng hơn, xem thêm bài viết về <a href=""/blog/vibe-coding-la-gi"">Vibe Coding — xu hướng lập trình mới với AI</a>.</p>",
                    ContentEn = @"<p><img src=""https://images.unsplash.com/photo-1620712943543-bcc4688e7485?w=1200&auto=format&fit=crop&q=80"" alt=""Building with Claude Code AI assistant"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>For over a year, <strong>Claude Code</strong> — Anthropic's CLI that runs directly in the terminal — has fundamentally changed how I work with code. Not as an ""extra support tool"", but as a workflow transformation: how I debug, how I write tests, how I approach refactoring across a large enterprise codebase.</p>

<p>This post shares real-world experience — not a demo, not a tutorial — from an actual working environment with real deadlines and genuinely complex enterprise code.</p>

<h2>Context: What Kind of Codebase?</h2>
<p>I primarily use Claude Code while developing <a href=""https://portal.konfipay.de"" target=""_blank"" rel=""noopener"">konfipay</a> — an enterprise online banking platform implementing the EBICS standard, with a multi-tenant architecture where each customer has their own dedicated SQL Server database. The solution spans multiple projects: an API server, a Blazor WASM client, a background job processor, and a migration tool. Hundreds of thousands of lines of code, complex business logic, and high security requirements since it processes real financial transactions.</p>

<p>This is not an ideal environment to ""experiment with AI"" — it's a place where AI has to prove real value.</p>

<h2>Where Claude Code Excels</h2>

<h3>1. Debugging and tracing complex logic across multiple layers</h3>
<p>This is where Claude Code stands out most clearly. When a Hangfire background job was misbehaving in production but passing all tests, I described the symptoms and pasted the stack trace — Claude read through the service registrations and found the issue immediately: a service registered in <code>Windata.Server</code> (the debug host) but missing from <code>Windata.Server.Jobs</code> (the production Hangfire host). This type of bug typically takes hours to find manually; with Claude it took under 10 minutes.</p>

<p>The key: providing enough context — the project architecture, the feature flow, and specific symptoms. Claude doesn't guess when it has sufficient information.</p>

<h3>2. Writing tests quickly and in the project's conventions</h3>
<p>Describe the requirement: <em>""write an NUnit test for <code>GetPagedAsync</code>, SQLite in-memory, FakeItEasy for service mocks, Shouldly assertions, following the pattern of existing tests""</em>. Claude reads the existing test files, understands the pattern, and generates new tests that completely match the convention — not generic code. Time writing boilerplate dropped from 30 minutes to 3.</p>

<h3>3. Safe large-scale refactoring</h3>
<p>The project is mid-migration from synchronous to async across the entire codebase — hundreds of methods need updating. Claude converts accurately: <code>GetPaged</code> → <code>GetPagedAsync</code>, <code>.ToList()</code> → <code>await .ToListAsync()</code>, <code>using var db</code> → <code>await using var db</code>, and finds all callsites that need updating. More importantly, it flags potential <code>.Result</code> or <code>.Wait()</code> calls that risk deadlock.</p>

<h3>4. Respecting the project's architecture and conventions</h3>
<p>Claude Code reads the <code>CLAUDE.md</code> file — which describes the architecture, conventions, and key technical decisions — and follows them consistently: thin controllers, business logic in services, DB access in repos. It never crosses layer boundaries or puts logic in the wrong place, even when I don't explicitly mention it in the question.</p>

<h3>5. Explaining unfamiliar code</h3>
<p>When working with undocumented legacy parts of the codebase, Claude reads and explains the logic in business context terms — not line-by-line syntax explanations, but <em>why</em> the code does what it does. This significantly shortens the time needed to onboard into a new module.</p>

<h2>Real Limitations You Need to Know</h2>
<p>Claude Code is not without weaknesses. After over a year of real use, these are the most important limitations:</p>

<ul>
    <li><strong>No runtime verification:</strong> Code that compiles doesn't mean it runs correctly. Be especially careful with async deadlocks, DI scope mismatches, and EF Core tracking behavior — these errors aren't caught at compile time.</li>
    <li><strong>API hallucination:</strong> Occasionally Claude suggests a method or overload that looks plausible but doesn't exist in the version you're using. Always verify with IntelliSense or docs.</li>
    <li><strong>Context window limits:</strong> On very large codebases, Claude may miss some related files. Explicitly pointing to which files to read gives better results than letting Claude discover them.</li>
    <li><strong>Complex business logic needs careful review:</strong> Code related to payments, authentication, or database migrations — never merge without reading every line, regardless of who (or what) wrote it.</li>
</ul>

<h2>How to Use Claude Code Most Effectively</h2>
<p>After months of trial and error, here are the principles I apply:</p>

<ol>
    <li><strong>Maintain a CLAUDE.md file:</strong> Describe the architecture, conventions, and key technical decisions. Claude reads this file before anything else — it's the AI's ""long-term memory"" about your project.</li>
    <li><strong>Ask contextual questions:</strong> Instead of ""fix this bug"", say ""this is service X, method Y is throwing Z when input is W, here's the stack trace, here's the relevant code"".</li>
    <li><strong>Work in small iterations:</strong> Don't ask for a full feature in one shot. Confirm the approach first, implement piece by piece, review before continuing.</li>
    <li><strong>Use it for exploration:</strong> Claude is great at answering ""is there a better way?"" or ""what problems might this pattern have?"" — use it as a senior dev to pair with, not just a code generator.</li>
</ol>

<h2>Conclusion</h2>
<p>Claude Code doesn't replace a developer's thinking — it <strong>amplifies</strong> the speed and accuracy of every technical decision. Developers with strong architectural thinking, who ask the right questions and review results seriously, benefit the most.</p>

<p>If you want to explore how AI is changing the programming workflow more broadly, check out the post on <a href=""/blog/vibe-coding-la-gi"">Vibe Coding — the new AI-driven approach to software development</a>.</p>"
                },
                new BlogPost
                {
                    Id = 3,
                    Title = "Vibe Coding là gì? Khi lập trình viên 'cảm' thay vì 'gõ'",
                    TitleEn = "What Is Vibe Coding? When Developers Feel Instead of Type",
                    Slug = "vibe-coding-la-gi",
                    Summary = "Vibe coding — xu hướng lập trình mới nơi bạn mô tả ý tưởng bằng ngôn ngữ tự nhiên và AI tạo ra code. Không còn gõ từng dòng, chỉ cần 'cảm' đúng hướng. Nhưng đây có phải tương lai của nghề lập trình?",
                    SummaryEn = "Vibe coding is the emerging practice of describing your intent in natural language and letting AI write the code. No more typing line by line — just feel the direction. But is this really the future of software development?",
                    CoverImageUrl = "https://cdn.sanity.io/images/bj34pdbp/migration/285b93a3e464a3cd61067037083f75f1b902f2a5-4800x2520.png?w=3840&q=75&fit=clip&auto=format",
                    Tags = "Vibe Coding,AI,Developer Experience,Claude,Productivity",
                    IsPublished = true,
                    CreatedAt = new DateTime(2026, 6, 7, 0, 0, 0, DateTimeKind.Utc),
                    Content = @"<p><img src=""https://cdn.sanity.io/images/bj34pdbp/migration/285b93a3e464a3cd61067037083f75f1b902f2a5-4800x2520.png?w=3840&q=75&fit=clip&auto=format"" alt=""Vibe Coding — lập trình cùng AI"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Đầu năm 2025, Andrej Karpathy — cựu giám đốc AI của Tesla và là một trong những nhà nghiên cứu AI hàng đầu thế giới — đăng một tweet ngắn gọn mà sau đó trở thành định nghĩa cho một xu hướng mới: <strong>Vibe Coding</strong>.</p>

<blockquote>
<p><em>""There's a new kind of coding I call vibe coding, where you fully give in to the vibes, embrace exponentials, and forget that the code even exists.""</em><br />— Andrej Karpathy</p>
</blockquote>

<p>Vậy vibe coding là gì, và tại sao nó đang thay đổi cách chúng ta nghĩ về nghề lập trình?</p>

<h2>Vibe Coding là gì?</h2>
<p>Vibe coding là phong cách lập trình nơi developer không gõ code thủ công từng dòng, mà thay vào đó <strong>mô tả ý định bằng ngôn ngữ tự nhiên</strong> — và để AI tạo ra code. Bạn ""cảm"" hướng mình muốn đi, AI hiểu và thực thi.</p>

<p>Quy trình điển hình:</p>
<ol>
    <li>Bạn mô tả tính năng: <em>""Thêm một chat widget vào trang web, có form nhập tên/số điện thoại/email, kết nối SignalR real-time, gửi tin nhắn lên Telegram""</em></li>
    <li>AI (Claude Code, Copilot, Cursor...) đọc toàn bộ codebase, hiểu ngữ cảnh</li>
    <li>AI tạo ra tất cả: component Blazor, service, hub SignalR, SCSS styling</li>
    <li>Bạn test, feedback, lặp lại</li>
</ol>

<p>Không cần nhớ API signature. Không cần tra Google từng lỗi nhỏ. Không cần viết boilerplate tay. Bạn <em>suy nghĩ ở mức cao hơn</em> — AI xử lý phần còn lại.</p>

<h2>Tại sao vibe coding lại hiệu quả đến vậy?</h2>

<h3>1. AI hiểu ngữ cảnh toàn bộ codebase</h3>
<p>Đây là điểm khác biệt lớn nhất so với autocomplete truyền thống. Claude Code đọc tất cả file liên quan trước khi viết một dòng code. Nó biết bạn đang dùng kiến trúc nào, convention là gì, pattern nào đã có sẵn. Kết quả là code được tạo ra phù hợp với project — không phải code generic copy-paste từ StackOverflow.</p>

<h3>2. Vòng lặp feedback cực ngắn</h3>
<p>Thay vì mất 2 tiếng để implement một tính năng từ đầu, bạn có thể có bản prototype hoạt động trong 15 phút. Thời gian còn lại dành cho việc quan trọng hơn: <strong>xác định đúng vấn đề cần giải quyết</strong>.</p>

<h3>3. Barrier to entry giảm mạnh</h3>
<p>Bạn không cần thuộc lòng 100% API của một framework để sử dụng nó hiệu quả. Developer .NET có thể nhanh chóng làm việc với Ruby on Rails — chỉ cần mô tả ý định, AI tạo code đúng cú pháp. Đây vừa là lợi thế lớn, vừa là điểm cần cẩn thận.</p>

<h2>Trải nghiệm thực tế — website này được vibe code</h2>
<p>Trang web bạn đang đọc — <strong>vodongha.id.vn</strong> — được xây dựng hoàn toàn theo phong cách vibe coding cùng Claude Code.</p>

<p>Một ví dụ cụ thể: chat widget với country auto-detection. Yêu cầu ban đầu nghe có vẻ đơn giản — <em>""tự nhận mã quốc gia từ IP của user""</em>. Nhưng thực tế phức tạp hơn nhiều:</p>
<ul>
    <li>Blazor Server có SSR phase và circuit phase — <code>HttpContext</code> chỉ available trong SSR</li>
    <li>Nếu server gọi ipinfo.io, nó dùng IP của Fly.io Singapore — không phải IP của user</li>
    <li>Giải pháp đúng: nhúng IP vào HTML trong SSR, để browser JS tự gọi ipinfo.io</li>
</ul>

<p>Claude Code phân tích vấn đề, đề xuất đúng giải pháp, implement qua 3 lần iteration. Không phải lần đầu tiên đúng ngay — nhưng quy trình debug cùng AI nhanh hơn nhiều so với làm một mình.</p>

<h2>Vibe coding ≠ không cần hiểu code</h2>
<p>Đây là quan niệm sai lầm phổ biến nhất. Vibe coding hiệu quả <em>vì</em> bạn hiểu đủ để:</p>
<ul>
    <li><strong>Nhận ra khi AI sai</strong> — AI tự tin đề xuất giải pháp không tối ưu hoặc thậm chí sai về mặt logic nghiệp vụ</li>
    <li><strong>Đặt câu hỏi đúng</strong> — biết cần hỏi về security implication, performance, hay edge case nào</li>
    <li><strong>Review code quan trọng</strong> — payment logic, auth flow, database migration không bao giờ nên tin tưởng mù quáng</li>
</ul>

<p>Developer giỏi nhất trong kỷ nguyên vibe coding không phải người gõ nhanh nhất — mà là người <strong>có tư duy kiến trúc tốt nhất</strong> và biết khi nào cần kiểm soát chặt, khi nào có thể delegate cho AI.</p>

<h2>Công cụ vibe coding tốt nhất hiện nay</h2>

<table style=""width:100%;border-collapse:collapse;margin:1rem 0"">
<thead>
<tr style=""border-bottom:1px solid #333"">
<th style=""text-align:left;padding:0.5rem"">Công cụ</th>
<th style=""text-align:left;padding:0.5rem"">Điểm mạnh</th>
<th style=""text-align:left;padding:0.5rem"">Phù hợp với</th>
</tr>
</thead>
<tbody>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Claude Code</strong></td>
<td style=""padding:0.5rem"">Hiểu toàn bộ codebase, làm việc trong terminal</td>
<td style=""padding:0.5rem"">Project lớn, refactor, architecture</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Cursor</strong></td>
<td style=""padding:0.5rem"">IDE tích hợp AI, UX tốt</td>
<td style=""padding:0.5rem"">Developer muốn giữ IDE workflow</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>GitHub Copilot</strong></td>
<td style=""padding:0.5rem"">Inline suggestion mượt mà</td>
<td style=""padding:0.5rem"">Autocomplete, boilerplate nhanh</td>
</tr>
<tr>
<td style=""padding:0.5rem""><strong>Bolt / v0</strong></td>
<td style=""padding:0.5rem"">Tạo UI từ mô tả, preview ngay</td>
<td style=""padding:0.5rem"">Prototype UI nhanh</td>
</tr>
</tbody>
</table>

<h2>Vibe coding có phải tương lai không?</h2>
<p>Câu trả lời ngắn: <strong>có</strong> — nhưng không phải theo nghĩa AI thay thế hoàn toàn developer.</p>

<p>Vibe coding là sự chuyển dịch về <em>mức độ abstraction</em>. Giống như khi các ngôn ngữ bậc cao thay thế assembly — lập trình viên không biến mất, họ chuyển sang làm việc ở tầng cao hơn. Thay vì quản lý memory thủ công, họ thiết kế hệ thống. Thay vì viết từng dòng SQL, họ thiết kế data model.</p>

<p>Vibe coding đẩy tiếp xu hướng đó: thay vì viết từng function, developer <strong>thiết kế kiến trúc, định nghĩa behaviour, và đảm bảo chất lượng</strong>.</p>

<p>Nếu bạn chưa thử vibe coding trong một project thực tế, đây là thời điểm tốt để bắt đầu. Không cần chờ đến khi nó ""hoàn hảo"" — công cụ hiện tại đã đủ tốt để tăng tốc độ làm việc của bạn lên đáng kể.</p>",
                    ContentEn = @"<p><img src=""https://cdn.sanity.io/images/bj34pdbp/migration/285b93a3e464a3cd61067037083f75f1b902f2a5-4800x2520.png?w=3840&q=75&fit=clip&auto=format"" alt=""Vibe Coding — programming with AI"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>In early 2025, Andrej Karpathy — former AI Director at Tesla and one of the world's leading AI researchers — posted a short tweet that went on to define a new trend: <strong>Vibe Coding</strong>.</p>

<blockquote>
<p><em>""There's a new kind of coding I call vibe coding, where you fully give in to the vibes, embrace exponentials, and forget that the code even exists.""</em><br />— Andrej Karpathy</p>
</blockquote>

<p>So what is vibe coding, and why is it changing how we think about software development as a profession?</p>

<h2>What Is Vibe Coding?</h2>
<p>Vibe coding is a style of programming where the developer no longer types code line by line, but instead <strong>describes their intent in natural language</strong> — and lets AI generate the code. You ""feel"" the direction you want to go, and AI understands and executes.</p>

<p>A typical workflow:</p>
<ol>
    <li>You describe the feature: <em>""Add a chat widget to the website with a name/phone/email form, real-time SignalR connection, and Telegram message forwarding""</em></li>
    <li>AI (Claude Code, Copilot, Cursor...) reads the entire codebase and understands the context</li>
    <li>AI generates everything: Blazor component, service, SignalR hub, SCSS styling</li>
    <li>You test, give feedback, iterate</li>
</ol>

<p>No need to memorize API signatures. No Googling minor syntax errors. No writing boilerplate by hand. You <em>think at a higher level</em> — AI handles the rest.</p>

<h2>Why Is Vibe Coding So Effective?</h2>

<h3>1. AI understands the full codebase context</h3>
<p>This is the biggest difference from traditional autocomplete. Claude Code reads all relevant files before writing a single line of code. It knows your architecture, your conventions, the patterns already in place. The result is code that fits your project — not generic StackOverflow copy-paste.</p>

<h3>2. Extremely short feedback loops</h3>
<p>Instead of spending 2 hours implementing a feature from scratch, you can have a working prototype in 15 minutes. The remaining time goes to what matters more: <strong>identifying the right problem to solve</strong>.</p>

<h3>3. Dramatically lower barrier to entry</h3>
<p>You don't need to memorize 100% of a framework's API to use it effectively. A .NET developer can quickly work with Ruby on Rails — just describe the intent, AI generates syntactically correct code. This is both a major advantage and a reason to stay careful.</p>

<h2>Real Experience — This Website Was Vibe Coded</h2>
<p>The website you're reading — <strong>vodongha.id.vn</strong> — was built entirely in the vibe coding style with Claude Code.</p>

<p>A concrete example: the chat widget with country auto-detection. The initial requirement sounded simple — <em>""auto-detect the user's country from their IP""</em>. But the reality was more complex:</p>
<ul>
    <li>Blazor Server has an SSR phase and a circuit phase — <code>HttpContext</code> is only available during SSR</li>
    <li>If the server calls ipinfo.io, it uses the Fly.io Singapore IP — not the user's IP</li>
    <li>The correct solution: embed the IP in the HTML during SSR, and let browser JS call ipinfo.io directly</li>
</ul>

<p>Claude Code analysed the problem, proposed the right solution, and implemented it across 3 iterations. Not right on the first try — but debugging alongside AI was significantly faster than doing it alone.</p>

<h2>Vibe Coding ≠ Not Understanding Code</h2>
<p>This is the most common misconception. Vibe coding is effective <em>because</em> you understand enough to:</p>
<ul>
    <li><strong>Recognise when AI is wrong</strong> — AI confidently proposes suboptimal or even logically incorrect solutions</li>
    <li><strong>Ask the right questions</strong> — knowing which security implications, performance concerns, or edge cases to probe</li>
    <li><strong>Review critical code</strong> — payment logic, auth flows, and database migrations should never be trusted blindly</li>
</ul>

<p>The best developers in the vibe coding era aren't the fastest typists — they're the ones with the <strong>strongest architectural thinking</strong> and who know when to maintain tight control versus when to delegate to AI.</p>

<h2>The Best Vibe Coding Tools Right Now</h2>

<table style=""width:100%;border-collapse:collapse;margin:1rem 0"">
<thead>
<tr style=""border-bottom:1px solid #333"">
<th style=""text-align:left;padding:0.5rem"">Tool</th>
<th style=""text-align:left;padding:0.5rem"">Strengths</th>
<th style=""text-align:left;padding:0.5rem"">Best for</th>
</tr>
</thead>
<tbody>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Claude Code</strong></td>
<td style=""padding:0.5rem"">Full codebase understanding, terminal-native</td>
<td style=""padding:0.5rem"">Large projects, refactoring, architecture</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Cursor</strong></td>
<td style=""padding:0.5rem"">AI-integrated IDE, great UX</td>
<td style=""padding:0.5rem"">Developers who prefer an IDE workflow</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>GitHub Copilot</strong></td>
<td style=""padding:0.5rem"">Smooth inline suggestions</td>
<td style=""padding:0.5rem"">Autocomplete, fast boilerplate</td>
</tr>
<tr>
<td style=""padding:0.5rem""><strong>Bolt / v0</strong></td>
<td style=""padding:0.5rem"">Generate UI from description with live preview</td>
<td style=""padding:0.5rem"">Fast UI prototyping</td>
</tr>
</tbody>
</table>

<h2>Is Vibe Coding the Future?</h2>
<p>The short answer: <strong>yes</strong> — but not in the sense that AI fully replaces developers.</p>

<p>Vibe coding is a shift in the <em>level of abstraction</em>. It's like when high-level languages replaced assembly — programmers didn't disappear, they moved up to work at a higher layer. Instead of managing memory manually, they designed systems. Instead of writing raw SQL, they designed data models.</p>

<p>Vibe coding continues that trend: instead of writing every function, developers <strong>design architecture, define behaviour, and ensure quality</strong>.</p>

<p>If you haven't tried vibe coding in a real project yet, now is a great time to start. No need to wait for it to be ""perfect"" — the current tools are already good enough to meaningfully accelerate your work.</p>"
                },
                new BlogPost
                {
                    Id = 2,
                    Title = "AI Skills cho Developer: 5 Kỹ năng cốt lõi trong kỷ nguyên AI",
                    TitleEn = "AI Skills for Developers: 5 Core Skills Every Developer Needs in the Age of AI",
                    Slug = "ai-skills-for-developers",
                    Summary = "AI không thay thế developer — nhưng developer biết dùng AI sẽ thay thế người không biết. Đây là 5 kỹ năng thực tế giúp bạn làm việc hiệu quả với AI coding assistant trong công việc hàng ngày.",
                    SummaryEn = "AI won't replace developers — but developers who use AI effectively will outpace those who don't. Here are 5 practical skills to work with AI coding assistants productively every day.",
                    CoverImageUrl = "https://images.unsplash.com/photo-1677442135703-1787eea5ce01?w=1200&auto=format&fit=crop&q=80",
                    Tags = "AI,Claude Code,Developer Skills,Productivity,Prompt Engineering",
                    IsPublished = true,
                    CreatedAt = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                    Content = @"<p><img src=""https://images.unsplash.com/photo-1677442135703-1787eea5ce01?w=1200&auto=format&fit=crop&q=80"" alt=""AI Skills cho Developer — làm việc cùng AI coding assistant"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>""AI có thay thế lập trình viên không?"" — câu hỏi này đã không còn là trọng tâm nữa. Thực tế năm 2025 cho thấy câu hỏi đúng là: <strong>""Developer biết dùng AI có đang làm việc nhanh hơn bạn 3–5 lần không?""</strong> Câu trả lời, trong nhiều trường hợp, là có.</p>

<p>Nhưng làm việc hiệu quả với AI không chỉ là ""dùng ChatGPT để hỏi câu hỏi"". Đó là một bộ kỹ năng cụ thể, có thể học và luyện tập. Bài viết này tổng hợp 5 kỹ năng cốt lõi nhất từ kinh nghiệm thực tế của tôi sau hơn một năm dùng Claude Code hàng ngày trên codebase enterprise.</p>

<h2>1. Prompt Engineering — Kỹ năng giao tiếp với AI</h2>
<p>Đây là kỹ năng căn bản nhất và cũng là nơi hầu hết developer mắc lỗi đầu tiên. AI không đọc được suy nghĩ của bạn — nó chỉ làm tốt với những gì bạn mô tả.</p>

<p><strong>Prompt yếu:</strong> <em>""fix bug này""</em> — AI không biết bug là gì, ở đâu, mong đợi gì.</p>

<p><strong>Prompt mạnh:</strong> <em>""Đây là ASP.NET Core service. Method <code>GetOrdersAsync</code> đang throw <code>NullReferenceException</code> tại dòng 47 khi <code>customerId</code> không tồn tại trong DB. Stack trace: [paste]. Code hiện tại: [paste]. Mong đợi: trả về empty list thay vì throw. Tìm nguyên nhân và sửa.""</em></p>

<p>Công thức cho một prompt hiệu quả: <strong>Ngữ cảnh + Vấn đề cụ thể + Input/Output mong đợi + Ràng buộc</strong>. Mỗi phần thiếu là một lần AI phải đoán — và đoán sai.</p>

<h2>2. Context Management — Dạy AI hiểu project của bạn</h2>
<p>AI coding assistant không nhớ project của bạn giữa các session. Mỗi lần bắt đầu, nó là một tờ giấy trắng. Đây là lý do <strong>file context</strong> quan trọng hơn bất kỳ prompt đơn lẻ nào.</p>

<p>Với Claude Code, tôi duy trì file <code>CLAUDE.md</code> trong mỗi project — mô tả:</p>
<ul>
    <li>Kiến trúc tổng thể và các layer (Controller → Service → Repo)</li>
    <li>Convention đặt tên, coding style</li>
    <li>Những quyết định kỹ thuật quan trọng và lý do</li>
    <li>Những gì <em>không được làm</em> (ví dụ: ""không dùng AutoMapper"", ""không thêm raw SQL"")</li>
    <li>Các gotcha đặc thù của project</li>
</ul>

<p>Claude Code đọc file này trước khi làm bất cứ việc gì — kết quả là code được tạo ra đúng convention ngay từ đầu, không cần sửa lại nhiều lần.</p>

<h2>3. Critical Verification — Không tin tưởng mù quáng</h2>
<p>AI tự tin là đặc điểm, không phải dấu hiệu của sự đúng đắn. Khả năng phân biệt khi nào AI đúng và khi nào cần verify kỹ hơn là kỹ năng phân tách developer dùng AI hiệu quả với developer bị AI dắt mũi.</p>

<p><strong>Verify bắt buộc:</strong></p>
<ul>
    <li><strong>Security-sensitive code</strong>: authentication, authorization, encryption, input validation</li>
    <li><strong>Database migration</strong>: không bao giờ chạy script AI tạo ra mà không đọc từng dòng</li>
    <li><strong>Payment và financial logic</strong>: một lỗi nhỏ có thể gây ra hậu quả không thể đảo ngược</li>
    <li><strong>Async/concurrent code</strong>: deadlock và race condition không bị bắt ở compile time</li>
</ul>

<p><strong>Tin tưởng tương đối:</strong> boilerplate code, test setup, CRUD đơn giản, SCSS/styling, documentation.</p>

<p>Rule of thumb: mức độ verify tỉ lệ thuận với hậu quả nếu code sai.</p>

<h2>4. Iterative Decomposition — Chia nhỏ để kiểm soát</h2>
<p>Đây là lỗi phổ biến nhất của người mới dùng AI: yêu cầu implement toàn bộ tính năng trong một lần. AI sẽ tạo ra hàng trăm dòng code — và bạn không biết bắt đầu review từ đâu.</p>

<p>Workflow hiệu quả hơn:</p>
<ol>
    <li><strong>Phân tích trước:</strong> <em>""Phân tích approach để implement X, liệt kê các bước và rủi ro""</em></li>
    <li><strong>Xác nhận hướng đi:</strong> Đồng ý với approach trước khi bắt đầu code</li>
    <li><strong>Implement từng bước:</strong> <em>""Implement bước 1 trước: tạo data model và migration""</em></li>
    <li><strong>Review trước khi tiếp:</strong> Đọc kỹ output bước 1, fix nếu cần, rồi mới sang bước 2</li>
</ol>

<p>Cách này giúp bạn bắt lỗi sớm hơn, hiểu được code AI tạo ra, và tránh được những thay đổi ngoài scope gây ra rắc rối sau.</p>

<h2>5. Tool-Task Matching — Chọn đúng công cụ cho đúng việc</h2>
<p>Không có AI tool nào tốt nhất cho mọi việc. Sử dụng sai công cụ làm giảm hiệu quả đáng kể:</p>

<table style=""width:100%;border-collapse:collapse;margin:1rem 0"">
<thead>
<tr style=""border-bottom:1px solid #333"">
<th style=""text-align:left;padding:0.5rem"">Công cụ</th>
<th style=""text-align:left;padding:0.5rem"">Dùng tốt nhất cho</th>
<th style=""text-align:left;padding:0.5rem"">Không nên dùng cho</th>
</tr>
</thead>
<tbody>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Claude Code</strong></td>
<td style=""padding:0.5rem"">Codebase lớn, debug cross-layer, refactor, architecture review</td>
<td style=""padding:0.5rem"">Câu hỏi nhanh không cần đọc file</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>GitHub Copilot</strong></td>
<td style=""padding:0.5rem"">Inline completion trong IDE, boilerplate nhanh</td>
<td style=""padding:0.5rem"">Task cần hiểu toàn bộ codebase</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Claude.ai / ChatGPT</strong></td>
<td style=""padding:0.5rem"">Brainstorming, giải thích concept, viết documentation</td>
<td style=""padding:0.5rem"">Task cần truy cập file thực tế</td>
</tr>
<tr>
<td style=""padding:0.5rem""><strong>Cursor</strong></td>
<td style=""padding:0.5rem"">Tích hợp AI sâu trong IDE, muốn giữ GUI workflow</td>
<td style=""padding:0.5rem"">Terminal-heavy workflow</td>
</tr>
</tbody>
</table>

<h2>Bonus: Kỹ năng quan trọng nhất — Biết khi nào KHÔNG dùng AI</h2>
<p>Đây là kỹ năng ít được nhắc đến nhất. AI làm chậm bạn khi:</p>
<ul>
    <li>Task đủ đơn giản để bạn làm trực tiếp nhanh hơn</li>
    <li>Cần sự sáng tạo thực sự hoặc domain knowledge sâu mà AI không có</li>
    <li>Đang trong quá trình học — làm tay giúp bạn hiểu sâu hơn nhờ AI</li>
</ul>

<h2>Kết luận</h2>
<p>AI là công cụ khuếch đại, không phải công cụ thay thế. Developer giỏi nhất trong kỷ nguyên AI không phải người có nhiều AI tool nhất — mà là người <strong>biết dùng chúng đúng lúc, đúng chỗ, với mức độ tin tưởng phù hợp</strong>.</p>

<p>Muốn xem AI coding assistant hoạt động như thế nào trong thực tế? Xem bài <a href=""/blog/building-with-ai-experience-with-claude-code"">Trải nghiệm thực tế làm việc cùng Claude Code</a> trên codebase enterprise .NET.</p>",
                    ContentEn = @"<p><img src=""https://images.unsplash.com/photo-1677442135703-1787eea5ce01?w=1200&auto=format&fit=crop&q=80"" alt=""AI Skills for Developers — working with AI coding assistants"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>""Will AI replace programmers?"" — that question is no longer the point. The reality in 2025 is that the right question is: <strong>""Are developers who use AI well already working 3–5× faster than you?""</strong> In many cases, the answer is yes.</p>

<p>But working effectively with AI is not just ""asking ChatGPT questions"". It's a specific set of skills — learnable and practicable. This post covers the 5 most essential skills, drawn from over a year of using Claude Code daily on an enterprise codebase.</p>

<h2>1. Prompt Engineering — Communicating Effectively with AI</h2>
<p>This is the most foundational skill and also where most developers make their first mistake. AI doesn't read your mind — it only performs as well as what you describe.</p>

<p><strong>Weak prompt:</strong> <em>""fix this bug""</em> — AI doesn't know what the bug is, where it is, or what you expect.</p>

<p><strong>Strong prompt:</strong> <em>""This is an ASP.NET Core service. The <code>GetOrdersAsync</code> method is throwing <code>NullReferenceException</code> at line 47 when <code>customerId</code> doesn't exist in the DB. Stack trace: [paste]. Current code: [paste]. Expected behaviour: return an empty list instead of throwing. Find the root cause and fix it.""</em></p>

<p>The formula for an effective prompt: <strong>Context + Specific problem + Expected input/output + Constraints</strong>. Each missing element is something AI has to guess — and guessing wrong wastes your time.</p>

<h2>2. Context Management — Teaching AI to Understand Your Project</h2>
<p>AI coding assistants don't remember your project between sessions. Each time you start fresh, it's a blank slate. This is why a <strong>context file</strong> matters more than any individual prompt.</p>

<p>With Claude Code, I maintain a <code>CLAUDE.md</code> file in every project describing:</p>
<ul>
    <li>Overall architecture and layer boundaries (Controller → Service → Repo)</li>
    <li>Naming conventions and coding style</li>
    <li>Important technical decisions and their rationale</li>
    <li>What is explicitly <em>not allowed</em> (e.g. ""no AutoMapper"", ""no raw SQL in application code"")</li>
    <li>Project-specific gotchas and quirks</li>
</ul>

<p>Claude Code reads this file before doing anything — the result is code that matches your conventions from the first attempt, with far less rework.</p>

<h2>3. Critical Verification — Not Trusting Blindly</h2>
<p>AI confidence is a feature, not a signal of correctness. The ability to distinguish when AI is right from when it needs deeper scrutiny is what separates developers who use AI effectively from those who get misled by it.</p>

<p><strong>Always verify:</strong></p>
<ul>
    <li><strong>Security-sensitive code</strong>: authentication, authorization, encryption, input validation</li>
    <li><strong>Database migrations</strong>: never run an AI-generated script without reading every line</li>
    <li><strong>Payment and financial logic</strong>: a small error can cause irreversible consequences</li>
    <li><strong>Async/concurrent code</strong>: deadlocks and race conditions aren't caught at compile time</li>
</ul>

<p><strong>Relatively trustworthy:</strong> boilerplate code, test setup, simple CRUD, SCSS/styling, documentation.</p>

<p>Rule of thumb: verification effort is proportional to the consequence of the code being wrong.</p>

<h2>4. Iterative Decomposition — Breaking Down for Control</h2>
<p>This is the most common mistake among new AI users: asking to implement an entire feature in one shot. AI generates hundreds of lines of code — and you don't know where to start reviewing.</p>

<p>A more effective workflow:</p>
<ol>
    <li><strong>Analyse first:</strong> <em>""Analyse the approach to implement X, list the steps and risks""</em></li>
    <li><strong>Confirm direction:</strong> Agree on the approach before any code is written</li>
    <li><strong>Implement step by step:</strong> <em>""Implement step 1 first: create the data model and migration""</em></li>
    <li><strong>Review before continuing:</strong> Read step 1's output carefully, fix if needed, then move to step 2</li>
</ol>

<p>This approach catches errors earlier, ensures you understand the code being generated, and prevents out-of-scope changes that cause headaches later.</p>

<h2>5. Tool-Task Matching — Right Tool for the Right Job</h2>
<p>No AI tool is best at everything. Using the wrong tool significantly reduces the benefit:</p>

<table style=""width:100%;border-collapse:collapse;margin:1rem 0"">
<thead>
<tr style=""border-bottom:1px solid #333"">
<th style=""text-align:left;padding:0.5rem"">Tool</th>
<th style=""text-align:left;padding:0.5rem"">Best for</th>
<th style=""text-align:left;padding:0.5rem"">Avoid for</th>
</tr>
</thead>
<tbody>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Claude Code</strong></td>
<td style=""padding:0.5rem"">Large codebases, cross-layer debugging, refactoring, architecture review</td>
<td style=""padding:0.5rem"">Quick questions that don't need file reading</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>GitHub Copilot</strong></td>
<td style=""padding:0.5rem"">Inline completion in IDE, fast boilerplate</td>
<td style=""padding:0.5rem"">Tasks needing whole-codebase understanding</td>
</tr>
<tr style=""border-bottom:1px solid #222"">
<td style=""padding:0.5rem""><strong>Claude.ai / ChatGPT</strong></td>
<td style=""padding:0.5rem"">Brainstorming, explaining concepts, writing documentation</td>
<td style=""padding:0.5rem"">Tasks requiring access to actual project files</td>
</tr>
<tr>
<td style=""padding:0.5rem""><strong>Cursor</strong></td>
<td style=""padding:0.5rem"">Deep AI integration in IDE, prefer GUI workflow</td>
<td style=""padding:0.5rem"">Terminal-heavy workflows</td>
</tr>
</tbody>
</table>

<h2>Bonus: The Most Important Skill — Knowing When NOT to Use AI</h2>
<p>This is the least talked-about skill. AI slows you down when:</p>
<ul>
    <li>The task is simple enough to do directly faster than prompting</li>
    <li>You need genuine creativity or deep domain knowledge AI doesn't have</li>
    <li>You're actively learning — doing it by hand with AI guidance builds understanding better than having AI do it for you</li>
</ul>

<h2>Conclusion</h2>
<p>AI is an amplifier, not a replacement. The best developers in the AI era aren't the ones with the most AI tools — they're the ones who <strong>use them at the right time, in the right place, with the right level of trust</strong>.</p>

<p>Want to see AI coding assistants in action in a real codebase? Read about <a href=""/blog/building-with-ai-experience-with-claude-code"">a year of using Claude Code on an enterprise .NET codebase</a>.</p>"
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
                new Skill { Id = 21, Name = "CoffeeScript",      Category = "Frontend", Icon = "devicon-coffeescript-plain",  Proficiency = 60, Order = 21 },
                // DevOps extras
                new Skill { Id = 22, Name = "CI/CD",             Category = "DevOps",   Icon = "devicon-githubactions-plain", Proficiency = 80, Order = 22 },
                new Skill { Id = 23, Name = "Linux / Bash",      Category = "DevOps",   Icon = "devicon-linux-plain",         Proficiency = 75, Order = 23 },
                new Skill { Id = 24, Name = "Fly.io",            Category = "DevOps",   Icon = "devicon-flyio-plain",         Proficiency = 72, Order = 24 },
                // Backend extras
                new Skill { Id = 25, Name = "Entity Framework",  Category = "Backend",  Icon = "devicon-dotnetcore-plain",    Proficiency = 85, Order = 25 },
                new Skill { Id = 26, Name = "SignalR",           Category = "Backend",  Icon = "devicon-dotnetcore-plain",    Proficiency = 78, Order = 26 },
                new Skill { Id = 27, Name = "Elasticsearch",     Category = "Backend",  Icon = "devicon-elasticsearch-plain", Proficiency = 70, Order = 27 },
                new Skill { Id = 28, Name = "Hangfire",          Category = "Backend",  Icon = "devicon-dotnetcore-plain",    Proficiency = 75, Order = 28 },
                // Frontend extras
                new Skill { Id = 29, Name = "SCSS / Sass",       Category = "Frontend", Icon = "devicon-sass-plain",          Proficiency = 82, Order = 29 },
                new Skill { Id = 30, Name = "Bootstrap",         Category = "Frontend", Icon = "devicon-bootstrap-plain",     Proficiency = 78, Order = 30 }
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
