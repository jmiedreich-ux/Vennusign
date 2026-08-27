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
- **Delivery status (2026-08-14): complete.** Owner passed 6/6 against product `b1e62c4`; the acceptance-requested focus correction was independently approved at exact PR head `95f6e5c`. PR #719 merged as `b27159d`, issue #718 closed, and the feature branch was deleted.
- From a fully resolved 6-A1 session, choose **Create a new menu**, enter/confirm its name, and perform the first menu mutation only at final confirmation.
- Recheck the session lease, permission, tier, ceilings and revision under one set-based, atomic, idempotent transaction. A refusal rolls back all menu changes and preserves still-valid session answers; retry cannot create duplicates.
- Paste prices are menu-scoped and never silently mutate another menu. Completion says **Not live yet** and offers **Review draft in builder** or **Done for now**; screens remain unchanged until later Publish.
- Acceptance workbook: create happy path, validation on entry and edit, abandon/back/resume, expiry, refusal, permission/tier change, double-submit/retry/idempotency, truthful completion and builder handoff. Criterion 12 and the create portion of criterion 13.

#### Milestone 6-A3 — replace an existing menu
- **Delivery status (2026-08-14): complete.** PR #721 merged to `master` as `c32fda2`; issue #720 is closed and the feature branch is deleted. Acceptance passed 7/7 against product `58e8258` at `2026-08-14T05:40:26.942Z`; record: `m6a3-acceptance-record.json`. Focused API/migration tests passed 51/51; MenuImport LocalDB passed 12/12 with Azure unset; Back Office static passed 204/204; focused desktop replacement Playwright passed. Engineering and Impeccable reviews approve.
- From a fully resolved 6-A1 session, choose **Replace an existing menu** and show target identity, the server-computed unpublished-change total/category breakdown, what changes, and what stays live.
- Recheck the import-session and target leases, permission, tier, ceilings and revisions under one set-based, atomic, idempotent transaction. Preserve menu identity, theme, assignments, published snapshot and active availability/86 state. Conflict, stale target, lock loss or refusal changes no menu data and preserves still-valid review work.
- Atomically preserve the complete pre-import working state. Keep all historical replacement snapshots; centralized configuration and tier determine stored scope, retention, restore eligibility and limits. Restoration creates a new working revision and never rewinds published history.
- Completion uses the same **Not live yet**, **Review draft in builder**, and **Done for now** contract as 6-A2. Screens remain unchanged until later Publish.
- Acceptance workbook: replacement happy path, wrong/stale target, unpublished delta, conflict/lock loss, permission/tier change, snapshot retention/restore, active 86 and assignment preservation, cross-menu price isolation, retry/idempotency and truthful completion. Replacement portion of criterion 13.

#### Milestone 6-A4 — the parser reads an ordinary menu

**Why this exists.** 6-A1 through 6-A3 are complete and the import route is wired end to end — client → `BackOfficeMenuImportsController` → `MenuImportService` → repository, migrations 068–071. Pasting a real menu into it on 2026-08-25 returned `201` with the sections found correctly and **zero items**. Every item line came back `unresolved` / `item_format_not_recognized`.

##### What was wrong

`MenuPasteParser.PriceAtEnd` demanded **two or more spaces**, or a dot leader, between an item's name and its price:

```
^(?<name>.+?)(?:\s{2,}|\s+[.·•-]{2,}\s*)(?<price>\$?\d+(?:\.\d{1,2})?|MP)$
```

So these all failed to parse, and each is an ordinary way to write a menu line:

| Pasted line | Read as | Why |
| --- | --- | --- |
| `Garlic Bread 6.50` | nothing | one space between name and price |
| `Garlic Bread→6.50` (tab) | nothing | a tab is one whitespace character, not two spaces |
| `Garlic Bread 7` | nothing | one space |
| `Burger  12` | item | two spaces — the only shape that worked |

A tab is what a spreadsheet paste produces, and a single space is what a person types. The screen's own promise in `docs/features/menus/README.md` is **"no syntax to learn"**; the parser had a syntax, it was undocumented, and it was two spaces.

**Why the test suite did not catch it.** Every existing parser test wrote its fixture as `"Burger  12"` — two spaces. The suite passed while encoding the defect as the expectation. This is the failure worth remembering: tests written from the same assumption as the code confirm the assumption rather than the requirement.

##### The change

The separator becomes "one or more whitespace, with an optional dot leader":

```
^(?<name>.+?)(?:\s+[.·•-]{2,}\s*|\s+)(?<price>\$?\d+(?:\.\d{1,2})?|MP)$
```

The **number format is untouched**. Whole numbers, a leading currency symbol and `MP` already parsed correctly; they only ever failed for want of a second space.

##### The trade-off, accepted deliberately

A capitals-only heading that ends in a bare number — `SPECIALS 2` — now reads as an item priced at 2.

This is the same trade the parser already made, not a new one: `Parse_PricedUppercaseLineIsAnItemNotAHeading` asserts that `BLT  12` is an item, and `SPECIALS 2` cannot be told from `BLT 12` by shape alone. A guard against it was written and then removed, because it broke that existing, deliberate assertion. Review can promote any line to a section, so the case is recoverable by the person doing the import. The trade is recorded as its own test so the next reader meets it as a decision rather than a surprise.

##### The tasks, answered

