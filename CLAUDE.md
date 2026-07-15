# vodongha.id.vn — CLAUDE.md

## Project overview

Personal portfolio website of Võ Đông Hà. Blazor Web App (.NET 10) + PostgreSQL (Neon, Singapore) + SCSS dark theme, **self-hosted with Docker on a home server behind a Cloudflare Tunnel** (DB stays on Neon).

- **Live:** https://vodongha.id.vn
- **Repo:** https://github.com/vodongha/vodongha-personal
- **Admin:** https://vodongha.id.vn/admin/login
- **Project file:** `vodongha-personal.csproj` (RootNamespace: `VodonghaPersonal`, AssemblyName: `vodongha-personal`)

## Technology stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Frontend | Blazor Web App — **Home page: static SSR** (no render mode, no circuit, zero flicker); **Blog pages, NavBar, FooterSection, ChatWidget, AiWidget**: `@rendermode InteractiveWebAssembly` (in `VodonghaPersonal.Client`); **Admin panel**: `@rendermode InteractiveWebAssembly` (in `VodonghaPersonal.Client`) |
| Database | PostgreSQL via Neon (Singapore, cloud — unchanged). Env var: `ConnectionStrings__DefaultConnection` |
| ORM | Entity Framework Core — hybrid `int Id` (internal PK) + `Guid Rid` (external API identifier) on all 14 entities; no raw SQL in application code |
| SCSS | Two entry points compiled by `AspNetCore.SassCompiler` on `dotnet build`: `Styles/app.scss` → `wwwroot/app.css` (public), `Styles/admin.scss` → `wwwroot/admin.css` (admin). Compiled CSS is **gitignored** — never commit `wwwroot/app.css` or `wwwroot/admin.css`. |
| Email | Resend API (`Email__ResendApiKey`). Sender: `no-reply@vodongha.id.vn`, recipient: `REDACTED_EMAIL` |
| Chat | Telegram Bot API + SignalR (`Microsoft.AspNetCore.SignalR.Client`). Secrets: `Telegram__BotToken`, `Telegram__ChatId`, `Telegram__WebhookSecret` |
| Real-time | ASP.NET Core SignalR (`ChatHub`) — session groups, admin group, typing events |
| Charts | Chart.js 4.5.1 via CDN — wrapped in `wwwroot/js/healthChart.js` |
| PDF generation | QuestPDF 2026.7.1 Community — `CvPdfService.Generate(cv, template, avatarBytes)` dispatches to 3 template methods; `QuestPDF.Settings.License = LicenseType.Community` set at call site |
| Image processing | SkiaSharp 4.150.1 — `CropSquareTop(byte[])` crops image to square (center-horizontal, top-vertical) before QuestPDF so `FitArea()` fills the circle without letterboxing |
| Phone validation | `libphonenumber-csharp` — validates per country's numbering plan (`IsValidNumberForRegion`) |
| Geo IP | ipinfo.io — called from browser JS (`chatUtils.detectCountry`) for country code detection |
| Deploy | **Self-hosted** (Docker) on a home server, public via **Cloudflare Tunnel**. Merge PR to `master` → `deploy.yml` POSTs to the home-server webhook → `git pull` + `docker compose up -d --build` (~2 min). See *Self-hosting & deployment* below. |
| CI | GitHub Actions — `ci.yml`: `dotnet build` on push to `develop` and PRs to `master`; `test.yml`: `dotnet test VodonghaPersonal.slnx` on push to `develop` and PRs to `master`; `lint.yml`: `dotnet format --verify-no-changes` + ESLint + Stylelint on PRs to `master`; `deploy.yml`: build + test + format + lint must pass, then the `deploy` job POSTs to the home-server webhook (`hooks.vodongha.id.vn`) instead of deploying to Fly |
| Migrations | EF Core, applied automatically on startup via `MigrateAsync()` in `Program.cs` |

