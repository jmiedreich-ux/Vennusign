# Vennu — Product Development Roadmap
### Version 5 · Confidential

**every venue · every menu**

---

## Key Changes in This Version

- Super Admin CRM moved to Phase 04 — built before the customer CMS
- Features spread across phases — no single phase overloaded
- Each item adds new information — no repeated content across groups

| | |
|---|---|
| **16** Total Phases | **3** Setup Phases |
| **7** Build Phases | **6** Plan Phases |

---

## Contents

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
| 10 | Upgrade Prompts & Billing UX | PLAN | Mos 5–7 |
| 11 | POS Integration | PLAN | Mos 6–9 |
| 12 | Multilingual Support | PLAN | Mos 8–10 |
| 13 | Staff Mobile App | PLAN | Mos 10–12 |
| 14 | TV Apps & Platform Distribution | PLAN | Mos 12–16 |
| 15 | AI Features | PLAN | Mos 15–19 |
| 16 | Analytics & Smart Features | PLAN | Mos 18–23 |

---

## Phase 01 — Foundation & Launch Setup
**SETUP · Weeks 1–3 · Milestone: Ready to start writing code**

Everything needed before Phase 02 begins. Azure at dev scale costs under $20/mo. These steps are one-time — do them once and move on. The trademark filing is the only item with genuine time pressure: file it before any public launch.

### Azure Infrastructure

| Service | Tier | Cost | Purpose |
|---|---|---|---|
| App Service | B1 | $13/mo | API hosting · scales to B2 at first customer |
| Azure SQL | Basic | $5/mo | 2GB · plenty for hundreds of venues |
| Static Web Apps | Free | $0/mo | CDN-backed hosting for all React SPAs |
| Blob Storage | LRS | ~$1/mo | Photos, display assets, HTML templates |
| SignalR Service | Free tier | $0/mo | Up to 20 concurrent screen connections — upgrade at scale |
| App Insights | Pay as you go | ~$0/mo | Telemetry from day one · free under 5GB/mo |

### Business Setup

- **Stripe account** — 2.9% + 30¢ per transaction · create product and price objects for all 4 tiers before Phase 03
- **Business bank account** — Mercury or Relay · free · keeps personal and business finances separated from day one
- **Trademark** — USPTO Class 042 — File immediately · $250–350 + attorney · protects the name before any public-facing launch
- **Domain registration** — vennu.app or chosen alternative · register the moment the name is confirmed · cannot be delayed

### Dev Tooling

- **Visual Studio 2022 Community** — Free · primary IDE · build, debug, breakpoints, NuGet · .NET 8 templates included
- **Vite + React 18 + TypeScript** — Same setup for all three SPAs — admin CMS, super admin, display app
- **GitHub + Actions CI/CD** — Free private repos · auto-deploy to Azure on push to main · set up before writing any code
- **Postman** — Free · API testing and webhook simulation · create a Vennu collection from Phase 02
- **Claude Code CLI** — `npm install -g @anthropic-ai/claude-code` · Node 18+ required · works in VS integrated terminal

> **CLAUDE.md** — Create `CLAUDE.md` in the project root before the first Claude Code session. Include project description, current phase, build commands, naming conventions (screen IDs: `sc-{6 chars}`, feature keys: `snake_case`), and SignalR group format (`screen:{screenId}`, `venue:{venueId}`). This file is read automatically every session.

---

## Phase 02 — Core Backend & Real-Time Engine
**BUILD · Weeks 3–7 · Milestone: Screens update in real time**

The foundation every other phase depends on. Get the API, SignalR hub, and display player boot sequence working end-to-end before building any UI. A save in the admin should appear on a test TV within 200ms before Phase 03 begins.

### Data Models

| Entity | Key Fields | Notes |
|---|---|---|
| Venue | Id, Name, Timezone, Type, PrimaryLanguage | Top-level tenant · Type: Restaurant/Bar/QSR/Café/Brewery/FoodHall |
| Screen | Id, VenueId, ScreenKey, Name, WallGroup, Position, LastSeen, Status, Platform | ScreenKey format: `sc-{6 random chars}` |
| ScreenPairingCode | Code, VenueId, ScreenId, ExpiresAt, IsClaimed | 6-digit code · 10-min expiry · polling endpoint checks IsClaimed |
| Menu / MenuSection | Id, VenueId, Name, IsActive, DisplayOrder | Sections ordered by DisplayOrder · IsActive controls visibility |
| MenuItem | Id, SectionId, Name, Desc, Price, HHPrice, Available, Qty, Tags, ImageUrl | HHPrice used when isHappyHour active · Qty drives 'Only N left!' |
| MenuItemTranslation | ItemId, LanguageCode, Name, Desc | Multilingual baked in from day one · no schema change needed in Phase 12 |
| TapItem | Id, VenueId, Name, Style, ABV, IBU, Desc, Price, GlassColor, NameColor, Available | Separate entity from MenuItem · beer-specific fields |

### DbUp Migration Setup

Install `dbup-sqlserver` NuGet package. Create `Vennu.Data/Scripts/` folder. Set all `.sql` files as Embedded Resource. `DatabaseMigrator.Run()` is called in `Program.cs` before `builder.Build()` — API does not start if any migration fails.