| Task | What it means |
| --- | --- |
| T1 · Confirm the import route is wired | Done before any code changed. Client `api.ts` → `BackOfficeMenuImportsController` (`api/back-office/menu-imports`) → `MenuImportService` → `IMenuImportRepository`, registered at `src/Vennu.Data/Extensions/ServiceCollectionExtensions.cs:25`. Nothing was missing; the wiring was never the fault. |
| T2 · Widen the separator | One regex in `src/Vennu.Api/Menus/MenuPasteParser.cs`, with the reasoning kept next to it as a doc comment so it is not re-narrowed by someone tidying up. |
| T3 · Test the shapes a real menu uses | `Parse_ReadsAnItemWhateverSeparatesTheNameFromThePrice` — eight cases: single space, tab, whole number, currency symbol, `MP`, two spaces, and dot leaders. |
| T4 · Test a whole pasted menu | `Parse_ReadsAnOrdinaryPastedMenu` — asserts 4 items across 2 sections with no review questions raised. This is the test that would have failed before the fix; none of the old ones did. |
| T5 · Record the trade-off | `Parse_ReadsACapitalsLineEndingInAPriceAsAnItem` — states the `SPECIALS 2` behaviour as intended, with the reason. |
| T6 · Re-verify against the running API | **Not done in the fix PR.** The local dev stack could not be restarted: `start-ui-test-env.ps1 -Stop` reported PID 22416 could not be terminated and ports 5175/5177/5199 stayed held by orphaned processes. Disclosed in the PR rather than glossed. Needs a live paste after deploy. |

**Status:** 490/490 unit tests pass. PR #862.

#### Milestone 6-A5 — the import has a door

**Why this exists.** 6-A1 through 6-A4 built the paste import end to end and it is verified against deployed dev. **Nothing in the product navigates to it.** The route `#/menu/import` renders `MenuPasteImport` (`src/back-office/src/App.tsx:624`), but the only code that ever sets that hash is the redirect *inside* the flow itself (`App.tsx:631`). The only way a customer reaches four shipped milestones of work is to type a URL.

All three entry affordances the design specifies exist in `MenusHome.tsx`, and all three call `setNamingMenu(true)` — a dialog headed **"Start a blank menu"**:

| Design authority | Code | What it does today |
| --- | --- | --- |
| Empty shelf: three route cards + *"or start from a blank board"* — `README.md:158` | `MenusHome.tsx:261` | one **Add a menu** button → blank-name dialog |
| Add-a-menu tile, sub-copy *"Photo, paste, spreadsheet / or start blank"* — `README.md:118` | `MenusHome.tsx:357` | sub-copy reads "Paste it in, or start blank" → blank-name dialog |
| Header / at-scale **Add a menu** button — `README.md:150`, Q166 | `MenusHome.tsx:292` | blank-name dialog |

The comments in `MenusHome.tsx:41` and `:72` say the import routes "arrive in milestone 6". 6-A shipped without them; the interim was never replaced.

**Design authority.** Decision 17 (*getting a menu in is permanent — import lives on the Menus home forever, not in a wizard*), decision 30 (*import ends where it begins — all routes converge*), decision 4 as applied by `README.md:178` to POS (*a route that is not available leaves no trace*), `README.md:118/150/158/162`, Q166.

##### The behaviour, stated whole

A venue operator with a menu on paper wants it in Vennusign. They open **Menus**, choose to add one, choose **paste**, and land in the flow 6-A1 already built. Immediately before: the shelf, empty or populated. Immediately after: `#/menu/import`, which is unchanged by this milestone. The same "add a menu" behaviour lives in exactly three places, all in `MenusHome.tsx`; all three are in scope.

##### What ships, and what deliberately does not

Only **paste** and **blank** exist. Photo, spreadsheet and POS do not. `README.md:178` already settles how to treat a route that is not available — *"when it is not, there is no trace of it — decision 4."* So there are **no greyed-out "coming soon" cards**. The route set is data, not a hardcoded row of three, so photo/spreadsheet/POS append later without a redesign.

Paste inherits the lead treatment (2px `#87ceeb`, `#f2fbff`) that `README.md:158` gave photo. Photo was the lead because it is the easiest thing a restaurant already has; paste is now the only route that actually gets a menu in.

Blank stays an underlined text link below the cards, per `README.md:158` — not a peer card. Promoting it to a peer is how it became the only route.

##### The naming order changes

Today **Add a menu** demands a menu name before anything else. For the paste route that is backwards twice: the operator names a menu before it has content, and the import flow *already* proposes a name from the paste (`proposedMenuName`) and confirms it at the destination step (`MenuPasteImport.tsx:117`). They would be asked twice.

Naming stays where the import already puts it. The **blank** route keeps its name field, because there the name is the only thing that exists.

##### The tasks

| Task | What it means |
| --- | --- |
| T1 · One route chooser | A single component rendering routes from a list. Opened by all three affordances. Paste leads; blank is the underlined link. Cited authority for tokens: `src/back-office/src/sky-ui-tokens.css`. |
| T2 · The empty shelf uses it full-page | Decision 17 — onboarding is the empty state of this screen, not a wizard. No dialog, nothing to dismiss, nothing to fall out of. Same route list, same copy source as T1, so the two cannot drift. |
| T3 · Wire all three affordances | `MenusHome.tsx:261`, `:292`, `:357` stop calling `setNamingMenu(true)` directly and open the chooser. The blank-name dialog becomes a destination *inside* it. |
| T4 · Paste route goes straight to the flow | Sets `#/menu/import`. No name asked at the door. |
| T5 · Absent routes leave no trace | The list is data; unavailable routes are not rendered at all. Two cards centred must read as deliberate, not as a three-slot grid with a hole. |
| T6 · Playwright specs | Each of the three affordances reaches the paste screen; blank still creates a menu and opens the builder; empty shelf; the 900px floor; cancel and Escape; double-click; and the at-scale (≥7 menus) variant, which is a different affordance and a different code path. |
| T7 · Correct the records | `README.md:118`'s tile sub-copy and `README.md:162`'s route list currently describe routes that do not ship. Amend to state what ships, with the unbuilt routes named under *After this build*. |
| T8 · Screenshot A/B against master | All three entry points, both themes, 900px and 1920px. Per the standing bar for this area — a layout claim made from reasoning alone is not evidence. |

