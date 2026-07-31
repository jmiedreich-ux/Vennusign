# Vennu Project Status

## Current Phase

**Phase 09 — Tap List Boards — Breweries & Bars: Active**

## Milestone

The internal Super Admin CRM is complete; the next milestone is the first venue managing its board.

## Completed

- Phase 02 — Core Backend and Real-Time Engine
- WP-02.08 through WP-02.14
- Phase 03 — Tier System and Feature Flags
- WP-03.01 — Feature and Tier Core Models
- WP-03.02 — Feature Resolution Engine
- WP-03.03 — Subscription Management
- WP-03.04 — Usage Metering
- WP-03.05 — Stripe Billing Catalog
- WP-03.06 — Stripe Event Idempotency
- WP-03.07 — Stripe Subscription Event Handling
- WP-03.08 — Stripe Webhook Endpoint
- Phase 04 — Super Admin CRM
- WP-04.01 — Super Admin CRM Foundation
- WP-04.02 — Venue Directory
- WP-04.03 — Venue Detail & Support View
- WP-04.04 — Tier Management
- WP-04.05 — Feature Matrix
- WP-04.06 — Venue Feature Overrides
- WP-04.07 — Operational Dashboard
- WP-04.08 — Live Stripe Revenue Snapshot
- WP-04.09 — Recent Commercial Events
- WP-04.10 — Venue Tier Switching
- WP-04.11 — Revenue Trend Snapshots
- WP-04.12 — Phase 04 Validation and Closure
- WP-05.01 — Menu Domain and Persistence Foundation
- WP-05.02 — Menu Editor Read Model and Section Management
- WP-05.03 — Inline Menu Item Editing and Sync
- WP-05.04 — Availability, Quantity, and Menu Badges
- WP-05.05 — Tier-Aware Venue Admin Patterns
- WP-05.06 — Quick Update Mode
- WP-05.07 — Screen Management Core
- WP-05.08 — Screen Targeting and Overflow Visualization
- WP-05.09 — Video Wall Builder
- WP-05.10 — Phase 05 Validation and Closure
- Phase 05 — Admin CMS Core Editing
- WP-06.01 — Display Layout Contract and Registry Foundation
- WP-06.02 — Photo Grid Core Layout
- WP-06.03 — Photo Grid Merchandising States
- WP-06.04 — Photo Grid Density and Multi-Screen Overflow
- WP-06.05 — Classic Diner Core Layout
- WP-06.06 — Classic Diner Pricing and Daily Special
- WP-06.07 — Basic Theme Domain and Persistence
- WP-06.08 — Basic Theme Builder and Live Preview
- WP-06.09 — Player Media Caching and Offline Resilience
- WP-06.10 — Phase 06 Validation and Closure
- Phase 06 — Display Layouts — Restaurants & Cafes
- WP-07.01 — Advanced Theme Domain and Preset Foundation
- WP-07.02 — Full Theme Builder Controls and Preview
- WP-07.03 — Neon Chalkboard Core Layout
- WP-07.04 — Neon Motion, Texture, and Accessibility
- WP-07.05 — Noto Font Preloading
- WP-07.06 — Split Layout Domain and Core Rendering
- WP-07.07 — Split Layout Administration and TV Polish
- WP-07.08 — Daily Special Hero Core
- WP-07.09 — Hero Rotation and Administration
- WP-07.10 — Phase 07 Validation and Closure
- Phase 07 — Display Layouts — Bars
- WP-08.01 — Meal Period Domain and Persistence
- WP-08.02 — Venue Timezone Schedule Resolver
- WP-08.03 — Meal Period Administration
- WP-08.04 — Scheduled Content Activation
- WP-08.05 — Happy Hour Scheduling and Manual Override
- WP-08.06 — Happy Hour Administration and Display
- WP-08.07 — Playlist Domain and Player Rotation
- WP-08.08 — Emergency Broadcast
- WP-08.09 — Date-Range Promotions
- WP-08.10 — Phase 08 Validation and Closure
- Phase 08 — Scheduling Engine
- WP-09.01 — Tap Domain and Persistence
- WP-09.02 — Tap List Administration and Availability
- WP-09.03 — Classic Chalkboard Drinks Core
- WP-09.04 — Classic Chalkboard Administration and TV Polish
- WP-09.05 — Tap Strips Core
- WP-09.06 — Tap Strips Administration and Motion Polish
- WP-09.07 — Digital Tap Board Core
- WP-09.08 — Digital Tap Overflow and Brewing States
- WP-09.09 — Pairing Code Registration Completion

## Active Work Package

None. **WP-09.10 — Phase 09 Validation and Closure** is the next package in roadmap order.

## Phase 04 Result

- Delivered protected venue support, tier management, feature matrix, overrides, operational health, and commercial events.
- Added live Stripe USD revenue, safe venue tier switching, and persisted monthly revenue trends.
- Added repeatable authorization and critical UI journey validation.

## Phase 05 Result

- Delivered venue-scoped menu editing, presentation states, and tier-aware controls.
- Added mobile Quick Update with venue-local midnight availability restoration.
- Added screen registration, health, one/all targeting, deterministic overflow guidance, and supported video walls.
- Added repeatable protected-route, service, repository, worker, migration, and frontend validation.

## Phase 06 Result

- Delivered Photo Grid and Classic Diner layouts through the additive player registry.
- Added multi-screen density/overflow, merchandising, pricing, daily-special, and basic-theme behavior.
- Added versioned offline content/media caching with safe invalidation and online recovery.
- Added repeatable layout, theme, realtime, overflow, and offline critical-journey validation.

## Phase 07 Result

- Delivered advanced Pro themes, Neon Chalkboard, Split Layout, and Daily Special Hero.
- Added accessible motion, multilingual Noto delivery, exact previews, and bounded hero rotation.
- Added repeatable tier, layout, pricing, realtime, and offline critical-journey validation.

## Phase 08 Result

- Delivered venue-local meal periods, happy hour, screen playlists, emergency broadcasts, and date-range promotions.
- Added transition-only evaluation, deterministic precedence, and authoritative realtime player refresh.
- Added repeatable timezone, DST, scheduling, tier, realtime, offline, precedence, and recovery validation.

## Standing Validation Exception

- Integration-type tests are skipped for every AWP under the repository owner's standing instruction.
- Restore, Release build, display production build, unit tests, and applicable non-integration validation remain required.

## Next Action

Claim and implement **WP-09.10 — Phase 09 Validation and Closure**.

## Phase 05 Work Packages

- WP-05.01 through WP-05.10 are defined in `docs/phase-plans/phase-05-admin-cms-core-editing.md`.

## Phase 06 Work Packages

- WP-06.01 through WP-06.10 are defined in `docs/phase-plans/phase-06-display-layouts-restaurants-cafes.md`.

## Phase 07 Work Packages

- WP-07.01 through WP-07.10 are defined in `docs/phase-plans/phase-07-display-layouts-bars.md`.

## Phase 08 Work Packages

- WP-08.01 through WP-08.10 are defined in `docs/phase-plans/phase-08-scheduling-engine.md`.

## Phase 09 Work Packages

- WP-09.01 through WP-09.10 are defined in `docs/phase-plans/phase-09-tap-list-boards.md`.
