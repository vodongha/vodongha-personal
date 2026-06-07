namespace vodongha.Services;

/// <summary>
/// Scoped service — captures the real client IP from the initial HTTP request.
/// Fly.io sets Fly-Client-IP; fallback to X-Forwarded-For then RemoteIpAddress.
/// </summary>
public class ClientIpService
{
    public string? IpAddress { get; }

    public ClientIpService(IHttpContextAccessor accessor)
    {
        HttpContext? ctx = accessor.HttpContext;
        if (ctx == null)
        {
            return;
        }

        // Fly.io proxy sets this header with the original client IP
        string? flyIp = ctx.Request.Headers["Fly-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(flyIp))
        {
            IpAddress = flyIp.Trim();
            return;
        }

        // Standard reverse-proxy header
        string? forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            IpAddress = forwarded.Split(',')[0].Trim();
            return;
        }

        IpAddress = ctx.Connection.RemoteIpAddress?.ToString();
    }
}
