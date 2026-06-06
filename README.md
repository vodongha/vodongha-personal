# vodongha.id.vn

Personal website of **Võ Đông Hà** — Full-Stack Developer.

Live: [https://vodongha.id.vn](https://vodongha.id.vn)

## Stack

| Layer | Technology |
|---|---|
| Framework | Blazor Web App (.NET 10, Interactive Server) |
| Database | PostgreSQL via Neon (Singapore) |
| ORM | Entity Framework Core |
| Styling | SCSS → `wwwroot/app.css` (AspNetCore.SassCompiler) |
| Deploy | Fly.io, app `vodongha`, region Singapore |
| Email | Resend API (contact form notifications) |

## Project structure

```
vodongha-personal/
├── Components/
│   ├── Layout/          # NavBar, MainLayout, FooterSection, AdminLayout, ReconnectModal
│   ├── Pages/
│   │   ├── Admin/       # Login, Dashboard, AdminSkills, AdminProjects,
│   │   │                #   AdminBlog, AdminEducation, AdminExperience, AdminSettings
│   │   ├── Blog/        # BlogPostPage
│   │   └── Home.razor
│   ├── Sections/        # Hero, Skills, Projects, Experience, Education, Blog, Contact
│   └── Shared/          # ProjectCard, BlogCard
├── Data/
│   ├── AppDbContext.cs  # EF context + seed data
│   └── Models/          # Skill, Project, BlogPost, Experience, Education,
│                        #   ContactMessage, SiteSetting
├── Services/            # Blog, Project, Skill, Experience, Education,
│                        #   Contact, Email, Language, SiteSetting
├── Styles/              # _variables, _base, _nav, _hero, _skills, _projects,
│                        #   _timeline, _blog, _contact, _footer, _admin, app.scss
├── Migrations/          # EF Core migrations
├── Dockerfile
├── fly.toml
└── vodongha-personal.csproj
```

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
  }
}
```

```bash
dotnet run
```

EF Core migrations apply automatically on startup.

**SCSS:** edit `Styles/_*.scss` → `dotnet build` → commit both `.scss` and `wwwroot/app.css`.

## Admin panel

`/admin/login` — manage Skills, Projects, Blog, Experience, Education, and site settings (bio VI/EN, social links).

## i18n

Vietnamese/English toggle via `LanguageService`. Content models with dual-language fields use `Description` (VI) + `DescriptionEn` (EN). `SiteSetting` stores `Bio` and `BioEn` separately.

## Deploy

Push to `master` → Fly.io auto-deploy (~2 min).

Secrets are stored in Fly.io — never commit them.
