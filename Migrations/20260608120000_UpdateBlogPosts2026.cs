using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBlogPosts2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Content", "ContentEn", "Summary", "SummaryEn", "Tags", "Title", "TitleEn", "UpdatedAt" },
                values: new object[] { @"<p><img src=""https://images.unsplash.com/photo-1677442135703-1787eea5ce01?w=1200&auto=format&fit=crop&q=80"" alt=""Xây website thực tế với Claude Code và .NET 10 Blazor"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Khi tôi bắt đầu xây <strong>VodonghaPersonal.id.vn</strong> vào đầu năm 2026, tôi đặt ra một quy tắc cho bản thân: toàn bộ codebase sẽ được viết với Claude Code, không có exception. Không phải vì tôi lười — mà vì tôi muốn biết thật sự công cụ này làm được gì khi gặp một dự án thực tế, không phải tutorial demo.</p>

<p>Kết quả? Phức tạp hơn tôi nghĩ. Có những lúc tôi ngồi nhìn màn hình và nghĩ ""thứ này kỳ diệu thật"". Cũng có những lúc tôi muốn đập bàn. Bài viết này là toàn bộ câu chuyện đó — không cắt bỏ phần xấu.</p>

<h2>Stack kỹ thuật của VodonghaPersonal.id.vn</h2>

<p>Trước khi đi vào chi tiết, để tôi nói rõ stack tôi đang dùng:</p>

<ul>
  <li><strong>Blazor Web App .NET 10</strong> với InteractiveServer render mode</li>
  <li><strong>PostgreSQL</strong> qua Neon (Singapore region, suspend mode)</li>
  <li><strong>EF Core + Npgsql 10</strong> với <code>IDbContextFactory</code></li>
  <li><strong>SignalR</strong> cho live chat</li>
  <li><strong>Fly.io</strong> hosting (shared-cpu-1x, 256MB, Singapore)</li>
  <li><strong>QuestPDF 2026.5.0 + SkiaSharp</strong> cho tính năng xuất CV PDF</li>
  <li><strong>GitHub Actions</strong> CI/CD, SCSS, Chart.js</li>
</ul>

<p>Đây không phải todo app. Có auth, có live chat qua SignalR, có PDF generation với 3 template khác nhau, có chart analytics. Đủ phức tạp để Claude Code phải ""nghĩ"".</p>

<h2>Claude Code là gì và tại sao tôi chọn nó</h2>

<p>Claude Code ra mắt GA vào tháng 5/2025 và đến đầu 2026 đã có hơn 4.2 triệu developer dùng hàng tuần, triển khai tại hơn 1,400 tổ chức doanh nghiệp. Doanh thu đạt 2.5 tỷ USD annualized vào tháng 2/2026 — được gọi là ""fastest product ramp in enterprise software history"".</p>

<p>Nhưng con số không phải lý do tôi chọn nó. Lý do là: Claude Code được thiết kế để làm <strong>multi-file, multi-step tasks</strong> — đọc toàn bộ project, hiểu context, rồi thực hiện thay đổi xuyên suốt nhiều file cùng lúc. Đây chính xác là thứ tôi cần khi xây một web app từ đầu.</p>

<p>Nghiên cứu học thuật từ arxiv (2605.25438, tháng 5/2026) trên 5,838 developer trong 28 tháng cho thấy: ở tháng đầu dùng Claude Code, số commit tăng trung bình <strong>+40.7 commits/tháng — tương đương tăng 191%</strong> so với baseline 21.3 commits trước đó. Developer còn contribute thêm vào 1.5 repository mới và dùng thêm 0.83 ngôn ngữ lập trình mới.</p>

<h2>Những gì Claude Code làm tốt — ví dụ cụ thể</h2>

<h3>1. Setup boilerplate và CRUD layers</h3>

<p>Khi tôi mô tả entity <code>BlogPost</code> với các field cần thiết, Claude Code tạo ra: EF Core entity, migration, repository interface + implementation, service layer, controller, và cả DTO mapping — tất cả trong một lần. Không phải copy-paste từng file. Không phải nhớ xem interface cần declare method gì.</p>

<p>So sánh benchmark giữa Claude Code và GitHub Copilot cho .NET/C#:</p>

<ul>
  <li>CRUD implementation: Claude Code 5/5, Copilot 4/5</li>
  <li>Business logic across layers: Claude Code 5/5, Copilot 3/5</li>
  <li>Testing: Claude Code 5/5, Copilot 4/5</li>
  <li>Refactoring + explanation: Claude Code 5/5, Copilot 3/5</li>
  <li>Overall: <strong>24/25 vs 19/25</strong></li>
</ul>

<h3>2. Refactoring phức tạp</h3>

<p>Tôi cần chuyển từ <code>DbContext</code> trực tiếp sang <code>IDbContextFactory</code> pattern để hỗ trợ Blazor InteractiveServer (SignalR circuit không thể dùng scoped DbContext). Claude Code hiểu toàn bộ implication, đề xuất đúng pattern, và thực hiện thay đổi xuyên suốt tất cả repository và service — không bỏ sót file nào.</p>

<pre><code>// Trước — sai với Blazor InteractiveServer
public class BlogService(AppDbContext db) { ... }

// Sau — đúng pattern
public class BlogService(IDbContextFactory&lt;AppDbContext&gt; dbFactory)
{
    public async Task&lt;List&lt;BlogPost&gt;&gt; GetPublishedAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.BlogPosts
            .Where(p =&gt; p.IsPublished)
            .OrderByDescending(p =&gt; p.PublishedAt)
            .ToListAsync();
    }
}</code></pre>

<h3>3. Test generation với mock setup</h3>

<p>Claude Code viết test NUnit + FakeItEasy cho tôi với mock setup đúng cách. Khác với Copilot thường generate test compilable nhưng mock behavior sai, Claude Code hiểu được mục đích test và setup assertion có ý nghĩa.</p>

<h2>Những gì Claude Code làm sai — không phải phỏng đoán</h2>

<p>Đây là phần quan trọng nhất. Tôi đã gặp những vấn đề này, và chúng được documented trong các nghiên cứu độc lập — không phải tôi tự nghĩ ra.</p>

<h3>1. Blazor architectural discipline — vấn đề lớn nhất</h3>

<p>Blazor InteractiveServer có kiến trúc rất cụ thể: component chỉ là UI, logic nằm ở service, data access nằm ở repository. AI — kể cả Claude Sonnet, Claude Opus, ChatGPT-5 — đều có xu hướng nhét logic vào thẳng Razor component.</p>

<p>Ví dụ tôi gặp: tôi yêu cầu tạo component hiển thị danh sách blog post. Claude Code trả về component có <code>OnInitializedAsync</code> gọi thẳng <code>dbFactory.CreateDbContextAsync()</code>, query trực tiếp, format date trong component. Tôi phải explicitly yêu cầu tách ra nhiều lần.</p>

<p>Nghiên cứu từ Medium (mpholoane) cho thấy: kể cả khi có instruction rõ ràng về separation of concerns, các model AI vẫn default về pattern ""nhét vào component"". Đây là trap của Blazor — Razor component <em>cho phép</em> bạn làm vậy, nên AI làm theo.</p>

<p><strong>Giải pháp:</strong> Tôi tạo một file <code>CLAUDE.md</code> trong project với architectural rules rõ ràng. Mỗi session mới tôi đều reference file này. Không có file này, bạn sẽ phải nhắc lại mỗi lần.</p>

<h3>2. Context window saturation</h3>

<p>Khi project lớn dần (30+ file), Claude Code bắt đầu ""quên"" context từ đầu session. Tôi đã gặp tình huống nó thêm lại một UI element mà tôi đã yêu cầu bỏ đi — trong cùng một session. Instruction persistence là vấn đề thực.</p>

<h3>3. Tự đánh giá code của mình quá cao</h3>

<p>Đừng bao giờ nhờ Claude Code review code mà chính nó vừa viết. Một nghiên cứu cho thấy Claude Opus tự chấm code Blazor có vấn đề kiến trúc nghiêm trọng là 88/100. ChatGPT-5 review lại cùng code đó và cho 72/100, chỉ ra ""poor separation of concerns"". Đây là lý do bạn cần một bước review độc lập.</p>

<h3>4. The last 10% problem</h3>

<p>70-80% đầu tiên của một feature? Claude Code nhanh và chính xác. Nhưng phần còn lại — edge cases, auth integration, complex state management — ""takes 10x the effort because the AI doesn't truly understand the system it built."" Tôi thấy câu này đúng với trải nghiệm của mình.</p>

<h2>.NET 10 và Blazor: những thay đổi thực sự quan trọng</h2>

<p>.NET 10 ra mắt ngày 13/11/2025 (LTS, support đến 11/2028). Đây là những thay đổi Blazor quan trọng nhất với tôi:</p>

<h3>[PersistentState] attribute</h3>

<p>Đây là fix lớn nhất cho Blazor InteractiveServer. Trước đây, khi component prerender rồi reconnect qua SignalR, state bị mất — dẫn đến double fetch và flash UI. <code>[PersistentState]</code> serialize state vào prerendered HTML để restore khi reconnect.</p>

<pre><code>[PersistentState]
private List&lt;BlogPost&gt; _posts = [];

protected override async Task OnInitializedAsync()
{
    // Chỉ fetch nếu chưa có data từ prerender
    if (_posts.Count == 0)
    {
        _posts = await BlogService.GetPublishedAsync();
    }
}</code></pre>

<h3>Blazor script size giảm 76%</h3>

<p>Từ 183 KB xuống còn 43 KB nhờ precompression tự động và fingerprinting của <code>blazor.web.js</code>. Với site trên Fly.io shared-cpu-1x, 256MB RAM, đây là improvement thực sự.</p>

<h3>ReconnectModal component built-in</h3>

<p>Trước .NET 10, bạn phải tự wire reconnect overlay. Giờ nó có sẵn trong template — và quan trọng hơn, không còn vi phạm Content Security Policy như implementation cũ.</p>

<h3>EF Core 10: LeftJoin và RightJoin là first-class LINQ</h3>

<pre><code>// Thay vì GroupJoin/SelectMany/DefaultIfEmpty nightmare
var result = await db.Posts
    .LeftJoin(db.Tags,
        post =&gt; post.TagId,
        tag =&gt; tag.Id,
        (post, tag) =&gt; new { post.Title, TagName = tag != null ? tag.Name : ""Uncategorized"" })
    .ToListAsync();</code></pre>

<h2>Neon PostgreSQL và Fly.io: thực tế vận hành</h2>

<p>Neon với suspend mode (scale-to-zero) là lựa chọn tốt cho portfolio site về chi phí. Nhưng cold start latency là vấn đề thực tế — lần đầu tiên request vào một database đang suspend, bạn sẽ chờ 1-3 giây trong khi Neon wake up instance.</p>

<p>Cách tôi xử lý: tôi implement một ""warmup ping"" đơn giản trong <code>Program.cs</code> — sau khi app start, gửi một query nhẹ vào DB để wake nó dậy. Không elegant, nhưng hoạt động.</p>

<pre><code>// Program.cs — warmup Neon database on app start
app.Lifetime.ApplicationStarted.Register(async () =&gt;
{
    using var scope = app.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService&lt;IDbContextFactory&lt;AppDbContext&gt;&gt;();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.ExecuteSqlRawAsync(""SELECT 1"");
});
</code></pre>

<p>Fly.io với <code>shared-cpu-1x, 256MB</code> vẫn đủ cho portfolio site. Pricing thay đổi tháng 2/2026 với inter-region private network usage — nhưng không ảnh hưởng đến setup single-region của tôi.</p>

<p>Một gotcha quan trọng: SignalR stateful reconnect + Fly.io auto-scaling cần sticky sessions. Nếu bạn có nhiều hơn một instance, user có thể reconnect vào instance khác và mất circuit. Tôi dùng một instance nên không bị, nhưng đây là điều cần nhớ khi scale.</p>

<h2>QuestPDF + SkiaSharp: tính năng CV PDF</h2>

<p>Tính năng tôi thích nhất của site là export CV ra PDF với 3 template khác nhau. QuestPDF 2026.5.0 với SkiaSharp là stack production-ready trên .NET 10.</p>

<p>Claude Code làm tốt phần scaffold ban đầu — tạo document structure, define layout với FluentAPI của QuestPDF. Nhưng với custom graphics qua SkiaSharp canvas, tôi phải điều chỉnh tay nhiều hơn. AI không ""thấy"" output PDF, nên nó không biết khi nào layout sai.</p>

<pre><code>// QuestPDF 2026.5.0 — SkiaSharp custom canvas
.Canvas((canvas, size) =&gt;
{
    using var paint = new SKPaint
    {
        Color = SKColor.Parse(""#2563EB""),
        IsAntialias = true
    };
    canvas.DrawRoundRect(
        new SKRoundRect(new SKRect(0, 0, size.Width, size.Height), 8),
        paint);
});</code></pre>

<h2>Workflow thực tế: kết hợp Claude Code đúng cách</h2>

<p>Sau mấy tháng làm việc với Claude Code, workflow của tôi là:</p>

<ul>
  <li><strong>Dùng Claude Code cho:</strong> thiết kế kiến trúc, refactoring xuyên nhiều file, viết test, giải thích trade-off giữa các approach</li>
  <li><strong>Dùng Copilot (inline) cho:</strong> autocomplete nhanh khi đang trong flow, snippet đơn giản, không muốn context switch</li>
  <li><strong>Tự làm tay:</strong> auth flow, payment logic (nếu có), bất kỳ thứ gì mà ""sai một chút là hỏng cả hệ thống""</li>
</ul>

<p>Nghiên cứu Stack Overflow 2025 cho thấy 84% developer dùng hoặc có kế hoạch dùng AI tools, nhưng 46% không tin vào độ chính xác của output. Con số này tăng từ 31% năm 2024 — tức là càng nhiều người dùng, càng nhiều người hoài nghi. Đây là healthy skepticism, không phải vấn đề.</p>

<p>Quy tắc của tôi: <strong>AI viết draft, tôi review và chịu trách nhiệm.</strong> Không bao giờ commit code mà tôi không đọc và hiểu.</p>

<h2>FAQ</h2>

<h3>Claude Code có thực sự tăng năng suất hay chỉ là marketing?</h3>

<p>Tăng, nhưng không đều. Nghiên cứu độc lập trên 5,838 developer (28 tháng) cho thấy tăng 191% commit output ở tháng đầu dùng. Nhưng cũng có nghiên cứu khác (early 2025) cho thấy một số nhóm developer bị <em>chậm đi 19%</em> — do over-reliance và context-switching. Đến đầu 2026, cùng nhóm đó có +18% speedup — tức là có learning curve. Với tôi, lợi ích rõ ràng nhất là ở các task refactoring phức tạp và viết test, không phải ở code đơn giản.</p>

<h3>Blazor InteractiveServer có phù hợp để build portfolio site không?</h3>

<p>Phù hợp nếu bạn chấp nhận trade-off: cần kết nối WebSocket liên tục, cold start của Neon sẽ thêm latency lần đầu. Lợi thế là bạn dùng C# toàn stack, không cần học thêm framework JS. Với .NET 10, hai vấn đề lớn nhất đã được fix: prerender flash (<code>[PersistentState]</code>) và reconnect UI (<code>ReconnectModal</code>). Tôi hài lòng với lựa chọn này.</p>

<h3>Nên dùng Neon hay Fly.io Managed Postgres?</h3>

<p>Fly.io Managed Postgres thực ra là Neon ở backend (Fly.io đã integrate Neon từ 2025). Sự khác biệt là quản lý: Fly Managed Postgres có PgBouncer connection pooling sẵn và tích hợp tốt hơn với Fly.io networking. Neon trực tiếp có database branching (1 branch per PR) — rất hữu ích cho workflow development. Cho portfolio site nhỏ, cả hai đều ổn. Tôi chọn Neon trực tiếp vì branching.</p>

<h3>Claude Code có hiểu .NET 10 và Blazor mới không?</h3>

<p>Phần lớn là có, với một số hạn chế. Claude Code biết <code>[PersistentState]</code>, EF Core 10 LINQ operators, Npgsql 10 features. Nhưng nó đôi khi suggest pattern cũ (ví dụ: <code>GroupJoin/SelectMany</code> thay vì <code>LeftJoin</code>) nếu bạn không explicitly mention .NET 10. Luôn khai báo rõ version trong prompt hoặc <code>CLAUDE.md</code>.</p>

<h3>Mất bao lâu để xây VodonghaPersonal.id.vn với Claude Code?</h3>

<p>Khoảng 3-4 tuần part-time. Ước tính nếu làm tay hoàn toàn sẽ mất gấp đôi. Nhưng tôi cũng mất nhiều thời gian hơn dự kiến ở phần architecture refactoring — vì Claude Code tạo ra code hoạt động nhưng không đúng kiến trúc Blazor, và tôi phải sửa lại. Bài học: đầu tư vào <code>CLAUDE.md</code> từ đầu tiết kiệm rất nhiều thời gian sau.</p>

<h2>Kết luận</h2>

<p>Xây <strong>VodonghaPersonal.id.vn</strong> với Claude Code là trải nghiệm thực sự khai sáng — theo cả nghĩa tốt và xấu. Công cụ này là real productivity multiplier khi bạn dùng đúng chỗ. Nhưng nó không thay thế được hiểu biết kiến trúc, và nó không tự review được chính nó.</p>

<p>Điều quan trọng nhất tôi học được: <strong>AI viết code nhanh hơn bạn, nhưng bạn phải hiểu code đó sâu hơn AI.</strong> Vì khi production có vấn đề lúc 2 giờ sáng, bạn mới là người ngồi debug — không phải Claude Code.</p>

<p>Nếu bạn đang tìm hiểu về AI-assisted development, hãy đọc thêm:</p>
<ul>
  <li><a href=""/blog/vibe-coding-la-gi"">Vibe Coding là gì? Lập trình trong kỷ nguyên AI</a></li>
  <li><a href=""/blog/ai-skills-for-developers"">Kỹ năng AI nào developer cần thật sự có trong 2026</a></li>
  <li><a href=""/blog/deploy-dotnet-fly-io-2025"">Deploy .NET app lên Fly.io: hướng dẫn thực tế</a></li>
  <li><a href=""/blog/blazor-vs-react-2025"">Blazor vs React 2025: chọn gì cho dự án mới?</a></li>
  <li><a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL + EF Core: Best practices tôi dùng trong production</a></li>
</ul>", @"<p><img src=""https://images.unsplash.com/photo-1677442135703-1787eea5ce01?w=1200&auto=format&fit=crop&q=80"" alt=""Building a real website with Claude Code and .NET 10 Blazor"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>When I started building <strong>VodonghaPersonal.id.vn</strong> in early 2026, I set one rule for myself: the entire codebase would be written with Claude Code — no exceptions. Not because I wanted to take shortcuts, but because I wanted to know what this tool actually does when it encounters a real project, not a tutorial demo.</p>

<p>The result was more complicated than I expected. There were moments where I stared at the screen thinking ""this is genuinely remarkable."" There were also moments of real frustration. This post is the complete story — I am not cutting out the bad parts.</p>

<h2>The Tech Stack of VodonghaPersonal.id.vn</h2>

<p>Before going into the details, here is exactly what I built with:</p>

<ul>
  <li><strong>Blazor Web App .NET 10</strong> with InteractiveServer render mode</li>
  <li><strong>PostgreSQL</strong> via Neon (Singapore region, suspend mode)</li>
  <li><strong>EF Core + Npgsql 10</strong> with <code>IDbContextFactory</code></li>
  <li><strong>SignalR</strong> for live chat</li>
  <li><strong>Fly.io</strong> hosting (shared-cpu-1x, 256MB RAM, Singapore)</li>
  <li><strong>QuestPDF 2026.5.0 + SkiaSharp</strong> for CV PDF export</li>
  <li><strong>GitHub Actions</strong> CI/CD, SCSS, Chart.js</li>
</ul>

<p>This is not a todo app. It has authentication, live chat via SignalR, PDF generation with three distinct templates, and chart-based analytics. Complex enough to make Claude Code actually think.</p>

<h2>What Claude Code Is and Why I Chose It</h2>

<p>Claude Code went GA in May 2025 and by early 2026 had over 4.2 million weekly active developers using it, deployed at 1,400+ enterprise engineering organizations. It hit $2.5 billion annualized revenue in February 2026 — described as the fastest product ramp in enterprise software history.</p>

<p>But statistics were not my reason for choosing it. My reason was that Claude Code was designed specifically for <strong>multi-file, multi-step tasks</strong> — reading the entire project, understanding context, then making coordinated changes across multiple files simultaneously. That is exactly what building a web app from scratch requires.</p>

<p>An independent academic study (arxiv 2605.25438, May 2026) covering 5,838 developers over 28 months found that in the first month of adopting Claude Code, commit output increased by an average of <strong>+40.7 commits — a 191% increase</strong> over the pre-adoption baseline of 21.3 commits. Developers also contributed to 1.5 additional repositories and used 0.83 new programming languages.</p>

<h2>What Claude Code Does Well — Concrete Examples</h2>

<h3>1. Boilerplate and CRUD layers</h3>

<p>When I described a <code>BlogPost</code> entity with the fields I needed, Claude Code generated: EF Core entity, migration, repository interface and implementation, service layer, controller, and DTO mapping — in a single pass. No copy-pasting between files. No mental overhead about which methods to declare on which interface.</p>

<p>A published benchmark comparing Claude Code and GitHub Copilot for .NET/C# shows:</p>

<ul>
  <li>CRUD implementation: Claude Code 5/5, Copilot 4/5</li>
  <li>Business logic across layers: Claude Code 5/5, Copilot 3/5</li>
  <li>Testing: Claude Code 5/5, Copilot 4/5</li>
  <li>Refactoring and explanation: Claude Code 5/5, Copilot 3/5</li>
  <li>Overall: <strong>24/25 vs 19/25</strong></li>
</ul>

<h3>2. Complex refactoring</h3>

<p>I needed to migrate from direct <code>DbContext</code> injection to <code>IDbContextFactory</code> to properly support Blazor InteractiveServer (a scoped DbContext cannot survive a SignalR circuit). Claude Code understood all the implications, proposed the correct pattern, and implemented changes across every repository and service — without missing a file.</p>

<pre><code>// Before — incorrect with Blazor InteractiveServer
public class BlogService(AppDbContext db) { ... }

// After — correct pattern
public class BlogService(IDbContextFactory&lt;AppDbContext&gt; dbFactory)
{
    public async Task&lt;List&lt;BlogPost&gt;&gt; GetPublishedAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.BlogPosts
            .Where(p =&gt; p.IsPublished)
            .OrderByDescending(p =&gt; p.PublishedAt)
            .ToListAsync();
    }
}</code></pre>

<h3>3. Test generation with proper mock setup</h3>

<p>Claude Code wrote NUnit + FakeItEasy tests with correctly configured mock behavior. Where Copilot often generates tests that compile but have subtly wrong mock behavior (parameters ignored, assertions missing), Claude Code understood the intent and produced meaningful assertions.</p>

<h2>What Claude Code Gets Wrong — Not Speculation</h2>

<p>This is the most important section. I encountered these problems directly, and they are independently documented in published research — not just my personal frustration.</p>

<h3>1. Blazor architectural discipline — the biggest problem</h3>

<p>Blazor InteractiveServer has a clear architectural contract: components are pure UI, logic lives in services, data access lives in repositories. AI models — including Claude Sonnet, Claude Opus, and ChatGPT-5 — all default to embedding logic directly in Razor components.</p>

<p>An example I experienced: I asked Claude Code to create a component that displays a list of blog posts. It returned a component with <code>OnInitializedAsync</code> calling <code>dbFactory.CreateDbContextAsync()</code> directly, querying inline, formatting dates inside the component. I had to explicitly ask for separation multiple times across multiple sessions.</p>

<p>Research published on Medium (mpholoane) confirmed this: even with explicit separation-of-concerns instructions, all tested AI models defaulted back to the ""dump it in the component"" pattern. Blazor makes this worse because Razor components syntactically allow you to do it — so the AI does.</p>

<p><strong>The fix:</strong> I created a <code>CLAUDE.md</code> file at the project root with architectural rules written explicitly. I reference this file at the start of every new session. Without it, you will repeat yourself endlessly.</p>

<h3>2. Context window saturation</h3>

<p>As the project grew past 30 files, Claude Code started ""forgetting"" context from earlier in a session. I encountered a case where it added back a UI element I had explicitly asked it to remove — in the same session. Instruction persistence is a real problem, not an edge case.</p>

<h3>3. Self-assessment inflation</h3>

<p>Never ask Claude Code to review code it just wrote. One study found that Claude Opus rated architecturally flawed Blazor code 88/100. When ChatGPT-5 reviewed the same code independently, it gave 72/100 and called out ""poor separation of concerns."" The model that wrote the code cannot reliably critique it.</p>

<h3>4. The last 10% problem</h3>

<p>The first 70-80% of a feature? Claude Code is fast and accurate. But edge cases, auth integration, and complex state management: this part ""takes 10x the effort because the AI doesn't truly understand the system it built."" I found this to be an accurate description of my experience.</p>

<h2>.NET 10 and Blazor: Changes That Actually Matter</h2>

<p>.NET 10 released November 13, 2025 (LTS, supported through November 2028). These are the Blazor changes that meaningfully impacted my work:</p>

<h3>The [PersistentState] attribute</h3>

<p>This is the most significant fix for Blazor InteractiveServer. Previously, when a component prerendered and then reconnected via SignalR, state was lost — causing double data fetches and a UI flash. <code>[PersistentState]</code> serializes state into the prerendered HTML so it can be restored on reconnect.</p>

<pre><code>[PersistentState]
private List&lt;BlogPost&gt; _posts = [];

protected override async Task OnInitializedAsync()
{
    // Only fetch if state was not restored from prerender
    if (_posts.Count == 0)
    {
        _posts = await BlogService.GetPublishedAsync();
    }
}</code></pre>

<h3>Blazor script size reduced by 76%</h3>

<p>From 183 KB down to 43 KB through automatic precompression and fingerprinting of <code>blazor.web.js</code>. On a Fly.io shared-cpu-1x instance with 256MB RAM, this is a meaningful improvement in practice.</p>

<h3>Built-in ReconnectModal component</h3>

<p>Before .NET 10, you had to wire the reconnection overlay manually — and the existing implementation caused Content Security Policy violations. The new <code>ReconnectModal</code> ships in the project template and resolves the CSP problem.</p>

<h3>EF Core 10: LeftJoin and RightJoin as first-class LINQ</h3>

<pre><code>// Instead of the GroupJoin/SelectMany/DefaultIfEmpty pattern
var result = await db.Posts
    .LeftJoin(db.Tags,
        post =&gt; post.TagId,
        tag =&gt; tag.Id,
        (post, tag) =&gt; new { post.Title, TagName = tag != null ? tag.Name : ""Uncategorized"" })
    .ToListAsync();</code></pre>

