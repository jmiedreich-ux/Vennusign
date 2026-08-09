# Menus Feature — Milestone Plan

- **Status:** Approved by owner 2026-08-07; **reconciled 2026-08-07 with the complete open-questions register (208/208 resolved)**. Qn references below point at recorded answers in `open-questions.md`, which govern on any conflict with earlier prose.
- **Authority:** `docs/design/approved/menus/` (`decisions.md` wins on any conflict) + `build-decisions.md` (17 owner decisions) + `open-questions.md` (all recorded answers). The bundle in `docs/design/approved/menus/` is owner-confirmed as the approved version (Q193).
- **Model:** small functional vertical milestones per the approved Track 1 retrospective. Every milestone ships schema → API → UI → Playwright specs together; tests are written with the implementation, never after. Each milestone is independently mergeable and leaves master releasable. **Every milestone ends with a short owner acceptance workbook (5–10 minutes) before the next milestone starts** (decision 17); milestone 1 gets a demo script instead since it has no screens.

## Scope guardrails

Single venue only. No multi-venue affordance may leak (decision 29; criterion 18 gets a named milestone-2 spec re-checked every UI milestone, Q194). Out of scope entirely: item library UI, fallback-card authoring, price requests, group permissions, POS-priced fields, mobile layouts (Q158 → #681), redesign of other nav areas' content, scheduling. Happy-hour price display parked until Schedules-owned pricing. Imports: paste + start-blank only; the confirm step is built once so spreadsheet/photo/POS plug in later.

Additional exclusions recorded in the register: item photos and upload (Q108 → #679), featured-item control (Q107 → #678), print (Q206 → #676), currency/format setting (Q115/Q190 → #675 — prices render exactly as typed), language/translation work (Q200 → #683 — Menus UI ships English-only; venue language fields dormant), separate audit/analytics system (Q207 → #677), keyboard reorder and canvas keyboard reachability (Q120/Q202 → #672), add-item-row keyboard flow (Q122 → #673), Play interaction spec (Q146 → #674), Welcome/title panel (Q98 → #670), room-distance line (Q133 → #671).

## The save model — owner decision, 2026-08-09

Two readings of "draft" were possible, and the first implementation drifted
between them. The owner settled it: **the draft is derived, not authored.**

- The live rows are the working state. An edit changes the menu immediately.
- The screens show the last **published snapshot**, and nothing else.
- The draft is **computed** by comparing the working state against that
  snapshot. "3 changes not on your screens" is that comparison, so the count
  cannot disagree with what Publish will ship.
- Publish snapshots the working state as it stands and sends it to the screens.
- Clients never supply a `beforeValue`. The previous value always comes from the
  published snapshot, so a stale caller cannot misreport it or delete another
  editor's pending change.

This is what Q182 already described — "each thing currently different from the
screens" — and what makes "the canvas *is* the preview" true rather than a
layering trick. Its one cost, accepted knowingly: you publish a menu whole. There
is no partial publish of selected changes, which matches one queue per menu.

Two consequences landed with it, both in milestone 1:

- **There is no draft table.** Migration 058 creates none, and nothing writes one,
  so no stored queue can disagree with what Publish ships.
- **There is no legacy editor path.** The owner's rule — "there is no legacy,
  because it was not live" — is applied: the existing editor writes through
  `Items`/`Placements` like everything else, the owner-killed concepts (happy-hour
  price, quantity, tags, featured, per-item archive, daily special) have no
  endpoints or controls left, and platform operations is read-only for menus
  (Q36) until the backlogged impersonation-with-consent model exists.

## Cross-cutting rules (from the register)

- **Timestamps render in the venue's local time** from the venue's stored Timezone — on every Menus surface (Q196). Deviation from today's viewer-clock behavior.
- **Copy:** natural singular/zero forms everywhere, approved shapes for 2+ (Q181); no possessive phrasing in the draft pill (Q147); banned words (unpublish/supersede/restore/archive) enforced in Menus and rewritten surfaces only, elsewhere logged as copy debt #682 (Q179); criterion 4 reworded to name its deliberate acts (Q187).
- **Ceilings are tier-configurable** in the entitlement/allowance model, never constants (Q201): defaults ~50 menus/venue, ~500 items/menu, ~2,000-line paste; every refusal names the reason in a plain sentence.
- **Configurable timing:** page dwell is a per-menu setting, default 8s (Q9); the board-too-long warning threshold is a setting, default 60s of real loop time (Q175).
- **At-scale behavior ships inside each surface's own milestone** — never retrofitted — and the Playwright suite carries a 20-screen/13-menu seed (Q176).
- **Failure honesty:** failed draft saves flip the byline to amber "Couldn't save your last change — retrying…", auto-retry, and block Publish until the queue is confirmed (Q197); publish is atomic server-side with an honest failure sentence and untouched draft (Q198); a 401 mid-edit shows a sign-back-in prompt, holds the unsent change, and sends it after sign-in (Q199).
- **"N changes" counts the current diff** — latest state per field/item, exactly what Publish ships (Q182). After-publish sentences use generic forms; typed phrasing only for single-kind drafts (Q183).
- **Icons: lucide-react** (owner decision Q185 — new dependency, documented at introduction). Token batch-2 preserves the exact hi-fi values incl. selection-blue #2a78d6; board palette lives in theme definitions (Q178). Venue-name eyebrow uses `#64748b` (Q184). The venue name is a static label everywhere — no caret (Q186).
- **"Stale" is a distinct screen state** (online but silent 5 min, client-derived): its own amber exception, excluded from "ready", still publishable-to (Q160).

## Design follow-ups (needed before the milestone that consumes them)

1. **Before milestone 3:** quick price-change flow feel (Q76 flag — shared-price edits must feel easy).
2. **Before milestone 5:** a small design spot for the dwell-setting control (Q9) and the loop-warning threshold (Q175).
3. **Before milestone 6:** reconcile the "Skip these for now" path with the owner's resolve-at-import rule — a menu with flagged/guessed items must never publish them to a live board; skipping must force resolution before publish or the skip path goes (Q83). Plus a design spot for the near-miss match picker (Q94).

## Milestones

### Milestone 1 — The spine: item library + draft/publish save model
The schema everything else stands on. No visible UI change yet beyond keeping the current editor compiling.
- Item library data model: `Item` (venue-scoped), `Placement` (item on a section of a menu, ordered), sections restructured to hold placements. Migration **drops** `HappyHourPrice`, `QuantityAvailable`, `Tags`, `IsPopular`, per-item translations, and the `AvailabilityResetUtc` auto-reset concept — the migration script names what it discards. Field limits carry over: name ≤200 and never blank, description ≤1000 (Q119).
- **Menu↔screen assignment as its own table** (Q1/Q2): exactly one menu per screen, stored as a separate assignment record so Schedules can multiplex later without migration. Seeded/dev menus are auto-marked assigned+published so fixtures and demos work (Q3).
- Availability (86): item × venue, boolean + timestamp + who. Commits instantly, never queues, survives publish, stays off until a person turns it back on.
- Draft model: `DraftChange` per menu; the queue's count is the **current diff** (Q182). Explicit Publish ships the set **atomically** (Q198) as a `PublishEvent` with per-target delivery state; history is attributable; retention is tier-configurable. Discard draft and Put away commit immediately and are recorded as history entries in the same transaction. **Take off the screens is permanent, so it queues as a draft change and is applied by Publish** (Q68) — it is not instant like an 86. History is the provisional audit record, flagged in the demo (Q207).
- Tier-configurable ceilings land here with their defaults (Q201). Per-menu dwell (default 8s) and loop-warning threshold (default 60s) land as settings (Q9/Q175).
- API: draft queue read/write, publish, history list, "go back to" (produces a draft), availability toggle, assignment read/write. API exposes the venue timezone for venue-local rendering (Q196).
- Acceptance: demo script (seeded data walked through the API contract). Criteria 1, 2, 3, 4.

### Milestone 2 — App shell + board render engine + M1 Menus home + M1b named actions
- **New 76px icon nav rail app-wide** (decision 12) hosting every area; decision 19's nav gating lands here. Icons via **lucide-react** (Q185). Interim wiring rule: anything whose destination doesn't exist yet is absent, never greyed; card-click temporarily opens the existing editor and Add-a-menu uses the existing create flow until milestones 3/6 land (Q100).
- **Board render engine v1**: Coastal + Classic dark (provisional pending Q86), sections, dotted leaders, 86'd items not rendered. Empty sections don't render; the theme-generated venue-name header strip is the branded title (Q98 provisional). Prices render exactly as typed, em dash for a missing size price (Q115/Q190). One engine flag separates preview surfaces (annotations shown) from the guest TV (none) (Q135). Continuation pages repeat the section heading for guests; "2 OF 2" counters are back-office only (Q137).
- Token batch-2 merges with the approved additions; components consume variables only (Q178). Playfair Display self-hosted.
- M1: shelf with live board-render cards; pending-changes cards crop top-aligned (Q191); status headline names each holding menu, screen-count phrases capped at the top three (Q169); "Fix these 2" opens Screens filtered to those screens (Q170); "Not in use" strip; Add-a-menu tile. **Scale ships now** (Q176): one cutover at ≥7 total menus — search, single-select filter chips (none active on load, Q164), compact 6-across grid (on-screen menus always visible, most-recently-edited fill, inline "N more ▾" staying open for the session, Q165), "Add a menu" as a plain button beside search (Q166), Not-in-use collapse; at ≤6 the shelf is exactly M1 (Q163). Empty state = the onboarding routes (paste / blank for now).
- M1b: card ⋯ menu — six items, "Put away" directly after Duplicate, "Take off the screens" alone below the last divider (Q195); Take-off dialog showing the generated fallback card; Go back to… as time-phrased history. Singular/zero copy forms (Q181).
- Venue fallback: generated logo-and-name card object — shown, not authorable. Venue-name eyebrow `#64748b` (Q184); static venue label, no caret (Q186).
- **Criterion 18 gets its named spec here** and is re-checked every UI milestone (Q194); the 20-screen/13-menu seed enters the Playwright fixtures (Q176).
- Acceptance workbook: shelf, actions, gating. Criteria 5, 6, 8.

### Milestone 3 — M2 builder + M2a adding items
- Four-column builder: section rail (navigator only; '+' adds a section as an inline row in typing mode, Q95), canvas-as-preview with drag-to-reorder (pill on hover as well as selection, Q103; cross-section moves wait for milestone 5), 86'd rendering (item selectable and editable, red-tinted "Off right now" panel, Q104), One-section view grows and scrolls (Q105), first open shows One-section view/top section/nothing selected (Q116), single selection only (Q117), in-place editing is the price only (Q118), inspector keeps its place with a quiet placeholder when nothing is selected (Q106). Section rename by typing over the canvas heading; quiet delete releases items back to the library (Q96). Item removal: Delete/Backspace + a quiet inspector link, item stays in the library (Q97). **No "Feature on the board" checkbox, no photo affordances** (Q107/Q108).
- Inspector theme footer opens a small picker over the canvas, only shipped looks, queued as a draft change (Q109).
- **⌘K find-an-item-on-this-board ships here** (Q121 — plan omission corrected).
- M2a: one inline add row per section — search covers the whole venue library including 86'd items; an item already on this board is labelled and picking it jumps instead of duplicating (Q112); "Create as new" born with the typed name, empty price/description, inspector focused on name; missing price = quiet flag, publish not blocked (Q113); "where it lives" vocabulary locked (Q123). Bulk place drawer opens from an "Add many at once" link on the add-item row (Q95), stays open after a place with a brief note (Q124). No "From POS" chip (Q114).
- Publish bar: clean state is the home of screen status (Q111); chips per screen at ≤6 targets, count-plus-exception-cards above, one "Publish N changes" label (Q161); the bar grows and wraps when exceptions outnumber one row (Q167); footer strip ships only the "See all →" link opening a read-only screens panel (Q168); "discard draft" carries the provisional stakes-naming confirmation (Q110 → #680); save-failure and 401 behaviors per the cross-cutting rules (Q197/Q199).
- Undo/redo keystroke, session-scoped. Quick update | Build segmented control absent until milestone 6 (Q100).
- "Viewing as" dropdown: target screens including offline (last-reported shape), "No screens yet" over the default canvas, names without resolution until milestone 4 (Q101). Play button always visible with honest empty/blocked states (Q102).
- Acceptance workbook: edit → draft → publish end-to-end. Criteria 7, plus 2/4 re-asserted through the UI.

### Milestone 4 — The player: board rendering + geometry + honest delivery
- **Display player renders published boards** with the same engine: pages, dwell cycle, venue fallback when nothing is published. **An 86 takes the item down immediately, even mid-page; a publish swaps at the next page turn and restarts the cycle at page 1** (Q203). The TV renders zero annotations (Q135); pagination derives from guest-visible content only (Q136).
- Heartbeat extends to report device geometry; `Screen` gains reported-geometry fields + report timestamp. Device-reported, never user-entered. Degrades honestly: resolution without panel size keeps the Readable-from panel with a plain reason, never a guessed size (Q139). "Stale" remains the distinct third state (Q160).
- 86 age copy gains the day when older than today (Q189). The 86 warning/toast counts every screen showing the item through any menu, with honest one/zero-screen forms and per-screen truth when a screen is offline (Q180).
- Hygiene rider: enforce `screen.content.target` on push/push-all (+ reset/unpair gates).
- **Compatibility bar:** the engine runs wherever the current display app already runs — no new browser floor — proven on one real device this milestone; capture the fleet hardware inventory at that check (Q205).
- Acceptance workbook: publish → watch the TV change (a **paired browser tab is the TV**, its window size the reported geometry; one real-TV check before build close if available, Q204); 86 → vanishes within the **10-second pass line** on an online screen (Q188); offline screen catches up. Criteria 1 (on-screen), 4, 9 (partial).

### Milestone 5 — M2b board view + M2c Play
- M2b: whole-board zoom, sections as draggable blocks, pages as a consequence of overflow. Page strip is navigation only (Q125). Per-screen override = scope switch with a visible marker and one revert action, no separate editor (Q126). Overflow fixes: generic "Split into two sections" at the overflow point (Q127); "Starts on page 3" is a read-only fact (Q128); dashed slot is both drop target and click-to-create (Q130); chip wording plain — "online — ready · 3 pages on this screen" (Q129). **Only "Shorten the dwell" ships as a live fix action** (Q162).
- M2c: **full-window takeover** (Q134), auto-plays on entry from page 1 on the "Viewing as" screen (Q140). Draft pill counts all queued changes, no possessive phrasing, disappears when the queue is empty, never clickable (Q141/Q147). An 86 applies live at the next page turn; colleagues' drafts appear on next entry (Q142). Unassigned menus playable against any reported screen (Q143); offline/stale screens selectable with the honest note (Q138). Problem card: page-splits only, absent at zero (Q131), one line per differing screen (Q173). Readable-from: cap-height × 10 ft (Q132), no room-comparison line (Q133), panel survives at scale for the selected screen (Q174). Picker: chips ≤6 targets, searchable sidebar above (Q171) with representative rows + expandable identical extras (Q172). Timeline: dashed only for pure overflow continuations (Q144), auto-derived labels (Q145), scale behavior per Q149; transport entirely simulated (Q150); default interactions only (Q146 → #674). Loop-warning at the configured threshold (Q175).
- Acceptance workbook: arrange a board, watch it play as each screen. Criterion 9 complete.

### Milestone 6 — M3 Quick Update + import routes (paste, blank) + shared confirm step
- M3: flat searchable list of the menu **as published** (Q151); search also matches on-air items from other menus, labelled and toggleable in place (Q152); one availability toggle per row (bulk 86 accepted as lost; watch the workbook, Q157); unplaced items keep their toggle with the no-screen sub-line (Q154); browse groups by placement with a "Not on any board" tail, off items pinned first on expand (Q155), sections collapse past six (Q156); rows off >1 day go amber with a day count, footer names the oldest (Q153); undo toast ~10s with live age, the toggle itself is always the undo (Q159). Desktop-only; stacks below ~900px without claiming mobile (Q158 → #681).
- Import: paste route (live parse, caps line = section, ~2,000-line tier-configurable ceiling with a plain refusal, Q201) and start-blank (New menu draft → Pick-a-look → builder with one empty renameable section, add-item row focused; shelf card "Never published"; Coastal default, Q85). Pick-a-look sequencing per Q89.
- **Shared confirm step**: always appears as the one ending, zero-question form when clean, hosts the Name field (pre-filled from a title-like first line, else "New menu", Q87/Q88); "See all" is a read-only amber-highlighted list (Q91); near-miss matching is library-only and tidying-level, when in doubt a new item (Q90/Q93), one grouped question with the match picker (Q94, design spot per follow-up 3); draft created at route commit, abandoning = skip, confirm is one-shot (Q92). **Resolve-at-import rule governs (Q83): flagged/guessed items never publish to a live board — the reconciled skip/flag design from follow-up 3 applies here.** Re-import destination line on the route surface — "This will be a new menu · or replace one you have ▾" (Q84); import-into-existing replaces, preserves layout/theme/86s.
- Acceptance workbook: the 11pm bartender path + getting a menu in. Criteria 10, 12, 13 (11 waits for the spreadsheet route).

## After this build (not planned, just named)
Spreadsheet import; photo import (needs OCR provider + cost decision); POS import route; item library UI; multi-venue build; upgrade/marketing rework; Schedules-owned time pricing (returns happy-hour display); fallback-card authoring; plus the register's backlog issues #670–#683.

## Per-milestone quality gates
Playwright specs with implementation (seed endpoint per spec, parallel-safe; 20-screen/13-menu scale seed from milestone 2); impeccable detector on every UI edit + a critique/audit pass against the hi-fis before milestone close; independent code review; exact-head CI green; owner acceptance workbook per milestone (decision 17); the 18 acceptance criteria tracked as a running checklist — criterion 18 asserted by a named spec from milestone 2 and re-checked each UI milestone, criteria 11 and 14–17 stamped "deferred to a later build" (Q194); a criterion flips to "met" only with a named spec or review asserting it. **Hosted-agent subjective QA on demand** (owner-approved 2026-08-07): when a milestone carries subjective judgment cases the deterministic specs cannot assert, run them through the Track 1 hosted-agent pattern (`scripts/run-track1-qa.ps1` lineage, ~$1.70/run) before the owner workbook; the cases and cost are noted in that milestone's workbook.
