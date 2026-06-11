using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VodonghaPersonal.Client;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// HttpClient with base address of the Server host
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register all API clients
builder.Services.AddScoped<SkillApiClient>();
builder.Services.AddScoped<ProjectApiClient>();
builder.Services.AddScoped<BlogApiClient>();
builder.Services.AddScoped<EducationApiClient>();
builder.Services.AddScoped<ExperienceApiClient>();
builder.Services.AddScoped<ContactApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<SettingsApiClient>();
builder.Services.AddScoped<AnalyticsApiClient>();
builder.Services.AddScoped<ChatApiClient>();
builder.Services.AddScoped<ApiKeyApiClient>();
builder.Services.AddScoped<DependencyApiClient>();
builder.Services.AddScoped<HealthApiClient>();
builder.Services.AddScoped<CostApiClient>();
builder.Services.AddScoped<PublicBlogApiClient>();
builder.Services.AddScoped<PublicChatApiClient>();
builder.Services.AddScoped<PublicAiApiClient>();
builder.Services.AddScoped<PublicSiteApiClient>();

// Authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();

// Client-only services (no DB, no server resources)
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AdminLocalizationService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<TimezoneService>();

await builder.Build().RunAsync();
