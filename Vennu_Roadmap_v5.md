# Vennu — Product Development Roadmap
### Version 5 · Confidential · Full Implementation Detail

**every venue · every menu**

---

## Key Decisions in This Version

- Super Admin CRM moved to Phase 04 — built before the customer CMS
- RepoDb + DbUp replaces EF Core — explicit data access, proven migrations
- Features spread across phases — each phase ships incremental customer value

| | | | |
|---|---|---|---|
| **16** Phases | **3** POS Systems | **5** TV Platforms | **9** Display Layouts |
| **30+** Feature Flags | | | |

**Stack:** React 18 + TypeScript · .NET 8 Web API · ASP.NET SignalR · RepoDb · DbUp · Azure SQL + Blob

**Integrations:** Square · Toast · Clover · Claude AI · Android TV · Fire TV · Samsung Tizen · LG webOS · React Native

---

## Timeline Overview — All 16 Phases

*Early phases shown as weeks · Later phases shown as months · Super Admin at Phase 04 — before the customer CMS*

| Phase | Title | Type | Timeline |
|---|---|---|---|
| 01 | Foundation & Launch Setup | SETUP | Wks 1–3 |
| 02 | Core Backend & Real-Time Engine | BUILD | Wks 3–7 |
| 03 | Tier System & Feature Flags | BUILD | Wks 6–9 |
| 04 | Super Admin CRM | BUILD | Wks 8–11 |
| 05 | Admin CMS — Core Editing | BUILD | Wks 10–14 |
| 06 | Display Layouts — Restaurants & Cafes | BUILD | Wks 13–17 |
| 07 | Display Layouts — Bars | BUILD | Wks 16–20 |
| 08 | Scheduling Engine | BUILD | Wks 18–22 |
| 09 | Tap List Boards — Breweries & Bars | BUILD | Wks 20–24 |
| 10 | Upgrade Prompts & Billing UX | BUILD | Wks 20–28 |
| 11 | POS Integration | PLAN | Mos 5–8 (Wks 24–36) |
| 12 | Multilingual Support | PLAN | Mos 7–9 (Wks 32–40) |
| 13 | Staff Mobile App | PLAN | Mos 9–11 (Wks 40–48) |
| 14 | TV Apps & Platform Distribution | PLAN | Mos 11–15 (Wks 48–64) |
| 15 | AI Features | PLAN | Mos 14–18 (Wks 60–76) |
| 16 | Analytics & Smart Features | PLAN | Mos 17–22 (Wks 72–92) |

---

## Phase 01 — Foundation & Launch Setup
**SETUP · Weeks 1–3 · Milestone: Ready to build**

Everything needed before writing product code. Azure at dev scale costs under $20/mo. These are one-time actions — do them all in week one. The trademark filing is the only item with genuine time pressure: file before any public-facing launch.

### Azure Infrastructure — Dev Tier

| Service | Tier | Cost | Purpose |
|---|---|---|---|
| App Service | B1 | $13/mo | Dev/test tier · scales to B2 when first paying customer signs up |
| Azure SQL | Basic | $5/mo | 2GB · sufficient for hundreds of venues at launch · upgrade to S1 at scale |
| Azure Static Web Apps | Free | $0/mo | CDN-backed hosting for admin CMS, super admin, and display player SPAs |
| Azure Blob Storage | LRS | ~$1/mo | Menu photos, tap photos, custom display assets, AI-generated HTML templates |
| Azure SignalR Service | Free tier | $0/mo | Up to 20 concurrent screen connections · upgrade to Standard at scale |
| App Insights | Pay as you go | ~$0/mo | Telemetry from day one · free under 5GB/mo · essential for debugging SignalR |
| GitHub + Actions CI/CD | — | Free | Auto-deploy to Azure on push to main · separate staging and production slots |

### Business Setup

- **Stripe account** — 2.9% + 30¢ per transaction · create product and price objects for all 4 tiers immediately
- **Business bank account** — Mercury or Relay · free · keeps business finances separated from personal from day one
- **Trademark 'Vennu'** — USPTO Class 042 · File immediately · $250–350 + attorney · do not launch publicly before filing
- **Domain registration** — vennu.app or chosen alternative · register the moment the name is confirmed — cannot be delayed

### Dev Tooling

- **Visual Studio 2022 Community** — Free · full .NET 8 IDE · build, debug, breakpoints, NuGet · primary coding environment
- **Vite + React 18 + TypeScript** — Lightning-fast HMR · same Vite config across admin CMS, super admin, and display player
- **Claude Code CLI** — `npm install -g @anthropic-ai/claude-code` · Node 18+ required · runs in VS integrated terminal
- **Postman** — Free · API testing and webhook simulation · create a Vennu collection from Phase 02 onwards
- **Figma** — $15/mo · UI design · JSX prototypes from planning phase used as production reference

> **CLAUDE.md** — Create `CLAUDE.md` in the project root before the first Claude Code session. Include: project description, current phase, build commands (`dotnet build`, `dotnet run`), naming conventions (screen IDs: `sc-{6 random chars}`, feature keys: `snake_case` e.g. `happy_hour`), and SignalR group format (`screen:{screenId}`, `venue:{venueId}`). Claude Code reads this file automatically at the start of every session.

---

## Phase 02 — Core Backend & Real-Time Engine
**BUILD · Weeks 3–7 · Milestone: Screens update in real time**

The foundation every other phase builds on. Build the .NET 8 API, RepoDb repositories, DbUp migrations, and SignalR hub before any UI. A content save should appear on a test TV within 200ms before Phase 03 begins — validate this end-to-end with two browser tabs.

### Data Models

Core entities only — feature flag and tier entities come in Phase 03. All PKs use `UNIQUEIDENTIFIER` with `NEWID()` default.

| Entity | Key Fields |
|---|---|
| Venue | Id, Name, Timezone (IANA string), Type (Restaurant/Bar/QSR/Café/Brewery/FoodHall), PrimaryLanguage, SecondaryLanguage |
| Screen | Id, VenueId, ScreenKey (sc-a3f9bc), Name, Location, WallGroup, WallPosition, LastSeen, Status, Platform, AppVersion |
| ScreenPairingCode | Code (6-digit string), VenueId, ScreenId (null until claimed), ExpiresAt, IsClaimed · TV polls IsClaimed every 3s |
| Menu / MenuSection | Menu: Id, VenueId, Name, IsActive · MenuSection: Id, MenuId, Name, DisplayOrder, IsActive |
| MenuItem | Id, SectionId, Name, Description, Price, HHPrice, Available, Qty, Tags (CSV), ImageUrl, IsPopular |
| MenuItemTranslation | (ItemId, LanguageCode, Name, Description, IsAutoTranslated) · baked in from day one · no schema change in Phase 12 |
| TapItem | Id, VenueId, Name, Style, ABV, IBU, Description, Price, GlassColor, NameColor, BreweryName, Available, DisplayOrder |