## Git workflow

```
feature/* ──┐
bug/*    ──→  develop  →  PR → develop (merged)  →  PR → master  →  home-server auto-deploy
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

# 5. Merge develop → master → home-server auto-deploys
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

# 4. Merge → home-server auto-deploys
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
├── Source/
│   ├── VodonghaPersonal.Server/              # ASP.NET Core host — static SSR shell + REST API
│   │   ├── Api/
│   │   │   ├── Admin*/                       # 15 admin API groups (.RequireAuthorization())
│   │   │   │   ├── AdminSkillsApi.cs         # GET list, POST, PUT /{id}, DELETE /{id}
│   │   │   │   ├── AdminProjectsApi.cs       # + PUT /order
│   │   │   │   ├── AdminBlogApi.cs
│   │   │   │   ├── AdminEducationApi.cs
│   │   │   │   ├── AdminExperienceApi.cs
│   │   │   │   ├── AdminContactsApi.cs       # + PUT /{id}/read, PUT /read-all
│   │   │   │   ├── AdminSettingsApi.cs       # + POST /avatar (IFormFile)
│   │   │   │   ├── AdminDashboardApi.cs
│   │   │   │   ├── AdminAnalyticsApi.cs
│   │   │   │   ├── AdminApiKeysApi.cs
│   │   │   │   ├── AdminHealthApi.cs
│   │   │   │   ├── AdminCostsApi.cs
│   │   │   │   ├── AdminDependenciesApi.cs
│   │   │   │   ├── AdminChatApi.cs
│   │   │   │   └── AdminMenuApi.cs
│   │   │   ├── PublicBlogApi.cs              # GET /api/blog, /{slug}, /{id}/related, POST /{id}/view
│   │   │   ├── PublicAiApi.cs                # POST /api/ai/ask (Gemini, AiAskRequest{History})
│   │   │   ├── PublicAuthApi.cs              # GET /api/auth/state → {isAuthenticated} for WASM
│   │   │   ├── PublicChatApi.cs              # Chat session + message REST endpoints
│   │   │   └── PublicSiteApi.cs              # GET /api/site/settings (injects APP_VERSION from assembly),
│   │   │                                     #   GET /api/site/visitor-count
│   │   ├── Components/
│   │   │   ├── App.razor                     # HTML root, SEO meta, sectionToggle JS, scripts
│   │   │   ├── Layout/MainLayout.razor       # Static SSR shell — renders NavBar/Footer placeholders
│   │   │   ├── Pages/                        # Home (static SSR), Login, Error, NotFound
│   │   │   └── Sections/                     # HeroSection, SkillsSection, ProjectsSection,
│   │   │                                     #   ExperienceSection, EducationSection, BlogSection,
│   │   │                                     #   ContactSection (static SSR, expand/collapse via JS)
│   │   ├── Data/                             # AppDbContext, AppDbContextFactory
│   │   ├── Hubs/                             # ChatHub (SignalR)
│   │   ├── Migrations/
│   │   ├── Services/                         # HealthMonitorService, CostMonitorService,
│   │   │                                     #   AppSecretsService, AnalyticsService, etc.
│   │   ├── Styles/                           # app.scss, admin.scss, partials
│   │   └── wwwroot/js/
│   ├── VodonghaPersonal.Client/              # Blazor WASM — public interactive + admin
│   │   ├── ApiClients/
│   │   │   ├── Public/                       # PublicBlogApiClient, PublicSiteApiClient,
│   │   │   │                                 #   PublicChatApiClient, PublicAiApiClient
│   │   │   └── Admin/                        # 14 HttpClient-based admin API clients
│   │   │       ├── SkillApiClient.cs
│   │   │       ├── ProjectApiClient.cs
│   │   │       ├── BlogApiClient.cs
│   │   │       ├── EducationApiClient.cs
│   │   │       ├── ExperienceApiClient.cs
│   │   │       ├── ContactApiClient.cs
│   │   │       ├── SettingsApiClient.cs
│   │   │       ├── DashboardApiClient.cs
│   │   │       ├── AnalyticsApiClient.cs
│   │   │       ├── ApiKeyApiClient.cs
│   │   │       ├── HealthApiClient.cs
│   │   │       ├── CostApiClient.cs
│   │   │       ├── DependencyApiClient.cs
│   │   │       └── ChatApiClient.cs
│   │   ├── Components/
│   │   │   ├── Layout/                       # NavBar, FooterSection, AdminLayout
│   │   │   │                                 #   (AdminLayout loads /admin.css — no @Assets in WASM)
│   │   │   ├── Pages/
│   │   │   │   ├── Public/                   # BlogListPage, BlogPostPage (InteractiveWebAssembly)
│   │   │   │   └── Admin/                    # All 16 admin pages (InteractiveWebAssembly)
│   │   │   └── Shared/                       # BlogCard, BlogShareButtons, TableOfContents,
│   │   │                                     #   RelatedPosts, ChatWidget, AiWidget,
│   │   │                                     #   TimezoneDetector, ToastContainer, AdminNav,
│   │   │                                     #   AdminBreadcrumb, ConfirmDialog, Pagination, ChatHubParser
│   │   ├── Services/                         # CookieAuthStateProvider, ToastService,
│   │   │                                     #   AdminLocalizationService, LanguageService, TimezoneService
│   │   ├── _Imports.razor
│   │   └── Program.cs                        # WASM entry — registers HttpClient, auth services,
│   │                                         #   CookieAuthStateProvider, all API clients
│   └── VodonghaPersonal.Shared/              # Shared between Server and Client
│       ├── Models/                           # All entity models + CvData record
│       └── DTOs/
│           └── AdminDtos.cs                  # All API request/response records
├── Directory.Build.props                 # Shared: TargetFramework net10.0, Nullable enable, ImplicitUsings enable
├── NuGet.Config                          # Package source: api.nuget.org
└── Test/
    ├── VodonghaPersonal.Client.Tests/    # NUnit + Shouldly — Client (34 tests: ChatHubParser, ToastService, TimezoneService, localization)
    ├── VodonghaPersonal.Server.Tests/    # NUnit + Shouldly — Server (12 tests: DependencyInfo)
    └── VodonghaPersonal.Shared.Tests/    # NUnit + Shouldly — Shared (12 tests: CvData, model defaults)
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

### Stylelint rules — what to use in SCSS

The project uses `stylelint-config-standard-scss`. Two rules are disabled in `.stylelintrc.json` to stay compatible with SCSS variable usage:

- **`color-function-alias-notation`** — disabled. Using `rgba(...)` is fine (both plain literals and SCSS variable form `rgba($color, 0.1)` are accepted).
- **`declaration-property-value-keyword-no-deprecated`** — disabled. However, prefer `overflow-wrap: break-word` over `word-break: break-word` in new code — `word-break: break-word` is non-standard and deprecated in most browsers.

## Blazor — important rules

**Scripts only execute from `App.razor`** — not from `.razor` layout or page components. All `<script>` tags must be placed in `App.razor` (before `</body>`). Currently: `<script src="js/admin.js"></script>`.

**Blazor circuit reconnect config** — `App.razor` uses `autostart="false"` on `blazor.web.js` and calls `Blazor.start({ circuit: { reconnectionOptions: { maxRetries: 10, retryIntervalMilliseconds: (n) => n < 3 ? 1000 : n < 6 ? 3000 : 6000 } } })`. This handles a cold-start reconnect after idle (~3-5s) without showing a reconnect error to the user.

**Static SSR expand/collapse** — Home page sections (Skills, Projects, Experience, Education, Blog) use `window.sectionToggle(btn)` defined in `App.razor`. Overflow items get `class="section__overflow"` + `hidden` attribute. The JS function toggles `hidden`, `aria-expanded`, the chevron icon class, and the show/hide span visibility. No Blazor `@onclick` — static SSR has no Blazor runtime on the page.

**Use event delegation** for admin JS — admin panel uses InteractiveWebAssembly which hydrates asynchronously, so `document.querySelectorAll()` called immediately may return nothing. Attach listeners to `document` instead:

```js
document.addEventListener('mousedown', function (e) {
    var el = e.target.closest('.some-selector');
    if (!el) return;
    // handle event
});
```

**Static asset fingerprinting** — .NET 10 fingerprints CSS/JS URLs at build time. `@Assets["admin.css"]` resolves to `/admin.j51ad4dks9.css`. When CSS changes, the hash changes and browsers are forced to reload the file.

**WASM exception:** `@Assets["admin.css"]` is a Razor feature that only works in Server-rendered components. `AdminLayout.razor` in `VodonghaPersonal.Client` must use a plain `/admin.css` path — never `@Assets[...]`.

## Admin panel

- Login: `/admin/login` → POST `/admin/do-login` → cookie auth (7-day sliding expiration)
- Each admin page uses `@layout AdminLayout` and `@attribute [Authorize]`
- Admin pages in `VodonghaPersonal.Client` communicate with the Server via HTTP API clients (`XxxApiClient`). Server-side API handlers in `Api/AdminXxxApi.cs` access the DB directly via `IDbContextFactory<AppDbContext>`.
- All admin pages follow the `.razor` + `.razor.cs` code-behind pattern

### Server-side table processing

All admin entity tables (Skills, Education, Experience, Blog, Contacts, Projects) fetch **one page at a time** from the DB — search, sort, and pagination happen in SQL, not the browser.

- **Server:** each `Api/AdminXxxApi.cs` exposes `GET /api/admin/{entity}/paged?page=&pageSize=&search=&sortBy=&sortDir=` returning `PagedResult<T>(List<T> Items, int Total)` (DTO in `Shared/DTOs/AdminDtos.cs`). Built with EF Core: `EF.Functions.ILike` for case-insensitive search, a `switch` on `sortBy` for sorting, then `IQueryable<T>.ToPagedResultAsync(page, pageSize)` (`Api/PagingExtensions.cs`). The original non-paged `GET /` list endpoints are kept.
- **Client:** `BaseCrudApiClient<T>.GetPagedAsync(...)` (and `ContactApiClient.GetPagedAsync`) call them.
- **QuickGrid pages** (Skills/Education/Experience/Blog/Contacts) use `ItemsProvider` (not `Items=`) with an explicit `TGridItem` and an `@ref` so `RefreshDataAsync()` reloads after search/save/delete. `AdminGrid.MapSort` / `AdminGrid.PageOf` (`Components/Pages/Admin/AdminGrid.cs`) translate the `GridItemsProviderRequest` into `sortBy/sortDir/page`. The grid is always rendered (hidden via inline style while `_loading`) so its provider can run; the first-load skeleton clears inside the provider.
- **AdminContacts** reads its unread badge from `GET /api/admin/contacts/unread-count` (it no longer holds the full list); the empty-state shows when the page total is 0.

### Auth state in WASM

WASM cannot read HttpOnly cookies. `CookieAuthStateProvider` (registered in `Client/Program.cs`) calls `GET /api/auth/state` on the server to check if the current session cookie is valid, then returns an `AuthenticationState` for `<AuthorizeView>` in NavBar. The `/api/auth/state` endpoint calls `ctx.AuthenticateAsync()` server-side and returns `{ isAuthenticated: bool }`.

### prerender: false on Client shared components

`AdminLayout` (Client assembly) is rendered SSR by the Server as the outer shell — even though admin pages themselves have `InteractiveWebAssembly(prerender: false)`. This means any component inside `AdminLayout` that injects a Client-only service (`VodonghaPersonal.Client.Services.*`) will throw a DI resolution error during SSR.

**Rule:** all Client shared components used in `AdminLayout` must declare `@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))` at the top of their `.razor` file:

- `TimezoneDetector.razor` — injects `Client.Services.TimezoneService`
- `ToastContainer.razor` — injects `Client.Services.ToastService`
- `AdminNav.razor` — injects `ChatApiClient`, `ContactApiClient`, `SettingsApiClient`

Without this, Server tries to instantiate the component during SSR and fails to resolve Client-only services. The `prerender: false` directive makes Blazor emit a `<blazor-component>` placeholder instead of rendering the component server-side.

### Blazor code-behind pattern

Every admin page uses a `.razor.cs` partial class:

```csharp
public partial class AdminSkills : ComponentBase
{
    [Inject] private SkillApiClient ApiClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
}
```

The `.razor` file contains only markup — no `@code { }` block, no `@inject` directives.

### AdminProjects — manual pagination + drag-to-reorder

AdminProjects uses a hand-rolled `<table>` (not QuickGrid) because it needs HTML5 drag-and-drop for ordering. It is server-side paged like the others (`GetPagedAsync` → `_pageItems`), so drag-reorder operates **within the current page** (cross-page drag isn't possible in a table UI anyway). On drop, the page's existing `Order` values are permuted among the reordered rows and persisted via `PUT /api/admin/projects/reorder` (rid→order pairs), leaving the global ordering of other pages untouched. Drag indices are local to the page.

### AdminSkills / AdminBlog / AdminEducation / AdminExperience — QuickGrid

These use QuickGrid in **server-side `ItemsProvider` mode** (see *Server-side table processing* above).

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
- Webhook registered at: `https://vodongha.id.vn/api/telegram/webhook`
- Each chat session = one forum topic in the group (title: `Name | Email`)
- `.env` keys: `Telegram__BotToken`, `Telegram__ChatId`, `Telegram__WebhookSecret`

