using Microsoft.AspNetCore.Components;

namespace VodonghaPersonal.Client.Components.Shared;

public partial class AdminBreadcrumb : ComponentBase
{
    private static readonly Dictionary<string, (string? Group, string Page)> _map = new()
    {
        ["/admin"] = (null, "Dashboard"),
        ["/admin/skills"] = ("Portfolio", "Skills"),
        ["/admin/projects"] = ("Portfolio", "Projects"),
        ["/admin/education"] = ("Portfolio", "Education"),
        ["/admin/experience"] = ("Portfolio", "Experience"),
        ["/admin/blog"] = ("Portfolio", "Blog"),
        ["/admin/cv"] = ("Portfolio", "CV"),
        ["/admin/contacts"] = ("Communication", "Messages"),
        ["/admin/chats"] = ("Communication", "Chats"),
        ["/admin/analytics"] = ("Insights", "Analytics"),
        ["/admin/health"] = ("Insights", "Health"),
        ["/admin/costs"] = ("Insights", "Costs"),
        ["/admin/api-keys"] = ("System", "API Keys"),
        ["/admin/profile"] = ("System", "Profile"),
        ["/admin/dependencies"] = ("System", "Dependencies"),
    };

    private string? _group;
    private string? _page;

    protected override void OnInitialized()
    {
        Uri uri = new(Nav.Uri);
        string path = uri.AbsolutePath.TrimEnd('/').ToLowerInvariant();
        if (_map.TryGetValue(path, out (string? Group, string Page) entry))
        {
            _group = entry.Group;
            _page = entry.Page;
        }
    }
}