### DbUp Migration Setup

- **Install `dbup-sqlserver` NuGet** — Add to Vennu.Data project · no other dependencies
- **Scripts folder** — `Vennu.Data/Scripts/` · all `.sql` files set as Embedded Resource in project properties
- **Naming convention** — `001_create_venues.sql` · `002_create_screens.sql` · never modify existing scripts, always add new numbered ones
- **`WithTransactionPerScript()`** — Each script runs in its own transaction · failed script rolls back cleanly without affecting already-applied scripts
- **`Program.cs` startup call** — `DatabaseMigrator.Run(connectionString)` before `builder.Build()` · API does not accept traffic if any migration fails

### RepoDb Setup

- **Install `RepoDb.SqlServer` NuGet** — Add to Vennu.Data · also install `RepoDb.SqlServer.BulkOperations` for bulk insert and merge operations
- **`SqlServerBootstrap.Initialize()`** — Called in `Program.cs` · registers all SQL Server type maps and handlers globally
- **Repository pattern** — `IXxxRepository` interface + `XxxRepository` class per entity · registered as scoped via DI
- **Connection per operation** — `using var conn = new SqlConnection(_connectionString)` · connection opened and closed per call
- **Extension method style** — `await conn.QueryAsync(s => s.ScreenKey == key)` · explicit, readable, no change tracking

### REST API — Core Endpoints

| Method · Route | Purpose |
|---|---|
| GET /api/display/{screenId}/content | Full board payload for a screen · called by display player on boot |
| POST /api/display/{screenId}/heartbeat | Screen health ping every 30s · updates LastSeen and Status |
| POST /api/screens | Register new screen · returns screenKey (sc-a3f9bc) |
| POST /api/screens/pairing-code | TV calls this · generates 6-digit code with 10-min expiry |
| GET /api/screens/pairing/{code}/status | TV polls every 3s · returns {linked, screenId} when admin claims |
| POST /api/screens/pairing/{code}/claim | Admin claims code · links screen to venue · TV auto-redirects |
| POST /api/venues | Create venue · assigns default Starter tier |
| GET /api/venues/{id}/features | Returns computed feature set (tier + overrides) for admin frontend |
| POST /api/media/upload | Upload image → Azure Blob CDN → returns CDN URL |

### ASP.NET SignalR Hub — VennuHub.cs

| Method / Event | Direction | Purpose |
|---|---|---|
| JoinScreen(screenId) | Client → Hub | TV joins its named group on connect · all push events target this group |
| JoinVideoWall(wallId, position) | Client → Hub | Wall screens join shared group · position used for CSS translateX offset |
| ContentUpdated(payload) | Hub → Client | Full board payload pushed within ~200ms of any admin save |
| ThemeUpdated(theme) | Hub → Client | Colours and fonts only · lighter than full ContentUpdated |
| ItemAvailabilityChanged(itemId, avail) | Hub → Client | Pushed when POS webhook fires · player patches single item in <500ms |
| SyncTick(serverTimeMs) | Hub → Client | 16ms broadcast for video wall frame sync · ~60fps software sync |
| withAutomaticReconnect() | Client config | Reconnects silently after network drops · no manual page refresh needed |

### Display Player — Boot Sequence

1. **Fetch content** — GET `/api/display/{screenId}/content` · sets initial board state
2. **Connect SignalR** — `JoinScreen(screenId)` · subscribes to ContentUpdated, ThemeUpdated, ItemAvailabilityChanged, SyncTick
3. **Start heartbeat** — POST `/heartbeat` every 30s · marks screen Online in admin dashboard
4. **Register Service Worker** — Caches last successful payload · board keeps showing if internet drops mid-service
5. **Layout switch** — `board.layout` field selects React component · new layouts = new components only · boot sequence never changes
6. **Video wall mode** — If `wallPosition !== null` · full canvas rendered · CSS `translateX(-pos × 1920px)` clips the correct section

### Background Services

- **ScheduleEvaluator** — IHostedService · runs every 60s · evaluates all active schedules · pushes ContentUpdated when a change is due
- **HeartbeatMonitor** — Marks screens Offline if no ping in 90s · immediately pushes dashboard notification to admin
- **StripeWebhookReceiver** — Handles `subscription.updated`, `invoice.paid`, `customer.subscription.deleted` · all processed asynchronously

---

## Phase 03 — Tier System & Feature Flags
**BUILD · Weeks 6–9 · Milestone: Monetisation infrastructure live**

Built before any customer-facing UI so `HasFeatureAsync()` already exists when the first admin panel is written in Phase 05. Every feature check, upgrade prompt, and billing webhook in every later phase calls this service.

### Data Models

| Entity | Key Fields |
|---|---|
| Feature | Id, Key (snake_case e.g. `happy_hour`), Label, Category, IsActive (master kill switch) |
| SubscriptionTier | Id, Name, Slug, Price, MaxScreens (-1 = unlimited), IsPublic, IsActive, StripeProductId |
| TierFeature | TierId, FeatureId, LimitValue (stores metered limits e.g. '20' for AI descriptions per month) |
| VenueSubscription | VenueId, TierId, StripeSubscriptionId, Status (active/trialing/past_due/canceled), TrialEndsAt, CurrentPeriodEnd |
| VenueFeatureOverride | VenueId, FeatureId, Enabled (true=unlock, false=block), Reason (required), ExpiresAt (null=permanent), CreatedByAdminId |

### Feature Resolution Service

- **HasFeatureAsync(venueId, key)** — Checks VenueFeatureOverride first (if not expired) · falls back to tier's TierFeature entries · returns bool
- **GetFeatureSetAsync(venueId)** — Returns full computed feature dictionary · used by admin frontend to decide what to show, dim, or lock
- **Override beats tier always** — Most specific wins · enables beta testing, custom deals, and grandfathering specific customers
- **Metered features** — LimitValue on TierFeature stores numeric cap · `GetUsageAsync(venueId, key)` tracks monthly consumption
- **IMemoryCache** — 60s sliding expiry · invalidated on tier change or override update · prevents a DB hit on every request

### Initial Tier Definitions

