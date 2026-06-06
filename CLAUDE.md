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
| SCSS | Two entry points compiled by `AspNetCore.SassCompiler` on `dotnet build`: `Styles/app.scss` → `wwwroot/app.css` (public), `Styles/admin.scss` → `wwwroot/admin.css` (admin). Compiled CSS is **gitignored** — never commit `wwwroot/app.css` or `wwwroot/admin.css`. |
| Email | Resend API (`Email__ResendApiKey`). Sender: `no-reply@vodongha.id.vn`, recipient: `REDACTED_EMAIL` |
| Chat | Telegram Bot API + SignalR (`Microsoft.AspNetCore.SignalR.Client`). Secrets: `Telegram__BotToken`, `Telegram__ChatId`, `Telegram__WebhookSecret` |
| Deploy | Fly.io, app `vodongha`, region `sin`. Merge PR to `master` → auto-deploy (~2 min) |
| Migrations | EF Core, applied automatically on startup via `MigrateAsync()` in `Program.cs` |

## Git workflow

**All commits go to `develop`. `master` is production-only — never commit directly to `master`.**

```
develop  →  Pull Request  →  master  →  Fly.io auto-deploy
```

### Daily workflow

```bash
# Always work on develop
git checkout develop

# Make changes, commit
git add <files>
git commit -m "short description of what changed"
git push origin develop

# Open PR: develop → master
gh pr create --title "PR title" --body "description" --base master --head develop

# After merge, master deploys automatically to Fly.io
```

### PR conventions

- One PR per feature or fix
- Title: short imperative phrase ("Add blog post", "Fix mobile layout")
- Base branch: always `master`
- Merge with merge commit (no squash, no rebase)

### Releases

After significant merges, tag a release on GitHub:

```bash
git tag v1.x.x
git push origin v1.x.x
gh release create v1.x.x --title "v1.x.x — description" --notes "changelog"
```

## Solution structure

```
vodongha-personal/
├── Components/
│   ├── App.razor                     # Root — <head> SEO meta tags (OG, Twitter Card, canonical) + <script> tags
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
│   │   │   ├── AdminEducation.razor + .cs
│   │   │   ├── AdminExperience.razor + .cs
│   │   │   ├── AdminContacts.razor + .cs    # unread badge, mark read, reply
│   │   │   └── AdminSettings.razor + .cs    # avatar upload, social links, bio
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
│   ├── _base.scss                    # Global styles + .section layout
│   ├── _nav.scss
│   ├── _hero.scss
│   ├── _skills.scss
│   ├── _projects.scss
│   ├── _timeline.scss                # Shared by Experience + Education sections
│   ├── _blog.scss
│   ├── _contact.scss
│   ├── _footer.scss
│   └── _reconnect.scss
├── wwwroot/
│   └── js/admin.js                   # Event delegation for admin UI (select arrow open/close)
├── Migrations/                       # EF Core — never modify existing migrations
├── Program.cs                        # DI, middleware (visitor tracking), auth, routes
├── Dockerfile
├── .dockerignore                     # Excludes wwwroot/app.css + admin.css → Docker forces Dart Sass recompile
├── .gitignore                        # Excludes wwwroot/app.css, admin.css, *.css.map
├── fly.toml
└── vodongha-personal.csproj
```

## SCSS pipeline

Two separate CSS outputs — public and admin are completely independent:

| SCSS entry | Output | Layout that loads it |
|---|---|---|
| `Styles/app.scss` | `wwwroot/app.css` | `MainLayout.razor` |
| `Styles/admin.scss` | `wwwroot/admin.css` | `AdminLayout.razor` |

**Compiled CSS is gitignored.** Never commit `wwwroot/app.css` or `wwwroot/admin.css`.

`AspNetCore.SassCompiler` runs Dart Sass with `--update` during `dotnet build`. The `--update` flag skips recompile if the output `.css` is newer than the source `.scss`.

**If CSS appears stale locally**, force recompile by running Dart Sass directly:

```
dart.exe sass.snapshot --style=expanded --no-source-map Styles\app.scss wwwroot\app.css
dart.exe sass.snapshot --style=expanded --no-source-map Styles\admin.scss wwwroot\admin.css
```

Find `dart.exe` in the NuGet package cache under `AspNetCore.SassCompiler` tools.

## Blazor — important rules

**Scripts only execute from `App.razor`** — not from `.razor` layout or page components. All `<script>` tags must be placed in `App.razor` (before `</body>`). Currently: `<script src="js/admin.js"></script>`.

**Use event delegation** for admin JS — Blazor InteractiveServer renders elements after the WebSocket connects, so `document.querySelectorAll()` called immediately returns nothing. Attach listeners to `document` instead:

```js
document.addEventListener('mousedown', function (e) {
    var el = e.target.closest('.some-selector');
    if (!el) return;
    // handle event
});
```

**Static asset fingerprinting** — .NET 10 fingerprints CSS/JS URLs at build time. `@Assets["admin.css"]` resolves to `/admin.j51ad4dks9.css`. When CSS changes, the hash changes and browsers are forced to reload the file.

## Admin panel

- Login: `/admin/login` → POST `/admin/do-login` → cookie auth (7-day sliding expiration)
- Each admin page uses `@layout AdminLayout` and `@attribute [Authorize]`
- Direct DB access via `IDbContextFactory<AppDbContext>` — no separate API layer
- All admin pages follow the `.razor` + `.razor.cs` code-behind pattern

### Blazor code-behind pattern

Every admin page uses a `.razor.cs` partial class:

```csharp
public partial class AdminSkills : ComponentBase
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
}
```

The `.razor` file contains only markup — no `@code { }` block, no `@inject` directives.

### AdminProjects — manual pagination + drag-to-reorder

AdminProjects uses a hand-rolled `<table>` (not QuickGrid) because it needs HTML5 drag-and-drop for ordering.

Drag indices must be translated to global list indices when pagination is active:

```csharp
// Local index = position on current page (0..pageSize-1)
// Global index = position in full list
int globalIndex = _page * _pageSize + localIndex;
```

### AdminSkills / AdminBlog / AdminEducation / AdminExperience — QuickGrid

QuickGrid renders empty filler rows as `<tr><td></td>...</tr>`. They are hidden in `_admin-styles.scss`:

```scss
tbody tr:not(:has(> td:not(:empty))) { display: none !important; }
```

### AdminBlog — auto-slug

`OnTitleInput` → `GenerateSlug()` — lowercases, strips Vietnamese diacritics, replaces spaces with hyphens. Auto-filled only when adding (not editing).

### Admin mobile responsive

On screens ≤ 768px, the sidebar becomes a fixed bottom navigation bar:

- `.admin-shell` → `display: block !important` (removes flex layout)
- `.admin-sidebar` → `position: fixed !important; bottom: 0; left: 0; right: 0; width: 100%`
- `.admin-main` → `width: 100%; padding-bottom: 5rem` (space for bottom nav)

All critical mobile overrides use `!important` to guarantee they win over desktop flex styles.

## Chat widget

Floating chat button (bottom-right) on all public pages via `MainLayout.razor`. Users fill a contact form (name, phone, email), then chat in real-time.

### Architecture

```
User types message
  → ChatService.SendUserMessageAsync()
  → saves ChatMessage (IsFromUser=true) to DB
  → TelegramService creates forum topic (first message) or sends to existing topic
  → Admin replies in Telegram group
  → POST /api/telegram/webhook fires
  → ChatService.HandleTelegramWebhookAsync()
  → saves ChatMessage (IsFromUser=false) to DB
  → IHubContext<ChatHub>.Clients.Group("session_{id}").SendAsync("ReceiveMessage", ...)
  → ChatWidget receives via HubConnection, updates UI
```

Admin can also reply from `/admin/chats` → `ChatService.SendAdminReplyAsync()` → pushes via SignalR AND sends to Telegram topic.