- **Script naming** — `001_create_venues.sql` · `002_create_screens.sql` · etc. — never modify existing scripts
- **GO handling** — DbUp splits on GO batch separator automatically
- **Transaction per script** — `.WithTransactionPerScript()` — a failed script rolls back cleanly

### SignalR Hub — VennuHub.cs

| Method / Event | Direction | Purpose |
|---|---|---|
| JoinScreen(screenId) | Client → Hub | TV joins its named group on connect |
| JoinVideoWall(wallId, pos) | Client → Hub | Wall screens join shared group · pos used for CSS translateX offset |
| ContentUpdated(payload) | Hub → Client | Full board payload pushed on any admin save · ~200ms |
| ThemeUpdated(theme) | Hub → Client | Colors and fonts only · lighter than full ContentUpdated |
| ItemAvailabilityChanged(itemId, available) | Hub → Client | Single item patch · triggered by POS webhook · board grays item in <500ms |
| SyncTick(serverTimeMs) | Hub → Client | 16ms broadcast for video wall frame sync · ~60fps software sync |
| withAutomaticReconnect() | Client config | Reconnects silently after network drops · no manual refresh needed |

### REST API — Core Endpoints

| Method + Route | Purpose |
|---|---|
| GET /api/display/{screenId}/content | Full board payload for a screen · called on player boot |
| POST /api/display/{screenId}/heartbeat | Screen health ping every 30s · updates LastSeen and Status |
| POST /api/screens | Register new screen · returns screenKey (sc-a3f9bc) |
| POST /api/screens/pairing-code | TV calls this · generates 6-digit code with 10-min expiry |
| GET /api/screens/pairing/{code}/status | TV polls every 3s · returns {linked, screenId} when claimed |
| POST /api/screens/pairing/{code}/claim | Admin claims code · links screen to venue · TV redirects |
| POST /api/media/upload | Upload image → Azure Blob → returns CDN URL |
| GET /api/venues/{id}/features | Returns computed feature set (tier + overrides) · used by admin frontend |

### Display Player — Boot Sequence

The player is a React SPA at `vennu.app/display/{screenId}`. It has four responsibilities — all logic lives server-side.

1. **Fetch content** — GET `/api/display/{screenId}/content` · sets initial board state
2. **Connect SignalR** — `JoinScreen(screenId)` · subscribes to all hub events
3. **Start heartbeat** — POST `/heartbeat` every 30s · marks screen Online in admin
4. **Register Service Worker** — Caches last payload · board survives internet drops during service

### Background Services

- **ScheduleEvaluator** — IHostedService · runs every 60s · compares current time to schedules · pushes ContentUpdated if change needed
- **HeartbeatMonitor** — Marks screens Offline if no ping received in 90s · triggers push notification to admin
- **StripeWebhookReceiver** — Handles subscription.updated · invoice.paid · customer.subscription.deleted · all async

---

## Phase 03 — Tier System & Feature Flags
**BUILD · Weeks 6–9 · Milestone: Monetisation infrastructure live**

Built before the customer CMS so that `HasFeatureAsync()` already exists when the first admin panel is written. Every feature check in Phase 05+ calls this service — retrofitting it later would touch every controller.

### Data Models

| Entity | Key Fields | Notes |
|---|---|---|
| Feature | Id, Key, Label, Category, IsActive | Key is snake_case e.g. `happy_hour` · IsActive is master kill switch |
| SubscriptionTier | Id, Name, Slug, Price, MaxScreens, IsPublic, IsActive, StripeProductId | Dynamic · no hardcoding · MaxScreens: -1 = unlimited |
| TierFeature | TierId, FeatureId, LimitValue | LimitValue stores metered limits e.g. '20' for AI descriptions per month |
| VenueSubscription | VenueId, TierId, StripeSubscriptionId, Status, TrialEndsAt, CurrentPeriodEnd | Status: active · trialing · past_due · canceled |
| VenueFeatureOverride | VenueId, FeatureId, Enabled, Reason, ExpiresAt, CreatedByAdminId | Enabled=true unlocks · Enabled=false blocks · ExpiresAt=null = permanent |

### Feature Resolution Service

`HasFeatureAsync(venueId, featureKey)` is the single function every controller calls. It checks VenueFeatureOverride first — overrides always win — then falls back to the tier's TierFeature entries. Result is cached per-request.

- **Override → Tier fallback chain** — Venue override (if not expired) → Tier default → false
- **GetFeatureSetAsync(venueId)** — Returns the full computed feature dictionary · used by admin frontend to know what to show or lock
- **Caching strategy** — IMemoryCache with 60s sliding expiry · invalidated on tier change or override update

### Initial Tier Definitions

| Tier | Price | Screens | Key Features |
|---|---|---|---|
| Starter | $39/mo | 2 | photo_grid · classic_diner · basic_scheduling · allergen_badges · analytics |
| Restaurant Starter | $49/mo | 1 | Above + meal_periods · bilingual_display · ai_translation (1 lang) · quick_update |
| Pro | $89/mo | 6 | All Starter features + all layouts · happy_hour · pos_integration · staff_app |
| Business | $179/mo | ∞ | All Pro features + ai_custom_builder · multi_location · white_label · html_editor |

