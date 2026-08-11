# Menu Builder — action inventory

Companion to `docs/design/approved/menus/menu-builder-v2/workflow-handoff.md`.
Where the handoff maps functions, this maps **branches**: every action on the
Menu Builder screen, its preconditions, each path it can split into, and the end
state of each path.

Status column:

- **V2** — specified in the V2 workflow handoff or the decisions register.
- **Decided** — settled with the owner during the 11 Aug 2026 session.
- **Parked** — deliberately out of scope for this milestone.

All branches are now decided; nothing in this document is awaiting an answer.

Drawn against `menus/Menu Builder Preview.dc.html`.

---

## The 86 timing rule — resolved

Switching Available off cancels the 86, **and the cancellation rides with the
hide**: both land together at the next publish. Until then the item stays on the
board with its Sold out label, because that is still true.

This matters because the two halves run on different clocks. Cancelling an 86 is
immediate (decision 3); hiding an item is a draft edit that waits for publish
(decision 2). Landing them separately would put the item back on sale between the
toggle and the publish — the operator having just said it was gone. Holding the 86
until the hide publishes closes that window.

**Amendment:** decision 3 says an 86 never queues. It still doesn't — but its
*cancellation* does, when the cancellation is a side effect of a queued edit. That
refinement needs recording.

---

## A · Global and menu-level

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| A1 | Leave for Menus home | `Menus` breadcrumb | — | Draft exists → autosaved, no prompt | Menus home, menu card shows draft | V2 |
| A2 | Rename menu | Pencil beside menu name | — | Enter/blur → saves · Esc → reverts · Empty → rejected, keeps previous name | Name updated in breadcrumb, card and history | V2 |
| A3 | Add content | `+ Add content` | — | New menu → Import landing · Existing menu → **add or replace** decision first (decision 32: replace wins outright, theme and live 86s survive) | Import landing, then review, then builder | V2 |
| A4 | Leave via nav rail | Any nav item | — | Draft autosaved | That area, draft intact | V2 |
| A5 | Menu duplicate / delete | — | — | **No home on this screen.** Removed with the top-bar overflow; waits for the Menus home revision | — | Parked |

## B · Page rail

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| B1 | Select page | Page tab | — | — | Sections, header, count, assignment, capacity and board all reload for that page | V2 |
| B2 | Add page | `+` | — | Named → created and selected · Abandoned blank → discarded, no page created | New empty page, inline naming active | V2 |
| B3 | Reorder pages | Drag tab | 2+ pages | Page is in a screen rotation → rotation sequence follows tab order | New order, rotation updated | V2 |
| B4 | Many pages | Tabs exceed width | — | Row scrolls horizontally; tabs never wrap | All tabs reachable | V2 |

## C · Page header

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| C1 | Open page actions | `⋯` beside page name | — | Rename · Duplicate · Delete | Menu open | V2 |
| C2 | Rename page | Page actions → Rename | — | Enter/blur saves · Esc reverts · Empty rejected | Tab and header updated, logged | V2 |
| C3 | Duplicate page | Page actions → Duplicate | — | Sections and items copied; **screen assignments are not** — the copy starts unassigned | New page beside the original, unassigned | Decided |
| C4 | Delete page | Page actions → Delete | Not the only page | Empty → confirm and delete · Holds sections → **offer to move them to another page** · Assigned to screens → confirmation names the screens that lose it · Only page in menu → blocked (a menu always keeps one page) | Page gone, sections moved or deleted, assignments removed | Decided |
| C5 | Change viewing scope | Chip: `Whole page` / section name | — | Whole page → all sections rendered together in order · Section → that section alone · More than five sections → extras **collapse behind `More ▾`**, the row never scrolls or wraps | Board redraws at the new scope | Decided |
| C6 | Quick assign | Assignment pill | — | Screen is free → assigned · Screen already has a page → **ask: rotate both, or replace** · Replace → confirmation names the displaced page | Draft assignment change, pending publish | Decided |
| C7 | Full assignment management | Pill → `Manage` | — | Save → returns with draft assignments · Cancel → restores the pre-entry snapshot | Screen Assignments view | V2 |

