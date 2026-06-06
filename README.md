# vodongha.id.vn

Personal portfolio website of **Võ Đông Hà** — Full-Stack Developer.

**Live:** [https://vodongha.id.vn](https://vodongha.id.vn) | **Admin:** [https://vodongha.id.vn/admin/login](https://vodongha.id.vn/admin/login)

---

## Features

- **Landing page** — Hero, Skills & Technologies, Featured Projects, Work Experience, Education, Blog, Contact
- **Bilingual (VI / EN)** — toggle on every page; all content models have dual-language fields
- **Expand / collapse** — each section shows 2 items by default; "Show more" to reveal the rest
- **Blog** — full posts with bilingual content, per-page Open Graph + Twitter Card meta tags
- **Contact form** — messages saved to DB + email notification via Resend
- **Visitor counter** — unique visitors tracked by IP, displayed in the footer
- **Admin panel** — manage Skills, Projects, Blog, Experience, Education, Contact Messages, and site settings
- **Mobile responsive** — admin panel has bottom navigation bar on screens ≤ 768px

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | Blazor Web App (.NET 10, Interactive Server) |
| Database | PostgreSQL via [Neon](https://neon.tech) (Singapore) |
| ORM | Entity Framework Core |
| Styling | SCSS compiled by `AspNetCore.SassCompiler` — `Styles/app.scss` → public, `Styles/admin.scss` → admin |
| Email | [Resend](https://resend.com) API |
| Deploy | [Fly.io](https://fly.io), app `vodongha`, region Singapore |
| CI/CD | Merge PR to `master` → auto-deploy |

---

## Project structure

```
vodongha-personal/
├── Components/
│   ├── App.razor               # HTML root — SEO meta tags (OG, Twitter, canonical), scripts
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
├── wwwroot/
│   └── js/admin.js             # Event delegation for admin UI (select arrow toggle)
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

Two separate CSS pipelines:

| Source | Output | Used by |
|---|---|---|
| `Styles/app.scss` | `wwwroot/app.css` | Public pages |
| `Styles/admin.scss` | `wwwroot/admin.css` | Admin panel |

Compiled CSS is **not committed** — listed in `.gitignore`. `dotnet build` compiles SCSS automatically via `AspNetCore.SassCompiler`. Docker also compiles from scratch (CSS excluded from `.dockerignore`).

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

## Git workflow

All work is done on the `develop` branch. Changes are merged into `master` via Pull Request.

```bash
# Always work on develop
git checkout develop

# Make changes, commit
git add <files>
git commit -m "describe the change"
git push origin develop

# Open a PR: develop → master
gh pr create --title "PR title" --body "description" --base master --head develop

# After merge, master deploys automatically to Fly.io
```

`master` is production — Fly.io deploys automatically on every merge to `master`.

---

## Deploy

Merging a PR into `master` triggers Fly.io auto-deploy (~2 minutes). Secrets managed via `flyctl secrets set KEY=VALUE` — never committed.

### Fly.io secrets

| Secret | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL connection string |
| `Admin__Username` | Admin panel username |
| `Admin__Password` | Admin panel password |
| `Email__ResendApiKey` | Resend API key |

---

## Visitor counter

Unique visitors tracked server-side by IP via middleware in `Program.cs`. Each IP stored once in `VisitorLogs`. Localhost and private IPs excluded. Count displayed in footer.

---

## i18n

Language toggle (VI / EN) via `LanguageService`. Default: **English**.

- UI strings: `Lang.T("key")`
- Bilingual content: `Lang.IsVi ? item.Description : (item.DescriptionEn ?? item.Description)`

---

## Built with

[Claude Code](https://claude.ai/code) by Anthropic.
