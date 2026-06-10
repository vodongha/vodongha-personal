using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class CvCacheService(
    IWebHostEnvironment env,
    ILogger<CvCacheService> logger,
    IServiceProvider serviceProvider)
{
    private const int TemplateCount = 3;

    private string CacheDir => Path.Combine(env.WebRootPath, "uploads", "cv");
    public string FilePath(int template) => Path.Combine(CacheDir, $"cv-t{template}.pdf");

    public async Task<byte[]?> ReadAsync(int template)
    {
        string path = FilePath(template);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return await File.ReadAllBytesAsync(path);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "CV cache: failed to read template {T}", template);
            return null;
        }
    }

    public async Task WriteAsync(int template, byte[] pdf)
    {
        Directory.CreateDirectory(CacheDir);
        try
        {
            await File.WriteAllBytesAsync(FilePath(template), pdf);
            logger.LogInformation("CV cache: saved template {T} ({Bytes} bytes)", template, pdf.Length);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "CV cache: failed to write template {T}", template);
        }
    }

    /// <summary>
    /// Deletes all cached PDFs, then kicks off background regeneration for all templates.
    /// Returns immediately — does not block the caller.
    /// </summary>
    public void InvalidateAndRegenerate()
    {
        for (int t = 0; t < TemplateCount; t++)
        {
            string path = FilePath(t);
            if (File.Exists(path))
            {
                try { File.Delete(path); }
                catch (Exception ex) { logger.LogWarning(ex, "CV cache: failed to delete template {T}", t); }
            }
        }

        for (int t = 0; t < TemplateCount; t++)
        {
            int template = t;
            _ = Task.Run(async () =>
            {
                try
                {
                    logger.LogInformation("CV cache: background regen started for template {T}", template);
                    byte[] pdf = await BuildPdfAsync(template);
                    await WriteAsync(template, pdf);
                    logger.LogInformation("CV cache: background regen done for template {T}", template);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "CV cache: background regen failed for template {T}", template);
                }
            });
        }
    }

    public async Task<byte[]> BuildPdfAsync(int template)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IServiceProvider svc = scope.ServiceProvider;
        IDbContextFactory<AppDbContext> dbFactory = svc.GetRequiredService<IDbContextFactory<AppDbContext>>();
        SiteSettingService settingsSvc = svc.GetRequiredService<SiteSettingService>();
        CvPdfService cvPdf = svc.GetRequiredService<CvPdfService>();

        // EF Core does not allow concurrent operations on the same DbContext instance.
        // Run each query sequentially; settings uses its own internal context so it's fine in parallel.
        Task<Dictionary<string, string>> settingsTask = settingsSvc.GetAllAsync();
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<Skill> skills = await db.Skills.OrderBy(s => s.Order).ToListAsync();
        List<Experience> experiences = await db.Experiences.OrderBy(e => e.Order).ToListAsync();
        List<Education> educations = await db.Educations.OrderBy(e => e.Order).ToListAsync();
        List<Project> projects = await db.Projects.OrderBy(p => p.Order).ToListAsync();

        Dictionary<string, string> settings = await settingsTask;
        CvData data = new(
            Name: settings.GetValueOrDefault("Name", ""),
            Title: settings.GetValueOrDefault("Title", ""),
            Email: settings.GetValueOrDefault("Email", ""),
            Phone: settings.GetValueOrDefault("Phone", ""),
            Location: settings.GetValueOrDefault("Location", ""),
            GitHub: settings.GetValueOrDefault("GitHub", ""),
            LinkedIn: settings.GetValueOrDefault("LinkedIn", ""),
            Bio: settings.GetValueOrDefault("BioEn", settings.GetValueOrDefault("Bio", "")),
            AvatarUrl: settings.GetValueOrDefault("AvatarUrl", ""),
            Skills: skills,
            Experiences: experiences,
            Educations: educations,
            Projects: projects
        );

        byte[]? avatarBytes = null;
        if (!string.IsNullOrEmpty(data.AvatarUrl))
        {
            try
            {
                if (!data.AvatarUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string filePath = Path.Combine(env.WebRootPath, data.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(filePath))
                    {
                        avatarBytes = await File.ReadAllBytesAsync(filePath);
                    }
                }
                else
                {
                    using HttpClient http = new() { Timeout = TimeSpan.FromSeconds(5) };
                    avatarBytes = await http.GetByteArrayAsync(data.AvatarUrl);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("CV: could not load avatar: {Msg}", ex.Message);
            }
        }

        logger.LogInformation("CV: generating PDF for {Name}, template={T}", data.Name, template);
        byte[] pdf = await Task.Run(() => cvPdf.Generate(data, template, avatarBytes));
        logger.LogInformation("CV: generated {Bytes} bytes for template {T}", pdf.Length, template);
        return pdf;
    }
}