Annual billing (2 months free) and 14-day free trial with no credit card required across all tiers. Stripe trial status grants full Pro features until the trial ends — then restricts automatically via webhook.

### Stripe Wiring

- **Products + Prices** — One Stripe product per tier · monthly and annual price objects · IDs stored in `SubscriptionTier.StripeProductId`
- **subscription.created** — Sets `VenueSubscription.Status = trialing or active` · grants feature access
- **invoice.paid** — Confirms payment · extends CurrentPeriodEnd
- **customer.subscription.updated** — Handles plan change (upgrade/downgrade) · updates TierId on VenueSubscription
- **customer.subscription.deleted** — Sets Status = canceled · feature access restricted immediately
- **Webhook idempotency** — All handlers check for duplicate events by Stripe event ID before processing

---

## Phase 04 — Super Admin CRM
**BUILD · Weeks 8–11 · Milestone: Internal operations tooling live**

Built immediately after the tier system so it can be used during development of the customer CMS. Serves as the internal interface for managing test venues, toggling feature flags, and validating the tier system works correctly — before any real customer signs up.

### Dashboard

- **MRR + ARR** — Live from Stripe API · month-over-month trend · split by tier
- **Venue count** — Active · trialing · cancelled in last 30 days · displayed separately
- **Screen health map** — Every screen across every venue as a coloured dot · green=online · red=offline · hover shows venue + screen name + last seen
- **Recent events feed** — Upgrades · new signups · overrides applied · churn · in reverse chronological order

### Tier Manager

- Create / edit / clone / archive tiers — Full CRUD · no code deployment needed · changes are live immediately
- Feature toggle per tier — Checkbox grid per tier · Enable All / Clear All shortcuts · save applies to all venues on that tier
- Stripe product ID field — Links tier to Stripe product · validated on save · kept in sync
- Public / private flag — Private tiers invisible on pricing page · used for custom enterprise deals
- MaxScreens field — -1 = unlimited · stored as int · resolver handles -1 as no limit

### Feature Matrix

- All features × all active tiers as a grid — One cell = one checkbox · click to toggle · unsaved changes highlighted in blue
- Category grouping — Display · Scheduling · Language · Mobile · POS · AI · Analytics · Enterprise
- Save all changes at once — Single save button applies all pending toggles · logged in audit table with admin ID and timestamp

### Venue CRM

- Search and filter — Filter by name · tier · status (active, trialing, cancelled) · screen health
- Venue table — Name · type · tier · MRR · screen count · last active · override count · health indicator
- Tier switcher — Change any venue's tier in one click · Stripe subscription updated via API automatically
- Override panel — Add feature unlock or block · reason field (required) · optional expiry date · effective immediately
- Effective features view — Shows each feature, its source (tier or override), and override reason if applicable
- Support context — See exactly what a customer has access to before responding to any support request

> **Development use.** During Phase 05–09 development, use the Super Admin to create test venues, assign them to different tiers, and validate that `HasFeatureAsync()` correctly locks and unlocks features. This catches tier logic bugs before any real customer encounters them.

---

## Phase 05 — Admin CMS — Core Editing
**BUILD · Weeks 10–14 · Milestone: First venue can manage their board**

The daily interface for restaurant and bar owners. Every panel calls `HasFeatureAsync()` before rendering — locked features show a tier badge and a soft prompt, never an error. Built mobile-responsive from day one because Quick Update mode is a key retention driver for small venues.

### Menu Editor

- **Section expand/collapse** — Manage large menus without scrolling · focus one section at a time · state persisted in localStorage
- **Inline item editing** — Edit name, description, price, HH price in-place · no modal · Save & Sync pushes via SignalR
- **Availability toggle** — 86 items — One click marks item unavailable · Live/Off pill shown · auto-resets at midnight via ScheduleEvaluator
- **Sold-out / limited qty badge** — 'Only 3 left!' shown on display · auto-removed when Qty reaches zero · POS integration updates this in Phase 11
- **Allergen and dietary badges** — GF · Vegetarian · Vegan · Halal · Kosher · Nuts · Spice level · stored as comma-separated tags on MenuItem
- **HH price field** — Always visible in editor · disabled with tier badge if venue is below Pro · shows greyed value not hidden

### Quick Update Mode

A stripped-back mobile-first view for the solo operator running service. Available on Restaurant Starter and above.

- **Daily special field** — Single text input · push button · live on all screens in under 10 seconds
- **86 toggle list** — Every menu item as a large tap-friendly toggle · no section navigation · one scroll
- **Auto-midnight reset** — ScheduleEvaluator restores all 86'd items at midnight · no morning action required

### Screen Management

- Registration URL generation — Admin creates a screen record · gets `vennu.app/display/sc-a3f9bc`
- Health dashboard — Online/Offline per screen · last-seen timestamp · click to push content manually
- Multi-screen overflow visualiser — Mini strip showing item distribution across screens · density selector (2×2 · 3×2 · 4×2 · 3×3)
- Video wall builder — Assign screens to a wall group by position · 2×1 · 3×1 · 2×2 configs · Pro tier only

### Tier-Aware UI Patterns

