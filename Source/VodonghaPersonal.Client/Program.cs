using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
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
builder.Services.AddScoped<GitHubVersionService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AdminLocalizationService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<TimezoneService>();

WebAssemblyHost host = builder.Build();

// The SSR sections are rendered in the cookie's language by the server, but the WASM
// LanguageService starts fresh (defaults to "en"). Sync it from the same `lang` cookie so
// interactive components (NavBar, Footer, Chat/AI widgets, Blog) match — and so the toggle
// computes the correct opposite language instead of always switching to Vietnamese.
try
{
    IJSRuntime js = host.Services.GetRequiredService<IJSRuntime>();
    string lang = await js.InvokeAsync<string>("getLangCookie");
    if (lang is "vi" or "en")
    {
        host.Services.GetRequiredService<LanguageService>().Set(lang);
    }
}
catch { /* non-critical — fall back to default language */ }

await host.RunAsync();
