# vodongha.id.vn — CLAUDE.md

## Project overview

Personal portfolio website of Võ Đông Hà. Blazor Web App (.NET 10) + PostgreSQL (Neon, Singapore) + SCSS dark theme, deployed on Fly.io.

- **Live:** https://vodongha.id.vn
- **Repo:** https://github.com/vodongha/vodongha-personal
- **Admin:** https://vodongha.id.vn/admin/login
- **Project file:** `vodongha-personal.csproj` (RootNamespace: `VodonghaPersonal`, AssemblyName: `vodongha-personal`)

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
| Real-time | ASP.NET Core SignalR (`ChatHub`) — session groups, admin group, typing events |
| Charts | Chart.js 4.4 via CDN — wrapped in `wwwroot/js/healthChart.js` |
| PDF generation | QuestPDF 2026.5.0 Community — `CvPdfService.Generate(cv, template, avatarBytes)` dispatches to 3 template methods; `QuestPDF.Settings.License = LicenseType.Community` set at call site |
| Image processing | SkiaSharp 3.116.1 — `CropSquareTop(byte[])` crops image to square (center-horizontal, top-vertical) before QuestPDF so `FitArea()` fills the circle without letterboxing |
| Phone validation | `libphonenumber-csharp` — validates per country's numbering plan (`IsValidNumberForRegion`) |
| Geo IP | ipinfo.io — called from browser JS (`chatUtils.detectCountry`) for country code detection |
| Deploy | Fly.io, app `vodongha`, region `sin`, `auto_stop_machines = "suspend"`. Merge PR to `master` → auto-deploy (~2 min) |
| CI | GitHub Actions `ci.yml` — `dotnet build` on every push to `develop` and on PRs to `master` |
| Migrations | EF Core, applied automatically on startup via `MigrateAsync()` in `Program.cs` |

## Git workflow

```
feature/* ──┐
bug/*    ──→  develop  →  PR → develop (merged)  →  PR → master  →  Fly.io auto-deploy
                                                                          ↓
hotfix/* ──────────────────────────────────────────────→  PR → master    ↓
                                                                    develop ← auto-synced by sync-develop.yml
```

### Branch rules

| Branch type | Base branch | PR target | When to use |
|---|---|---|---|
| `feature/short-description` | `develop` | `develop` | New feature |
| `bug/short-description` | `develop` | `develop` | Non-urgent bug fix |
| `hotfix/short-description` | `master` | `master` | Urgent production fix |

**`master` is production-only — never commit directly.** All feature/bug work goes through `develop` first. Hotfixes bypass `develop` and go straight to `master`; `develop` picks them up via auto-sync.

### Feature / bug workflow

```bash
# 1. Branch from develop
git checkout develop && git pull origin develop
git checkout -b feature/my-feature   # or bug/my-fix

# 2. Make changes, commit, push
git add <files>
git commit -m "short description"
git push origin feature/my-feature

# 3. PR: feature → develop
gh pr create --title "Add my feature" --base develop --head feature/my-feature

# 4. After PR merged into develop → immediately open PR: develop → master
gh pr create --title "v2.x.x — description" --base master --head develop

# 5. Merge develop → master → Fly.io auto-deploys
# 6. sync-develop.yml auto-syncs develop ← master (no action needed)
```

### Hotfix workflow

```bash
# 1. Branch from master
git checkout master && git pull origin master
git checkout -b hotfix/urgent-fix

# 2. Make changes, commit, push
git add <files>
git commit -m "Fix: short description"
git push origin hotfix/urgent-fix

# 3. PR: hotfix → master (bypasses develop)
gh pr create --title "Fix: urgent description" --base master --head hotfix/urgent-fix

# 4. Merge → Fly.io auto-deploys
# 5. sync-develop.yml auto-syncs develop ← master (hotfix lands in develop automatically)
```

### PR conventions

- Title: short imperative phrase ("Add blog pagination", "Fix mobile nav overlap")
- Merge with **merge commit** (no squash, no rebase)
- Tag a release after each merge to `master` (see Releases below)