##### Paths, and what validates each

| Path | Validated by |
| --- | --- |
| Empty shelf → paste → import | T6 spec |
| Populated shelf tile → paste → import | T6 spec |
| Header button (≥7 menus) → paste → import | T6 spec |
| Any affordance → blank → named → builder | T6 spec (existing `menus-shelf.spec.ts` behaviour must survive) |
| Chooser cancelled / Escape | T6 spec |
| Double-click on a route | T6 spec |
| Below 900px | T6 spec — the import route already refuses below 900 with a return path; the chooser must not offer a door into a screen that will refuse. **Open: does the chooser hide paste below 900, or let the import screen state the refusal?** Recommended: let the import screen refuse, because it already does it well and hiding the route silently is the ghost-UI failure decision 4 exists to prevent. |
| Menus tier-gated off (decision 19) | **Unvalidated** — the Menu nav item does not render at all, so no affordance exists. Named here rather than left implied. |

**Scope.** Front-end only. No schema, no API, no parser change — the import backend is complete and verified on deployed dev at `339690fc`. `#/menu/import` remains a working deep link; import sessions stay resumable and shareable by URL.


#### Milestone 6-A6 — the shelf is a page about menus

**Why this exists.** Raised by the owner on 2026-08-26, reviewing M6.5 and M8: *"this screen is about showing menus, not screens… we need somewhere to locate the recently deleted, remove the put on shelf pills."*

Three things about the Menus home are decided here, and they are one change because they are all the same row of the page.

- **The "Not in use" strip is furniture.** A labelled row of pill chips, each with its own *put back* button (`MenusHome.tsx:378`–`:400`, `README.md:156`), pinned below the grid on a page whose subject is menus.
- **It contradicts the shelf at scale.** At seven menus or more the same set is already a filter chip, `not-in-use` (`menusShelf.mjs:63`). Below seven it is a strip. The product answers "where are my put-away menus" two different ways depending on how many menus a venue owns.
- **M8's recycle bin needs a home**, and it is the same kind of thing: a menu that is not on the shelf but is not gone.

##### What it ships

`shelfFilters` at every size, the pill strip removed, and **Recently deleted** added as a peer chip when M8 lands. The shelf becomes one grid plus one row of filters at any scale — the cutover at seven stays, but it changes only *how many cards are drawn*, never *where a thing lives*.

Two consequences that are not optional:

- **Put back must reappear on the card.** The strip is the only affordance for it today; removing the strip without moving the action deletes a customer capability.
- **The compact count is settled here too** (Q215) — twelve rather than six, so a venue at scale is not expanding the shelf on every visit.

##### Blocked on

**Q214** and **Q215**. Both have recommendations in the register; neither is settled. The *Recently deleted* chip additionally waits on **Q213** and M8 — it is added when there is something to put in it, not drawn empty.

##### Why it is not folded into M6.5

Same file, different subject. M6.5's subject is the import's missing entry point; this is the shelf's information architecture. AGENTS.md says keep changes bounded and do not begin future-milestone work, and an A/B screenshot pass that changes two unrelated things at once cannot attribute a regression to either.

##### The tasks

| Task | What it means |
| --- | --- |
| T1 · Settle Q214 and Q215 | Owner answers recorded before any code. |
| T2 · Filters at every size | `shelfFilters` rendered below seven menus as well as above; no chip appears for a state no menu is in. |
| T3 · Retire the strip | `MenusHome.tsx:378`–`:400` and its `menus-home__idle` styles; `README.md:156` amended in the same commit. |
| T4 · Put back moves to the card | Inside the `not-in-use` filtered view. The capability does not disappear with its strip. |
| T5 · Compact count | Per Q215. `MenusHome.tsx:142`. |
| T6 · Playwright specs | Below and above the cutover; each filter with zero, one and many matches; put back from the filtered view; the existing `not-in-use-chip` and `put-back` specs retired or rewritten in the same PR. |
| T7 · Screenshot A/B against master | Both themes, 900px and 1920px, at 3 menus and at 13. |


#### Milestone 6-A7 — the parser reads a real printed menu

**Why this exists.** M6.4 fixed the two-space separator and was verified — with a menu the agent wrote itself. On 2026-08-26 the owner pasted a real four-page restaurant menu out of its own PDF. The review screen said **"91 items need you"**.

Measured against the deployed dev parser at `339690fc`, the first half of that menu (69 lines) produced:

| | Before |
| --- | --- |
| Items | 19 — **three of them nonsense** |
| Sections | **0** |
| Unresolved | 48, every one `item_format_not_recognized` |
| Descriptions | all lost |

Decision 18 says *confirm only what we were unsure of*. Ninety-one questions is not a messy menu; it is the parser being unsure of almost everything, which turns the review screen — meant to be the exception list — into the whole menu, one line at a time. Nobody answers that. They retype the menu instead.

##### The four defects

| # | What | Why it happened |
| --- | --- | --- |
| 1 | **No sections at all.** `Appetizers`, `Salads`, `Soups`, `Noodles`, `Rice` all came back unresolved | A heading had to be ALL CAPS. Printed menus use Title Case. |
| 2 | **Every description lost** — 30-odd lines, unresolved | Q81 settled on 2026-08-07 that an unpriced line under an item is its description. It was never implemented. |
| 3 | **Price sets became items.** `Chicken $11.95, Beef $12.95, Shrimp $13.95` parsed as an item **named** "Chicken $11.95, Beef $12.95, Shrimp" priced **$13.95** | `PriceAtEnd` matched it, taking everything up to the last price as the name. |
| 4 | **The dishes under those headers vanished** — Pad Thai, Pad Se-Ew, Thai Fried Rice and six more | No price on their own line, so nothing matched. |