| Tier | Price | Screens | Key Features |
|---|---|---|---|
| Starter | $39/mo | 2 | photo_grid · classic_diner · basic_scheduling · allergen_badges · analytics |
| Restaurant Starter | $49/mo | 1 | Above + meal_periods · bilingual_display · ai_translation (1 lang) · quick_update |
| Pro | $89/mo | 6 | All Starter features + all layouts · happy_hour · pos_integration · staff_app |
| Business | $179/mo | ∞ | All Pro features + ai_custom_builder · multi_location · white_label · html_editor |

14-day free trial with no credit card required. Annual billing: 2 months free (~17% discount). Target 40–60% of customers on annual to improve cash flow.

### Stripe Integration

- **Products + Prices per tier** — One Stripe product per tier · monthly and annual Price objects · IDs in `SubscriptionTier.StripeProductId`
- **subscription.created** — Sets `VenueSubscription.Status = trialing or active` · grants full feature access per tier
- **invoice.paid** — Confirms payment · extends CurrentPeriodEnd · no action if already active
- **customer.subscription.updated** — Handles plan changes · updates TierId · Stripe handles proration automatically
- **customer.subscription.deleted** — Sets Status = canceled · feature access restricted immediately
- **Webhook idempotency** — All handlers log Stripe event IDs in `ProcessedStripeEvents` table · safe to receive same event twice
- **HaaS contract billing** — Separate Stripe subscription per hardware bundle · early cancel triggers buyout charge calculation

---

## Phase 04 — Super Admin CRM
**BUILD · Weeks 8–11 · Milestone: Internal ops tooling ready**

Built immediately after the tier system so it can be used during development of the customer CMS. Use it daily during Phases 05–10 to create test venues, assign tiers, toggle feature flags, and validate `HasFeatureAsync()` before any real customer encounters tier logic.

### Revenue Dashboard

- **MRR + ARR** — Live from Stripe API · month-over-month trend · breakdown by tier
- **Active venue counts** — Active · trialing (listed separately) · churned in last 30 days
- **Avg revenue per venue** — MRR ÷ active venues · tracked over time as a retention health indicator
- **Recent events feed** — New signups · upgrades · downgrades · overrides applied · churn · reverse chronological
- **Screen health map** — Every screen across every venue as a dot · green=online · red=offline · hover shows venue, screen name, last seen

### Tier Manager

- **Create / edit / clone / archive** — Full CRUD on tiers · changes take effect immediately · no code deployment needed
- **Feature toggle per tier** — Checkbox per feature per tier · Enable All / Clear All shortcuts · save broadcasts to all venues on that tier
- **MaxScreens field** — -1 = unlimited · stored as integer · resolver treats -1 as no limit
- **Stripe product ID field** — Links tier to Stripe product · validated on save · kept in sync on every tier change
- **Public / private flag** — Private tiers invisible on pricing page · used for custom enterprise deals

### Feature Matrix

- **All features × all tiers as a grid** — One cell = one checkbox · click to toggle · unsaved changes highlighted in amber
- **Category grouping** — Display · Scheduling · Language · Mobile · POS · AI · Analytics · Enterprise
- **Save all changes at once** — Single save button applies all pending toggles in one transaction
- **Audit trail** — Every change logged with admin ID, timestamp, previous value, and new value

### Venue CRM

- **Search and filter** — Filter by name · tier · status (active/trialing/canceled) · screen health
- **Venue table** — Name · type · tier · MRR · screen count · last active · override count · health indicator
- **Tier switcher** — Change any venue's tier in one click · Stripe subscription updated automatically
- **Override panel** — Add feature unlock or block · reason field (required) · optional expiry date · effective immediately
- **Effective features view** — Shows each feature, its source (tier default or override), reason if applicable, expiry date
- **Support context** — See exactly what a customer has access to before picking up any support request

> **Development workflow.** During Phases 05–10: create a test venue per tier in Super Admin, toggle features on and off, and verify the admin CMS shows the correct locked/unlocked state for each. Find tier logic bugs in development rather than after a paying customer signs up.

---

## Phase 05 — Admin CMS — Core Editing
**BUILD · Weeks 10–14 · Milestone: First venue can manage their board**

The interface restaurant and bar owners use every day. Every panel calls `HasFeatureAsync()` before rendering. Locked features show a tier badge and soft prompt — never an error message. Built mobile-responsive from day one for Quick Update mode.

### Menu Editor

- **Section expand / collapse** — Manage large menus without scrolling · collapsed state persisted in localStorage
- **Inline item editing** — Edit name, description, price, HH price without leaving the list · Save & Sync pushes via SignalR in ~200ms
- **Availability toggle — 86 items** — One click marks item unavailable · Live/Off pill · SignalR push in 200ms · auto-resets at midnight
- **Sold-out / limited qty badges** — 'Only 3 left!' shown on display · auto-removed when Qty reaches zero
- **Allergen and dietary badges** — GF · V · Vegan · Halal · Kosher · Nuts · Spice · stored as CSV on `MenuItem.Tags`
- **Bestseller tag** — ★ Popular badge on display board · driven by admin toggle on `MenuItem.IsPopular`
- **HH price field visibility** — Always visible · disabled with PRO badge if below Pro tier · not hidden, shows greyed value

### Quick Update Mode

- **Purpose** — Built for the solo restaurant owner running lunch service from a phone at the pass
- **Daily special push** — Single text input · push button · live on all screens in under 10 seconds
- **86 toggle list** — Every menu item as a large tap-friendly toggle · one scroll · no section navigation
- **Auto-midnight reset** — ScheduleEvaluator restores all 86'd items at midnight · no morning action required from any staff
- **Mobile-first layout** — Optimised for phones · large touch targets · no horizontal scrolling

### Screen Management

- **Registration URL generation** — Admin creates screen record · gets `vennu.app/display/sc-a3f9bc` · pairing code added in Phase 09
- **Screen health dashboard** — Online/Offline per screen · last-seen timestamp · click any screen to push content manually
- **Screen naming and location** — 'Main Bar TV', 'Patio Board', 'Entrance Display' · location label shown in super admin screen map
- **Push to specific screen** — Target one screen or broadcast to all venue screens simultaneously
- **Multi-screen overflow visualiser** — Mini strip showing item distribution across screens · density selector (2×2 · 3×2 · 4×2 · 3×3)
- **Video wall builder** — Assign screens to wall group by position · 2×1, 3×1, 2×2 configs · Pro tier only

### Tier-Aware UI Patterns

