using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class AdminApiKeysApi
{
    public static void MapAdminApiKeysApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/api-keys").RequireAuthorization();

        group.MapGet("/", async (AppSecretsService secrets, IConfiguration config) =>
        {
            List<AppSecret> dbSecrets = await secrets.GetAllAsync();
            Dictionary<string, string> dbValues = dbSecrets.ToDictionary(s => s.Key, s => s.Value);

            Dictionary<string, string> envValues = [];
            foreach (AppSecretDefinition def in AppSecretsService.Definitions)
            {
                string? envVal = config[def.Key];
                if (!string.IsNullOrEmpty(envVal))
                {
                    envValues[def.Key] = envVal;
                }
            }

            List<ApiKeyDefinitionDto> defs = AppSecretsService.Definitions
                .Select(d => new ApiKeyDefinitionDto(d.Key, d.DisplayName, d.Description, d.Category, d.Sensitive))
                .ToList();

            return Results.Ok(new ApiKeysPageDto(defs, dbValues, envValues));
        });

        group.MapPost("/", async (ApiKeyDto req, AppSecretsService secrets) =>
        {
            bool ok = await secrets.SaveAsync(req.Key, req.Value);
            return ok ? Results.Ok() : Results.Problem("Save failed");
        }).DisableAntiforgery();

        group.MapDelete("/{key}", async (string key, AppSecretsService secrets) =>
        {
            await secrets.SaveAsync(key, "");
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapGet("/{key}/value", async (string key, AppSecretsService secrets) =>
        {
            string? val = await secrets.GetValueAsync(key);
            return Results.Ok(new { value = val ?? "" });
        });
    }
}