<h2>Neon PostgreSQL and Fly.io: Operational Reality</h2>

<p>Neon's suspend mode (scale-to-zero) is excellent for cost management on a portfolio site. But cold start latency is a real user experience problem — when the database is suspended and the first request comes in, you are waiting 1-3 seconds while Neon wakes the instance.</p>

<p>My solution was a warmup ping in <code>Program.cs</code> — after the app starts, fire a lightweight query to wake the database before the first user request arrives:</p>

<pre><code>// Program.cs — warmup Neon on app start
app.Lifetime.ApplicationStarted.Register(async () =&gt;
{
    using var scope = app.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService&lt;IDbContextFactory&lt;AppDbContext&gt;&gt;();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.ExecuteSqlRawAsync(""SELECT 1"");
});</code></pre>

<p>Fly.io with shared-cpu-1x, 256MB is sufficient for a portfolio site. The February 2026 pricing change (inter-region private network usage now charged) does not affect a single-region deployment.</p>

<p>One important gotcha: SignalR stateful reconnect combined with Fly.io auto-scaling requires sticky sessions. If you have more than one instance running, a user may reconnect to a different instance and lose their circuit. I run a single instance so this was not a problem for me — but it is essential to know before you scale.</p>

<h2>QuestPDF + SkiaSharp: The CV PDF Feature</h2>

<p>The feature I am most satisfied with is exporting the CV to PDF in three different templates. QuestPDF 2026.5.0 with SkiaSharp is production-ready on .NET 10.</p>

<p>Claude Code handled the initial document structure and FluentAPI layout well. But for custom graphics work via the SkiaSharp canvas API, I had to intervene manually much more often. The AI cannot see the PDF output, so it has no way to know when the layout is visually wrong.</p>

<pre><code>// QuestPDF 2026.5.0 — SkiaSharp custom canvas
.Canvas((canvas, size) =&gt;
{
    using var paint = new SKPaint
    {
        Color = SKColor.Parse(""#2563EB""),
        IsAntialias = true
    };
    canvas.DrawRoundRect(
        new SKRoundRect(new SKRect(0, 0, size.Width, size.Height), 8),
        paint);
});</code></pre>

<h2>Practical Workflow: How to Use Claude Code Effectively</h2>

<p>After months of working with Claude Code on this project, my actual workflow is:</p>

<ul>
  <li><strong>Use Claude Code for:</strong> architectural design, cross-file refactoring, test writing, reasoning through trade-offs between approaches</li>
  <li><strong>Use Copilot (inline) for:</strong> fast autocomplete while in flow state, simple snippets, cases where context-switching to Claude Code would break concentration</li>
  <li><strong>Do by hand:</strong> auth flows, anything with payment data, any logic where ""slightly wrong"" causes systemic failure</li>
</ul>

<p>The Stack Overflow 2025 Developer Survey found 84% of developers use or plan to use AI tools — but 46% do not trust AI tool output accuracy, up from 31% in 2024. More developers actively distrust (46%) than trust (33%). This is healthy skepticism from people who have used these tools in production.</p>

<p>My rule: <strong>AI writes the draft, I review it and own it.</strong> I do not commit code I have not read and understood. The AI works faster than me, but I am the one debugging production at 2 AM.</p>

<h2>FAQ</h2>

<h3>Does Claude Code actually improve productivity, or is it just marketing?</h3>

<p>It does improve productivity, but not uniformly. The independent academic study of 5,838 developers over 28 months found a 191% increase in commit output at adoption. But a separate study from early 2025 found some developer cohorts actually slowed down by 19% — due to over-reliance and context-switching overhead. By early 2026, the same cohort showed an 18% speedup, confirming there is a genuine learning curve. In my experience, the clearest benefits are on complex refactoring tasks and test writing, not on simple boilerplate.</p>

<h3>Is Blazor InteractiveServer a good choice for a portfolio site?</h3>

<p>It works well if you accept the trade-offs: you need a persistent WebSocket connection, and Neon's cold start will add latency on the first request. The upside is a full C# stack with no JavaScript framework to learn. With .NET 10, the two biggest historical pain points are fixed: the prerender flash (<code>[PersistentState]</code>) and the reconnect UI (<code>ReconnectModal</code>). I am satisfied with the choice.</p>

<h3>Should I use Neon directly or Fly.io Managed Postgres?</h3>

<p>Fly.io Managed Postgres is Neon under the hood — Fly.io integrated Neon as its managed Postgres backend in 2025. The practical difference is in management: Fly Managed Postgres includes PgBouncer connection pooling out of the box and integrates more tightly with Fly.io networking. Direct Neon gives you database branching (one branch per PR for isolated test environments), which is genuinely useful for a development workflow. For a small portfolio site, either is fine. I chose direct Neon for the branching.</p>

<h3>Does Claude Code actually understand .NET 10 and the newer Blazor APIs?</h3>

<p>Mostly yes, with caveats. Claude Code knows about <code>[PersistentState]</code>, the new EF Core 10 LINQ operators, and Npgsql 10 features. But it will sometimes suggest older patterns — for instance, <code>GroupJoin/SelectMany/DefaultIfEmpty</code> instead of the new <code>LeftJoin</code> operator — unless you explicitly mention .NET 10 in your prompt. Always declare your target version clearly in your prompt or in a <code>CLAUDE.md</code> project context file.</p>

<h3>How long did it take to build VodonghaPersonal.id.vn with Claude Code?</h3>

<p>About 3-4 weeks part-time. My estimate for doing it entirely by hand is roughly twice that. However, I also spent more time than expected on architecture refactoring — Claude Code produced working code that did not conform to proper Blazor layering, and fixing that took real effort. The lesson: invest in a solid <code>CLAUDE.md</code> from day one. It saves significantly more time than it costs.</p>

<h2>Conclusion</h2>

<p>Building <strong>VodonghaPersonal.id.vn</strong> with Claude Code was a genuinely educational experience — in both directions. The tool is a real productivity multiplier when you use it for the right tasks. But it does not replace architectural understanding, and it cannot review itself honestly.</p>

<p>The most important thing I learned: <strong>AI writes code faster than you, but you need to understand that code more deeply than the AI does.</strong> When something breaks in production, you are the one debugging it — not Claude Code.</p>

<p>If you are exploring AI-assisted development further, these posts are worth reading next:</p>
<ul>
  <li><a href=""/blog/vibe-coding-la-gi"">What Is Vibe Coding? Programming in the AI Era</a></li>
  <li><a href=""/blog/ai-skills-for-developers"">Which AI Skills Do Developers Actually Need in 2026</a></li>
  <li><a href=""/blog/deploy-dotnet-fly-io-2025"">Deploying a .NET App to Fly.io: A Practical Guide</a></li>
  <li><a href=""/blog/blazor-vs-react-2025"">Blazor vs React in 2025: What to Choose for a New Project</a></li>
  <li><a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL + EF Core: Best Practices I Use in Production</a></li>
</ul>", @"Tôi đã dùng Claude Code để xây VodonghaPersonal.id.vn từ đầu — .NET 10, Blazor InteractiveServer, Neon PostgreSQL, Fly.io. Đây là những gì thực sự xảy ra: điểm mạnh, điểm yếu, và bài học thực chiến.", @"I built VodonghaPersonal.id.vn entirely with Claude Code — .NET 10, Blazor InteractiveServer, Neon PostgreSQL, Fly.io. Here is what actually happened: the wins, the failures, and the lessons learned in production.", @"claude-code, blazor, dotnet, ai-development, fly-io, neon-postgresql, full-stack", @"Kinh nghiệm xây website thực tế với Claude Code (.NET 10 + Blazor)", @"Building a Real Website with Claude Code (.NET 10 + Blazor): My Experience", new DateTime(2026, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Content", "ContentEn", "Summary", "SummaryEn", "Tags", "Title", "TitleEn", "UpdatedAt" },
                values: new object[] { @"<p><img src=""https://images.unsplash.com/photo-1655720828018-edd2daec9349?w=1200&auto=format&fit=crop&q=80"" alt=""AI Skills for Developers in 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Đầu năm 2026, khi tôi ngồi nhìn lại codebase của <strong>VodonghaPersonal.id.vn</strong> — một portfolio site Blazor .NET 10 chạy trên Fly.io với PostgreSQL Neon — tôi nhận ra một điều thú vị: gần như toàn bộ cái site này được build bằng Claude Code. Không phải ""AI hỗ trợ một chút"", mà là thực sự dùng AI agent để viết code, refactor, debug, và deploy từ đầu đến cuối.</p>

<p>Theo khảo sát 15,000 developer năm 2026, <strong>73% team kỹ thuật đang dùng AI coding tools hàng ngày</strong> — tăng từ 41% năm 2025 và 18% năm 2024. Con số này không còn là xu hướng nữa, nó là thực tế. Vấn đề không còn là ""có nên dùng AI không"" mà là ""dùng như thế nào cho đúng"".</p>

<p>Trong bài này mình sẽ chia sẻ 5 điều thực tế nhất mà một developer cần biết về AI trong năm 2026, dựa trên kinh nghiệm trực tiếp và dữ liệu thực từ cộng đồng.</p>

<h2>1. Bức tranh toàn cảnh AI coding tools — ai đang dẫn đầu?</h2>

<p>Năm 2026, cuộc chiến AI coding tools đã có kết quả rõ ràng hơn. Đây là những tên tuổi chính bạn cần biết:</p>

<h3>Claude Code — kẻ lên ngôi bất ngờ</h3>
<p>Ra mắt tháng 5/2025 dưới dạng terminal-based agentic coding tool, Claude Code đã làm điều không ai ngờ: chỉ trong 8 tháng, nó vượt qua cả GitHub Copilot (đã có mặt từ 2021) lẫn Cursor về mức độ hài lòng của developer. Tính đến tháng 2/2026, Claude Code chiếm <strong>41% thị phần</strong> so với Copilot 38% trong nhóm professional developers. 71% developer dùng AI agent coi Claude Code là công cụ chính.</p>
<p>Điểm mạnh nhất: <strong>95% first-try correctness rate</strong> — cao nhất trong số các agent được test. Đây chính là lý do mình chọn Claude Code để build toàn bộ VodonghaPersonal.id.vn, từ Blazor components, EF Core migrations, cho đến CI/CD pipeline trên GitHub Actions.</p>
<p>File quan trọng nhất khi dùng Claude Code là <code>CLAUDE.md</code> — đây là nơi bạn khai báo project instructions, coding standards, architecture decisions. Agent sẽ đọc file này ở mỗi session. Một CLAUDE.md tốt có thể là sự khác biệt giữa AI hiểu context của bạn và AI viết code không ăn nhập gì với codebase hiện tại.</p>

<h3>GitHub Copilot — vẫn phổ biến nhờ zero switching cost</h3>
<p>$10/tháng, tích hợp thẳng vào VS Code, JetBrains, Visual Studio. Default model là GPT-4o nhưng giờ bạn có thể chọn Claude Sonnet 4.6 hoặc Gemini 2.5 Pro. SWE-bench đạt 56.0% (tháng 3/2026). Từ 1/6/2026, Copilot chuyển sang billing theo usage. Ưu điểm lớn nhất vẫn là không cần đổi IDE.</p>

<h3>Cursor — IDE của dân senior</h3>
<p>$20/tháng, VS Code fork được AI hóa toàn diện. SWE-bench 51.7%, inline suggestions nhanh hơn Copilot 15–25ms. Hỗ trợ GPT-5.4, Claude Opus 4.6, Gemini 3 Pro, Grok Code. Stack được senior developers đồng thuận nhất năm 2026: <strong>Cursor cho daily work + Claude Code cho complex tasks</strong>, tốn khoảng $40/tháng.</p>

<h3>Gemini Code Assist — free tier hào phóng nhất</h3>
<p>6,000 completions/ngày (180,000/tháng) — nhiều hơn 90 lần so với free tier của Copilot. Nếu bạn đang bắt đầu và chưa muốn trả tiền, đây là lựa chọn đáng thử.</p>

<h2>2. Sự thật về năng suất — không phải lúc nào cũng như bạn nghĩ</h2>

<p>Đây là phần nhiều developer không muốn nghe, nhưng cần biết.</p>

<p>84–93% developer dùng AI tools và tự báo cáo tiết kiệm được ~4 giờ/tuần. Nghe hay đấy. Nhưng <strong>nghiên cứu thực tế của METR</strong> — đo lường trực tiếp, không phải self-report — cho thấy điều ngược lại: developers mất nhiều hơn <strong>19% thời gian</strong> khi dùng AI so với không dùng, trong khi bản thân họ vẫn tin rằng mình nhanh hơn 20%. Đây là perception gap kinh điển.</p>

<p>Tại sao? Một vài lý do thực tế:</p>
<ul>
  <li>Thời gian đọc và verify code AI tạo ra không được tính vào cảm nhận chủ quan</li>
  <li>AI hay tạo ra boilerplate nhanh nhưng lại làm chậm ở phần logic phức tạp cần context sâu</li>
  <li>41% code được viết năm 2025 là AI-generated; 26.9% code production hiện tại là do AI viết — nhưng chất lượng không đồng đều</li>
</ul>

<p>Điều này không có nghĩa là AI vô dụng. Năng suất tăng ~10% ở mức ổn định khi dùng đúng cách. Vấn đề là kỳ vọng không thực tế. Hãy dùng AI như một junior developer rất nhanh tay nhưng cần review cẩn thận — không phải như một senior có thể tự tin giao hết việc.</p>

<h2>3. Prompt Engineering năm 2026 — đã tiến hóa, không biến mất</h2>

<p>Vai trò ""Prompt Engineer"" độc lập đã gần như biến mất (Fast Company, 5/2025). 68% công ty hiện coi prompt engineering là kỹ năng cơ bản cho mọi vị trí, không phải chuyên ngành riêng. Nhưng điều đó không có nghĩa là prompting không còn quan trọng — ngược lại, nó đã trở nên tinh tế hơn nhiều.</p>

<h3>Framework 4 tín hiệu thực tế</h3>
<p>Cấu trúc prompt hiệu quả nhất hiện tại: <strong>Role + Context + Task + Format</strong>.</p>

<pre><code>You are a senior .NET developer reviewing a Blazor WASM component.
Context: This component renders a paginated list of bank transactions using
EF Core 10 with SQL Server. Performance is critical — up to 10,000 rows.
Task: Review the following code for N+1 query issues and suggest fixes.
Format: List issues as bullet points, then provide corrected code snippet.</code></pre>

<p>Điểm quan trọng về độ dài: LLM reasoning suy giảm rõ rệt sau khoảng 3,000 tokens. Practical sweet spot là <strong>150–300 từ</strong> cho một prompt. Đừng paste cả file 500 dòng vào rồi hỏi ""có vấn đề gì không?"".</p>

<h3>Context Engineering — kỹ năng thực sự cần có</h3>
<p>Từ ""prompt engineering"" đang dần nhường chỗ cho ""context engineering"". Theo LangChain, có 4 chiến lược quản lý context:</p>
<ul>
  <li><strong>Write:</strong> Lưu thông tin vào memory/files để dùng lại (CLAUDE.md là ví dụ điển hình)</li>
  <li><strong>Select (RAG):</strong> Chỉ lấy đúng phần context cần thiết, không dump tất cả</li>
  <li><strong>Compress:</strong> Tóm tắt context cũ để giữ window sạch</li>
  <li><strong>Isolate:</strong> Tách task phức tạp thành sub-tasks với context riêng biệt</li>
</ul>

<p>Khi mình build tính năng live chat với SignalR cho VodonghaPersonal.id.vn, thay vì paste toàn bộ codebase vào Claude Code, mình dùng CLAUDE.md để khai báo architecture và chỉ provide context của component liên quan. Kết quả tốt hơn hẳn.</p>

<h3>Treat prompting như system design</h3>
<p>Cách tư duy tốt nhất: xem prompt như một interface — định nghĩa input, output, constraints, failure modes, và evaluation criteria. Đừng viết prompt như đang chat; viết như đang viết API spec.</p>

<h2>4. AI Code Review — khi nào KHÔNG nên tin</h2>

<p>Đây là phần quan trọng nhất mà ít người nói thẳng.</p>

<p>Số liệu thực tế năm 2026:</p>
<ul>
  <li>76% developer gặp AI hallucinations thường xuyên</li>
  <li>29–45% code AI-generated chứa security vulnerabilities</li>
  <li>~20% package recommendations của AI trỏ đến libraries không tồn tại</li>
  <li>59% developer dùng AI-generated code mà bản thân không hiểu đầy đủ (Clutch.co)</li>
  <li>65% nói ""thiếu context"" là vấn đề lớn nhất — vượt cả hallucinations</li>
</ul>

<h3>Vấn đề ""AI reviewing AI""</h3>
<p>Một anti-pattern nguy hiểm đang phổ biến: dùng AI để review code do AI tạo ra. Vấn đề là các model được train trên data tương tự nhau, dẫn đến chúng thường đồng ý với nhau ngay cả khi cả hai đều sai. <strong>Consensus không bằng correctness</strong>. Đến năm 2026, lượng AI-generated code được dự báo vượt quá capacity review của con người đến 40% — gọi là ""AI code generation gap"".</p>

<h3>Security là rủi ro thực, không phải lý thuyết</h3>
<p>Trong codebase .NET, những lỗi AI thường mắc phải:</p>
<ul>
  <li>SQL injection qua string interpolation (dù EF Core, AI đôi khi gợi ý raw SQL không safe)</li>
  <li>Missing authorization checks — AI tạo controller action mà quên <code>[Authorize]</code></li>
  <li>Insecure deserialization khi xử lý JSON từ external sources</li>
  <li>Logging secrets — AI hay log toàn bộ request object bao gồm cả sensitive fields</li>
</ul>

<pre><code>// AI-generated code — DANGEROUS, đừng dùng
public async Task&lt;IActionResult&gt; GetUser(string id)
{
    // AI quên authorization check
    var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Id == id); // Không check tenant isolation
    return Ok(user);
}

// Corrected version
[Authorize]
public async Task&lt;IActionResult&gt; GetUser(string id)
{
    var currentTenant = User.GetTenantId();
    var user = await _db.Users
        .Where(u => u.TenantId == currentTenant && u.Id == id)
        .FirstOrDefaultAsync();
    return user == null ? NotFound() : Ok(user);
}</code></pre>

<p>Rule của mình: bất kỳ code AI viết liên quan đến authentication, authorization, hoặc data access đều phải được review bởi người, không phải AI khác.</p>

<h2>5. Tác động đến sự nghiệp — thực tế hơn bạn nghĩ</h2>

<p>28% vị trí entry-level đã giảm so với đỉnh năm 2022. 65% developer kỳ vọng vai trò của mình sẽ được định nghĩa lại trong năm 2026 — từ ""viết code"" sang ""kiến trúc, tích hợp, và ra quyết định AI-enabled"". Đây là xu hướng thực, không phải FUD.</p>

<p>Nhưng US Bureau of Labor Statistics vẫn dự báo <strong>15% tăng trưởng việc làm cho software developer</strong> — cao hơn nhiều mức trung bình. Không phải AI thay developer; AI thay những developer không biết dùng AI.</p>

<h3>Những kỹ năng đang tăng giá trị</h3>
<ul>
  <li><strong>System design:</strong> Khi AI có thể viết code, khả năng thiết kế hệ thống đúng trở nên quý hơn bao giờ hết</li>
  <li><strong>Security:</strong> AI tạo ra nhiều code hơn nhưng không tự động tạo ra code an toàn hơn</li>
  <li><strong>AI integration:</strong> Biết cách tích hợp LLM, RAG, vector search vào sản phẩm</li>
  <li><strong>Data engineering:</strong> Hiểu data pipeline để feed cho AI systems</li>
</ul>

<h3>Con đường junior developer đang thay đổi</h3>
<p>Entry-level giảm không có nghĩa là không có cơ hội. Junior developer năm 2026 cần học nhanh hơn vì AI giúp giải quyết boilerplate — nhưng điều đó cũng có nghĩa là senior có thể kỳ vọng họ ramp up nhanh hơn. Những junior biết dùng AI để học (không phải để chép) sẽ có lợi thế lớn.</p>

<h2>6. Stack thực tế của mình — .NET 10 + AI workflow</h2>

<p>.NET 10 ra tháng 11/2025 (LTS, support đến 11/2028) với nhiều cải thiện hiệu suất: ít allocations hơn, better inlining, devirtualization tốt hơn, AVX 10.2 support, và Blazor WASM preloading. EF Core 10 thêm <code>LeftJoin</code>/<code>RightJoin</code> native, JSON columns, <strong>vector search không còn experimental</strong> (dùng <code>VECTOR_DISTANCE()</code> trên SQL Server 2025/Azure SQL).</p>

<p>Workflow của mình với Claude Code cho VodonghaPersonal.id.vn:</p>
<ol>
  <li>Viết <code>CLAUDE.md</code> chi tiết: tech stack, coding conventions, database schema, deployment target</li>
  <li>Describe task theo format Role + Context + Task + Format</li>
  <li>Review output — đặc biệt phần auth, data access, và external dependencies</li>
  <li>Chạy tests trước khi merge (GitHub Actions pipeline)</li>
  <li>Với code security-critical: always human review, không delegate cho AI</li>
</ol>

<p>Cost thực tế: ~$20/tháng Claude Code + $0 Fly.io (free tier cho dev) + $0 Neon (free tier, suspend mode). Rẻ hơn nhiều so với thuê freelancer để build từng feature nhỏ.</p>

<h2>7. Những sai lầm phổ biến cần tránh</h2>

<ul>
  <li><strong>Tin AI 100% với code bạn không hiểu:</strong> 59% developer làm vậy — đừng là người trong số đó. Nếu bạn không thể giải thích code hoạt động như thế nào, đừng deploy nó.</li>
  <li><strong>Dùng AI không có context:</strong> ""Write me a user authentication system"" không đủ. Specify: .NET 9, JWT, multi-tenant, SQL Server, existing patterns trong codebase.</li>
  <li><strong>Bỏ qua CLAUDE.md / system prompt:</strong> Không đầu tư vào onboarding file là lãng phí tiềm năng của agent.</li>
  <li><strong>Dùng AI review code do AI viết:</strong> Đây là circular validation. AI reviewing AI là anti-pattern.</li>
  <li><strong>Kỳ vọng AI giải quyết được mọi thứ:</strong> AI giỏi ở boilerplate, patterns quen thuộc, và refactoring. AI yếu ở domain-specific business logic phức tạp và novel architecture decisions.</li>
</ul>

<h2>8. FAQ — Câu hỏi thường gặp</h2>

<h3>Claude Code hay GitHub Copilot, tôi nên chọn cái nào?</h3>
<p>Nếu bạn đang dùng VS Code hoặc JetBrains và chỉ muốn thêm AI assistance mà không đổi workflow, Copilot ($10/tháng) là lựa chọn ít ma sát nhất. Nếu bạn muốn làm những task phức tạp hơn — refactor toàn bộ module, viết tests, debug multi-file issues — Claude Code có first-try correctness rate cao hơn và phù hợp hơn cho agentic workflows. Nhiều senior developer dùng cả hai: Cursor cho daily coding, Claude Code cho complex tasks.</p>

<h3>Prompt engineering còn quan trọng không khi AI ngày càng thông minh hơn?</h3>
<p>Vẫn quan trọng, nhưng đã thay đổi. Kỹ năng ""viết câu hỏi hay"" ít quan trọng hơn. Kỹ năng ""quản lý context tốt"" ngày càng quan trọng hơn. Biết khi nào cần provide thêm context, khi nào cần isolate task, khi nào cần compress history — đây là những gì phân biệt developer dùng AI hiệu quả với người dùng qua loa. CLAUDE.md và system prompts tốt là đầu tư một lần, lợi nhiều lần.</p>

<h3>AI có thay thế developer không?</h3>
<p>Entry-level positions đang giảm (28% từ đỉnh 2022), nhưng tổng số việc làm developer vẫn được dự báo tăng 15% theo BLS. Thực tế hơn là: AI đang thay đổi <em>cơ cấu</em> việc làm, không xóa toàn bộ ngành. Những developer biết dùng AI như một force multiplier — không phải như magic wand — sẽ có năng suất cao hơn và khó bị thay thế hơn. Những người coi AI là ""tự động hóa hoàn toàn"" và không kiểm soát output sẽ gặp rủi ro lớn hơn về chất lượng và security.</p>

<h3>Tôi nên bắt đầu học AI integration từ đâu với stack .NET?</h3>
<p>Thứ tự thực tế: (1) Thành thạo một AI coding tool — Claude Code hoặc Copilot. (2) Học cách viết CLAUDE.md/system prompts tốt. (3) Tìm hiểu Microsoft.Extensions.AI và Semantic Kernel cho .NET — đây là official abstractions của Microsoft để integrate LLMs vào .NET apps. (4) Thử EF Core 10 vector search nếu bạn cần RAG. (5) Đọc về prompt injection và AI security risks trước khi expose AI features ra production.</p>

<h3>Tôi có thể tin AI review security code không?</h3>
<p>Câu trả lời ngắn: không hoàn toàn. 29–45% AI-generated code chứa security vulnerabilities, và AI thường không phát hiện ra lỗi của chính nó khi review. Dùng AI như một lớp đầu tiên — nó có thể bắt được những lỗi obvious — nhưng với code liên quan đến auth, payment, data access, hoặc bất kỳ sensitive operation nào, human review là bắt buộc. Tools như Snyk, SonarQube, hoặc GitHub Advanced Security nên là lớp review thứ hai độc lập với AI.</p>

<h2>Kết luận</h2>

<p>Năm 2026, AI không còn là optional extra trong toolkit của developer — nó là baseline expectation. Nhưng ""biết dùng AI"" không đồng nghĩa với ""tin AI mù quáng"". Kỹ năng quan trọng nhất không phải là chọn được tool tốt nhất, mà là hiểu được giới hạn của từng tool và biết khi nào cần can thiệp của người.</p>

<p>Mình xây dựng VodonghaPersonal.id.vn với Claude Code và có thể khẳng định: AI tăng tốc đáng kể ở những việc có pattern rõ ràng (Blazor components, EF Core migrations, CI/CD config). Nhưng architecture decisions, security review, và business logic phức tạp vẫn cần tư duy của con người. Đó là sự cân bằng mà mình nghĩ mọi developer cần tìm ra cho chính mình.</p>

<p>Bài viết liên quan bạn có thể quan tâm:</p>
<ul>
  <li><a href=""/blog/building-with-ai-experience-with-claude-code"">Kinh nghiệm build dự án thực tế với Claude Code</a></li>
  <li><a href=""/blog/vibe-coding-la-gi"">Vibe coding là gì và tại sao nó đang gây tranh cãi</a></li>
  <li><a href=""/blog/deploy-dotnet-fly-io-2025"">Deploy .NET app lên Fly.io: hướng dẫn thực tế</a></li>
  <li><a href=""/blog/blazor-vs-react-2025"">Blazor vs React năm 2025: chọn gì cho dự án mới?</a></li>
  <li><a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL + EF Core: best practices từ thực tế</a></li>
