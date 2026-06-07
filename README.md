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

### Live chat widget
- Floating chat button on all public pages
- Visitor fills a contact form (name *, phone *, email *) then chats in real-time
- Country code dropdown with flag emoji (full list via Google libphonenumber), **auto-detected from visitor's IP** via ipinfo.io (browser-side call); falls back to timezone
- Phone validated per country's numbering plan (libphonenumber); email validated on blur
- Name, phone, email all show inline blur-validation errors with i18n messages
- Auto welcome message on session start
- Real-time typing indicators (both sides)
- Read receipts: ✓ sent, ✓✓ read
- Date dividers (Today / Hôm nay / Yesterday / Hôm qua / dd/MM/yyyy) — fully translated VI/EN
- All chat widget strings translated (VI/EN), re-renders instantly on language switch
- Unread badge on FAB; "New messages" divider when reopening with unread messages
- Messages forwarded to a Telegram group — one forum topic per session
- Telegram topic auto-recreated if deleted; session delete synced with Telegram

### Admin panel
- **Dashboard** — overview stats (visitors, messages, chats, server health)
- **Skills** — add/edit/delete with proficiency and devicon class
- **Projects** — add/edit/delete (VI + EN), drag-to-reorder, paginated
- **Blog** — write and publish posts (VI + EN), auto-slug from Vietnamese title
- **Education / Experience** — manage timeline entries
- **Messages** — contact form submissions with unread badge, mark read, delete, reply
- **Chats** — live chat sessions with real-time messages, typing indicator, read receipts
- **Server Health** — live memory + DB response time charts (Chart.js), auto-refresh every 30s
- **Settings** — bio (VI/EN), social links, avatar upload
- **Mobile responsive** — fixed bottom navigation bar on screens ≤ 768px

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
develop  →  PR  →  master  →  Fly.io auto-deploy (~2 min)
                      ↓
                  develop  ← auto-synced by sync-develop.yml
```

`master` is branch-protected — no direct push. All changes via `develop` → PR. After merge, `develop` is automatically synced from `master` via the `sync-develop` workflow.

```bash
git checkout develop
# make changes...
git add <files>
git commit -m "describe the change"
git push origin develop
gh pr create --title "..." --base master --head develop
```

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
