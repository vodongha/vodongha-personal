using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Resend;
using VodonghaPersonal.Api;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Components;
using VodonghaPersonal.Data;
using VodonghaPersonal.Hubs;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    // Default 32KB kills the circuit when a large paste lands in a bound input —
    // the whole interactive UI dies and entered form data is lost.
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 256 * 1024)
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

// EF Core navigation properties create circular references (e.g. ChatSession ↔ ChatMessage).
// IgnoreCycles prevents System.Text.Json from throwing when serializing minimal API responses.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Persist Data Protection keys to DB so they survive redeploys
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("vodongha");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Use DbContextFactory for all DB access — avoids scoped/singleton DI conflicts in Blazor Server.
// The transient registration below allows health checks (and any code requiring a raw AppDbContext)
// to obtain one via the factory without registering a separate scoped AddDbContext.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection"),
               o => o.UseOracleSQLCompatibility(Microsoft.EntityFrameworkCore.OracleSQLCompatibility.DatabaseVersion19))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddTransient<AppDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o => o.ApiToken = builder.Configuration["Email:ResendApiKey"] ?? "");
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddScoped<ExperienceService>();
builder.Services.AddScoped<EducationService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<SiteSettingService>();
builder.Services.AddScoped<VisitorService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<PushNotificationService>();
builder.Services.AddHttpClient<TelegramService>()
    .AddTypedClient((http, sp) => new TelegramService(http, sp.GetRequiredService<AppSecretsService>()));
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<TimezoneService>();
builder.Services.AddSingleton<AppSecretsService>();
builder.Services.AddSingleton<HealthMonitorService>();
builder.Services.AddScoped<CvPdfService>();
builder.Services.AddSingleton<CvCacheService>();
builder.Services.AddHttpClient<AiService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("deps").ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddHttpClient("github").ConfigureHttpClient(c =>
{
    c.Timeout = TimeSpan.FromSeconds(5);
    c.DefaultRequestHeaders.Add("User-Agent", "vodongha-personal");
});
builder.Services.AddSingleton<DependencyCheckService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<HealthMonitorService>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<AdminAuthService>();

// Public API clients — registered in Server DI for SSR prerendering.
// Each client gets its own HttpClient instance via factory to avoid "already started" error.
builder.Services.AddHttpClient("prerender").ConfigureHttpClient((sp, client) =>
{
    IHttpContextAccessor ctx = sp.GetRequiredService<IHttpContextAccessor>();
    HttpRequest req = ctx.HttpContext!.Request;
    client.BaseAddress = new Uri($"{req.Scheme}://{req.Host}");
});
builder.Services.AddScoped<PublicBlogApiClient>(sp =>
    new PublicBlogApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("prerender")));
builder.Services.AddScoped<PublicChatApiClient>(sp =>
    new PublicChatApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("prerender")));
builder.Services.AddScoped<PublicAiApiClient>(sp =>
    new PublicAiApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("prerender")));
builder.Services.AddScoped<PublicSiteApiClient>(sp =>
    new PublicSiteApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("prerender")));

// Rate limiter — 10 login attempts per 5 minutes per IP to prevent brute-force
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(5);
        limiterOptions.PermitLimit = 10;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

WebApplication app = builder.Build();

// Auto migrate on startup
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed initial admin user from config if AdminUsers table is empty.
    // After first deploy, credentials live in DB — env vars are no longer used for login.
    string seedUser = app.Configuration["Admin:Username"] ?? "admin";
    string seedPass = app.Configuration["Admin:Password"] ?? "changeme";
    AdminAuthService adminAuth = scope.ServiceProvider.GetRequiredService<AdminAuthService>();
    await adminAuth.SeedFromConfigAsync(seedUser, seedPass);
}

