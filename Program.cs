using Resend;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using vodongha.Components;
using vodongha.Data;
using vodongha.Hubs;
using vodongha.Services;
using vodongha.Data.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

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

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// Use DbContextFactory to avoid concurrency issues in Blazor Server
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Also register scoped DbContext (used by health checks and migration)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

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
builder.Services.AddHttpClient<TelegramService>()
    .AddTypedClient((http, sp) => new TelegramService(http, sp.GetRequiredService<AppSecretsService>()));
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AdminLocalizationService>();
builder.Services.AddScoped<TimezoneService>();
builder.Services.AddSingleton<AppSecretsService>();
builder.Services.AddSingleton<HealthMonitorService>();
builder.Services.AddScoped<CvPdfService>();
builder.Services.AddSingleton<CostMonitorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<HealthMonitorService>());
builder.Services.AddHttpContextAccessor();

WebApplication app = builder.Build();

// Auto migrate on startup
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

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
        VisitorService visitorSvc = context.RequestServices.GetRequiredService<VisitorService>();
        await visitorSvc.LogAsync(ip, ua);
    }

    await next(context);
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapHealthChecks("/health");

app.MapPost("/admin/do-login", async (HttpContext ctx, IConfiguration config) =>
{
    string username = ctx.Request.Form["username"].ToString();
    string password = ctx.Request.Form["password"].ToString();
    string adminUser = config["Admin:Username"] ?? "admin";
    string adminPass = config["Admin:Password"] ?? "changeme";
    if (username == adminUser && password == adminPass)
    {
        System.Security.Claims.Claim[] claims = [new(System.Security.Claims.ClaimTypes.Name, username), new(System.Security.Claims.ClaimTypes.Role, "Admin")];
        System.Security.Claims.ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
        ctx.Response.Redirect("/admin");
    }
    else
    {
        ctx.Response.Redirect("/admin/login?error=1");

    }
}).DisableAntiforgery();

app.MapPost("/admin/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/");
}).DisableAntiforgery();
app.MapHub<ChatHub>("/chathub");

app.MapGet("/api/cv/download", async (
    IDbContextFactory<AppDbContext> dbFactory,
    SiteSettingService settingsSvc,
    CvPdfService cvPdf,
    ILogger<Program> logger) =>
{
    try
    {
        Dictionary<string, string> settings = await settingsSvc.GetAllAsync();
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        CvData data = new(
            Name:        settings.GetValueOrDefault("Name", ""),
            Title:       settings.GetValueOrDefault("Title", ""),
            Email:       settings.GetValueOrDefault("Email", ""),
            Phone:       settings.GetValueOrDefault("Phone", ""),
            Location:    settings.GetValueOrDefault("Location", ""),
            GitHub:      settings.GetValueOrDefault("GitHub", ""),
            LinkedIn:    settings.GetValueOrDefault("LinkedIn", ""),
            Bio:         settings.GetValueOrDefault("BioEn", settings.GetValueOrDefault("Bio", "")),
            AvatarUrl:   settings.GetValueOrDefault("AvatarUrl", ""),
            Skills:      await db.Skills.OrderBy(s => s.Order).ToListAsync(),
            Experiences: await db.Experiences.OrderBy(e => e.Order).ToListAsync(),
            Educations:  await db.Educations.OrderBy(e => e.Order).ToListAsync(),
            Projects:    await db.Projects.OrderBy(p => p.Order).ToListAsync()
        );

        logger.LogInformation("CV download: generating PDF for {Name}", data.Name);
        byte[] pdf = await Task.Run(() => cvPdf.Generate(data));
        logger.LogInformation("CV download: PDF generated, {Bytes} bytes", pdf.Length);
        string name = data.Name.ToLower().Replace(" ", "-");
        return Results.File(pdf, "application/pdf", $"cv-{name}.pdf");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "CV download failed: {Message}", ex.Message);
        return Results.Problem($"PDF generation failed: {ex.Message}");
    }
}).RequireAuthorization();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
