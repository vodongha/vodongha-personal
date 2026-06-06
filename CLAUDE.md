# vodongha.id.vn — CLAUDE.md

## Project overview

Personal portfolio website of Võ Đông Hà. Blazor Web App (.NET 10) + PostgreSQL (Neon, Singapore) + SCSS dark theme, deployed on Fly.io.

- **Live:** https://vodongha.id.vn
- **Repo:** https://github.com/vodongha/vodongha-personal
- **Admin:** https://vodongha.id.vn/admin/login
- **Project file:** `vodongha-personal.csproj` (RootNamespace: `vodongha`, AssemblyName: `vodongha-personal`)

## Technology stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Frontend | Blazor Web App — `@rendermode InteractiveServer` on pages/components with state |
| Database | PostgreSQL via Neon (Singapore). Fly.io secret: `ConnectionStrings__DefaultConnection` |
| ORM | Entity Framework Core — no raw SQL in application code |
| SCSS | `Styles/app.scss` → compiled to `wwwroot/app.css` via `AspNetCore.SassCompiler` on `dotnet build`. Always commit both `.scss` and `wwwroot/app.css`. |
| Email | Resend API (`Email__ResendApiKey`). Sender: `no-reply@vodongha.id.vn`, recipient: `REDACTED_EMAIL` |
| Deploy | Fly.io, app `vodongha`, region `sin`. Push `master` → auto-deploy (~2 min) |
| Migrations | EF Core, applied automatically on startup via `MigrateAsync()` in `Program.cs` |

## Solution structure

```
vodongha-personal/
├── Components/
│   ├── App.razor                     # Root — <head> with SEO meta tags (OG, Twitter Card, canonical)
│   ├── Layout/
│   │   ├── MainLayout.razor          # Public layout
│   │   ├── AdminLayout.razor         # Admin layout
│   │   ├── NavBar.razor              # Top navigation (InteractiveServer — language toggle)
│   │   ├── FooterSection.razor       # Footer with visitor count badge (InteractiveServer)
│   │   └── ReconnectModal.razor      # Blazor reconnect overlay
│   ├── Pages/
│   │   ├── Home.razor                # Landing page (InteractiveServer)
│   │   ├── Blog/BlogPostPage.razor   # Individual blog post with per-page OG meta tags
│   │   ├── Admin/
│   │   │   ├── Login.razor           # /admin/login
│   │   │   ├── Dashboard.razor       # /admin
│   │   │   ├── AdminSkills.razor     # /admin/skills
│   │   │   ├── AdminProjects.razor   # /admin/projects
│   │   │   ├── AdminBlog.razor       # /admin/blog
│   │   │   ├── AdminEducation.razor  # /admin/education
│   │   │   ├── AdminExperience.razor # /admin/experience
│   │   │   ├── AdminContacts.razor   # /admin/contacts — view, read, delete contact messages
│   │   │   └── AdminSettings.razor   # /admin/settings — site info, social links, bio
│   │   ├── Error.razor
│   │   └── NotFound.razor
│   ├── Sections/                     # One file per landing page section
│   │   ├── HeroSection.razor         # Name, role, bio, social links (from SiteSettings)
│   │   ├── SkillsSection.razor       # Skills grid grouped by category — expand/collapse
│   │   ├── ProjectsSection.razor     # Featured projects grid — expand/collapse
│   │   ├── ExperienceSection.razor   # Work experience timeline — expand/collapse
│   │   ├── EducationSection.razor    # Education timeline — expand/collapse
│   │   ├── BlogSection.razor         # Latest blog posts — expand/collapse
│   │   └── ContactSection.razor      # Contact form
│   └── Shared/
│       ├── ProjectCard.razor         # Reusable project card
│       └── BlogCard.razor            # Reusable blog post card
├── Data/
│   ├── AppDbContext.cs               # EF context + seed data (Skills, Projects, Experience, Education, SiteSettings, BlogPost)
│   └── Models/
│       ├── Skill.cs
│       ├── Project.cs
│       ├── BlogPost.cs
│       ├── Experience.cs
│       ├── Education.cs
│       ├── ContactMessage.cs
│       ├── SiteSetting.cs            # Key-value store for site metadata
│       └── VisitorLog.cs             # Unique visitors by IP — IpAddress, FirstSeenAt, UserAgent
├── Services/
│   ├── BlogService.cs
│   ├── ProjectService.cs
│   ├── SkillService.cs
│   ├── ExperienceService.cs
│   ├── EducationService.cs
│   ├── ContactService.cs             # Save message to DB + send email notification via Resend
│   ├── EmailService.cs
│   ├── LanguageService.cs            # VI/EN toggle; T("key") for UI strings; OnChange event
│   ├── SiteSettingService.cs
│   └── VisitorService.cs             # LogAsync(ip) — deduplicated by IP; GetCountAsync()
├── Styles/
│   ├── app.scss                      # Imports all partials
│   ├── _variables.scss               # Design tokens (colors, spacing, fonts)
│   ├── _base.scss                    # Global styles + .section layout (including expand button)
│   ├── _nav.scss
│   ├── _hero.scss
│   ├── _skills.scss
│   ├── _projects.scss
│   ├── _timeline.scss                # Shared by Experience + Education sections
│   ├── _blog.scss
│   ├── _contact.scss
│   ├── _footer.scss                  # Footer + visitor count badge
│   ├── _reconnect.scss
│   └── _admin.scss                   # All admin panel styles
├── Migrations/                       # EF Core — never modify existing migrations
├── Program.cs                        # DI, middleware (visitor tracking), auth, routes
├── Dockerfile
├── fly.toml
└── vodongha-personal.csproj
```