| Pattern | Behaviour |
|---|---|
| Tier badge | Small coloured pill (e.g. PRO in amber) beside locked nav item, field, or layout card — informational only |
| Locked nav item | 50% opacity in sidebar · tier badge beside label · click opens upgrade modal, not an error |
| Locked section preview | Blurred glimpse of the feature · one concrete benefit sentence · soft 'Unlock with Pro' CTA · not blocking |
| Disabled form field | Field visible but greyed · tooltip explains requirement · not hidden |
| One prompt per screen | Never more than one upgrade suggestion simultaneously · prevents alert fatigue |
| All prompts dismissible | Per-session dismiss memory · dismissed hint never reappears in the same browser session |

---

## Phase 06 — Display Layouts — Restaurants & Cafes
**BUILD · Weeks 13–17 · Milestone: Can sell to restaurants and cafes**

The first two display layouts cover the broadest market. Photo Grid targets food-photo-heavy venues. Classic Diner is the simple text board most small restaurants actually want. Basic theme builder ships here.

### Photo Grid Layout

- **Purpose** — QSR · Chinese · Vietnamese · Mexican · any restaurant using food photography as primary menu communication
- **Grid density options** — 2×2 (4 items) · 3×2 (6 items) · 4×2 (8 items) · 3×3 (9 items) · admin selects per venue
- **Food photo cards** — Azure Blob CDN image with gradient placeholder while loading · name, short description, price overlay
- **Bestseller ribbon** — ★ POPULAR badge top-left of card · driven by `MenuItem.IsPopular` toggle in admin
- **Sold-out overlay** — Card dims to 40% opacity with SOLD OUT text · auto-restores at midnight · instant via SignalR from POS in Phase 11
- **Happy hour pricing** — HH price shown in amber · regular price struck through · only when `isHappyHour=true` in content payload
- **Multi-screen overflow** — `start = (screenPosition-1) × itemsPerScreen` · screen self-selects its slice · no server-side per-screen logic

### Classic Diner Layout

- **Purpose** — Diners · cafes · sandwich shops · venues where text legibility at distance matters more than photos
- **White background** — Warm cream `#faf8f4` · inverts the dark theme · high contrast dark ink typography
- **Multi-column grid** — 2 or 3 columns · sections side by side · familiar laminated menu aesthetic
- **Category headers** — Bold Playfair Display section titles · 1px rule below each · generous vertical spacing
- **Daily special banner** — Full-width strip at bottom · 'Soup of the Day: Tomato Bisque · $5' · driven by Quick Update special field
- **Price alignment** — Right-aligned prices with dot leaders · clean and readable at any screen size

### Theme Builder — Basic (All Tiers)

- **Background colour picker** — 6 quick swatches + full colour picker · warm cream recommended for Classic Diner
- **Accent colour** — Single brand colour applied to prices, section headers, and highlights
- **Venue name font** — 3 options: DM Sans · Playfair Display · Syne · full library in Phase 07
- **Live preview pane** — Shows exact TV output on every change · no save needed to preview
- **Push to all screens** — One button · SignalR broadcasts ThemeUpdated to every connected screen instantly

### Player Evolution — Phase 06

- **Layout switch added** — `board.layout` selects React component · map grows with each phase · boot sequence unchanged
- **Azure Blob CDN image load** — Lazy loaded with gradient placeholder · CDN cache headers set on upload
- **Overflow position logic** — `screenPos` from screen registration · `start = (pos-1) × itemsPerScreen`
- **Service Worker caching** — Full content payload cached · CDN photos cached separately · offline resilient

---

## Phase 07 — Display Layouts — Bars
**BUILD · Weeks 16–20 · Milestone: Can sell to bars and upscale restaurants**

The Neon Chalkboard is the visual centrepiece of the platform — the display that makes customers stop and look. It is the primary reason a bar chooses Vennu over a generic signage tool. The full theme builder with neon controls ships alongside it.

### Neon Chalkboard Layout

- **Chalk board texture** — SVG fractal noise at 5% opacity on near-black · authentic chalk surface
- **Multi-layer neon text-shadow** — 8-layer CSS stack: white `0px 0px 2px` core → coloured mid-glow → wide ambient · true tube-light appearance
- **Neon flicker animation** — CSS keyframes on venue title · opacity dips at irregular ms intervals — not a repeating loop
- **Glow breathe animation** — Slow 3s ease-in-out brightness pulse on all neon elements · feels alive and warm
- **Chalk draw-in animation** — Items clip-path left-to-right on load · as if someone is writing on the board in real time
- **Scanline overlay** — repeating-linear-gradient every 4px at 4% opacity · subtle CRT/TV screen texture across the board
- **Neon frame + corner brackets** — Box-shadow border with matching glow · SVG corner bracket accents · frames the board like a real sign
- **Section neon dividers** — Gradient horizontal rules with glow shadow · each section's divider matches its assigned colour

### Theme Builder — Full (Pro Tier)

- **5 built-in preset themes** — Bar Classic · Violet Lounge · Hot Summer · Ocean Dive · Rose Gold · sets all values at once
- **Title colour + glow pickers** — Separate pickers · glow should be a darker shade of title colour for depth
- **Section colours** — One neon ink colour per menu column · independent pickers · up to 4 section colours
- **Glow intensity slider** — 0.2× subtle chalk marker → 2.0× full electric overload · scales all text-shadow layers proportionally
- **Venue name font — 6 options** — Pacifico · Lobster · Righteous · Fredoka One · Bungee · Permanent Marker
- **Menu items font — 4 options** — Caveat · Kalam · Patrick Hand · Permanent Marker · shown in actual item text in live preview
- **Board background** — Colour picker + 6 quick swatches for common dark board tones
- **Noto font preloading** — Noto Sans SC, KR, JP, Arabic preloaded from this phase · prevents CJK flash when Phase 12 ships

### Split Layout (Pro Tier)

- **Left half — hero photos** — 2–4 bestseller photos · large format · pulled from `MenuItem.IsPopular` items
- **Right half — text menu** — Complete menu in text columns · all items · allergen badges · pricing
- **Adjustable split ratio** — Admin slider: 40/60 or 50/50 · stored on Screen entity · per-screen setting
- **Best for** — Mid-range casual restaurants wanting visual appeal and complete menu information

### Daily Special Hero Layout (Pro Tier)

- **One large featured item** — Full-screen photo + name + description + price · maximum visual impact
- **Rotating secondary strip** — 3–4 smaller items below · auto-rotates every 8 seconds · configurable dwell time
- **'Today Only' badge** — Amber banner on the hero item · creates scarcity · drives impulse orders
- **Best for** — Restaurants pushing a new special every day · replaces paper A-frame signs

---

## Phase 08 — Scheduling Engine
**BUILD · Weeks 18–22 · Milestone: Menus run themselves — zero daily effort**