| Pattern | Implementation |
|---|---|
| Tier badge | Small coloured pill (e.g. PRO) shown beside any locked field or nav item — informational only |
| Locked nav item | Visible at 50% opacity · tier badge beside label · clicking opens upgrade modal not an error |
| Locked section preview | Blurred glimpse of the feature · one benefit sentence · soft 'Unlock with Pro' CTA |
| Disabled form field | Field visible but greyed · tooltip explains tier requirement · not hidden |
| Inline feature hint | Amber-accented card at bottom of relevant panel · one per panel · dismissible |
| Sidebar nudge | Rotates locked features at bottom of sidebar · 7-second intervals · per-feature dismiss |
| Upgrade modal (bottom sheet) | All tier features as pills · single CTA · current tier shown · 'Maybe later' exit |

---

## Phase 06 — Display Layouts — Restaurants & Cafes
**BUILD · Weeks 13–17 · Milestone: Can sell to restaurants and cafes**

The first two display layouts cover the widest market. Photo Grid targets food-photo-heavy venues. Classic Diner targets text-only operators. The basic theme builder ships here — full theme builder comes in Phase 07.

### Layout Architecture

The player selects a React component based on `board.layout`. Adding a new layout in any phase = one new component + one map entry. The boot sequence never changes.

### Photo Grid Layout

- **Grid density** — 2×2 (4 items) · 3×2 (6) · 4×2 (8) · 3×3 (9) · admin selects per venue
- **Item cards** — Azure Blob CDN image with gradient placeholder while loading · name, desc, price overlay
- **Bestseller ribbon** — ★ POPULAR badge top-left · driven by admin toggle · not automatic
- **Sold-out overlay** — Card dims with SOLD OUT text · restores at midnight or on POS event in Phase 11
- **Happy hour pricing** — HH price shown in amber · regular price struck through · only visible when `isHappyHour=true` in payload
- **Multi-screen overflow** — `start = (screenPosition-1) × itemsPerScreen` · screen self-selects its slice · no server-side per-screen logic

### Classic Diner Layout

- **Background** — Warm cream `#faf8f4` · inverts the dark theme · most legible at distance for text-heavy boards
- **Multi-column text grid** — 2 or 3 columns · sections side-by-side · familiar laminated menu aesthetic
- **Category headers** — Bold Playfair Display section titles · 1px rule below each · generous vertical spacing
- **Daily special banner** — Full-width strip at bottom · 'Soup of the Day: Tomato Bisque · $5' · driven by Quick Update field
- **Price formatting** — Right-aligned with dot leaders · clean and readable at any screen size

### Theme Builder — Basic (All Tiers)

- **Background colour** — 6 quick swatches + full colour picker · warm tones recommended for Classic Diner
- **Accent colour** — Applied to prices, section headers, highlights across the board
- **Venue name font** — 3 options — DM Sans · Playfair Display · Syne
- **Live preview pane** — Shows exact TV output · updates on every interaction · Push to All Screens via SignalR

> **Theme note.** Full theme builder with glow intensity slider, 6 font options per element, 5 neon presets, and per-section colour control ships in Phase 07 alongside the Neon Chalkboard.

---

## Phase 07 — Display Layouts — Bars
**BUILD · Weeks 16–20 · Milestone: Can sell to bars and upscale restaurants**

The Neon Chalkboard is the visual centrepiece of the platform — the display that makes customers stop and look. It is the primary reason a bar chooses Vennu over a generic signage tool. The full theme builder ships here.

### Neon Chalkboard Layout

- **Chalk board texture** — SVG fractal noise overlay on near-black · 5% opacity · authentic chalk surface
- **Multi-layer text shadow** — 8-layer CSS stack: white core → coloured mid-glow → wide ambient · each layer tuned separately
- **Neon flicker** — CSS keyframes on venue title · opacity dips at irregular ms intervals · not a simple loop
- **Glow breathe** — Slow brightness pulse on all neon elements · 3s ease-in-out infinite · feels alive not static
- **Chalk draw-in animation** — Items clip-path from left to right on load · like someone writing on the board
- **Scanline overlay** — repeating-linear-gradient every 4px · 4% opacity · subtle TV screen texture
- **Neon frame** — Box-shadow border with glow · corner bracket SVG accents · frames the board like a real sign

### Theme Builder — Full (Pro Tier)

- **5 built-in presets** — Bar Classic · Violet Lounge · Hot Summer · Ocean Dive · Rose Gold
- **Title colour + glow** — Separate pickers · glow should be a darker shade of the title colour for depth
- **Section colours** — One neon ink colour per menu column · independent pickers
- **Glow intensity slider** — 0.2× subtle chalk marker → 2.0× full electric overload · affects all shadow layers proportionally
- **Venue name font** — 6 options — Pacifico · Lobster · Righteous · Fredoka One · Bungee · Permanent Marker
- **Menu items font** — 4 options — Caveat · Kalam · Patrick Hand · Permanent Marker · shown in actual item text in preview
- **Board background** — Colour picker + 6 quick swatches for common dark board tones

### Additional Layouts (Pro Tier)

- **Split Layout** — Hero photos left half · full text menu right half · adjustable 40/60 or 50/50 split ratio · casual mid-range restaurants
- **Daily Special Hero** — One full-screen featured item · rotating secondary strip below · 8-second dwell · 'Today Only' amber badge