**Note:** the Telegram webhook points at `https://vodongha.id.vn/api/telegram/webhook` (self-hosted via Cloudflare Tunnel).

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

**Overlapping prevention:** `CollectMetrics` is `async void` (required by `TimerCallback`). An `Interlocked` flag (`_running`) prevents the timer from firing a second collection while one is still running — important because the DB ping has a 5-second timeout and the timer fires every 30 seconds.

## IMemoryCache — public services

All 6 public-facing services cache their data in `IMemoryCache` (1-hour TTL) to avoid hitting the DB on every page render. `InvalidateCache()` is called by the corresponding admin API endpoint after every mutation.

| Service | Cache key(s) | Invalidated by |
|---|---|---|
| `SiteSettingService` | `sitesettings_all` | `AdminSettingsApi` POST/PUT |
| `SkillService` | `skills_all` | `AdminSkillsApi` POST/PUT/DELETE |
| `ProjectService` | `projects_all`, `projects_featured` | `AdminProjectsApi` POST/PUT/DELETE/PUT-order |
| `ExperienceService` | `experiences_all` | `AdminExperienceApi` POST/PUT/DELETE |
| `EducationService` | `educations_all` | `AdminEducationApi` POST/PUT/DELETE |
| `BlogService` | `blog_published`, `blog_slugs` | `AdminBlogApi` POST/PUT/DELETE |