Eliminates the most common daily pain point: manually switching the menu board. Set it once and it runs indefinitely. The IHostedService evaluator runs every 60s and pushes ContentUpdated via SignalR whenever a schedule change is due.

### Meal Period Auto-Switch

- **Period definitions** — Breakfast · Lunch · Afternoon · Dinner · Late Night · each with TimeOnly start and end · stored per venue
- **Day-of-week control** — Each period active on configurable days · e.g. Breakfast Mon–Fri, Brunch Sat–Sun
- **Layout per period** — Each period can switch to a different layout, apply a menu filter, or change the active theme
- **Venue timezone evaluation** — ScheduleEvaluator converts UTC to venue's IANA timezone before comparing · stored on Venue entity
- **Zero staff action** — Once configured, no operator needs to touch the board between periods — ever

### Happy Hour Scheduler (Pro Tier)

- **Time window config** — Start time + end time + active days · e.g. 4pm–7pm Mon–Fri · stored on HappyHourSchedule entity
- **Per-item HH pricing** — `MenuItem.HHPrice` shown automatically during active window via `isHappyHour=true` in content payload
- **Auto-activation** — ScheduleEvaluator pushes `isHappyHour=true` at start time · false at end · no manual trigger
- **Manual override** — Admin can force-activate or force-deactivate regardless of schedule · useful for events
- **HH banner on display** — Pulsing 'HAPPY HOUR · 4PM–7PM · MON–FRI' banner shown on board during active window
- **Countdown timer widget** — 'Happy Hour ends in 47 min' · live countdown · proven to drive last-minute orders

### Playlist Rotation

- **Multiple slides per screen** — Screen rotates through a defined slide list · configurable dwell time per slide in seconds
- **Slide types** — Menu board · Daily special · Event promo · Image-only · Custom HTML (Phase 15)
- **Schedule per slide** — Each slide has its own optional time window · only appears during its configured period
- **Drag-to-reorder** — Admin reorders slides by dragging · live preview shows rotation order instantly

### Emergency Broadcast

- **One button, all screens** — 'Cash only tonight' · 'Kitchen closing in 30 mins' · SignalR push reaches all screens instantly
- **Full-screen override** — Broadcast replaces the current layout entirely · not a banner overlay
- **Auto-expire** — Set a duration in minutes · board returns to normal layout automatically
- **Scope control** — Push to all venue screens or target a specific screen

### Date-Range Promotions

- **Start + end date** — Seasonal specials with defined calendar dates · auto-activates and auto-expires
- **Christmas menu example** — Activates Dec 1 · reverts Dec 27 · configured once, runs automatically
- **Event-specific layouts** — Valentine's Day menu · Super Bowl specials · St Patrick's Day board · any date-bounded display change

---

## Phase 09 — Tap List Boards — Breweries & Bars
**BUILD · Weeks 20–24 · Milestone: Can sell to breweries and taprooms**

Three distinct tap list styles covering the full range of brewery and bar aesthetics. TapItem is a separate entity from MenuItem — beer-specific fields make the data model honest. Each layout uses the TapItem fields differently.

### TapItem Field Usage Across Layouts

| Field | Classic Chalk Board | Tap Strips Board | Digital Tap Board |
|---|---|---|---|
| Name | Column list item | Large hand-lettered | Bold card header |
| Style | Not shown | Small sub-label | Subtitle e.g. 'West Coast IPA' |
| ABV | Not shown | Small mono text | Shown with % suffix |
| IBU | Not shown | Not shown | Shown beside ABV |
| Description | Not shown | Not shown | 2-line clamp tasting notes |
| Price | Category price only | Below style label | Top-right of card |
| GlassColor | Not used | Not used | SVG pint glass liquid fill colour |
| NameColor | Not used | Neon glow colour | Not used |
| Available | Greys list item | Hides or dims strip | Greys card to 40% |

### Classic Chalkboard Drinks Board

- **Purpose** — Cocktail bars and bars with a fixed beer list · category pricing model rather than per-item prices
- **Drinks title** — Large Pacifico header with blue neon glow · authentic chalk aesthetic
- **Category pricing** — All cocktails $10.95 · Import Beer $4.00 · Domestic $3.00 · prices in bordered boxes per category
- **Two-column cocktail list** — Items in warm gold Caveat font · two columns · no individual prices shown
- **Beer sub-sections** — Import and Domestic beers as named lists with bullet-dot separators
- **Chalk art illustrations** — SVG cocktail glasses and beer bottle in chalk style · centred between sections

### Tap Strips Board

- **Purpose** — Taprooms and craft breweries · each keg gets its own strip · authentically brewery-style
- **3-column grid** — Each strip is a dark panel · tap number top-right · strips arranged in grid
- **Hand-lettered name** — Each tap uses a rotating font from [Permanent Marker, Bungee, Righteous, Pacifico, Caveat] by index
- **Name glow colour** — Each tap glows in its NameColor · neon text-shadow stack from Phase 07
- **Style + ABV** — Caveat font · smaller and muted · supports the name without competing
- **Draw-in animation** — Strips animate in sequentially left-to-right, top-to-bottom on load

### Digital Tap Board

- **Purpose** — Modern taprooms wanting a polished digital look · direct Taplist.io competitor
- **Wood texture** — SVG fractal noise over warm dark brown + repeating-linear-gradient grain lines at 18% opacity
- **Beer glass SVG** — Hand-drawn pint glass · liquid colour = GlassColor · foam drawn as white ellipses
- **Two-column card grid** — 2 columns × 3 rows = 6 taps · each card shows name, style, ABV, IBU, description, price
- **'Now Brewing' callout** — Coming-soon taps shown with amber badge · builds anticipation

### Pairing Code Registration — Phase 09 Addition

- **Why here** — Brewery customers have multiple TVs · typing URLs on TV remotes is the most-cited setup frustration
- **TV loads `vennu.app/pair`** — Fullscreen display of 6-digit code + plain-language instructions · no URL to type
- **TV polls every 3 seconds** — GET `/api/screens/pairing/{code}/status` · returns `{linked: true, screenId}` when admin claims
- **Admin enters code** — POST `/api/screens/pairing/{code}/claim` · links screen · TV auto-redirects to its display URL
- **Code expiry** — 10-minute expiry · regenerates automatically · prevents stale codes from wrong venue claims

---

## Phase 10 — Upgrade Prompts & Billing UX
**BUILD · Weeks 20–28 · Milestone: Self-serve upgrade funnel generating revenue**

The in-product experience that turns trials into paying customers and paying customers into higher tiers. Every prompt is non-invasive — customers see what they're missing without their current workflow being interrupted or blocked.