</ul>", @"<p><img src=""https://images.unsplash.com/photo-1655720828018-edd2daec9349?w=1200&auto=format&fit=crop&q=80"" alt=""AI Skills for Developers in 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>When I look back at the codebase for <strong>VodonghaPersonal.id.vn</strong> — a Blazor .NET 10 portfolio site running on Fly.io with Neon PostgreSQL — I realize something interesting: I built almost the entire thing with Claude Code. Not ""AI helped here and there,"" but genuinely used an AI agent to write code, refactor, debug, and configure deployment end-to-end.</p>

<p>According to a 2026 survey of 15,000 developers, <strong>73% of engineering teams now use AI coding tools daily</strong> — up from 41% in 2025 and 18% in 2024. This is no longer a trend. It is the current reality. The question is no longer whether to use AI, but how to use it well.</p>

<p>Here are the five things I believe every developer genuinely needs to understand about AI in 2026, based on direct experience and real data from the community.</p>

<h2>1. The AI Coding Tool Landscape — Who Is Actually Winning?</h2>

<p>The competitive picture has clarified considerably by 2026. Here are the tools that matter:</p>

<h3>Claude Code — the unexpected market leader</h3>
<p>Launched in May 2025 as a terminal-based agentic coding tool, Claude Code did something no one expected: within eight months it overtook both GitHub Copilot (on the market since 2021) and Cursor in developer satisfaction. As of February 2026, Claude Code holds <strong>41% market share</strong> versus Copilot's 38% among professional developers. 71% of developers who use AI agents name Claude Code as their primary tool.</p>
<p>The single biggest differentiator: a <strong>95% first-try correctness rate</strong> — highest among all tested agents. That is why I chose Claude Code to build VodonghaPersonal.id.vn — from Blazor components and EF Core migrations to the full GitHub Actions CI/CD pipeline.</p>
<p>The most important file when working with Claude Code is <code>CLAUDE.md</code>. This is where you declare project instructions, coding standards, and architecture decisions. The agent loads this file at every session start. A well-written CLAUDE.md is the difference between an AI that understands your codebase context and one that generates technically correct but contextually wrong code.</p>

<h3>GitHub Copilot — still dominant through zero switching cost</h3>
<p>$10/month, integrated directly into VS Code, JetBrains, and Visual Studio. The default model is GPT-4o, but you can now switch to Claude Sonnet 4.6 or Gemini 2.5 Pro. SWE-bench score: 56.0% (March 2026). As of June 1, 2026, Copilot has moved to usage-based billing. Its biggest advantage remains frictionless integration — no workflow changes required.</p>

<h3>Cursor — the senior developer's daily driver</h3>
<p>$20/month, an AI-native VS Code fork. SWE-bench: 51.7%; inline suggestions are 15–25ms faster than Copilot. Model flexibility: GPT-5.4, Claude Opus 4.6, Gemini 3 Pro, Grok Code. The senior developer consensus stack for 2026: <strong>Cursor for daily work + Claude Code for complex tasks</strong> at roughly $40/month total.</p>

<h3>Gemini Code Assist — most generous free tier</h3>
<p>6,000 completions per day (180,000 per month) — 90x more than Copilot's free tier. If you are starting out and not yet ready to pay, this is the obvious starting point.</p>

<h2>2. The Productivity Reality — Not What You Would Expect</h2>

<p>This is the part most AI coverage glosses over, and it matters.</p>

<p>84–93% of developers use AI tools and self-report saving approximately four hours per week. That sounds compelling. But the <strong>METR real-world measurement study</strong> — which actually measured task completion time rather than relying on self-reporting — found the opposite: developers took <strong>19% longer</strong> on tasks when using AI versus not using it, while still believing they were 20% faster. A textbook perception gap.</p>

<p>Why does this happen? A few practical reasons:</p>
<ul>
  <li>The time spent reading and verifying AI-generated code does not register as ""work"" in the developer's mental accounting</li>
  <li>AI generates boilerplate quickly but slows down on complex logic that requires deep context</li>
  <li>41% of all code written in 2025 was AI-generated; 26.9% of current production code is AI-authored — but quality is highly uneven</li>
</ul>

<p>None of this means AI is useless. Productivity gains plateau at a real but modest ~10% when used correctly. The problem is unrealistic expectations. Think of AI as a very fast-typing junior developer who needs careful review — not a senior you can confidently hand off entire features to.</p>

<h2>3. Prompt Engineering in 2026 — Evolved, Not Dead</h2>

<p>The standalone ""Prompt Engineer"" job title has largely disappeared (Fast Company, May 2025). 68% of companies now treat it as standard training for all roles rather than a specialized function. But that does not mean prompting matters less — it means it has become more sophisticated and more deeply embedded in everyday developer work.</p>

<h3>The practical 4-signal framework</h3>
<p>The most consistently effective prompt structure: <strong>Role + Context + Task + Format</strong>.</p>

<pre><code>You are a senior .NET developer reviewing a Blazor WASM component.
Context: This component renders a paginated list of bank transactions using
EF Core 10 with SQL Server. Performance is critical — up to 10,000 rows.
Task: Review the following code for N+1 query issues and suggest fixes.
Format: List issues as bullet points, then provide a corrected code snippet.</code></pre>

<p>On length: LLM reasoning degrades noticeably around 3,000 tokens. The practical sweet spot is <strong>150–300 words</strong> for a single prompt. Do not paste a 500-line file and ask ""any issues?"" — you will get shallow, generic feedback.</p>

<h3>Context engineering — the skill that actually differentiates</h3>
<p>The phrase ""prompt engineering"" is gradually giving way to ""context engineering."" LangChain identifies four core context strategies:</p>
<ul>
  <li><strong>Write:</strong> Persist information to memory or files for reuse across sessions (CLAUDE.md is the canonical example)</li>
  <li><strong>Select (RAG):</strong> Pull only the relevant context rather than dumping everything</li>
  <li><strong>Compress:</strong> Summarize older context to keep the active window clean</li>
  <li><strong>Isolate:</strong> Break complex tasks into sub-tasks with their own isolated context</li>
</ul>

<p>When I built the SignalR live chat feature for VodonghaPersonal.id.vn, rather than feeding Claude Code the entire codebase, I used CLAUDE.md to declare the architecture and provided only the context for the relevant components. The output quality was substantially better than when I had tried the ""give it everything"" approach earlier in the project.</p>

<h3>Treat prompts as system design artifacts</h3>
<p>The most useful mental model: treat a prompt like an interface definition — specify input, expected output, constraints, failure modes, and evaluation criteria. Write it like an API spec, not a chat message.</p>

<h2>4. AI Code Review — Knowing When Not to Trust It</h2>

<p>This is the most important section of this post, and it is the one that receives the least coverage in AI hype cycles.</p>

<p>Real numbers from 2026 research:</p>
<ul>
  <li>76% of developers report frequent AI hallucinations</li>
  <li>29–45% of AI-generated code contains security vulnerabilities</li>
  <li>~20% of AI package recommendations point to libraries that do not exist</li>
  <li>59% of developers ship AI-generated code they do not fully understand (Clutch.co)</li>
  <li>65% say missing context is the biggest problem — ranking above hallucinations themselves</li>
</ul>

<h3>The ""AI reviewing AI"" problem</h3>
<p>An increasingly common anti-pattern: using AI to review code that AI wrote. The problem is that models trained on similar data converge on similar wrong answers. <strong>Consensus is not the same as correctness.</strong> By 2026, AI-generated code volume is projected to outstrip human review capacity by 40% — what researchers are calling the ""AI code generation gap.""</p>

<h3>Security is a real risk, not a theoretical one</h3>
<p>In .NET codebases specifically, the errors AI tends to make repeatedly include:</p>
<ul>
  <li>SQL injection via string interpolation (even with EF Core, AI sometimes suggests raw SQL that is not properly parameterized)</li>
  <li>Missing authorization checks — generating controller actions without <code>[Authorize]</code></li>
  <li>Insecure deserialization when handling JSON from external sources</li>
  <li>Logging secrets — AI frequently logs entire request objects including sensitive fields</li>
</ul>

<pre><code>// AI-generated code — DANGEROUS, do not use
public async Task&lt;IActionResult&gt; GetUser(string id)
{
    // Missing authorization check, missing tenant isolation
    var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Id == id);
    return Ok(user);
}

// Corrected version
[Authorize]
public async Task&lt;IActionResult&gt; GetUser(string id)
{
    var currentTenant = User.GetTenantId();
    var user = await _db.Users
        .Where(u => u.TenantId == currentTenant && u.Id == id)
        .FirstOrDefaultAsync();
    return user == null ? NotFound() : Ok(user);
}</code></pre>

<p>My working rule: any code AI writes that touches authentication, authorization, or data access gets reviewed by a human — not by another AI.</p>

<h2>5. Career Impact — More Nuanced Than the Headlines Suggest</h2>

<p>Entry-level job postings have declined 28% from 2022 peaks. 65% of developers expect their role to be redefined in 2026 — from ""write code"" to ""architect systems, integrate AI, make AI-enabled decisions."" This is real, not FUD.</p>

<p>But the US Bureau of Labor Statistics still projects <strong>15% software developer job growth</strong> — well above the average for all occupations. AI is not replacing developers. AI is replacing developers who do not know how to use AI.</p>

<h3>Skills gaining value right now</h3>
<ul>
  <li><strong>System design:</strong> When AI can write code, the ability to design the right system becomes more valuable, not less</li>
  <li><strong>Security:</strong> AI generates more code volume but does not automatically generate more secure code</li>
  <li><strong>AI integration:</strong> Knowing how to integrate LLMs, RAG pipelines, and vector search into real products</li>
  <li><strong>Data engineering:</strong> Understanding data pipelines that feed AI systems</li>
</ul>

<h3>The junior developer pathway is the most disrupted</h3>
<p>The decline in entry-level positions does not mean there are no opportunities. Junior developers in 2026 are expected to ramp up faster because AI handles the boilerplate that used to be their primary training ground. Juniors who use AI to accelerate learning — not to shortcut understanding — will have a real competitive advantage. Those who ship AI code they cannot explain will become a liability rather than an asset.</p>

<h2>6. My Actual Stack — .NET 10 + AI Workflow in Practice</h2>

<p>.NET 10 shipped in November 2025 (LTS, supported through November 2028) with meaningful runtime performance improvements: reduced allocations, better method inlining, improved devirtualization, AVX 10.2 support, and Blazor WASM preloading. EF Core 10 adds native <code>LeftJoin</code>/<code>RightJoin</code> operators, native JSON columns, and <strong>vector search that is no longer experimental</strong> (using <code>VECTOR_DISTANCE()</code> on SQL Server 2025 and Azure SQL).</p>

<p>My Claude Code workflow for VodonghaPersonal.id.vn:</p>
<ol>
  <li>Write a detailed <code>CLAUDE.md</code>: tech stack, coding conventions, database schema, deployment target, and any constraints</li>
  <li>Describe tasks using Role + Context + Task + Format structure</li>
  <li>Review every output — especially auth, data access, and external dependencies</li>
  <li>Run the full test suite before merging (GitHub Actions pipeline handles this automatically)</li>
  <li>For security-critical code: mandatory human review, no exceptions</li>
</ol>

<p>Actual cost: approximately $20/month for Claude Code, $0 for Fly.io (free tier for development), $0 for Neon (free tier with suspend mode). Substantially cheaper than hiring out individual features, and faster for a solo developer maintaining their own portfolio.</p>

<h2>7. Common Mistakes to Avoid</h2>

<ul>
  <li><strong>Shipping code you cannot explain:</strong> 59% of developers do this. If you cannot walk through what the code does line by line, do not deploy it.</li>
  <li><strong>Prompting without context:</strong> ""Write me a user authentication system"" is not a useful prompt. Specify: .NET 9, JWT, multi-tenant architecture, SQL Server, existing patterns in the codebase.</li>
  <li><strong>Skipping CLAUDE.md or system prompts:</strong> Not investing in an onboarding file wastes most of the agent's potential. This is a one-time investment that pays back on every session.</li>
  <li><strong>Using AI to review AI-generated code:</strong> This is circular validation. It is an anti-pattern that feels productive but provides false confidence.</li>
  <li><strong>Expecting AI to handle everything equally well:</strong> AI is strong at patterns it has seen before — boilerplate, standard library usage, common refactors. It is weak at novel architecture decisions and complex domain-specific business logic. Know the difference.</li>
</ul>

<h2>8. FAQ</h2>

<h3>Should I use Claude Code or GitHub Copilot?</h3>
<p>If you are already in VS Code or JetBrains and want AI assistance without changing your workflow, Copilot at $10/month has the lowest friction. If you need to tackle more complex tasks — refactoring entire modules, generating tests, debugging multi-file issues, or running agentic workflows — Claude Code's 95% first-try correctness rate and deeper context handling make it the better fit. Many senior developers in 2026 use both: Cursor or Copilot for everyday inline assistance, Claude Code for complex autonomous tasks. The combined cost of roughly $40/month is well within the budget of a professional developer given the productivity impact.</p>

<h3>Is prompt engineering still worth learning when models keep improving?</h3>
<p>Yes, but the skills that matter have shifted. The ability to ""ask a clever question"" matters less than before. The ability to manage context effectively — knowing when to provide more, when to isolate a sub-task, when to compress history — matters more than ever. Writing good CLAUDE.md files and system prompts is a one-time investment that returns value on every subsequent session. Think of it as infrastructure, not a trick.</p>

<h3>Will AI replace software developers?</h3>
<p>Entry-level positions are declining (down 28% from 2022 peaks), but overall developer employment is still projected to grow 15% according to the Bureau of Labor Statistics. The more accurate framing is that AI is restructuring the composition of developer work rather than eliminating the field. Developers who treat AI as a force multiplier — increasing their throughput while maintaining responsibility for output quality — will be more productive and harder to replace. Developers who treat AI as a black box that can be trusted without oversight will accumulate technical debt and security risk that eventually catches up with them.</p>

<h3>Where should I start with AI integration in a .NET stack?</h3>
<p>A practical learning sequence: (1) Get proficient with one AI coding tool — Claude Code or Copilot. (2) Learn to write effective CLAUDE.md and system prompt files. (3) Explore Microsoft.Extensions.AI and Semantic Kernel — Microsoft's official abstractions for integrating LLMs into .NET applications. (4) Try EF Core 10 vector search if you need RAG capabilities in a data-heavy application. (5) Read about prompt injection and AI security risks before exposing any AI-facing features to production users.</p>

<h3>Can I trust AI to review security-sensitive code?</h3>
<p>Not fully, and definitely not as the sole review layer. Between 29–45% of AI-generated code contains security vulnerabilities, and AI typically fails to catch its own mistakes when asked to review them — models trained on similar data tend to make and miss the same errors. Use AI as a first pass for obvious issues, but for any code touching authentication, payments, multi-tenant data access, or sensitive operations, a human code review is non-negotiable. Pair it with independent static analysis tools like Snyk, SonarQube, or GitHub Advanced Security. Defense in depth applies to AI-assisted development just as it applies to everything else in security.</p>

<h2>Conclusion</h2>

<p>In 2026, AI is not an optional extra in a developer's toolkit — it is a baseline expectation. But ""knowing how to use AI"" is not the same as ""trusting AI unconditionally."" The most important skill is not choosing the best tool; it is understanding the limits of whichever tool you choose, and knowing precisely when human judgment must override the machine.</p>

<p>Building VodonghaPersonal.id.vn with Claude Code confirmed this for me in practice. AI accelerated the work significantly on tasks with clear patterns: Blazor components, EF Core schema migrations, CI/CD configuration, test scaffolding. Architecture decisions, security reviews, and complex business logic still required sustained human thinking. That balance — leveraging AI's speed while maintaining genuine ownership of the output — is the discipline I think every developer needs to develop intentionally rather than accidentally.</p>

<p>Related posts you may find useful:</p>
<ul>
  <li><a href=""/blog/building-with-ai-experience-with-claude-code"">Building a real project with Claude Code — my experience</a></li>
  <li><a href=""/blog/vibe-coding-la-gi"">What is vibe coding and why is it controversial?</a></li>
  <li><a href=""/blog/deploy-dotnet-fly-io-2025"">Deploying a .NET app to Fly.io — a practical guide</a></li>
  <li><a href=""/blog/blazor-vs-react-2025"">Blazor vs React in 2025 — what to choose for a new project?</a></li>
  <li><a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL + EF Core best practices from production experience</a></li>
</ul>", @"73% team kỹ thuật dùng AI coding tools hàng ngày năm 2026. Là developer .NET, mình chia sẻ 5 kỹ năng AI thực tế bạn cần nắm — từ chọn tool đến prompt engineering và review code an toàn.", @"73% of engineering teams use AI coding tools daily in 2026. As a .NET developer who built his portfolio entirely with Claude Code, here are 5 AI skills you actually need — tools, prompting, code review, and career impact.", @"AI, Developer Skills, Claude Code, Prompt Engineering, GitHub Copilot, 2026, Productivity, Career", @"Kỹ năng AI cho Developer năm 2026: 5 điều bạn cần biết", @"AI Skills for Developers in 2026: 5 Things You Need to Know", new DateTime(2026, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Content", "ContentEn", "Summary", "SummaryEn", "Tags", "Title", "TitleEn", "UpdatedAt" },
                values: new object[] { @"<p><img src=""https://images.unsplash.com/photo-1620712943543-bcc4688e7485?w=1200&amp;auto=format&amp;fit=crop&amp;q=80"" alt=""Vibe Coding - Lập trình với AI năm 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Ngày 2 tháng 2 năm 2025, Andrej Karpathy — đồng sáng lập OpenAI, cựu giám đốc AI của Tesla — đăng một tweet ngắn làm cả cộng đồng lập trình tranh luận suốt cả năm. Ông gọi nó là <strong>vibe coding</strong>: ""fully give in to the vibes, embrace exponentials, and forget that the code even exists"" — tạm dịch: hoàn toàn buông theo cảm giác, đón nhận sự tăng trưởng theo hàm mũ, và quên đi rằng code thậm chí tồn tại.</p>

<p>Chưa đầy một năm sau, Collins English Dictionary đặt nó là <strong>từ của năm 2025</strong>. Merriam-Webster xếp nó vào mục ""slang &amp; trending"" từ tháng 3/2025. Wall Street Journal đưa tin về các kỹ sư chuyên nghiệp dùng nó trong môi trường thương mại từ tháng 7/2025.</p>

<p>Tôi xây dựng <a href=""https://VodonghaPersonal.id.vn"">VodonghaPersonal.id.vn</a> — trang portfolio cá nhân này — hoàn toàn bằng Claude Code. Không phải vì tôi không thể viết code, mà vì tôi <em>chọn</em> không viết từng dòng một. Đây là những gì tôi học được sau khoảng thời gian đó.</p>

<h2>Vibe Coding là gì — định nghĩa thực sự</h2>

<p>Karpathy mô tả vibe coding là cách lập trình mà bạn diễn đạt yêu cầu bằng ngôn ngữ tự nhiên, chấp nhận output từ AI, và lặp lại qua các prompt thay vì đọc hay viết code từng dòng. Ông đang mở rộng tuyên bố năm 2023 của mình: ""ngôn ngữ lập trình hot nhất hiện nay là tiếng Anh.""</p>

<p>Điều quan trọng cần hiểu: vibe coding <strong>không phải</strong> là non-technical người dùng nhấn nút tạo app. Đây là bằng chứng sắc bén nhất: Y Combinator tiết lộ rằng 25% startup trong batch Winter 2025 có codebase được AI tạo ra 95% trở lên — và <em>tất cả</em> các founder đó đều có khả năng kỹ thuật để tự viết. Họ chọn AI vì nó nhanh hơn.</p>

<p>Karpathy gọi đây là một sự thay đổi nhận thức thực sự: <strong>tin vào kết quả thay vì đọc code</strong>. Đó là điều mới mẻ thực sự.</p>

<h2>Dữ liệu thực tế: Mức độ phổ biến năm 2026</h2>

<p>Con số không nói dối. Theo các khảo sát lớn nhất trong ngành:</p>

<ul>
  <li><strong>90%</strong> developer đang dùng AI tools trong công việc (DORA 2025)</li>
  <li><strong>85%</strong> dùng thường xuyên (JetBrains tháng 10/2025)</li>
  <li><strong>84%</strong> đang dùng hoặc có kế hoạch dùng AI tools (Stack Overflow 2025)</li>
  <li><strong>80%</strong> developer mới trên GitHub dùng Copilot ngay trong tuần đầu (GitHub Octoverse 2025)</li>
  <li><strong>90%</strong> công ty Fortune 100 dùng GitHub Copilot</li>
  <li><strong>67%</strong> Fortune 500 dùng Cursor</li>
</ul>

<p>Riêng về Claude Code — tool tôi dùng để xây dựng site này — con số tăng trưởng đặc biệt ấn tượng: từ <strong>3% lên 18% chỉ trong 9 tháng</strong> (tháng 4/2025 đến tháng 1/2026). Điều đáng chú ý hơn: <strong>46% developer senior (10+ năm kinh nghiệm)</strong> hiện ưa thích Claude Code hơn các tool khác. Đây không phải junior đang tìm cách rút ngắn học tập — đây là những người giỏi nhất trong ngành chọn tool hiệu quả nhất.</p>

<p>Nhưng có một tín hiệu đáng lo ngại: <strong>mức tin tưởng đang giảm</strong>. Sentiment tích cực với AI tools giảm từ hơn 70% (2023-2024) xuống còn 60% (2025). Tin tưởng vào độ chính xác của AI output giảm từ 40% xuống 29%. 46% developer chủ động <em>không tin</em> output của AI (tăng từ 31%).</p>

<h2>Năng suất: Sự thật phức tạp</h2>

<p>Đây là phần mà nhiều bài viết chỉ nói một chiều. Tôi sẽ nói thẳng cả hai mặt.</p>

<p><strong>Lợi ích năng suất có thật:</strong></p>
<ul>
  <li>Tiết kiệm trung bình <strong>3.6 giờ/tuần</strong> mỗi developer (DX Q4/2025)</li>
  <li>Developer dùng AI hàng ngày tạo ra <strong>60% nhiều PR hơn</strong> (2.3 vs 1.4 PRs/tuần)</li>
  <li>Thời gian review PR giảm từ 9.6 ngày xuống <strong>2.4 ngày</strong> (giảm 75% — Accenture RCT)</li>
  <li>Developer mới đạt 10 PR đầu tiên trong <strong>49 ngày</strong> thay vì 91 ngày</li>
</ul>

<p><strong>Nghịch lý METR (tháng 7/2025):</strong> Một thử nghiệm ngẫu nhiên có kiểm soát trên 16 developer open-source giàu kinh nghiệm — những người làm việc với repo có trung bình 22,000 stars và 1 triệu dòng code — cho thấy họ <strong>chậm hơn 19%</strong> khi dùng AI tools, trong khi <em>tự nhận</em> mình nhanh hơn 20%. Khoảng cách nhận thức là 39 điểm phần trăm.</p>

<p>Lưu ý quan trọng: METR một phần rút lại nghiên cứu vào tháng 2/2026, thừa nhận có vấn đề về phương pháp. Nhưng điều này không phủ nhận sự thật cốt lõi: <strong>lợi ích năng suất phân bổ không đều</strong>. Chúng rõ ràng nhất với code mới (greenfield), và sụp đổ với việc bảo trì codebase phức tạp có nhiều legacy.</p>

<p>Khi tôi xây dựng VodonghaPersonal.id.vn — greenfield, một developer, stack được document đầy đủ — đây chính xác là điều kiện tốt nhất cho vibe coding. Không có legacy constraints, không có institutional memory cần bảo vệ, không có reviewer khác cần thuyết phục.</p>

<h2>Bảo mật: Khoảng cách không thể bỏ qua</h2>

<p>Đây là phần tôi muốn nói thẳng nhất, vì các con số thực sự đáng lo ngại.</p>

<ul>
  <li>PR có AI co-author chứa <strong>1.7x nhiều major issues</strong> hơn (CodeRabbit, tháng 12/2025, 470 PRs)</li>
  <li>Lỗ hổng bảo mật: <strong>cao hơn 2.74 lần</strong> trong code AI co-author</li>
  <li>40–62% code AI-generated chứa security flaws (Veracode)</li>
  <li>Tỷ lệ thất bại bảo mật Java: <strong>72%</strong></li>
  <li>Tỷ lệ thất bại XSS protection: <strong>86%</strong></li>
  <li>Developer dùng AI bị lộ credentials: <strong>gấp 2 lần</strong> (Apiiro)</li>
  <li>Security findings tăng <strong>10 lần</strong> trong 6 tháng (Apiiro, tháng 12/2024 – 6/2025)</li>
</ul>

<p>Và đây là ba sự cố thực tế không thể bỏ qua:</p>

<p><strong>Moltbook (28/1/2026):</strong> Founder tự hào ""không viết một dòng code nào"" — mạng xã hội AI-generated. Toàn bộ production database bị lộ trong 3 ngày, bao gồm 1.5 triệu API auth token.</p>

<p><strong>Lovable (tháng 5/2025):</strong> Platform tạo code AI sinh ra code dễ bị tấn công trong 170 trong số 1,645 ứng dụng.</p>

<p><strong>Replit AI agent (tháng 7/2025):</strong> Xóa production database dù được hướng dẫn rõ ràng không được chỉnh sửa hệ thống.</p>

<p>Với VodonghaPersonal.id.vn, tôi xử lý vấn đề này bằng cách dùng pattern IDbContextFactory thay vì long-lived DbContext (AI thường sinh đúng pattern này nếu bạn mô tả yêu cầu chính xác), review mọi migration script thủ công trước khi chạy, và không bao giờ commit secrets — mọi thứ đều qua environment variables.</p>

<h2>Stack của VodonghaPersonal.id.vn và tại sao nó phù hợp với vibe coding</h2>

<p>Không phải stack nào cũng ""AI-friendly"" như nhau. Đây là lý do tôi chọn stack này và nó hoạt động tốt với Claude Code:</p>

<p><strong>Blazor InteractiveServer (.NET 10):</strong> Không cần API layer riêng — một mental model duy nhất. Claude Code hiểu rõ pattern này và sinh code đúng. .NET 10 còn có <code>[PersistentState]</code> attribute cho declarative state persistence — đây là loại ceremony mà AI có thể xử lý tốt hơn viết tay:</p>

<pre><code>// .NET 10 - PersistentState tự động persist qua page refresh
[PersistentState(nameof(counter))]
private PersistentState&lt;int&gt; counter;
</code></pre>

<p><strong>EF Core với IDbContextFactory:</strong> Pattern này ngăn AI tạo long-lived DbContext — một lỗi phổ biến. Khi bạn mô tả yêu cầu đúng cách, Claude Code sinh pattern đúng:</p>

<pre><code>// IDbContextFactory pattern - AI-friendly, thread-safe
public class BlogService(IDbContextFactory&lt;AppDbContext&gt; factory)
{
    public async Task&lt;List&lt;Post&gt;&gt; GetPostsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Posts
            .OrderByDescending(p =&gt; p.PublishedAt)
            .ToListAsync();
    }
}
</code></pre>

