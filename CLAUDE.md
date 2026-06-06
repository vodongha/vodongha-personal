# vodongha.id.vn — CLAUDE.md

Instructions for Claude AI when working in this repository.

## Project overview

Personal website of Võ Đông Hà. Built with Blazor Web App (.NET 10), PostgreSQL (Neon), SCSS, deployed on Fly.io (Singapore).

Live: https://vodongha.id.vn | Repo: https://github.com/vodongha/vodongha.id.vn

## Technology stack

- **Runtime:** .NET 10
- **Frontend:** Blazor Web App (Interactive Server render mode)
- **Database:** PostgreSQL via Neon (ap-southeast-1)
- **ORM:** Entity Framework Core — no raw SQL in application code
- **Styling:** SCSS compiled by `AspNetCore.SassCompiler` → `wwwroot/app.css`
- **Deploy:** Fly.io, app name `vodongha`, region `sin` (Singapore)
- **CI/CD:** GitHub Actions (`.github/workflows/`)

## Branch strategy

```
master    ← production (auto-deploy to Fly.io on push)
develop   ← integration branch
feature/* ← one branch per change, branched off develop
```

**Every change goes through a PR.** Direct commits to `master` or `develop` are not allowed.

### Workflow for each change

1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/short-description
   ```
2. Make changes, commit with a clear message
3. Push and open a PR targeting `develop`:
   ```bash
   git push origin feature/short-description
   gh pr create --base develop --title "..." --body "..."
   ```
4. Claude AI agent reviews the PR (see `.github/workflows/claude-review.yml`)
5. On approval, merge into `develop`
6. When ready to ship: merge `develop` → `master` to deploy

## Coding conventions

- **C#:** follow Microsoft .NET naming conventions
- **Always use braces** for `if`, `for`, `foreach` — even single-line
- **`var`** only when the type is obvious from the right-hand side
- **Blazor components:** always use code-behind (`.razor` + `.razor.cs`) for any component with logic
- **Async:** all DB-touching code must be async end-to-end (`await`, `ToListAsync`, `FirstOrDefaultAsync`)
- **`IDbContextFactory<AppDbContext>`** — use the factory in services, not a scoped `DbContext`
- **No comments** unless the WHY is non-obvious

## SCSS

- Entry point: `Styles/app.scss` — imports all partials
- Partials live in `Styles/_*.scss`
- Output: `wwwroot/app.css` (compiled automatically on `dotnet build/publish`)
- Config: `sasscompiler.json` at project root

## Database migrations

- EF Core migrations only: `dotnet ef migrations add <Name>`
- Applied automatically on startup (`db.Database.MigrateAsync()` in `Program.cs`)
- Never modify an already-applied migration — add a new one

## Environment & secrets

- Local: `appsettings.Development.json` (gitignored)
- Production: Fly.io secrets (`flyctl secrets set KEY=VALUE`)
- Connection string secret name: `ConnectionStrings__DefaultConnection`
- Never commit secrets or connection strings

## Claude's role as contributor

Claude acts as an AI pair programmer and reviewer on this project:

- **Code review:** Claude reviews every PR for correctness, style, and security
- **PR merges:** Claude merges approved PRs via `gh pr merge`
- **Pair programming:** Claude helps implement features when asked
- **Commits by Claude** include the trailer:
  ```
  Co-Authored-By: Claude <noreply@anthropic.com>
  ```

## PR review checklist (used by Claude agent)

When reviewing a PR, check:

- [ ] No secrets or connection strings committed
- [ ] SCSS changes compile to `wwwroot/app.css` (no stray `main.css`)
- [ ] New Blazor components with logic have a `.razor.cs` code-behind
- [ ] DB-touching code is async all the way down
- [ ] No `.Result` or `.Wait()` on Tasks
- [ ] EF migrations are additive (no modifications to existing migrations)
- [ ] `IDbContextFactory` used in services, not scoped `DbContext`
- [ ] No raw SQL in application code