### Core Principles

| Principle | Rule |
|---|---|
| Show benefit, not tier name | 'Items sell out → board updates in seconds' — NOT 'Upgrade to Pro for POS integration' |
| Never block a workflow | Locked features are visible and informative · never an error or dead end |
| One prompt per screen | Never show more than one upgrade suggestion simultaneously · prevents alert fatigue |
| All prompts dismissible | Per-session dismiss memory · dismissed hints never reappear in the same session |
| Upgrade in one click | Prompt → upgrade modal → single CTA → Stripe Checkout · maximum two taps total |

### Tier Badge

- Small coloured pill e.g. 'PRO' in amber beside any locked feature · informational, never alarming
- Used on nav items · section headers · individual form fields · layout cards
- Colour matches tier: Starter=slate · Restaurant Starter=green · Pro=amber · Business=purple

### Locked Nav Items

- Visible at 50% opacity — feature exists in sidebar · customers know it's there · not hidden from view
- Tier badge beside label — clear at a glance which tier unlocks this feature
- Clicking opens modal — not an error · upgrade modal opens with this specific feature as context

### Locked Section Preview

- **Blurred glimpse** — 0.3px blur on a mockup screenshot of the feature · tantalising, not frustrating
- **One benefit sentence** — Concrete and specific: 'Prices switch automatically at 4pm — no staff needed'
- **Soft CTA** — Muted button · 'Unlock with Pro · $89/mo' · not aggressive · easily ignored
- **No blocking behaviour** — Customer continues using all unlocked features normally while preview is visible

### Inline Feature Hint

- **Contextual placement** — Appears at bottom of the most relevant panel · e.g. POS hint shown on menu editor tab
- **Blue left accent bar** — Styled like an informational tip, not an advertisement
- **One per panel** — Only the single most relevant locked feature shown per panel · never a list
- **Dismiss button** — × closes for the session · hint never nags

### Sidebar Nudge

- **Bottom of sidebar** — Quiet and ambient · never in the way of main navigation
- **Rotates locked features** — 7-second intervals · dots indicate how many features are queued
- **Per-feature dismiss** — Dismissing one rotates to the next · dismissed features never shown again
- **One sentence + one link** — 'Your team can update the board from anywhere · See how →'

### Upgrade Modal — Bottom Sheet

- **Slides up from bottom** — Native-feeling · not a pop-up · clicking backdrop dismisses
- **Feature name + benefit** — What this specific feature does · concrete, tied to the feature clicked
- **All tier features as pills** — Every feature at that tier shown as a pill · makes total value tangible
- **Current tier shown** — 'You're on Restaurant Starter' · context for the upgrade decision
- **Single CTA** — 'Upgrade to Pro — $89/mo' · one button · goes directly to Stripe Checkout
- **'Maybe later' text link** — Easy non-guilt exit · customer remembers the feature when they're ready

### Stripe Self-Serve Billing

- **Stripe Billing Portal** — Pre-built hosted portal for plan changes and payment management · no custom UI needed
- **Upgrade flow** — Upgrade modal CTA → Stripe Checkout with pre-selected plan → webhook fires → features unlock immediately
- **Trial conversion** — Trial ends → Stripe invoice → grace period → if unpaid → past_due → features restricted
- **HaaS contract billing** — Separate Stripe subscription per bundle · early cancel triggers buyout charge based on remaining term

---

## Phase 11 — POS Integration
**PLAN · Months 5–8 (Weeks 24–36) · Milestone: Beats every generic signage competitor**

Real-time POS sync is the single most powerful differentiator. When an item sells out at the register the board updates in under 500ms. Square is built first — best developer API, free sandbox, 4M+ restaurant customers, .NET SDK available.

### Square — Build First

- **Why first** — Best developer API · free sandbox · 4M+ restaurant customers · .NET SDK · Square Marketplace listing
- **OAuth 2.0 connect flow** — 'Connect Square' button in admin · one-click auth · access token encrypted in Azure SQL
- **Catalog API sync on connect** — Full menu auto-imported from Square on first connect · zero manual data entry for the customer
- **Inventory webhook receiver** — POST `/webhooks/square` · return HTTP 200 immediately · process in background `Task.Run()` · never times out
- **Price sync** — Price change in Square POS → webhook → ContentUpdated or ItemAvailabilityChanged via SignalR → board updates <200ms
- **Square App Marketplace** — Free listing · gives access to 4M+ restaurant customers searching for display tools

### Toast — Build Second

- **Why second** — Dominates full-service restaurant market — the primary Pro tier customer profile
- **Toast developer sandbox** — dev.toasttab.com · free sandbox for development and testing
- **Webhook registration** — Not self-serve · submit production endpoint URL to Toast developer contact · requires approval
- **Hourly polling fallback** — GET menu availability every 60 minutes as resilience · Toast recommends alongside webhooks
- **GUID deduplication** — Log Stripe event IDs in ProcessedWebhookEvents table · idempotent handlers safe for duplicate events

### Clover — Build Third

- **Why third** — Fills mid-market gap between Square (SMB) and Toast (enterprise)
- **Clover REST API + OAuth** — GET `/v3/merchants/{merchantId}/items` · same OAuth flow pattern as Square
- **Inventory update webhook** — Subscribe to inventory change events via Clover Developer Dashboard
- **Same IPosProvider pattern** — Third provider adds one class · no changes to Square or Toast code

### Integration Architecture

- **Unified webhook endpoint** — POST `/webhooks/{provider}` · one controller receives all three providers · routes to correct IPosProvider
- **IPosProvider interface** — Clean .NET abstraction · fourth POS adds one class and nothing else changes
- **ItemAvailabilityChanged push** — SignalR pushes to all venue screens within 500ms of POS event · player patches single item · no full re-render
- **Apideck evaluation** — $300–500/mo unified wrapper · evaluate when 50+ venues request less common POS systems simultaneously

---

## Phase 12 — Multilingual Support
**PLAN · Months 7–9 (Weeks 32–40) · Milestone: Ethnic restaurant market unlocked**

No signage competitor does multilingual well. MenuItemTranslation was created in Phase 02 and Noto fonts were preloaded in Phase 07 — this phase builds the editing UI and bilingual rendering on top of that foundation. No new migrations or font loading needed.

### Admin UI Translation

- **react-i18next** — Auto-detects browser language · language preference stored on User entity
- **Language switcher in header** — Flag emoji + language name · instant switch without page reload · persists across sessions
- **Launch languages — 3** — Spanish · Simplified Chinese · Vietnamese · highest-ROI non-English restaurant owner markets in the US
- **Phase 2 languages** — Korean · Portuguese · add after validating demand from the initial three
- **All UI strings externalised** — Zero hardcoded English text from Phase 05 · this phase only provides the translation JSON files