### Releases

After each merge to `master`, tag the release:

```bash
git checkout master && git pull origin master
git tag v2.x.x
git push origin v2.x.x
gh release create v2.x.x --title "v2.x.x — description" --notes "changelog"
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
│   │   └── ReconnectModal.razor      # Blazor reconnect overlay — bilingual VI/EN; circuit retries configured in App.razor Blazor.start()
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
│   │   │   ├── AdminChats.razor + .cs       # live chat sessions, real-time messages, typing, read receipts
│   │   │   ├── AdminHealth.razor + .cs      # server health: memory + DB ping charts, snapshot table
│   │   │   ├── AdminSettings.razor + .cs    # avatar upload, social links, bio
│   │   │   ├── AdminCv.razor + .cs          # CV PDF download — template picker, live preview per template
│   │   │   └── AdminAnalytics.razor + .cs   # /admin/analytics — page views, daily chart, top pages/countries/referrers
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
│       ├── ConfirmDialog.razor       # Delete confirmation modal (type "Delete" to enable button)
│       ├── ChatWidget.razor + .cs    # Floating chat button on all public pages (InteractiveServer)
│       ├── AdminNav.razor + .cs      # Shared admin sidebar — collapsible groups (Portfolio/Communication/Insights/System), auto-opens active group; collapsible sidebar (64px icon-only ↔ 220px expanded, chevron toggle in logo area, state in localStorage key `admin-sidebar-collapsed`); compact icon-only top controls (Website/Dark/Lang, labels hidden on desktop, `title` tooltip on hover); all group headers and nav items have `title` tooltips; group header style: no uppercase, 0.82rem, color-text-dim; clicking a group header when collapsed expands sidebar first; desktop only — mobile bottom bar (4 items: Menu / Dark icon-only / VI|EN active-only / Logout icon-only; Website removed)
│       └── TimezoneDetector.razor    # Invisible InteractiveServer component — reads browser IANA timezone via JS on first render, stores in TimezoneService
├── Data/
│   ├── AppDbContext.cs               # EF context — DbSets + index/constraint definitions only. No HasData() seed — all data managed at runtime via Admin UI.
│   └── Models/
│       ├── Skill.cs
│       ├── Project.cs
│       ├── BlogPost.cs
│       ├── Experience.cs
│       ├── Education.cs
│       ├── ContactMessage.cs
│       ├── SiteSetting.cs            # Key-value store for site metadata
│       ├── VisitorLog.cs             # Unique visitors by IP — IpAddress, FirstSeenAt, UserAgent
│       └── PageView.cs               # Analytics — Path, Referrer, Country (no IP stored — GDPR), CreatedAt
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
│   ├── VisitorService.cs             # LogAsync(ip) — deduplicated by IP; GetCountAsync()
│   ├── ChatService.cs                # Sessions, messages, Telegram webhook handler, SignalR push
│   ├── TelegramService.cs            # Bot API: CreateTopicAsync, SendMessageAsync (returns TopicDeleted flag), DeleteTopicAsync, SendTypingAsync
│   ├── HealthMonitorService.cs       # Singleton + IHostedService — collects metrics every 30s, 24-snapshot circular buffer
│   ├── TimezoneService.cs            # Scoped — stores browser IANA timezone; ToUserTime(DateTime utc); fires OnTimezoneSet event for component re-render
│   ├── CvPdfService.cs               # QuestPDF: Generate(CvData, template, avatarBytes?) → byte[]; 3 templates (0=DarkSidebar 1=Minimal 2=Professional); CropSquareTop() via SkiaSharp
│   └── AnalyticsService.cs           # Page view tracking — TrackAsync (fire-and-forget from middleware), geo lookup via ip-api.com (24h cache), daily/top queries
├── Styles/
│   ├── app.scss                      # Public site entry point — imports all _*.scss partials
│   ├── admin.scss                    # Admin entry point — imports _admin-styles.scss
│   ├── _admin-styles.scss            # All admin panel styles (BEM: .admin-*) — desktop only
│   ├── _admin-mobile.scss            # Admin mobile overrides — bottom nav layout, page-specific mobile queries
│   ├── _client-mobile.scss           # Cross-component client mobile overrides (stub)
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
├── Hubs/
│   └── ChatHub.cs                    # SignalR: JoinSession, LeaveSession, JoinAdminGroup, StartTyping, StopTyping, MarkRead
├── wwwroot/
│   └── js/
│       ├── admin.js                  # Event delegation for admin UI (select arrow open/close)
│       ├── analytics-charts.js       # Chart.js wrappers for analytics page (renderLine, renderBar, destroy)
│       ├── chat.js                   # chatUtils.scrollToBottom(id), chatUtils.scrollToUnread(id)
│       └── healthChart.js            # healthChart.init/update/destroy — Chart.js wrappers
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
| `Styles/admin.scss` (`@use admin-styles + admin-mobile`) | `wwwroot/admin.css` | `AdminLayout.razor` |

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

**Blazor circuit reconnect config** — `App.razor` uses `autostart="false"` on `blazor.web.js` and calls `Blazor.start({ circuit: { reconnectionOptions: { maxRetries: 10, retryIntervalMilliseconds: (n) => n < 3 ? 1000 : n < 6 ? 3000 : 6000 } } })`. This handles Fly.io `suspend` mode wakeup (~3-5s cold start) without showing a reconnect error to the user.

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
| `Hubs/ChatHub.cs` | SignalR hub — `JoinSession`, `LeaveSession`, `JoinAdminGroup`, `StartTyping`, `StopTyping`, `MarkRead` |
| `Services/TelegramService.cs` | `CreateTopicAsync`, `SendMessageAsync` (returns `(MessageId, TopicDeleted)` tuple), `DeleteTopicAsync`, `SendTypingAsync` |
| `Services/ChatService.cs` | Sessions, messages, auto welcome, webhook handler, Telegram topic recreation on 404, SignalR push |
| `Components/Shared/ChatWidget.razor` + `.cs` | Public chat widget — form, chat state, typing, read receipts, date dividers, optimistic UI |
| `Components/Pages/Admin/AdminChats.razor` + `.cs` | Admin chat management — live updates, typing, read receipts, delete with Telegram sync |
| `wwwroot/js/chat.js` | `chatUtils.scrollToBottom(id)`, `chatUtils.scrollToUnread(id)`, `chatUtils.detectCountry()` |

### Session persistence

`ProtectedLocalStorage` stores `chatSessionId`, `chatLastReadId`, `chatAdminReadId` in the browser. On reload the widget restores the session, reconnects SignalR, and recomputes unread count.

### Chat features

- **Contact form** — name, phone, and email are all required (`CanStartChat` checks all three); blur validation with i18n error messages
- **Optimistic UI** — messages appear instantly; replaced with real DB ID on server response
- **Typing indicator** — `StartTyping` / `StopTyping` SignalR events, auto-stop after 2s idle
- **Read receipts** — `MarkRead` SignalR event; ✓ = sent, ✓✓ = admin/user read
- **Date dividers** — "Today" / "Yesterday" / "dd/MM/yyyy" between messages on different days (user timezone, translated VI/EN)
- **Unread divider** — "New messages" divider on reopen; badge count on FAB
- **Auto welcome** — `CreateSessionAsync` saves a greeting `ChatMessage (IsFromUser=false)` immediately after session creation
- **Telegram topic lifecycle** — topic created on first user message; auto-recreated with contact re-pin if deleted on Telegram side; `DeleteSessionAsync` calls `DeleteTopicAsync` before DB delete
- **Full i18n** — all chat widget UI strings use `Lang.T("chat.*")` keys; widget re-renders on `Lang.OnChange`; `placeholder="email@example.com"` kept as-is (universal)
- **Country code dropdown** — full country list from `libphonenumber-csharp`; flag emoji auto-generated from ISO region code; auto-detected from visitor IP via `chatUtils.detectCountry()` (calls `ipinfo.io/json` from browser); falls back to timezone detection
- **Phone validation** — `PhoneNumberUtil.IsValidNumberForRegion` per selected country; shown on blur; leading `0` auto-stripped, country dial code prepended on submit
- **Blur validation** — errors shown only after user leaves the field (`@onblur`), not on page load

### Telegram setup

- Bot: `@vodongha_personal_bot` (Forum group: `vodongha-personal Chat`)
- Webhook registered at: `https://vodongha.fly.dev/api/telegram/webhook`
- Each chat session = one forum topic in the group (title: `Name | Email`)
- Fly.io secrets: `Telegram__BotToken`, `Telegram__ChatId`, `Telegram__WebhookSecret`