### Noto Font Preloading

Preload Noto Sans SC, KR, JP, and Arabic in the display app regardless of current venue settings — prevents flash of unstyled text when a bilingual venue's board first loads on a TV browser.

---

## Phase 08 — Scheduling Engine
**BUILD · Weeks 18–22 · Milestone: Menus run themselves — zero daily effort**

Eliminates the most common daily pain point: manually switching the board. The IHostedService evaluator runs every 60 seconds and pushes a ContentUpdated event whenever a schedule change is due. Zero staff action needed after setup.

### Meal Period Auto-Switch

- **Period definitions** — Breakfast · Lunch · Afternoon · Dinner · Late Night · each with start and end time as TimeOnly
- **Day-of-week control** — Each period active on configurable days · e.g. Breakfast only Mon–Fri
- **Layout per period** — Each period can switch to a different layout, menu filter, or theme
- **Timezone evaluation** — ScheduleEvaluator converts UTC to venue's local timezone before comparing · stored on Venue entity
- **No manual trigger ever** — Once configured, no staff action needed between periods for the lifetime of the subscription

### Happy Hour Scheduler (Pro tier)

- **Time window** — Start time + end time + active days · e.g. 4pm–7pm Mon–Fri
- **Per-item HH pricing** — HHPrice field on MenuItem populated in admin · shown automatically during active window
- **isHappyHour in payload** — Boolean added to content payload · display layouts use it to switch price display
- **Manual override** — Admin can force-activate or force-deactivate regardless of schedule · useful for events
- **HH banner on board** — Pulsing 'HAPPY HOUR · 4PM–7PM · MON–FRI' shown during active window · dismissible per venue
- **Countdown timer widget** — 'Happy Hour ends in 47 min' · timer counts down live on the board · drives last-minute orders

### Playlist Rotation

- **Multiple slides per screen** — Screen rotates through slides · configurable dwell time per slide in seconds
- **Slide types** — Menu board · Daily special · Event promo · Image-only · Custom HTML (Phase 15)
- **Schedule per slide** — Each slide can have its own active time window · only shows during configured period
- **Drag-to-reorder** — Admin reorders slides by dragging · live preview updates rotation order instantly

### Emergency Broadcast

- **One button, all screens** — 'Cash only tonight' · 'Kitchen closing in 30 mins' · SignalR push to all venue screens instantly
- **Full-screen override** — Broadcast replaces current layout entirely · not a banner overlay
- **Auto-expire** — Set a duration in minutes · board returns to normal automatically when expired
- **Scope control** — Push to all screens or target a specific screen · useful for multi-room venues

---

## Phase 09 — Tap List Boards — Breweries & Bars
**BUILD · Weeks 20–24 · Milestone: Can sell to breweries and taprooms**

Three distinct tap list styles covering the full range of brewery and bar aesthetics. TapItem is a separate entity from MenuItem — beer-specific fields make the data model honest rather than forcing beers into a generic item structure.

### TapItem Field Usage Across Layouts

| Field | Classic Chalk | Tap Strips | Digital Board |
|---|---|---|---|
| Name | Column list item | Large hand-lettered | Bold header |
| Style | Not shown | Sub-label | Subtitle (e.g. 'West Coast IPA') |
| ABV | Not shown | Small mono text | Shown with % suffix |
| IBU | Not shown | Not shown | Shown beside ABV |
| Description | Not shown | Not shown | 2-line clamp below ABV |
| Price | Category price only | Below style | Top-right of card |
| GlassColor | Not used | Not used | SVG liquid fill colour |
| NameColor | Not used | Neon glow colour | Not used |
| Available | Greys out item | Hides or greys strip | Greys out card |

### Classic Chalkboard Drinks Board

- **Category pricing model** — One price for all cocktails in a section · displayed in a bordered box · matches real bar board conventions
- **Two-column cocktail list** — Items in warm gold Caveat font · two columns · no individual prices per item
- **Beer sub-sections** — Import and Domestic beer as named lists · bullet-dot separators between items
- **Chalk art illustrations** — SVG cocktail glasses and beer bottle drawn in chalk style · centred between sections

### Tap Strips Board

- **3-column grid** — Strips arranged in a grid · each strip is a dark panel with tap number top-right
- **Rotating fonts** — Each tap uses a different font from [Permanent Marker, Bungee, Righteous, Pacifico, Caveat] by index
- **Name glow** — Each tap glows in its NameColor · text-shadow stack matches Neon Chalkboard pattern
- **Draw-in animation** — Strips animate in sequentially left-to-right · top-to-bottom on initial load

### Digital Tap Board

- **Wood texture** — SVG fractal noise over warm dark brown · repeating-linear-gradient grain lines · 18% opacity
- **Beer glass SVG** — Hand-drawn pint glass · liquid colour = GlassColor field · foam drawn as white ellipses at top
- **Two-column card grid** — 2 columns × 3 rows = 6 taps · each card shows full detail · overflow to next page if more than 6
- **'Now Brewing' callout** — Coming-soon taps shown with amber badge · builds anticipation for upcoming kegs

### Pairing Code Registration — Phase 09 Addition

Added here because brewery customers typically have multiple TVs and typing long URLs on TV remotes is the most-cited setup frustration.

