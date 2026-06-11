using Microsoft.AspNetCore.Authentication;

namespace VodonghaPersonal.Api;

public static class PublicAuthApi
{
    public static void MapPublicAuthApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/state", async (HttpContext ctx) =>
        {
            AuthenticateResult result = await ctx.AuthenticateAsync();
            return Results.Ok(new { isAuthenticated = result.Succeeded });
        });
    }
}