**Note:** `vodongha.id.vn` may fail DNS resolution from Telegram servers — use `vodongha.fly.dev` for the webhook URL.

## Server health monitor

`HealthMonitorService` is a **singleton + `IHostedService`**. It collects metrics every 30 seconds into a 24-entry circular `LinkedList<HealthMetricSnapshot>`:
- `MemoryMb` — `Process.GetCurrentProcess().WorkingSet64`
- `DbPingMs` — ADO.NET `SELECT 1` round-trip time
- `DbHealthy` — whether the DB ping succeeded
- `ThreadCount` — `Process.GetCurrentProcess().Threads.Count`

`AdminHealth.razor` shows stat cards (Uptime, Memory, Threads, DB Status, Started At), two Chart.js line charts (memory + DB ping), and a sortable/paginated snapshot table. Auto-refreshes every 30 seconds via a background loop.

**Registration:** singleton + hosted service share the same instance:
```csharp
builder.Services.AddSingleton<HealthMonitorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<HealthMonitorService>());
```

## Analytics

`AnalyticsService` tracks page views for the admin analytics dashboard. Registered as `AddScoped<AnalyticsService>()`.

### Data model

`PageView` — `Path`, `Referrer` (domain only), `Country` (from geo IP), `CreatedAt`. **No IP address stored** (GDPR). Index on `CreatedAt`.