- TV loads `vennu.app/pair` — Fullscreen display of 6-digit code + instruction text
- TV polls every 3 seconds — GET `/api/screens/pairing/{code}/status` · returns `{linked, screenId}` when claimed
- Admin enters code — POST `/api/screens/pairing/{code}/claim` · links screen to venue · TV auto-redirects
- Code expires in 10 minutes — Regenerates automatically · prevents stale codes from being claimed by wrong venue

---

## Phase 10 — Upgrade Prompts & Billing UX
**PLAN · Months 5–7 · Milestone: Self-serve upgrade funnel generating revenue**

The in-product upgrade experience. Every prompt is non-invasive — customers see what they're missing without their current workflow being interrupted.

### Core Principles

| Principle | Rule |
|---|---|
| Show benefit, not tier name | 'Items sell out → board updates in seconds' not 'Upgrade to Pro for POS' |
| Never block a workflow | Locked features show a hint · never an error · customer always gets past it |
| One prompt per screen | Never more than one upgrade suggestion visible simultaneously |
| All prompts dismissible | Per-session memory · dismissed hints never reappear in the same session |
| Upgrade in one click | Modal → single CTA → Stripe checkout · no more than two taps total |

### Six Prompt Patterns

- **Tier badge** — Small coloured pill (PRO / RESTAURANT STARTER) beside any locked item · informational only
- **Locked nav item** — 50% opacity in sidebar · tier badge beside label · clicking opens modal not an error
- **Locked section preview** — 0.3px blur on a mockup of the feature · one benefit sentence · soft unlock CTA
- **Inline feature hint** — Amber-accented card at bottom of relevant panel · contextual · one per panel · dismissible
- **Sidebar nudge** — Rotates locked features at bottom of sidebar · 7-second intervals · per-feature dismiss · dots show queue
- **Upgrade modal (bottom sheet)** — Feature benefit · all tier features as pills · current tier shown · single CTA · 'Maybe later' exit

### Stripe Self-Serve Checkout

- **Stripe Billing Portal** — Pre-built hosted portal for plan changes · no custom UI needed for billing management
- **Upgrade flow** — Upgrade modal CTA → Stripe Checkout with pre-selected plan → webhook fires → features unlock immediately
- **Downgrade flow** — Via Billing Portal → effective at end of billing period · features restricted at period end
- **Trial conversion** — 14-day trial ends → Stripe sends invoice → if unpaid within grace period → status = past_due → features restricted
- **HaaS contract billing** — Separate Stripe subscription with 18/24/36 month term · early cancel triggers buyout charge

---

## Phase 11 — POS Integration
**PLAN · Months 6–9 · Milestone: Beats every generic signage competitor**

Real-time POS sync is the single most powerful differentiator. When an item sells out at the register the board updates in under 500ms. No generic signage tool does this reliably.

### Square — Build First

- **OAuth 2.0 connect** — 'Connect Square' button in admin · one-click auth · access token encrypted in Azure SQL
- **Catalog API sync on connect** — Full menu auto-imported from Square on first connect · zero manual data entry for the customer
- **Inventory webhook** — POST `/webhooks/square` · Return HTTP 200 immediately · process in background Task · never times out
- **Price sync** — Item price change in Square POS → SignalR push → board updates within 200ms
- **Square App Marketplace** — Free distribution channel · listing gives access to 4M+ restaurant customers

### Toast — Build Second

- Why second: Dominates full-service restaurant market — the primary Pro tier customer profile
- Webhook registration — Not self-serve · submit production URL to Toast developer contact · requires approval and review
- Hourly polling fallback — GET menu availability every 60 minutes · resilience if a webhook event is missed
- GUID deduplication — Log Stripe event IDs in a processed-events table · idempotent handlers safe to receive duplicates

### Clover — Build Third

- Why third: Fills the mid-market gap between Square (SMB) and Toast (enterprise)
- REST API + OAuth — GET `/v3/merchants/{merchantId}/items` · same OAuth pattern as Square · IPosProvider slots straight in

### Shared Integration Architecture

- **Unified webhook endpoint** — POST `/webhooks/{provider}` · one controller receives all three · routes to IPosProvider implementation
- **IPosProvider interface** — Abstraction means a fourth POS (Lightspeed, EPOS Now etc.) adds one class and nothing else changes
- **ItemAvailabilityChanged push** — SignalR pushes to all venue screens · player patches single item in board state · no full re-render
- **Apideck evaluation** — $300–500/mo unified wrapper · evaluate at 50+ venues requesting less common POS systems

---

## Phase 12 — Multilingual Support
**PLAN · Months 8–10 · Milestone: Ethnic restaurant market unlocked**

No generic signage competitor does multilingual well. The MenuItemTranslation table was created in Phase 02 — this phase builds the editing UI and display rendering on top of existing schema.

### Admin UI Translation

- **react-i18next** — Auto-detects browser language · language stored on User entity · instant switch via header dropdown
- **Launch languages** — Spanish · Simplified Chinese · Vietnamese — highest-ROI non-English restaurant owner markets in the US
- **Phase 2 languages** — Korean · Portuguese — add after validating demand from launch three
- **Zero hardcoded English** — All UI strings externalised from Phase 05 · this phase just provides the translation JSON files

