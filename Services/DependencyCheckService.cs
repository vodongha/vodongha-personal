using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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

    // Strip leading ^ ~ v for comparison
    private static string NormalizeVersion(string v) =>
        v.TrimStart('^', '~', 'v').Split('-')[0]; // ignore pre-release suffix
}

public class DependencyCheckService(IMemoryCache cache, IHttpClientFactory httpFactory, IWebHostEnvironment env)
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);
    private const string CacheKey = "dep_check_result";

    // CDN libraries hardcoded from App.razor / AdminLayout.razor
    private static readonly (string Name, string CurrentVersion, string NpmPackage, string? Notes)[] CdnLibraries =
    [
        ("Chart.js",         "4.5.1",  "chart.js",           null),
        ("Bootstrap Icons",  "1.13.1", "bootstrap-icons",    null),
        ("SortableJS",             "1.15.3", "sortablejs",                   null),
        ("Devicon",                "latest", "devicon",                      "No version pinned in CDN URL"),
    ];

    public async Task<IReadOnlyList<DependencyInfo>> GetAllAsync()
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyList<DependencyInfo>? cached) && cached is not null)
        {
            return cached;
        }

        using var http = httpFactory.CreateClient("deps");
        var tasks = new List<Task<DependencyInfo?>>();

        // NuGet
        foreach (var (name, version) in ReadNuGetPackages())
        {
            tasks.Add(CheckNuGetAsync(http, name, version));
        }

        // npm
        foreach (var (name, version) in ReadNpmPackages())
        {
            tasks.Add(CheckNpmAsync(http, name, version));
        }

        // CDN
        foreach (var (name, current, npmPackage, notes) in CdnLibraries)
        {
            tasks.Add(CheckCdnAsync(http, name, current, npmPackage, notes));
        }

        var results = await Task.WhenAll(tasks);
        var list = results.OfType<DependencyInfo>().OrderBy(d => d.Type).ThenBy(d => d.Name).ToList();

        cache.Set(CacheKey, (IReadOnlyList<DependencyInfo>)list, CacheTtl);
        return list;
    }

    public void InvalidateCache() => cache.Remove(CacheKey);

    // ── Parsers ──────────────────────────────────────────────────────────────

    private IEnumerable<(string Name, string Version)> ReadNuGetPackages()
    {
        var csproj = Directory.GetFiles(env.ContentRootPath, "*.csproj").FirstOrDefault();
        if (csproj is null)
        {
            yield break;
        }

        var doc = XDocument.Load(csproj);
        foreach (var el in doc.Descendants("PackageReference"))
        {
            var name = el.Attribute("Include")?.Value;
            var version = el.Attribute("Version")?.Value;
            if (name is not null && version is not null)
            {
                yield return (name, version);
            }
        }
    }

    private IEnumerable<(string Name, string Version)> ReadNpmPackages()
    {
        var packageJson = Path.Combine(env.ContentRootPath, "package.json");
        if (!File.Exists(packageJson))
        {
            yield break;
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(packageJson));
        var root = doc.RootElement;

        foreach (var section in new[] { "dependencies", "devDependencies" })
        {
            if (!root.TryGetProperty(section, out var deps))
            {
                continue;
            }

            foreach (var prop in deps.EnumerateObject())
            {
                yield return (prop.Name, prop.Value.GetString() ?? "");
            }
        }
    }

    // ── API calls ────────────────────────────────────────────────────────────

    private static async Task<DependencyInfo?> CheckNuGetAsync(HttpClient http, string name, string version)
    {
        try
        {
            var url = $"https://api.nuget.org/v3-flatcontainer/{name.ToLowerInvariant()}/index.json";
            var response = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var versions = doc.RootElement.GetProperty("versions")
                .EnumerateArray()
                .Select(v => v.GetString() ?? "")
                // exclude pre-release (anything with a hyphen after digits)
                .Where(v => !Regex.IsMatch(v, @"-"))
                .ToList();
            var latest = versions.LastOrDefault();
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
            // URL-encode scoped packages (@eslint/js → %40eslint%2Fjs)
            var encoded = Uri.EscapeDataString(name);
            var url = $"https://registry.npmjs.org/{encoded}/latest";
            var response = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var latest = doc.RootElement.GetProperty("version").GetString();
            return new DependencyInfo(name, version.TrimStart('^', '~'), latest, DependencyType.Npm,
                $"https://www.npmjs.com/package/{name}");
        }
        catch
        {
            return new DependencyInfo(name, version.TrimStart('^', '~'), null, DependencyType.Npm,
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