### Key files

| File | Role |
|---|---|
| `Hubs/ChatHub.cs` | SignalR hub — `JoinSession(sessionId)` adds client to group |
| `Services/TelegramService.cs` | Bot API calls: `CreateTopicAsync`, `SendMessageAsync` |
| `Services/ChatService.cs` | Business logic: sessions, messages, webhook handler, SignalR push |
| `Components/Shared/ChatWidget.razor` + `.cs` | Public chat widget (floating button, form, chat window) |
| `Components/Pages/Admin/AdminChats.razor` + `.cs` | Admin chat management page |
| `wwwroot/js/chat.js` | `chatUtils.scrollToBottom(elementId)` — called from C# via IJSRuntime |

### Session persistence

`ProtectedLocalStorage` stores `chatSessionId` in the browser. On page reload, the widget restores the session and reconnects to the SignalR group.

### Telegram setup

- Bot: `@vodongha_personal_bot` (Forum group: `vodongha-personal Chat`)
- Webhook registered at: `https://vodongha.fly.dev/api/telegram/webhook`
- Each chat session = one forum topic in the group (title: `Name | Email`)
- Fly.io secrets: `Telegram__BotToken`, `Telegram__ChatId`, `Telegram__WebhookSecret`

**Note:** `vodongha.id.vn` may fail DNS resolution from Telegram servers — use `vodongha.fly.dev` for the webhook URL.

## Coding conventions

- Microsoft .NET naming conventions throughout
- **Always use braces** for `if`, `for`, `foreach` — even single-line
- `var` only when the type is obvious from the right-hand side
- All DB-touching code async end-to-end: `await`, `ToListAsync()`, `FirstOrDefaultAsync()`
- `await using var db = await DbFactory.CreateDbContextAsync()` — factory pattern, never scoped DbContext
- No `.Result` or `.Wait()` on Tasks
- No comments unless the WHY is non-obvious

## Admin table styles

Admin tables (both `<table class="quickgrid">` and `<QuickGrid class="quickgrid">`) share the same CSS:

- Header: dark background (`#111827`), green-tinted text (`#6ee7b7`)
- Odd rows: `#0d0d0d`, even rows: `#121212`
- Hover: `#1e2a24` (overrides stripe with `!important`)

## i18n

`LanguageService` handles VI/EN toggle. Default language: **English**.

- UI strings: `Lang.T("key")`
- Content with dual fields: `Lang.IsVi ? item.Description : (item.DescriptionEn ?? item.Description)`
- Components that react to language changes need `@rendermode InteractiveServer` + `Lang.OnChange += StateHasChanged`

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

## Visitor tracking

`VisitorService` deduplicates by IP — each unique IP stored once in `VisitorLogs`.

Middleware in `Program.cs` fires on GET requests to non-static, non-admin, non-framework paths. Reads real IP from `X-Forwarded-For` (Fly.io proxy). Localhost (`::1`, `127.x`, `10.x`) excluded.

## Sections — expand/collapse

All 5 landing page sections (Skills, Projects, Experience, Education, Blog) show 2 items initially with a "Show more / Thu gọn" button. Button only renders when total count > 2. i18n keys: `common.showmore` / `common.showless`.

## SEO

`App.razor` — static Open Graph, Twitter Card, canonical meta for homepage.

`BlogPostPage.razor` — per-post `<HeadContent>` with dynamic `og:title`, `og:description`, `og:url`, `og:image`, `twitter:card`, `link rel=canonical` from blog post fields.

## Database migrations

Never modify an existing migration. Add new migrations with:

```bash
dotnet ef migrations add <MigrationName>
```

Migrations apply automatically on startup. Seed data lives in `AppDbContext.OnModelCreating` via `HasData`.

## Fly.io secrets

| Secret | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL |
| `Admin__Username` | Admin panel login |
| `Admin__Password` | Admin panel login |
| `Email__ResendApiKey` | Resend API key for contact form |

Set with: `flyctl secrets set KEY=VALUE`