### AI Bulk Translation — Claude API

- **One-click translate** — 'Translate to Chinese' button · Claude translates all items in approximately 15 seconds
- **Cost** — ~$0.30 to translate a 50-item menu into 3 languages · essentially free at any volume
- **Context-aware** — Claude understands restaurant terminology · 'Kung Pao' → correct translation not a literal word-for-word output
- **Review table** — All translations shown with IsAutoTranslated flag · owner edits inline · marks as reviewed

### Bilingual Display Modes

- **Stack** — Primary language large · secondary smaller below · cleanest option for most boards
- **Side-by-side** — Two full columns one per language · equal prominence · good for EN + ZH parity
- **Subtitle** — Primary large · translation as small italic subtitle · minimal extra vertical space
- **Font pairing** — English in Caveat/Kalam · CJK/Arabic in matching-weight Noto · Noto was preloaded in Phase 07

### RTL Support

- Arabic and Hebrew — `dir=rtl` applied to RTL language content · layout mirrors via CSS logical properties
- Noto Sans Arabic — Already preloaded in display app from Phase 07 · no additional font load time

---

## Phase 13 — Staff Mobile App
**PLAN · Months 10–12 · Milestone: Pro tier becomes sticky**

The single most-requested feature post-launch. A bar manager who can't find the laptop at 6pm on a Friday will cancel. The one who can update the board from their iPhone will refer Vennu to every venue they know. This app is the primary retention driver for the Pro tier — included as a listed feature, not an add-on.

### React Native — iOS + Android

- **One codebase** — React Native · submitted to App Store and Google Play · included in Pro and above at no extra charge
- **Shared API** — Same .NET endpoints as web admin · no new backend required · biometric auth (Face ID / Touch ID)
- **Push notifications** — Firebase Cloud Messaging · screen offline · keg blow · daily special reminder

### Core Actions

- **Quick 86 toggle** — Mark item unavailable in 2 taps · SignalR push · board updates in under 200ms · most-used feature post-launch
- **Daily special push** — Text entry → live on all screens in under 10 seconds
- **Happy hour override** — Activate or deactivate instantly regardless of schedule
- **Emergency broadcast** — 'Cash only tonight' from the phone · all screens update immediately

### Brewery-Specific

- **Keg blow notification** — Staff marks keg empty → board removes tap + manager push notification
- **Tap list quick-edit** — Change ABV, price, or description directly from the phone without opening a laptop
- **'Now Pouring' update** — Mark a new keg live · board updates the tap strip immediately via SignalR

---

## Phase 14 — TV Apps & Platform Distribution
**PLAN · Months 12–16 · Milestone: App store presence on all major TV platforms**

Thin native wrappers around the existing React display SPA. The player code is identical across every platform — only the packaging changes. Pairing code flow handles setup with zero URL typing on any platform.

### Android TV + Amazon Fire TV

- Why first: Fire TV Stick is the most common hospitality hardware · one APK covers both platforms
- **Kotlin WebView wrapper** — Thin native shell · `loadUrl('vennu.app/display/{screenId}')` · zero changes to player code
- **Pairing code on first launch** — App shows 6-digit code automatically · admin claims it · screen linked in 30 seconds
- **Boot receiver** — Android BOOT_COMPLETED intent · app starts automatically when TV powers on
- **Kiosk mode** — Device owner mode · prevents exit without PIN · Fire TV settings locked
- **Google Play for TV** — Vennu searchable in TV app store · customers install it like Netflix
- **Amazon Appstore** — Same APK · separate submission · covers all Fire Stick variants including 4K

### Samsung Tizen

- Why important: Samsung dominates commercial display market · app store listing removes all setup friction
- **Tizen web app package** — React SPA + config.xml manifest · submitted to Samsung Smart Signage Platform (SSSP)
- **Zero player changes** — Same display app · Tizen packages and hosts it · pairing code works identically

### LG webOS

- **IPK package** — Same React SPA · different build target · submitted to LG Smart Signage Solution (SSS)
- **webOS simulator** — Free LG developer tool · test on Mac or Windows without physical hardware

### HaaS Pre-Registration

- Screens arrive pre-configured — Create screen records before shipping · venue's content loaded at the factory
- Plug in and it works — Customer powers on TV · correct menu already showing · no setup required
- Card in the box — 'Your screens are already set up. Plug in, power on, done.'
- Platform field on Screen entity — Records 'chrome' · 'android_tv' · 'fire_tv' · 'tizen' · 'webos' · visible in Super Admin

---

## Phase 15 — AI Features
**PLAN · Months 15–19 · Milestone: Premium tier upsell driver**

Intelligence that lowers the skill floor. A restaurant owner who has never written a menu description gets professional copy in seconds. All AI features use the Claude API via `/v1/messages` at ~$0.01–0.03 per call. AI translation was introduced in Phase 12 — this phase adds the remaining AI features.

### AI Menu Content

- **Description writer** — 'grilled salmon, lemon butter' → 'Pan-seared Atlantic salmon with house-made lemon beurre blanc'
- **Bulk generation** — Generate descriptions for every item lacking one · one click · review table before publishing
- **Smart naming** — Highlights generic names like 'Chicken Dish' · suggests specific alternatives · owner accepts or edits
- **Allergen detection** — Claude reads description · suggests relevant allergen badges · owner confirms before applying