## Visitor tracking

`VisitorService` deduplicates by IP address — each unique IP is stored once in `VisitorLogs`.

Middleware in `Program.cs` fires on GET requests to non-static, non-admin, non-framework paths. It reads the real IP from the `X-Forwarded-For` header (Fly.io proxy) and calls `VisitorService.LogAsync()`.

`FooterSection.razor` calls `VisitorService.GetCountAsync()` on load to display the count.

Localhost (`::1`, `127.x`, `10.x`) IPs are excluded from tracking.

## Sections — expand/collapse

All 5 landing page sections (Skills, Projects, Experience, Education, Blog) show 2 items initially with a pill-style "Show more / Thu gọn" button. The button only renders when the total count exceeds 2. i18n keys: `common.showmore` / `common.showless`.

## Admin panel

- Login: `/admin/login` → POST `/admin/do-login` → cookie auth (7-day sliding expiration)
- Each admin page uses `@layout AdminLayout` and `@attribute [Authorize]`
- Direct DB access via `IDbContextFactory<AppDbContext>` — no API layer
- **AdminContacts** (`/admin/contacts`): lists received contact messages with unread badge count, mark-read, delete, reply via mailto

## Coding conventions

- Microsoft .NET naming conventions throughout
- **Always use braces** for `if`, `for`, `foreach` — even single-line
- `var` only when the type is obvious from the right-hand side
- All DB-touching code async end-to-end: `await`, `ToListAsync()`, `FirstOrDefaultAsync()`
- `await using var db = await DbFactory.CreateDbContextAsync()` — factory pattern, never scoped DbContext
- No `.Result` or `.Wait()` on Tasks
- No comments unless the WHY is non-obvious

## i18n

`LanguageService` handles VI/EN toggle. Default language: **English**.

- UI strings: `Lang.T("key")`
- Content with dual fields: `Lang.IsVi ? item.Description : (item.DescriptionEn ?? item.Description)`
- Layout components that react to language changes need `@rendermode InteractiveServer` + `Lang.OnChange += StateHasChanged`

Bilingual content models: `Project`, `Experience`, `Education` (Description/DescriptionEn), `BlogPost` (Title/TitleEn, Summary/SummaryEn, Content/ContentEn), `SiteSetting` (Bio + BioEn as separate keys).

## SiteSettings keys

| Key | Purpose |
|---|---|
| `Name` | Full name |
| `Title` | Job title |
| `Tagline` | Short tagline |
| `Bio` | Bio in Vietnamese |
| `BioEn` | Bio in English |
| `Email` | Contact email |
| `Phone` | Phone number |
| `Location` | City/country |
| `GitHub` | GitHub profile URL |
| `LinkedIn` | LinkedIn profile URL |
| `Facebook` | Facebook profile URL (optional, hidden if empty) |
| `AvatarUrl` | Avatar image path |

Social links in HeroSection are loaded dynamically from SiteSettings — links with empty values are hidden.

## SEO

`App.razor` contains static Open Graph, Twitter Card, and canonical meta tags for the homepage.

`BlogPostPage.razor` renders per-post `<HeadContent>` with dynamic `og:title`, `og:description`, `og:url`, `og:image`, `twitter:card`, and `link rel=canonical` — populated from the blog post's Title/TitleEn, Summary/SummaryEn, CoverImageUrl, and Slug.

## Database migrations

Never modify an existing migration. Add new migrations with:
```
dotnet ef migrations add <MigrationName>
```

Migrations apply automatically on startup. Seed data for Skills, Projects, Experience, Education, SiteSettings, and the first blog post live in `AppDbContext.OnModelCreating` via `HasData`.

## Git workflow

Direct commits to `master` are fine for this solo project.

Commit messages: short, focused on what changed. No trailing summaries.

Deploy by pushing `master` — Fly.io auto-deploys in ~2 minutes.

## Fly.io secrets

| Secret | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL |
| `Admin__Username` | Admin panel login |
| `Admin__Password` | Admin panel login |
| `Email__ResendApiKey` | Resend API key for contact form |

Set with: `flyctl secrets set KEY=VALUE`