## D · Capacity

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| D1 | Recalculate | Any content, theme or assignment change | Page has ≥1 assigned screen | Fits → no banner · Nearly full → amber · Overflowing → amber, stronger wording | Banner state above the board | Decided |
| D2 | Live evaluation | Keystroke while typing an item | — | Recalculates as you type, not at publish | Banner updates mid-edit | Decided |
| D3 | Check fit | `Check fit` in the banner | Banner present | Results per assigned screen, naming affected content and corrections | Fit results | V2 |
| D4 | Publish while overflowing | Review & publish | Overflow unresolved | **Publishes and drops the excess, naming exactly which items** — never silent (V2 §7) | Screens show what fits; dropped items named at review and in history | Decided |
| D5 | Warning scope | — | — | Capacity is only ever reported against **assigned** screens | — | V2 |

## E · Board

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| E1 | Select item | Click row | — | — | Inspector loads that item | V2 |
| E2 | Edit on the board | Click a row | — | **Nothing is edited on the board.** Adding is the only inline act; every edit happens in the panel. Reverses Q118, which allowed in-place price editing | Inspector loads the item | Decided |
| E3 | Reorder or move item | Drag row | 2+ items on the page | Within its section → new order · **Across sections → the item moves to that section**, at the drop position · Dragging into an empty section is allowed | New order or new section | Decided |
| E4 | Remove item | Control on the selected row | — | Always confirms, naming the page · Last item in a section → section remains, empty · Selection moves to the next valid item · Same action as H7 | Item removed from the page, kept in the library | Decided |
| E5 | Add item | `+ Add an item` on the board, or the inspector button | A section is in scope | Row appears at the **end of that section**, caret in the name, Tab to price · Abandoned blank → silently discarded · Name near-matches the library → suggestion offered with the **existing item pre-selected**, since a tidied name is the common case (decision 33) · Unnamed at publish → listed at review to finish or drop, does not block the other changes · No price → legal (`MP` and blank are both real) | New draft item, inspector focused on it | Decided |
| E6 | 86 a brand-new item | 86 toggle on an unpublished item | — | Inert until the item has been published once, **with the reason shown** — not a bare disabled state | No 86 possible yet | Decided |

## F · Sections (inside the page panel)

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| F1 | Select section | Section row | — | — | Viewing scope and editing focus move there | V2 |
| F2 | Rename section | Pencil on the row | — | Enter/blur saves · Esc reverts · Empty rejected | Row, board heading and history updated | V2 |
| F3 | Reorder sections | Drag handle | 2+ sections | Within the current page only | New order, board redraws | V2 |
| F4 | Delete section | Trash on the row | — | Empty → confirm · Holds items → **offer "move these items to…" inside the confirmation** · Last section on the page → allowed; the page then draws nothing and is flagged at review | Section gone, items moved or deleted | Decided |
| F5 | Add section | `+ Add section` | A page is selected | Created inside the selected page, inline naming · Abandoned blank → discarded | New empty section | V2 |
| F6 | Empty section at publish | Review & publish | A section has no items | **Flagged at review** — "Bites is empty and won't appear" — publish is not blocked | Published; section draws nothing on the guest screen | Decided |

## G · History (page-scoped, inside the panel)

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| G1 | Read page history | — | — | Shows events for this page only, newest first | — | Decided |
| G2 | Menu-level facts | Foot of the list | — | `Menu history →` plus the published time; menu events never appear in the page list | Menu history view | Decided |
| G3 | Click an entry | Entry | — | **Read-only in M3** — nothing happens; no restore-from-entry, no diff view | — | Decided |

## H · Item inspector

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| H1 | Add item to section | `+ Add item to <section>` | — | Same as E5 | — | Decided |
| H2 | Close inspector | `✕` | — | Blank new item → discarded · Named item → kept | Nothing selected | Decided |
| H3 | More details | Tab | — | Image, dietary/allergen, modifiers, schedule, nutrition — each absent entirely when outside the plan (decision 4) | That pane | V2 |
| H4 | Edit name / description / price | Typing | — | Board updates live, draft autosaves · Price renders **exactly as typed** (Q115/Q190); `MP` never becomes `MP.00` | Draft change | V2 |
| H5 | Available off | Toggle | — | Cancels the 86, **and the cancellation waits for the same publish** · Item leaves the screen at that publish · Until then it stays on the board, still labelled Sold out | Draft change | Decided |
| H6 | 86 on | Toggle | Item published at least once | Immediate, never queued (decision 3) · Item stays on the board with the theme's Sold out label (V2 §8) · Message, time and author boxed under the switch · Reaches every screen showing the item, counted in the wording (Q180) | Live on the screens now | Decided |
| H7 | Remove item from the page | Inspector link, or the control on the board row | — | **One action, not two** — the board control and the inspector link do the same thing. Wording: the link reads *Remove from this page*, the board control is an icon with the same label, and the confirmation reads "Remove Classic Burger from Lunch Page? It stays in your item library, and on any other page using it." *Delete* is not used anywhere on this screen, because nothing here destroys an item outright | Draft change; item still in the library | Decided |

