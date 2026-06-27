# vodongha.id.vn

Personal portfolio website of **Võ Đông Hà** — Full-Stack Developer.

**Live:** [https://vodongha.id.vn](https://vodongha.id.vn) | **Admin:** [https://vodongha.id.vn/admin/login](https://vodongha.id.vn/admin/login)

[![CI](https://github.com/vodongha/vodongha-personal/actions/workflows/ci.yml/badge.svg)](https://github.com/vodongha/vodongha-personal/actions/workflows/ci.yml)
[![Tests](https://github.com/vodongha/vodongha-personal/actions/workflows/test.yml/badge.svg)](https://github.com/vodongha/vodongha-personal/actions/workflows/test.yml)
[![Deploy](https://github.com/vodongha/vodongha-personal/actions/workflows/deploy.yml/badge.svg)](https://github.com/vodongha/vodongha-personal/actions/workflows/deploy.yml)
[![Lint](https://github.com/vodongha/vodongha-personal/actions/workflows/lint.yml/badge.svg)](https://github.com/vodongha/vodongha-personal/actions/workflows/lint.yml)

---

## Features

### Public site
- **Landing page** — Hero, Skills & Technologies, Featured Projects, Work Experience, Education, Blog, Contact
- **Bilingual (VI / EN)** — toggle on every page; all content models have dual-language fields
- **Expand / collapse** — each section shows 2 items by default with "Show more"
- **Blog** — full posts with bilingual content, per-page Open Graph + Twitter Card meta tags
- **Contact form** — all fields required, blur validation, i18n error messages, "Send" disabled until valid; messages saved to DB + email notification via Resend; contact info (location, email, GitHub, LinkedIn, Facebook) loaded dynamically from DB (`SiteSettings`)
- **Visitor counter** — unique visitors tracked by IP, displayed in the footer
- **Browser timezone** — all timestamps display in the visitor's local timezone (detected via browser JS)
- **Dark / Light mode** — toggle in navbar; defaults to OS `prefers-color-scheme`; user choice persisted in localStorage
- **Blog enhancements** — view count per post, related posts, share buttons (copy link, LinkedIn, X), sticky table of contents (≥4 headings), copy button on code blocks, reading progress bar
- **Dynamic sitemap** — `/sitemap.xml` with all published posts; `robots.txt`
- **UX** — back-to-top button (appears at 400 px scroll), lazy loading on blog card cover images

### Live chat widget
- Floating chat button on all public pages
- Visitor fills a contact form (name *, phone *, email *) then chats in real-time
- **Searchable country code dropdown** — pure JS, full list with flag emoji, filterable by name or dial code; auto-detected from visitor IP via ipinfo.io
- Phone validated per country's numbering plan (libphonenumber); email validated on blur; leading zero stripped server-side
- Input lag-free — textarea and dial picker handled entirely in JS (no Blazor round-trip per keystroke); Enter sends, Shift+Enter newline
- Real-time typing indicators (both sides); read receipts: ✓ sent, ✓✓ read
- Date dividers (Today / Yesterday / dd/MM/yyyy) — fully bilingual VI/EN
- Unread badge on FAB; "New messages" divider when reopening with unread messages
- **Web Push notifications** — admin receives a browser push when a new message arrives; click notification jumps directly to that session
- Notification permission denied → amber banner with browser-specific "How to enable" link
- Messages forwarded to Telegram — one forum topic per session; topic auto-recreated if deleted
- SignalR reconnect with exponential backoff (handles Fly.io cold-start EAGAIN errors)
- Full light / dark mode support

### Admin panel
- **Dashboard** — overview stats (visitors, messages, chats, server health)
- **Skills** — add/edit/delete with proficiency and devicon class
- **Projects** — add/edit/delete (VI + EN), drag-to-reorder, paginated
- **Blog** — write and publish posts (VI + EN), auto-slug from Vietnamese title
- **Education / Experience** — manage timeline entries
- **Messages** — contact form submissions with unread badge, mark read, delete, reply
- **Chats** — live chat sessions; clicking a session opens it instantly; sessions auto-reorder by latest message; real-time typing indicator and read receipts; hub group rejoined automatically after SignalR reconnect; push notification URL includes `?session=ID` so clicking it auto-opens the right conversation
- **API Keys** — manage secrets (VAPID, Telegram, Resend…) stored encrypted in DB; synced from Fly.io ENV on first startup
- **Server Health** — live memory + DB response time charts (Chart.js), auto-refresh every 30 s; chart colors adapt to light/dark theme on toggle
- **Analytics** — self-hosted page view dashboard: daily views chart, top pages, top countries (geo IP via ip-api.com), top referrers; 7/30/90 day period selector; GDPR-safe (no IP stored)
- **Hồ sơ (Settings)** — bio (VI/EN), social links, avatar upload
- **CV / Resume PDF** — generate a polished PDF CV; 3 templates (Dark Sidebar, Minimal, Professional); template picker colors work in light mode
- **Dependencies tracker** — `/admin/dependencies` checks all NuGet packages, npm devDependencies, and CDN libraries against their registries; filter chips (All/Outdated/Mới nhất/NuGet/npm/CDN) + search box; 1-hour cache with manual refresh; `SemaphoreSlim` prevents thundering herd on cache miss
- **App version** — current release version displayed in footer and health page; injected automatically from git tag at build time
- **Server-side tables** — every admin entity table (Skills, Education, Experience, Blog, Messages, Projects) fetches one page at a time from the database; search, sort, and pagination run in SQL (`EF.Functions.ILike` + dynamic `OrderBy`) via `GET /api/admin/{entity}/paged`, so the browser never loads the full list. QuickGrid runs in `ItemsProvider` mode; Projects keeps drag-to-reorder within the current page
- **Shimmer skeleton loading** — all admin pages and all public sections show animated placeholders while data loads
- **Mobile responsive** — collapsible sidebar on desktop (64 px icon-only ↔ 220 px expanded, state persisted in localStorage); 4-item fixed bottom bar on screens ≤ 768 px (Menu / Dark / VI|EN / Logout); admin chat is full-screen on mobile with back button
- **Dark / Light mode** — complete coverage across public site, chat widget, admin panel, and Chart.js charts

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | Blazor Web App (.NET 10) — Home page: static SSR; Blog pages, NavBar, Footer, ChatWidget: `InteractiveWebAssembly` (WASM); Admin: `InteractiveWebAssembly` |
| API layer | ASP.NET Core Minimal APIs — 15 admin endpoint groups under `/api/admin/*` + 4 public groups (`/api/site`, `/api/blog`, `/api/ai`, `/api/auth`) |
| Database | PostgreSQL via [Neon](https://neon.tech) (Singapore) |
| ORM | Entity Framework Core — hybrid `int Id` (internal PK) + `Guid Rid` (external API identifier) on all 14 entities |
| Styling | SCSS — `Styles/app.scss` → public, `Styles/admin.scss` → admin |
| Real-time | ASP.NET Core SignalR |
| Charts | Chart.js 4.5.1 |
| Email | [Resend](https://resend.com) API |
| Chat backend | Telegram Bot API (forum topics per session) |
| PDF generation | [QuestPDF](https://www.questpdf.com) (Community) |
| Image processing | SkiaSharp — avatar square-crop before PDF rendering |
| Phone validation | Google libphonenumber (`libphonenumber-csharp`) |
| Geo IP | [ipinfo.io](https://ipinfo.io) (browser-side, free tier) |
| Deploy | [Fly.io](https://fly.io), region Singapore (`suspend` mode) |
| CI/CD | GitHub Actions — CI (build) + Tests (58 unit tests) on `develop`/PRs; Lint (dotnet format + ESLint + Stylelint) on PRs to `master`; deploy (requires all checks to pass) on merge to `master`; sync `develop` ← `master` after each merge |

---

## Project structure

```
vodongha-personal/
├── .github/
│   └── workflows/             # ci.yml, deploy.yml, lint.yml, pr-setup.yml, sync-develop.yml
├── Source/
│   ├── VodonghaPersonal.Server/      # ASP.NET Core host — static SSR shell + REST API
│   │   ├── Components/
│   │   │   ├── App.razor             # HTML root, SEO meta, sectionToggle JS, scripts
│   │   │   ├── Layout/               # MainLayout (static SSR shell)
│   │   │   ├── Pages/                # Home (static SSR), Login, Error, NotFound
│   │   │   │                         # Blog pages live in Client (InteractiveWebAssembly)
│   │   │   └── Sections/             # HeroSection, SkillsSection, ProjectsSection,
│   │   │                             #   ExperienceSection, EducationSection, BlogSection,
│   │   │                             #   ContactSection (all static SSR, sectionToggle via JS)
│   │   ├── Api/
│   │   │   ├── Admin*/               # 15 admin API groups under /api/admin/* (.RequireAuthorization())
│   │   │   ├── PublicBlogApi.cs      # GET /api/blog, GET /api/blog/{slug},
│   │   │   │                         #   GET /api/blog/{id}/related, POST /api/blog/{id}/view
│   │   │   ├── PublicAiApi.cs        # POST /api/ai/ask (Gemini, conversation history)
│   │   │   ├── PublicAuthApi.cs      # GET /api/auth/state (cookie auth state for WASM)
│   │   │   ├── PublicChatApi.cs      # Chat session + message REST endpoints
│   │   │   └── PublicSiteApi.cs      # GET /api/site/settings (incl. APP_VERSION from assembly),
│   │   │                             #   GET /api/site/visitor-count
│   │   ├── Data/                     # AppDbContext, AppDbContextFactory
│   │   ├── Hubs/                     # ChatHub (SignalR)
│   │   ├── Migrations/               # EF Core migrations
│   │   ├── Services/                 # HealthMonitorService, CostMonitorService, ...
│   │   ├── Styles/                   # app.scss, admin.scss, partials
│   │   └── wwwroot/js/               # admin.js, chat.js, charts, push.js
│   ├── VodonghaPersonal.Client/      # Blazor WASM — public interactive + admin
│   │   ├── ApiClients/
│   │   │   ├── Public/               # PublicBlogApiClient, PublicSiteApiClient,
│   │   │   │                         #   PublicChatApiClient, PublicAiApiClient
│   │   │   └── Admin/                # 14 admin HttpClient-based API clients
│   │   │       ├── SkillApiClient.cs
│   │   │       ├── ProjectApiClient.cs
│   │   │       ├── BlogApiClient.cs
│   │   │       └── ... (14 total)
│   │   ├── Components/
│   │   │   ├── Layout/               # NavBar, FooterSection, AdminLayout
│   │   │   ├── Pages/
│   │   │   │   ├── Public/           # BlogListPage, BlogPostPage (InteractiveWebAssembly)
│   │   │   │   └── Admin/            # All 16 admin pages (InteractiveWebAssembly)
│   │   │   └── Shared/               # BlogCard, BlogShareButtons, TableOfContents,
│   │   │                             #   RelatedPosts, ChatWidget, AiWidget,
│   │   │                             #   TimezoneDetector, ToastContainer, AdminNav,
│   │   │                             #   AdminBreadcrumb, ConfirmDialog, Pagination
│   │   ├── Services/                 # CookieAuthStateProvider, ToastService,
│   │   │                             #   AdminLocalizationService, LanguageService, TimezoneService
│   │   └── Program.cs                # WASM entry — registers HttpClient, auth, all API clients
│   └── VodonghaPersonal.Shared/      # Shared across Server + Client
│       ├── Models/                   # Skill, Project, BlogPost, Experience, Education,
│       │                             #   ContactMessage, ChatSession, ChatMessage, CvData
│       └── DTOs/
│           └── AdminDtos.cs          # API request/response types (DashboardStatsDto, ...)
├── Directory.Build.props             # Shared build properties (TargetFramework, Nullable, ImplicitUsings)
├── NuGet.Config                      # NuGet package source (api.nuget.org)
└── Test/
    ├── VodonghaPersonal.Client.Tests/    # NUnit + Shouldly — Client (WASM) unit tests
    ├── VodonghaPersonal.Server.Tests/    # NUnit + Shouldly — Server unit tests
    └── VodonghaPersonal.Shared.Tests/    # NUnit + Shouldly — Shared unit tests
```

---

## Local development

**Prerequisites:** .NET 10 SDK, PostgreSQL instance

```bash
git clone https://github.com/vodongha/vodongha-personal.git
cd vodongha-personal
git checkout develop
dotnet build   # builds Server + Client + Shared
```

Create `Source/VodonghaPersonal.Server/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "Admin": { "Username": "admin", "Password": "changeme" },
  "Email": { "ResendApiKey": "" },
  "Telegram": { "BotToken": "", "ChatId": "", "WebhookSecret": "" }
}
```

```bash
dotnet run --project Source/VodonghaPersonal.Server
```

EF Core migrations apply automatically on startup.

### SCSS

Compiled CSS is **not committed**. `dotnet build` compiles automatically via `AspNetCore.SassCompiler`.

| Source | Output |
|---|---|
| `Styles/app.scss` | `wwwroot/app.css` |
| `Styles/admin.scss` | `wwwroot/admin.css` |

---

## Git workflow

```
feature/* ──┐
bug/*    ──→  develop  →  PR → develop  →  PR → master  →  Fly.io auto-deploy (~2 min)
                                                                 ↓
hotfix/* ───────────────────────────────────→  PR → master       ↓
                                                           develop ← auto-synced
```

| Branch type | Base | PR target | Use for |
|---|---|---|---|
| `feature/description` | `develop` | `develop` | New features |
| `bug/description` | `develop` | `develop` | Non-urgent fixes |
| `hotfix/description` | `master` | `master` | Urgent production fixes |

**Feature / bug — standard flow:**

```bash
git checkout develop && git pull origin develop
git checkout -b feature/my-feature

# ... make changes ...
git push origin feature/my-feature
gh pr create --title "Add my feature" --base develop --head feature/my-feature

# After PR merged into develop → open PR develop → master
gh pr create --title "v2.x.x — description" --base master --head develop
```

**Hotfix — bypass develop:**

```bash
git checkout master && git pull origin master
git checkout -b hotfix/urgent-fix

# ... fix ...
git push origin hotfix/urgent-fix
gh pr create --title "Fix: description" --base master --head hotfix/urgent-fix
# After merge → sync-develop.yml automatically syncs develop ← master
```

`master` is branch-protected — no direct push. After every merge to `master`, tag a release and `develop` is automatically synced via `sync-develop.yml`.

---

## Fly.io secrets

| Secret | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL |
| `Admin__Username` / `Admin__Password` | Admin panel credentials |
| `Email__ResendApiKey` | Resend API key |
| `Telegram__BotToken` | Telegram bot token |
| `Telegram__ChatId` | Telegram group chat ID |
| `Telegram__WebhookSecret` | Webhook verification token |
| `FLY_API_TOKEN` | GitHub Actions deploy secret |

---

## License

[MIT](LICENSE)

---

## Built with

[Claude Code](https://claude.ai/code) by Anthropic. 🤖
