using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class CvPdfService
{
    // ── Shared colors ────────────────────────────────────────────────────────
    private static readonly string MainText = "#1e293b";
    private static readonly string MainMuted = "#64748b";
    private static readonly string DividerLine = "#e2e8f0";

    // ── Template 0 — Dark Sidebar ────────────────────────────────────────────
    private static readonly string SidebarBg = "#0f1923";
    private static readonly string AccentGreen = "#6ee7b7";
    private static readonly string SidebarText = "#cbd5e1";

    // ── Template 1 — Minimal ────────────────────────────────────────────────
    private static readonly string MinimalAccent = "#059669";
    private static readonly string MinimalMuted = "#6b7280";
    private static readonly string MinimalSideBg = "#f8fafc";

    // ── Template 2 — Professional ───────────────────────────────────────────
    private static readonly string ProHeader = "#1e3a5f";
    private static readonly string ProAccent = "#3b82f6";
    private static readonly string ProLight = "#eff6ff";

    // ── Dispatch ─────────────────────────────────────────────────────────────
    public byte[] Generate(CvData cv, int template = 0, byte[]? avatarBytes = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // Pre-crop avatar to square (center-horizontal, top-vertical) so
        // QuestPDF FitArea() fills the circle cleanly without letterboxing.
        byte[]? avatar = avatarBytes != null ? CropSquareTop(avatarBytes) : null;

        return template switch
        {
            1 => GenerateMinimal(cv, avatar),
            2 => GenerateProfessional(cv, avatar),
            _ => GenerateDarkSidebar(cv, avatar),
        };
    }

    /// <summary>Crops image bytes to a square, centered horizontally and anchored to top vertically.</summary>
    private static byte[] CropSquareTop(byte[] imageBytes)
    {
        try
        {
            using SKBitmap src = SKBitmap.Decode(imageBytes);
            int size = Math.Min(src.Width, src.Height);
            int x = (src.Width - size) / 2;   // center horizontally
            int y = 0;                          // anchor to top (show face)

            using SKBitmap cropped = new(size, size);
            src.ExtractSubset(cropped, new SKRectI(x, y, x + size, y + size));

            using SKImage img = SKImage.FromBitmap(cropped);
            using SKData data = img.Encode(SKEncodedImageFormat.Jpeg, 90);
            return data.ToArray();
        }
        catch
        {
            return imageBytes; // fallback: pass original if decode fails
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Template 0 — Dark Sidebar
    // ════════════════════════════════════════════════════════════════════════
    private static byte[] GenerateDarkSidebar(CvData cv, byte[]? avatarBytes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(ts => ts.FontFamily("Noto Sans").FontSize(9).FontColor(MainText));

                page.Content().Row(row =>
                {
                    // ── Sidebar ──────────────────────────────────────────────
                    row.ConstantItem(175).Background(SidebarBg).Padding(20).Column(col =>
                    {
                        col.Spacing(16);

                        // Avatar
                        if (avatarBytes != null && avatarBytes.Length > 0)
                        {
                            col.Item().AlignCenter().Width(80).Height(80)
                                .CornerRadius(40).AlignCenter().AlignMiddle()
                                .Image(avatarBytes).FitArea();
                        }
                        else
                        {
                            col.Item().AlignCenter().Width(80).Height(80)
                                .CornerRadius(40).Background(AccentGreen)
                                .AlignCenter().AlignMiddle()
                                .Text(Initials(cv.Name)).FontSize(26).Bold().FontColor(SidebarBg);
                        }

                        // Name + Title
                        col.Item().AlignCenter().Column(c =>
                        {
                            c.Item().AlignCenter().Text(cv.Name).Bold().FontSize(13).FontColor("#ffffff");
                            if (!string.IsNullOrEmpty(cv.Title))
                            {
                                c.Item().AlignCenter().Text(cv.Title).FontSize(9).FontColor(AccentGreen).Italic();
                            }
                        });

                        col.Item().Height(1).Background("#1e3a2f");

                        // Contact
                        SidebarLabel(col, "CONTACT", AccentGreen);
                        if (!string.IsNullOrEmpty(cv.Email)) { SidebarContactRow(col, "Email", cv.Email, SidebarText, AccentGreen); }
                        if (!string.IsNullOrEmpty(cv.Phone)) { SidebarContactRow(col, "Tel", cv.Phone, SidebarText, AccentGreen); }
                        if (!string.IsNullOrEmpty(cv.Location)) { SidebarContactRow(col, "Loc", cv.Location, SidebarText, AccentGreen); }
                        if (!string.IsNullOrEmpty(cv.GitHub)) { SidebarContactRow(col, "Git", cv.GitHub.Replace("https://github.com/", "github.com/"), SidebarText, AccentGreen); }
                        if (!string.IsNullOrEmpty(cv.LinkedIn)) { SidebarContactRow(col, "in", cv.LinkedIn.Replace("https://linkedin.com/in/", "linkedin.com/in/").Replace("https://www.linkedin.com/in/", "linkedin.com/in/"), SidebarText, AccentGreen); }

                        // Skills
                        if (cv.Skills.Count > 0)
                        {
                            col.Item().Height(1).Background("#1e3a2f");
                            SidebarLabel(col, "SKILLS", AccentGreen);
                            foreach (IGrouping<string, Skill> group in cv.Skills.GroupBy(s => s.Category))
                            {
                                col.Item().Text(group.Key.ToUpper()).FontSize(7).Bold().FontColor(AccentGreen).LetterSpacing(0.05f);
                                col.Item().Inlined(il =>
                                {
                                    il.Spacing(3);
                                    foreach (Skill skill in group)
                                    {
                                        il.Item().Padding(2).Background("#1a2a3a").Border(1).BorderColor("#2d4a5a")
                                            .PaddingHorizontal(5).PaddingVertical(2)
                                            .Text(skill.Name).FontSize(7).FontColor(SidebarText);
                                    }
                                });
                            }
                        }
                    });

                    // ── Main content ─────────────────────────────────────────
                    row.RelativeItem().Background("#ffffff").Padding(28).Column(col =>
                    {
                        col.Spacing(18);
                        if (!string.IsNullOrWhiteSpace(cv.Bio))
                        {
                            MainSectionTitle(col, "PROFILE", AccentGreen);
                            col.Item().Text(cv.Bio).FontSize(9).FontColor(MainMuted).LineHeight(1.5f);
                        }
                        BuildExperience(col, cv, AccentGreen);
                        BuildEducation(col, cv, AccentGreen);
                        BuildProjects(col, cv, AccentGreen);
                    });
                });
            });
        }).GeneratePdf();
    }

    // ════════════════════════════════════════════════════════════════════════
    // Template 1 — Minimal (clean white, two-column, green accent)
    // ════════════════════════════════════════════════════════════════════════
    private static byte[] GenerateMinimal(CvData cv, byte[]? avatarBytes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(ts => ts.FontFamily("Noto Sans").FontSize(9).FontColor(MainText));

                page.Content().Column(root =>
                {
                    // ── Name header ──────────────────────────────────────────
                    root.Item().Background("#ffffff").PaddingHorizontal(32).PaddingTop(30).PaddingBottom(12).Column(h =>
                    {
                        h.Item().Row(r =>
                        {
                            // Avatar — use AutoItem + explicit Width/Height so the circle is
                            // always a perfect square and FitArea() fills it without left-offset
                            if (avatarBytes != null && avatarBytes.Length > 0)
                            {
                                r.AutoItem().AlignMiddle().Width(64).Height(64)
                                    .CornerRadius(32).AlignCenter().AlignMiddle()
                                    .Image(avatarBytes).FitArea();
                            }
                            else
                            {
                                r.AutoItem().AlignMiddle().Width(64).Height(64)
                                    .CornerRadius(32).Background(MinimalAccent)
                                    .AlignCenter().AlignMiddle()
                                    .Text(Initials(cv.Name)).FontSize(22).Bold().FontColor("#ffffff");
                            }
                            r.ConstantItem(16); // spacer
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text(cv.Name).Bold().FontSize(20).FontColor(MainText);
                                if (!string.IsNullOrEmpty(cv.Title))
                                {
                                    c.Item().Text(cv.Title).FontSize(10).FontColor(MinimalAccent).Italic();
                                }
                                c.Item().PaddingTop(6).Row(cr =>
                                {
                                    if (!string.IsNullOrEmpty(cv.Email)) { InlineContact(cr, cv.Email, MinimalMuted); }
                                    if (!string.IsNullOrEmpty(cv.Phone)) { InlineContact(cr, cv.Phone, MinimalMuted); }
                                    if (!string.IsNullOrEmpty(cv.Location)) { InlineContact(cr, cv.Location, MinimalMuted); }
                                });
                                c.Item().PaddingTop(2).Row(cr =>
                                {
                                    if (!string.IsNullOrEmpty(cv.GitHub)) { InlineContact(cr, cv.GitHub.Replace("https://github.com/", "github.com/"), MinimalAccent); }
                                    if (!string.IsNullOrEmpty(cv.LinkedIn)) { InlineContact(cr, cv.LinkedIn.Replace("https://www.linkedin.com/in/", "linkedin.com/in/").Replace("https://linkedin.com/in/", "linkedin.com/in/"), MinimalAccent); }
                                });
                            });
                        });
                        h.Item().PaddingTop(14).Height(2).Background(MinimalAccent);
                    });

                    // ── Body: left skills + right content ────────────────────
                    root.Item().Row(body =>
                    {
                        // Left: skills
                        body.ConstantItem(160).Background(MinimalSideBg).Padding(20).Column(col =>
                        {
                            col.Spacing(14);
                            if (cv.Skills.Count > 0)
                            {
                                foreach (IGrouping<string, Skill> group in cv.Skills.GroupBy(s => s.Category))
                                {
                                    col.Item().Text(group.Key.ToUpper()).FontSize(7).Bold().FontColor(MinimalAccent).LetterSpacing(0.06f);
                                    col.Item().Inlined(il =>
                                    {
                                        il.Spacing(3);
                                        foreach (Skill skill in group)
                                        {
                                            il.Item().Border(1).BorderColor("#d1fae5").Background("#ecfdf5")
                                                .PaddingHorizontal(5).PaddingVertical(2)
                                                .Text(skill.Name).FontSize(7).FontColor("#065f46");
                                        }
                                    });
                                }
                            }
                        });

                        // Right: main content
                        body.RelativeItem().Background("#ffffff").Padding(24).Column(col =>
                        {
                            col.Spacing(16);
                            if (!string.IsNullOrWhiteSpace(cv.Bio))
                            {
                                MinimalSectionTitle(col, "PROFILE");
                                col.Item().Text(cv.Bio).FontSize(9).FontColor(MainMuted).LineHeight(1.5f);
                            }
                            BuildExperience(col, cv, MinimalAccent);
                            BuildEducation(col, cv, MinimalAccent);
                            BuildProjects(col, cv, MinimalAccent);
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    // ════════════════════════════════════════════════════════════════════════
    // Template 2 — Professional (navy header band, clean single-column)
    // ════════════════════════════════════════════════════════════════════════
    private static byte[] GenerateProfessional(CvData cv, byte[]? avatarBytes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(ts => ts.FontFamily("Noto Sans").FontSize(9).FontColor(MainText));

                page.Content().Column(root =>
                {
                    // ── Full-width navy header ────────────────────────────────
                    root.Item().Background(ProHeader).PaddingHorizontal(30).PaddingVertical(22).Row(h =>
                    {
                        // Avatar — AutoItem + explicit square so CornerRadius circle stays centered
                        if (avatarBytes != null && avatarBytes.Length > 0)
                        {
                            h.AutoItem().AlignMiddle().Width(72).Height(72)
                                .CornerRadius(36).AlignCenter().AlignMiddle()
                                .Image(avatarBytes).FitArea();
                        }
                        else
                        {
                            h.AutoItem().AlignMiddle().Width(72).Height(72)
                                .CornerRadius(36).Background(ProAccent)
                                .AlignCenter().AlignMiddle()
                                .Text(Initials(cv.Name)).FontSize(24).Bold().FontColor("#ffffff");
                        }
                        h.ConstantItem(20);
                        h.RelativeItem().Column(c =>
                        {
                            c.Item().Text(cv.Name).Bold().FontSize(18).FontColor("#ffffff");
                            if (!string.IsNullOrEmpty(cv.Title))
                            {
                                c.Item().PaddingTop(2).Text(cv.Title).FontSize(10).FontColor("#93c5fd").Italic();
                            }
                            c.Item().PaddingTop(8).Row(cr =>
                            {
                                if (!string.IsNullOrEmpty(cv.Email)) { ProContact(cr, cv.Email); }
                                if (!string.IsNullOrEmpty(cv.Phone)) { ProContact(cr, cv.Phone); }
                                if (!string.IsNullOrEmpty(cv.Location)) { ProContact(cr, cv.Location); }
                            });
                            c.Item().PaddingTop(3).Row(cr =>
                            {
                                if (!string.IsNullOrEmpty(cv.GitHub)) { ProContact(cr, cv.GitHub.Replace("https://github.com/", "github.com/")); }
                                if (!string.IsNullOrEmpty(cv.LinkedIn)) { ProContact(cr, cv.LinkedIn.Replace("https://www.linkedin.com/in/", "linkedin.com/in/").Replace("https://linkedin.com/in/", "linkedin.com/in/")); }
                            });
                        });
                    });

                    // ── Blue accent stripe ────────────────────────────────────
                    root.Item().Height(4).Background(ProAccent);

                    // ── Body: two columns ─────────────────────────────────────
                    root.Item().Row(body =>
                    {
                        // Left: skills
                        body.ConstantItem(170).Background(ProLight).Padding(20).Column(col =>
                        {
                            col.Spacing(14);
                            if (cv.Skills.Count > 0)
                            {
                                col.Item().Text("SKILLS").FontSize(8).Bold().FontColor(ProAccent).LetterSpacing(0.08f);
                                col.Item().Height(1.5f).Background(ProAccent);
                                foreach (IGrouping<string, Skill> group in cv.Skills.GroupBy(s => s.Category))
                                {
                                    col.Item().PaddingTop(4).Text(group.Key.ToUpper()).FontSize(7).Bold().FontColor(ProHeader).LetterSpacing(0.05f);
                                    col.Item().Inlined(il =>
                                    {
                                        il.Spacing(3);
                                        foreach (Skill skill in group)
                                        {
                                            il.Item().Background("#dbeafe").Border(1).BorderColor("#bfdbfe")
                                                .PaddingHorizontal(5).PaddingVertical(2)
                                                .Text(skill.Name).FontSize(7).FontColor(ProHeader);
                                        }
                                    });
                                }
                            }
                        });

                        // Right: main content
                        body.RelativeItem().Background("#ffffff").Padding(24).Column(col =>
                        {
                            col.Spacing(16);
                            if (!string.IsNullOrWhiteSpace(cv.Bio))
                            {
                                ProSectionTitle(col, "PROFILE");
                                col.Item().Text(cv.Bio).FontSize(9).FontColor(MainMuted).LineHeight(1.5f);
                            }
                            BuildExperience(col, cv, ProAccent);
                            BuildEducation(col, cv, ProAccent);
                            BuildProjects(col, cv, ProAccent);
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    // ════════════════════════════════════════════════════════════════════════
    // Shared content builders
    // ════════════════════════════════════════════════════════════════════════

    private static void BuildExperience(ColumnDescriptor col, CvData cv, string accent)
    {
        if (cv.Experiences.Count == 0)
        {
            return;
        }

        MainSectionTitle(col, "EXPERIENCE", accent);
        col.Item().Column(expCol =>
        {
            expCol.Spacing(12);
            foreach (Experience exp in cv.Experiences.OrderBy(e => e.Order))
            {
                expCol.Item().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(exp.Role).Bold().FontSize(10).FontColor(MainText);
                        r.AutoItem().Text(DateRange(exp.StartYear, exp.StartMonth, exp.EndYear, exp.EndMonth, exp.IsCurrent))
                            .FontSize(8).FontColor(MainMuted).Italic();
                    });
                    c.Item().Text(t =>
                    {
                        t.Span(exp.Company).FontColor(accent).FontSize(9).Bold();
                        if (!string.IsNullOrEmpty(exp.Location))
                        {
                            t.Span($"  ·  {exp.Location}").FontSize(8).FontColor(MainMuted);
                        }
                    });
                    string desc = !string.IsNullOrWhiteSpace(exp.DescriptionEn) ? exp.DescriptionEn : exp.Description ?? "";
                    if (!string.IsNullOrWhiteSpace(desc))
                    {
                        c.Item().PaddingTop(3).Text(desc).FontSize(8.5f).FontColor(MainMuted).LineHeight(1.5f);
                    }
                });
                expCol.Item().Height(0.5f).Background(DividerLine);
            }
        });
    }

    private static void BuildEducation(ColumnDescriptor col, CvData cv, string accent)
    {
        if (cv.Educations.Count == 0)
        {
            return;
        }

        MainSectionTitle(col, "EDUCATION", accent);
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
                            if (!string.IsNullOrEmpty(edu.Field)) { t.Span($" — {edu.Field}").FontSize(9).FontColor(MainMuted); }
                        });
                        r.AutoItem().Text($"{edu.StartYear} – {edu.EndYear?.ToString() ?? "Present"}").FontSize(8).FontColor(MainMuted).Italic();
                    });
                    c.Item().Text(edu.School).FontColor(accent).FontSize(9).Bold();
                    string desc = !string.IsNullOrWhiteSpace(edu.DescriptionEn) ? edu.DescriptionEn : edu.Description ?? "";
                    if (!string.IsNullOrWhiteSpace(desc))
                    {
                        c.Item().PaddingTop(2).Text(desc).FontSize(8.5f).FontColor(MainMuted).LineHeight(1.5f);
                    }
                });
            }
        });
    }

    private static void BuildProjects(ColumnDescriptor col, CvData cv, string accent)
    {
        List<Project> featured = cv.Projects.Where(p => p.IsFeatured).OrderBy(p => p.Order).Take(4).ToList();
        if (featured.Count == 0)
        {
            return;
        }

        MainSectionTitle(col, "FEATURED PROJECTS", accent);
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
                        if (!string.IsNullOrEmpty(proj.LiveUrl)) { r.AutoItem().Text(proj.LiveUrl).FontSize(7).FontColor(accent); }
                    });
                    string desc = !string.IsNullOrWhiteSpace(proj.DescriptionEn) ? proj.DescriptionEn : proj.Description;
                    if (!string.IsNullOrWhiteSpace(desc))
                    {
                        c.Item().PaddingTop(2).Text(desc).FontSize(8.5f).FontColor(MainMuted).LineHeight(1.5f);
                    }
                    if (!string.IsNullOrEmpty(proj.Technologies))
                    {
                        c.Item().PaddingTop(3).Text(proj.Technologies).FontSize(7.5f).FontColor(accent).Italic();
                    }
                });
                projCol.Item().Height(0.5f).Background(DividerLine);
            }
        });
    }

    // ════════════════════════════════════════════════════════════════════════
    // Section title helpers (per template style)
    // ════════════════════════════════════════════════════════════════════════

    private static void MainSectionTitle(ColumnDescriptor col, string title, string accent)
    {
        col.Item().Column(c =>
        {
            c.Item().Text(title).FontSize(8).Bold().FontColor(accent).LetterSpacing(0.1f);
            c.Item().Height(1.5f).Background(accent);
        });
    }

    private static void MinimalSectionTitle(ColumnDescriptor col, string title)
    {
        col.Item().Column(c =>
        {
            c.Item().Text(title).FontSize(8).Bold().FontColor(MinimalAccent).LetterSpacing(0.1f);
            c.Item().Height(1.5f).Background(MinimalAccent);
        });
    }

    private static void ProSectionTitle(ColumnDescriptor col, string title)
    {
        col.Item().Column(c =>
        {
            c.Item().Text(title).FontSize(8).Bold().FontColor(ProAccent).LetterSpacing(0.1f);
            c.Item().Height(1.5f).Background(ProAccent);
        });
    }

    // ════════════════════════════════════════════════════════════════════════
    // Contact row helpers
    // ════════════════════════════════════════════════════════════════════════

    private static void SidebarLabel(ColumnDescriptor col, string title, string accent)
    {
        col.Item().Text(title).FontSize(7.5f).Bold().FontColor(accent).LetterSpacing(0.08f);
    }

    private static void SidebarContactRow(ColumnDescriptor col, string label, string value, string textColor, string labelColor)
    {
        col.Item().Row(r =>
        {
            r.ConstantItem(28).Text(label).FontSize(7).Bold().FontColor(labelColor);
            r.RelativeItem().Text(value).FontSize(7.5f).FontColor(textColor);
        });
    }

    private static void InlineContact(RowDescriptor row, string value, string color)
    {
        row.AutoItem().PaddingRight(12).Text(value).FontSize(7.5f).FontColor(color);
    }

    private static void ProContact(RowDescriptor row, string value)
    {
        row.AutoItem().PaddingRight(14).Text(value).FontSize(7.5f).FontColor("#bfdbfe");
    }

    // ════════════════════════════════════════════════════════════════════════
    // Shared utils
    // ════════════════════════════════════════════════════════════════════════

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
        string end = isCurrent ? "Present" : endYear.HasValue ? $"{MonthShort(endMonth ?? 1)} {endYear}" : "Present";
        return $"{start} – {end}";
    }

    private static string MonthShort(int month) => month switch
    {
        1 => "Jan",
        2 => "Feb",
        3 => "Mar",
        4 => "Apr",
        5 => "May",
        6 => "Jun",
        7 => "Jul",
        8 => "Aug",
        9 => "Sep",
        10 => "Oct",
        11 => "Nov",
        12 => "Dec",
        _ => ""
    };
}