<p><strong>Neon (serverless PostgreSQL):</strong> Suspend mode nghĩa là free tier khả thi cho personal site với traffic không đều. Neon đã announce PostgreSQL 18 features bao gồm UUIDv7 (time-ordered, rất phù hợp cho event tables), Virtual Generated Columns (không cần migration cho computed values), và database branching — database-per-PR tích hợp tự nhiên với AI-driven workflow.</p>

<p><strong>Fly.io:</strong> CLI-first, được represent tốt trong training data của AI. Scale-to-zero hoạt động tốt với Blazor SignalR — khi connection đã được thiết lập, không có cold-start penalty. Shared-cpu-1x/256MB là floor khả thi cho một Blazor app.</p>

<p>Đọc thêm về kinh nghiệm deploy tại: <a href=""/blog/deploy-dotnet-fly-io-2025"">Deploy .NET lên Fly.io 2025</a>.</p>

<h2>Vai trò của developer năm 2026: Không phải kết thúc, mà là tái định nghĩa</h2>

<p>65% developer kỳ vọng vai trò của họ sẽ được tái định nghĩa theo hướng architecture, orchestration và oversight. Entry-level job postings giảm ~40% từ đỉnh 2022. Kỹ năng AI xuất hiện trong 42% job descriptions phần mềm (tăng từ 8% năm 2022). Developer thành thạo AI tools tìm được việc <strong>nhanh hơn 2.3 lần</strong>.</p>

<p>Nhưng CNN báo cáo tháng 4/2026: ""The demise of software engineering jobs has been greatly exaggerated"" — tổng số job listings vẫn tăng 11% hàng năm. Chỉ là cấu trúc thay đổi.</p>

<p>Framing trung thực nhất: vibe coding <strong>hạ thấp sàn</strong> (ai cũng có thể ship thứ gì đó) nhưng <strong>nâng cao kỳ vọng trần</strong> (nếu bạn có thể ship với AI, bạn được kỳ vọng ship nhiều hơn). Developer xây dựng portfolio site với Claude Code trong một cuối tuần đang thể hiện AI fluency, không phải gian lận.</p>

<p>Bottleneck mới không phải ""bạn có thể viết code không"" mà là <strong>""bạn có thể verify, review, và architect những gì AI tạo ra không.""</strong></p>

<p>Xem thêm: <a href=""/blog/ai-skills-for-developers"">AI Skills cho Developer năm 2026</a> và <a href=""/blog/building-with-ai-experience-with-claude-code"">Kinh nghiệm xây dựng với Claude Code</a>.</p>

<h2>Khi nào nên dùng, khi nào nên tránh</h2>

<p><strong>Vibe coding hiệu quả nhất khi:</strong></p>
<ul>
  <li>Greenfield project, không có legacy constraints</li>
  <li>Stack được document tốt và AI đã ""biết"" (Blazor, EF Core, React, Next.js, PostgreSQL)</li>
  <li>Một developer hoặc team nhỏ với architectural clarity</li>
  <li>Prototype nhanh, MVP, personal projects</li>
  <li>Boilerplate nặng: CRUD, migrations, component scaffolding</li>
</ul>

<p><strong>Vibe coding rủi ro cao khi:</strong></p>
<ul>
  <li>Codebase phức tạp, lâu năm với ít documentation</li>
  <li>Payment processing, authentication, cryptography</li>
  <li>Bất cứ thứ gì xử lý dữ liệu nhạy cảm mà không có security review</li>
  <li>Không có người review output của AI</li>
  <li>Cần bảo trì dài hạn bởi nhiều người</li>
</ul>

<h2>FAQ</h2>

<h3>Vibe coding có phải là lập trình ""thật"" không?</h3>
<p>Câu hỏi này đặt sai vấn đề. Developer thành thạo vibe coding vẫn phải đưa ra mọi quyết định kiến trúc quan trọng: chọn stack, thiết kế data model, định nghĩa render mode, cấu hình CI/CD, xác định security boundaries. AI là code-generation layer, không phải architect. Tương tự, developer dùng IDE với autocomplete vẫn đang ""lập trình thật"" — họ chỉ dùng tool tốt hơn.</p>

<h3>Claude Code khác gì GitHub Copilot?</h3>
<p>Copilot chủ yếu là inline autocomplete — nó gợi ý code khi bạn gõ. Claude Code là agent-mode — nó có thể đọc files trong project, chạy build, diễn giải compiler errors, và iterate nhiều bước mà không cần bạn can thiệp. Đây là sự khác biệt định tính. Khi tôi xây VodonghaPersonal.id.vn, Claude Code không chỉ gợi ý từng dòng — nó đọc toàn bộ context của project và đưa ra quyết định coherent. Đó là lý do 46% senior developer thích nó.</p>

<h3>Vibe coding có an toàn để dùng trong production không?</h3>
<p>Với điều kiện đúng: có. Điều kiện đó bao gồm: bạn review mọi thứ AI tạo ra trước khi deploy, có security review riêng (đặc biệt với authentication và data exposure), và không bao giờ blind-commit output của AI vào production. Sự cố Moltbook không phải do AI tạo code — mà do không có review process. 2.74x nhiều vulnerabilities là con số thật, nhưng nó đo lường code không được review. Developer có kinh nghiệm với review process tốt có thể giảm rủi ro đó xuống đáng kể.</p>

<h3>Tôi có nên lo lắng về việc mất việc làm không?</h3>
<p>Lo lắng ít thôi, nhưng thích ứng ngay. Entry-level jobs giảm 40%, nhưng tổng listings vẫn tăng 11%. Cấu trúc thay đổi: người thắng là developer có thể làm việc hiệu quả với AI — verify output, review security, architect hệ thống. Người thua là developer cố chống lại tool. 42% job descriptions hiện đề cập AI skills; developer thành thạo tìm việc nhanh hơn 2.3 lần. Bạn đang đọc bài này — bạn đã đi đúng hướng.</p>

<h3>Stack .NET 10 có phù hợp để học vibe coding không?</h3>
<p>Rất phù hợp. Blazor, EF Core, ASP.NET Core đều được represent tốt trong training data của các AI models lớn. .NET 10 mang lại các pattern mới như <code>[PersistentState]</code> và circuit resilience mà AI có thể sinh đúng nếu bạn mô tả yêu cầu rõ ràng. Khó khăn nhất không phải là AI không biết .NET — mà là bạn cần đủ kiến thức để nhận ra khi AI sinh pattern sai (ví dụ: long-lived DbContext thay vì IDbContextFactory). Xem thêm: <a href=""/blog/blazor-vs-react-2025"">Blazor vs React 2025</a> và <a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL &amp; EF Core Best Practices</a>.</p>

<h2>Kết luận</h2>

<p>Vibe coding năm 2026 không phải là hype đơn thuần, cũng không phải giải pháp hoàn hảo. Đó là một sự thay đổi thực sự trong cách developer tiếp cận code — với lợi ích rõ ràng (3.6 giờ/tuần tiết kiệm, 60% nhiều PR hơn, onboarding nhanh hơn gấp đôi) và rủi ro rõ ràng không kém (2.74x vulnerabilities, 86% XSS failure rate, Moltbook exposing 1.5M tokens).</p>

<p>Khi tôi xây VodonghaPersonal.id.vn với Claude Code, tôi không ""quên code tồn tại"" theo nghĩa literal của Karpathy. Tôi vẫn review mọi thứ, vẫn hiểu mọi architectural decision, vẫn đọc mọi migration script. Nhưng tôi không còn là người gõ từng dòng boilerplate nữa. Tôi là architect sử dụng AI như một code-generation layer.</p>

<p>Đó chính là vai trò của developer năm 2026: không phải người gõ code nhanh nhất, mà là người biết <em>chỉ đạo, verify, và chịu trách nhiệm</em> về những gì AI tạo ra.</p>

<p>Bài tiếp theo: <a href=""/blog/building-with-ai-experience-with-claude-code"">Kinh nghiệm thực tế xây dựng với Claude Code — từ zero đến production</a>.</p>", @"<p><img src=""https://images.unsplash.com/photo-1620712943543-bcc4688e7485?w=1200&amp;auto=format&amp;fit=crop&amp;q=80"" alt=""Vibe Coding - AI-driven development in 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>On February 2, 2025, Andrej Karpathy — OpenAI co-founder, former Tesla AI director — posted a short tweet that sparked an industry-wide debate that lasted the entire year. He called it <strong>vibe coding</strong>: ""fully give in to the vibes, embrace exponentials, and forget that the code even exists"" — a practice where you describe what you want in natural language, accept the AI-generated output, and iterate through prompts rather than reading or writing code line by line.</p>

<p>Less than a year later, Collins English Dictionary named it <strong>Word of the Year 2025</strong>. Merriam-Webster listed it as ""slang &amp; trending"" in March 2025. The Wall Street Journal reported professional engineers using it commercially as early as July 2025.</p>

<p>I built <a href=""https://VodonghaPersonal.id.vn"">VodonghaPersonal.id.vn</a> — this portfolio site — entirely with Claude Code. Not because I cannot write code, but because I <em>chose</em> not to write every line myself. Here is what I learned.</p>

<h2>What Vibe Coding Actually Means</h2>

<p>Karpathy describes vibe coding as programming by describing intent in natural language, accepting AI output, and iterating through prompts rather than reading the underlying code. He was extending his 2023 claim that ""the hottest new programming language is English.""</p>

<p>The crucial distinction: vibe coding is <strong>not</strong> non-technical people clicking a button to generate an app. The sharpest evidence for this is the Y Combinator statistic: 25% of startups in YC Winter 2025 had codebases that were 95%+ AI-generated — and <em>every single founder</em> was technically capable of writing it themselves. They chose AI because it was faster.</p>

<p>Karpathy describes this as a genuine epistemic shift: <strong>trusting outcomes over reading code</strong>. That is what is actually new.</p>

<h2>The Numbers: How Widespread Is It in 2026?</h2>

<p>The data makes the scale of adoption impossible to dismiss:</p>

<ul>
  <li><strong>90%</strong> of developers use AI tools at work (DORA 2025)</li>
  <li><strong>85%</strong> use them regularly (JetBrains October 2025)</li>
  <li><strong>84%</strong> are using or planning to use AI tools (Stack Overflow 2025)</li>
  <li><strong>80%</strong> of new GitHub developers use Copilot in their first week (GitHub Octoverse 2025)</li>
  <li><strong>90%</strong> of Fortune 100 companies use GitHub Copilot</li>
  <li><strong>67%</strong> of Fortune 500 companies use Cursor</li>
</ul>

<p>Claude Code — the tool I used to build this site — shows particularly striking growth: from <strong>3% to 18% adoption in just nine months</strong> (April 2025 to January 2026). More telling: <strong>46% of senior developers (10+ years of experience)</strong> now prefer Claude Code over alternatives. These are not juniors trying to shortcut their learning — these are the most experienced people in the industry choosing the most effective tool.</p>

<p>But there is a concerning signal alongside the adoption numbers: <strong>trust is declining</strong>. Positive sentiment toward AI tools dropped from over 70% (2023–2024) to 60% (2025). Trust in AI output accuracy fell from 40% to 29%. 46% of developers actively distrust AI output — up from 31%.</p>

<h2>Productivity: The Complicated Truth</h2>

<p>Many articles about vibe coding tell only one side of the productivity story. Here is the honest picture.</p>

<p><strong>The gains are real:</strong></p>
<ul>
  <li>Average time saved: <strong>3.6 hours per developer per week</strong> (DX Q4 2025)</li>
  <li>Daily AI users produce <strong>60% more PRs</strong> (2.3 vs 1.4 per week)</li>
  <li>PR cycle time reduced from 9.6 days to <strong>2.4 days</strong> (75% reduction — Accenture RCT)</li>
  <li>New developer onboarding: first 10 merged PRs in <strong>49 days</strong> instead of 91</li>
</ul>

<p><strong>The METR paradox (July 2025):</strong> A randomized controlled trial on 16 experienced open-source developers — working on repositories averaging 22,000 stars and 1 million lines of code — found they were <strong>19% slower</strong> when using AI tools, while <em>believing</em> they were 20% faster. The perception gap was 39 percentage points.</p>

<p>Important caveat: METR partially retracted the study in February 2026, acknowledging methodological flaws. But this does not eliminate the core insight: <strong>productivity gains are unevenly distributed</strong>. They are clearest on new greenfield work and collapse for careful maintenance of complex, long-lived codebases.</p>

<p>When I built VodonghaPersonal.id.vn — greenfield, one developer, a well-documented stack — these were exactly the best-case conditions for vibe coding. No legacy constraints, no institutional memory to protect, no other reviewers to convince.</p>

<h2>Security: The Gap You Cannot Ignore</h2>

<p>This is where I want to be most direct, because the numbers are genuinely alarming.</p>

<ul>
  <li>AI co-authored PRs contain <strong>1.7x more major issues</strong> (CodeRabbit, December 2025, 470 open-source PRs)</li>
  <li>Security vulnerabilities: <strong>2.74x higher</strong> in AI co-authored code</li>
  <li>40–62% of AI-generated code contains security flaws (Veracode)</li>
  <li>Java security failure rate: <strong>72%</strong></li>
  <li>XSS protection failure rate: <strong>86%</strong></li>
  <li>Credential exposure: <strong>2x higher</strong> for AI-assisted developers (Apiiro)</li>
  <li>Security findings increased <strong>10x in 6 months</strong> (Apiiro, December 2024 – June 2025)</li>
  <li>March 2026: 35 new CVEs directly caused by AI-generated code</li>
</ul>

<p>Three real incidents make these statistics concrete:</p>

<p><strong>Moltbook (January 28, 2026):</strong> The founder proudly announced he ""didn't write a single line of code"" — an AI-generated social network. The entire production database was exposed within three days, including 1.5 million API auth tokens.</p>

<p><strong>Lovable (May 2025):</strong> The AI code generation platform produced vulnerable code in 170 of 1,645 applications.</p>

<p><strong>Replit AI agent (July 2025):</strong> Deleted a production database despite explicit instructions not to modify systems.</p>

<p>For VodonghaPersonal.id.vn, I addressed this by using the IDbContextFactory pattern rather than a long-lived DbContext (AI generates this correctly when you describe the requirement precisely), manually reviewing every migration script before execution, and routing all secrets through environment variables — never committed to the repository.</p>

<h2>Why This Stack Works Well with Vibe Coding</h2>

<p>Not all stacks are equally AI-friendly. Here is why my choices worked well with Claude Code:</p>

<p><strong>Blazor InteractiveServer (.NET 10):</strong> No separate API layer — a single mental model. Claude Code understands this pattern well and generates correct code. .NET 10 also introduces the <code>[PersistentState]</code> attribute for declarative state persistence — exactly the kind of boilerplate ceremony that AI handles better than writing by hand:</p>

<pre><code>// .NET 10 - PersistentState persists across page refreshes declaratively
[PersistentState(nameof(counter))]
private PersistentState&lt;int&gt; counter;
</code></pre>

<p><strong>EF Core with IDbContextFactory:</strong> This pattern prevents AI from generating the long-lived DbContext anti-pattern — a common mistake. When you describe the requirement clearly, Claude Code generates the right pattern:</p>

<pre><code>// IDbContextFactory pattern - AI-friendly, thread-safe
public class BlogService(IDbContextFactory&lt;AppDbContext&gt; factory)
{
    public async Task&lt;List&lt;Post&gt;&gt; GetPostsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Posts
            .OrderByDescending(p =&gt; p.PublishedAt)
            .ToListAsync();
    }
}
</code></pre>

<p><strong>Neon (serverless PostgreSQL):</strong> Suspend mode means the free tier is viable for a personal site with intermittent traffic. Neon has announced PostgreSQL 18 features including UUIDv7 (time-ordered, ideal for event tables), Virtual Generated Columns (computed values without migrations), and database branching — database-per-PR pairs naturally with AI-driven workflows.</p>

<p><strong>Fly.io:</strong> CLI-first, well-represented in AI training data. Scale-to-zero works well with Blazor SignalR — once a connection is established, there is no cold-start penalty. The shared-cpu-1x / 256MB tier is a viable floor for a Blazor app.</p>

<p>For a deeper look at the deployment side: <a href=""/blog/deploy-dotnet-fly-io-2025"">Deploying .NET to Fly.io in 2025</a>.</p>

<h2>The Developer Role in 2026: Redefined, Not Eliminated</h2>

<p>65% of developers expect their role to be redefined toward architecture, orchestration, and oversight. Entry-level job postings are down roughly 40% from 2022 peaks. AI skills now appear in 42% of software job descriptions — up from 8% in 2022. Developers with demonstrated AI tool proficiency secure roles <strong>2.3x faster</strong>.</p>

<p>But CNN reported in April 2026: ""The demise of software engineering jobs has been greatly exaggerated"" — overall job listings are still up 11% annually. The composition has shifted structurally, not collapsed.</p>

<p>The most honest framing: vibe coding <strong>lowered the floor</strong> (anyone can ship something) but <strong>raised the ceiling expectations</strong> (if you can ship with AI, you are expected to ship more). The developer who builds a portfolio site with Claude Code over a weekend is demonstrating AI fluency — not taking a shortcut.</p>

<p>The new bottleneck is not ""can you write the code"" but <strong>""can you verify, review, and architect what the AI produces.""</strong></p>

<p>Related reading: <a href=""/blog/ai-skills-for-developers"">AI Skills for Developers in 2026</a> and <a href=""/blog/building-with-ai-experience-with-claude-code"">Building with Claude Code: A Real-World Experience</a>.</p>

<h2>When to Use It, When to Be Careful</h2>

<p><strong>Vibe coding works best when:</strong></p>
<ul>
  <li>Greenfield project with no legacy constraints</li>
  <li>Well-documented stack that AI models know well (Blazor, EF Core, React, Next.js, PostgreSQL)</li>
  <li>Single developer or small team with clear architectural direction</li>
  <li>Fast prototyping, MVPs, personal projects</li>
  <li>Heavy boilerplate: CRUD operations, migrations, component scaffolding</li>
</ul>

<p><strong>Vibe coding carries high risk when:</strong></p>
<ul>
  <li>Complex, long-lived codebase with limited documentation</li>
  <li>Payment processing, authentication, cryptography</li>
  <li>Anything handling sensitive data without a dedicated security review</li>
  <li>No process for reviewing AI output before it reaches production</li>
  <li>Long-term maintenance by a large team</li>
</ul>

<h2>FAQ</h2>

<h3>Is vibe coding ""real"" programming?</h3>
<p>This question frames the problem incorrectly. Developers skilled at vibe coding still make every meaningful architectural decision: choosing the stack, designing the data model, defining render modes, configuring the CI/CD pipeline, identifying security boundaries. AI is a code-generation layer, not an architect. Similarly, a developer using an IDE with autocomplete is still ""really programming"" — they are simply using better tools. The question worth asking is not whether the approach is legitimate, but whether the developer understands and is accountable for what gets shipped.</p>

<h3>How is Claude Code different from GitHub Copilot?</h3>
<p>Copilot is primarily inline autocomplete — it suggests code as you type. Claude Code operates in agent mode — it can read files in your project, run builds, interpret compiler errors, and iterate through multiple steps without requiring your intervention at each one. This is a qualitative difference, not just a capability upgrade. When I built VodonghaPersonal.id.vn, Claude Code was not suggesting individual lines — it was reading the full project context and making coherent decisions across the codebase. This explains why 46% of senior developers prefer it: it mirrors how they think about problems, not just how they type.</p>

<h3>Is it safe to use in production?</h3>
<p>Under the right conditions: yes. Those conditions are: you review everything AI generates before deployment, you have a dedicated security review process (especially for authentication, data exposure, and input handling), and you never blindly commit AI output into production. The Moltbook incident was not caused by AI writing code — it was caused by the absence of a review process. The 2.74x vulnerability figure measures unreviewed AI code. An experienced developer with a solid review process can reduce that risk substantially. The 86% XSS failure rate is a reason to add security checks, not a reason to avoid AI-assisted development entirely.</p>

<h3>Should I worry about my job as a developer?</h3>
<p>Worry less; adapt immediately. Entry-level postings are down 40%, but total job listings are still growing at 11% annually. The composition has shifted: the winners are developers who can work effectively with AI — verifying output, reviewing for security issues, designing systems. The risk is in resisting the tools. 42% of job descriptions now mention AI skills; developers proficient in AI tooling secure roles 2.3 times faster. The developer reading this article has already started adapting. That is the right move.</p>

<h3>Is the .NET 10 stack a good choice for learning vibe coding?</h3>
<p>It is an excellent choice. Blazor, EF Core, and ASP.NET Core are all well-represented in the training data of major AI models. .NET 10 introduces new patterns like <code>[PersistentState]</code> and improved circuit resilience that AI can generate correctly when you describe requirements clearly. The challenge is not that AI does not know .NET — it is that you need enough knowledge to recognize when AI generates a wrong pattern (for example, a long-lived DbContext instead of IDbContextFactory). Related: <a href=""/blog/blazor-vs-react-2025"">Blazor vs React in 2025</a> and <a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL and EF Core Best Practices</a>.</p>

<h2>Conclusion</h2>

<p>Vibe coding in 2026 is neither pure hype nor a perfect solution. It represents a genuine shift in how developers approach code — with clear benefits (3.6 hours saved per week, 60% more PRs, onboarding twice as fast) and equally clear risks (2.74x vulnerabilities, 86% XSS failure rate, Moltbook exposing 1.5 million tokens in three days).</p>

<p>When I built VodonghaPersonal.id.vn with Claude Code, I did not ""forget that the code even exists"" in Karpathy's literal sense. I reviewed everything, understood every architectural decision, and read every migration script. But I stopped being the person who typed every line of boilerplate. I became an architect using AI as a code-generation layer.</p>

<p>That is the developer role in 2026: not the fastest typist, but the person who can <em>direct, verify, and take responsibility</em> for what the AI produces.</p>

<p>Next up: <a href=""/blog/building-with-ai-experience-with-claude-code"">Building with Claude Code: From Zero to Production — A Real-World Account</a>.</p>", @"Vibe Coding là gì và tại sao nó trở thành từ của năm 2025? Khám phá thực tế năm 2026: năng suất, bảo mật, rủi ro và bài học từ việc xây dựng VodonghaPersonal.id.vn hoàn toàn bằng Claude Code.", @"What is Vibe Coding and why did it become Word of the Year 2025? Explore the 2026 reality: productivity gains, security risks, and lessons from building VodonghaPersonal.id.vn entirely with Claude Code.", @"vibe-coding, ai-development, claude-code, productivity, software-engineering, dotnet, 2026", @"Vibe Coding là gì? Thực tế năm 2026", @"What Is Vibe Coding? The Reality in 2026", new DateTime(2026, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Content", "ContentEn", "Summary", "SummaryEn", "Tags", "Title", "TitleEn", "UpdatedAt" },
                values: new object[] { @"<p><img src=""https://images.unsplash.com/photo-1667372393119-3d4c48d07fc9?w=1200&auto=format&fit=crop&q=80"" alt=""Deploy .NET 10 Blazor lên Fly.io với Neon PostgreSQL"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Trang portfolio <strong>VodonghaPersonal.id.vn</strong> của mình chạy trên .NET 10 Blazor InteractiveServer, PostgreSQL qua Neon (Singapore), và được host trên Fly.io — tất cả với chi phí gần như bằng 0 cho một site lưu lượng thấp. Bài viết này tổng hợp lại toàn bộ quá trình setup, bao gồm những cái bẫy mà mình đã tự mình vấp phải.</p>

<p>Nếu bạn đang muốn host một side project hoặc portfolio cá nhân với stack .NET hiện đại mà không tốn nhiều tiền, đây là bài viết dành cho bạn.</p>

<h2>Tại sao chọn .NET 10 + Fly.io + Neon?</h2>

<p>.NET 10 được phát hành ngày <strong>11 tháng 11 năm 2025</strong> và là bản LTS (hỗ trợ 3 năm) — lựa chọn tốt cho một site cá nhân vì mình không muốn upgrade liên tục. Blazor trong .NET 10 cũng có khá nhiều cải tiến thực chất, đặc biệt là <code>PersistentState</code> attribute giúp chia sẻ state giữa các render và khôi phục lại component state khi reconnect — rất hữu ích cho InteractiveServer.</p>

<p>Fly.io thì mình chọn vì nó có máy chủ ở Singapore, deploy bằng Docker đơn giản, và hỗ trợ <code>auto_stop_machines = ""suspend""</code> — tính năng snapshot memory ra đĩa, giúp resume nhanh hơn nhiều so với cold-stop thông thường.</p>

<p>Neon PostgreSQL có region Singapore (<code>ap-southeast-1</code>), đồng vị với Fly.io Singapore, free tier hào phóng, và giá compute giảm mạnh sau khi Databricks mua lại vào tháng 5 năm 2025 (storage chỉ còn <strong>$0.35/GB-tháng</strong>).</p>

<h2>Chuẩn bị: Tạo Neon Database</h2>

<p>Đầu tiên, tạo project trên <a href=""https://neon.tech"" target=""_blank"">neon.tech</a>, chọn region <strong>Asia Pacific (Singapore)</strong>.</p>

<p>Sau khi tạo xong, vào phần <strong>Connection Details</strong> và lấy <strong>pooled connection string</strong> (không phải direct). Đây là điểm quan trọng: với Blazor Server, các SignalR circuit tồn tại lâu dài và có thể mở nhiều connection đồng thời. Dùng pooled string của Neon (qua PgBouncer) để tránh cạn kiệt connection.</p>

<p>Connection string sẽ trông giống như:</p>

<pre><code>postgresql://user:password@ep-xxx-pooler.ap-southeast-1.aws.neon.tech/neondb?sslmode=require</code></pre>

<p>Lưu ý có <code>-pooler</code> trong hostname — đó chính là pooled endpoint.</p>

<h2>Dockerfile cho .NET 10 Blazor</h2>

<p>Dùng multi-stage build để giữ image nhỏ gọn:</p>

<pre><code>FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT [""dotnet"", ""YourApp.dll""]</code></pre>

<p>Một vài lưu ý quan trọng:</p>
<ul>
  <li>Dùng image <code>aspnet:10.0</code> (không phải <code>sdk</code>) cho runtime — nhỏ hơn nhiều.</li>
  <li><strong>Bắt buộc</strong> set <code>ENV ASPNETCORE_URLS=http://+:8080</code>. Fly.io mặc định kỳ vọng port 8080, và ASP.NET Core Docker image từ .NET 8 trở đi cũng dùng 8080 — nhưng cứ set tường minh cho chắc.</li>
  <li>Nếu bạn có nhiều project trong solution, copy file <code>.sln</code> và chỉ định đúng project khi publish.</li>
</ul>

<h2>Cấu hình fly.toml</h2>

<p>Chạy <code>fly launch</code> lần đầu để sinh file <code>fly.toml</code>, sau đó chỉnh lại:</p>

<pre><code>app = ""your-app-name""
primary_region = ""sin""

[build]

[env]
  ASPNETCORE_ENVIRONMENT = ""Production""

[[services]]
  internal_port = 8080
  protocol = ""tcp""

  [[services.ports]]
    handlers = [""http""]
    port = 80
    force_https = true

  [[services.ports]]
    handlers = [""tls"", ""http""]
    port = 443

  [services.concurrency]
    type = ""connections""
    hard_limit = 25
    soft_limit = 20

  [[services.http_checks]]
    interval = ""15s""
    timeout = ""10s""
    grace_period = ""30s""
    method = ""GET""
    path = ""/health""

[machines]
  auto_stop_machines = ""suspend""
  auto_start_machines = true
  min_machines_running = 0</code></pre>

<p>Phần <code>auto_stop_machines = ""suspend""</code> là khác biệt quan trọng so với <code>""stop""</code>. Suspend snapshot memory ra đĩa, khi có request mới machine resume trong vài trăm millisecond thay vì nhiều giây. Rất phù hợp cho portfolio site lưu lượng không đều.</p>

<h2>Cái bẫy lớn nhất: Sticky Sessions với Blazor Server</h2>

<p>Đây là gotcha mình muốn nhấn mạnh nhất. <strong>Fly.io không hỗ trợ sticky sessions</strong>. Blazor Server dùng SignalR — mỗi connection cần được route về đúng process đã tạo ra nó. Nếu bạn chạy 2 machine trở lên trong cùng một region, load balancer của Fly.io sẽ phân phối request ngẫu nhiên, và SignalR connection sẽ bị đứt.</p>

<p>Giải pháp thực tế:</p>
<ul>
  <li><strong>Chạy đúng 1 machine mỗi region.</strong> Scale theo chiều ngang bằng cách thêm region, không phải thêm machine trong cùng region.</li>
  <li>Hoặc scale dọc (CPU/RAM lớn hơn) thay vì scale ngang.</li>
  <li>Nếu bạn thực sự cần nhiều replica, phải dùng Redis backplane cho SignalR — nhưng với portfolio site thì không cần thiết.</li>
</ul>

<p>Mình đã bị lỗi này khi thử deploy 2 machine để tăng availability, mất cả buổi debug trước khi hiểu ra vấn đề.</p>

<h2>EF Core với IDbContextFactory — Bắt buộc, Không Tùy Chọn</h2>

<p>Với Blazor Server, <strong>không được dùng <code>AddDbContext</code> với scoped lifetime</strong>. SignalR circuit tồn tại lâu dài và có thể có nhiều thao tác đồng thời — <code>DbContext</code> không thread-safe và không được thiết kế để tồn tại qua nhiều request.</p>

<p>Đăng ký trong <code>Program.cs</code>:</p>

<pre><code>builder.Services.AddPooledDbContextFactory&lt;AppDbContext&gt;(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(""DefaultConnection"")));</code></pre>