// One-time: sync secrets from ENV → DB on startup.
// Only saves keys that have a value in ENV and are NOT yet in the DB.
// After all keys are saved, this block becomes a no-op.
{
    using IServiceScope scope = app.Services.CreateScope();
    AppSecretsService secretsSvc = scope.ServiceProvider.GetRequiredService<AppSecretsService>();
    IConfiguration config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    foreach (AppSecretDefinition def in AppSecretsService.Definitions)
    {
        // Skip if already in DB
        if (await secretsSvc.HasOverrideAsync(def.Key))
        {
            continue;
        }

        string? value = config[def.Key];
        if (!string.IsNullOrWhiteSpace(value))
        {
            await secretsSvc.SaveAsync(def.Key, value);
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Redirect www → non-www canonical
app.Use(async (context, next) =>
{
    HostString host = context.Request.Host;
    if (host.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
    {
        string canonical = $"https://{host.Host[4..]}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(canonical, permanent: true);
        return;
    }
    await next();
});

// Apply language preference from cookie so SSR sections render in the correct language.
// Only applies to page requests (GET, not /_framework, not API) to avoid touching
// unrelated scoped services on every API call.
app.Use(async (context, next) =>
{
    bool isPageGet = context.Request.Method == "GET"
        && !context.Request.Path.StartsWithSegments("/_framework")
        && !context.Request.Path.StartsWithSegments("/api")
        && !context.Request.Path.StartsWithSegments("/health");
    if (isPageGet && context.Request.Cookies.TryGetValue("lang", out string? lang) && (lang == "vi" || lang == "en"))
    {
        try { context.RequestServices.GetRequiredService<LanguageService>().Set(lang); }
        catch { /* non-critical — never break the pipeline */ }
    }
    await next(context);
});

// Track unique visitors by IP on page requests
app.Use(async (context, next) =>
{
    string path = context.Request.Path.Value ?? "";
    bool isPageRequest = context.Request.Method == "GET"
        && !path.StartsWith("/_")
        && !path.StartsWith("/health")
        && !path.StartsWith("/admin")
        && !Path.HasExtension(path);

    if (isPageRequest)
    {
        string ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
        string? ua = context.Request.Headers.UserAgent.FirstOrDefault();
        string? referrer = context.Request.Headers.Referer.FirstOrDefault();
        VisitorService visitorSvc = context.RequestServices.GetRequiredService<VisitorService>();
        AnalyticsService analyticsSvc = context.RequestServices.GetRequiredService<AnalyticsService>();
        ILogger<Program> mwLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        await visitorSvc.LogAsync(ip, ua);
        _ = analyticsSvc.TrackAsync(path, referrer, ip)
            .ContinueWith(t => mwLogger.LogError(t.Exception, "Analytics tracking failed for {Path}", path),
                TaskContinuationOptions.OnlyOnFaulted);
    }

    await next(context);
});
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapHealthChecks("/health");

app.MapPost("/admin/do-login", async (HttpContext ctx, AdminAuthService adminAuth) =>
{
    string username = ctx.Request.Form["username"].ToString();
    string password = ctx.Request.Form["password"].ToString();

    if (await adminAuth.ValidateAsync(username, password))
    {
        System.Security.Claims.Claim[] claims =
        [
            new(System.Security.Claims.ClaimTypes.Name, username),
            new(System.Security.Claims.ClaimTypes.Role, "Admin")
        ];
        System.Security.Claims.ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
        ctx.Response.Redirect("/admin");
    }
    else
    {
        ctx.Response.Redirect("/admin/login?error=1");
    }
}).DisableAntiforgery().RequireRateLimiting("login");

app.MapPost("/admin/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/");
}).RequireAuthorization();

app.MapPost("/admin/change-password", async (HttpContext ctx, AdminAuthService adminAuth) =>
{
    string username = ctx.User.Identity?.Name ?? "";
    string current = ctx.Request.Form["currentPassword"].ToString();
    string newPass = ctx.Request.Form["newPassword"].ToString();

    if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 8)
    {
        return Results.BadRequest("Mật khẩu mới phải có ít nhất 8 ký tự.");
    }

    bool ok = await adminAuth.ChangePasswordAsync(username, current, newPass);
    return ok ? Results.Ok() : Results.BadRequest("Mật khẩu hiện tại không đúng.");
}).DisableAntiforgery().RequireAuthorization();

app.MapHub<ChatHub>("/chathub");

app.MapGet("/api/cv/download", async (
    CvCacheService cvCache,
    ILogger<Program> logger,
    int template = 0) =>
{
    byte[]? cached = await cvCache.ReadAsync(template);
    if (cached is not null)
    {
        logger.LogInformation("CV download: serving cached file, template={T}", template);
        return Results.File(cached, "application/pdf", "cv.pdf");
    }

    try
    {
        byte[] pdf = await cvCache.BuildPdfAsync(template);
        await cvCache.WriteAsync(template, pdf);
        return Results.File(pdf, "application/pdf", "cv.pdf");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "CV download failed: {Message}", ex.Message);
        return Results.Problem($"PDF generation failed: {ex.Message}");
    }
}).RequireAuthorization();

// ── Web Push subscription endpoints ──────────────────────────────────────────

app.MapPost("/api/push/subscribe", async (HttpContext ctx, PushNotificationService pushSvc) =>
{
    var body = await ctx.Request.ReadFromJsonAsync<PushSubscribeRequest>();
    if (body is null || string.IsNullOrEmpty(body.Endpoint))
    {
        return Results.BadRequest();
    }

    // Determine IsAdmin server-side — never trust the client to set this
    bool isAdmin = ctx.User.Identity?.IsAuthenticated == true && ctx.User.IsInRole("Admin");
    await pushSvc.SaveSubscriptionAsync(body.Endpoint, body.P256DH, body.Auth, body.ChatSessionId, isAdmin);
    return Results.Ok();
}).DisableAntiforgery();

app.MapDelete("/api/push/unsubscribe", async (HttpContext ctx, PushNotificationService pushSvc) =>
{
    var body = await ctx.Request.ReadFromJsonAsync<PushUnsubscribeRequest>();
    if (body is null || string.IsNullOrEmpty(body.Endpoint))
    {
        return Results.BadRequest();
    }

    await pushSvc.RemoveSubscriptionAsync(body.Endpoint);
    return Results.Ok();
}).DisableAntiforgery();

