# vodongha.id.vn — CLAUDE.md

## Project overview

Personal website of Võ Đông Hà. Blazor Web App (.NET 10) + PostgreSQL (Neon, Singapore) + SCSS dark theme, deployed on Fly.io.

- Live: https://vodongha.id.vn
- Repo: https://github.com/vodongha/vodongha-personal
- Admin: https://vodongha.id.vn/admin/login

## Technology stack

- **Runtime:** .NET 10
- **Frontend:** Blazor Web App — `@rendermode InteractiveServer` on pages/components with logic
- **Database:** PostgreSQL via Neon (Singapore). Fly.io secret: `ConnectionStrings__DefaultConnection`
- **ORM:** Entity Framework Core — no raw SQL in application code
- **SCSS:** `Styles/app.scss` imports all partials → compiled to `wwwroot/app.css` via `AspNetCore.SassCompiler` on `dotnet build`. Always commit both the `.scss` change and the updated `app.css`.
- **Email:** Resend API (`Email__ResendApiKey` secret). Sender: `no-reply@vodongha.id.vn`, recipient: `REDACTED_EMAIL`
- **Deploy:** Fly.io, app `vodongha`, region `sin`. Push `master` → auto-deploy (~2 min)
- **Migrations:** EF Core, applied automatically on startup via `MigrateAsync()` in `Program.cs`

## Solution structure

| Path | Purpose |
|---|---|
| `Components/Layout/` | NavBar (InteractiveServer), MainLayout, AdminLayout, ReconnectModal |
| `Components/Pages/` | Home, BlogPostPage, Error, NotFound |
| `Components/Pages/Admin/` | Login, Dashboard, AdminSkills, AdminProjects, AdminBlog, AdminEducation, AdminExperience, AdminSettings |
| `Components/Sections/` | Hero, Skills, Projects, Experience, Education, Blog, Contact — one file per landing page section |
| `Components/Shared/` | ProjectCard, BlogCard |
| `Data/Models/` | Skill, Project, BlogPost, Experience, Education, ContactMessage, SiteSetting |
| `Data/AppDbContext.cs` | EF Core context. Seed data for Skills, Projects, Experience, Education, SiteSettings lives here. |
| `Services/` | BlogService, ProjectService, SkillService, ExperienceService, EducationService, ContactService, EmailService, LanguageService |
| `Styles/` | `_variables`, `_base`, `_nav`, `_hero`, `_skills`, `_projects`, `_timeline`, `_blog`, `_contact`, `_footer`, `_reconnect`, `_admin`, `app.scss` |
| `Migrations/` | EF Core migration files — never modify an existing migration, always add new |

## Admin panel

Login: `/admin/login` → POST `/admin/do-login` → cookie auth.

Each admin page (`/admin/*`) uses `@layout AdminLayout` and `@attribute [Authorize]`. They inject `IDbContextFactory<AppDbContext>` or the relevant service directly — no separate API layer.

## Coding conventions

- Follow Microsoft .NET naming conventions
- **Always use braces** — even for single-line `if`, `for`, `foreach`
- Use `var` only when the type is obvious from the right-hand side
- Blazor components with logic use code-behind (`.razor` + `.razor.cs`). Simple display-only components may be single-file.
- All DB-touching code must be async end-to-end: `await`, `ToListAsync()`, `FirstOrDefaultAsync()`, etc.
- Use `await using var db = await DbFactory.CreateDbContextAsync()` — factory pattern, not scoped DbContext
- No `.Result` or `.Wait()` on Tasks
- No comments unless the WHY is non-obvious

## i18n

`LanguageService` handles Vietnamese/English toggle. Use `Lang.T("key")` for UI strings and `Lang.IsVi ? item.Description : (item.DescriptionEn ?? item.Description)` for content with dual-language fields.

## Git workflow

Direct commits to `master` are fine for this solo project. Branch naming: `feature/short-description` for larger changes.

Commit messages: short, focused on what changed. No trailing summaries.

Deploy by pushing to `master`.

## Fly.io secrets

| Secret | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL |
| `Admin__Username` | Admin panel login |
| `Admin__Password` | Admin panel login |
| `Email__ResendApiKey` | Resend API key for contact form |

Set with: `flyctl secrets set KEY=VALUE`
