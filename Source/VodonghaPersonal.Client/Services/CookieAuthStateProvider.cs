using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace VodonghaPersonal.Client.Services;

public class CookieAuthStateProvider(HttpClient http) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            AuthStateResponse? resp = await http.GetFromJsonAsync<AuthStateResponse>("/api/auth/state");
            if (resp is { IsAuthenticated: true })
            {
                ClaimsIdentity identity = new(authenticationType: "cookie");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }
        catch
        {
            // Network error or server unavailable — treat as anonymous
        }

        return Anonymous;
    }

    private record AuthStateResponse(bool IsAuthenticated);
}
