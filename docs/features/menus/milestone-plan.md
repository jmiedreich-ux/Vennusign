# Menus Feature — Milestone Plan

- **Status:** Approved by owner 2026-08-07; **reconciled 2026-08-07 with the complete open-questions register (208/208 resolved)** and amended 2026-08-13 for the owner-approved Milestone 4 foundation boundary. Qn references below point at recorded answers in `open-questions.md`, which govern product behavior; the milestone amendment governs when that behavior ships.
- **Authority:** `docs/design/approved/menus/` (`decisions.md` wins on any conflict) + `build-decisions.md` (17 owner decisions) + `open-questions.md` (all recorded answers). The bundle in `docs/design/approved/menus/` is owner-confirmed as the approved version (Q193).
- **Model:** small functional vertical milestones per the approved Track 1 retrospective. Every milestone ships schema → API → UI → Playwright specs together; tests are written with the implementation, never after. Each milestone is independently mergeable and leaves master releasable. **Every milestone ends with a short owner acceptance workbook (5–10 minutes) before the next milestone starts** (decision 17); milestone 1 gets a demo script instead since it has no screens.
- **Owner planning workspace for Milestones 4–6:** [VennueSign Menus M3-A Slices 4–6 Planning](https://docs.google.com/spreadsheets/d/1DCtCrn5NAXCTNt5csmrjAOJvcCws7l9fdsnGQUCHFkM/edit). Google Sheets is the owner's planning space; agents use GitHub and the controlled repository records for their planning. Do not edit the Sheet unless the owner explicitly asks. An owner decision made there becomes implementation authority only after it is synchronized into the controlled repository records.

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

- **Keyboard is out of scope for this build — owner ruling 2026-08-10, reaffirmed.** Nothing further is built or tested for keyboard, no keyboard findings are filed against these milestones, and no acceptance step may depend on a keystroke. Q202 (canvas reachability) and Q120 (keyboard reorder) were already backlogged to **#672**; this extends that to the build as a whole. It does **not** mean deleting behaviour already shipped and named here — the undo/redo keystroke and ⌘K (Q121) stay in the code; they simply stop being relied on or extended. The owner reversed course on this after it was raised repeatedly; it is settled, and re-raising it is the defect.
- **Mobile interactions are out of scope for this build — Q158 → #681, owner reaffirmed 2026-08-10.** Desktop browser interaction is the Menus milestone gate. The shared Playwright mobile project may continue to catch accidental layout crashes, but a desktop-only interaction such as item drag is explicitly skipped there rather than gaining an invented touch design.
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

1. **Before milestone 3:** quick price-change flow feel (**Q5** flag — shared-price edits must feel easy). *Corrected 2026-08-10: this line cited Q76, which is about refresh cadence; the flag is Q5's.* **Resolved at the milestone 3 readiness pass (2026-08-10), provisionally:** the inspector states the fact quietly and permanently under the price — "Also on Late Night and Brunch — they show the new price when you publish them", using Q123's locked vocabulary — with no dialog and no separate quick-price mode. A confirmation on every price edit is the opposite of "feels easy", and a second editing mode is undesigned and would be the second editor decision 15 refuses.
2. **Before milestone 5:** a small design spot for the dwell-setting control (Q9) and the loop-warning threshold (Q175).
3. **Before milestone 6-A:** reconcile the "Skip these for now" path with the owner's resolve-at-import rule and design the near-miss picker (Q83/Q94). **Resolved 2026-08-13:** the approved paste-import storyboard removes skip-to-builder, requires every ambiguity before confirmation, provides **Same item / New item / another candidate**, and preselects no semantic match. Authority: `docs/design/approved/menus/paste-import/` and decisions 33, 37–43.

## Milestones

### Milestone 1 — The spine: item library + draft/publish save model
The schema everything else stands on. No visible UI change yet beyond keeping the current editor compiling.
- Item library data model: `Item` (venue-scoped), `Placement` (item on a section of a menu, ordered), sections restructured to hold placements. The migration **drops** per-item translations and the `AvailabilityResetUtc` auto-reset concept outright, and names every field it discards. `HappyHourPrice`, `QuantityAvailable`, `Tags` and `IsPopular` are **retained as columns and deliberately unread** — POS sync still writes them, so they are dropped by the milestone that retires their last reader (owner decision at the first acceptance). Field limits carry over: name ≤200 and never blank, description ≤1000 (Q119).
- **Menu↔screen assignment as its own table** (Q1/Q2): exactly one menu per screen, stored as a separate assignment record so Schedules can multiplex later without migration. Seeded/dev menus are auto-marked assigned+published so fixtures and demos work (Q3).
- Availability (86): item × venue, boolean + timestamp + who. Commits instantly, never queues, survives publish, stays off until a person turns it back on. This milestone proves the notification contract; the screens render the new item model when the milestone 4 player lands.
- Save model: **there is no draft table** — the draft is derived (see §The save model). The count is the **current diff** (Q182), and the publish that ships it proves the menu has not moved since the diff was computed, so what history records always describes the snapshot that went out. Explicit Publish ships **atomically** (Q198) as a `PublishEvent` with per-target delivery state; history is attributable; retention is tier-configurable. Discard draft, Put away and Take off the screens each commit with their history entry in the same transaction. **Take off the screens is permanent, so it waits as a difference and reaches the screens on the next Publish** (Q68) — it is not instant like an 86, and the publish that carries it records `taken_off_screens` under its own name. A screen another menu has since been given is never touched by a stale act and is named to the caller. History is the provisional audit record, flagged in the demo (Q207).
- Tier-configurable ceilings land here with their defaults (Q201), enforced under the same lock as the write they bound. Put-away menus do not count against the menu ceiling, so the refusal's advice works. Per-menu dwell (default 8s) and loop-warning threshold (default 60s) land as settings (Q9/Q175).
- API: draft read, publish, history list, "go back to" (produces a draft), availability toggle, assignment read/write, take-off, and put away / put back on the shelf. API exposes the venue timezone for venue-local rendering (Q196). The pre-existing editor is consolidated onto this spine; ops menu writes retire (Q36).
- Acceptance: demo script (seeded data walked through the API contract). Criteria 1, 2, 3, 4.

### Milestone 2 — App shell + board render engine + M1 Menus home + M1b named actions
- **New 76px icon nav rail app-wide** (decision 12) hosting every area; decision 19's nav gating lands here. Icons via **lucide-react** (Q185). Interim wiring rule: anything whose destination doesn't exist yet is absent, never greyed; card-click temporarily opens the existing editor and Add-a-menu uses the existing create flow until milestones 3/6 land (Q100).
- **Board render engine v1**: **no named looks ship** (Q86 resolved). The engine renders whatever menu theme the menu has attached, so themes built later in the theme editor need no engine change. **A menu with no theme attached is a valid state and still renders** — plainly and badly, but never blank, never a silently invented fallback, never a failure. Sections, dotted leaders, 86'd items not rendered. Empty sections don't render. The engine draws **no venue-name title strip** — if the TV carries one, the theme editor owns it (Q98 resolved).
- **The shell theme is this milestone's theme work** (Q86): the icon nav rail, token batch-2, the sky-blue chrome. One shell theme ships, built so a dark variant or others can be added without reopening it — variables only, no colour baked into a component. Prices render exactly as typed, em dash for a missing size price (Q115/Q190). One engine flag separates preview surfaces (annotations shown) from the guest TV (none) (Q135). Continuation pages repeat the section heading for guests; "2 OF 2" counters are back-office only (Q137).
- Token batch-2 merges with the approved additions; components consume variables only (Q178). Playfair Display self-hosted.
- M1: shelf with live board-render cards; pending-changes cards crop top-aligned (Q191); status headline names each holding menu, screen-count phrases capped at the top three (Q169); "Fix these 2" opens Screens filtered to those screens (Q170); "Not in use" strip; Add-a-menu tile. **Scale ships now** (Q176): one cutover at ≥7 total menus — search, single-select filter chips (none active on load, Q164), compact 6-across grid (on-screen menus always visible, most-recently-edited fill, inline "N more ▾" staying open for the session, Q165), "Add a menu" as a plain button beside search (Q166), Not-in-use collapse; at ≤6 the shelf is exactly M1 (Q163). Empty state = the onboarding routes (paste / blank for now).
- M1b: card ⋯ menu — six items, "Put away" directly after Duplicate, "Take off the screens" alone below the last divider (Q195); Take-off dialog showing the generated fallback card; Go back to… as time-phrased history. Singular/zero copy forms (Q181).
- Venue fallback: generated logo-and-name card object — shown, not authorable. Venue-name eyebrow `#64748b` (Q184); static venue label, no caret (Q186).
- **Criterion 18 gets its named spec here** and is re-checked every UI milestone (Q194); the 20-screen/13-menu seed enters the Playwright fixtures (Q176).
- Acceptance workbook: shelf, actions, gating. Criteria 5, 6, 8.

### Milestone 3 — M2 builder + M2a adding items
- Four-column builder: section rail (navigator only; '+' adds a section as an inline row in typing mode, Q95), canvas-as-preview with drag-to-reorder (pill on hover as well as selection, Q103; cross-section moves wait for milestone 5), 86'd rendering (item selectable and editable, red-tinted "Off right now" panel, Q104), One-section view grows and scrolls (Q105), first open shows One-section view/top section/nothing selected (Q116), single selection only (Q117), in-place editing is the price only (Q118), inspector keeps its place with a quiet placeholder when nothing is selected (Q106). Section rename by typing over the canvas heading; quiet delete releases items back to the library (Q96). Item removal: Delete/Backspace + a quiet inspector link, item stays in the library (Q97). **No "Feature on the board" checkbox, no photo affordances** (Q107/Q108).
- Inspector theme footer opens a small picker over the canvas, listing the menu themes that exist for the venue, queued as a draft change (Q109). With none built yet it shows the empty state rather than a hard-coded list (Q86).
- **⌘K find-an-item-on-this-board ships here** (Q121 — plan omission corrected).
- M2a: one inline add row per section — search covers the whole venue library including 86'd items; an item already on this board is labelled and picking it jumps instead of duplicating (Q112); "Create as new" born with the typed name, empty price/description, inspector focused on name; missing price = quiet flag, publish not blocked (Q113); "where it lives" vocabulary locked (Q123). Bulk place drawer opens from an "Add many at once" link on the add-item row (Q95), stays open after a place with a brief note (Q124). No "From POS" chip (Q114).
- Publish bar: clean state is the home of screen status (Q111); chips per screen at ≤6 targets, count-plus-exception-cards above, one "Publish N changes" label (Q161); the bar grows and wraps when exceptions outnumber one row (Q167); footer strip ships only the "See all →" link opening a read-only screens panel (Q168); "discard draft" carries the provisional stakes-naming confirmation (Q110 → #680); save-failure and 401 behaviors per the cross-cutting rules (Q197/Q199).
- Undo/redo keystroke, session-scoped. Quick update | Build segmented control absent until milestone 6 (Q100).
- "Viewing as" dropdown: target screens including offline (last-reported shape), "No screens yet" over the default canvas, names without resolution until milestone 4 (Q101). Play button always visible with honest empty/blocked states (Q102).
- Acceptance workbook: edit → draft → publish end-to-end. Criteria 7, plus 2/4 re-asserted through the UI.

### Milestone 4 — Content and delivery foundations

**Owner amendments, 2026-08-13:** this milestone no longer delivers the display player, and geometry-driven pagination is also deferred because it belongs with canvas/theme/player work. This milestone establishes non-visual content and delivery foundations without changing what `src/display` renders. Q135/Q136/Q160/Q180/Q189 remain governing behavior where applicable; Q139/Q188/Q203/Q204/Q205 move with the later canvas/player delivery and are not acceptance claims here.

- Build a published-snapshot-only guest projection. Drafts never enter it; unavailable/86'd items and empty guest sections are removed before layout; missing prices remain valid; editor/preview annotations never consume guest layout space (Q135/Q136).
- Preserve authored page/section order in the guest projection, but do not calculate geometry-driven overflow or new page boundaries. Add the older-than-today 86 age copy and honest zero/one/many plus offline-screen impact wording (Q180/Q189). Stale remains distinct from offline (Q160).
- Hygiene rider: enforce `screen.content.target` on push/push-all, reset, and unpair, including permission and stale-actor refusals with no partial mutation.
- Acceptance workbook proves foundation-visible outcomes: published versus draft projection, guest filtering, availability impact copy, and permission refusals. Applicable Back Office Playwright remains required; player-visible Playwright is not applicable because no player surface changes.
- **Explicitly deferred:** geometry reporting; geometry-driven pagination/overflow; canvas/theme layout work; rendering Menus in `src/display`; dwell/cycling; venue fallback playback; publish cutover at a page boundary; immediate mid-page 86 mutation; offline-player catch-up; the 10-second live-screen pass line; paired-TV and real-device checks; compatibility certification. These require separately owner-planned canvas/player work.

### Milestone 5 — M2b board view + M2c Play — out of scope for the current sequence
- Owner planning decision synchronized 2026-08-13: all Slice 5 rows in `VennuSign Planning` are `Out of scope / Blocked`. Backlog issue #709 retains the bundle for later canvas/player planning; it is not the next milestone and is not a dependency of Slice 6.
- M2b: whole-board zoom, sections as draggable blocks, pages as a consequence of overflow. Page strip is navigation only (Q125). Per-screen override = scope switch with a visible marker and one revert action, no separate editor (Q126). Overflow fixes: generic "Split into two sections" at the overflow point (Q127); "Starts on page 3" is a read-only fact (Q128); dashed slot is both drop target and click-to-create (Q130); chip wording plain — "online — ready · 3 pages on this screen" (Q129). **Only "Shorten the dwell" ships as a live fix action** (Q162).
- M2c: **full-window takeover** (Q134), auto-plays on entry from page 1 on the "Viewing as" screen (Q140). Draft pill counts all queued changes, no possessive phrasing, disappears when the queue is empty, never clickable (Q141/Q147). An 86 applies live at the next page turn; colleagues' drafts appear on next entry (Q142). Unassigned menus playable against any reported screen (Q143); offline/stale screens selectable with the honest note (Q138). Problem card: page-splits only, absent at zero (Q131), one line per differing screen (Q173). Readable-from: cap-height × 10 ft (Q132), no room-comparison line (Q133), panel survives at scale for the selected screen (Q174). Picker: chips ≤6 targets, searchable sidebar above (Q171) with representative rows + expandable identical extras (Q172). Timeline: dashed only for pure overflow continuations (Q144), auto-derived labels (Q145), scale behavior per Q149; transport entirely simulated (Q150); default interactions only (Q146 → #674). Loop-warning at the configured threshold (Q175).
- If revived through fresh owner planning, its acceptance workbook arranges a board and watches it play as each screen; only then can criterion 9 become complete.

### Milestone 6 — M3 Quick Update + blank creation — next; redesigned concept received
- Owner planning decision synchronized 2026-08-13: Quick Update is now the “86 board,” a three-column surface in which the section rail answers where the operator is looking, the tile grid answers what can be taken off, and the off-list answers what is already off. Taking an item off removes it from the grid and adds it to the across-menu panel. Guest vocabulary is “Sold out”; staff vocabulary is “86”; counts state exactly what they count. Decision detail is landed in approved decision 15; the owner-supplied concept image is retained as `docs/design/approved/menus/86-board-7b.png`.
- `decisions.md` decision 15 now carries the resolved planning-sheet Q12–Q20 behavior (not feature-register Q12–Q20): one tile per published placement; confirmation before every 86; search only across published menus assigned to screens; authored menu/section rail labels; prior-venue-day carryover review without automatic restore; global Back on sale; honest offline/stale reporting; no unplaced items. Blank menu creation remains a separate Menus Home route in this slice.
- The older feature-register Q151–Q159 specification below remains historical input for the required design/path audit, not automatic implementation authority where it conflicts with the new concept.
- M3: flat searchable list of the menu **as published** (Q151); search also matches on-air items from other menus, labelled and toggleable in place (Q152); one availability toggle per row (bulk 86 accepted as lost; watch the workbook, Q157); unplaced items keep their toggle with the no-screen sub-line (Q154); browse groups by placement with a "Not on any board" tail, off items pinned first on expand (Q155), sections collapse past six (Q156); rows off >1 day go amber with a day count, footer names the oldest (Q153); undo toast ~10s with live age, the toggle itself is always the undo (Q159). Desktop-only; stacks below ~900px without claiming mobile (Q158 → #681).
- Start blank: New menu draft → Pick-a-look → builder with one empty renameable section, add-item row focused; shelf card "Never published"; the venue's default menu theme, Q85 — not a named look, Q86. Pick-a-look sequencing per Q89.
- Acceptance workbook: redesigned Quick Update plus blank menu creation.

### Milestone 6-A — paste import delivery sequence

**Approved design authority:** `docs/design/approved/menus/paste-import/` (owner approved 2026-08-13). The storyboard is the visual authority; decisions 33 and 37–43 govern behavior and supersede conflicting Q83/Q84/Q92/Q93/Q94 answers. Delivery is split into three sequential vertical milestones. Each ships its own schema → API → UI → Playwright coverage and owner acceptance workbook; the split is by customer outcome, never by technical layer.

#### Milestone 6-A1 — paste, parse, and review
- **Delivery status (2026-08-14): complete.** PR #716 merged to `master` as `ac4cc98`; issue #714 is closed. All seven workbook checks passed against product `547aea7`; independent exact-head review approved. The durable acceptance record is `m6a1-acceptance-record.json`.
- Persist a resumable import session containing raw paste, parsed structure, identity decisions, revisions and the resolved absolute expiry. Live parse retains every source line; a caps line becomes a section. Resolve the tier-configurable ~2,000-line and ~500-item ceilings before parsing. Session retention comes from centralized tier configuration; successful mutations may renew it and passive reads never do.
- Present unresolved questions first and settled inventory collapsed. Automatic matching is limited to exact normalization of case, punctuation and spacing. Semantic near-misses are grouped with no preselection. Bounded **Accept safe matches** applies only to normalization-safe rows and leaves ambiguous rows unanswered.
- Use one **Imported items** fallback with per-line reason metadata. Allow an eligible unreadable line to become a section only through an explicit reversible review action. Refresh preserves still-valid answers and clears/explains only dependency-affected answers.
- End state: a fully resolved, resumable review session ready for destination choice, with **no menu mutation**.
- Acceptance workbook: paste/read progress, clean and ambiguous review, fallback and promotion, refresh/resume, sign-in recovery, expiry, permission/tier change, smallest/largest supported widths, and maximum-size evidence. Criterion 10.

#### Milestone 6-A2 — create a new menu
- **Delivery status (2026-08-14): owner acceptance ready.** Draft PR #719 has product `b1e62c4`; local gates, UI finish review and exact-head independent engineering review approve. Run `m6a2-acceptance-workbook.html`; owner acceptance remains before merge.
- From a fully resolved 6-A1 session, choose **Create a new menu**, enter/confirm its name, and perform the first menu mutation only at final confirmation.
- Recheck the session lease, permission, tier, ceilings and revision under one set-based, atomic, idempotent transaction. A refusal rolls back all menu changes and preserves still-valid session answers; retry cannot create duplicates.
- Paste prices are menu-scoped and never silently mutate another menu. Completion says **Not live yet** and offers **Review draft in builder** or **Done for now**; screens remain unchanged until later Publish.
- Acceptance workbook: create happy path, validation on entry and edit, abandon/back/resume, expiry, refusal, permission/tier change, double-submit/retry/idempotency, truthful completion and builder handoff. Criterion 12 and the create portion of criterion 13.

#### Milestone 6-A3 — replace an existing menu
- From a fully resolved 6-A1 session, choose **Replace an existing menu** and show target identity, the server-computed unpublished-change total/category breakdown, what changes, and what stays live.
- Recheck the import-session and target leases, permission, tier, ceilings and revisions under one set-based, atomic, idempotent transaction. Preserve menu identity, theme, assignments, published snapshot and active availability/86 state. Conflict, stale target, lock loss or refusal changes no menu data and preserves still-valid review work.
- Atomically preserve the complete pre-import working state. Keep all historical replacement snapshots; centralized configuration and tier determine stored scope, retention, restore eligibility and limits. Restoration creates a new working revision and never rewinds published history.
- Completion uses the same **Not live yet**, **Review draft in builder**, and **Done for now** contract as 6-A2. Screens remain unchanged until later Publish.
- Acceptance workbook: replacement happy path, wrong/stale target, unpublished delta, conflict/lock loss, permission/tier change, snapshot retention/restore, active 86 and assignment preservation, cross-menu price isolation, retry/idempotency and truthful completion. Replacement portion of criterion 13.

**Shared display/accessibility scope:** the supported-width floor is 900px; below it, preserve the session and offer a resumable wider-window handoff rather than compressing the workflow. Keyboard-specific interaction design/testing is excluded; semantic controls, accessible names/relationships, visible focus, and screen-reader-compatible status/error announcements remain required.

## After this build (not planned, just named)
Spreadsheet import; photo import (needs OCR provider + cost decision); POS import route; item library UI; multi-venue build; upgrade/marketing rework; Schedules-owned time pricing (returns happy-hour display); fallback-card authoring; plus the register's backlog issues #670–#683.

## Per-milestone quality gates
Playwright specs with implementation (seed endpoint per spec, parallel-safe; 20-screen/13-menu scale seed from milestone 2); impeccable detector on every UI edit + a critique/audit pass against the hi-fis before milestone close; independent code review; exact-head CI green; owner acceptance workbook per milestone (decision 17); the 18 acceptance criteria tracked as a running checklist — criterion 18 asserted by a named spec from milestone 2 and re-checked each UI milestone, criteria 11 and 14–17 stamped "deferred to a later build" (Q194); a criterion flips to "met" only with a named spec or review asserting it. **Hosted-agent subjective QA on demand** (owner-approved 2026-08-07): when a milestone carries subjective judgment cases the deterministic specs cannot assert, run them through the Track 1 hosted-agent pattern (`scripts/run-track1-qa.ps1` lineage, ~$1.70/run) before the owner workbook; the cases and cost are noted in that milestone's workbook.

## Acceptance workbook conventions

Each milestone's workbook is built from the last one, so these carry forward rather
than being rediscovered. Every item here came from an owner running one.

- **Evidence lives with the thing it is evidence of.** Every check takes pasted
  screenshots (Ctrl+V, drop, or file pick), and so does the closing decision — a shot
  of what decided it belongs with the decision, not filed under whichever check
  happened to be open. Images are downscaled before storage; the record lives in
  `localStorage` and a couple of raw 4K PNGs would fill it and start losing outcomes
  silently.
- **Never ask twice for the same thing.** The M3 workbook had a "Decisions on the
  three flagged items" box restating what the case sections already carried in
  context; it collected a duplicate or a shrug, and neither is evidence. Removed at
  the owner's instruction, 2026-08-10.
- **Recording an outcome advances the workbook** (owner instruction 2026-08-10, from
  M4 on): choosing Pass, Fail or Needs Adjustment collapses that check and opens the
  next one. Walking a workbook is a sequence, and the reviewer should not be closing
  and opening sections by hand between every observation. The last check in a journey
  opens the first of the next; the last check overall leaves the closing decision in
  view. Anything already recorded stays reopenable — this is advancing, not locking.
- **Judgment calls are marked as such** and named in the intro, so an owner knows
  which cases are asking for a decision rather than an observation.
- **Checks covering behaviour a review found missing say so**, because an owner
  walking a shorter list could not have noticed the absence themselves.
- **The fixture supplies every non-trivial precondition.** An acceptance case does
  not ask the owner to duplicate, create, pair or otherwise construct the state it
  is meant to judge. Case 6 in M3 was skipped after "board" was read as "screen";
  the shared-item condition is now pre-seeded and the step says "two menus".
- **The closing decision stays disabled** until every check has an outcome, and
  "Accept" stays disabled while any check is not a Pass.