**DependencyCheckService** uses `IMemoryCache` (1-hour TTL) plus a `SemaphoreSlim(1,1)` to prevent thundering herd — when multiple requests miss the cache simultaneously, only one fires the NuGet/npm HTTP calls while others wait and get the populated cache result.

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

`GET /api/cv/download?template={0|1|2}` — requires auth (`.RequireAuthorization()`). Loads `SiteSettings` + all Skills/Experiences/Educations/Projects, reads avatar bytes directly from `env.WebRootPath` filesystem (not via HTTP self-request, which is unreliable behind a reverse proxy), calls `CvPdfService.Generate()`, returns `application/pdf`.

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
- No `.Result` or `.Wait()` on Tasks — always `await`
- No `Task.Run(async () => await SomeIoAsync())` — wrapping I/O in `Task.Run` is redundant; call directly and `await`
- No `Task.WhenAll` for sequential logic — use sequential `await` for clarity; `Task.WhenAll` only when genuinely needed (it is NOT needed for I/O-bound work on separate DbContext instances)
- **`Task.Run` is only acceptable for CPU-bound work** (e.g. `await Task.Run(() => cpuPdf.Generate(...))`) — not for I/O
- **Event handler async pattern** — event handlers that call `InvokeAsync(StateHasChanged)` must be `async void` and use `await`:
  ```csharp
  // Correct
  private async void OnLangChanged() => await InvokeAsync(StateHasChanged);
  // Wrong — unawaited
  private void OnLangChanged() => InvokeAsync(StateHasChanged);
  ```
