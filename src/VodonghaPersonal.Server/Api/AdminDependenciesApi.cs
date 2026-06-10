using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminDependenciesApi
{
    public static void MapAdminDependenciesApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/dependencies").RequireAuthorization();

        group.MapGet("/", async (DependencyCheckService svc) =>
        {
            IReadOnlyList<DependencyInfo> all = await svc.GetAllAsync();
            List<DependencyDto> dtos = all.Select(d => new DependencyDto(
                d.Name, d.CurrentVersion, d.LatestVersion,
                d.Type.ToString(), d.RegistryUrl, d.Notes, d.Status.ToString()
            )).ToList();
            return Results.Ok(dtos);
        });

        group.MapPost("/invalidate", (DependencyCheckService svc) =>
        {
            svc.InvalidateCache();
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
