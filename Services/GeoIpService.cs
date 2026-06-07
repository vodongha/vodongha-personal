namespace vodongha.Services;

/// <summary>
/// Singleton — resolves a 2-letter ISO country code from an IP address
/// using ipinfo.io (free tier, no API key, HTTPS, 50k req/month).
/// </summary>
public class GeoIpService(IHttpClientFactory httpClientFactory)
{
    public async Task<string?> GetCountryCodeAsync(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)
            || ip == "::1"
            || ip.StartsWith("127.")
            || ip.StartsWith("192.168.")
            || ip.StartsWith("10."))
        {
            return null;
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient("geoip");
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(3));
            string result = await client.GetStringAsync($"https://ipinfo.io/{ip}/country", cts.Token);
            string code = result.Trim();
            return code.Length == 2 ? code.ToUpperInvariant() : null;
        }
        catch
        {
            return null;
        }
    }
}
