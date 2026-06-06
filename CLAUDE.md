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
| SCSS | Two entry points compiled by `AspNetCore.SassCompiler` on `dotnet build`: `Styles/app.scss` → `wwwroot/app.css` (public), `Styles/admin.scss` → `wwwroot/admin.css` (admin). Always commit both `.scss` and the compiled `.css`. |
| Email | Resend API (`Email__ResendApiKey`). Sender: `no-reply@vodongha.id.vn`, recipient: `vodongha@hotmail.com` |
| Deploy | Fly.io, app `vodongha`, region `sin`. Push `master` → auto-deploy (~2 min) |
| Migrations | EF Core, applied automatically on startup via `MigrateAsync()` in `Program.cs` |

## Solution structure

```
vodongha-personal/
├── Components/
│   ├── App.razor                     # Root — <head> with SEO meta tags (OG, Twitter Card, canonical)
│   ├── Layout/
│   │   ├── MainLayout.razor          # Public layout — loads app.css
│   │   ├── AdminLayout.razor         # Admin layout — loads admin.css
│   │   ├── NavBar.razor              # Top navigation (InteractiveServer — language toggle)
│   │   ├── FooterSection.razor       # Footer with visitor count badge (InteractiveServer)
│   │   └── ReconnectModal.razor      # Blazor reconnect overlay
│   ├── Pages/
│   │   ├── Home.razor                # Landing page (InteractiveServer)
│   │   ├── Blog/BlogPostPage.razor   # Individual blog post with per-page OG meta tags
│   │   ├── Admin/
│   │   │   ├── Login.razor + .cs         # /admin/login
│   │   │   ├── Dashboard.razor + .cs     # /admin
│   │   │   ├── AdminSkills.razor + .cs   # /admin/skills — QuickGrid + PaginationState
│   │   │   ├── AdminProjects.razor + .cs # /admin/projects — manual pagination + drag-to-reorder
│   │   │   ├── AdminBlog.razor + .cs     # /admin/blog — auto-slug from Vietnamese title
│   │   │   ├── AdminEducation.razor + .cs   # /admin/education — QuickGrid
│   │   │   ├── AdminExperience.razor + .cs  # /admin/experience — QuickGrid
│   │   │   ├── AdminContacts.razor + .cs    # /admin/contacts — unread badge, mark read, reply
│   │   │   └── AdminSettings.razor + .cs    # /admin/settings — avatar upload, social links
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
│       ├── BlogCard.razor            # Reusable blog post card
│       └── ConfirmDialog.razor       # Delete confirmation modal (type "Delete" to enable button)
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
│   ├── app.scss                      # Public site entry point — imports all _*.scss partials
│   ├── admin.scss                    # Admin entry point — imports _admin-styles.scss
│   ├── _admin-styles.scss            # All admin panel styles (BEM: .admin-*)
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
│   └── _reconnect.scss
├── Migrations/                       # EF Core — never modify existing migrations
├── Program.cs                        # DI, middleware (visitor tracking), auth, routes
├── Dockerfile
├── .dockerignore                     # Excludes wwwroot/app.css and wwwroot/admin.css so Docker build forces Dart Sass recompile
├── .gitattributes                    # wwwroot/*.css marked linguist-generated (hidden from GitHub language stats)
├── fly.toml
└── vodongha-personal.csproj
```

## SCSS pipeline

Two separate CSS outputs — public and admin are completely independent:

| SCSS entry | Output | Layout that loads it |
|---|---|---|
| `Styles/app.scss` | `wwwroot/app.css` | `MainLayout.razor` |
| `Styles/admin.scss` | `wwwroot/admin.css` | `AdminLayout.razor` |

`AspNetCore.SassCompiler` runs Dart Sass with `--update` during `dotnet build`. The `--update` flag skips recompile if the output `.css` is newer than the source `.scss`. This causes stale output when:
- You edit `.scss` and the `.css` already exists with a newer timestamp
- Docker build (`COPY . .` preserves host timestamps)

**Fix for Docker:** `wwwroot/app.css` and `wwwroot/admin.css` are listed in `.dockerignore`, so Docker never copies them — Dart Sass always recompiles from scratch.