##### The change

The parser stops being a single pass. It now reads every line's **shape** first — blank, priced, comma-priced, caps heading, Title Case, prose, parenthesised note — and then walks the document deciding what each line **is**, with the shape of its neighbours available. A line's meaning depends on context: "Pad Thai" is a dish under a price set and a heading anywhere else, and a parser that never looks at line n+1 cannot tell those apart.

The rules, in the order they fire:

- **Title Case is a heading; sentence case is a description.** `Noodle Soups` against `Steamed healthy soybeans`. This one distinction does almost all the work, and it needs no length threshold and no comma counting. Words under three letters are skipped — `&`, `w.`, `of` — because title case does not capitalise them.
- **A heading needs something to hold.** A Title Case line with nothing after it stays a question rather than becoming an empty section: a stray line off the bottom of a PDF (a restaurant name, a tagline) looks exactly like a heading.
- **Prose under an item is that item's description**, joined across wrapped physical lines, attached to the item *and* recorded on its own line with a new `description` disposition — Q81's "never silently drop a line" holds.
- **A comma-separated run of priced fragments is never an item.** Followed by an unpriced dish name it is a **price set** (`price_set_needs_choosing`); otherwise it is several items on one line (`multiple_items_on_one_line`). Both raise exactly one question. A plausible-looking row of nonsense is worse than a question, because nothing about it asks to be checked.
- **A dish under a price set is an item with no price** — which A11 already allows. It deliberately does **not** take the first price: silently claiming Pad Thai is $11.95 when there are three prices puts a wrong number on a guest-facing board.
- **A parenthesised line is a note, never a dish**, and the look-ahead skips it — `(Served w. Steamed Jasmine Rice)` sits between a price set and the dishes it prices.

##### One rule deleted rather than duplicated

`MenuImportService` carried a byte-identical private copy of the parser's heading test, used to work out which sections were the operator's doing. It was about to disagree with the original, because Title Case headings are natural now and the copy did not know it. `MenuPasteParser.IsNaturalHeading` is public and called from both places.

##### Schema

Migration **076** adds `description` to `CK_MenuImportSourceLines_Disposition`. It discards nothing and changes no existing value's meaning. Menu creation and replacement filter on `Disposition=N'item'` and `N'section'`, so a description row is invisible to both by construction, while `ParsedDescription` — a column that already existed and was never populated — now reaches the built menu.

##### Not fixed, and named

**Several items on one line stay one question.** `Sides: Jasmine Rice $2.00, Brown Rice $3.00, Peanut Sauce $2.00` is three items, and splitting it is not possible here: a source line is one row, keyed `(SessionId, LineNumber)`. That is a schema change and its own milestone. Three such lines in the owner's menu, worth roughly fifteen items.

**The price set raises a generic "what should this line become?" question.** The review screen has no question kind for *choose which price applies*, so it borrows the unreadable one. The parser reason is honest (`price_set_needs_choosing`); the UI is not yet. A follow-up, not a blocker.

##### The fixture is the owner's own menu

Every parser test before this one was written from the same assumptions as the parser. That is exactly why the suite stayed green while the two-space defect shipped in M6.4, and why it stayed green again while a printed menu could not be read at all. `RealPrintedMenu` in `MenuPasteParserTests` is the real thing, pasted from the real PDF, and the assertions are counts a person can check by eye.


#### Milestone 6-A8 — the parser matches the menu

**Why this exists.** M6.7 was reported as a success on "91 questions became 15". That is a ratio between two wrong answers. Looking at what it actually produced:

- **17 of 47 items had no price** — Pad Thai, all four fried rices, all three curries, Tom Yum. A third of the menu, unusable on a screen. It had been justified as the safe choice; it was a hole with a rationale.
- **The restaurant's own name was a dish**, priced $11.95, and its tagline was a section.
- **`(Served w. Steamed Jasmine Rice)` asked ten times** — the owner counted them. One note, ten identical questions.
- **~15 items were missing**, excused in the release notes with `(SessionId, LineNumber)` — an internal key handed over as though it were a product limit.

The benchmark was available the whole time and was not used: the correct reading of the menu, produced by reading it. `RealPrintedMenuTests` now asserts against that, over the owner's entire paste, in numbers a person can check by eye off the printed page.

##### What changed

| | M6.7 | M6.8 |
| --- | --- | --- |
| Items | 47 | **46** |
| Items with no price | **17** | **0** |
| Sections | 15, four of them junk | **11**, all real |
| Questions | 15 | **5** |

- **A price set prices the dishes under it.** The dish takes the first price and carries the whole set in its description — `$11.95` with "Chicken $11.95, Beef $12.95, Shrimp $13.95" printed underneath, which is what the paper menu says. `MenuItems.Price` is one `DECIMAL(19,4)`, so three prices cannot all be the price. The set raises no question: it is stated on the dish.
- **A repeated note is never a repeated question.** Decision 33's rule for near-misses — one fact is one question, never thirty — applied to notes. `(Served w. …)` is kept as a note on the section it sits in and asked about not once.
- **Two Title Case lines in a row are neither a heading nor a dish.** That is what the restaurant's name and tagline are, straddling a page break inside a price set. They stay questions rather than being guessed at.
- **A price is a price wherever its parenthetical sits.** `Tea $2.00 *(Green, Jasmine, Black & Red)` is an item priced $2.00 whose parenthetical becomes its description.
- **A sentence addressed to the reader is a notice, not a menu line.** The allergy notice at the foot of the page.
- **A price set ends at a blank line.**