## I · Footer

| # | Action | Trigger | Preconditions | Branches | End state | Status |
|---|---|---|---|---|---|---|
| I1 | Read draft state | — | — | `Draft changes saved · not on your screens yet · published <time>` — no change count on the bar | — | Decided |
| I2 | Change theme | Theme control | — | Board rerenders, capacity re-evaluates, banner may appear | Draft change | V2 |
| I3 | Open the actions menu | `Finish ▾` | — | Review & publish · Save & exit · Discard · Restore an earlier version | Menu open | Decided |
| I4 | Review & publish | Menu → first item | — | Review lists pages, sections, assignments, rotation, theme, availability changes, capacity results, additions and removals · Unnamed items listed to finish or drop · Dropped-by-overflow items named · Publish now, schedule if supported, or return | Screens updated only on the explicit publish | V2 |
| I5 | Save & exit | Menu | — | Draft saved, nothing published | Menus home | V2 |
| I6 | Discard | Menu → Discard | Draft differs from published | **Confirms, naming every change** with before/after where a value moved · Live 86s are preserved — an 86 is a fact about tonight, not a draft edit | Page back to the published state | Decided |
| I7 | Restore an earlier version | Menu → Restore | A published version exists | **Confirms, then replaces the current draft** · The replaced draft is recoverable for the session — **one session only in this version**, not a stack · Restored state is a draft and still needs publishing · Wording reverses decision 11 and stays provisional | New draft from an earlier version | Decided (wording pending) |
| I8 | Offline screen at publish | Publish | A target screen is offline | **Publishes to the online screens and names the offline one**, which catches up on reconnect · Never blocks (decision 5: offline is a real state that must say what it is) | Published; offline screen named until it reconnects | Decided |

---

## Coverage

| Group | Actions | Branch paths |
|---|---|---|
| A · Global | 5 | 8 |
| B · Page rail | 4 | 7 |
| C · Page header | 7 | 15 |
| D · Capacity | 5 | 8 |
| E · Board | 6 | 14 |
| F · Sections | 6 | 11 |
| G · History | 3 | 3 |
| H · Inspector | 7 | 13 |
| I · Footer | 8 | 14 |
| **Total** | **51** | **93** |

Of the 51 actions: 24 rest on V2 or the decisions register, 26 were settled on
11 Aug 2026, and 1 is parked until Menus home is revised. **Nothing is unanswered.**

## Amendments — recorded in the register

All sixteen are written into `docs/design/approved/menus/decisions.md` as **A1–A16**,
which is authoritative. The eleven below are the subset that governs this screen,
kept here so a branch and its amendment can be read together.

| # | Amends | Now reads |
|---|---|---|
| 1 | V2 §4 | Sections live inside the page panel, not the outer left rail. Intent unchanged: scoped to the selected page, not a second page navigator |
| 2 | Decision 8 | Edit history is page-scoped inside the tab; menu-level facts sit at its foot behind *Menu history →* |
| 3 | Decision 3, V2 §8 | *86* is the staff word for the control, message and history; the guest board keeps *Sold out*, drawn by the theme |
| 4 | Decision 3 | An 86 still never queues — but its *cancellation* does, when it is a side effect of a queued edit (Available off) |
| 5 | V2 §9 | Publish, exit, discard and restore sit behind one *Finish* menu; the change count is off the bar and lives in the menu |
| 6 | Decision 11 | *Restore an earlier version* replaces *Go back to…*, and **"restore" leaves the banned-words list**. Decision 11's objection was that the word reads as version control; with restore-from-history deferred past M3 and the Finish menu action producing an ordinary draft, the word is now accurate. The `tests/ui` banned-words array drops it; "unpublish", "supersede" and "archive" stay |
| 7 | Q118 | Nothing is edited on the board. Adding is the only inline act; edits happen in the panel |
| 8 | V2 §5 | Viewing is chips — *Whole page* plus each section — collapsing behind *More ▾* past five |
| 9 | V2 §7 | Items drag across sections, not only within one |
| 10 | V2 §8 | Six add-item rules: end of section, name required, price optional, abandon-blank discards, unnamed listed at review, 86 inert until published once |
| 11 | V2 §7/§8 | One removal action, named *Remove from this page*; *Delete* is not used on this screen |

