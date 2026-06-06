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
vodongha/
├── Components/
│   ├── Layout/          # NavBar, MainLayout, AdminLayout, ReconnectModal
│   ├── Pages/
│   │   ├── Admin/       # Login, Dashboard, AdminSkills, AdminProjects,
│   │   │                #   AdminBlog, AdminEducation, AdminExperience, AdminSettings
│   │   ├── Blog/        # BlogPostPage
│   │   └── Home.razor
│   ├── Sections/        # Hero, Skills, Projects, Experience, Education, Blog, Contact
│   └── Shared/          # ProjectCard, BlogCard
├── Data/
│   ├── AppDbContext.cs
│   └── Models/          # Skill, Project, BlogPost, Experience, Education,
│                        #   ContactMessage, SiteSetting
├── Services/            # Blog, Project, Skill, Experience, Education,
│                        #   Contact, Email, Language
├── Styles/              # _variables, _base, _nav, _hero, _skills, _projects,
│                        #   _timeline, _blog, _contact, _footer, _admin, app.scss
├── Migrations/          # EF Core migrations
├── Dockerfile
└── fly.toml
```

## Local development

**Prerequisites:** .NET 10 SDK, PostgreSQL instance

```bash
git clone https://github.com/vodongha/vodongha-personal.git
cd vodongha-personal
```

Create `appsettings.Development.json` with your local connection string:

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

The app applies EF Core migrations automatically on startup.

**SCSS:** edit `Styles/_*.scss`, then run `dotnet build` — `wwwroot/app.css` is regenerated. Commit both the `.scss` and `app.css` changes.

## Admin panel

`/admin/login` — manage Skills, Projects, Blog, Experience, Education, and site settings.

## Deploy

Push to `master` → GitHub Actions triggers a Fly.io deploy (~2 min).

Production secrets are stored in Fly.io (`flyctl secrets set KEY=VALUE`). Never commit secrets.