### Tracking

Middleware in `Program.cs` calls `analyticsSvc.TrackAsync(path, referrer, ip)` fire-and-forget on every qualifying GET request (same exclusion rules as visitor tracking).

`TrackAsync` checks an in-memory `ConcurrentDictionary<string, (Country, ExpiresAt)>` geo cache (static, survives scopes). On cache miss, `LookupAndCacheAsync(ip)` calls `http://ip-api.com/json/{ip}?fields=country` (free, 45 req/min) with a 3-second timeout. The country is awaited before saving the `PageView` record so it is always stored on the first visit.

### Dashboard — `/admin/analytics`

- Period selector: 7 / 30 / 90 days
- Stat cards: views in period, all-time total, daily average
- Daily views line chart (Chart.js)
- Top pages + Top countries bar charts + tables (ellipsis truncation, tooltip on hover, max-height scroll)
- Top referrers table with relative bar
- Chart rendering uses `_pendingChartRender` flag — set after data load, consumed in `OnAfterRenderAsync` — so charts render exactly once per data load without double-render race.

## Blog

Blog posts are managed at runtime via `/admin/blog`. No seed data in code — all posts live in the database.

**SEO rules for new posts:** Title 50–60 chars, Summary (meta description) 145–158 chars, H2 first (no H1 in content body), FAQ section, internal links, bilingual VI/EN content.

## CV PDF

`CvPdfService` generates an A4 PDF CV from live database data. Registered as `AddScoped<CvPdfService>()`.

### Endpoint

`GET /api/cv/download?template={0|1|2}` — requires auth (`.RequireAuthorization()`). Loads `SiteSettings` + all Skills/Experiences/Educations/Projects, reads avatar bytes directly from `env.WebRootPath` filesystem (not via HTTP self-request which fails on Fly.io), calls `CvPdfService.Generate()`, returns `application/pdf`.

### Templates

