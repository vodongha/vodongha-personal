# vodongha.id.vn

Personal website of **Võ Đông Hà** — Full-Stack Developer.

Live: [https://vodongha.id.vn](https://vodongha.id.vn)

## Stack

| Layer | Technology |
|---|---|
| Framework | Blazor Web App (.NET 10) |
| Database | PostgreSQL (Neon — Singapore) |
| ORM | Entity Framework Core |
| Styling | SCSS (compiled via AspNetCore.SassCompiler) |
| Deploy | Fly.io (Singapore region) |
| CI/CD | GitHub Actions |

## Project structure

```
vodongha/
├── Components/
│   ├── Layout/          # NavMenu, MainLayout, ReconnectModal
│   ├── Pages/           # Home, BlogPostPage, Error, NotFound
│   ├── Sections/        # HeroSection, SkillsSection, ProjectsSection, BlogSection, ContactSection
│   └── Shared/          # ProjectCard, BlogCard
├── Data/
│   ├── AppDbContext.cs
│   └── Models/          # BlogPost, Project, Skill, ContactMessage
├── Services/            # BlogService, ProjectService, SkillService, ContactService
├── Styles/              # SCSS partials + app.scss entry point
├── Migrations/          # EF Core migrations
├── Dockerfile
└── fly.toml
```

## Local development

**Prerequisites:** .NET 10 SDK, PostgreSQL (or Docker)

```bash
# Clone
git clone https://github.com/vodongha/vodongha.id.vn.git
cd vodongha.id.vn

# Set connection string
cp appsettings.Development.json.example appsettings.Development.json
# Edit DefaultConnection in appsettings.Development.json

# Run
dotnet run
```

The app auto-migrates the database on startup.

## Branch & PR workflow

```
master  ←── merge (auto-deploy to Fly.io)
  ↑
develop ←── merge (after Claude review)
  ↑
feature/xxx  ←── your work
```

1. Branch off `develop`: `git checkout -b feature/my-change develop`
2. Push and open a PR targeting `develop`
3. Claude AI agent reviews the PR automatically
4. On approval, the PR is merged into `develop`
5. Merging `develop` → `master` triggers deploy to production

See [CLAUDE.md](CLAUDE.md) for full conventions used by the AI agent.

## Contributors

| Name | Role |
|---|---|
| [Võ Đông Hà](https://github.com/vodongha) | Author & maintainer |
| [Claude](https://claude.ai) (Anthropic) | AI contributor — code review, PR merges, pair programming |

<a href="https://github.com/vodongha/vodongha.id.vn/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=vodongha/vodongha.id.vn" />
</a>