##### The one thing left, and it is a decision, not a rule

Three lines, worth roughly fifteen items:

```
Sides: Steamed Jasmine Rice $2.00, Brown Rice $3.00, Sticky Rice $2.00, …
Beverages: Thai Ice Tea $4.00, Coconut Juice $4.00, Soda $2.00, …
Desserts: Fried Banana $5.00, Mango Sticky Rice $6.00, Fried Ice Cream $6.00, …
```

Each is five or six real items on one physical line. `MenuImportSourceLines` is keyed `(SessionId, LineNumber)`, so **one pasted line can become at most one item.** No parser rule fixes that. The fork:

- **Widen the key** — migration adds `LineSubIndex`, one line yields many items, "jump to line 18" keeps meaning what it says. Touches the repository's insert/select, the question-line joins, and the create/replace SQL — all shipped and accepted milestones.
- **Redefine the number** — `LineNumber` becomes a row ordinal rather than a position in the paste. No migration, but line-number traceability back to the pasted text is gone, and the review screen's "Line 18" stops being true.

Owner decision. Recorded as **Q216**.

#### Milestone 6-A9 — one pasted line, several items (Q216)

**The answer, measured before it was chosen.** Both options were built against the owner's real menu rather than argued about, and they produce an identical result: **60 items, 0 unpriced, 14 sections, 2 questions** — the whole menu, Sides and Beverages and Desserts included. Everything that separated them was cost and consequence.

| | Widen the key | Row ordinal |
| --- | --- | --- |
| The menu | identical | identical |
| "Line 128" still means line 128 | **yes** | **no** — drifts after the first multi-item line |
| Production code | 3 files, 36 lines + migration 077 | 1 file, 22 lines |
| Test call sites | 6 | 0 |
| Verified against a real database | **12/12 integration tests** | n/a |
| API suite | **547/548** | 542/548 |

##### What only running it found

The first migration **failed and took 82 tests with it** — `MenuImportQuestionLines` carries a foreign key into the exact key being replaced, so the API could not start. That would have shipped on reasoning alone. It is dropped and rebuilt inside the same migration, and the question-line key gains its own sub-index: a question is raised about a *line*, never about one item inside a line, so it points at sub-index 0.

The row-ordinal alternative's cost is quieter than it sounds. On this menu the multi-item lines sit at the foot of the page, so the drift is small. Put a `Sides:` line near the top and every line number after it is wrong — including the "Line 18" the review screen prints beside each question, and the traceability Q81's never-drop-a-line invariant rests on.

##### What ships

`LineSubIndex` joins the source-line key. `LineNumber` keeps meaning a line of the pasted text. A labelled list becomes its label as a section and each fragment as an item, all sharing the line number they were pasted on.

##### Where the import stands

From 91 questions on the first real menu to **2**, and both of those are right to ask: the restaurant's own name and its tagline, off the top of a page. They are not menu content, and guessing at them is what put them in the menu the first time.

#### Milestone 6-A10 — the review screen speaks plainly

**Why this exists.** Owner feedback on the first real review screen, 2026-08-26: *"each question takes too much space… clarify exactly what Keep in Imported items does… the actions themselves should tell the story."*

Three faults, one screen.

- **Five blocks per decision.** A line number, a heading, the pasted line in a blockquote, a sentence of explanation, then the buttons — roughly 280px to make one small choice. Two questions read as roomy; fifteen was a page of scrolling.
- **The actions named mechanisms.** *"Keep in Imported items"* tells a first-time operator nothing about what it does, that it creates an item, or where that item goes. It broke **decision 10** — *"never a bare action: it states what replaces it, in the same click"* — on a screen that is nothing but actions.
- **There was no way out.** The design always specified three answers for an unreadable line — *"An item / A section / Leave it out"* (M1a, S6A-Q07) — and only two were built. The only way past a line the parser could not read was to import it and delete it afterwards.

##### What ships

**One decision, one row.** `question-row` at **110px**, measured, against roughly 280px before — three questions where one used to sit. The pasted text stays where the eye already is and the choices sit beside it, wrapping under it below the container width rather than shrinking, because a choice you cannot read is not a choice.

**Choices that name outcomes:**

| | |
| --- | --- |
| **A section heading** | Everything under it goes in this group |
| **A dish** | Goes in an Imported items group to sort later |
| **Leave it out** | Not imported. Your pasted text still has it |

*Imported items* survives as the place a dish lands, which is plumbing nobody needs told — but the copy says so anyway, because it is where the thing actually goes and decision 10 asks the action to say. The first draft of this copy said *"You'll add its price in the builder"*, which was true and incomplete; checking the create SQL showed a `fallback` answer is always placed in *Imported items* regardless of the headings above it, so the copy says that instead.

**`leave_out`** joins the answer vocabulary. Nothing is destroyed: menu creation pulls unresolved lines only where the answer is `fallback`, so a line answered `leave_out` is never placed while its text stays on the session — which is what Q81's *never silently drop a line* asks for.

**`Menus.Description`** lands in the data model — back-office only by owner decision, and deliberately not on a guest board. Nothing fills it until **M6.11**, which is where the read path and the builder field ship with it rather than as plumbing nobody can see.

##### One stale promise removed

When the review had nothing left to ask, the screen said *"This saved review can now be used to create or replace a menu **when those destination steps open**."* They opened in M6.2 and M6.3. It now says what to do next.

#### Milestone 6-A11 — what the rules cannot place, suggested rather than asked