- **JSInvokable async** — `[JSInvokable]` methods that call `InvokeAsync` must return `Task` and use `await`:
  ```csharp
  [JSInvokable]
  public async Task SetActiveHeading(string id) { await InvokeAsync(StateHasChanged); }
  ```
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
- Configuration values (API keys, secrets, connection strings) must use environment variables / the host `.env` — never hardcoded values, not even in `.env.example` (use placeholder values like `your-value-here`).
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

`LanguageService` handles VI/EN toggle for the **public site**. `AdminLocalizationService` handles VI/EN for the **admin panel**. Default language: **English** in both.

### Public site
- UI strings: `Lang.T("key")`
- Content with dual fields: `Lang.IsVi ? item.Description : (item.DescriptionEn ?? item.Description)`
- Components that react to language changes must be `InteractiveWebAssembly` (in Client assembly) and subscribe `Lang.OnChange += StateHasChanged` in `OnInitializedAsync`; static SSR sections (HeroSection etc.) do not react to language toggle

Bilingual content models: `Project`, `Experience`, `Education` (Description/DescriptionEn), `BlogPost` (Title/TitleEn, Summary/SummaryEn, Content/ContentEn), `SiteSetting` (Bio + BioEn as separate keys).

### Admin panel
- UI strings: `Loc.T("key")` (inject `AdminLocalizationService Loc`)
- Vietnamese translations live in the `_vi` dictionary in `AdminLocalizationService.cs`
- Components subscribe: `Loc.OnChanged += OnLangChanged` in `OnInitializedAsync` and unsubscribe in `Dispose`/`DisposeAsync`

