using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using vodongha.Data.Models;

namespace vodongha.Services;

public record CvData(
    string Name,
    string Title,
    string Email,
    string Phone,
    string Location,
    string GitHub,
    string LinkedIn,
    string Bio,
    string AvatarUrl,
    List<Skill> Skills,
    List<Experience> Experiences,
    List<Education> Educations,
    List<Project> Projects
);

public class CvPdfService
{
    // ── Colors ──────────────────────────────────────────────────────────────
    private static readonly string SidebarBg   = "#0f1923";
    private static readonly string AccentGreen = "#6ee7b7";
    private static readonly string SidebarText = "#cbd5e1";
    private static readonly string SidebarMuted= "#64748b";
    private static readonly string MainBg      = "#ffffff";
    private static readonly string MainText    = "#1e293b";
    private static readonly string MainMuted   = "#64748b";
    private static readonly string DividerLine = "#e2e8f0";
    private static readonly string TagBg       = "#f1f5f9";

    public byte[] Generate(CvData cv)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        IDocument doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(ts => ts.FontFamily("Arial").FontSize(9).FontColor(MainText));

                page.Content().Row(row =>
                {
                    // ── Left sidebar ────────────────────────────────────────
                    row.ConstantItem(175).Background(SidebarBg).Padding(20).Column(col =>
                    {
                        col.Spacing(16);

                        // Avatar placeholder / initials circle
                        col.Item().AlignCenter().Width(80).Height(80).Background(AccentGreen)
                            .Border(2).BorderColor(AccentGreen)
                            .Element(e => e.AlignCenter().AlignMiddle()
                                .Text(Initials(cv.Name))
                                .FontSize(26).Bold().FontColor(SidebarBg));

                        // Name + Title
                        col.Item().AlignCenter().Column(c =>
                        {
                            c.Item().AlignCenter().Text(cv.Name)
                                .Bold().FontSize(13).FontColor("#ffffff");
                            if (!string.IsNullOrEmpty(cv.Title))
                            {
                                c.Item().AlignCenter().Text(cv.Title)
                                    .FontSize(9).FontColor(AccentGreen).Italic();
                            }
                        });

                        // Divider
                        col.Item().Height(1).Background("#1e3a2f");

                        // Contact
                        SidebarSection(col, "CONTACT");
                        if (!string.IsNullOrEmpty(cv.Email))
                        {
                            col.Item().Row(r =>
                            {
                                r.ConstantItem(14).Text("✉").FontSize(8).FontColor(AccentGreen);
                                r.RelativeItem().Text(cv.Email).FontSize(8).FontColor(SidebarText);
                            });
                        }
                        if (!string.IsNullOrEmpty(cv.Phone))
                        {
                            col.Item().Row(r =>
                            {
                                r.ConstantItem(14).Text("📞").FontSize(8).FontColor(AccentGreen);
                                r.RelativeItem().Text(cv.Phone).FontSize(8).FontColor(SidebarText);
                            });
                        }
                        if (!string.IsNullOrEmpty(cv.Location))
                        {
                            col.Item().Row(r =>
                            {
                                r.ConstantItem(14).Text("📍").FontSize(8).FontColor(AccentGreen);
                                r.RelativeItem().Text(cv.Location).FontSize(8).FontColor(SidebarText);
                            });
                        }
                        if (!string.IsNullOrEmpty(cv.GitHub))
                        {
                            col.Item().Row(r =>
                            {
                                r.ConstantItem(14).Text("⌥").FontSize(8).FontColor(AccentGreen);
                                r.RelativeItem().Text(cv.GitHub.Replace("https://github.com/", "github.com/"))
                                    .FontSize(8).FontColor(SidebarText);
                            });
                        }
                        if (!string.IsNullOrEmpty(cv.LinkedIn))
                        {
                            col.Item().Row(r =>
                            {
                                r.ConstantItem(14).Text("in").FontSize(8).Bold().FontColor(AccentGreen);
                                r.RelativeItem().Text(cv.LinkedIn.Replace("https://linkedin.com/in/", "linkedin.com/in/")
                                    .Replace("https://www.linkedin.com/in/", "linkedin.com/in/"))
                                    .FontSize(8).FontColor(SidebarText);
                            });
                        }

                        // Skills by category
                        if (cv.Skills.Count > 0)
                        {
                            col.Item().Height(1).Background("#1e3a2f");
                            SidebarSection(col, "SKILLS");

                            foreach (IGrouping<string, Skill> group in cv.Skills.GroupBy(s => s.Category))
                            {
                                col.Item().Text(group.Key.ToUpper())
                                    .FontSize(7).Bold().FontColor(AccentGreen).LetterSpacing(0.05f);

                                col.Item().Row(r =>
                                {
                                    foreach (Skill skill in group)
                                    {
                                        r.AutoItem().Padding(2)
                                            .Background("#1a2a3a").Border(1).BorderColor("#2d4a5a")
                                            .PaddingHorizontal(5).PaddingVertical(2)
                                            .Text(skill.Name).FontSize(7).FontColor(SidebarText);
                                    }
                                });
                            }
                        }
                    });

                    // ── Right main content ───────────────────────────────────
                    row.RelativeItem().Background(MainBg).Padding(28).Column(col =>
                    {
                        col.Spacing(18);

                        // Bio / Summary
                        if (!string.IsNullOrWhiteSpace(cv.Bio))
                        {
                            MainSection(col, "PROFILE");
                            col.Item().Text(cv.Bio).FontSize(9).FontColor(MainMuted).LineHeight(1.5f);
                        }

                        // Experience
                        if (cv.Experiences.Count > 0)
                        {
                            MainSection(col, "EXPERIENCE");
                            col.Item().Column(expCol =>
                            {
                                expCol.Spacing(12);
                                foreach (Experience exp in cv.Experiences.OrderBy(e => e.Order))
                                {
                                    expCol.Item().Column(c =>
                                    {
                                        // Role + date range on same row
                                        c.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text(t =>
                                            {
                                                t.Span(exp.Role).Bold().FontSize(10).FontColor(MainText);
                                            });
                                            r.AutoItem().Text(DateRange(exp.StartYear, exp.StartMonth, exp.EndYear, exp.EndMonth, exp.IsCurrent))
                                                .FontSize(8).FontColor(MainMuted).Italic();
                                        });
                                        // Company + location
                                        c.Item().Text(t =>
                                        {
                                            t.Span(exp.Company).FontColor(AccentGreen).FontSize(9).Bold();
                                            if (!string.IsNullOrEmpty(exp.Location))
                                            {
                                                t.Span($"  ·  {exp.Location}").FontSize(8).FontColor(MainMuted);
                                            }
                                        });
                                        // Description
                                        string desc = !string.IsNullOrWhiteSpace(exp.DescriptionEn)
                                            ? exp.DescriptionEn
                                            : exp.Description ?? "";
                                        if (!string.IsNullOrWhiteSpace(desc))
                                        {
                                            c.Item().PaddingTop(3).Text(desc)
                                                .FontSize(8.5f).FontColor(MainMuted).LineHeight(1.5f);
                                        }
                                    });
                                    expCol.Item().Height(0.5f).Background(DividerLine);
                                }
                            });
                        }

                        // Education
                        if (cv.Educations.Count > 0)
                        {
                            MainSection(col, "EDUCATION");
                            col.Item().Column(eduCol =>
                            {
                                eduCol.Spacing(10);
                                foreach (Education edu in cv.Educations.OrderBy(e => e.Order))
                                {
                                    eduCol.Item().Column(c =>
                                    {
                                        c.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text(t =>
                                            {
                                                t.Span(edu.Degree).Bold().FontSize(10).FontColor(MainText);
                                                if (!string.IsNullOrEmpty(edu.Field))
                                                {
                                                    t.Span($" — {edu.Field}").FontSize(9).FontColor(MainMuted);
                                                }
                                            });
                                            r.AutoItem().Text($"{edu.StartYear} – {edu.EndYear?.ToString() ?? "Present"}")
                                                .FontSize(8).FontColor(MainMuted).Italic();
                                        });
                                        c.Item().Text(edu.School).FontColor(AccentGreen).FontSize(9).Bold();
                                        string desc = !string.IsNullOrWhiteSpace(edu.DescriptionEn)
                                            ? edu.DescriptionEn : edu.Description ?? "";
                                        if (!string.IsNullOrWhiteSpace(desc))
                                        {
                                            c.Item().PaddingTop(2).Text(desc)
                                                .FontSize(8.5f).FontColor(MainMuted).LineHeight(1.5f);
                                        }
                                    });
                                }
                            });
                        }