### AI Bulk Translation — Claude API

- **One-click translate menu** — 'Translate to Chinese' button · Claude API call · all items translated in approximately 15 seconds
- **Cost** — ~$0.30 to translate a 50-item menu into 3 languages · essentially zero cost at any customer volume
- **Context-aware** — Claude understands restaurant terminology · 'Kung Pao' → correct translation, not a literal word-for-word output
- **Review table** — All translations shown side-by-side · owner edits any inline · marks as reviewed
- **IsAutoTranslated flag** — DB flag distinguishes AI-generated from owner-entered text · used for quality tracking

### Bilingual Display Modes

- **Stack mode** — Primary language full size · secondary smaller below each item · cleanest option for most boards
- **Side-by-side mode** — Two full columns one per language · equal visual weight · good for equal-prominence bilingual venues
- **Subtitle mode** — Primary large · translation as small italic subtitle · minimal extra vertical space
- **Per-venue setting** — Admin chooses mode · stored on Venue entity · applied across all layouts
- **Font pairing** — English in Caveat/Kalam · CJK/Arabic in matching-weight Noto (preloaded since Phase 07)

### RTL Support

- **Arabic and Hebrew** — `dir=rtl` applied to RTL language content · layout mirrors via CSS logical properties
- **Noto Sans Arabic** — Already preloaded from Phase 07 · no additional font loading time in this phase
- **Admin RTL mode** — Admin UI can also render in RTL for Arabic and Hebrew-speaking venue owners

### Screen Registration — Phase 12 Update

- **Language on pairing screen** — `vennu.app/pair` shown in browser's detected language automatically
- **Venue language inherited** — New screens automatically inherit venue's primary and secondary language settings

---

## Phase 13 — Staff Mobile App
**PLAN · Months 9–11 (Weeks 40–48) · Milestone: Pro tier becomes sticky**

The single most-requested feature post-launch. A bar manager who can't find the laptop at 6pm Friday will cancel. The one who can update from their iPhone will refer Vennu to every venue they know. Included in Pro at no extra charge — it is a retention feature, not an add-on.

### React Native — iOS + Android

- **One codebase** — React Native · submitted to App Store and Google Play · no separate teams needed
- **Shared .NET API** — Same endpoints as web admin · no new backend required · existing auth tokens work on mobile
- **Biometric authentication** — Face ID / Touch ID · fast login for operators who check it mid-service
- **Push notifications** — Firebase Cloud Messaging · screen offline alerts · keg blow · daily special reminders

### Core Actions

- **Quick 86 toggle** — Mark item unavailable in 2 taps from the kitchen · SignalR push · board updates in under 200ms
- **Daily special push** — Text entry + push button · live on all screens in under 10 seconds
- **Happy hour override** — Activate or deactivate instantly regardless of schedule · useful for events or early close
- **Emergency broadcast** — 'Cash only tonight' from the phone · all screens update immediately via SignalR

### Screen Health

- **All screens at a glance** — Online/offline status · last-seen timestamp · manual push button per screen
- **Offline alert notification** — Push notification the moment any screen goes offline during service hours
- **Remote content refresh** — Force any screen to reload its full content payload with one tap

### Brewery-Specific

- **Keg blow notification** — Staff marks keg empty → board removes/greys that tap + push notification to manager
- **Tap list quick-edit** — Change ABV, price, or description from the phone without opening a laptop
- **'Now Pouring' update** — Mark new keg as live · board updates the tap strip immediately via SignalR

---

## Phase 14 — TV Apps & Platform Distribution
**PLAN · Months 11–15 (Weeks 48–64) · Milestone: App store presence on all major TV platforms**

Thin native wrappers around the existing React display SPA. Player code is identical across every platform — only packaging and build target change. The pairing code flow from Phase 09 handles all setup with no URL typing on any platform.

### Android TV + Amazon Fire TV

- **Why first** — Fire TV Stick is the most common hospitality hardware · one APK covers both platforms
- **Kotlin WebView wrapper** — Thin native shell · `webView.loadUrl('vennu.app/display/{screenId}')` · zero player code changes
- **Pairing code on first launch** — App shows 6-digit code automatically · admin claims · TV redirects to display URL in 30s
- **Boot receiver** — Android `BOOT_COMPLETED` intent · Vennu starts automatically when TV powers on
- **Kiosk / pinned mode** — Device owner mode · prevents exit without PIN · always-on display
- **Google Play for TV** — Vennu searchable in TV app store · customers install like Netflix · no sideloading
- **Amazon Appstore submission** — Same APK · separate submission · covers all Fire Stick variants including 4K Max

### Samsung Tizen

- **Why important** — Samsung dominates commercial display market · app store listing removes all setup friction
- **Tizen web app package** — React SPA + config.xml manifest · submitted to Samsung Smart Signage Platform (SSSP)
- **Zero player changes** — Same display app code · Tizen packages and hosts it · pairing code works identically
- **Samsung B2B program** — Enables pre-installation on Samsung screens shipped as part of HaaS bundles

### LG webOS

- **IPK package** — Same React SPA · different build target · submitted to LG Smart Signage Solution (SSS)
- **webOS simulator** — Free LG developer tool for Mac/Windows · test without physical hardware
- **Coverage** — LG is the second-largest commercial display brand globally

### Screen Registration — Phase 14 Update

- **TV app pairing screen** — App shows code fullscreen on first launch · vennu.app/pair instruction text below
- **Auto-redirect on claim** — TV transitions seamlessly from pairing screen to content display when claimed
- **Platform field** — `Screen.Platform` set to 'android_tv' / 'fire_tv' / 'tizen' / 'webos' · visible in Super Admin
- **App version tracking** — Player reports app version in each heartbeat ping · Super Admin flags outdated screens

### HaaS Pre-Registration

- **Screens arrive pre-configured** — Create screen records before shipping · venue content loaded before the box is sealed
- **Customer experience** — Powers on the TV · correct menu already showing · zero setup required
- **Card in the box** — 'Your screens are already set up. Plug in, power on, done.'
- **No pairing code needed** — Screens registered to venue before delivery · shows Online after first heartbeat

---

## Phase 15 — AI Features
**PLAN · Months 14–18 (Weeks 60–76) · Milestone: Premium tier upsell driver**