### Checklist for every new admin page or component

When adding a new admin page or component, go through **all three** before committing:

1. **i18n** — every user-visible string (titles, labels, placeholders, empty-state messages, button text) must use `Loc.T("key")`. Add the Vietnamese translation in `AdminLocalizationService._vi`. Never leave hardcoded English strings in admin razor files.

2. **Dark mode** — verify all colors work on the default dark theme. Prefer CSS custom properties (`var(--admin-surface)`, `var(--admin-border)`, `var(--color-text)`, `var(--color-text-dim)`, `var(--color-text-muted)`, `var(--admin-bg)`) which adapt automatically. Avoid hardcoded hex colors in new SCSS — use `$color-accent`, `$color-primary`, etc. (SCSS aliases that resolve to CSS vars).

3. **Light mode** — switch to light theme and visually verify: text is readable, surfaces are not invisible, borders are visible, no white-on-white or black-on-black. If a component needs explicit light-mode overrides add them as `[data-theme="light"] .my-class { ... }` in `_admin-styles.scss`. The skeleton shimmer uses `var(--skel-admin-base)` / `var(--skel-admin-shimmer)` which are defined for both themes — always use these instead of hardcoded grey values.

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

Middleware in `Program.cs` fires on GET requests to non-static, non-admin, non-framework paths. Reads real IP from `X-Forwarded-For` (Cloudflare Tunnel). Localhost (`::1`, `127.x`, `10.x`) excluded.

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