| # | Name | Layout |
|---|---|---|
| 0 | Dark Sidebar | Dark `#0f1923` sidebar (175pt) + white main; green `#6ee7b7` accents |
| 1 | Minimal | White full-width header (avatar + name + contacts) + green divider + 2-col body (160pt skills left, content right) |
| 2 | Professional | Navy `#1e3a5f` full-width header + blue `#3b82f6` 4pt stripe + 2-col body (170pt skills left, content right) |

### Avatar pipeline

1. `Program.cs`: reads avatar from `wwwroot` filesystem for relative URLs; HTTP download for absolute URLs
2. `CvPdfService.CropSquareTop(byte[])`: SkiaSharp — crops to square (center-X, `y=0` top-anchor to show face), returns JPEG bytes
3. QuestPDF: `AutoItem().AlignMiddle().Width(N).Height(N).CornerRadius(N/2).AlignCenter().AlignMiddle().Image(bytes).FitArea()` — `AutoItem` instead of `ConstantItem` ensures square container so `FitArea()` centers correctly

### QuestPDF rules

- Always use `FitArea()` not `FitWidth()` — `FitWidth()` can overflow height constraints
- Skills use `Inlined` container (not `Row` + `AutoItem`) — `Row` doesn't wrap and causes overflow crash
- Font: `FontFamily("Noto Sans")` — requires `fonts-noto` + `fonts-liberation` + `libfontconfig1` in Dockerfile for Vietnamese diacritic support on Linux
- `QuestPDF.Settings.License = LicenseType.Community` must be set before every `Document.Create()` call

### Admin preview

`AdminCv.razor` shows a live HTML preview that matches each PDF layout:
- Template 0: uses existing `.cv-preview` (dark sidebar CSS)
- Template 1: uses `.cv-minimal-preview` (white header row + green 2-col)
- Template 2: uses `.cv-pro-preview` (navy header + blue 2-col)

CSS: `.cv-preview__entry-link`, `.cv-preview__tech`, `.cv-preview__entry-sub` overridden per theme inside `.cv-pro-preview { }` wrapper to use blue instead of the site's default green.

## Timezone detection

`TimezoneService` is **scoped per Blazor circuit**. `TimezoneDetector.razor` (`@rendermode InteractiveServer`) reads `Intl.DateTimeFormat().resolvedOptions().timeZone` via JS on first render and calls `TzService.Set(ianaId)`.

After `Set()`, the service fires `OnTimezoneSet` — all components that display times subscribe in `OnInitialized` and call `InvokeAsync(StateHasChanged)` to re-render with the correct timezone.

`TimezoneDetector` is embedded in both `MainLayout.razor` and `AdminLayout.razor`.

**Usage:** `Tz.ToUserTime(utcDateTime).ToString("HH:mm")` — never use `.ToLocalTime()` (converts to server timezone).

## Coding conventions

- Microsoft .NET naming conventions throughout
- **Always use braces** for `if`, `for`, `foreach` — even single-line
- `var` only when the type is obvious from the right-hand side
- All DB-touching code async end-to-end: `await`, `ToListAsync()`, `FirstOrDefaultAsync()`
- `await using var db = await DbFactory.CreateDbContextAsync()` — factory pattern, never scoped DbContext
- No `.Result` or `.Wait()` on Tasks
- No comments unless the WHY is non-obvious

## Security & code hygiene

### Language — English only

**All source code must be in English** — comments, variable names, string literals, config file annotations, SCSS comments, JS comments, TOML comments, YAML comments, `.env.example` annotations.

- No Vietnamese in any source file. This is a hard rule, not a style preference.
- If you write or review code containing Vietnamese text in comments or config annotations, translate it to English immediately.
- UI-facing strings (the actual text shown to users) remain bilingual VI/EN via `LanguageService` keys — this rule applies only to code/comments.

### No personal information in code

**Never hardcode personal information in source files.** This includes phone numbers, email addresses, full names used as data (not as display labels), ID numbers, or any other PII.

- Personal data lives in the **database only** (managed via Admin UI at runtime).
- `AppDbContext.OnModelCreating` must NOT contain `HasData()` calls with personal information. It defines schema/indexes only.
- Configuration values (API keys, secrets, connection strings) must use environment variables / Fly.io secrets — never hardcoded values, not even in `.env.example` (use placeholder values like `your-value-here`).
- If you spot hardcoded PII anywhere in the codebase, remove it and record the change.