<p>Dùng trong service hoặc component:</p>

<pre><code>public class BlogService
{
    private readonly IDbContextFactory&lt;AppDbContext&gt; _dbContextFactory;

    public BlogService(IDbContextFactory&lt;AppDbContext&gt; dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task&lt;List&lt;BlogPost&gt;&gt; GetPostsAsync()
    {
        await using AppDbContext db = await _dbContextFactory.CreateDbContextAsync();
        return await db.BlogPosts
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }
}</code></pre>

<p><code>AddPooledDbContextFactory</code> còn tốt hơn <code>AddDbContextFactory</code> vì nó tái sử dụng context instance (pool size mặc định 1024), kết hợp tốt với PgBouncer của Neon để tối ưu connection.</p>

<p>Mình cũng dùng EF Core 9 (phát hành tháng 11 năm 2024) thay vì EF Core 10 vì EF Core 9 là STS còn được support đến tháng 11 năm 2026, và mình chưa cần các tính năng mới của EF Core 10.</p>

<h2>Xử lý Connection String và Secrets</h2>

<p>Không bao giờ commit connection string vào code. Dùng Fly.io secrets:</p>

<pre><code>fly secrets set ConnectionStrings__DefaultConnection=""postgresql://...""
fly secrets set DataProtection__KeyPath=""/app/keys""</code></pre>

<p>Trong <code>appsettings.Production.json</code>, để placeholder:</p>

<pre><code>{
  ""ConnectionStrings"": {
    ""DefaultConnection"": """"
  }
}</code></pre>

<p>Fly.io inject secrets thành environment variables, ASP.NET Core tự động map <code>ConnectionStrings__DefaultConnection</code> (dấu <code>__</code> là separator cho nested config).</p>

<p>Một lưu ý với Neon: nếu bạn dùng <strong>suspend mode</strong>, lần kết nối đầu tiên sau khi database ""ngủ"" sẽ mất khoảng 1.8–2.6 giây (median theo benchmark thực tế năm 2025). Neon đang cải thiện và cam kết sub-1s vào cuối 2026. Với portfolio site, điều này chấp nhận được — visitor đầu tiên thấy loading hơi chậm, các visitor sau thì bình thường.</p>

<h2>GitHub Actions CI/CD Pipeline</h2>

<p>Tạo file <code>.github/workflows/deploy.yml</code>:</p>

<pre><code>name: Deploy to Fly.io

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: nuget-

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Deploy to Fly.io
        uses: superfly/flyctl-actions/setup-flyctl@master

      - run: flyctl deploy --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}</code></pre>

<p>Thêm <code>FLY_API_TOKEN</code> vào GitHub repository secrets (lấy từ <code>fly auth token</code>). Flag <code>--remote-only</code> cho phép Fly.io build Docker image trên infrastructure của họ, không cần build local trong CI — nhanh hơn và không tốn GitHub Actions minutes cho Docker build.</p>

<p>Nếu bạn muốn build local trong CI để dùng layer cache:</p>

<pre><code>      - name: Build and push
        uses: docker/build-push-action@v6
        with:
          context: .
          push: false
          tags: registry.fly.io/your-app:${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max</code></pre>

<h2>Những tính năng .NET 10 Blazor mình đang dùng</h2>

<p>Sau khi upgrade từ .NET 9 lên .NET 10, mình đã tận dụng một số tính năng mới:</p>

<p><strong>PersistentState attribute:</strong> Trước đây, khi Blazor Server reconnect sau mất kết nối, component bị reset về state ban đầu. Với <code>[PersistentState]</code>, bạn có thể đánh dấu những property cần được khôi phục:</p>

<pre><code>@code {
    [PersistentState]
    private int CurrentPage { get; set; } = 1;

    [PersistentState]
    private string SearchQuery { get; set; } = """";
}</code></pre>

<p><strong>NotFoundPage trên Router:</strong> Thay vì hack <code>RouteView</code> và <code>LayoutView</code>, giờ có thể khai báo tường minh:</p>

<pre><code>&lt;Router AppAssembly=""typeof(App).Assembly""&gt;
    &lt;Found Context=""routeData""&gt;
        &lt;RouteView RouteData=""routeData"" DefaultLayout=""typeof(MainLayout)"" /&gt;
    &lt;/Found&gt;
    &lt;NotFound&gt;
        &lt;NotFoundPage /&gt;
    &lt;/NotFound&gt;
&lt;/Router&gt;</code></pre>

<p>Đơn giản và sạch hơn nhiều so với cách cũ.</p>

<h2>Chi phí thực tế</h2>

<p>Fly.io đã bỏ free tier từ năm 2024. Với setup của mình:</p>
<ul>
  <li><strong>Fly.io:</strong> shared-cpu-1x, 256MB RAM, Singapore. Với <code>auto_stop_machines = ""suspend""</code> và <code>min_machines_running = 0</code>, chi phí khoảng <strong>$0–$1/tháng</strong> cho site lưu lượng thấp (machine chỉ chạy khi có request).</li>
  <li><strong>Neon:</strong> Free tier có 0.5 compute unit giờ/tháng và 512MB storage — đủ cho portfolio site. Storage tính thêm chỉ $0.35/GB-tháng.</li>
  <li><strong>Tổng:</strong> Gần như miễn phí cho tháng đầu trial Fly.io, sau đó khoảng $0–$2/tháng tùy lưu lượng.</li>
</ul>

<p>Khác với nhiều giải pháp hosting khác, cả Fly.io lẫn Neon đều không cần credit card chỉ để thử nghiệm — Neon free tier không hết hạn, Fly.io cho 2 VM-hour trial.</p>

<h2>FAQ</h2>

<h3>Tại sao không dùng Azure App Service cho .NET?</h3>
<p>Azure App Service hợp lý cho môi trường enterprise (đặc biệt nếu đã có Azure subscription). Nhưng cho personal site, plan rẻ nhất của Azure vẫn đắt hơn setup này và không có suspend mode. Fly.io gần với Docker hơn — nếu bạn đã quen Docker, workflow rất tự nhiên. Mình dùng Azure ở công việc chính nhưng chọn Fly.io cho side project.</p>

<h3>Blazor WASM có vấn đề sticky sessions không?</h3>
<p>Không. Blazor WASM chạy hoàn toàn trên browser, server chỉ serve static files và API calls stateless. Vấn đề sticky sessions chỉ xảy ra với <strong>Blazor Server (InteractiveServer)</strong> vì nó duy trì SignalR connection với server. Nếu bạn dùng Blazor WASM, bạn có thể scale thoải mái.</p>

<h3>Neon cold start có ảnh hưởng nhiều không?</h3>
<p>Phụ thuộc vào use case. Với portfolio site, visitor đầu tiên sau nhiều giờ không có traffic sẽ thấy loading ~2–3 giây cho database wake up. Các request tiếp theo bình thường. Nếu bạn không chấp nhận được điều này, có thể dùng Fly.io Machine để tạo cron job ping database 5–10 phút/lần để giữ nó luôn warm — nhưng sẽ tốn thêm chi phí compute. Với mình, cold start không phải vấn đề.</p>

<h3>Làm sao để chạy EF Core migrations khi deploy?</h3>
<p>Mình chạy migration trong application startup với một guard condition:</p>
<pre><code>if (app.Environment.IsProduction())
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService&lt;AppDbContext&gt;();
    await db.Database.MigrateAsync();
}</code></pre>
<p>Cách này đơn giản cho single-instance deployment. Nếu bạn có nhiều instance hoặc cần rollback phức tạp, nên tách migration thành một bước riêng trong CI/CD pipeline.</p>

<h3>Tại sao dùng <code>suspend</code> thay vì <code>stop</code> cho Fly.io?</h3>
<p><code>stop</code> dừng machine hoàn toàn — khi có request mới phải boot lại từ đầu, mất vài giây. <code>suspend</code> snapshot toàn bộ memory state ra đĩa — khi resume chỉ cần load snapshot lại, mất vài trăm millisecond. Với Blazor Server, suspend đặc biệt có lợi vì .NET runtime và Blazor circuit đã được khởi tạo sẵn trong snapshot, không cần warm up lại.</p>

<h2>Kết luận</h2>

<p>Setup .NET 10 Blazor + Fly.io + Neon PostgreSQL hoạt động rất tốt cho personal site và portfolio. Chi phí thực tế gần như bằng 0, deploy workflow sạch với Docker và GitHub Actions, và .NET 10 LTS đảm bảo bạn không phải upgrade trong vài năm tới.</p>

<p>Hai điều cần nhớ nhất: <strong>sticky sessions</strong> (1 machine/region với Blazor Server) và <strong>IDbContextFactory</strong> (bắt buộc, không phải tùy chọn). Nếu hiểu hai điều này từ đầu, bạn sẽ tiết kiệm được rất nhiều thời gian debug.</p>

<p>Trang <a href=""https://VodonghaPersonal.id.vn"" target=""_blank"">VodonghaPersonal.id.vn</a> của mình hiện đang chạy với đúng stack này — bạn có thể xem source code trên GitHub để tham khảo thêm.</p>

<p>Nếu bạn quan tâm đến cách mình xây dựng toàn bộ site này với sự hỗ trợ của AI, đọc thêm bài <a href=""/blog/building-with-ai-experience-with-claude-code"">Kinh nghiệm dùng Claude Code để build portfolio</a>. Để hiểu sâu hơn về EF Core với PostgreSQL, xem bài <a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL và EF Core: Best Practices thực tế</a>. Và nếu bạn đang phân vân giữa Blazor và React cho project tiếp theo, bài <a href=""/blog/blazor-vs-react-2025"">Blazor vs React 2025</a> có thể giúp ích.</p>", @"<p><img src=""https://images.unsplash.com/photo-1667372393119-3d4c48d07fc9?w=1200&auto=format&fit=crop&q=80"" alt=""Deploy .NET 10 Blazor to Fly.io with Neon PostgreSQL"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>My portfolio site <strong>VodonghaPersonal.id.vn</strong> runs on .NET 10 Blazor InteractiveServer, PostgreSQL via Neon (Singapore region), and is hosted on Fly.io — all at effectively zero cost for a low-traffic personal site. This post documents the full setup, including every gotcha I stumbled into along the way.</p>

<p>If you want to host a side project or personal portfolio with a modern .NET stack without spending much money, this is the guide for you.</p>

<h2>Why .NET 10 + Fly.io + Neon?</h2>

<p>.NET 10 was released on <strong>November 11, 2025</strong> as an LTS release with three years of support — a good choice for a personal site since I do not want to deal with frequent upgrades. Blazor in .NET 10 brings meaningful improvements, particularly the <code>PersistentState</code> attribute for sharing state between interactive renders and restoring component state on reconnect, which is directly useful for InteractiveServer mode.</p>

<p>I chose Fly.io because it has servers in Singapore, uses Docker for deployment (a workflow I already know), and supports <code>auto_stop_machines = ""suspend""</code> — a feature that snapshots machine memory to disk so resume takes hundreds of milliseconds rather than several seconds for a full cold boot.</p>

<p>Neon PostgreSQL offers a Singapore region (<code>ap-southeast-1</code>) that co-locates with Fly.io Singapore for minimal latency, a generous free tier, and pricing that dropped significantly after the Databricks acquisition in May 2025 — storage is now just <strong>$0.35/GB-month</strong>.</p>

<h2>Step 1: Create Your Neon Database</h2>

<p>Create a project on <a href=""https://neon.tech"" target=""_blank"">neon.tech</a> and select the <strong>Asia Pacific (Singapore)</strong> region (or whichever is closest to your Fly.io region).</p>

<p>After creation, go to <strong>Connection Details</strong> and copy the <strong>pooled connection string</strong> — not the direct one. This distinction matters for Blazor Server: SignalR circuits are long-lived and can initiate multiple concurrent operations. Using Neon's pooled endpoint (via PgBouncer) prevents connection exhaustion.</p>

<p>The pooled connection string looks like:</p>

<pre><code>postgresql://user:password@ep-xxx-pooler.ap-southeast-1.aws.neon.tech/neondb?sslmode=require</code></pre>

<p>Note the <code>-pooler</code> in the hostname — that is what routes through PgBouncer.</p>

<h2>Step 2: Dockerfile for .NET 10</h2>

<p>Use a multi-stage build to keep the final image small:</p>

<pre><code>FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT [""dotnet"", ""YourApp.dll""]</code></pre>

<p>A few important notes:</p>
<ul>
  <li>Use the <code>aspnet:10.0</code> image (not <code>sdk</code>) for the runtime stage — it is significantly smaller.</li>
  <li><strong>Always set <code>ENV ASPNETCORE_URLS=http://+:8080</code> explicitly.</strong> Fly.io expects port 8080, and ASP.NET Core Docker images default to 8080 since .NET 8 — but being explicit avoids surprises.</li>
  <li>If your solution has multiple projects, make sure you are publishing the correct one. Specify the project path in the <code>dotnet publish</code> command if needed.</li>
</ul>

<h2>Step 3: Configure fly.toml</h2>

<p>Run <code>fly launch</code> once to generate the initial <code>fly.toml</code>, then edit it:</p>

<pre><code>app = ""your-app-name""
primary_region = ""sin""

[build]

[env]
  ASPNETCORE_ENVIRONMENT = ""Production""

[[services]]
  internal_port = 8080
  protocol = ""tcp""

  [[services.ports]]
    handlers = [""http""]
    port = 80
    force_https = true

  [[services.ports]]
    handlers = [""tls"", ""http""]
    port = 443

  [services.concurrency]
    type = ""connections""
    hard_limit = 25
    soft_limit = 20

  [[services.http_checks]]
    interval = ""15s""
    timeout = ""10s""
    grace_period = ""30s""
    method = ""GET""
    path = ""/health""

[machines]
  auto_stop_machines = ""suspend""
  auto_start_machines = true
  min_machines_running = 0</code></pre>

<p>The <code>auto_stop_machines = ""suspend""</code> setting is the key difference from <code>""stop""</code>. Suspend snapshots the machine's memory to disk; when a new request arrives, the machine resumes from that snapshot in a few hundred milliseconds instead of doing a full cold boot that takes several seconds. This is ideal for a portfolio site with irregular traffic patterns.</p>

<h2>The Biggest Gotcha: Sticky Sessions and Blazor Server</h2>

<p>This is the single most important thing to understand before deploying Blazor Server to Fly.io. <strong>Fly.io does not support sticky sessions.</strong> Blazor Server uses SignalR — every request for a given circuit must reach the same process that created it. If you run two machines in the same region, Fly.io's load balancer distributes requests randomly, and SignalR connections break.</p>

<p>The practical solutions:</p>
<ul>
  <li><strong>Run exactly one machine per region.</strong> Scale horizontally by adding regions, not by adding machines within the same region.</li>
  <li>Alternatively, scale vertically — use a larger machine type (more CPU, more RAM) to handle more concurrent connections on a single process.</li>
  <li>If you genuinely need multiple replicas in the same region, you need a Redis SignalR backplane — but that adds cost and complexity that is unnecessary for a personal site.</li>
</ul>

<p>I ran into this issue when I tried deploying two machines for higher availability and spent an afternoon debugging intermittent disconnects before understanding the root cause. If you are deploying Blazor WASM instead of Blazor Server, this is not a concern — WASM runs entirely on the browser and makes stateless API calls.</p>

<h2>EF Core with IDbContextFactory — Required, Not Optional</h2>

<p>With Blazor Server, <strong>do not use <code>AddDbContext</code> with a scoped lifetime.</strong> A SignalR circuit is long-lived, and Blazor components can initiate multiple concurrent database operations within that circuit. <code>DbContext</code> is not thread-safe and is not designed to survive across multiple requests.</p>

<p>Register in <code>Program.cs</code>:</p>

<pre><code>builder.Services.AddPooledDbContextFactory&lt;AppDbContext&gt;(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(""DefaultConnection"")));</code></pre>

<p>Use it in a service or component:</p>

