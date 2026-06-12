namespace VodonghaPersonal.Client.Components.Shared;

public partial class AdminFooter
{
    private string _version = "v3.0.3";

    protected override void OnInitialized()
    {
        Version? ver = typeof(AdminFooter).Assembly.GetName().Version;
        if (ver != null && ver.Major > 0)
        {
            _version = $"v{ver.Major}.{ver.Minor}.{ver.Build}";
        }
    }
}