**Why this exists.** After M6.8 and M6.9 a real four-page menu leaves exactly two lines the deterministic parser cannot classify: the restaurant's own name and its tagline, straddling a page break. No rule reaches them, because a heading and a restaurant's name are the same shape. Opus 5 read both correctly on the first attempt, said why, and cost **$0.009**.

**Tested before it was built into anything.** The call was made by hand against the real residue and its answer checked, rather than designed on the assumption it would work:

```json
{ "menuName": "Mana-Thai Cuisine",
  "menuDescription": "All Natural Authentic Thai Cuisine",
  "lines": [
    { "lineNumber": 68, "verdict": "menu_name", "confidence": "high",
      "why": "Restaurant's own name appearing as a header/branding break between sections, not a dish." },
    { "lineNumber": 72, "verdict": "menu_description", "confidence": "high",
      "why": "Strapline describing the cuisine style, directly following the restaurant name header." }]}
```

##### Why this is allowed, when the same model on the main path was not

Earlier in the same feature an AI parser was argued **against**, and that argument stands: on 91 unresolved lines it would have papered over a broken parser, and A18 would have reduced it to 91 suggestions instead of 91 questions. What changed is not the opinion but the situation. Two lines out of 133, after the rules have gone as far as rules go, is the *residue* — and a fallback on the residue was named as legitimate at the time and then not built.

**A18 is satisfied, not bypassed.** It forbids pre-answering unless a rule can name why, and a model names a reason rather than a rule. So the verdict is stored beside the question and applied only when the operator says so. The screen's job changes from *decide two things* to *check one thing*, and one click clears both.

##### What it may and may not do

- **It sees the residue and the two lines either side of it, and nothing more.** Those neighbours are the judgement — whether a line is a heading depends entirely on what follows it. The first test written for this asserted that no settled line is ever sent and **failed, correctly**; the service's comment was the thing that was wrong, and it was corrected rather than the test relaxed.
- **A verdict about a line nobody asked about is discarded on the way back in.** The schema constrains the reply's shape; it cannot constrain its line numbers. The guarantee is enforced at the boundary, not requested in the prompt.
- **More than twelve unplaced lines and it does not run at all.** Thirteen lines the rules could not read is a parser defect, and asking a model to paper over it is exactly how the 91-question import would have been declared solved.
- **It fails quietly.** No key, no network, a refusal, a timeout, a malformed body: the import is the import it would have been without it. A convenience on two lines may not break a paste that otherwise works. Seven tests cover those paths.
- **Off unless a key is configured**, so every environment without one behaves exactly as before.

##### The model, and why

**Opus 5, low effort, structured output.** The entire purpose of this call is the judgement a rule cannot make, so the tier that makes judgements well is the one to use; the volume — roughly 1,500 tokens in, 300 out — makes the cost argument moot at under a cent an import. Effort is low because it is a short classification, not because the answer matters less. Fable 5 was considered and rejected: thinking cannot be disabled on it, its turns run long by design, and an operator is watching this screen between pressing *Read menu* and the review opening.

##### On the screen

Each suggestion appears **on the row it is about** — *"We read this as the menu's name"*, with the model's own reason as the row's title — rather than in a banner detached from the thing it describes. Above them, one action applies both: *"Is this menu called Mana-Thai Cuisine?"* with **Yes, use these** and **No, I'll answer them** at equal weight, because a proposal you cannot easily decline is a decision somebody else made.

Accepting answers those lines `leave_out` — a restaurant's name is not a dish and not a heading — and carries the name and description to the destination step, where the operator confirms them again before anything is created.

**Shared display/accessibility scope:** the supported-width floor is 900px; below it, preserve the session and offer a resumable wider-window handoff rather than compressing the workflow. Keyboard-specific interaction design/testing is excluded; semantic controls, accessible names/relationships, visible focus, and screen-reader-compatible status/error announcements remain required.

#### Milestone 6-A12 — a price belongs to the menu it is printed on

**Why this exists.** Amendment **A19**, ruled by the owner on 2026-08-27, withdraws Q5. A dish may cost different amounts on different menus. `Items.Price` survives but is demoted: it is the default a dish carries when it is placed somewhere new, and it is never a fact one menu can change underneath another.

The evidence was a real menu. It carries the same dish in two sections, and it prices whole sections per protein — *Chicken $11.95, Beef $12.95, Shrimp $13.95*. The model could not hold that at all, so the import took the first price and printed the rest into the description. That is a workaround for a data model answering the wrong question.

**Decision 3 is untouched.** 86 stays item-level and venue-wide. Availability is a fact about tonight; price is a fact about a menu.

##### What is already true, and why this is smaller than it looks

`Placements.ImportedPriceOverride` exists (migration 069) and **every read path already coalesces it over `Items.Price`** — the board read, the builder read, duplicate-page, create-from-import and replace-from-import all do. `UQ_Placements_SectionItem UNIQUE (MenuSectionId, ItemId)` has been in the baseline since the beginning, so the storage can already express a per-section price.

What is missing is that the column is only *sometimes* filled, and one writer addresses the wrong row.

##### The defect this milestone actually fixes

`UpdateItemValuesGuardedSql` joins the placement by **menu**, not by section:

```sql
LEFT JOIN dbo.Placements p ON p.ItemId=i.Id AND p.VenueId=i.VenueId AND p.MenuId=@MenuId
```

**Corrected while building.** The first version of this plan said "a dish in two sections of one menu", and that is not reachable — `UQ_Placements_MenuItem` would have forbidden it. Migration **062** replaced that constraint with `UQ_Placements_PageItem (PageId, ItemId)`: once per **page**. So a dish may sit on two *pages* of one menu, and a four-page printed menu that repeats a dish is exactly that shape.