<pre><code>public class BlogService
{
    private readonly IDbContextFactory&lt;AppDbContext&gt; _dbContextFactory;

    public BlogService(IDbContextFactory&lt;AppDbContext&gt; dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task&lt;List&lt;BlogPost&gt;&gt; GetPostsAsync()
    {
        await using AppDbContext db = await _dbContextFactory.CreateDbContextAsync();
        return await db.BlogPosts
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }
}</code></pre>

<p><code>AddPooledDbContextFactory</code> is preferable to <code>AddDbContextFactory</code> because it reuses context instances from a pool (default size: 1024), which pairs well with Neon's PgBouncer pooling to minimize the number of actual connections opened against Postgres.</p>

<p>I am running EF Core 9 (released November 2024) rather than EF Core 10, because EF Core 9 remains supported until November 2026 and I do not yet need EF Core 10 features. Both work identically with this setup.</p>

<h2>Managing Secrets and Connection Strings</h2>

<p>Never commit connection strings. Use Fly.io secrets:</p>

<pre><code>fly secrets set ConnectionStrings__DefaultConnection=""postgresql://...""
fly secrets set DataProtection__KeyPath=""/app/keys""</code></pre>

<p>In <code>appsettings.Production.json</code>, leave the value empty as a placeholder:</p>

<pre><code>{
  ""ConnectionStrings"": {
    ""DefaultConnection"": """"
  }
}</code></pre>

<p>Fly.io injects secrets as environment variables, and ASP.NET Core automatically maps <code>ConnectionStrings__DefaultConnection</code> (the double underscore is the hierarchy separator for nested configuration).</p>

<p>One real-world note on Neon cold starts: the first connection after the database has been idle will take approximately 1.8–2.6 seconds (median, based on 2025 benchmarks). Neon has been improving this and has publicly committed to sub-1-second cold starts by end of 2026. For a portfolio site, this is an acceptable trade-off — the first visitor after a long quiet period sees a slightly slower initial load, and everyone after that is fine.</p>

<h2>GitHub Actions CI/CD Pipeline</h2>

<p>Create <code>.github/workflows/deploy.yml</code>:</p>

<pre><code>name: Deploy to Fly.io

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: nuget-

      - name: Set up flyctl
        uses: superfly/flyctl-actions/setup-flyctl@master

      - name: Deploy
        run: flyctl deploy --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}</code></pre>

<p>Add <code>FLY_API_TOKEN</code> to your GitHub repository secrets — get it from <code>fly auth token</code>. The <code>--remote-only</code> flag tells Fly.io to build the Docker image on their infrastructure rather than inside the GitHub Actions runner. This is faster and does not consume GitHub Actions minutes for the Docker build step.</p>

<p>If you want local CI builds with layer caching instead:</p>

<pre><code>      - name: Build with cache
        uses: docker/build-push-action@v6
        with:
          context: .
          push: false
          tags: registry.fly.io/your-app:${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max</code></pre>

<h2>.NET 10 Blazor Features Worth Using</h2>

<p>After upgrading VodonghaPersonal.id.vn from .NET 9 to .NET 10, I adopted a few of the new Blazor features immediately.</p>

<p><strong>PersistentState attribute:</strong> Previously, when Blazor Server lost and re-established a connection, components would reset to their initial state. With <code>[PersistentState]</code>, you can mark properties that should survive reconnection:</p>

<pre><code>@code {
    [PersistentState]
    private int CurrentPage { get; set; } = 1;

    [PersistentState]
    private string SearchQuery { get; set; } = """";
}</code></pre>

<p>This is especially useful for paginated lists or search results — users no longer lose their position after a brief disconnect.</p>

<p><strong>NotFoundPage on the Router:</strong> Instead of the old hack of nesting <code>RouteView</code> and <code>LayoutView</code> to handle 404s, you can now declare it cleanly:</p>

<pre><code>&lt;Router AppAssembly=""typeof(App).Assembly""&gt;
    &lt;Found Context=""routeData""&gt;
        &lt;RouteView RouteData=""routeData"" DefaultLayout=""typeof(MainLayout)"" /&gt;
    &lt;/Found&gt;
    &lt;NotFound&gt;
        &lt;NotFoundPage /&gt;
    &lt;/NotFound&gt;
&lt;/Router&gt;</code></pre>

<p>Small change, noticeably cleaner.</p>

<h2>Actual Costs</h2>

<p>Fly.io removed its free tier in 2024. New accounts get a 2 VM-hour / 7-day trial. With this stack, my monthly costs are:</p>
<ul>
  <li><strong>Fly.io:</strong> shared-cpu-1x, 256MB RAM, Singapore. With <code>auto_stop_machines = ""suspend""</code> and <code>min_machines_running = 0</code>, the machine only runs when requests arrive. For a low-traffic portfolio site this works out to approximately <strong>$0–$1/month</strong>.</li>
  <li><strong>Neon:</strong> Free tier includes 0.5 compute unit hours/month and 512MB storage — more than enough for a portfolio. Additional storage is $0.35/GB-month.</li>
  <li><strong>Total:</strong> Effectively free during the Fly.io trial, then roughly $0–$2/month depending on traffic.</li>
</ul>

<p>Neither Neon nor Fly.io require a credit card just to experiment — Neon's free tier does not expire, and Fly.io gives you the trial compute hours before asking for payment details.</p>

<h2>FAQ</h2>

<h3>Why not use Azure App Service for a .NET application?</h3>
<p>Azure App Service makes sense in enterprise settings, especially if you already have an Azure subscription. But for a personal site, Azure's cheapest plans are more expensive than this setup and do not offer a suspend equivalent. Fly.io is closer to Docker-native — if you already know Docker, the deployment workflow feels natural. I use Azure at my day job but chose Fly.io for personal projects precisely because it gets out of the way.</p>

<h3>Does Blazor WASM have the sticky sessions problem?</h3>
<p>No. Blazor WASM runs entirely in the browser — the server only serves static files and handles stateless API calls. The sticky sessions constraint is specific to <strong>Blazor Server (InteractiveServer)</strong> because it maintains a persistent SignalR connection back to the server process. If you use Blazor WASM, you can scale with multiple replicas freely.</p>

<h3>How much does Neon's cold start actually affect the user experience?</h3>
<p>It depends on your traffic patterns. For a portfolio site, the first visitor after several hours of no activity will see a 2–3 second delay while the database wakes up. Subsequent requests are normal. If this is unacceptable, you can create a Fly.io cron Machine that pings the database every 5–10 minutes to keep it warm — but this adds compute cost. For my use case, the occasional cold start is an acceptable trade-off for near-zero hosting costs.</p>

<h3>How do you run EF Core migrations on deploy?</h3>
<p>I run migrations during application startup with a guard condition:</p>
<pre><code>if (app.Environment.IsProduction())
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService&lt;AppDbContext&gt;();
    await db.Database.MigrateAsync();
}</code></pre>
<p>This is straightforward for a single-instance deployment. If you have multiple instances or need complex rollback capabilities, separate the migration step into a dedicated CI/CD job that runs before the deploy, using <code>dotnet ef database update</code> or a custom migration runner.</p>

<h3>What is the difference between <code>suspend</code> and <code>stop</code> on Fly.io?</h3>
<p>With <code>stop</code>, the machine shuts down completely. When a new request arrives, Fly.io must do a full cold boot — download the image, start the .NET runtime, initialize the Blazor application. This takes several seconds. With <code>suspend</code>, the entire memory state of the running machine is snapshotted to disk. Resume means loading that snapshot back into memory, which takes a few hundred milliseconds. For Blazor Server in particular, suspend is especially beneficial because the .NET runtime and SignalR infrastructure are already initialized in the snapshot.</p>

<h2>Conclusion</h2>

<p>The .NET 10 Blazor + Fly.io + Neon PostgreSQL stack works very well for personal sites and portfolios. Real-world costs are near zero for low-traffic sites, the Docker-based deployment workflow is clean and reproducible, and .NET 10 LTS means you are not scrambling to upgrade every year.</p>

<p>The two things worth internalizing before you start: the <strong>sticky sessions constraint</strong> (one machine per region for Blazor Server) and <strong><code>IDbContextFactory</code></strong> (mandatory, not optional). Understanding these upfront will save you significant debugging time.</p>

<p><a href=""https://VodonghaPersonal.id.vn"" target=""_blank"">VodonghaPersonal.id.vn</a> is currently running this exact stack — you are reading a page served by it right now.</p>

<p>If you are interested in how this entire site was built with AI assistance, read <a href=""/blog/building-with-ai-experience-with-claude-code"">my experience using Claude Code to build the portfolio</a>. For a deeper dive into EF Core patterns with PostgreSQL, see <a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL and EF Core: Real-World Best Practices</a>. And if you are deciding between Blazor and React for your next project, <a href=""/blog/blazor-vs-react-2025"">Blazor vs React in 2025</a> covers that comparison in detail.</p>", @"Hướng dẫn thực tế deploy Blazor InteractiveServer .NET 10 lên Fly.io với Neon PostgreSQL, từ Dockerfile, fly.toml đến CI/CD — bao gồm các gotcha về sticky sessions và cold start.", @"A practical guide to deploying .NET 10 Blazor InteractiveServer on Fly.io with Neon PostgreSQL — covering Dockerfile, fly.toml, CI/CD pipeline, and real-world gotchas like sticky sessions and cold starts.", @"dotnet, blazor, flyio, neon-postgresql, devops, cicd, efcore, csharp", @"Deploy .NET 10 Blazor lên Fly.io với Neon PostgreSQL (2026)", @"Deploy .NET 10 Blazor to Fly.io with Neon PostgreSQL (2026)", new DateTime(2026, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Content", "ContentEn", "Summary", "SummaryEn", "Tags", "Title", "TitleEn", "UpdatedAt" },
                values: new object[] { @"<p><img src=""https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=1200&auto=format&fit=crop&q=80"" alt=""Blazor vs React 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Đã có hàng trăm bài so sánh Blazor vs React trên mạng. Hầu hết đều được viết bởi những người chưa thực sự ship cả hai lên production, hoặc đã cũ từ thời .NET 6–7 khi Blazor còn khá thô. Tôi muốn viết bài này từ góc nhìn thực tế hơn: tôi đang dùng Blazor Web App .NET 10 để chạy <strong>VodonghaPersonal.id.vn</strong> — trang portfolio cá nhân của mình — và hằng ngày làm việc với cả React lẫn Blazor trong các dự án khác nhau.</p>

<p>Đây không phải bài ""cái nào tốt hơn"". Đây là bài giúp bạn đưa ra quyết định đúng cho dự án của mình.</p>

<h2>1. Bối cảnh năm 2026: Hai framework đang hội tụ</h2>

<p>Năm 2026, ranh giới giữa Blazor và React đang mờ dần theo một cách thú vị. Cả hai đang tiến về cùng một kiến trúc: <strong>render tĩnh phía server kết hợp với interactive components phía client</strong>.</p>

<ul>
  <li><strong>Blazor .NET 10</strong> (LTS, ra tháng 11/2025): Mô hình ""Blazor United"" từ .NET 8 — SSR + InteractiveServer + InteractiveAuto + WASM trong cùng một app — giờ đã trưởng thành và battle-tested. Có hơn 35 thay đổi đáng chú ý trong release này.</li>
  <li><strong>React 19.2</strong> (ra tháng 10/2025): Partial Pre-rendering (PPR) — render shell tĩnh từ CDN, resume với dynamic content sau. Component <code>&lt;Activity /&gt;</code> giúp ẩn/hiện subtree mà không unmount. React đang đi theo hướng rất giống Blazor's rendering model.</li>
</ul>

<p>Nhưng dù kiến trúc đang hội tụ, hai framework vẫn rất khác nhau về triết lý, hệ sinh thái, và use case phù hợp.</p>

<h2>2. Rendering modes: Blazor vẫn dẫn đầu về sự linh hoạt</h2>

<p>Đây là điểm Blazor .NET 10 thực sự tỏa sáng. Bạn có thể chọn render mode <em>trên từng component</em>, không phải cho toàn bộ app:</p>

<table style=""width:100%;border-collapse:collapse;margin:1rem 0"">
  <thead>
    <tr style=""background:#f3f4f6"">
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">Mode</th>
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">First Load</th>
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">Interactivity</th>
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">Phù hợp cho</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Static SSR</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Nhanh nhất</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Không (full page reload)</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">SEO pages, nội dung tĩnh</td>
    </tr>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb"">InteractiveServer</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Nhanh</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Thấp (SignalR)</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Intranet, real-time dashboard</td>
    </tr>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb""><strong>InteractiveAuto</strong></td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Nhanh</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Thấp → Zero (sau WASM load)</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">General purpose tốt nhất</td>
    </tr>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Interactive WASM</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Chậm (~1.5MB runtime)</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Zero sau khi load</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">CPU-intensive, offline support</td>
    </tr>
  </tbody>
</table>

<p>Trang <strong>VodonghaPersonal.id.vn</strong> của tôi dùng <strong>InteractiveServer</strong>. Lý do thực dụng: Fly.io shared-cpu-1x 256MB RAM, Neon PostgreSQL ở Singapore với suspend mode. InteractiveServer cho phép SignalR live chat hoạt động tốt, và với traffic thấp của một portfolio cá nhân, memory per circuit không phải vấn đề. Nhưng tôi phải thành thật: nếu traffic tăng đột biến, mỗi visitor sẽ giữ một WebSocket circuit trên server — đây là trade-off cần tính đến.</p>

<p>Một pain point thực tế: <strong>Neon suspend mode + SignalR circuit init = cold start stack khá đáng sợ</strong>. Lần đầu vào trang sau khi Neon ngủ, bạn sẽ cảm thấy 1–3 giây chờ đợi. Không lý tưởng nhưng chấp nhận được cho personal project.</p>

<h2>3. Những tính năng .NET 10 thực sự hữu ích</h2>

<p>Sau khi đọc release notes và thử nghiệm, đây là những thứ tôi thực sự quan tâm:</p>

<h3>PersistentState — giảm ~20 dòng boilerplate</h3>

<p>Trước .NET 10, để persist state qua prerendering, bạn phải viết kiểu này:</p>

<pre><code>// .NET 9 — boilerplate đau lòng
protected override async Task OnInitializedAsync()
{
    if (RendererInfo.IsInteractive)
    {
        // restore from PersistentComponentState
        if (_applicationState.TryTakeFromJson&lt;WeatherForecast[]&gt;(
            ""fetchdata"", out var restored))
        {
            _forecasts = restored;
            return;
        }
    }
    _forecasts = await ForecastService.GetForecastAsync();
    _applicationState.RegisterOnPersisting(() =>
    {
        _applicationState.PersistAsJson(""fetchdata"", _forecasts);
        return Task.CompletedTask;
    });
}</code></pre>

<p>Với .NET 10, chỉ cần:</p>

<pre><code>// .NET 10 — sạch hơn nhiều
[PersistentState]
private WeatherForecast[]? _forecasts;

protected override async Task OnInitializedAsync()
{
    _forecasts ??= await ForecastService.GetForecastAsync();
}</code></pre>

<p>Đây là quality-of-life improvement thực sự đáng kể.</p>

<h3>blazor.web.js giảm ~76% kích thước</h3>

<p>Framework assets giờ được serve dưới dạng fingerprinted compressed static assets, preloaded qua <code>Link</code> headers. Download bắt đầu trong khi trang vẫn đang render. Kết hợp với việc <code>blazor.boot.json</code> được inline vào <code>dotnet.js</code>, bạn tiết kiệm được một round-trip mạng.</p>

<h3>Passkey / WebAuthn built-in</h3>

<p>Blazor Web App template .NET 10 giờ có WebAuthn/FIDO2 passkey support out-of-the-box. Không cần thư viện thứ ba. Với security-conscious enterprise apps, đây là tin vui.</p>

<h3>JS Interop mới</h3>

<pre><code>// Constructor và property interop không cần workaround
var instance = await JSRuntime.InvokeConstructorAsync(""MyClass"", args);
var value = await JSRuntime.GetValueAsync&lt;string&gt;(jsRef, ""propertyName"");
await JSRuntime.SetValueAsync(jsRef, ""propertyName"", newValue);</code></pre>

<h2>4. React 19 — những điểm nổi bật thực sự</h2>

<p>React 19.2 (tháng 10/2025) là bản release đáng chú ý nhất trong series 19.x.</p>

<h3>Component &lt;Activity /&gt;</h3>

<p>Đây là tính năng tôi thấy thú vị nhất từ React gần đây. <code>&lt;Activity /&gt;</code> cho phép ẩn/hiện subtree mà không unmount component, giữ nguyên state (ví dụ: input field values khi back navigation). Nó còn pre-render hidden routes mà user có thể navigate đến tiếp theo.</p>

<p>Nếu bạn đã quen với Blazor component lifecycle, đây trông rất quen thuộc — Blazor làm điều tương tự từ lâu.</p>

<h3>React Compiler (opt-in)</h3>

<p>Automatic memoization. Early adopters báo cáo 25–40% ít re-renders hơn trong complex apps mà không cần thay đổi code. Quan trọng: đây <em>vẫn là opt-in</em>, không phải default. Bạn vẫn phải manage memoization thủ công nếu chưa enable.</p>

<h3>useEffectEvent</h3>

<p>Fix cho một trong những lỗi được Google nhiều nhất của React — dependency array bugs trong <code>useEffect</code>. <code>useEffectEvent</code> tách event logic ra khỏi Effect dependencies. Cần <code>eslint-plugin-react-hooks</code> v6.</p>

<h3>Partial Pre-rendering (PPR)</h3>

<p>Pre-render static shell từ CDN, resume với dynamic content sau. Đây là React đang tiến về phía kiến trúc giống Blazor's SSR+interactive hybrid. Ranh giới thực sự đang mờ dần.</p>

<h2>5. Hệ sinh thái: React vẫn cách biệt xa</h2>

<p>Phải thành thật: đây là điểm yếu lớn nhất của Blazor năm 2026.</p>

<ul>
  <li><strong>React:</strong> ~40% developers dùng (Stack Overflow 2024/2025), 20M+ npm downloads/tuần, được dùng bởi Facebook, Instagram, Netflix. Hàng nghìn thư viện UI: D3.js, Leaflet, Stripe Elements, Mapbox — gần như không có C# equivalent.</li>
  <li><strong>Blazor:</strong> ~35,000+ live sites theo BuiltWith cuối 2024 (tăng ~3x so với cuối 2023). Khoảng 43% .NET developers dùng Blazor trong production (JetBrains 2025). Hệ sinh thái nhỏ hơn React khoảng 10–20 lần về số lượng thư viện, Stack Overflow answers, và tutorials.</li>
</ul>

<p>Nếu bạn cần tích hợp một thư viện visualisation, map, payment widget bất kỳ — React gần như luôn có npm package sẵn. Với Blazor, bạn thường phải wrap JS library qua interop, hoặc chờ community làm Blazor wrapper.</p>

<p>Tuy nhiên, hệ sinh thái .NET là một lợi thế riêng: EF Core, ASP.NET Identity, SignalR, gRPC, tất cả first-class và không cần thêm gì. Nếu app của bạn chủ yếu là CRUD + business logic, không cần exotic JS widgets, Blazor ecosystem hoàn toàn đủ dùng.</p>

<h2>6. Developer experience: C# end-to-end vs JavaScript everywhere</h2>

<p>Đây là điểm tôi thấy Blazor thực sự tỏa sáng với .NET developers.</p>

<p>Trong một Blazor app, bạn có thể share:</p>

<ul>
  <li><strong>Models/DTOs</strong> — dùng cùng class ở frontend và backend</li>
  <li><strong>Validation logic</strong> — <code>DataAnnotations</code> hoặc FluentValidation chạy ở cả client và server</li>
  <li><strong>Business logic</strong> — không cần viết lại logic ở TypeScript</li>
  <li><strong>Enums và constants</strong> — không cần sync giữa hai codebases</li>
</ul>

<pre><code>// Shared model — dùng cho cả API response lẫn Blazor component
public record BlogPostDto(
    int Id,
    string Title,
    string Slug,
    DateTime PublishedAt,
    bool IsPublished
);

// Validation chạy ở cả server và client
public class BlogPostValidator : AbstractValidator&lt;BlogPostDto&gt;
{
    public BlogPostValidator()
    {
        RuleFor(x =&gt; x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x =&gt; x.Slug).Matches(@""^[a-z0-9-]+$"");
    }
}</code></pre>

<p>Với React + TypeScript, bạn vẫn phải maintain hai separate type definitions, hai separate validation schemas. Tools như tRPC hay OpenAPI codegen giúp, nhưng vẫn có overhead.</p>

<p>Tôi build VodonghaPersonal.id.vn <a href=""/blog/building-with-ai-experience-with-claude-code"">hoàn toàn với Claude Code</a> — và một trong những điều tôi thấy thú vị là AI assist viết code Blazor/C# khá tốt, phần lớn nhờ type safety và shared models giúp AI ""hiểu"" context tốt hơn.</p>

<h2>7. Performance thực tế: không có winner tuyệt đối</h2>

<p>Một số điểm thực tế về performance:</p>

<h3>Blazor WASM cold start</h3>
<p>Vẫn là pain point dù đã cải thiện nhiều trong .NET 10. .NET runtime (~1.5MB) cần download lần đầu. Với preloading và fingerprinted caching, lần sau sẽ nhanh hơn nhiều. AOT compilation giảm WASM payload khoảng 40% theo một số nguồn. Nhưng lần đầu vào app trên mạng chậm vẫn sẽ có độ trễ rõ ràng.</p>

<h3>InteractiveServer scalability</h3>
<p>Mỗi active user giữ một server-side SignalR circuit. Với hosting resources hạn chế (như 256MB Fly.io của tôi), đây là constraint thực sự. WASM thực ra scale tốt hơn ở server side vì client làm hết rendering work — nhưng lại có cold start penalty.</p>

<h3>React bundle size</h3>
<p>React vẫn ship JavaScript cho mọi client. Không có native WASM execution path. Nhưng với React Compiler và PPR, overall perceived performance có thể rất tốt, đặc biệt khi kết hợp với CDN-served static shell.</p>

<p>Tôi đã viết chi tiết hơn về kinh nghiệm <a href=""/blog/deploy-dotnet-fly-io-2025"">deploy .NET lên Fly.io</a> nếu bạn muốn xem setup thực tế.</p>

<h2>8. Khi nào chọn Blazor, khi nào chọn React</h2>

<p>Đây là framework quyết định mà tôi sẽ dùng cho các dự án:</p>

<h3>Chọn Blazor khi:</h3>
<ul>
  <li>Team là .NET/C# developers — zero context switching, shared models, shared validation</li>
  <li>Enterprise internal tools: line-of-business apps, intranet portals, admin dashboards</li>
  <li>Real-time requirements (SignalR first-class, built-in)</li>
  <li>Tight integration với .NET APIs, EF Core, ASP.NET Identity</li>
  <li>CPU-intensive client logic phù hợp với WASM (financial calculations, document processing)</li>
  <li>Muốn một ngôn ngữ cho toàn bộ stack (C#, không phải TypeScript)</li>
</ul>

<h3>Chọn React khi:</h3>
<ul>
  <li>Public-facing, SEO-critical content sites</li>
  <li>Team có JavaScript expertise hoặc cần hire JavaScript developers</li>
  <li>Cần ecosystem breadth: D3.js, Leaflet, Stripe Elements, Mapbox, hàng nghìn npm packages không có C# equivalent</li>
  <li>Mobile-first PWA với offline requirements</li>
  <li>Micro-frontend architecture với nhiều teams</li>
  <li>Maximize hiring pool và community support</li>
</ul>

<p><strong>Câu trả lời thực tế năm 2026:</strong> Ranh giới đã mờ đi nhiều. Blazor's hybrid rendering và React's PPR đang hội tụ về cùng một ý tưởng kiến trúc. Differentiator thực sự không phải framework capability — mà là <em>team language fluency</em> và <em>ecosystem needs</em> của dự án cụ thể.</p>

<h2>FAQ</h2>

<h3>Blazor có thể thay thế React hoàn toàn không?</h3>
<p>Về mặt kỹ thuật, Blazor .NET 10 có thể làm được hầu hết những gì React làm — SSR, client-side rendering, hybrid. Nhưng ""thay thế hoàn toàn"" là câu hỏi sai. Bạn nên chọn tool phù hợp với team và use case. Nếu team của bạn là C# developers và bạn build enterprise apps, Blazor có thể hoàn toàn là lựa chọn tốt hơn. Nếu bạn cần một ecosystem với hàng nghìn ready-to-use JS libraries và hire frontend specialists, React vẫn là lựa chọn mạnh hơn.</p>

<h3>Blazor WASM có còn chậm không?</h3>
<p>Cải thiện đáng kể trong .NET 10 nhưng cold start vẫn là điểm cần lưu ý. blazor.web.js giảm ~76% kích thước, assets preloaded, boot.json inline vào dotnet.js. Nhưng .NET runtime vẫn cần download lần đầu (~1.5MB). Với InteractiveAuto mode (khuyến nghị cho general-purpose apps), bạn bắt đầu bằng InteractiveServer (nhanh) và WASM load ngầm ở background — lần sau sẽ dùng WASM không cần server. Đây là trade-off tốt nhất hiện tại.</p>

<h3>Học Blazor có tốn nhiều thời gian không nếu đã biết React?</h3>
<p>Khá nhanh nếu bạn đã có .NET background. Component model tương tự, lifecycle hooks tương đương. Khác biệt chính là: không có JSX (dùng Razor syntax), state management khác (no Redux — bạn dùng services với DI), và cần hiểu render modes. Nếu bạn là React developer nhưng không có .NET background, learning curve sẽ dốc hơn vì bạn cần học cả C# ecosystem.</p>

<h3>EF Core với Blazor Server có vấn đề gì không?</h3>
<p>Có một gotcha quan trọng: với Blazor Server (InteractiveServer), bạn không nên inject <code>DbContext</code> trực tiếp qua DI vì SignalR's single-threaded circuit có thể gây concurrency issues. Pattern đúng là dùng <code>IDbContextFactory&lt;T&gt;</code> và tạo context mới cho mỗi operation:</p>
<pre><code>// Đúng với Blazor InteractiveServer
@inject IDbContextFactory&lt;AppDbContext&gt; DbContextFactory

private async Task LoadDataAsync()
{
    await using var db = await DbContextFactory.CreateDbContextAsync();
    _posts = await db.BlogPosts.Where(p =&gt; p.IsPublished).ToListAsync();
}</code></pre>
<p>Tôi có bài chi tiết hơn về <a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL và EF Core best practices</a> nếu bạn muốn đọc thêm.</p>

<h3>2026 là lúc thích hợp để bắt đầu với Blazor không?</h3>
<p>Thời điểm tốt nhất. .NET 10 là LTS release, Blazor United model đã mature, Microsoft rõ ràng đang đầu tư mạnh vào Blazor (35+ changes trong một release). Nếu bạn là .NET developer và chưa thử Blazor, đây là lúc ecosystem đã đủ stable để build production apps một cách tự tin. Không còn cảm giác ""đang dùng beta"" như thời .NET 5–6 nữa.</p>

<h2>Kết luận</h2>

<p>Tôi bắt đầu viết bài này định trả lời câu hỏi ""Blazor hay React tốt hơn?"" — nhưng càng viết càng nhận ra đó là câu hỏi sai.</p>

<p>Năm 2026, cả hai framework đều rất capable. React 19.2 mang PPR và <code>&lt;Activity /&gt;</code> tiến gần hơn đến Blazor's rendering model. Blazor .NET 10 mang <code>[PersistentState]</code>, passkey support, và WASM performance improvements tiến gần hơn đến developer experience mà React đã có từ lâu.</p>

<p>Câu trả lời thực sự phụ thuộc vào: <strong>team của bạn giỏi gì, và dự án của bạn cần gì từ ecosystem</strong>.</p>

<p>Cá nhân tôi? Với enterprise .NET projects ở công ty, Blazor là lựa chọn tự nhiên và productive hơn nhiều. Với VodonghaPersonal.id.vn, tôi chọn Blazor vì muốn trải nghiệm thực tế và vì tôi thích C# hơn TypeScript. Đó là lý do hoàn toàn hợp lệ.</p>

<p>Nếu bạn quan tâm đến cách tôi build site này, đọc thêm bài <a href=""/blog/building-with-ai-experience-with-claude-code"">kinh nghiệm build với Claude Code</a> hoặc bài về <a href=""/blog/vibe-coding-la-gi"">vibe coding là gì</a>. Còn nếu bạn muốn biết AI đang thay đổi workflow của developers như thế nào, xem bài <a href=""/blog/ai-skills-for-developers"">AI skills for developers</a>.</p>", @"<p><img src=""https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=1200&auto=format&fit=crop&q=80"" alt=""Blazor vs React 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>There are hundreds of Blazor vs React comparisons on the internet. Most are written by people who have never shipped both to production, or are outdated from the .NET 6–7 era when Blazor was genuinely rough around the edges. I want to write this from a more grounded perspective: I run <strong>VodonghaPersonal.id.vn</strong> on Blazor Web App .NET 10, and I work with both React and Blazor across different projects every day.</p>

<p>This is not a ""which one is better"" post. It is a post to help you make the right decision for your specific project.</p>

<h2>1. The 2026 Landscape: Two Frameworks Converging</h2>

<p>In 2026, the boundary between Blazor and React is blurring in an interesting way. Both are converging toward the same architecture: <strong>static server rendering combined with interactive client-side components</strong>.</p>

<ul>
  <li><strong>Blazor .NET 10</strong> (LTS, released November 2025): The ""Blazor United"" model from .NET 8 — SSR + InteractiveServer + InteractiveAuto + WASM all in one app — is now mature and battle-tested. This release has 35+ documented changes.</li>
  <li><strong>React 19.2</strong> (released October 2025): Partial Pre-rendering (PPR) pre-renders a static shell from a CDN and resumes with dynamic content. The new <code>&lt;Activity /&gt;</code> component hides and shows subtrees without unmounting. React is moving toward an architecture that looks remarkably like Blazor's rendering model.</li>
</ul>

<p>But even as the architectures converge, the two frameworks remain fundamentally different in philosophy, ecosystem, and appropriate use cases.</p>

<h2>2. Rendering Modes: Blazor Still Leads on Flexibility</h2>

<p>This is where Blazor .NET 10 genuinely shines. You can choose a render mode <em>per component</em>, not just per app:</p>

<table style=""width:100%;border-collapse:collapse;margin:1rem 0"">
  <thead>
    <tr style=""background:#f3f4f6"">
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">Mode</th>
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">First Load</th>
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">Interactivity Latency</th>
      <th style=""padding:8px;text-align:left;border:1px solid #e5e7eb"">Best For</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Static SSR</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Fastest</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">None (full page reload)</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">SEO pages, read-only content</td>
    </tr>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb"">InteractiveServer</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Fast</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Low on good network</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Intranet, real-time dashboards</td>
    </tr>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb""><strong>InteractiveAuto</strong></td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Fast</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Low → Zero (after WASM loads)</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Best general-purpose choice</td>
    </tr>
    <tr>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Interactive WASM</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Slow (~1.5MB runtime)</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">Zero after load</td>
      <td style=""padding:8px;border:1px solid #e5e7eb"">CPU-intensive, offline support</td>
    </tr>
  </tbody>
</table>

<p>My personal site <strong>VodonghaPersonal.id.vn</strong> uses <strong>InteractiveServer</strong>. The practical reason: Fly.io shared-cpu-1x with 256MB RAM, Neon PostgreSQL in Singapore running in suspend mode. InteractiveServer makes SignalR-based live chat work cleanly, and with the low traffic of a personal portfolio, memory per circuit is not a problem. But I have to be honest: if traffic spikes, each visitor holds an open WebSocket circuit on the server. That is a real constraint for resource-limited hosting.</p>

<p>A real-world pain point I want to call out: <strong>Neon suspend mode + SignalR circuit init = a noticeable cold start stack</strong>. A visitor who hits the site after Neon has gone to sleep will wait 1–3 seconds. Not ideal, but acceptable for a personal project.</p>

<h2>3. .NET 10 Blazor Features That Actually Matter</h2>

<p>After reading the release notes and testing them hands-on, these are the changes I actually care about:</p>

<h3>PersistentState — ~20 lines of boilerplate gone</h3>

<p>Before .NET 10, persisting state across prerendering required this kind of ceremony:</p>

<pre><code>// .NET 9 — painful boilerplate
protected override async Task OnInitializedAsync()
{
    if (RendererInfo.IsInteractive)
    {
        if (_applicationState.TryTakeFromJson&lt;WeatherForecast[]&gt;(
            ""fetchdata"", out var restored))
        {
            _forecasts = restored;
            return;
        }
    }
    _forecasts = await ForecastService.GetForecastAsync();
    _applicationState.RegisterOnPersisting(() =>
    {
        _applicationState.PersistAsJson(""fetchdata"", _forecasts);
        return Task.CompletedTask;
    });
}</code></pre>

<p>With .NET 10:</p>

<pre><code>// .NET 10 — dramatically cleaner
[PersistentState]
private WeatherForecast[]? _forecasts;

protected override async Task OnInitializedAsync()
{
    _forecasts ??= await ForecastService.GetForecastAsync();
}</code></pre>

<p>This is a genuine quality-of-life improvement, not marketing.</p>

<h3>blazor.web.js at ~76% reduced size</h3>

<p>Framework assets are now served as fingerprinted, compressed static web assets and preloaded via <code>Link</code> headers. The download starts while the page is still rendering. Combined with <code>blazor.boot.json</code> being inlined into <code>dotnet.js</code>, you save a full network round-trip on first load.</p>

<h3>Passkey / WebAuthn built in</h3>

<p>The Blazor Web App template in .NET 10 ships with WebAuthn/FIDO2 passkey support out of the box. No third-party library required. For security-conscious enterprise applications, this is significant.</p>

<h3>New JS Interop APIs</h3>

<pre><code>// Constructor and property interop without workarounds
var instance = await JSRuntime.InvokeConstructorAsync(""MyClass"", args);
var value = await JSRuntime.GetValueAsync&lt;string&gt;(jsRef, ""propertyName"");
await JSRuntime.SetValueAsync(jsRef, ""propertyName"", newValue);</code></pre>

<h2>4. React 19 — What Actually Stands Out</h2>

<p>React 19.2 (October 2025) is the most notable release in the 19.x series.</p>

<h3>The &lt;Activity /&gt; Component</h3>

<p>This is the React feature I find most interesting from recent releases. <code>&lt;Activity /&gt;</code> lets you hide and show subtrees without unmounting, preserving state — for example, input field values survive back-navigation. It also pre-renders hidden routes the user is likely to navigate to next.</p>

<p>If you are familiar with Blazor's component lifecycle, this will look very familiar. Blazor has had equivalent behavior for some time.</p>

<h3>React Compiler (opt-in)</h3>

<p>Automatic memoization. Early adopters report 25–40% fewer re-renders in complex apps without code changes. Important caveat: this is still opt-in, not the default. If you have not enabled it, you still manage memoization manually.</p>

<h3>useEffectEvent</h3>

<p>A fix for one of the most Googled React issues — dependency array bugs in <code>useEffect</code>. <code>useEffectEvent</code> separates event logic from Effect dependencies. Requires <code>eslint-plugin-react-hooks</code> v6.</p>

<h3>Partial Pre-rendering (PPR)</h3>

<p>Pre-renders a static shell from a CDN, resumes with dynamic content later. This is React converging toward Blazor's SSR + interactive hybrid model. The boundary between the two frameworks is genuinely blurring at the architectural level.</p>

<h2>5. Ecosystem: React Is Still Significantly Ahead</h2>

<p>To be direct: this remains Blazor's most significant weakness in 2026.</p>

<ul>
  <li><strong>React:</strong> Used by roughly 40% of professional developers (Stack Overflow 2024/2025), 20M+ npm downloads per week, deployed by Facebook, Instagram, Netflix. Thousands of ecosystem libraries: D3.js, Leaflet, Stripe Elements, Mapbox — most with no C# equivalent.</li>
  <li><strong>Blazor:</strong> ~35,000+ live sites per BuiltWith at end of 2024 (roughly 3x growth from late 2023). Approximately 43% of .NET developers use Blazor in production (JetBrains 2025). The ecosystem is 10–20x smaller than React's in terms of available libraries, Stack Overflow answers, and tutorials.</li>
</ul>

<p>If you need to integrate any visualization library, mapping widget, or payment UI component — React almost always has an npm package ready. With Blazor, you typically need to wrap the JS library via interop or wait for the community to build a Blazor wrapper.</p>

<p>That said, the .NET ecosystem is its own advantage: EF Core, ASP.NET Identity, SignalR, gRPC — all first-class, all available without additional packages. If your application is primarily CRUD and business logic without exotic JS widgets, the Blazor ecosystem is entirely sufficient.</p>

<h2>6. Developer Experience: C# End-to-End vs JavaScript Everywhere</h2>

<p>This is where Blazor genuinely shines for .NET developers.</p>

<p>In a Blazor application, you can share across the full stack:</p>

<ul>
  <li><strong>Models and DTOs</strong> — the same class used in both frontend and backend</li>
  <li><strong>Validation logic</strong> — DataAnnotations or FluentValidation runs on both client and server</li>
  <li><strong>Business logic</strong> — no need to rewrite logic in TypeScript</li>
  <li><strong>Enums and constants</strong> — no sync required between two codebases</li>
</ul>

<pre><code>// Shared model — used in both API responses and Blazor components
public record BlogPostDto(
    int Id,
    string Title,
    string Slug,
    DateTime PublishedAt,
    bool IsPublished
);

// Validation runs on both server and client
public class BlogPostValidator : AbstractValidator&lt;BlogPostDto&gt;
{
    public BlogPostValidator()
    {
        RuleFor(x =&gt; x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x =&gt; x.Slug).Matches(@""^[a-z0-9-]+$"");
    }
}</code></pre>

<p>With React and TypeScript, you still maintain two separate type definitions and two separate validation schemas. Tools like tRPC and OpenAPI codegen help, but there is always overhead.</p>

<p>I built VodonghaPersonal.id.vn <a href=""/blog/building-with-ai-experience-with-claude-code"">entirely with Claude Code</a>, and one thing I noticed is that AI assistance writes Blazor/C# code quite effectively — largely because type safety and shared models give the model cleaner context to work with.</p>

<h2>7. Real-World Performance: No Absolute Winner</h2>

<h3>Blazor WASM cold start</h3>
<p>Still a real concern, though significantly improved in .NET 10. The .NET runtime (~1.5MB) must download on first visit. With preloading and fingerprinted caching, subsequent visits are fast. AOT compilation reportedly reduces WASM payload by up to 40%. But on a slow connection, first-visit latency is noticeable. InteractiveAuto mode is currently the best trade-off: start with InteractiveServer (fast), load WASM silently in the background, use WASM from the next visit onward.</p>

<h3>InteractiveServer scalability</h3>
<p>Each active user holds a server-side SignalR circuit. With constrained hosting resources — like my 256MB Fly.io instance — this is a real constraint. WASM actually scales better server-side because the client handles all rendering work. But WASM has the cold start penalty. The right choice depends on your traffic pattern and hosting budget.</p>

<h3>React bundle size</h3>
<p>React still ships JavaScript to every client. There is no native WASM execution path. But with React Compiler reducing unnecessary re-renders and PPR serving a static shell from a CDN, perceived performance can be excellent — particularly for content-heavy public sites where the static shell renders instantly.</p>

<p>I wrote in more detail about the practical experience of <a href=""/blog/deploy-dotnet-fly-io-2025"">deploying .NET to Fly.io</a> if you want to see the real setup.</p>

<h2>8. When to Choose Each: A Decision Framework</h2>

<h3>Choose Blazor when:</h3>
<ul>
  <li>Your team is .NET/C# — zero context switching, shared models, shared validation logic</li>
  <li>Building enterprise internal tools: line-of-business apps, intranet portals, admin dashboards</li>
  <li>Real-time requirements (SignalR is first-class and built-in)</li>
  <li>Tight integration with .NET APIs, EF Core, ASP.NET Identity</li>
  <li>CPU-intensive client logic suited to WASM (financial calculations, document processing)</li>
  <li>You want one language for the full stack — C#, not TypeScript</li>
</ul>

<h3>Choose React when:</h3>
<ul>
  <li>Public-facing, SEO-critical content sites</li>
  <li>Teams with existing JavaScript expertise or hiring JavaScript developers</li>
  <li>Ecosystem breadth matters: D3.js, Leaflet, Stripe Elements, Mapbox, thousands of npm packages with no C# equivalent</li>
  <li>Mobile-first progressive web apps with offline requirements</li>
  <li>Micro-frontend architecture with multiple teams owning independent UI slices</li>
  <li>Maximizing hiring pool and community support</li>
</ul>

<p><strong>The honest 2026 answer:</strong> The lines have blurred considerably. Blazor's hybrid rendering and React's PPR are converging on the same architectural idea. The real differentiator is not framework capability — it is <em>team language fluency</em> and the <em>ecosystem requirements</em> of your specific project.</p>

<h2>FAQ</h2>

<h3>Can Blazor fully replace React in 2026?</h3>
<p>Technically, Blazor .NET 10 can do most of what React does — SSR, client-side rendering, hybrid rendering. But ""fully replace"" is the wrong question. You should choose the tool that fits your team and use case. If your team consists of C# developers building enterprise applications, Blazor may well be the more productive choice. If you need an ecosystem with thousands of ready-to-use JS libraries and plan to hire frontend specialists, React remains the stronger option. The capability gap has narrowed significantly; the ecosystem and talent-pool gap has not.</p>

<h3>Is Blazor WASM still slow in 2026?</h3>
<p>Substantially improved but cold start is still a real consideration. In .NET 10, <code>blazor.web.js</code> is ~76% smaller, assets are preloaded via Link headers, and <code>blazor.boot.json</code> is inlined into <code>dotnet.js</code> — saving a network round-trip. AOT compilation reduces WASM payload further. But the .NET runtime still needs to download on first visit (~1.5MB). InteractiveAuto mode is the recommended pattern for general-purpose apps: start fast with InteractiveServer, load WASM silently in the background, and subsequent visits run entirely client-side with zero server latency.</p>

<h3>How steep is the Blazor learning curve for a React developer?</h3>
<p>Quite manageable if you already have a .NET background. The component model is similar, lifecycle hooks have direct equivalents. The main differences are: no JSX (Razor syntax instead), different state management patterns (dependency injection rather than Redux/Zustand), and understanding render modes. If you are a React developer without .NET experience, the learning curve is steeper because you are learning C# and the .NET ecosystem simultaneously — not just the framework.</p>

<h3>What is the correct pattern for EF Core with Blazor InteractiveServer?</h3>
<p>There is an important gotcha: you should not inject <code>DbContext</code> directly via DI in Blazor InteractiveServer components. SignalR's single-threaded circuit can cause concurrency issues if the same DbContext instance is accessed across multiple asynchronous operations. The correct pattern is to use <code>IDbContextFactory&lt;T&gt;</code> and create a fresh context for each operation:</p>
<pre><code>// Correct pattern for Blazor InteractiveServer
@inject IDbContextFactory&lt;AppDbContext&gt; DbContextFactory

private async Task LoadDataAsync()
{
    await using var db = await DbContextFactory.CreateDbContextAsync();
    _posts = await db.BlogPosts
        .Where(p =&gt; p.IsPublished)
        .OrderByDescending(p =&gt; p.PublishedAt)
        .ToListAsync();
}</code></pre>
<p>I cover this and related patterns in more depth in my post on <a href=""/blog/postgresql-entity-framework-core-best-practices"">PostgreSQL and EF Core best practices</a>.</p>

<h3>Is 2026 a good time to start with Blazor for a new project?</h3>
<p>.NET 10 is an LTS release, and this is the most stable and capable Blazor has ever been. The Blazor United hybrid rendering model has been shipping in production since .NET 8 and is now genuinely mature. Microsoft's investment in Blazor is clear from the scale of improvements in .NET 10. If you are a .NET developer who has been waiting for Blazor to stabilize before committing to it, the answer is yes — this is no longer an ""early adopter"" technology. That said, set realistic expectations around ecosystem breadth and be prepared to wrap JS libraries via interop when you need something the .NET ecosystem does not yet cover natively.</p>

<h2>Conclusion</h2>

<p>I started writing this post intending to answer ""which is better — Blazor or React?"" The more I wrote, the more I realized that is the wrong question.</p>

<p>In 2026, both frameworks are genuinely capable. React 19.2 brings PPR and <code>&lt;Activity /&gt;</code> closer to Blazor's rendering model. Blazor .NET 10 brings <code>[PersistentState]</code>, passkey support, and WASM performance improvements closer to the developer experience React has long offered. The gap in architectural capability has narrowed to the point where either can serve most use cases well.</p>

<p>The real answer depends on: <strong>what your team is fluent in, and what your project needs from the ecosystem</strong>.</p>

<p>Personally — for enterprise .NET projects, Blazor is the natural and more productive choice for me. For VodonghaPersonal.id.vn itself, I chose Blazor because I wanted real-world experience with it and because I prefer C# over TypeScript. Those are entirely valid reasons.</p>

<p>If you are curious about how I built this site, the post on <a href=""/blog/building-with-ai-experience-with-claude-code"">building with Claude Code</a> covers the process in detail. If you are thinking about how AI is changing developer workflows more broadly, take a look at <a href=""/blog/ai-skills-for-developers"">AI skills for developers</a>. And if you want to explore the vibe coding approach I used, <a href=""/blog/vibe-coding-la-gi"">this post</a> covers it from first principles.</p>", @"So sánh thực tế Blazor .NET 10 và React 19 năm 2026: hiệu năng, rendering modes, hệ sinh thái, và khi nào nên chọn cái nào — góc nhìn từ developer đã dùng cả hai.", @"A practical 2026 comparison of Blazor .NET 10 and React 19: rendering modes, performance, ecosystem, and an honest decision framework from a developer who has shipped both.", @"Blazor, React, .NET 10, Web Development, Frontend, C#, JavaScript, Full-Stack", @"Blazor vs React năm 2026: So sánh thực tế từ developer", @"Blazor vs React in 2026: A Practical Comparison from a Full-Stack Developer", new DateTime(2026, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Content", "ContentEn", "Summary", "SummaryEn", "Tags", "Title", "TitleEn", "UpdatedAt" },
                values: new object[] { @"<p><img src=""https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=1200&auto=format&fit=crop&q=80"" alt=""PostgreSQL + EF Core Best Practices 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>Khi xây dựng <strong>VodonghaPersonal.id.vn</strong> — một Blazor Web App chạy trên Fly.io với PostgreSQL qua Neon — mình đã phải đối mặt với khá nhiều quyết định kiến trúc liên quan đến EF Core và PostgreSQL. Đây là bài tổng hợp những gì mình học được, kết hợp với những thay đổi lớn trong năm 2025-2026: EF Core 10 ra mắt với tư cách LTS mới, PostgreSQL 18 mang lại async I/O, và Neon tiếp tục cải thiện cold start. Bài này viết từ góc nhìn thực tế của một developer đang chạy production trên stack này.</p>

<h2>1. Bức tranh phiên bản năm 2026</h2>

<p>Trước khi đi vào chi tiết, cần nắm rõ landscape hiện tại:</p>

<ul>
  <li><strong>.NET 10</strong> — Released tháng 11/2025, LTS, hỗ trợ đến tháng 11/2028.</li>
  <li><strong>EF Core 10</strong> — Released tháng 11/2025 cùng .NET 10. Đây là LTS mới nhất và là target chính cho các dự án mới.</li>
  <li><strong>EF Core 9</strong> — Released tháng 11/2024. Vẫn được support đến tháng 11/2026. Nếu dự án bạn đang chạy ổn định trên EF Core 9, không cần vội upgrade — nhưng nên lên kế hoạch.</li>
  <li><strong>PostgreSQL 17</strong> — Stable production từ tháng 9/2024.</li>
  <li><strong>PostgreSQL 18</strong> — Released tháng 9/2025. Thay đổi kiến trúc lớn nhất trong nhiều năm với Asynchronous I/O.</li>
</ul>

<p>Cá nhân mình đang chạy EF Core 9 + PostgreSQL (qua Neon) và đang lên kế hoạch upgrade lên EF Core 10. Con đường LTS-to-LTS này khá sạch và ít breaking change.</p>

<h2>2. EF Core 9 và EF Core 10 — Những tính năng đáng chú ý nhất</h2>

<h3>EF Core 9: Những gì bạn đã có</h3>

<p><strong>Native AOT compilation</strong> là tính năng mình hứng thú nhất — startup nhanh hơn đáng kể, memory footprint giảm. Quan trọng với Fly.io khi app bị scale xuống 0 và phải cold start.</p>

<p><strong>Migration concurrency lock</strong> giải quyết vấn đề race condition khi nhiều instance khởi động đồng thời và cùng cố chạy migrations. Trước đây phải tự handle, giờ EF Core lo.</p>

<p><strong>Primitive collections trong LINQ</strong> — giờ có thể query thẳng vào <code>List&lt;string&gt;</code> hoặc <code>List&lt;int&gt;</code> mà không cần join table phụ:</p>

<pre><code>// EF Core 9 - query trực tiếp vào primitive collection
var posts = await db.BlogPosts
    .Where(p => p.Tags.Contains(""postgresql""))
    .ToListAsync();</code></pre>

<h3>EF Core 10: Nâng cấp đáng làm nhất</h3>

<p><strong>LeftJoin / RightJoin operators</strong> — finally được support first-class. Trước đây phải dùng workaround với <code>DefaultIfEmpty()</code> khá xấu:</p>

<pre><code>// EF Core 10 - LeftJoin clean hơn nhiều
var result = await db.Users
    .LeftJoin(db.Orders,
        user => user.Id,
        order => order.UserId,
        (user, order) => new { user.Name, OrderId = order != null ? order.Id : (int?)null })
    .ToListAsync();</code></pre>

<p><strong>Named query filters</strong> là tính năng mình mong chờ nhất. EF Core 8/9 chỉ cho một filter per entity type — cực kỳ hạn chế khi bạn cần cả soft-delete lẫn tenant isolation:</p>

<pre><code>// EF Core 10 - multiple named filters
modelBuilder.Entity&lt;BlogPost&gt;()
    .HasQueryFilter(""softDelete"", p => !p.IsDeleted)
    .HasQueryFilter(""published"", p => p.PublishedAt != null);

// Disable một filter cụ thể per query
var drafts = await db.BlogPosts
    .IgnoreQueryFilters(""published"")
    .ToListAsync();</code></pre>

<p><strong>Vector data type support</strong> — native integration với Azure SQL cho AI/semantic search workloads. Nếu bạn đang build RAG hoặc embedding search, đây là tin rất tốt.</p>

<h2>3. IDbContextFactory — Không còn là tùy chọn trong Blazor Server</h2>

<p>Đây là phần quan trọng nhất với bất kỳ ai dùng Blazor Server (InteractiveServer render mode) như VodonghaPersonal.id.vn.</p>

<p>Vấn đề cốt lõi: với <code>AddDbContext&lt;T&gt;()</code> thông thường, scoped <code>DbContext</code> sống suốt lifetime của Blazor circuit — có thể là hàng giờ. Điều này dẫn đến:</p>

<ul>
  <li>Đọc dữ liệu cũ (stale data) vì context cache entity</li>
  <li>Thread-safety violations khi hai async operations chạy đồng thời trên cùng một context</li>
  <li>Lỗi kinh điển: <em>""A second operation was started on this context instance before a previous asynchronous operation completed""</em></li>
</ul>

<p>Microsoft giờ recommend <code>IDbContextFactory</code> làm default approach trong official docs cho Blazor Server. Cách setup:</p>

<pre><code>// Program.cs
builder.Services.AddDbContextFactory&lt;AppDbContext&gt;(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(""DefaultConnection"")));

// Component sử dụng - mỗi operation tạo context mới, short-lived
@inject IDbContextFactory&lt;AppDbContext&gt; DbFactory

private async Task LoadDataAsync()
{
    await using var db = DbFactory.CreateDbContext();
    _posts = await db.BlogPosts
        .Where(p => p.PublishedAt != null)
        .OrderByDescending(p => p.PublishedAt)
        .Select(p => new PostSummaryDto
        {
            Id = p.Id,
            Title = p.Title,
            PublishedAt = p.PublishedAt
        })
        .ToListAsync();
}</code></pre>

<p>Với Neon serverless, pattern này còn có thêm lợi ích: context được tạo và dispose ngay sau khi dùng xong, không giữ connection mở liên tục, giúp Neon compute có thể suspend đúng lúc.</p>

<p><strong>Rule tuyệt đối:</strong> Không inject <code>DbContext</code> trực tiếp vào singleton services. Luôn dùng <code>IDbContextFactory&lt;T&gt;</code> ở đó.</p>

<h2>4. N+1 Query — Vẫn là kẻ thù số một năm 2026</h2>

<p>N+1 không phải vấn đề mới nhưng vẫn là nguyên nhân phổ biến nhất của EF Core performance issues. Mình đã gặp nó khi build trang blog của VodonghaPersonal.id.vn.</p>

<h3>Eager Loading với Include()</h3>

<pre><code>// BAD - N+1: 1 query lấy posts, N queries lấy từng author
var posts = await db.BlogPosts.ToListAsync();
foreach (var post in posts)
{
    Console.WriteLine(post.Author.Name); // lazy load từng cái
}

// GOOD - Eager loading
var posts = await db.BlogPosts
    .Include(p => p.Author)
    .ToListAsync();</code></pre>

<h3>AsSplitQuery() khi Include nhiều collections</h3>

<p>Khi Include nhiều collection navigations, EF Core sinh ra một JOIN khổng lồ dẫn đến cartesian explosion. <code>AsSplitQuery()</code> chia thành N queries sạch:</p>

<pre><code>var posts = await db.BlogPosts
    .Include(p => p.Tags)
    .Include(p => p.Comments)
    .AsSplitQuery() // Sinh 3 queries thay vì 1 JOIN phức tạp
    .ToListAsync();</code></pre>

<h3>Projection với Select() — Cách tiếp cận tốt nhất</h3>

<p>Đây là discipline shift quan trọng nhất: thay vì Include-first, hãy tư duy Projection-first. Chỉ lấy đúng những gì view cần:</p>

<pre><code>// BEST - Projection, chỉ lấy columns cần thiết
var postDtos = await db.BlogPosts
    .Where(p => p.PublishedAt != null)
    .Select(p => new PostListDto
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        AuthorName = p.Author.Name, // EF Core tự JOIN, không N+1
        TagCount = p.Tags.Count(),
        PublishedAt = p.PublishedAt!.Value
    })
    .OrderByDescending(p => p.PublishedAt)
    .ToListAsync();</code></pre>

<p>Projection covers khoảng 90% trường hợp list/read endpoints. Hãy làm quen với nó.</p>

<p><strong>Tuyệt đối tránh lazy loading</strong> — nó làm N+1 invisible trong code và chỉ phát hiện được khi profiling. Nếu đang dùng, disable ngay:</p>

<pre><code>// Disable lazy loading
optionsBuilder.UseLazyLoadingProxies(false); // hoặc đơn giản là không cài package</code></pre>

<h2>5. Safe Migrations trong Production</h2>

<p>Đây là phần quan trọng mà nhiều developer bỏ qua, kể cả với personal projects.</p>

<h3>Không bao giờ gọi Database.Migrate() lúc startup</h3>

<pre><code>// ĐỪng làm thế này trong production
app.Services.GetRequiredService&lt;AppDbContext&gt;().Database.Migrate(); // BAD</code></pre>

<p>Tại sao? Vì nó:</p>
<ul>
  <li>Block startup — app không serve request trong khi migrate</li>
  <li>Race condition khi nhiều instance khởi động đồng thời (dù EF Core 9 có migration lock, nhưng vẫn không phải best practice)</li>
  <li>Không có rollback path nếu migration fail giữa chừng</li>
</ul>

<p>Thay vào đó, generate idempotent SQL script và chạy trong CI/CD pipeline trước khi deploy app:</p>

<pre><code># Generate idempotent migration script
dotnet ef migrations script --idempotent -o migration.sql

# Hoặc dùng Migration Bundle
dotnet ef migrations bundle --output ./efbundle</code></pre>

<h3>Expand-Contract Pattern cho Breaking Changes</h3>

<p>Khi cần rename column, đổi kiểu dữ liệu, hoặc xóa column — không bao giờ làm trong một deploy. Dùng Expand-Contract:</p>

<ol>
  <li><strong>Expand:</strong> Thêm column mới (nullable), giữ column cũ. App viết vào cả hai.</li>
  <li><strong>Migrate:</strong> Migrate data từ column cũ sang mới. App đọc từ column mới.</li>
  <li><strong>Contract:</strong> Xóa column cũ sau khi xác nhận app hoạt động ổn.</li>
</ol>

<p>Rule quan trọng: Database phải luôn forward-compatible với phiên bản app trước. Deploy DB changes trước, sau đó mới deploy app changes. Với Fly.io rolling deploys, điều này đặc biệt quan trọng.</p>

<h2>6. PostgreSQL 17 và 18 — Những Thay Đổi Quan Trọng</h2>

<h3>PostgreSQL 17 (đang dùng)</h3>

<p>Một số cải tiến đáng chú ý cho production workloads:</p>

<ul>
  <li><strong>VACUUM memory giảm đến 20x</strong> — quan trọng với large tables, maintenance nhẹ hơn nhiều</li>
  <li><strong>MERGE với RETURNING clause</strong> — upsert-and-return trong một statement</li>
  <li><strong>JSON_TABLE</strong> — shredding JSON thành relational rows theo SQL/JSON standard</li>
  <li><strong>EXPLAIN gains SERIALIZE và MEMORY options</strong> — debug query plan chi tiết hơn</li>
</ul>

<pre><code>-- PostgreSQL 17: MERGE với RETURNING
MERGE INTO blog_posts AS target
USING (VALUES (@id, @title, @content)) AS source(id, title, content)
ON target.id = source.id
WHEN MATCHED THEN UPDATE SET title = source.title, content = source.content
WHEN NOT MATCHED THEN INSERT (id, title, content) VALUES (source.id, source.title, source.content)
RETURNING target.id, target.updated_at;</code></pre>

<h3>PostgreSQL 18 — Asynchronous I/O: Thay Đổi Lớn Nhất Trong Nhiều Năm</h3>

<p>PostgreSQL 18 (released tháng 9/2025) mang đến <strong>Asynchronous I/O (AIO)</strong> — thay đổi kiến trúc cơ bản. Thay vì I/O synchronous truyền thống, PostgreSQL giờ không block process trong khi chờ disk/network.</p>

<p>Benchmarks cho thấy <strong>2-3x cải thiện performance cho read-heavy workloads</strong>. Đặc biệt quan trọng trong môi trường cloud như Neon, Fly.io Postgres, hay RDS — nơi I/O latency là bottleneck chính.</p>

<p>Nếu bạn dùng Neon, khi họ upgrade fleet lên PostgreSQL 18, bạn được hưởng lợi tự động mà không cần thay đổi code gì.</p>

<h2>7. Neon Serverless — Tips Thực Tế</h2>

<p>Mình đang chạy VodonghaPersonal.id.vn trên Neon (Singapore region) với free tier. Đây là những gì mình học được sau nhiều tháng.</p>

<h3>Cold Start Reality (2026)</h3>

<p>Neon cold start numbers thực tế:</p>
<ul>
  <li>Median: 1.8 giây</li>
  <li>P95: 2.6 giây</li>
  <li>Worst case: 3.1 giây</li>
</ul>

<p>Neon đã commit sub-1s cold starts vào cuối 2026. Nhưng hiện tại, nếu dùng free tier (suspend sau 5 phút không có traffic), visitor đầu tiên sau idle sẽ thấy delay này.</p>

<h3>Tránh Double-Pooling</h3>

<p>Neon dùng PgBouncer ở transaction mode. <strong>Không được</strong> để Npgsql's built-in connection pool + PgBouncer chạy đồng thời — sẽ gây connection exhaustion:</p>

<pre><code>// Option 1: Dùng pooled connection string (port 6432) và tắt Npgsql pooling
""ConnectionStrings"": {
  ""DefaultConnection"": ""Host=ep-xxx.ap-southeast-1.aws.neon.tech;Port=6432;Database=neondb;Username=xxx;Password=xxx;Pooling=false;SSL Mode=Require""
}

// Option 2: Dùng direct connection (port 5432) với Npgsql pooling bình thường
""ConnectionStrings"": {
  ""DefaultConnection"": ""Host=ep-xxx.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=xxx;Password=xxx;SSL Mode=Require""
}</code></pre>

<h3>Scale-to-Zero Trap</h3>

<p>Một bẫy mình đã mắc: SignalR keepalive pings và Chart.js auto-refresh trên VodonghaPersonal.id.vn đang giữ Neon compute không bao giờ suspend. Kết quả là compute chạy liên tục và tốn credits.</p>

<p>Audit những gì đang giữ connections mở: health check endpoints, SignalR heartbeat, polling intervals. Với free tier, đây là vấn đề thực sự.</p>

<h3>Pattern Đúng với IDbContextFactory + Neon</h3>

<pre><code>// Mỗi component operation tạo context mới và dispose ngay
private async Task&lt;List&lt;BlogPost&gt;&gt; GetPostsAsync()
{
    await using var db = DbFactory.CreateDbContext();
    return await db.BlogPosts
        .Where(p => p.PublishedAt != null)
        .OrderByDescending(p => p.PublishedAt)
        .Take(10)
        .ToListAsync();
} // db dispose ở đây, connection trả về pool (hoặc đóng nếu dùng pooler)</code></pre>

<h2>8. PostgreSQL Indexing — Best Practices 2026</h2>

<p>Index đúng cách là một trong những optimization có ROI cao nhất. Nhưng over-indexing cũng là vấn đề thực.</p>

<h3>Chọn loại index phù hợp</h3>

<ul>
  <li><strong>B-tree</strong> (default) — equality, range, ORDER BY. 90% use cases.</li>
  <li><strong>GIN</strong> — full-text search, JSONB containment, array overlap.</li>
  <li><strong>BRIN</strong> — append-only large tables với naturally ordered data (logs, time-series). Index size cực nhỏ.</li>
  <li><strong>Partial index</strong> — chỉ index rows thỏa điều kiện WHERE:</li>
</ul>

<pre><code>-- Partial index: chỉ index published posts
CREATE INDEX idx_posts_published ON blog_posts (published_at DESC)
WHERE published_at IS NOT NULL;

-- Covering index: include thêm columns để tránh heap access
CREATE INDEX idx_posts_list ON blog_posts (published_at DESC)
INCLUDE (title, slug, author_id)
WHERE published_at IS NOT NULL;</code></pre>

<h3>Tìm unused indexes</h3>

<pre><code>-- Query pg_stat_user_indexes để tìm indexes không được dùng
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
WHERE idx_scan = 0
ORDER BY tablename, indexname;</code></pre>

<p>Mỗi index thêm write overhead. Trên write-heavy table, 10 indexes nghĩa là 10 B-tree updates mỗi INSERT/UPDATE. Chỉ tạo index khi đã profile và confirm query cần nó.</p>

<h3>EF Core Fluent API cho Indexes</h3>

<pre><code>// Tạo partial index qua EF Core Fluent API
modelBuilder.Entity&lt;BlogPost&gt;()
    .HasIndex(p => p.PublishedAt)
    .HasFilter(""published_at IS NOT NULL"")
    .IsDescending()
    .IncludeProperties(p => new { p.Title, p.Slug });</code></pre>

<h2>FAQ</h2>

<h3>Nên upgrade từ EF Core 9 lên EF Core 10 chưa?</h3>

<p>Nếu dự án đang chạy ổn định trên EF Core 9, không cần vội — EF Core 9 được support đến tháng 11/2026. Nhưng nếu đang start dự án mới hoặc đang trên .NET 10, upgrade lên EF Core 10 là lựa chọn đúng đắn. Con đường LTS-to-LTS khá clean, breaking changes ít. Đáng chú ý nhất là named query filters và LeftJoin/RightJoin operators — nếu dự án bạn cần hai tính năng này, đó là lý do thuyết phục để upgrade sớm.</p>

<h3>IDbContextFactory có cần thiết với Blazor WASM không?</h3>

<p>Blazor WASM chạy trên browser, không có DbContext trực tiếp — nó gọi qua API. Vấn đề IDbContextFactory chỉ relevant với Blazor Server (InteractiveServer render mode) nơi DbContext chạy trên server. Tuy nhiên nếu bạn dùng Blazor United (cả Server lẫn WASM trong cùng app như .NET 8+), hãy dùng IDbContextFactory cho phần Server-side.</p>

<h3>Neon free tier có đủ dùng cho portfolio site không?</h3>

<p>Hoàn toàn đủ cho portfolio/blog cá nhân với traffic thấp. Neon free tier cho 0.5 GB storage, 191.9 compute hours/tháng (khoảng 8 giờ/ngày nếu compute không suspend). Trick là đảm bảo compute thực sự suspend khi không có traffic — audit kỹ những gì đang giữ connection mở (SignalR, health checks, polling). Nếu app suspend đúng cách, 191.9 hours có thể kéo dài cả tháng dù traffic thấp.</p>

<h3>Có nên dùng raw SQL thay vì EF Core cho performance-critical queries không?</h3>

<p>EF Core 9/10 đã rất tốt — trong phần lớn trường hợp, projection với <code>Select()</code> cho performance tương đương raw SQL. Trước khi resort to raw SQL, hãy thử: (1) Projection-first thay vì Include-first, (2) kiểm tra execution plan với <code>ToQueryString()</code>, (3) thêm index phù hợp. Raw SQL chỉ nên dùng khi EF Core không thể generate query tối ưu sau khi đã optimize — ví dụ các window functions phức tạp hoặc recursive CTEs. Với PostgreSQL, EF Core + Npgsql hỗ trợ khá nhiều PostgreSQL-specific features qua <code>EF.Functions</code>.</p>

<h3>Làm thế nào để test số lượng SQL queries EF Core sinh ra?</h3>

<p>Ba cách hiệu quả: (1) Enable EF Core query logging trong Development để xem tất cả SQL được sinh ra trong Output window; (2) Dùng <code>MiniProfiler.AspNetCore</code> — hiển thị SQL count và timing ngay trên page; (3) Viết integration tests với SQLite in-memory và intercept <code>CommandExecuted</code> event để assert số lượng queries. Cách (3) tốt nhất vì nó catch regression trong CI/CD.</p>

<h2>Kết luận</h2>

<p>Stack PostgreSQL + EF Core năm 2026 đang ở trạng thái tốt nhất từ trước đến nay. EF Core 10 với named query filters và LeftJoin operators lấp đầy những gap lớn của các phiên bản trước. PostgreSQL 18's async I/O là cải tiến kiến trúc lớn nhất trong nhiều năm và ai dùng Neon sẽ được hưởng lợi tự động.</p>

<p>Ba điều mình sẽ làm ngay nếu đang xây dựng app mới hôm nay: (1) Dùng <code>IDbContextFactory</code> ngay từ đầu cho Blazor Server, (2) tư duy Projection-first thay vì Include-first, (3) không bao giờ gọi <code>Database.Migrate()</code> lúc startup.</p>

<p>Nếu bạn đang deploy lên Fly.io như mình, hãy đọc thêm bài <a href=""/blog/deploy-dotnet-fly-io-2025"">Deploy .NET lên Fly.io</a> để có cái nhìn end-to-end. Và nếu tò mò về việc mình build site này thế nào với Claude Code, xem bài <a href=""/blog/building-with-ai-experience-with-claude-code"">Kinh nghiệm với Claude Code</a>. Còn nếu bạn đang cân nhắc giữa Blazor và React, bài <a href=""/blog/blazor-vs-react-2025"">Blazor vs React 2025</a> có thể giúp ích.</p>", @"<p><img src=""https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=1200&auto=format&fit=crop&q=80"" alt=""PostgreSQL + EF Core Best Practices 2026"" style=""width:100%;border-radius:12px;margin-bottom:1.5rem"" /></p>

<p>When I built <strong>VodonghaPersonal.id.vn</strong> — a Blazor Web App running on Fly.io with PostgreSQL via Neon — I had to make a lot of architectural decisions around EF Core and PostgreSQL. This post brings together what I learned from that experience alongside the major shifts in 2025-2026: EF Core 10 arriving as the new LTS, PostgreSQL 18 introducing async I/O, and Neon continuing to improve its cold start story. Everything here comes from running this stack in production, not just reading docs.</p>

<h2>1. The 2026 Version Landscape</h2>

<p>Before diving into practices, here is where things stand today:</p>

<ul>
  <li><strong>.NET 10</strong> — Released November 2025. LTS, supported until November 2028.</li>
  <li><strong>EF Core 10</strong> — Released November 2025 alongside .NET 10. The new LTS and the right target for new projects.</li>
  <li><strong>EF Core 9</strong> — Released November 2024. Still supported until November 2026. No need to rush if your project is stable on it, but start planning the upgrade.</li>
  <li><strong>PostgreSQL 17</strong> — Stable production since September 2024.</li>
  <li><strong>PostgreSQL 18</strong> — Released September 2025. The most significant architectural change in years, with Asynchronous I/O.</li>
</ul>

<p>I am currently running EF Core 9 + PostgreSQL via Neon, and I have a planned upgrade to EF Core 10 on the roadmap. The LTS-to-LTS path is clean with minimal breaking changes — this is the right time to migrate.</p>

<h2>2. EF Core 9 and EF Core 10 — What Actually Matters</h2>

<h3>EF Core 9: What You Already Have</h3>

<p><strong>Native AOT compilation support</strong> is the feature I find most relevant for Fly.io deployments — faster startup times and a smaller memory footprint matter when your app scales to zero and has to cold start.</p>

<p><strong>Migration concurrency lock</strong> solves a real problem: race conditions when multiple instances start simultaneously and all attempt to run migrations. Previously you had to implement this yourself.</p>

<p><strong>Primitive collections in LINQ</strong> — you can now query directly into <code>List&lt;string&gt;</code> or <code>List&lt;int&gt;</code> without a join table:</p>

<pre><code>// EF Core 9 - query directly into a primitive collection
var posts = await db.BlogPosts
    .Where(p => p.Tags.Contains(""postgresql""))
    .ToListAsync();</code></pre>

<h3>EF Core 10: The Upgrade Worth Making</h3>

<p><strong>LeftJoin / RightJoin operators</strong> now have first-class support. Previously this required an ugly <code>DefaultIfEmpty()</code> workaround:</p>

<pre><code>// EF Core 10 - clean LeftJoin
var result = await db.Users
    .LeftJoin(db.Orders,
        user => user.Id,
        order => order.UserId,
        (user, order) => new { user.Name, OrderId = order != null ? order.Id : (int?)null })
    .ToListAsync();</code></pre>

<p><strong>Named query filters</strong> is the feature I have been waiting for longest. EF Core 8 and 9 only allow one filter per entity type — severely limiting when you need both soft-delete and tenant isolation on the same entity:</p>

<pre><code>// EF Core 10 - multiple named filters per entity
modelBuilder.Entity&lt;BlogPost&gt;()
    .HasQueryFilter(""softDelete"", p => !p.IsDeleted)
    .HasQueryFilter(""published"", p => p.PublishedAt != null);

// Selectively disable one filter per query
var drafts = await db.BlogPosts
    .IgnoreQueryFilters(""published"")
    .ToListAsync();</code></pre>

<p><strong>Vector data type support</strong> adds native integration with Azure SQL for AI/semantic search workloads — relevant if you are building RAG pipelines or embedding-based search.</p>

<h2>3. IDbContextFactory — No Longer Optional in Blazor Server</h2>

<p>This section is the most important one for anyone running Blazor Server with InteractiveServer render mode, which is exactly what VodonghaPersonal.id.vn uses.</p>

<p>The core problem: with the standard <code>AddDbContext&lt;T&gt;()</code> registration, a scoped <code>DbContext</code> lives for the entire Blazor circuit lifetime — potentially hours. This leads to:</p>

<ul>
  <li>Stale data reads because the context caches entities</li>
  <li>Thread-safety violations when two async operations run concurrently on the same context instance</li>
  <li>The classic error: <em>""A second operation was started on this context instance before a previous asynchronous operation completed""</em></li>
</ul>

<p>Microsoft now recommends <code>IDbContextFactory</code> as the default approach for Blazor Server in the official ASP.NET Core documentation. Here is the correct setup:</p>

<pre><code>// Program.cs
builder.Services.AddDbContextFactory&lt;AppDbContext&gt;(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(""DefaultConnection"")));