### Git history hygiene

If PII is accidentally committed, use `git filter-repo --replace-text` to rewrite history across all branches, then force-push. Do not leave PII in git history even if it's been removed from the working tree.

```bash
# Create replacements file
echo "secret_value==>REDACTED" > /tmp/replacements.txt

# Rewrite all history
git filter-repo --replace-text /tmp/replacements.txt --force

# Re-add remote (filter-repo removes it as a safety measure)
git remote add origin https://github.com/vodongha/vodongha-personal.git

# Force push all branches and tags
git push --force origin master develop
git push --force origin --tags
```

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

Migrations apply automatically on startup. **There is no `HasData()` seed in `AppDbContext`** — all content (skills, projects, experience, education, blog posts, settings) is managed at runtime through the Admin UI and stored in the database.

## Fly.io secrets

| Secret | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL |
| `Admin__Username` | Admin panel login |
| `Admin__Password` | Admin panel login |
| `Email__ResendApiKey` | Resend API key for contact form |

Set with: `flyctl secrets set KEY=VALUE`

## Skeleton loading

All pages show animated shimmer placeholders while Blazor circuit connects and data loads. Pattern:

**Admin pages** — `private bool _loading = true;` in `.razor.cs`, set to `false` after `LoadAsync()`. Razor uses `@if (_loading) { <skeleton> } else { <real content> }`.

**Public sections** (SkillsSection, ProjectsSection, ExperienceSection, EducationSection, BlogSection) — data fields initialized as `null`, skeleton shown while null:
```razor
@if (_items is null) { <timeline-skel> }
else { <real content> }
```

**Special cases:**
- `HeroSection` — settings dictionary is always non-null, uses `private bool _loaded = false;` flag instead
- `BlogPostPage` — `_loading = true` distinguishes loading from not-found; `_post is null` after `_loading = false` means 404
- Admin pages with empty state (Contacts, Chats) use 3-branch chain: `@if (_loading) { skel } else if (count == 0) { empty } else { table }`

**SCSS:**
- Public skeletons: `Styles/_skeletons.scss` — `@keyframes skel-pub-shimmer` + `%pub-skel` extend placeholder; classes: `.hero-skel`, `.skills-skel`, `.projects-skel`, `.timeline-skel`, `.blog-skel`, `.blog-post-skel`
- Admin skeletons: in `Styles/_admin-styles.scss` — `.admin-tbl-skel`, `.admin-chat-skel`, `.apikeys-skel`, `.cv-skel`

## Current version

**v2.0.5**

| Version | Changes |
|---|---|
| v2.0.5 | Self-hosted analytics dashboard (page views, geo country, daily chart, top pages/countries/referrers); Admin sidebar collapsible groups (Portfolio/Communication/Insights/System); sidebar independent scroll; i18n for analytics; mobile bottom bar equal-width + dividers; Website button + Menu (mobile-only); SCSS refactor (_admin-mobile.scss, _client-mobile.scss); AI floating widget (Google Gemini); scroll-to-top position fix; collapsible sidebar (icon-only collapsed mode, localStorage); icon-only top controls with tooltips; mobile bottom bar 4-item; Dashboard → /admin/analytics |
| v2.0.4 | Security hardening (SignalR admin auth, rate limiting, constant-time login, server-side push IsAdmin); WCAG AA contrast fixes; loading bar scoping; accessibility; code quality; DI fix; git workflow updated |
| v2.0.3 | Web Push notifications, searchable dial-code picker, chat light/dark mode, admin chat UX fixes, API Keys admin, blog pagination, skeleton loading, theme system fixes |
| v2.0.2 | CV PDF (QuestPDF + SkiaSharp, 3 templates), AdminCv page |
| v2.0.1 | Chat widget (SignalR + Telegram), AdminChats |
| v2.0.0 | Initial launch |
