using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace VodonghaPersonal.Services;

public enum DependencyType { NuGet, Npm, Cdn, GitHubActions }

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
    private static readonly SemaphoreSlim _lock = new(1, 1);

    // Hardcoded — .csproj / package.json are not in Docker published output (/app contains only DLLs).
    // IMPORTANT: these are the *current* versions shown on the Dependencies page. Keep them in sync
    // with the real versions whenever a package is bumped — updating the .csproj/package.json/workflow
    // alone does NOT update this page.
    // When adding a new NuGet package: add a row to NuGetPackages below.
    // When adding a new npm devDependency: add a row to NpmPackages below.
    // When adding a new CDN library: add a row to CdnLibraries below AND update the CDN URL in App.razor / AdminLayout.razor.
    private static readonly (string Name, string Version)[] NuGetPackages =
    [
        ("AspNetCore.SassCompiler",                                              "1.101.0"),
        ("libphonenumber-csharp",                                                "9.0.34"),
        ("Microsoft.AspNetCore.Components.QuickGrid",                            "10.0.10"),
        ("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore",              "10.0.10"),
        ("Microsoft.AspNetCore.SignalR.Client",                                  "10.0.10"),
        ("Microsoft.EntityFrameworkCore.Design",                                 "10.0.10"),
        ("Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore",   "10.0.10"),
        ("Npgsql.EntityFrameworkCore.PostgreSQL",                                "10.0.3"),
        ("QuestPDF",                                                             "2026.7.1"),
        ("Resend",                                                               "0.6.0"),
        ("SkiaSharp",                                                            "4.150.1"),
        ("WebPush",                                                              "1.0.13"),
    ];

    private static readonly (string Name, string Version)[] NpmPackages =
    [
        ("@eslint/js",                    "10.0.1"),
        ("eslint",                        "10.7.0"),
        ("stylelint",                     "17.14.0"),
        ("stylelint-config-standard-scss","17.0.0"),
    ];

    private static readonly (string Name, string Version, string NpmPackage, string? Notes)[] CdnLibraries =
    [
        ("Chart.js",        "4.5.1",  "chart.js",        null),
        ("Bootstrap Icons", "1.13.1", "bootstrap-icons", null),
        ("SortableJS",      "1.15.7", "sortablejs",      null),
        ("Devicon",         "2.17.0", "devicon",         null),
    ];

    // GitHub Actions pinned by major tag in .github/workflows — kept in sync with
    // what Dependabot (github-actions ecosystem) watches. Versions are compared by
    // major tag (e.g. "v5"), since that's how the workflows pin them.
    // superfly/flyctl-actions/setup-flyctl is intentionally omitted — it's pinned to
    // @master, so there is no version to track.
    private static readonly (string Repo, string CurrentMajor)[] GitHubActions =
    [
        ("actions/checkout",      "v7"),
        ("actions/setup-dotnet",  "v6"),
        ("actions/setup-node",    "v7"),
        ("actions/github-script", "v9"),
    ];

    public async Task<IReadOnlyList<DependencyInfo>> GetAllAsync()
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyList<DependencyInfo>? cached) && cached is not null)
        {
            return cached;
        }

        await _lock.WaitAsync();
        try
        {
            // Re-check after acquiring lock — another caller may have populated cache while we waited
            if (cache.TryGetValue(CacheKey, out cached) && cached is not null)
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

            foreach (var (repo, currentMajor) in GitHubActions)
            {
                tasks.Add(CheckGitHubActionAsync(http, repo, currentMajor));
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
        finally
        {
            _lock.Release();
        }
    }

    public void InvalidateCache() => cache.Remove(CacheKey);

    // ── API calls ────────────────────────────────────────────────────────────

    private static async Task<DependencyInfo?> CheckNuGetAsync(HttpClient http, string name, string version)
    {
        try
        {
            // NuGet Search API returns the canonical "latest stable" version shown on nuget.org,
            // avoiding edge cases where flat-container LastOrDefault() picks an anomalous version
            // (e.g. QuestPDF published 2202.8.2 which sorts after 2026.x.x by SemVer).
            var url = $"https://azuresearch-usnc.nuget.org/query?q=packageid:{Uri.EscapeDataString(name)}&prerelease=false&take=1";
            var response = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");
            var latest = data.GetArrayLength() > 0
                ? data[0].GetProperty("version").GetString()
                : null;
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

    private static async Task<DependencyInfo?> CheckGitHubActionAsync(HttpClient http, string repo, string currentMajor)
    {
        var registryUrl = $"https://github.com/{repo}";
        try
        {
            // Compare by major tag: the latest release's major (e.g. "v5.2.0" -> "v5")
            // against the major the workflows pin (e.g. "v5").
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.github.com/repos/{repo}/releases/latest");
            request.Headers.Add("User-Agent", "vodongha-personal-dep-check");
            request.Headers.Add("Accept", "application/vnd.github+json");

            using var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var tag = doc.RootElement.GetProperty("tag_name").GetString();

            string? latestMajor = null;
            if (!string.IsNullOrEmpty(tag))
            {
                var major = tag.TrimStart('v').Split('.')[0];
                latestMajor = string.IsNullOrEmpty(major) ? null : $"v{major}";
            }

            return new DependencyInfo(repo, currentMajor, latestMajor, DependencyType.GitHubActions, registryUrl);
        }
        catch
        {
            return new DependencyInfo(repo, currentMajor, null, DependencyType.GitHubActions, registryUrl);
        }
    }
}
