# vodongha.id.vn

Personal portfolio website of **Võ Đông Hà** — Full-Stack Developer.

**Live:** [https://vodongha.id.vn](https://vodongha.id.vn)

---

## Features

- **Landing page** — Hero, Skills & Technologies, Featured Projects, Work Experience, Education, Blog, Contact
- **Bilingual (VI / EN)** — toggle on every page; all content models have dual-language fields
- **Expand / collapse** — each section shows 2 items by default; "Show more" to reveal the rest
- **Blog** — full posts with bilingual content, per-page Open Graph + Twitter Card meta tags
- **Contact form** — messages saved to DB + email notification via Resend
- **Visitor counter** — unique visitors tracked by IP, displayed in the footer
- **Admin panel** — manage Skills, Projects, Blog, Experience, Education, Contact Messages, and site settings

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | Blazor Web App (.NET 10, Interactive Server) |
| Database | PostgreSQL via [Neon](https://neon.tech) (Singapore) |
| ORM | Entity Framework Core |
| Styling | SCSS → `wwwroot/app.css` (public) + `wwwroot/admin.css` (admin) via `AspNetCore.SassCompiler` |
| Email | [Resend](https://resend.com) API |
| Deploy | [Fly.io](https://fly.io), app `vodongha`, region Singapore |
| CI/CD | Push to `master` → auto-deploy |

---

## Project structure

```
vodongha-personal/
├── Components/
│   ├── App.razor               # HTML root — SEO meta tags (OG, Twitter, canonical)
│   ├── Layout/                 # NavBar, MainLayout, FooterSection, AdminLayout
│   ├── Pages/
│   │   ├── Home.razor          # Landing page
│   │   ├── Blog/               # BlogPostPage (per-post dynamic SEO)
│   │   └── Admin/              # Login, Dashboard, Skills, Projects, Blog,
│   │                           #   Education, Experience, Contacts, Settings
│   │                           #   (each page: .razor + .razor.cs code-behind)
│   ├── Sections/               # HeroSection, SkillsSection, ProjectsSection,
│   │                           #   ExperienceSection, EducationSection,
│   │                           #   BlogSection, ContactSection
│   └── Shared/                 # ProjectCard, BlogCard, ConfirmDialog
├── Data/
│   ├── AppDbContext.cs          # EF context + seed data
│   └── Models/                 # Skill, Project, BlogPost, Experience, Education,
│                               #   ContactMessage, SiteSetting, VisitorLog
├── Services/                   # Blog, Project, Skill, Experience, Education,
│                               #   Contact, Email, Language, SiteSetting, Visitor
├── Styles/
│   ├── app.scss                # Entry point for public site → wwwroot/app.css
│   ├── admin.scss              # Entry point for admin panel → wwwroot/admin.css
│   ├── _admin-styles.scss      # All admin panel styles (imported by admin.scss)
│   └── _*.scss                 # Public site partials (variables, base, nav, hero, ...)
├── Migrations/                 # EF Core migrations
├── Dockerfile
├── fly.toml
└── vodongha-personal.csproj
```

---

## Local development

**Prerequisites:** .NET 10 SDK, PostgreSQL instance

```bash
git clone https://github.com/vodongha/vodongha-personal.git
cd vodongha-personal
```

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "Admin": {
    "Username": "admin",
    "Password": "changeme"
  },
  "Email": {
    "ResendApiKey": ""
  }
}
```

```bash
dotnet run
```

EF Core migrations run automatically on startup. The app is available at `https://localhost:5001`.

### SCSS

The project has two separate CSS pipelines:

| Source | Output | Used by |
|---|---|---|
| `Styles/app.scss` | `wwwroot/app.css` | Public pages |
| `Styles/admin.scss` | `wwwroot/admin.css` | Admin panel |

`dotnet build` triggers `AspNetCore.SassCompiler` to compile both. Always commit the compiled `.css` files alongside their `.scss` source.

If CSS appears stale after editing SCSS (Dart Sass skips recompile when the `.css` is newer), force recompile manually — find `dart.exe` under the SassCompiler NuGet tools path and run:

```
dart.exe sass.snapshot --style=expanded --no-source-map Styles\app.scss wwwroot\app.css
dart.exe sass.snapshot --style=expanded --no-source-map Styles\admin.scss wwwroot\admin.css
```

---

## Admin panel

URL: `/admin/login`

| Page | Path | Purpose |
|---|---|---|
| Dashboard | `/admin` | Overview stats |
| Skills | `/admin/skills` | Add/edit/delete skills with proficiency and devicon class |
| Projects | `/admin/projects` | Add/edit/delete projects (VI + EN), drag-to-reorder, paginated |
| Blog | `/admin/blog` | Write and publish posts (VI + EN, auto-slug from Vietnamese title) |
| Education | `/admin/education` | Manage education entries |
| Experience | `/admin/experience` | Manage work experience entries |
| Messages | `/admin/contacts` | View contact form submissions — unread badge, mark read, delete, reply |
| Settings | `/admin/settings` | Bio (VI/EN), social links (GitHub, LinkedIn, Facebook), avatar upload |

---

## Visitor counter

Unique visitors are tracked server-side by IP address using a middleware in `Program.cs`.

- Reads the real IP from `X-Forwarded-For` (Fly.io proxy) or `RemoteIpAddress`
- Each IP is stored once in `VisitorLogs` (unique index on `IpAddress`)
- Localhost and private IPs (`::1`, `127.x`, `10.x`) are excluded
- Count is displayed in the footer

---

## i18n

Language toggle (VI / EN) via `LanguageService`. Default: **English**.

- UI strings: `Lang.T("key")` — keys defined in `LanguageService.cs`
- Bilingual content: `Lang.IsVi ? item.Description : (item.DescriptionEn ?? item.Description)`

---

## Deploy

```bash
git push origin master
```

Fly.io detects the push and deploys automatically (~2 minutes). Secrets are managed via `flyctl secrets set KEY=VALUE` — never committed to the repo.

### Fly.io secrets

| Secret | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL connection string |
| `Admin__Username` | Admin panel username |
| `Admin__Password` | Admin panel password |
| `Email__ResendApiKey` | Resend API key |

---

## Built with

[Claude Code](https://claude.ai/code) by Anthropic.