// Component usage - each operation creates a short-lived context
@inject IDbContextFactory&lt;AppDbContext&gt; DbFactory

private async Task LoadDataAsync()
{
    await using var db = DbFactory.CreateDbContext();
    _posts = await db.BlogPosts
        .Where(p => p.PublishedAt != null)
        .OrderByDescending(p => p.PublishedAt)
        .Select(p => new PostSummaryDto
        {
            Id = p.Id,
            Title = p.Title,
            PublishedAt = p.PublishedAt
        })
        .ToListAsync();
}</code></pre>

<p>With Neon serverless, this pattern has an additional benefit: the context is created and disposed immediately after use, so it does not hold a connection open between user interactions, allowing Neon compute to suspend on schedule.</p>

<p><strong>Hard rule:</strong> Never inject <code>DbContext</code> directly into singleton services. Always use <code>IDbContextFactory&lt;T&gt;</code> there.</p>

<h2>4. N+1 Queries — Still the Number One EF Core Performance Killer</h2>

<p>N+1 is not a new problem, but it remains the most common cause of EF Core performance issues in 2026. I ran into it while building the blog section of VodonghaPersonal.id.vn.</p>

<h3>Eager Loading with Include()</h3>

<pre><code>// BAD - N+1: 1 query for posts, N queries for each author
var posts = await db.BlogPosts.ToListAsync();
foreach (var post in posts)
{
    Console.WriteLine(post.Author.Name); // lazy load triggered per post
}

