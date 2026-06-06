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
| Styling | SCSS → `wwwroot/app.css` (AspNetCore.SassCompiler) |
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
│   ├── Sections/               # HeroSection, SkillsSection, ProjectsSection,
│   │                           #   ExperienceSection, EducationSection,
│   │                           #   BlogSection, ContactSection
│   └── Shared/                 # ProjectCard, BlogCard
├── Data/
│   ├── AppDbContext.cs          # EF context + seed data
│   └── Models/                 # Skill, Project, BlogPost, Experience, Education,
│                               #   ContactMessage, SiteSetting, VisitorLog
├── Services/                   # Blog, Project, Skill, Experience, Education,
│                               #   Contact, Email, Language, SiteSetting, Visitor
├── Styles/                     # _variables, _base, _nav, _hero, _skills,
│                               #   _projects, _timeline, _blog, _contact,
│                               #   _footer, _reconnect, _admin, app.scss
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

Edit `Styles/_*.scss` → run `dotnet build` → commit both the `.scss` file **and** `wwwroot/app.css`.  
The `AspNetCore.SassCompiler` package handles compilation during build — no external `sass` CLI needed.

---

## Admin panel

URL: `/admin/login`

| Page | Path | Purpose |
|---|---|---|
| Dashboard | `/admin` | Overview |
| Skills | `/admin/skills` | Add/edit/delete skills with proficiency |
| Projects | `/admin/projects` | Add/edit/delete projects (VI + EN descriptions) |
| Blog | `/admin/blog` | Write and publish blog posts (VI + EN, auto-slug) |
| Education | `/admin/education` | Manage education entries |
| Experience | `/admin/experience` | Manage work experience entries |
| Messages | `/admin/contacts` | View contact form submissions — unread badge, mark read, delete, reply |
| Settings | `/admin/settings` | Bio (VI/EN), social links (GitHub, LinkedIn, Facebook), contact info |

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
