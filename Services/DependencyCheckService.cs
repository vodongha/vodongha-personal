using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace VodonghaPersonal.Services;

public enum DependencyType { NuGet, Npm, Cdn }

public enum DependencyStatus { UpToDate, Outdated, Unknown }

public record DependencyInfo(
    string Name,
    string CurrentVersion,
    string? LatestVersion,
    DependencyType Type,
    string RegistryUrl,
    string? Notes = null)
{
    public DependencyStatus Status => LatestVersion is null
        ? DependencyStatus.Unknown
        : NormalizeVersion(CurrentVersion) == NormalizeVersion(LatestVersion)
            ? DependencyStatus.UpToDate
            : DependencyStatus.Outdated;

    private static string NormalizeVersion(string v) =>
        v.TrimStart('^', '~', 'v').Split('-')[0];
}

public class DependencyCheckService(IMemoryCache cache, IHttpClientFactory httpFactory)
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);
    private const string CacheKey = "dep_check_result";

    // Hardcoded — .csproj / package.json are not in Docker published output (/app contains only DLLs).
    // When adding a new NuGet package: add a row to NuGetPackages below.
    // When adding a new npm devDependency: add a row to NpmPackages below.
    // When adding a new CDN library: add a row to CdnLibraries below AND update the CDN URL in App.razor / AdminLayout.razor.
    private static readonly (string Name, string Version)[] NuGetPackages =
    [
        ("AspNetCore.SassCompiler",                                              "1.100.0"),
        ("libphonenumber-csharp",                                                "9.0.32"),
        ("Microsoft.AspNetCore.Components.QuickGrid",                            "10.0.9"),
        ("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore",              "10.0.9"),
        ("Microsoft.AspNetCore.SignalR.Client",                                  "10.0.9"),
        ("Microsoft.EntityFrameworkCore.Design",                                 "10.0.9"),
        ("Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore",   "10.0.9"),
        ("Npgsql.EntityFrameworkCore.PostgreSQL",                                "10.0.2"),
        ("QuestPDF",                                                             "2026.5.0"),
        ("Resend",                                                               "0.5.1"),
        ("SkiaSharp",                                                            "3.119.4"),
        ("WebPush",                                                              "1.0.13"),
    ];

    private static readonly (string Name, string Version)[] NpmPackages =
    [
        ("@eslint/js",                    "10.0.1"),
        ("eslint",                        "10.4.1"),
        ("stylelint",                     "17.13.0"),
        ("stylelint-config-standard-scss","17.0.0"),
    ];

    private static readonly (string Name, string Version, string NpmPackage, string? Notes)[] CdnLibraries =
    [
        ("Chart.js",        "4.5.1",  "chart.js",        null),
        ("Bootstrap Icons", "1.13.1", "bootstrap-icons", null),
        ("SortableJS",      "1.15.7", "sortablejs",      null),
        ("Devicon",         "2.17.0", "devicon",         null),
    ];

    public async Task<IReadOnlyList<DependencyInfo>> GetAllAsync()
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyList<DependencyInfo>? cached) && cached is not null)
        {
            return cached;
        }

        using var http = httpFactory.CreateClient("deps");
        var tasks = new List<Task<DependencyInfo?>>();

        foreach (var (name, version) in NuGetPackages)
        {
            tasks.Add(CheckNuGetAsync(http, name, version));
        }

        foreach (var (name, version) in NpmPackages)
        {
            tasks.Add(CheckNpmAsync(http, name, version));
        }

        foreach (var (name, current, npmPackage, notes) in CdnLibraries)
        {
            tasks.Add(CheckCdnAsync(http, name, current, npmPackage, notes));
        }

        var results = await Task.WhenAll(tasks);
        var list = results.OfType<DependencyInfo>()
            .OrderBy(d => d.Type)
            .ThenBy(d => d.Status == DependencyStatus.Outdated ? 0 : d.Status == DependencyStatus.Unknown ? 1 : 2)
            .ThenBy(d => d.Name)
            .ToList();

        cache.Set(CacheKey, (IReadOnlyList<DependencyInfo>)list, CacheTtl);
        return list;
    }

    public void InvalidateCache() => cache.Remove(CacheKey);

    // ── API calls ────────────────────────────────────────────────────────────

    private static async Task<DependencyInfo?> CheckNuGetAsync(HttpClient http, string name, string version)
    {
        try
        {
            var url = $"https://api.nuget.org/v3-flatcontainer/{name.ToLowerInvariant()}/index.json";
            var response = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var latest = doc.RootElement.GetProperty("versions")
                .EnumerateArray()
                .Select(v => v.GetString() ?? "")
                .Where(v => !Regex.IsMatch(v, @"-"))
                .LastOrDefault();
            return new DependencyInfo(name, version, latest, DependencyType.NuGet,
                $"https://www.nuget.org/packages/{name}");
        }
        catch
        {
            return new DependencyInfo(name, version, null, DependencyType.NuGet,
                $"https://www.nuget.org/packages/{name}");
        }
    }

    private static async Task<DependencyInfo?> CheckNpmAsync(HttpClient http, string name, string version)
    {
        try
        {
            var encoded = Uri.EscapeDataString(name);
            var url = $"https://registry.npmjs.org/{encoded}/latest";
            var response = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var latest = doc.RootElement.GetProperty("version").GetString();
            return new DependencyInfo(name, version, latest, DependencyType.Npm,
                $"https://www.npmjs.com/package/{name}");
        }
        catch
        {
            return new DependencyInfo(name, version, null, DependencyType.Npm,
                $"https://www.npmjs.com/package/{name}");
        }
    }

    private static async Task<DependencyInfo?> CheckCdnAsync(
        HttpClient http, string displayName, string currentVersion, string npmPackage, string? notes)
    {
        if (currentVersion == "latest")
        {
            return new DependencyInfo(displayName, "latest", null, DependencyType.Cdn,
                $"https://www.npmjs.com/package/{npmPackage}", notes);
        }

        try
        {
            var encoded = Uri.EscapeDataString(npmPackage);
            var url = $"https://data.jsdelivr.com/v1/package/npm/{encoded}";
            var response = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var latest = doc.RootElement.GetProperty("tags").GetProperty("latest").GetString();
            return new DependencyInfo(displayName, currentVersion, latest, DependencyType.Cdn,
                $"https://www.npmjs.com/package/{npmPackage}", notes);
        }
        catch
        {
            return new DependencyInfo(displayName, currentVersion, null, DependencyType.Cdn,
                $"https://www.npmjs.com/package/{npmPackage}", notes);
        }
    }
}
