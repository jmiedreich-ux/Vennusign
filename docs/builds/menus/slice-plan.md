# Menus Build — Slice Plan

- **Status:** Approved by owner 2026-08-07 (plan accepted; round-2 planning Q&A folded in)
- **Authority:** `docs/design/approved/menus/` (`decisions.md` wins on any conflict) + `build-decisions.md` (17 owner decisions across two Q&A rounds)
- **Model:** functional vertical slices per the approved Track 1 retrospective. Every slice ships schema → API → UI → Playwright specs together; tests are written with the implementation, never after. Each slice is independently mergeable and leaves master releasable. **Every slice ends with a short owner acceptance workbook (5–10 minutes) before the next slice starts** (decision 17); slice 1 gets a demo script instead since it has no screens.

## Scope guardrails

Single venue only. No multi-venue affordance may leak (decision 29). Out of scope entirely: item library UI, fallback-card authoring, price requests, group permissions, POS-priced fields, mobile layouts, redesign of other nav areas' content, scheduling (decisions list + README "Out of scope"). Happy-hour price display is parked until Schedules-owned pricing (decision 15, round 2). Imports: paste + start-blank only; the confirm step is built once so spreadsheet/photo/POS plug in later.

## Slices

### Slice 1 — The spine: item library + draft/publish save model
The schema everything else stands on. No visible UI change yet beyond keeping the current editor compiling.
- Item library data model: `Item` (venue-scoped), `Placement` (item on a section of a menu, ordered), sections restructured to hold placements. Migration **drops** `HappyHourPrice`, `QuantityAvailable`, `Tags`, `IsPopular`, per-item translations, and the `AvailabilityResetUtc` auto-reset concept (decisions 6, 14, 15) — the migration script names what it discards.
- Availability (86): item × venue, boolean + timestamp + who. Commits instantly, never queues, survives publish, and **stays off until a person turns it back on** (decisions 3, 25, 14-r2).
- Draft model: `DraftChange` per menu (field, before, after, author, timestamp), explicit Publish ships the set as a `PublishEvent` with per-target delivery state; history is attributable and **retention is tier-configurable** (decision 3 — a config value in the entitlement/allowance model, numbers TBD).
- API: draft queue read/write, publish, history list, "go back to" (produces a draft), availability toggle.
- Acceptance: demo script (seeded data walked through the API contract). Criteria 1, 2, 3, 4 (README numbering).

### Slice 2 — App shell + board render engine + M1 Menus home + M1b named actions
- **New 76px icon nav rail app-wide** (decision 12): the shell from the hi-fis hosts every area; existing screens keep their content inside it. Decision 19's nav gating (Menus absent below tier) lands here, in the shell.
- **Board render engine v1**: the shared component that renders a menu as a board (Coastal + Classic dark themes, sections, dotted leaders, 86'd items not rendered). M1's cards, M2's canvas, Play, and the TV player all consume this one engine.
- Approved token additions merge into `sky-ui-tokens.css`; components consume variables only. Playfair Display self-hosted.
- M1: shelf with live board-render cards, status headline sentence (decision 12 of 36), pending-changes bar, "Not in use" strip, Add-a-menu tile. Empty state = the onboarding routes (paste / blank for now).
- M1b: card ⋯ menu — **six items** (Open / Quick update / — / Go back to… / Duplicate / **Put away** / — / Take off the screens; decision 16-r2), Take-off dialog showing the generated fallback card, Go back to… as time-phrased history. "Put away" moves a menu to the Not-in-use strip; placing it on a screen brings it back.
- Venue fallback: generated logo-and-name card object (decision 14 of 36) — shown, not authorable.
- Acceptance workbook: shelf, actions, gating. Criteria 5, 6, 8.

### Slice 3 — M2 builder + M2a adding items
- Four-column builder: section rail (navigator only), canvas-as-preview (the render engine, editable) with drag-to-reorder and 86'd rendering, six-control inspector with the loud availability panel, publish bar with per-screen readiness/delivery chips (rides the existing authoritative-vs-applied revision model).
- Undo/redo keystroke, session-scoped (decision 7).
- M2a: one inline add row per section — library search with "where it already lives" + create-new as last option; bulk place drawer.
- Quick update / Build segmented control (routes to slice 6's M3).
- "Viewing as" renders against a default landscape geometry until slice 4 supplies real reports.
- Acceptance workbook: edit → draft → publish end-to-end (TV proof lands next slice). Criteria 7, plus 2/4 re-asserted through the UI.

### Slice 4 — The player: board rendering + geometry + honest delivery
- **Display player renders published boards** with the same engine (decision 13-r2): pages, dwell cycle, instant 86 removal, venue fallback when nothing is published. This is the slice where Publish becomes true on a TV.
- Heartbeat extends to report device geometry (resolution, orientation, physical size where the platform exposes it); `Screen` gains reported-geometry fields + report timestamp. **Device-reported, never user-entered** (decision 13 of 36). Confirmed missing today. Degrades gracefully on platforms that hide panel size.
- Hygiene rider: enforce `screen.content.target` on push/push-all (+ reset/unpair gates) — the gap left open when #663 was archived.
- Acceptance workbook: publish → watch the TV change; 86 → watch it vanish; offline screen catches up. Criteria 1 (on-screen), 4, 9 (partial).

### Slice 5 — M2b board view + M2c Play
- M2b: whole-board zoom, sections as draggable blocks, pages as a consequence of overflow, adapt-first / override-where-broken per screen.
- M2c: Play takes over the canvas — real cycle, real dwell, draft included and labelled, readable-from panel (from reported geometry), proportional transport bar. Read-only.
- Acceptance workbook: arrange a board, watch it play as each screen. Criterion 9 complete.

### Slice 6 — M3 Quick Update + import routes (paste, blank) + shared confirm step
- M3: flat searchable list, one availability toggle per row, toast + Undo, "Off right now" filter with age, header status sentence. Deliberately not a builder view (decision 15 of 36).
- Paste route: live parse ("Looks like 2 sections and 6 items"), caps line = section. Start-blank route.
- Shared confirm step shell (decision 30's convergence point): only-what-we-couldn't-read, grouped near-miss question — built against paste now so spreadsheet/photo/POS reuse it later.
- Import-into-existing replaces (decision 32), name-matching against the library (decision 33).
- Acceptance workbook: the 11pm bartender path + getting a menu in. Criteria 10, 12, 13 (11 waits for the spreadsheet route).

## After this build (not planned, just named)
Spreadsheet import; photo import (needs OCR provider + cost decision); POS import route; item library UI; multi-venue build; upgrade/marketing rework; Schedules-owned time pricing (returns happy-hour display); fallback-card authoring.

## Per-slice quality gates
Playwright specs with implementation (seed endpoint per spec, parallel-safe); impeccable detector on every UI edit + a critique/audit pass against the hi-fis before slice close; independent code review; exact-head CI green; owner acceptance workbook per slice (decision 17); the 18 acceptance criteria tracked as a running checklist — a criterion flips to "met" only with a named spec or review asserting it.