With two placements on one menu, the join matches both. `@Price` takes whichever row the engine returned, the guard compares the operator's expectation against a price that may belong to the other page, and the `UPDATE` — also keyed by menu — writes **both** placements to the same value. A price change on the lunch page silently rewrites dinner.

Demonstrated before it was fixed: `GuardedItemEdit_PricesOnePlacementWithoutTouchingTheOtherPage` fails against the old write with `Assert.Null() Failure: Value is not null` — the lunch placement holding a price only dinner was given.

The edit is now addressed by **section**, which is a unique address for an item because a section belongs to exactly one page.

##### The three pieces

**1 · Always set the placement price, including new items.** Today a placement gets an override only when an import saw a price that differed from the library's. Every other placement stores `NULL` and leans on `Items.Price` — so editing that dish's price in the library, from any other menu, changes this menu without warning. Under A19 every placement carries its own price, set from the library default at the moment it is placed. `COALESCE` stays as the read for rows created before this lands; it stops being the mechanism.

**2 · The builder edits the placement price, not the library price.** Key the read, the guard and the write by `(MenuSectionId, ItemId)`. Name, description and listing stay on the item — they are facts about the dish. Price moves to the placement, unconditionally, and the `CASE WHEN @HasOverride` branch goes away with it.

**This overturns Q112**, which settled that a builder price edit writes through to the library when no override exists. Q112 was reasoned from Q5. With Q5 withdrawn its premise is gone, and it is recorded as overturned rather than quietly contradicted.

**3 · Replace confirm names what changed, not just counts.** Replacing a menu from an import can now change a price on this menu without touching the library, which makes *"12 items updated"* an unreadable summary of an unreviewable change. The confirm step names the dishes whose price moves and shows both numbers — decision 12: summarize the normal, name the exception.

##### Order, and why it is not the obvious one

**Readers and writers ship before the migration, not after.** A backfill that fills every `ImportedPriceOverride` while a writer still edits `Items.Price` through a menu produces exactly the divergence this milestone exists to prevent, and the window is however long the deploy takes. So:

1. Fix the section-keyed read/guard/write (piece 2) — correct against today's data, no migration needed.
2. Set the override on every new placement (piece 1) — correct against today's data.
3. *Then* backfill existing placements, once nothing can write the library price through a menu any more.
4. The confirm copy (piece 3) last, because it describes behaviour the first three establish.

Steps 1 and 2 are separately shippable and separately verifiable. Step 3 is a migration whose invariant — *no placement has a NULL override* — can only be asserted once it holds.

##### What must not regress

