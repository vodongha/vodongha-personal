using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminSettingsApi
{
    public static void MapAdminSettingsApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/settings").RequireAuthorization();

        group.MapGet("/", async (SiteSettingService svc) =>
        {
            Dictionary<string, string> settings = await svc.GetAllAsync();
            return Results.Ok(settings);
        });

        group.MapPost("/", async (SettingsSaveRequest req, SiteSettingService svc) =>
        {
            await svc.SaveAllAsync(req.Values);
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapPost("/avatar", async (HttpRequest request, IWebHostEnvironment env) =>
        {
            IFormFile? file = request.Form.Files.GetFile("file");
            if (file is null) { return Results.BadRequest("No file"); }

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            string fileName = $"avatar{ext}";
            string uploadsDir = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);
            string filePath = Path.Combine(uploadsDir, fileName);

            await using FileStream fs = new(filePath, FileMode.Create);
            await file.CopyToAsync(fs);

            return Results.Ok(new { url = $"/uploads/{fileName}" });
        }).DisableAntiforgery();
    }
}
