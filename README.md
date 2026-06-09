# vodongha.id.vn

Personal portfolio website of **Võ Đông Hà** — Full-Stack Developer.

**Live:** [https://vodongha.id.vn](https://vodongha.id.vn) | **Admin:** [https://vodongha.id.vn/admin/login](https://vodongha.id.vn/admin/login)

[![CI](https://github.com/vodongha/vodongha-personal/actions/workflows/ci.yml/badge.svg)](https://github.com/vodongha/vodongha-personal/actions/workflows/ci.yml)
[![Deploy](https://github.com/vodongha/vodongha-personal/actions/workflows/deploy.yml/badge.svg)](https://github.com/vodongha/vodongha-personal/actions/workflows/deploy.yml)

---

## Features

### Public site
- **Landing page** — Hero, Skills & Technologies, Featured Projects, Work Experience, Education, Blog, Contact
- **Bilingual (VI / EN)** — toggle on every page; all content models have dual-language fields
- **Expand / collapse** — each section shows 2 items by default with "Show more"
- **Blog** — full posts with bilingual content, per-page Open Graph + Twitter Card meta tags
- **Contact form** — all fields required, blur validation, i18n error messages, "Send" disabled until valid; messages saved to DB + email notification via Resend
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
- **Settings** — bio (VI/EN), social links, avatar upload
- **CV / Resume PDF** — generate a polished PDF CV; 3 templates (Dark Sidebar, Minimal, Professional); template picker colors work in light mode
- **Shimmer skeleton loading** — all admin pages and all public sections show animated placeholders while data loads
- **Mobile responsive** — fixed bottom navigation bar on screens ≤ 768 px; admin chat is full-screen on mobile with back button
- **Dark / Light mode** — complete coverage across public site, chat widget, admin panel, and Chart.js charts

### v2.0.4 — Security & quality hardening
- **Security** — `[Authorize(Roles="Admin")]` on SignalR admin group; push subscription `IsAdmin` determined server-side; constant-time login comparison; rate limiting (10 req / 5 min) on `/api/auth/login`
- **WCAG AA** — chat timestamp contrast fixed (2.5:1 → 4.6:1); admin button text contrast corrected for light mode; invalid `rgba(var(--css-var))` replaced with `color-mix()` throughout
- **Accessibility** — `aria-label` + `aria-expanded` on chat FAB; `:focus-visible` outlines on all interactive controls (nav toggles, dropdown items, expand buttons, delete button)
- **Loading bar scoping** — `type="button"` added to all non-submit buttons (toggles, scroll, pagination, chat send, toast close, etc.); loading bar no longer fires on in-page interactions
- **Code quality** — `ChatHubParser` shared helper extracted; `DotNetObjectReference` stored as field and disposed; `AdminChats` N+1 eliminated (in-memory session update); typing indicator topic ID cached to skip DB query per keystroke
- **DI fix** — removed `AddDbContext<AppDbContext>` (scoped) that conflicted with `AddDbContextFactory` (singleton) on startup

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | Blazor Web App (.NET 10, Interactive Server) |
| Database | PostgreSQL via [Neon](https://neon.tech) (Singapore) |
| ORM | Entity Framework Core |
| Styling | SCSS — `Styles/app.scss` → public, `Styles/admin.scss` → admin |
| Real-time | ASP.NET Core SignalR |
| Charts | Chart.js 4.4 |
| Email | [Resend](https://resend.com) API |
| Chat backend | Telegram Bot API (forum topics per session) |
| PDF generation | [QuestPDF](https://www.questpdf.com) (Community) |
| Image processing | SkiaSharp — avatar square-crop before PDF rendering |
| Phone validation | Google libphonenumber (`libphonenumber-csharp`) |
| Geo IP | ipinfo.io (browser-side, free tier) |
| Deploy | [Fly.io](https://fly.io), region Singapore (`suspend` mode) |
| CI/CD | GitHub Actions — CI on `develop`/PRs, deploy on merge to `master`, sync `develop` ← `master` after each merge |

---

## Project structure

```
vodongha-personal/
├── .github/
│   └── workflows/
│       ├── ci.yml              # Build check on develop push and PRs
│       ├── deploy.yml          # Fly.io deploy on merge to master
│       ├── pr-setup.yml        # Auto-assign, label, milestone, reviewer on PR open
│       └── sync-develop.yml    # Merge master → develop after every merge
├── Components/
│   ├── App.razor               # HTML root — SEO meta tags, client IP embedding, scripts
│   ├── Layout/                 # NavBar, MainLayout, FooterSection, AdminLayout
│   ├── Pages/
│   │   ├── Home.razor          # Landing page (InteractiveServer)
│   │   ├── Blog/               # BlogPostPage (dynamic SEO per post)
│   │   └── Admin/              # Login, Dashboard, Skills, Projects, Blog,
│   │                           #   Education, Experience, Contacts, Chats,
│   │                           #   Health, Settings (each: .razor + .razor.cs)
│   ├── Sections/               # HeroSection, SkillsSection, ProjectsSection,
│   │                           #   ExperienceSection, EducationSection,
│   │                           #   BlogSection, ContactSection
│   └── Shared/                 # ChatWidget, TimezoneDetector, BlogCard,
│                               #   ProjectCard, AdminNav, ConfirmDialog
├── Data/
│   ├── AppDbContext.cs
│   └── Models/                 # Skill, Project, BlogPost, Experience, Education,
│                               #   ContactMessage, SiteSetting, VisitorLog,
│                               #   ChatSession, ChatMessage
├── Hubs/
│   └── ChatHub.cs              # SignalR hub — session groups, typing events
├── Services/
│   ├── ChatService.cs          # Chat sessions, messages, Telegram webhook handler
│   ├── TelegramService.cs      # Telegram Bot API — topics, messages, typing
│   ├── HealthMonitorService.cs # Singleton — collects server metrics every 30s
│   ├── TimezoneService.cs      # Scoped — browser timezone for datetime conversion
│   └── ...                     # Blog, Project, Skill, Email, Language, etc.
├── Styles/
│   ├── app.scss                # Public site → wwwroot/app.css
│   ├── admin.scss              # Admin → wwwroot/admin.css
│   └── _*.scss                 # Partials (variables, base, nav, chat, ...)
├── wwwroot/js/
│   ├── admin.js                # Event delegation for admin UI
│   ├── chat.js                 # chatUtils — scroll, country detection (ipinfo.io)
│   └── healthChart.js          # Chart.js init/update/destroy wrappers
├── Migrations/
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
git checkout develop
```

Create `appsettings.Development.json`:

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
dotnet run
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

## Built with

[Claude Code](https://claude.ai/code) by Anthropic. 🤖