- **Q115 / Q190 — prices are stored exactly as typed.** `NVARCHAR(12)`, no parsing, no currency inference. A backfill copies the string; it does not normalise it.
- **86 still reaches every menu** (decision 3). Nothing here touches `Items.IsListed`.
- **Duplicate page, create-from-import and replace-from-import already carry the override.** Regression tests exist for the duplicate path (task #9); the other two need the same.
- **The library price is still the default**, so an item placed on a new menu shows the price the operator expects without being asked again.

### Milestone 8 — Delete a menu

**Why this is numbered 8 and not 7.1 (SOP step 2b).** M7's three pieces are `blocked`/`parked` by owner ruling — the mock-fidelity polish round is visual work the separate Foundry component system will settle, and its two full-page pieces await owner scoping. This milestone is behaviour, not polish, it shares no files with M7, and it unblocks a customer action that has been owner-approved and unbuilt since 2026-08-07. The reason is recorded here and cross-referenced from `menu-builder-v2/mock-fidelity-polish-plan.md`.

**Design authority.** Q79 in `open-questions.md:965`, answered by the owner on 2026-08-07 and overriding the recommendation:

> ADD DELETE this build. Spec confirmed 2026-08-07: "Delete forever" in the ⋯ only for menus on zero screens; hard confirmation naming the destroyed menu and history; shared library items survive.

Also governing: decision 4 (*locked by plan means invisible*), decision 5 (*blocked is not the same as absent*), decision 8 (*history is durable and attributable*), decision 42 (*retention is configuration and tier policy*), amendment A12 (*Delete is not used on the builder screen, because nothing there destroys an item* — this milestone is the one place in Menus where something is destroyed, which is why it earns the word).

##### What is missing

Nothing. Not the UI, not the API, not the repository method.

- The card ⋯ menu ships six items — `MenusHome.tsx:569`–`:609`: **Open · Quick update · — · Go back to… · Duplicate · Put away · — · Take off the screens**. There is no seventh.
- `BackOfficeMenusController` (`src/Vennu.Api/Controllers/BackOffice/BackOfficeMenusController.cs`) has `HttpGet`, `HttpPost`, `HttpPut("{menuId:guid}")` and the quick-availability `HttpPut`. There is no `HttpDelete`.
- There is no `DeleteMenuAsync` anywhere in `src/Vennu.Data` or `src/Vennu.Core.Models`.

**Put away** is currently the terminal state, which is exactly what Q79 asked about and the owner declined. A venue accumulates menus forever with no way to destroy one.

##### The behaviour, stated whole

An operator has a menu they will never use again — a duplicate made by mistake, a test menu, a seasonal menu whose history no longer matters. They take it off the screens (an existing action), then destroy it. Immediately before: the menu is on zero screens, on the shelf or in **Not in use**. Immediately after: it is gone from both, and the items it used still exist in the library and on every other menu that placed them.

The same "destroy a thing" behaviour lives elsewhere in Menus at a smaller scope — `deleteMenuPage` and `deleteMenuSection` (`src/back-office/src/api.ts:1132`, `:1185`), each of which takes a destination for the orphaned children rather than cascading blindly. **That is the established shape and this milestone follows it**, not a bare cascade.

##### The data problem

`dbo.Menus` is referenced by ten foreign keys across four migration files. Only two carry `ON DELETE CASCADE` (`001_baseline.sql`, `062_menu_pages.sql`); the rest — `MenuScreenAssignments`, `MenuPublishEvents`, `MenuPublishTargets`, `MenuHistoryEntries`, and the three import tables in `069`/`070` — do not. A naive `DELETE FROM dbo.Menus` fails on a constraint, and adding cascades everywhere would let an import session silently delete published history.

The delete is therefore **ordered and explicit**, in one transaction, in a new migration starting at **076**.

##### The three decisions, answered — and the one they raised

Q210, Q211 and Q212 were answered by the owner on 2026-08-26. A fourth, **Q213**, came out of that
conversation and now blocks the milestone in their place.

- **Q210 — eligibility.** A menu is deletable only when it is **on the shelf and on zero screens**. Not while it is on a screen, and **not while it is put away** — put away is parking ("put this menu away until next Christmas"), not a staging area for deletion. The *absent vs refusing* half resolves to **absent**: a Not-in-use chip has no ⋯ menu at all (`MenusHome.tsx:380`–`:392`), so refusing in the on-screen case while silently offering nothing in the put-away case would be two answers to one question.
- **Q211 — no typed confirmation.** The dialog names the menu, what is destroyed and what survives; the destructive button carries `#8a2929`.
- **Q212 — history dies with the menu.** `MenuHistoryEntries` and `MenuPublishEvents` are destroyed, not detached. Q213 moves *when*, not *whether*.
- **Q213 · BLOCKING — a recycle bin, purged after 30 days?** The owner's proposal. It makes "Delete forever" untrue as copy, moves Q212's destruction from delete to purge, and requires something that actually sweeps — nothing in the product does today, and the nearest precedent (import-session expiry, `MenuImportService.cs:40`) is swept opportunistically inside a request rather than by a timer. The real fork is whether the bin has a **surface**: a bin the operator can open needs a screen, a restore action, and a decision about where a restored menu lands. Recommended in the register: yes, visible, thirty days, restore to the shelf, named **Recently deleted** rather than *recycle bin* — and split M8.1 off for the bin's own surface if that is too much for one milestone.

**Storage, named honestly.** The owner raised storage optimization as the motivation for the bin. A menu's own rows are small; the bulk of this feature's storage is `MenuImportReplacementSnapshots`, which decision 42 says to preserve in full for every replacement. If bytes are the driver, that is where they are, and it is a different piece of work than this one.

##### The tasks

| Task | What it means |
| --- | --- |
| T1 · Settle Q213 | Q210–Q212 are answered and recorded. Q213 — the recycle bin, and whether it has a surface — decides T2, T3 and T7 and must land before any code. |
| T2 · Migration 076 | Ordered delete in one transaction. Names what it discards, per AGENTS.md. Decides nothing Q212 has not answered. |
| T3 · `DeleteMenuAsync` on the repository | Refuses when the menu has any row in `MenuScreenAssignments`. Refuses on a stale revision. Idempotent — a second delete of the same id is a success, not a 500. |
| T4 · Model invariants | Added to `tests/Vennu.Data.IntegrationTests/Fixtures/ModelInvariants.cs`: no `MenuSections`, `MenuItems`, `MenuPages`, `MenuScreenAssignments`, `MenuPublishEvents`, `MenuPublishTargets`, `MenuHistoryEntries` or import rows referencing an absent menu; and no `Items` row destroyed by a menu delete. Per AGENTS.md, every write path that could violate these is listed and covered. |
| T5 · `HttpDelete("{menuId:guid}")` | On `BackOfficeMenusController`. `409` with a named reason when the menu is on a screen. Entitlement and role checked. Asserted against a database, not a double — the refusal is enforced in SQL. |
| T6 · The seventh menu item | **Delete forever** in the card ⋯ menu, below the last divider with *Take off the screens*, danger-coloured. Visibility per Q210. Verbatim copy, added to `README.md`'s verbatim list. |
| T7 · The confirmation | Names the menu, states that its history goes with it, states that shared library items survive and stay on every other menu using them. Per Q211. |
| T8 · Playwright specs | Zero screens (deletes); on screens (per Q210); a put-away menu; double-click and repeat submission; refresh mid-dialog; cancel; permission denied; a second operator deleting the same menu first; and the shelf and **Not in use** strip both updating. |
| T9 · Screenshot A/B against master | Card menu, confirmation, and the shelf after. Both themes, 900px and 1920px. |

##### Paths, and what validates each

| Path | Validated by |
| --- | --- |
| Menu on zero screens → delete → shelf updates | T8 spec |
| Menu on ≥1 screen | T8 spec, shape decided by Q210 |
| Put-away menu → delete → **Not in use** strip updates | T8 spec |
| Confirmation cancelled / Escape | T8 spec |
| Double submit | T3 idempotency + T8 spec |
| Second operator deleted it first | T5 refusal + T8 spec |
| Permission denied / tier changed after creation | T5 + T8 spec |
| Menu with published history | T2/T4, shape decided by Q212 |
| Menu that is a group menu (multi-venue) | **Unvalidated.** Decision 34 says a venue cannot import over a head-office menu; whether it may delete one is not on record and multi-venue is not built. Named here rather than left implied; belongs to the multi-venue build. |
| Last menu deleted → shelf becomes empty | T8 spec — this lands the operator on the empty state that **6-A5** rebuilds. The two milestones meet here; 6-A5 ships first. |

**Scope.** Schema → API → UI → specs, the full vertical. `Duplicate`, `Put away` and `Take off the screens` are untouched.


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