Unchanged and worth stating: prices render exactly as typed (Q115/Q190) — the
formatting question was raised and closed without amendment.

## Connected screens — decisions of 11 Aug 2026, round 3

These cover the seven screens in the connected-screens design file. **All twelve are
now drawn**, including the six states added after this table was first written.

| # | Question | Decision | Drawn |
|---|---|---|---|
| 1 | A photo we cannot read | One message — "we couldn't read this" — offering a retake and the other three routes. No stage-level diagnosis | **Yes** (F) |
| 2 | A file we cannot open | A specific message per cause, each naming the fix: not a spreadsheet, password-protected, no Item column, empty, too large | **Yes** (F) |
| 3 | A publish that half-succeeds | Retry silently in the background; tell the operator only if it keeps failing | **Yes** (F) |
| 4 | After publishing | Stay in the builder, now clean, with a confirmation line | **Yes** (F) |
| 5 | Empty and first-run states | Each empty state points at the one action that fills it | **Yes** (F) |
| 6 | Tier-absent versions | Draw the Free-tier variant of each screen, controls simply absent (decision 4) | **Yes** (G) |
| 7 | Naming an imported page | A name field on the review screen, pre-filled from the heading we read | **Yes** |
| 8 | Replace-mode review | Review names what goes as well as what arrives, side by side | **Yes** (G) |
| 9 | Venue replacing a group menu | Out of scope for this milestone (decision 34 still stands) | n/a |
| 10 | Rotation interval | **The theme owns it.** Menus sets page order only; the interval is displayed read-only | **Yes** |
| 11 | History on a busy venue | Group same-kind events — "12 items 86'd on Tuesday" — expandable | **Yes** (G) |
| 12 | Retention wording | Configured per tier; the screen states the configured depth at the boundary and never hardcodes a number | **Yes** (E) |

### Round 4 — thresholds and limits, 11 Aug 2026

| Question | Decision |
|---|---|
| Which tier the reduced version is | Free **does** have the Menu Builder, with the tiered extras absent (decision 4). Decision 19's no-menu case remains true only for a one-screen static plan, which is a different product shape, not a Free variant of this screen |
| File size and formats | **Tiered setting**, starting at 5 MB. Formats as drawn (.xlsx, .xls, .csv); Google Sheets and Numbers are export-to-xlsx and that must be written on the upload screen, not discovered |
| When a failing publish stops being silent | After about **30 seconds** of failed retries — and the threshold is **configurable in venue settings**, not hardcoded |
| History grouping | Groups when there are **more than five** events of a kind in a day; five or fewer stay as individual rows |
| Retention depth | Read from config at runtime. The screen states whatever the plan keeps and never hardcodes a number |
| Confirmation after publish | **Names the screens and stays until the next edit**, then the change count replaces it. Not timed, so the evidence cannot be missed by looking away |

### Consequences worth carrying into the build

- **Rotation leaves Menus entirely.** No interval control on any Menus surface; Screen Assignments reads it from the theme. This also resolves two menus sharing one screen — neither owns timing.
- **A partial publish is not a user-facing state until it persists.** The retry policy needs a threshold and a place to surface — probably Q197's slot in the builder footer.
- **Free-tier variants are real layouts, not disabled ones.** Every screen needs its second drawing before an agent builds either.
- **All twelve are drawn.** Sections A–G of the connected-screens file cover them, including the five import failures, the after-publish confirmation, the empty states, the Free-tier variant, replace mode and history at volume.

## Surfaces this screen depends on

Import landing and review, Screen Assignments, publishing review and menu history
are all drawn — sections A–E of the connected-screens file. **One is not:** the
Menus home menu card, which is where menu-level duplicate and delete have to live
(A5). It waits for the Menus home revision, along with the stale and offline cards,
the publish/schedule gate, the Free-tier go-live screen and the Screens handoff.