**Manually-written migrations** (without `dotnet ef migrations add`) must include a `.Designer.cs` file with `[DbContext(typeof(AppDbContext))]` — without it EF Core silently skips the migration during `MigrateAsync()`.

> **Warning — seed data DELETE+INSERT:** EF Core auto-generates DELETE+INSERT for seeded rows whenever the snapshot changes. Always review generated migration files before committing. If EF emits a DELETE+INSERT for rows that must survive in production, replace with a hand-written `AddColumn` / `AlterColumn` only.

Migrations apply automatically on startup. **There is no `HasData()` seed in `AppDbContext`** — all content (skills, projects, experience, education, blog posts, settings) is managed at runtime through the Admin UI and stored in the database.

## Self-hosting & deployment

The app is **self-hosted with Docker on a home server** (not Fly.io). The database is unchanged — still Neon PostgreSQL in the cloud.

- **Runtime:** `docker compose up -d --build` from the repo dir. `docker-compose.yml` publishes port 8080; `docker-compose.override.yml` disables the compose `curl` healthcheck (the .NET runtime image has no `curl`).
- **Public access:** a **Cloudflare Tunnel** (`cloudflared`) on the server routes `https://vodongha.id.vn` → `localhost:8080`. No inbound ports are opened (the residential ISP blocks 80/443), and TLS is terminated at Cloudflare's edge.
- **Auto-deploy:** on push to `master`, `deploy.yml` runs CI, then the `deploy` job `POST`s to `https://hooks.vodongha.id.vn/deploy/vodongha-personal` with `Authorization: Bearer ${{ secrets.DEPLOY_TOKEN }}`. A small webhook receiver on the server verifies the token, `git reset --hard origin/master`, and re-runs `docker compose up -d --build`.

### Secrets & configuration

Runtime config lives in `.env` on the home server (git-ignored), read by the container via `env_file`. (Previously Fly.io secrets.)

| Key | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon PostgreSQL |
| `Admin__Username` | Admin panel login |
| `Admin__Password` | Admin panel login |
| `Email__ResendApiKey` | Resend API key for contact form |
| `DEPLOY_TOKEN` *(GitHub Actions secret)* | Auth for the home-server deploy webhook |

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

**v3.0.6**