                        // Projects (featured only, max 4)
                        List<Project> featured = cv.Projects
                            .Where(p => p.IsFeatured)
                            .OrderBy(p => p.Order)
                            .Take(4)
                            .ToList();

                        if (featured.Count > 0)
                        {
                            MainSection(col, "FEATURED PROJECTS");
                            col.Item().Column(projCol =>
                            {
                                projCol.Spacing(10);
                                foreach (Project proj in featured)
                                {
                                    projCol.Item().Column(c =>
                                    {
                                        c.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text(proj.Title).Bold().FontSize(10).FontColor(MainText);
                                            if (!string.IsNullOrEmpty(proj.LiveUrl))
                                            {
                                                r.AutoItem().Text(proj.LiveUrl).FontSize(7).FontColor(AccentGreen);
                                            }
                                        });
                                        string desc = !string.IsNullOrWhiteSpace(proj.DescriptionEn)
                                            ? proj.DescriptionEn : proj.Description;
                                        if (!string.IsNullOrWhiteSpace(desc))
                                        {
                                            c.Item().PaddingTop(2).Text(desc)
                                                .FontSize(8.5f).FontColor(MainMuted).LineHeight(1.5f);
                                        }
                                        if (!string.IsNullOrEmpty(proj.Technologies))
                                        {
                                            c.Item().PaddingTop(3).Text(proj.Technologies)
                                                .FontSize(7.5f).FontColor(AccentGreen).Italic();
                                        }
                                    });
                                    projCol.Item().Height(0.5f).Background(DividerLine);
                                }
                            });
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void SidebarSection(ColumnDescriptor col, string title)
    {
        col.Item().Text(title)
            .FontSize(7.5f).Bold().FontColor(AccentGreen).LetterSpacing(0.08f);
    }

    private static void MainSection(ColumnDescriptor col, string title)
    {
        col.Item().Column(c =>
        {
            c.Item().Text(title)
                .FontSize(8).Bold().FontColor(AccentGreen).LetterSpacing(0.1f);
            c.Item().Height(1.5f).Background(AccentGreen);
        });
    }

    private static string Initials(string name)
    {
        string[] parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
            : name.Length > 0 ? name[..Math.Min(2, name.Length)].ToUpper() : "?";
    }

    private static string DateRange(int startYear, int startMonth, int? endYear, int? endMonth, bool isCurrent)
    {
        string start = $"{MonthShort(startMonth)} {startYear}";
        string end = isCurrent ? "Present" : endYear.HasValue
            ? $"{MonthShort(endMonth ?? 1)} {endYear}"
            : "Present";
        return $"{start} – {end}";
    }

    private static string MonthShort(int month) => month switch
    {
        1 => "Jan", 2 => "Feb", 3 => "Mar", 4 => "Apr",
        5 => "May", 6 => "Jun", 7 => "Jul", 8 => "Aug",
        9 => "Sep", 10 => "Oct", 11 => "Nov", 12 => "Dec",
        _ => ""
    };
}