// GOOD - Eager loading fetches everything in one query
var posts = await db.BlogPosts
    .Include(p => p.Author)
    .ToListAsync();</code></pre>

<h3>AsSplitQuery() When Including Multiple Collections</h3>

<p>Including multiple collection navigations causes a cartesian explosion — the resulting JOIN produces a massive result set. <code>AsSplitQuery()</code> generates N clean separate queries instead:</p>

<pre><code>var posts = await db.BlogPosts
    .Include(p => p.Tags)
    .Include(p => p.Comments)
    .AsSplitQuery() // 3 clean queries instead of one massive JOIN
    .ToListAsync();</code></pre>

<h3>Projection with Select() — The Most Effective Approach</h3>

<p>This is the key discipline shift: think Projection-first rather than Include-first. Only fetch the columns the view actually needs:</p>

<pre><code>// BEST - Projection: only the columns needed, EF Core handles the JOIN
var postDtos = await db.BlogPosts
    .Where(p => p.PublishedAt != null)
    .Select(p => new PostListDto
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        AuthorName = p.Author.Name, // EF Core generates a JOIN, no N+1
        TagCount = p.Tags.Count(),
        PublishedAt = p.PublishedAt!.Value
    })
    .OrderByDescending(p => p.PublishedAt)
    .ToListAsync();</code></pre>

<p>Projection covers roughly 90% of list and read endpoint use cases. Make it your default, not your fallback.</p>

<p><strong>Avoid lazy loading entirely.</strong> It makes N+1 invisible in code and undetectable until profiling under load. If it is enabled in your project, disable it explicitly:</p>

<pre><code>// Ensure lazy loading is disabled
optionsBuilder.UseLazyLoadingProxies(false); // or simply do not install the proxy package</code></pre>

<h2>5. Safe Migrations in Production</h2>

<p>This is an area where even personal projects benefit from production-grade discipline, especially with rolling deploys on Fly.io.</p>

<h3>Never Call Database.Migrate() at Startup</h3>

<pre><code>// DO NOT do this in production
app.Services.GetRequiredService&lt;AppDbContext&gt;().Database.Migrate(); // BAD</code></pre>

<p>The problems with this approach:</p>
<ul>
  <li>Blocks startup — the app cannot serve requests while migrating</li>
  <li>Race conditions on multi-instance deployments, even with EF Core 9's migration lock</li>
  <li>No rollback path if a migration fails partway through</li>
</ul>

<p>Instead, generate an idempotent SQL script and run it as a dedicated CI/CD step before deploying the application:</p>

<pre><code># Generate idempotent migration script
dotnet ef migrations script --idempotent -o migration.sql

# Or use a Migration Bundle
dotnet ef migrations bundle --output ./efbundle</code></pre>

<h3>The Expand-Contract Pattern for Breaking Changes</h3>

<p>When you need to rename a column, change a data type, or remove a column, never do it in a single deployment. The Expand-Contract pattern keeps every deployment backward-compatible:</p>

<ol>
  <li><strong>Expand:</strong> Add the new column (nullable), keep the old one. Application writes to both.</li>
  <li><strong>Migrate:</strong> Backfill data to the new column. Update the application to read from the new column.</li>
  <li><strong>Contract:</strong> Drop the old column once the new version is confirmed stable.</li>
</ol>

<p>The fundamental rule: the database must always be forward-compatible with the previous application version. Deploy database changes before application changes. With Fly.io rolling deploys, there will always be a window where two versions of your app are running simultaneously.</p>

<h2>6. PostgreSQL 17 and 18 — What Changed</h2>

<h3>PostgreSQL 17 Notable Improvements</h3>

<ul>
  <li><strong>VACUUM memory usage reduced up to 20x</strong> — significant improvement for large table maintenance overhead</li>
  <li><strong>MERGE with RETURNING clause</strong> — upsert-and-return in a single statement</li>
  <li><strong>JSON_TABLE</strong> — shred JSON into relational rows per SQL/JSON standard</li>
  <li><strong>EXPLAIN gains SERIALIZE and MEMORY options</strong> — more detailed query plan analysis</li>
</ul>

<pre><code>-- PostgreSQL 17: MERGE with RETURNING
MERGE INTO blog_posts AS target
USING (VALUES (@id, @title, @content)) AS source(id, title, content)
ON target.id = source.id
WHEN MATCHED THEN UPDATE SET title = source.title, content = source.content
WHEN NOT MATCHED THEN INSERT (id, title, content) VALUES (source.id, source.title, source.content)
RETURNING target.id, target.updated_at;</code></pre>

<h3>PostgreSQL 18 — Asynchronous I/O: The Biggest Change in Years</h3>

<p>PostgreSQL 18, released September 2025, introduces <strong>Asynchronous I/O (AIO)</strong> as a fundamental architectural shift. Rather than blocking a process while waiting on disk or network I/O, PostgreSQL can now issue multiple I/O requests and process other work while waiting for results.</p>

<p>Benchmarks show <strong>2-3x performance improvement for read-heavy workloads</strong>. This is particularly impactful in cloud environments — Neon, Fly.io Postgres, RDS — where I/O latency is the primary bottleneck rather than CPU.</p>

<p>If you are using Neon, you will benefit from this automatically when Neon upgrades their fleet, with no code changes required on your side.</p>

<h2>7. Neon Serverless — Practical Production Tips</h2>

<p>I have been running VodonghaPersonal.id.vn on Neon's free tier in the Singapore region for several months. Here is what the experience actually looks like.</p>

<h3>Cold Start Numbers (2026)</h3>

<p>Real cold start latency on Neon today:</p>
<ul>
  <li>Median: 1.8 seconds</li>
  <li>P95: 2.6 seconds</li>
  <li>Worst case: 3.1 seconds</li>
</ul>

<p>Neon has committed to sub-1s cold starts by end of 2026. For now, the first visitor after an idle period will experience this delay. On the free tier, the suspend threshold is fixed at 5 minutes and is not configurable.</p>

<h3>Avoid Double-Pooling</h3>

<p>Neon uses PgBouncer in transaction mode. Running Npgsql's built-in connection pool simultaneously with PgBouncer causes connection exhaustion. Choose one approach:</p>

<pre><code>// Option 1: Use the pooled endpoint (port 6432) and disable Npgsql pooling
""ConnectionStrings"": {
  ""DefaultConnection"": ""Host=ep-xxx.ap-southeast-1.aws.neon.tech;Port=6432;Database=neondb;Username=xxx;Password=xxx;Pooling=false;SSL Mode=Require""
}

// Option 2: Use the direct endpoint (port 5432) with Npgsql's own pooling
""ConnectionStrings"": {
  ""DefaultConnection"": ""Host=ep-xxx.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=xxx;Password=xxx;SSL Mode=Require""
}</code></pre>

<h3>The Scale-to-Zero Trap</h3>

<p>One mistake I made on VodonghaPersonal.id.vn: SignalR keepalive pings and Chart.js auto-refresh intervals were keeping the Neon compute from ever suspending. The compute ran continuously and I burned through credits much faster than expected.</p>

<p>Audit what is keeping connections open: health check endpoints, SignalR heartbeat intervals, polling timers. On the free tier, this is a real concern. The correct pattern with <code>IDbContextFactory</code> helps here — each component operation gets a fresh, short-lived context that is disposed immediately and does not hold the compute awake between interactions.</p>

<pre><code>// Each operation creates and disposes its own context
private async Task&lt;List&lt;BlogPost&gt;&gt; GetRecentPostsAsync()
{
    await using var db = DbFactory.CreateDbContext();
    return await db.BlogPosts
        .Where(p => p.PublishedAt != null)
        .OrderByDescending(p => p.PublishedAt)
        .Take(10)
        .ToListAsync();
} // context disposed here, connection returned to pool or closed</code></pre>

<h2>8. PostgreSQL Indexing — 2026 Best Practices</h2>

<p>Correct indexing has one of the highest ROI of any database optimization. But over-indexing is an equally real problem that developers overlook.</p>

<h3>Choosing the Right Index Type</h3>

<ul>
  <li><strong>B-tree</strong> (default) — equality, range, ORDER BY. Covers 90% of use cases.</li>
  <li><strong>GIN</strong> — full-text search, JSONB containment queries, array overlap.</li>
  <li><strong>BRIN</strong> — large append-only tables with naturally ordered data (logs, time-series). Tiny index footprint.</li>
  <li><strong>Partial index</strong> — index only rows matching a WHERE condition. Dramatically smaller when the filtered set is a small fraction of total rows.</li>
  <li><strong>Covering index (INCLUDE)</strong> — include non-key columns to enable index-only scans, eliminating table heap access for hot read paths.</li>
</ul>

<pre><code>-- Partial index: only index published posts
CREATE INDEX idx_posts_published ON blog_posts (published_at DESC)
WHERE published_at IS NOT NULL;

-- Covering index: eliminate heap access for list queries
CREATE INDEX idx_posts_list ON blog_posts (published_at DESC)
INCLUDE (title, slug, author_id)
WHERE published_at IS NOT NULL;</code></pre>

<h3>Find and Remove Unused Indexes</h3>

<pre><code>-- Query pg_stat_user_indexes to find indexes with zero scans
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
WHERE idx_scan = 0
ORDER BY tablename, indexname;</code></pre>

<p>Every index adds write overhead. On a write-heavy table, 10 indexes means 10 additional B-tree updates per INSERT or UPDATE. Only create an index after profiling has confirmed a specific query needs it.</p>

<h3>EF Core Fluent API for PostgreSQL Indexes</h3>

<pre><code>// Define a partial covering index through EF Core
modelBuilder.Entity&lt;BlogPost&gt;()
    .HasIndex(p => p.PublishedAt)
    .HasFilter(""published_at IS NOT NULL"")
    .IsDescending()
    .IncludeProperties(p => new { p.Title, p.Slug });</code></pre>

<h2>FAQ</h2>

<h3>Should I upgrade from EF Core 9 to EF Core 10 now?</h3>

<p>If your project is stable on EF Core 9, there is no urgency — EF Core 9 is supported until November 2026. However, if you are starting a new project or already running on .NET 10, EF Core 10 is the clear choice. The LTS-to-LTS migration path is clean and breaking changes are minimal. The two most compelling reasons to upgrade sooner: named query filters (if you need more than one global filter per entity) and the cleaner LeftJoin/RightJoin syntax. If your project relies heavily on either of those, the upgrade pays for itself quickly.</p>

<h3>Does IDbContextFactory matter for Blazor WebAssembly?</h3>

<p>Blazor WebAssembly runs entirely in the browser and does not have direct database access — it communicates through your API. The IDbContextFactory concern only applies to Blazor Server (InteractiveServer render mode) where the DbContext runs server-side for the duration of a circuit. If you are using Blazor United (.NET 8+) with both server and WASM components in the same app, use IDbContextFactory for any server-side database access to be safe.</p>

<h3>Is Neon's free tier sufficient for a portfolio site?</h3>

<p>Yes, with some awareness. Neon free tier provides 0.5 GB storage and 191.9 compute hours per month. The key to making those hours last is ensuring the compute actually suspends when there is no traffic. Audit everything that might be keeping connections open — SignalR keepalive, health check polling, client-side auto-refresh timers. If your compute suspends correctly, 191.9 hours stretches a very long way for a low-traffic portfolio site. If something prevents suspension, you can burn through those hours in under two weeks.</p>

<h3>When should I use raw SQL instead of EF Core for performance?</h3>

<p>EF Core 9 and 10 are excellent query generators in the vast majority of cases. Before reaching for raw SQL, work through this checklist: (1) switch to Projection-first with <code>Select()</code> if you are still using Include-first, (2) inspect the generated SQL with <code>ToQueryString()</code> to confirm what EF Core is actually doing, (3) verify you have the right indexes in place. Raw SQL is warranted when EF Core cannot generate an efficient query after all optimizations — complex window functions, recursive CTEs, or highly specific PostgreSQL features. The Npgsql provider also exposes a lot of PostgreSQL-specific functionality through <code>EF.Functions</code>, so check there before dropping to raw SQL.</p>

<h3>How do I detect how many SQL queries EF Core is generating?</h3>

<p>Three practical approaches: (1) Enable EF Core query logging in Development — add <code>LogTo(Console.WriteLine, LogLevel.Information)</code> to your DbContext options and watch the output; (2) Install MiniProfiler.AspNetCore — it displays SQL count and timing inline on each page during development; (3) Write integration tests using SQLite in-memory and subscribe to the <code>CommandExecuted</code> diagnostic event to assert on query count. The integration test approach is the most valuable because it catches regressions in CI/CD before they reach production.</p>

<h2>Conclusion</h2>

<p>The PostgreSQL + EF Core stack in 2026 is in the best shape it has ever been. EF Core 10 fills the most significant gaps from earlier versions — named query filters and proper join operators were long overdue. PostgreSQL 18's async I/O is the architectural story of 2025-2026 in the database world, and Neon users will benefit from it automatically as the fleet upgrades.</p>

<p>Three things I would do immediately when starting a new project today: (1) register with <code>AddDbContextFactory</code> from day one for any Blazor Server app, (2) default to Projection-first with <code>Select()</code> rather than Include-first, (3) never call <code>Database.Migrate()</code> at startup — run migrations as a separate CI/CD step before deploying the application.</p>

<p>If you are deploying to Fly.io as I am, the post on <a href=""/blog/deploy-dotnet-fly-io-2025"">deploying .NET to Fly.io</a> covers the end-to-end deployment workflow. If you are curious about how I built VodonghaPersonal.id.vn using AI assistance, see the post on my <a href=""/blog/building-with-ai-experience-with-claude-code"">experience with Claude Code</a>. And if you are weighing Blazor against React for your next project, <a href=""/blog/blazor-vs-react-2025"">Blazor vs React 2025</a> covers the tradeoffs in detail.</p>", @"EF Core 10 LTS, PostgreSQL 18 với async I/O, IDbContextFactory trong Blazor, tránh N+1, migration zero-downtime và tips thực tế khi dùng Neon serverless.", @"EF Core 10 LTS, PostgreSQL 18 async I/O, IDbContextFactory in Blazor Server, N+1 avoidance, zero-downtime migrations, and practical Neon serverless tips for 2026.", @"postgresql,entity-framework-core,dotnet,blazor,neon,database,performance,migrations", @"PostgreSQL + EF Core: Best Practices cho năm 2026", @"PostgreSQL + EF Core: Best Practices for 2026", new DateTime(2026, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty â€” content update is not reversible without backup.
        }
    }
}