| Version | Changes |
|---|---|
| v3.0.4–v3.0.6 | Admin **server-side tables** (search/sort/paging in SQL) across all admin entity grids; language-toggle WASM init hotfix (sync `LanguageService` from the lang cookie); dependency bumps (QuestPDF 2026.7.0, dotnet/npm groups); Dependencies-page current-version sync; chat FAB aria-label i18n keys. **Self-host build fix:** `docker-compose.yml` passes `APP_VERSION` (from `git describe` in the deploy webhook) as a build arg → the assembly is stamped with the real version so the public footer + admin health/footer show the git-tag version instead of `0.0.0`/`unknown`. |
| v3.0.3 | **Islands architecture + hybrid Rid pattern** — Home page converted to static SSR (no Blazor circuit, zero flickering); NavBar, FooterSection, ChatWidget, AiWidget, BlogListPage, BlogPostPage, BlogCard, BlogShareButtons, TableOfContents, RelatedPosts moved to VodonghaPersonal.Client (InteractiveWebAssembly); public API expanded: GET /api/blog/{rid}/related, GET /api/auth/state; CookieAuthStateProvider added (WASM reads auth state via API); APP_VERSION injected from Server assembly into /api/site/settings (not env var); section expand/collapse via window.sectionToggle() JS (static SSR has no Blazor runtime); CI: actions/checkout@v5, FORCE_JAVASCRIPT_ACTIONS_TO_NODE24=true, git describe fallback to v0.0.0. **Hybrid int Id + Guid Rid pattern**: ALL 14 entities now carry `int Id` (internal PK, never exposed) + `Guid Rid` (external API identifier, placed immediately after Id); entities: BlogPost, Project, Experience, Education, Skill, ChatSession, ChatMessage, ContactMessage, AppSecret, AdminUser, PageView, PushSubscription, SiteSetting, VisitorLog; all REST routes changed from `/{id:int}` to `/{rid:guid}`; migrations `AddRidToAllEntities` + `AddRidToRemainingTables` use `gen_random_uuid()` default so existing rows are populated safely; manually-written migrations require `.Designer.cs` with `[DbContext(typeof(AppDbContext))]` for EF Core discovery; `ChatWidget` stores `chatSessionRid` (Guid) in localStorage |
| v3.0.1 | InteractiveWebAssembly admin panel: new VodonghaPersonal.Client WASM project; 15 REST API endpoint groups under /api/admin/*; all 16 admin pages migrated to @rendermode InteractiveWebAssembly; 14 HttpClient API clients; CvData moved to Shared; solution restructured into Source/Test layout; test split into Server/Client/Shared test projects (58 tests total: 12 Server + 34 Client + 12 Shared); Directory.Build.props and NuGet.Config moved to root; lint.yml workflow cleaned up. **Perf patch:** IMemoryCache (1h TTL) in all 6 public services; DependencyCheckService SemaphoreSlim thundering-herd fix; HealthMonitorService Interlocked overlap prevention; AnalyticsService geo-cache eviction; CvCacheService sequential await; all async event handlers converted to async void + await; PageViews indexes (Path, Country, Referrer); auto app version from git tag in footer + health page |
| v2.0.6 | i18n all 8 admin pages; Lint CI (dotnet format + ESLint + Stylelint); CI & Deploy flow; ES2026 JS (Uint8Array.fromBase64, ecmaVersion 2026); Dependencies tracker page (/admin/dependencies — NuGet/npm/CDN version checks, filter chips, search); unit tests (VodonghaPersonal.Tests, 12 NUnit + Shouldly tests); dep updates (Microsoft 10.0.8→10.0.9, SkiaSharp 3.116.1→3.119.4, eslint 9→10, stylelint 16→17, SortableJS 1.15.3→1.15.7, Devicon latest→2.17.0); menu renamed "Thông tin"→"Hồ sơ" (bi-person-vcard, first in Portfolio group); SCSS Stylelint fixes; analytics nav label fix; cost banner light-mode fix; Chart.js version mismatch fix (4.4.4→4.5.1) |
| v2.0.5 | Self-hosted analytics dashboard (page views, geo country, daily chart, top pages/countries/referrers); Admin sidebar collapsible groups (Portfolio/Communication/Insights/System); sidebar independent scroll; i18n for analytics; mobile bottom bar equal-width + dividers; Website button + Menu (mobile-only); SCSS refactor (_admin-mobile.scss, _client-mobile.scss); AI floating widget (Google Gemini); scroll-to-top position fix; collapsible sidebar (icon-only collapsed mode, localStorage); icon-only top controls with tooltips; mobile bottom bar 4-item; Dashboard → /admin/analytics |
| v2.0.4 | Security hardening (SignalR admin auth, rate limiting, constant-time login, server-side push IsAdmin); WCAG AA contrast fixes; loading bar scoping; accessibility; code quality; DI fix; git workflow updated |
| v2.0.3 | Web Push notifications, searchable dial-code picker, chat light/dark mode, admin chat UX fixes, API Keys admin, blog pagination, skeleton loading, theme system fixes |
| v2.0.2 | CV PDF (QuestPDF + SkiaSharp, 3 templates), AdminCv page |
| v2.0.1 | Chat widget (SignalR + Telegram), AdminChats |
| v2.0.0 | Initial launch |