### AI Custom Display Builder (Business Tier)

- **Plain English input** — 'Dark blue, gold text, logo top centre, beer list left, food right, countdown bottom corner'
- **Claude generates layout** — Full JSX/CSS generated · saved to Azure Blob as a reusable template
- **Template variables** — `{{venueName}}` · `{{items}}` · `{{isHappyHour}}` · `{{currentTime}}` · `{{sections}}` all injectable
- **iframe sandbox** — `sandbox='allow-scripts'` · security boundary · SignalR still pushes live content updates
- **Cost** — ~$0.02 per generation · Business tier only · usage tracked in Super Admin per venue

### HTML/CSS Sandbox Editor (Developer Tier)

- **Monaco Editor** — VS Code engine in the browser · syntax highlighting · autocomplete · not shown to standard owners
- **Variable reference panel** — Sidebar shows all `{{variables}}` with live examples using real venue data
- **Real-time preview** — Renders with live menu data while editing · no save needed to see result

### Smart Features

- **Happy hour suggester** — Analyses POS order volume by hour across past 30 days · recommends optimal windows · one-click apply
- **Auto-layout optimizer** — Analyses margin data from POS · suggests which items should be in position 1 on the grid
- **Photo background generator** — Upload plain food photo → AI generates styled background matching venue brand colours

---

## Phase 16 — Analytics & Smart Features
**PLAN · Months 18–23 · Milestone: Data-driven venues — clear ROI proof**

Analytics needs real usage data and POS history to be meaningful — it belongs at the end of the roadmap. By Phase 16 you have months of heartbeat pings, impression logs, and POS correlation data to make the dashboard genuinely useful rather than showing empty charts.

### Analytics Dashboard

- **Screen uptime** — Heartbeat pings logged · uptime % per screen per week · downtime periods shown on timeline
- **Content impressions** — Which items displayed · when · for how long · total impressions per item per day
- **POS sales correlation** — Display position vs. actual POS sales · identifies which board positions drive the most orders
- **Item performance scoring** — 'Your #2 revenue item sits at screen 3, position 8 — consider moving it to screen 1, position 1'
- **Happy hour ROI** — Order volume before, during, and after happy hour · quantified revenue impact per venue
- **Multi-location rollup** — Business tier only · aggregate reporting across all locations · compare venue performance

### A/B Layout Testing

- Test two layouts — Layout A Mon/Wed · Layout B Tue/Thu · metric is avg order value from POS
- Item position testing — Test item A in position 1 vs position 4 · measure POS sales impact over 2 weeks
- Automatic winner — System identifies winner after configurable period · prompts admin to make it permanent

### Dynamic & Smart Features

- **Dynamic pricing** — Rules engine: 'If weekday AND hour < 17:00 THEN use lunch price' · Business tier only
- **Foot traffic mode** — Google Maps Popular Times API → simplified board at peak rush · reduces cognitive load for customers
- **Weather-triggered promotions** — OpenWeather API · temperature > 85°F → cold drink special auto-activates
- **Social proof ticker** — Google and Yelp review rotation at point of decision · review fetched via Places API
- **Upsell prompts** — 'Pairs well with our Hazy IPA' shown beneath featured items · configured per item in admin
- **Zapier integration** — Outbound webhooks + Zapier · connects to Google Sheets price sync · Slack alerts · any webhook target

---

## Revenue Model

### Software-Only Tiers

| Tier | Price | Screens | Key Features |
|---|---|---|---|
| Starter | $39/mo | 2 | Photo Grid · Classic Diner · basic scheduling · allergen badges |
| Restaurant Starter | $49/mo | 1 | Above + meal periods · bilingual · AI translation (1 lang) · Quick Update |
| Pro | $89/mo | 6 | All layouts · happy hour · POS · staff app · video wall · full analytics |
| Business | $179/mo | Unlimited | All Pro + AI custom builder · multi-location · white-label · HTML editor |

Annual billing offers 2 months free (approx. 17% discount). 14-day free trial with no credit card required on all tiers.

### Hardware-as-a-Service Bundles

| Bundle | Monthly | Term | Includes | Post-Contract |
|---|---|---|---|---|
| Starter Kit | $89/mo | 18-mo | 1 screen + Fire Stick | Auto → $39 Starter |
| Bar Pack | $159/mo | 24-mo | 2 screens + mounting + sticks | Auto → $89 Pro |
| Full House | $249/mo | 36-mo | 4 screens + mounting + setup visit | Auto → $179 Business |

Hardware ownership transfers at contract end. Stripe auto-transitions to software tier. HaaS LTV over 5 years is approximately 8× software-only.

### MRR Milestones — Pro @ $89/mo

| Customers | MRR | Annual | Significance |
|---|---|---|---|
| 4 | $356 | $4,272 | Azure + Stripe break-even |
| 10 | $890 | $10,680 | Ramen profitable |
| 65 | $5,785 | $69,420 | Break-even with $5k/mo founder salary |
| 100 | $8,900 | $106,800 | Healthy single-founder business |
| 200 | $17,800 | $213,600 | First hire justified |
| 500 | $44,500 | $534,000 | Series-A territory |