AI that lowers the skill floor for every venue owner. A restaurant operator who has never written a menu description gets professional copy in seconds. All features use the Claude API at approximately $0.01–0.03 per call. AI bulk translation was introduced in Phase 12 — this phase adds all remaining AI capabilities.

### AI Menu Content — Claude API

- **Menu description writer** — 'grilled salmon, lemon butter' → 'Pan-seared Atlantic salmon with house-made lemon beurre blanc'
- **Bulk description generation** — Generate descriptions for every item missing one · one click · review table before publishing
- **Smart naming suggestions** — Highlights generic names like 'Chicken Dish' · suggests specific alternatives · owner accepts or edits
- **Allergen detection** — Claude reads description · suggests relevant allergen badges · owner confirms each before applying

### AI Custom Display Builder (Business Tier)

- **Plain English input** — 'Dark blue, gold text, logo top centre, beer list left, food right, countdown bottom right'
- **Claude generates layout** — Full JSX/CSS generated · saved to Azure Blob as a named reusable template
- **Template variables** — `{{venueName}}` · `{{items}}` · `{{sections}}` · `{{isHappyHour}}` · `{{currentTime}}` injected at render time
- **iframe sandbox** — `sandbox='allow-scripts'` · security boundary prevents XSS · SignalR still pushes live content updates
- **Saved and reusable** — Generated templates persist · owner regenerates with a new prompt anytime
- **~$0.02 per generation** — Business tier only · usage tracked per venue · metered via `TierFeature.LimitValue`

### HTML/CSS Sandbox Editor (Developer Tier)

- **Monaco Editor** — VS Code engine in the browser · syntax highlighting · autocomplete
- **Variable reference panel** — Sidebar lists all `{{variables}}` with live examples using real venue menu data
- **Real-time preview** — Renders with live menu data while editing · no save needed · immediate feedback
- **Not shown to standard owners** — Developer tier feature for technical operators and white-label agency resellers

### Smart Features

- **Happy hour window suggester** — Analyses POS order volume by hour · recommends optimal slow-period windows · one-click apply
- **Auto-layout position advisor** — Compares display position with POS sales · suggests moving high-margin items to prime positions
- **Photo background generator** — Upload a plain food photo · AI generates a styled background matching venue brand colours

---

## Phase 16 — Analytics & Smart Features
**PLAN · Months 17–22 (Weeks 72–92) · Milestone: Data-driven venues — clear ROI proof**

Analytics needs real usage data and POS history to be meaningful. By Phase 16 there are months of heartbeat logs, impression data, and POS correlation. Empty charts at launch would have zero value and damage customer trust in the feature.

### Analytics Dashboard

- **Screen uptime tracking** — Heartbeat pings logged per screen · uptime % per week · downtime periods on timeline
- **Content impression logging** — Which items displayed · when · for how long · total daily impressions per item
- **POS sales correlation** — Display position vs. actual POS sales · powered by Square/Toast integration data
- **Item performance scoring** — 'Your #2 revenue item sits at screen 3, position 8 — consider moving it to screen 1, position 1'
- **Happy hour ROI report** — Order volume and revenue before, during, and after happy hour · quantified financial impact
- **Multi-location rollup** — Business tier only · aggregate reporting across all branches · compare venue performance

### A/B Layout Testing

- **Test two layouts** — Layout A Mon/Wed · Layout B Tue/Thu · metric is average order value from POS
- **Item position testing** — Test item in position 1 vs position 4 · measure POS sales impact over 2 weeks
- **Automatic winner** — System identifies winner after configurable period · prompts admin to make it permanent

### Dynamic Pricing (Business Tier)

- **Time-of-day pricing** — Lunch specials at $9.99 auto-activate at 11am · revert at 3pm · ScheduleEvaluator driven
- **Rules engine** — Admin sets conditions: 'If weekday AND hour < 17:00 THEN use lunch_price field' · JSON rules
- **Demand-based pricing** — Slower periods get discounted prices automatically · yield management for sophisticated operators

### Smart Context Features

- **Foot traffic mode** — Google Maps Popular Times API · simplified board during peak rush hours
- **Weather-triggered promotions** — OpenWeather API · temperature > 85°F → cold drink special activates automatically
- **Social proof ticker** — Google and Yelp review rotation · fetched via Places API · shown at point of decision
- **Upsell prompt engine** — 'Pairs well with our Hazy IPA' shown beneath featured items · configured per-item in admin
- **Live sports ticker** — Footer bar for sports bars · team scores via sports API · keeps customers engaged

### Webhook & Automation

- **Outbound webhooks** — Vennu fires events to customer's own systems when content changes · POST to their configured endpoint
- **Zapier integration** — Connect to any web service · Google Sheets price sync · Slack screen-offline alerts
- **Google Sheets sync** — Owner updates a price in Sheets · Zapier → Vennu webhook → board updates automatically

---

## Revenue Model

### Software-Only Tiers

| Tier | Price | Screens | Key Features |
|---|---|---|---|
| Starter | $39/mo | 2 | Photo Grid · Classic Diner · basic scheduling · allergen badges · basic analytics |
| Restaurant Starter | $49/mo | 1 | Above + meal periods · bilingual · AI translation (1 lang) · Quick Update |
| Pro | $89/mo | 6 | All layouts · happy hour · POS · staff app · video wall · full analytics |
| Business | $179/mo | Unlimited | All Pro + AI custom builder · multi-location · white-label · HTML editor |

Annual billing: 2 months free (~17%). 14-day free trial with no credit card on all tiers.

### Hardware-as-a-Service Bundles

| Bundle | Monthly | Term | Includes | Post-Contract |
|---|---|---|---|---|
| Starter Kit | $89/mo | 18-mo | 1 screen + Fire Stick | Auto → $39 Starter |
| Bar Pack | $159/mo | 24-mo | 2 screens + mounting + sticks | Auto → $89 Pro |
| Full House | $249/mo | 36-mo | 4 screens + mounting + setup visit | Auto → $179 Business |

Hardware ownership transfers at contract end. Stripe auto-transitions to software tier. HaaS LTV over 5 years: Full House = ~$13,000 vs ~$1,600 software-only (8× more valuable).

### MRR Milestones — Pro @ $89/mo

| Customers | MRR | Annual | Significance |
|---|---|---|---|
| 4 | $356 | $4,272 | Azure + Stripe fees break-even |
| 10 | $890 | $10,680 | Ramen profitable |
| 65 | $5,785 | $69,420 | Break-even with $5,000/mo founder salary |
| 100 | $8,900 | $106,800 | Healthy single-founder business |
| 200 | $17,800 | $213,600 | First hire justified |
| 500 | $44,500 | $534,000 | Series-A territory |