**Fix for local stale CSS:** Force recompile by running Dart Sass directly. Find `dart.exe` in the NuGet package cache under `AspNetCore.SassCompiler` tools, then:

```
dart.exe sass.snapshot --style=expanded --no-source-map Styles\app.scss wwwroot\app.css
dart.exe sass.snapshot --style=expanded --no-source-map Styles\admin.scss wwwroot\admin.css
```

After editing `_admin-styles.scss`, touch `admin.scss` or run the command above — do NOT just run `dotnet build` and assume it compiled.

## Admin panel

- Login: `/admin/login` → POST `/admin/do-login` → cookie auth (7-day sliding expiration)
- Each admin page uses `@layout AdminLayout` and `@attribute [Authorize]`
- Direct DB access via `IDbContextFactory<AppDbContext>` — no separate API layer
- All admin pages follow the `.razor` + `.razor.cs` code-behind pattern (partial class inheriting `ComponentBase`, `[Inject]` properties)

### AdminProjects — manual pagination + drag-to-reorder

AdminProjects uses a hand-rolled `<table>` (not QuickGrid) because it needs HTML5 drag-and-drop for ordering.

Key pattern — drag indices must be translated to global list indices when pagination is active:

```csharp
// Local index = position within the current page (0..pageSize-1)
// Global index = position in the full Projects list
int globalIndex = _page * _pageSize + localIndex;
```

All four drag methods (`DragStart`, `DragOver`, `DragClass`, `Drop`) compute `globalIndex` from `localIndex` before touching the `Projects` list.

### AdminSkills / AdminBlog / AdminEducation / AdminExperience — QuickGrid

These pages use `QuickGrid` with `PaginationState`. The table is bound via `Items="Filtered"` where `Filtered` is a computed `IQueryable<T>` filtered by the search string.

QuickGrid renders empty filler rows as `<tr><td></td>...</tr>` (no class). They are hidden in `_admin-styles.scss`:

```scss
tbody tr:not(:has(> td:not(:empty))) { display: none !important; }
```

### AdminBlog — auto-slug

Typing in the Vietnamese title field triggers `OnTitleInput`, which calls `GenerateSlug()` — a static method that lowercases, strips Vietnamese diacritics, and replaces spaces with hyphens. The slug field is editable and is only auto-filled when adding a new post (not when editing).

## Coding conventions

- Microsoft .NET naming conventions throughout
- **Always use braces** for `if`, `for`, `foreach` — even single-line
- `var` only when the type is obvious from the right-hand side
- All DB-touching code async end-to-end: `await`, `ToListAsync()`, `FirstOrDefaultAsync()`
- `await using var db = await DbFactory.CreateDbContextAsync()` — factory pattern, never scoped DbContext
- No `.Result` or `.Wait()` on Tasks
- No comments unless the WHY is non-obvious

## Blazor code-behind pattern

Every admin page uses a `.razor.cs` partial class:

```csharp
public partial class AdminSkills : ComponentBase
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    // ...
}
```

The `.razor` file contains only markup — no `@code { }` block, no `@inject` directives.

## Admin table styles

Admin tables (both `<table class="quickgrid">` and `<QuickGrid class="quickgrid">`) share the same CSS rules in `_admin-styles.scss`:

- Header: dark background (`#111827`), green-tinted text (`#6ee7b7`)
- Odd rows: `#0d0d0d`, even rows: `#121212`
- Hover: `#1e2a24` (overrides stripe with `!important`)

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
| `AvatarUrl` | Avatar image path (relative to wwwroot) |

Social links in HeroSection are loaded dynamically — links with empty values are hidden.

## Visitor tracking

`VisitorService` deduplicates by IP address — each unique IP is stored once in `VisitorLogs`.

Middleware in `Program.cs` fires on GET requests to non-static, non-admin, non-framework paths. It reads the real IP from the `X-Forwarded-For` header (Fly.io proxy) and calls `VisitorService.LogAsync()`.

Localhost (`::1`, `127.x`, `10.x`) IPs are excluded from tracking.

## Sections — expand/collapse

All 5 landing page sections (Skills, Projects, Experience, Education, Blog) show 2 items initially with a pill-style "Show more / Thu gọn" button. The button only renders when the total count exceeds 2. i18n keys: `common.showmore` / `common.showless`.

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