app.MapPost("/api/telegram/webhook", async (HttpContext ctx, ChatService chatService, IConfiguration config) =>
{
    string secret = config["Telegram:WebhookSecret"] ?? "";
    string headerSecret = ctx.Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault() ?? "";
    if (!string.IsNullOrEmpty(secret) && headerSecret != secret)
    {
        return Results.Unauthorized();
    }

    TelegramUpdate? update = await ctx.Request.ReadFromJsonAsync<TelegramUpdate>();
    if (update != null)
    {
        await chatService.HandleTelegramWebhookAsync(update);
    }

    return Results.Ok();
}).DisableAntiforgery();

// ── One-time admin endpoint: sync all secrets from ENV → DB ──────────────────
// Call POST /api/admin/sync-secrets-to-db with Basic auth (admin credentials).
// Safe to call multiple times — skips keys with no ENV value, overwrites DB if a value exists.
app.MapPost("/api/admin/sync-secrets-to-db", async (HttpContext ctx, AppSecretsService secretsSvc, IConfiguration config) =>
{
    // Validate Basic auth
    string? authHeader = ctx.Request.Headers.Authorization.FirstOrDefault();
    if (authHeader == null || !authHeader.StartsWith("Basic ", StringComparison.Ordinal))
    {
        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"admin\"";
        return Results.Unauthorized();
    }

    string encoded = authHeader["Basic ".Length..].Trim();
    string decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
    string[] parts = decoded.Split(':', 2);
    string adminUser = config["Admin:Username"] ?? "admin";
    string adminPass = config["Admin:Password"] ?? "changeme";

    if (parts.Length != 2 || parts[0] != adminUser || parts[1] != adminPass)
    {
        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"admin\"";
        return Results.Unauthorized();
    }

    List<object> results = [];

    foreach (AppSecretDefinition def in AppSecretsService.Definitions)
    {
        string? value = config[def.Key];
        if (string.IsNullOrWhiteSpace(value))
        {
            results.Add(new { key = def.Key, status = "skipped", reason = "no value in ENV/config" });
            continue;
        }

        bool ok = await secretsSvc.SaveAsync(def.Key, value);
        results.Add(new { key = def.Key, status = ok ? "saved" : "error" });
    }

    return Results.Ok(results);
}).DisableAntiforgery();

app.MapGet("/sitemap.xml", async (BlogService blogSvc, IConfiguration config) =>
{
    string baseUrl = (config["App:BaseUrl"] ?? "https://VodonghaPersonal.id.vn").TrimEnd('/');
    List<VodonghaPersonal.Shared.Models.BlogPost> posts = await blogSvc.GetAllSlugsForSitemapAsync();
    System.Text.StringBuilder sb = new();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

    // Static pages
    foreach (string staticUrl in new[] { "", "/#projects", "/#blog", "/#contact" })
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/{staticUrl}</loc>");
        sb.AppendLine("    <changefreq>monthly</changefreq>");
        sb.AppendLine("    <priority>0.8</priority>");
        sb.AppendLine("  </url>");
    }

    // Blog posts
    foreach (VodonghaPersonal.Shared.Models.BlogPost post in posts)
    {
        string lastmod = (post.UpdatedAt ?? post.CreatedAt).ToString("yyyy-MM-dd");
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/blog/{post.Slug}</loc>");
        sb.AppendLine($"    <lastmod>{lastmod}</lastmod>");
        sb.AppendLine("    <changefreq>weekly</changefreq>");
        sb.AppendLine("    <priority>0.9</priority>");
        sb.AppendLine("  </url>");
    }

    sb.AppendLine("</urlset>");
    return Results.Content(sb.ToString(), "application/xml");
});

app.MapAdminSkillsApi();
app.MapAdminProjectsApi();
app.MapAdminBlogApi();
app.MapAdminEducationApi();
app.MapAdminExperienceApi();
app.MapAdminContactsApi();
app.MapAdminSettingsApi();
app.MapAdminDashboardApi();
app.MapAdminAnalyticsApi();
app.MapAdminApiKeysApi();
app.MapAdminHealthApi();
app.MapAdminDependenciesApi();
app.MapAdminChatApi();
app.MapAdminMenuApi();

app.MapPublicBlogApi();
app.MapPublicChatApi();
app.MapPublicAiApi();
app.MapPublicSiteApi();
app.MapPublicAuthApi();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(VodonghaPersonal.Client._Imports).Assembly);

app.Run();

// ── DTOs ──────────────────────────────────────────────────────────────────────
// IsAdmin is intentionally removed from this DTO — it is always determined server-side from auth state.
record PushSubscribeRequest(string Endpoint, string P256DH, string Auth, int? ChatSessionId);
record PushUnsubscribeRequest(string Endpoint);
