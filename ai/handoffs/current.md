Warning: truncated output (original token count: 51321)
Total output lines: 2230

# Vennusign Session Handoff

Updated 2026-09-09, after the Theme Studio working conversation was reconciled with the Content Platform architecture and preserved as a dated engineering guide.

## 2026-09-09 — Theme Studio conversation and wireframes preserved; still not implementation authority

Issue #974 records the documentation task. The new
`docs/features/theme-studio/theme-studio-engineering-guide-2026-09-09.md` contains the architecture
reconciliation, 12 invariants, accepted interaction sequence, 32-decision ledger, all 67 available
owner messages, a status register for all 19 generated wireframes, the complete visual appendix,
18 missing screens, and ten open owner decisions. The historical working bundle remains under
`docs/design/theme-studio/`; its README points to the new reconciliation.

What is established: Theme Studio binds one explicit immutable data-model version and stable model
paths; Content Builder owns records and their library/provider sources; Repeater is the neutral
visual component; the first compatible field derives its repeating record type; `state` is a normal
model field type; State appearance belongs in Style; operational state cannot change record geometry
or repaginate a wall; released Theme revisions are immutable; and all render consumers share the same
semantics. The accepted guided drag-and-drop sequence is preserved.

What is deliberately left for later: the guide is a working draft, not approved design authority and
not permission to implement. The exact Rules actions, State appearance controls, `When empty`
ownership, stored contract names, presets, output modes, and non-Repeater component panels remain
open. The exact assistant prose from the original chat was unavailable; the guide says so rather than
inventing it, and preserves the decisions plus every available visual response.

**One exact next action:** resolve the ten open owner decisions in a Theme Studio question register,
then draw the corrected panel-only inspector summary before any clickable prototype or implementation.

## 2026-08-29 — The Wall Planner: designed and approved, deliberately not built yet

**Landed in `docs/features/screens/wall-planner-design.md`** (branch `design/wall-planner`, docs only).
Twelve owner decisions (WP-1 … WP-12), the contract, the dealing algorithm with a reference
implementation and worked examples, the shared-clock rule, always-on-screen rules, delivery states,
what it needs from the rest of the platform, how it is tested, and what happens to today's code.

### Why this exists

After #958–#961 the owner said the display work had become "endless attempts of possible looks and
feels", and named the real case: Apps page 1 on screen 1, page 2 on screen 2, page 3 on screen 3 —
and if there is no screen 3, page 3 rotates back onto screen 1. Six readers mapped what exists
today, and the honest picture is: the packing is C# in a controller (item counts against a
hardcoded table, photo grid only), the TV separately shrinks pages in JS, the grid shape is written
a third time in CSS, every TV runs its own clock, and nothing in the data can say "screen 2 shows
page 2" — today's `WallGroup` means "split one page's *items* across screens by count". There is
also a latent arithmetic bug in wall + sub-paging (recorded in the spec §11, untested per Done
Records #960/#961).

### The three things that changed the design mid-conversation

1. **The architecture reset** — `docs/architecture/content-platform-architecture-renewal.md`,
   issue #939, owner-approved 2026-08-28, still on branch
   `feature/content-platform-m0-architecture-renewal`. Display moves to a *render package* from an
   immutable Content Release; display work resumes at renewal **M4**; "stop extending menu-only
   rendering".
2. **Theme Studio** (branch `agent/theme-studio-handoff`): the theme is a JSON *Canvas Render
   Definition* with capacity per region and ordered overflow rules, and **one shared TypeScript
   renderer** used everywhere. So "how many fit" is not the planner's job.
3. Therefore the planner became **small and pure**: pages in → per-screen schedule out. TypeScript,
   `packages/wall-planner`, beside the renderer. Built with the M4 renderer work, wired into
   delivery at the renewal's screens / player-packaging milestone (M5). **WP-5: no planner code against
   `DisplayController` — it would be retired with it.** The owner's words: "no sense working on code
   we are going to retire."

### Decisions the owner made, in order

Group is operator-made and ordered (WP-1) · structure > same text size > avoid rotation > balance
(WP-2) · only the spill rotates, the rest holds (WP-3) · same item count per screen, bigger TV =
bigger text, no correction (WP-4) · build at M4, not now (WP-5) · hold rule (WP-6) · filler/mirror
for spare first-slot screens (WP-7, owner judgment call, revisit on the first real wall) · one canvas
format per group (WP-8) · never show a half-downloaded package (WP-9) · a wall switches together,
with a grace period for offline screens (WP-10) · the last good package never expires (WP-11) ·
every delivery step timestamped and reportable (WP-12).

### Knock-ons recorded, not acted on

- **Menus M10 T2 re-pointed** (milestone-plan.md): the virtual screen becomes the renewal's fixture
  lab over the real renderer + planner, not a harness over `ComputeFrameStarts`. T1 unchanged; T3
  on hold.
- **Defect against WP-11:** `src/display/src/displayCache.mjs` throws the cached content away after
  7 days (`displayContentCacheMaxAgeMs`). Small "stabilize now" fix, separate PR — not started.
- **Constraint handed to Theme Studio (TS-C1):** a state response (86 / sold out) may never change
  how much space an item takes, or live state would re-paginate a wall.
- **Inputs to renewal planning session 4 and M2:** stable page ids + `continues` flag + section ids
  per page; dwell on the release; group = id + canvas format + ordered screen ids; package identity
  includes `plannerVersion`; delivery states.
- `docs/features/screens/workstream.json` now lists the design; `stage` stays `designing`.

### Open questions (spec §12, all with defaults)

Q-WP-1 filler vs background · Q-WP-2 grace defaults (proposed 120 s offline / 900 s ceiling) ·
Q-WP-3 per-page dwell · Q-WP-4 mixed canvas formats in one group.

### Review pass

A four-lens review (algorithm, consistency, fit with the renewal + Theme Studio, claims about
current code) raised 46 findings; 22 confirmed by refuters, 12 refuted, 12 unverified because the
subagent session limit was hit — those 12 were checked by hand against the repo and all held.
All 34 folded in. The ones worth knowing: the start time had been placed *inside* the
checksummed package but is set afterwards (now a separate `{ packageId, startAt }` message);
`WallPosition` is 1-based with gaps, so the planner sorts and uses rank; package identity needed
the group revision; `dbo.ScreenContentDeliveries` already exists and is now the seed for §8;
`SyncTick` exists server-side but is never emitted; the renewal puts screens/player packaging at
M5, not M4.

### One exact next action

Merge `design/wall-planner` (docs only). Then, when the renewal branch merges, add the pointer from
`content-platform-architecture-renewal.md` §10 (Screens / display delivery row) to the spec — that
file does not exist on master yet, so it could not be edited here.

## 2026-08-26 — M6.8: the benchmark was wrong, so the result was wrong

**Merged as `28c6909f` (#882, closes #881) and deployed to dev.** Verified by posting the owner's real menu at
the deployed API: **46 items, 0 unpriced, 11 sections, 5 questions.**

### The mistake worth carrying

M6.7 was reported as a success on **"91 questions became 15"**. The owner rejected it and asked whether that
was reasoning I would accept. It was not.

91 → 15 is a ratio between two wrong answers. What M6.7 actually produced, unlooked-at until the owner pushed
back: **17 of 47 items with no price** — a third of the menu, unusable on a screen — the **restaurant's own
name imported as a dish priced $11.95**, its tagline as a section, `(Served w. Steamed Jasmine Rice)` asked
**ten times**, and ~15 items missing, excused in the release notes with `(SessionId, LineNumber)` — an
internal key handed to the owner as though it were a product limit.

**The correct benchmark existed the whole time and was not used.** Earlier the same session this agent read
the menu and listed the appetizers correctly, with descriptions, in one shot. That reading was the right thing
to measure against. Instead the comparison was against the previous parser, which made a broken result look
like a triumph and hid every defect above.

Two habits this should change. **Measure against correct, never against the previous attempt** — an
improvement ratio says nothing about whether the output is usable. And **look at the rows**, not the counts: 47
items reads like success until you list them and find seventeen with no price and one called Mana-Thai Cuisine.

`RealPrintedMenuTests` now asserts the owner's entire paste against a correct reading, in numbers countable by
eye off the printed page.

### What changed

| | M6.7 | M6.8 |
| --- | --- | --- |
| Items | 47 | **46** |
| With no price | **17** | **0** |
| Sections | 15, four junk | **11**, all real |
| Questions | 15 | **5** |

- **A price set prices the dishes under it.** The dish takes the first price and carries the whole set in its
  description. `MenuItems.Price` is one `DECIMAL(19,4)`; three prices cannot all be the price. No question —
  the set is stated on the dish.
- **A repeated note is never a repeated question.** Decision 33's rule, applied to notes.
- **Two Title Case lines in a row** are neither heading nor dish — a heading is followed by something priced,
  a dish under a price set by its description.
- **A price is a price wherever its parenthetical sits**; a sentence addressed to the reader is a notice; a
  price set ends at a blank line.

### The one thing left is a decision, not a rule — Q216

Three lines hold five or six items each (Sides, Beverages, Desserts — about fifteen items and three of the
five remaining questions). `MenuImportSourceLines` is keyed `(SessionId, LineNumber)`, so **one pasted line
becomes at most one item.** No parser rule reaches it.

- **Widen the key** — a migration adds `LineSubIndex`; line numbers keep meaning what they say, which *Jump to
  line 18* and Q81's never-drop-a-line invariant both rest on. Touches the repository's insert, its `FOR JSON`
  read, the question-line join, and the shipped create/replace transaction.
- **Redefine the number** — `LineNumber` becomes a row ordinal. No migration, and traceability back to the
  pasted text quietly stops being true.

Recommended: widen the key. **Owner decision, not started.**

The other two remaining questions are the restaurant's name and its tagline. Those are arguably right to ask
about: they are not menu content, and guessing is what put them in the menu in the first place.

### One exact next action

Answer **Q216**. Everything else on this import is done and on dev.

## 2026-08-26 — M6.7: a real printed menu asked 91 questions, and now asks 15

**Merged as `ad4ce3ed` (#878, closes #877) with a follow-up at `6a9b9c18` (#879), both deployed to dev.**
`/health/version` reports `6a9b9c18` and `databaseSchemaVersion: 076_menu_import_description_lines`.

### What happened

M6.4 fixed the two-space separator and was verified — with a menu the agent wrote itself. The owner then
pasted a real four-page restaurant menu out of its own PDF. The review screen said **"91 items need you"**.

**This is the same failure shape as M6.4, one level up.** M6.4's lesson was that tests written from the code's
own assumptions confirm the assumptions. M6.4's *fix* was then verified with a fixture written from the fixed
code's assumptions. The suite was green, the deployed check passed, and a real menu was still unreadable. The
only thing that found it was a real menu.

### Measured, not estimated

Both numbers come from POSTing the owner's actual paste at the deployed dev API.

| | Before (`339690fc`) | After (`6a9b9c18`) |
| --- | --- | --- |
| Items | 19, three of them nonsense | **47** |
| Sections | **0** | **11**, all real |
| Descriptions | all lost | **55** |
| Questions | **91** | **15** |

### The four defects

- **No sections.** A heading had to be ALL CAPS. Printed menus use Title Case.
- **Every description lost.** Q81 settled this on 2026-08-07; it was never implemented.
- **Price sets became items.** `Chicken $11.95, Beef $12.95, Shrimp $13.95` parsed as an item *named*
  "Chicken $11.95, Beef $12.95, Shrimp" priced $13.95 — `PriceAtEnd` matched it and took everything up to the
  last price as the name.
- **The dishes under those headers vanished**, having no price of their own.

### The change, in one sentence

The parser reads every line's **shape** first, then walks the document deciding what each line **is** with its
neighbours available — because "Pad Thai" is a dish under a price set and a heading anywhere else, and a
single pass never looks at line n+1.

**Title Case against sentence case does almost all the work.** "Noodle Soups" versus "Steamed healthy
soybeans". No length threshold, no comma counting.

Deliberate calls worth knowing before touching this:

- A dish under a price set is an item with **no price** (A11 allows it). It does *not* take the first price —
  silently claiming Pad Thai is $11.95 when there are three prices puts a wrong number on a guest-facing board.
- A Title Case line with **nothing after it** stays a question. A stray line off the bottom of a PDF — a
  restaurant name, a tagline — looks exactly like a heading.
- A heading never carries a price, never labels itself with a colon, and never begins mid-sentence. All three
  exclusions came from the real-menu run (#879), not from reasoning: `Tea $2.00 *(Green, Jasmine, Black &
  Red)`, `Choice of Sauce: …`, and `& Red Curry Pineapple` had each produced a section.
- `MenuImportService`'s byte-identical private copy of the heading rule is **deleted**, not updated. It was
  about to disagree with the original.

### Schema

Migration **076** adds `description` to `CK_MenuImportSourceLines_Disposition`. Discards nothing. Create and
replace filter on `Disposition=N'item'`/`N'section'`, so description rows are invisible to both by
construction — while `ParsedDescription`, a column that existed since 068 and was never populated, now reaches
the built menu.

### Named and not fixed

- **Several items on one line stays one question** (`multiple_items_on_one_line`). A source line is one row
  keyed `(SessionId, LineNumber)`; splitting is a schema change and its own milestone. Three such lines in the
  owner's menu, worth roughly fifteen items.
- **The price set borrows the generic "what should this line become?" question.** The review screen has no
  kind for *choose which price applies*. The parser reason (`price_set_needs_choosing`) is honest; the UI is
  not yet.

### Also this session

**M6.5 (`d9ac7248`, #874)** gave the paste import a door: every "Add a menu" affordance opened a blank-name
prompt and nothing in the product reached the import, which had shipped and been verified for four milestones.
The name prompt is gone entirely — the builder names a blank menu inline.

**#876 filed:** `PairingFlow_CanBeDrivenThroughHttpApi` asserts the default layout and gets `photo_grid`.
**Pre-existing** — verified failing identically on a clean `origin/master` worktree. 538/539 otherwise pass.

**#866 must not be merged.** Its cert-trust step hangs for 30 minutes on a headless Windows runner instead of
trusting anything.

### One exact next action

The owner re-pastes the real menu at `dev.back-office.vennusign.com` on 2026-08-27 and reports what the review
screen says. Expect ~15 questions, 47 items, 11 sections.

## 2026-08-26 — M6.5 shipped: the paste import has a door, and no menu is named before it exists

**Merged as `d9ac7248` (#874, closes #867) and deployed to dev** — `dev.back-office.vennusign.com/version.json`
reports that commit. Back-office only; api, display, www and platform-operations all classified false and skipped.

### What was wrong

M6.1–M6.4 built the paste import end to end and M6.4 closed against deployed dev. **Nothing in the product
navigated to it.** `#/menu/import` rendered `MenuPasteImport` (`App.tsx:624`), and the only code that ever set
that hash was the redirect *inside* the flow (`App.tsx:631`). All three "Add a menu" affordances — empty shelf,
dashed tile, header button at scale — called `setNamingMenu(true)`, a dialog headed "Start a blank menu".

**Four milestones passed their own acceptance workbooks while the feature was unreachable, because every
workbook started from inside the flow.** That is the lesson worth carrying: a workbook that begins at step 2
cannot discover that step 1 does not exist. The M6.5 spec asserts each of the three affordances *separately*
rather than through one helper, for the same reason.

### What shipped

`MenuAddRoutes.tsx` — one component, two placements: full-page on an empty shelf (decision 17, nothing to
dismiss) and inside a dialog behind the tile and the header button. One route list, one copy source.

**Only built routes are drawn.** `README.md`'s M1a already settled this for POS — "when it is not, there is no
trace of it — decision 4" — so no disabled cards and no "coming soon". The grid is `auto-fit`, so one card
centres instead of leaving two holes in a row of three. Owner, 2026-08-26: *"just include on the new menu
button, about pasting, we can add the others when they get built."*

### Two owner decisions

- **No menu is named before it exists.** The prompt is gone. Blank creates the menu and the builder names it
  inline. `dbo.Menus.Name` is `NOT NULL` with `CK_Menus_Name_NotBlank`, so a nameless menu is impossible and an
  empty one could otherwise reach a guest-facing board — the menu carries `unnamedMenuName` and the crumb draws
  it muted with `data-unnamed`, reading as *not named yet*. **Deliberately not auto-focused**: a blank menu
  already puts the caret in the add-item row, and two things competing for focus on first paint is worse than
  one thing to click. Flagged to the owner as a judgement call, not settled by them.
- **The import keeps its own name confirmation** — it proposes a name from the paste and confirms it at the
  destination step, which is a suggestion once there is content rather than a demand before there is any.

### Verified, and not

Back-office suite **219/219** (two rewritten, two added); Release build clean; `tsc --noEmit` clean; the
component screenshotted at 1440 light, 1440 dark, 900px and as the dialog, with no horizontal overflow at any.

**UNTESTED: the Playwright suite has not run.** Local env is the one M6.4's T6 could not restart, and the CI
gate is red for an unrelated reason — see below. Five legacy specs were rewritten blind against the retired
prompt; they parse and list, but have not executed.

**Independent review was waived by the owner** for this PR, as it was for M3-A Slice 3-A. Not a standing
exception.

### Still open

- **#866, the CI cert fix, is not merged and should not be.** The "Trust the ASP.NET Core development
  certificate" step **hung for 30 minutes** and the job was killed by timeout — it never reached the tests. On
  `windows-latest`, `dotnet dev-certs https --trust` raises a certificate-install confirmation dialog and
  nothing answers it on a headless runner. The fix is to export the certificate and import it with PowerShell
  into the store directly (the runner is already elevated), never calling `--trust` interactively.
- **M6.5 needs a Playwright run and the owner acceptance workbook** before it can be called complete.
- **Q213 blocks M8 (#868)**; **Q214–Q215 block M6.6 (#871)**.

## 2026-08-26 — Two milestones planned: the import has no door, and no menu can be deleted

Planning only. **No product code changed.** Two GitHub issues, two plan sections, three register questions,
and the tracker, status and workstream records brought in line.

### M6.5 — the import has a door (#867)

M6.1 through M6.4 built the paste import end to end, and M6.4 closed against deployed dev. **Nothing in the
product navigates to it.** `#/menu/import` renders `MenuPasteImport` at `src/back-office/src/App.tsx:624`, and
the only code that ever sets that hash is the redirect *inside* the flow at `App.tsx:631`. A customer reaches
four shipped milestones of work by typing a URL.

All three entry affordances the design specifies exist, and all three call `setNamingMenu(true)` — a dialog
headed **"Start a blank menu"**:

| Design | Code | Today |
| --- | --- | --- |
| Empty shelf: three route cards + "or start from a blank board" (`docs/features/menus/README.md:158`) | `MenusHome.tsx:261` | one **Add a menu** button → blank-name dialog |
| Add-a-menu tile, "Photo, paste, spreadsheet / or start blank" (`README.md:118`) | `MenusHome.tsx:357` | copy reads "Paste it in, or start blank" → blank-name dialog |
| Header / at-scale **Add a menu** (`README.md:150`, Q166) | `MenusHome.tsx:292` | blank-name dialog |

`MenusHome.tsx:41` and `:72` both say the import routes "arrive in milestone 6". 6-A shipped without them and
the interim was never replaced. This is the shape worth remembering: **four milestones can pass their own
acceptance while the feature stays unreachable**, because every workbook started from inside the flow.

Two design calls made in the plan and worth knowing before implementing:

- **Photo, spreadsheet and POS leave no trace.** Not greyed-out "coming soon" cards. `README.md:178` already
  settles this for POS — *"when it is not, there is no trace of it — decision 4."* The route set is data, so
  the others append later without a redesign.
- **The name is no longer asked at the door.** The import already proposes a name from the paste and confirms
  it at the destination step (`MenuPasteImport.tsx:117`), so today's dialog would ask twice. The blank route
  keeps its name field, because there the name is all there is.

Front-end only. No schema, API or parser change; the backend is verified on dev at `339690fc`.

### M8 — delete a menu (#868)

There is no way to destroy a menu. **Put away** is the terminal state — which is exactly what Q79 asked, and
the owner declined on 2026-08-07: *"ADD DELETE this build. 'Delete forever' in the ⋯ only for menus on zero
screens; hard confirmation naming the destroyed menu and history; shared library items survive."*

Nothing was built. Six items in the card ⋯ menu (`MenusHome.tsx:569`–`:609`), no `HttpDelete` on
`BackOfficeMenusController`, no `DeleteMenuAsync` anywhere in `src/Vennu.Data` or `src/Vennu.Core.Models`.

**The data shape is the hard part.** `dbo.Menus` is referenced by ten foreign keys across four migration files;
only two carry `ON DELETE CASCADE`. A naive delete fails on a constraint, and cascading everywhere would let an
import session silently delete published history. So: an ordered, explicit delete in one transaction,
migration 076 — the shape `deleteMenuPage`/`deleteMenuSection` already established.

**It is blocked, deliberately, on three answers** now in the register:

- **Q210 · BLOCKING** — on a menu that is on a screen, is *Delete forever* absent (decision 4) or refusing with
  a named reason (decision 5)? Recommended absent: **Take off the screens** is two lines below in the same menu.
- **Q211 · important** — does the confirmation demand typing the menu's name? Recommended no: the paste-import
  second pass already settled the neighbouring case as "no typed-confirmation ritual".
- **Q212 · BLOCKING** — does delete destroy `MenuHistoryEntries` and `MenuPublishEvents`, or detach them?
  Recommended destroy: "forever" that leaves attributable rows behind is not forever.

### Why M8 took a top-level number ahead of M7 (SOP step 2b)

Recorded where the leapfrogged milestone's own record lives — `menu-builder-v2/mock-fidelity-polish-plan.md`.
M7 is blocked/parked by owner ruling behind Foundry and owner scoping, nothing there waits on a developer, and
M8 shares no files with it. Filing a schema→API→UI vertical as `M7.4` inside a round scoped to "make the
builder match the mocks" is the misfiling that step exists to make visible.

### Validated

Real local Atlas build against this working tree: **448 pages, no schema errors.** Both new milestone pages
render with working contents anchors. The build refused the first `workstream.json` edit — `position` was 291
characters against a 240 cap — which is the schema doing its job.

### One exact next action

Answer **Q210, Q211 and Q212** (`docs/features/menus/open-questions.md`), or start **M6.5 (#867)**, which is
unblocked and needs nothing from the owner.

## 2026-08-25 — Paste import parsed nothing, and CI had been red for everyone

**Two defects, both silent, both shipped fixed and deployed to dev.**

### The parser could not read a menu written the ordinary way (#864, PR #862, milestone M6.4)

The owner asked to wire up menu import and test it. It turned out to be **already fully wired** — client `api.ts` →
`BackOfficeMenuImportsController` (`api/back-office/menu-imports`) → `MenuImportService` → `IMenuImportRepository`,
DI registered at `src/Vennu.Data/Extensions/ServiceCollectionExtensions.cs:25`, migrations 068–071 in place. The
wiring was never the fault.

Pasting a real menu returned `201` with the **sections found correctly and `itemCount: 0`**. Every item line came
back `unresolved` / `item_format_not_recognized`.

`MenuPasteParser.PriceAtEnd` required **two or more spaces**, or a dot leader, between an item's name and its price.
So `Garlic Bread 6.50` parsed as nothing, and so did every **tab**-separated line — which is exactly what a
spreadsheet paste produces, a route the product advertises. `docs/features/menus/README.md` promises "no syntax to
learn"; the screen had a syntax, it was undocumented, and it was two spaces.

**Why the suite did not catch it, which is the part worth remembering:** every existing parser test wrote its
fixture as `"Burger  12"` — two spaces. The tests passed while encoding the defect as the expectation. Tests written
from the same assumption as the code confirm the assumption rather than the requirement.

Separator widened to `(?:\s+[.·•-]{2,}\s*|\s+)`. The **number format was not touched** — whole numbers, a leading
currency symbol and `MP` already parsed and only ever failed for want of a second space. 12 tests added, including
one whole realistic menu; 490/490 unit tests pass.

**One trade-off accepted deliberately:** a capitals-only heading ending in a bare number (`SPECIALS 2`) now reads as
an item priced 2. A guard against it was written and then **removed**, because it broke
`Parse_PricedUppercaseLineIsAnItemNotAHeading` — an existing, deliberate assertion that `BLT  12` is an item.
`SPECIALS 2` cannot be told from `BLT 12` by shape alone. Review can promote any line to a section, so it is
recoverable. Recorded as its own test so the next reader meets it as a decision, not a surprise.

### The merge-marker CI check had been failing on every branch since 2026-08-25 (PR #863)

PR #862 could not merge because `docs-validation` was red. It was not the PR. The job greps the **whole repo** for
`<<<<<<<`, `=======` or `>>>>>>>` at line start, and six terminal transcripts added under
`docs/research/local-ai-model-qualification/` contain plain ASCII rules (`======================`). Every branch cut
from master since then inherited a red CI job, including branches touching no documentation at all.

Fixed by searching only for `<<<<<<<` and `>>>>>>>`. A conflict git leaves behind **always** writes all three
markers, so the bare `=======` arm was never load-bearing — it only supplied false positives. No exclusion list to
maintain, no detection lost. Verified both directions: the repo is clean under the new pattern, and a file
containing a real conflict block is still caught.

### Tracker

**M6.4** added to `docs/features/menus/workstream.json` with issue #864 and a full write-up in `milestone-plan.md` —
the four line shapes that failed, why the suite passed anyway, the trade-off, and a task table that *answers* each
task rather than restating its title. **M7.3 corrected from `next` to `parked`**: the ruling putting it aside had
already been made (UI polish the separate Foundry component system will settle), but the tracker still advertised it
as the next action. Validated with a real local Atlas build — 446 pages, no schema errors, M6.4's page renders its
plan section with working contents anchors.

### Verified on deployed dev, not just in unit tests

The local dev stack could not be restarted during the fix (`start-ui-test-env.ps1 -Stop` reported PID 22416 could
not be terminated; ports 5175/5177/5199 held by orphaned processes — 5199 turned out to be a Vite dev server from
2026-08-23, not the API). So T6 was done against **deployed dev** instead, which is the stronger check anyway.

`dev.api.vennusign.com/health/version` reports `sourceCommit: 339690fc` — the fix's own merge commit. Signed in
through the real Entra flow as the QA account and posted the exact repro to
`POST /api/back-office/menu-imports`: **`itemCount: 4`**, sections `[STARTERS, MAINS]`, items
`Garlic Bread|6.50`, `Wings|12`, `Burger|14`, `Steak Frites|28.00`, **zero unresolved lines**. Tab-separated,
`MP`, and the `SPECIALS 2` trade-off all behave as documented. The back-office review screen reads "Build a new
unpublished menu from **all 4 imported items**" — screenshot in `output/murphy-2026-08-25/`.

**A regression spec now guards it:** `tests/ui/specs/menu-paste-import-parser.spec.ts`, run against a *deployed*
environment like `customer-menu-journey.spec.ts` rather than localhost, so it exercises the real parser. It skips
cleanly when QA credentials are absent. 2/2 passing against dev. This is the direct answer to how the bug survived:
the unit suite tested the parser through fixtures that shared the code's own assumption, and nothing tested a menu
a person would actually paste.

### Next action

**Nothing on Menus is blocked on code.** M7.1 (#852/#853) and M7.2 (#855) are blocked on owner scoping; M7.3 (#854)
and the entire design-system workstream (#856–#861) are parked pending Foundry. #864 can be closed.

**Unaddressed and pre-existing:** the Playwright "UI regression" CI job fails with 190
`AuthenticationException: ...UntrustedRoot` errors and zero assertion failures. Identical on master, verified again
this session. It is an infrastructure/certificate problem, not a test problem, and nobody has picked it up — which
means the gate has been reporting red for so long that a real failure inside it would not be noticed.

## 2026-08-24/25 — Menu-builder polish round shipped, then a design-system audit that found the real problem

**Ten PRs merged and deployed to dev** (#836–#840, #844, #846–#850), all under the #834/#842 mock-fidelity
feedback round against the approved mocks. Two app-crashing bugs, a documented dialog-field standard (decision O3),
section actions converted from modal to anchored dropdown, item-panel polish, bottom-bar rework, and a connecting
line through the history timeline. Every merge went through CI with only the known pre-existing `UntrustedRoot`
SSL failure on the Playwright job — verified identical on `master` each time.

**Real defects found while doing it, worth noting because they were all silent:**

- Two blank-screen crashes from the same React pattern: reading `event.currentTarget.value` *inside* a deferred
  state updater, where React has already nulled it. Three instances existed; only two were reported.
- The capacity banner referenced `--sky-color-warning-border` and `--sky-color-warning-soft`, **neither of which is
  defined anywhere**. It had been rendering with no background and a default dark border for an unknown period.
- The Remove-item dialog's Cancel button used class `.secondary`, which exists in no stylesheet, and that dialog
  alone ignored both Escape and click-outside.
- "Rename" in the section-actions menu did nothing: `useDialogFocus`'s cleanup unconditionally returned focus to
  the trigger, stealing it from the rename field that the same click had just opened.

**Then the design-system work, which is where the session actually went.** The owner's framing:
*"there is always some sort of disconnect between the mock which has all the agreed items to what actually gets
developed"* — and, on where to start, *"I honestly don't know."*

Audited it rather than guessed. **416 controls in the back-office. 247 buttons, 114 with no styling at all.** Seven
different looks for the main action, thirteen for the ordinary action, six for destructive — including one where
`.danger` renders identically to Cancel and to Save. Every `select`, checkbox, radio, number, time and colour input
in the product is unstyled; only dialogs are consistently styled.

The cause is structural, not a discipline failure: **a mock is a picture, and a picture cannot be reused.** Someone
reads it and writes an approximation, once per button, 247 times. The gap closes when the component becomes the
shared object between design and code rather than a picture and a re-implementation of one.

**Two owner corrections shaped how this is recorded, and both are now memories:**

- *"We can't trust what's in there because changes have happened after — it's three ways."* The design project, the
  written spec and the code each move independently and any one can be stale. Proven by the focus ring: design says
  3px sky blue, code changed it to 2px dark teal for a contrast fix sky blue genuinely fails, and this week's
  feedback removed it entirely on dialog fields. A design file is evidence, never a verdict.
- *"I don't see the actions button on this list."* It was in the generated audit and missing from the rendered
  picture, because the variants had been hand-picked — making the picture a summary again, the exact failure the
  work exists to stop.

**Everything is generated now, not written down.** `scripts/design-audit/collect.mjs` parses the source with the
TypeScript compiler; `render.mjs` draws every variant using the app's own stylesheets. This mattered: successive
regex passes counted inputs as 9, 13, then 39 against a true 111, and every wrong number looked plausible. Parsed
counts match ground truth exactly.

**Tracking.** Two workstreams now carry this, both live in Atlas: **Menus M7** (the polish round, with M7.1 Review
& Publish paused for owner review, M7.2 full history screen blocked on it, M7.3 the background tint needing
live-site investigation), and a new **Design system** workstream (M0 inventory awaiting owner rulings, then token
consolidation, components, and a `#/design` gallery route). Issues #852–#859, #861.

### Next action

**Nothing is blocked on code — it is blocked on owner rulings.** Design system M0 (#861) has twelve open questions
(T1–T13; T7 answered: components must be skinnable, because Theme Studio needs a different look over the same
controls). The rendered picture and full audit are on that milestone's page in Atlas. Menus M7.3 (#854, background
tint not visible on the live site despite shipping twice and verifying locally) is the one unblocked, actionable
item — and needs Playwright run against the real dev URL, not the local dev server, before another CSS attempt.

## 2026-08-23 — Tracker catch-up, owner triage on five open items, and a new venue-broadcast defect (#811)

**Why this entry exists.** `tracker/assignments.json` still showed #775 as *claimed* (05:50) as of this session's start, but git/GitHub showed seven more merges past it: #794/#796, #802/#805, #798/#808, #806/#807, #775/#804 itself, #797/#809 (partial), and #799/#812. None of that had been synced to the tracker or this file. Per AGENTS.md - "repository and GitHub state override chat history" - the tracker is now caught up (see its `previousCompletedAssignment`); this entry does the same for the handoff, at a summary level. Exact commits are `git log master`, not restated here.

**Stale-but-actually-fixed issues closed:** #775 (PR #804), #794 (PR #796), #802 (PR #805) - all had merged fixes but were still open on GitHub.

**Owner triage on five items, 2026-08-23:**

- **#744 (retire `dbo.MenuItems`) parked as a design doc, not scheduled.** Moved to `docs/design/proposed/retire-menuitems.md` (PR #813, doc-only) - same pattern as #774/#765: proposed doc merged, issue stays open, nothing scheduled. Owner: leave it until the time is right to discuss it.
- **#797 real-world story changes its actual scope.** The owner, using the delete-section flow, actually wanted to create a new page and move an existing section ("Dinner") onto it - not delete-and-redistribute-items, which is what #809 partially fixed. Recorded on #797: this needs a first-class "move this section to a page" action (existing or newly-created), distinct from the delete dialog's item-transfer path, and it touches the same same-page DB guard (`DeleteSectionSql`/`MovePlacementGuardedSql`) #809 found and left alone. Not implemented - needs its own design/scope decision.
- **#800 timing extended to cover every action, not just the write pipeline.** Owner wants per-action timing in the menu builder for investigation. PR #814: `run()` gained a `describe` override so publish/discard/add-page/duplicate-page/delete-page/save-assignments/toggle-availability/go-back-to-version stop collapsing into the generic "Your last change" bucket; `undo()`/`redo()` gained their own `[perf:undo]`/`[perf:redo]` logging - they call `step.undo()`/`step.redo()` directly and had **zero** timing before this, unlike every other write. Still console-only, temp, tied to #800/#774.
- **#776 (Murphy auto-run after deploy) closed, not planned.** Owner: "Murphy should only run when we say so for now." Recorded in memory so this isn't re-proposed.
- **Housekeeping:** `feat/799-restore-capability-and-view-all-scope` deleted locally (PR #812 already merged as `fd46adb6`); remote branch delete was blocked by the session's permission classifier (destructive remote op) and is left for the owner (`git push origin --delete feat/799-restore-capability-and-view-all-scope`).

**#811, venue-wide emergency broadcast never delivering in realtime, found and fixed same session.** Found independently reviewing #810 (the #769 venue-notify audit doc). `BackOfficeEmergencyBroadcastsController.NotifyAsync` fell through to `NotifyVenueContentUpdatedAsync` when `ScreenId` is null - the exact same dead `venue:{id}` SignalR group #769/#763 already proved nothing joins. "All venue screens" is the **pre-selected default** in `EmergencyBroadcastAdministration.tsx`, so this is the common path, not an edge case, and more urgent than #769's original case given what an emergency broadcast is for. Was self-healing via the 60s content-poll recovery, same masking mechanism as #769.

Fixed on the owner's "continue" - PR #816: the controller now also loops the venue's screens (`IScreenRepository.GetByVenueIdAsync`) and calls `NotifyScreenContentUpdatedAsync` for each, same shape as #763's publish fix. Venue-wide call kept alongside the loop, matching `ContentService.PublishAsync`'s own precedent. Payload needed no change - `applyRealtimeEvent` in `src/display` already merges an `emergency-broadcast` change directly regardless of `screenId`. 3 new focused unit tests (screen-targeted notifies only that screen and never touches the repository; venue-wide create/cancel both fan out to every screen plus the venue-wide call). `dotnet build` (Release) clean; `Vennu.Api.Tests` `Category=Unit` 467/467 (464 baseline + 3 new). **UNTESTED: live verification against a real deployed venue with multiple screens** - no authenticated dev session was set up for this pass.

### Next action

Independent review pending on PR #813 (docs, #744), PR #814 (#800 timing extension), and PR #816 (#811 fix - the one that actually matters functionally; review this one first). Behind those: #803 (local UI-test fixture reset broken, causing menu-cap creep across sessions) and the design decision on #797's actual scope (move-section-to-page).

## 2026-08-23 — A display diagnostics view, and two defects it would have caught on sight (#738, #790, #791)

**Built `GET /api/display/{screenId}/diagnostics` and `/display/{screenId}/diag`, web-visible facts only, per #738's scope narrowed to what the web can actually see** (device model is unreachable from a browser and only available on Tizen/webOS/Android through their own launchers, which is a separate, larger piece of work - not attempted here). The owner's issue said "for a future milestone, not now" on 2026-08-20; restarted today at the owner's direction with a live failure in hand.

**The server half is anonymous like every other player endpoint** - identifiers, states and timestamps only, no menu content, no PII. Screen identity, staleness against `HeartbeatMonitor`'s own configured threshold (not a hard-coded guess), delivery state (authoritative vs applied revision, failure code), the last receipt's player/shell version, and whether this screen is some journey's first display with its go-live timestamp. Required adding `ICustomerOnboardingRepository.GetByFirstScreenIdAsync` - the WHERE clause already existed inline in the go-live latch; this just exposes it as a read.

**The player half is a standalone probe page, not the live player** - it never heartbeats and never posts a content receipt, so opening it from a laptop cannot mark a screen Online or write a phantom delivery record (the same trap `DisplayPage.tsx`'s `preview=observer` guard exists for). It shows geometry (viewport, screen size, DPR) beside the server's configured size, board fit (rendered height vs viewport - neither is measured anywhere else in the product), which theme fields the active layout's own CSS actually consumes versus what is served, the content cached on this device, and a recent-events timeline.

**The player now records what it always knew and always threw away.** `startDisplayHeartbeat` gained an `onResult` callback - heartbeat failures were previously swallowed in a bare `catch {}` with nothing to show for it. `displayReceipts.mjs` gained `describeReceiptSkipReason` - a skipped receipt (no revision, no screen key) returned `null` exactly like a swallowed network failure, so the two were indistinguishable from the caller. Both, plus content-fetch source and connection-state transitions, are written to a per-device, per-screen rolling record in `localStorage` (`displayDiagnostics.mjs`) - deliberately per-device, since a laptop reading another device's cache key would show its own empty history as if it were the wall's.

**Two defects found scoping this, filed separately and not fixed here:**
- **#790** - the live QA screen's `photo_grid` board is 274px taller than a true 1920x1080 viewport with three of six items entirely off-screen, no scroll, no indicator. Fits only at ~4K, which reads as coincidence rather than design.
- **#791** - that same screen serves `contentRevision: null`, so it has never posted a content receipt. Very likely the cause of #749 (`ScreenContentDeliveries` never written).

**Verified the diagnostics page actually would have caught #790 on sight**, not just in principle: seeded a local cache with the live screen's real content and reloaded `/diag` - the board-fit panel read "155px taller than the viewport - content below the fold" and "Theme fields consumed: 3 of 10" for `photo_grid`, both correct, with a visible mismatched thumbnail. Screenshot taken; CORS blocked the server-half fetch from `localhost` as expected (`dev.display.vennusign.com` is allowlisted in `Cors:AllowedOrigins`, `localhost` is not and should not be) and the panel degraded to a message rather than breaking the page.

**Environment note:** `src/display`'s production build (`vite build`) was broken on this machine - `Cannot find module @rollup/rollup-linux-x64-gnu`, npm's known optional-dependency bug. Fixed with a plain `npm install`; the resulting `package-lock.json` diff was cosmetic (`libc` metadata only) and was reverted rather than committed, since it was not part of this change. Anyone hitting the same error on a fresh clone should just run `npm install` in `src/display`.

Evidence: `dotnet build` clean; `Vennu.Api.Tests` Category=Unit 464/464 (461 baseline + 3 new); `src/display` `npm test` 171/171; `tsc -b` clean; `vite build` succeeds. **UNTESTED:** the deployed `/diag` page against a real allowlisted origin (only verified locally with a seeded cache, per above) and the on-screen gesture the original issue also asked for - this ships the URL only, not a discovery gesture from the running player.

PR opened on `feature/738-display-diagnostics`, not yet merged - awaiting independent review per the merge gate.

### Next action

Independent review of the #738 PR, then #790 and #791 (the defects it found), then back to #775 (item double-submit) which was the exact next action before this was picked up.
## 2026-08-23 — #790 fixed generically, above the layout components rather than inside one of them

**Owner context that shaped the approach:** the current nine layout components (`photo_grid`, `neon_chalkboard`, etc.) are expected to be superseded by Theme Studio's JSON display-definition, which the display would then render dynamically. Checked before writing anything: Theme Studio is `stage: "designing"` with no design authority, no question register, no milestone plan, and "nothing in this repository yet" (`docs/features/theme-studio/workstream.json`); `dev-theme-studio`'s App Service exists with no application deployed. That is pre-planning, not imminent - so a fix confined to `PhotoGridLayout.tsx`/`photoGrid.css` specifically risked being real but throwaway work, while the underlying screens keep running the current renderer for an unknown span in the meantime.

**Fixed at the `DisplayFrame` wrapper instead - one place above all nine layouts, not inside any one of them.** Every layout sets `min-height: 100vh` on its own root with no ceiling, which is the actual defect shape, not something specific to `photo_grid`. `DisplayFrame` now measures the rendered board's height against the real viewport after layout, and applies a uniform `transform: scale()` to shrink it to fit - never scaling up, never below a 0.4 legibility floor past which some overflow is accepted rather than illegible text. `computeFitScale` is a pure function in `boardFitScale.mjs`, unit tested. This survives a Theme Studio renderer swap: whatever eventually draws the board still has to fit inside a real screen's viewport, and this wrapper doesn't care what produced the content underneath it.

**Verified against the live QA screen's real content**, not just the unit tests: seeded a local cache with the exact board from #790 and loaded `/display/{screenId}` in a real browser at 1920x1080. Scale computed as `1080 / 1354 = 0.7976`, and all six items - three of which were previously entirely off-screen with no scroll and no indicator - are now visible and legible (screenshot taken). A short board (one section, one item) verified separately to render at scale `1`, untouched.

**One known interaction with PR #792 (display diagnostics, #738), not addressed, noted in both PR descriptions.** #792's `/diag` page renders `DisplayLayout` inline at a simulated device size for its debug thumbnail rather than the page's actual viewport. This fix measures against `window.innerHeight`, correct for every real player - which is what #790 is actually about - but will slightly double-scale that thumbnail's cosmetic sizing once both branches merge. Not a data-correctness issue: the diagnostics board-fit panel measures `scrollHeight`, which CSS transforms do not affect, so the numbers it reports stay right regardless. Whoever reconciles the two branches should thread an explicit viewport-height override through if the thumbnail's exact pixel sizing ever matters.

PR opened on `fix/790-board-fit-safety` (#793), independent of #792 - branched from `master`, not from the diagnostics branch, so it does not depend on unreviewed work. Not yet merged.

Evidence: `tsc -b` clean; `npm test` 144/144 (140 baseline + 4 new); live-content verification above.

### Next action

Independent review of PR #792 and PR #793 (unrelated to each other, can review in either order or in parallel), then #791 (the same live screen's null `contentRevision`, roots #749), then back to #775 (item add double-submit), which remains the most owner-visible defect on the board.

## 2026-08-22 — The toolchain is written down, and two false claims in this handoff are corrected

**`AI_DEVELOPMENT_GUIDE.md` now has a *Local Toolchain* section**, verified by running every command in it rather than by recall. It records Linux Node and how to invoke Node tooling, Playwright and its real Chromium binary, Windows `dotnet.exe`, `SQLCMD.EXE` and what it can reach, and the short list of what is genuinely absent. Nothing recorded this before, which is how the two corrections below came to be believed.

**Two statements in this handoff were untrue and are fixed in place.** The 2026-08-20 machine note said the sandbox has no .NET SDK, no Node and no SQL client — only the .NET SDK part is true. The #752 entry said "Murphy has no database access today" and listed it as one of two gates; `SQLCMD.EXE` plus the `sql-dev-*` credentials in `kv-vennusign-dev` reach dev SQL, so that gate is a design choice, not a missing capability.

**The rule this follows:** a record that is behind is tolerable, one that states something untrue sends the next session down the wrong path. Both claims had already done that.

## 2026-08-22 — Publish became a real gate, and the realtime path turned out never to have worked

**The reported defect was real and is fixed.** `DisplayController.GetContent` composed the board from `dbo.Placements` joined to `dbo.Items`, and `dbo.MenuSections`, with **no publish predicate anywhere in the read path**. Every keystroke was live the moment it was typed; publish wrote a snapshot and delivery targets that nothing served from. That is menus decision 1 inverted and decision 38 contradicted. Reproduced on dev before changing anything - an item created and never published came back from `/api/display/{id}/content` alongside the published one - then fixed to read `GetLatestPublishedSnapshotAsync` ordered by the snapshot's own `SortOrder` (#757, `1ef9d4de`).

**The second claim did not reproduce.** "Reordering sections does not update the display after publish" was tested three ways - API serves the new order after publish; a live player picks up a publish unprompted; and the exact combination, live screen plus reorder plus publish. All passed. What was really happening is below.

**Making publish the only thing that changes a screen exposed two long-standing defects.** Both had been invisible because `DISPLAY_CONTENT_RECOVERY_INTERVAL_MS` (60s) was quietly doing all the work:

- **Nothing announced a publish at all.** Every individual builder edit calls `NotifyAsync`; publish did not. Fixed (#760) - and the fix measured as a complete no-op, which led to the real cause.
- **A venue-scoped notify reaches no display.** `displayConnection.mjs` joins only `screen:{id}`; every notify in `ContentService` broadcasts to `venue:{id}`. **No player has ever received one.** Publish now notifies each target screen directly (#763, `e578e4da`), and publish-to-wall fell from 63,686 ms to 12,757 ms. The rest of that audit is #769.
- **A display gave up reconnecting after 42 seconds, permanently.** `withAutomaticReconnect()` with no argument is four attempts and then it stops. Any deploy or restart killed a screen's realtime connection until a human reloaded it. Fixed with an unbounded policy (#771) and **verified against a live screen through a 158-second outage** - it held "reconnecting" past the old 42s cliff and recovered on its own.

**Builder responsiveness.** Dragging a section took **3,981 ms** because it awaited the write and then `refresh()` - which is four parallel GETs - before rendering a permutation the client had already computed. Now drawn first: **61 ms**. Two failed attempts got there: the first skipped `refresh()` entirely, which broke `draftCount` so the Publish button never appeared after a reorder (shipped and reverted, #762); the second kept the draw and restored the reconcile (#767). `venueFetch` also gained a 45s timeout - it was a bare `fetch` with no abort signal, so a hung request left `busy` true and `saveState` at "saving" forever, which is what a saturated B1 did to the builder for ten minutes.

**Owner rule recorded in `AGENTS.md`:** *client first, server last*, with its boundary - the server is still last word on ids, counts, refusals, and facts about the world rather than intent: availability, entitlement, money, and what is published.

### Database

**No schema change and no migration.** `databaseSchemaVersion` is still `073_customer_onboarding_go_live_achieved`, before and after.

**What changed is what the display reads.** It no longer reads `dbo.Placements`/`dbo.Items`/`dbo.MenuSections` for board content; it reads the latest published snapshot through `GetLatestPublishedSnapshotAsync` and orders from the snapshot's own `sortOrder`. Two joins stay live on purpose: `ItemAvailability`, because decision 3 says 86 never waits for a publish, and the live item row for `ImageUrl`, which the snapshot does not carry. An item published and since deleted in the builder therefore stays on the wall - deliberate, and the behaviour most worth a human sanity check.

`BoardItemsSql` and `SectionsForPageSql` are now unused by the display path. They were not deleted; other callers should be checked before removing them.

**Test data written to dev, and not cleaned up.** Roughly 25 menus on the QA venue with their sections, items, placements and screen assignments, named `QA journey *`, `QA publish-gate *`, `QA reorder *`, `QA timing *`, `QA live-refresh *`. This is #752 territory and the owner decision there is **report before removing**, so it is reported rather than deleted. #746 should be fixed first or every future run adds more.

### Infrastructure

Plan `appsrv-basic-web` moved **B1 -> B3** (4 vCPU / 7 GB, $51.10/mo), after passing through B1x3 and P0v3. Memory had been pinned at 84-86% and unmoved by restarting five apps, and CPU hit 97-100% during Playwright runs, causing several false test failures. Prod and stage app services remain **stopped** from this morning; `dev-theme-studio` and `dev-board-engine` stopped as unused. `dev-po` deliberately left running - `deploy-dev.yml:320` verifies it by fetching `version.json`, so stopping it plants a landmine for the next platform-operations deploy.

**Honest caveat:** the B1->B3 decision was taken on a 97% CPU reading without establishing the failing runs were CPU-bound. That is the normal situation today, and it is the argument in the observability proposal.

### Filed, not started

- **#774** observability, correlation and performance telemetry - feature, with `docs/design/proposed/observability-and-performance-telemetry.md` merged as *not approved*. Central position: measure the user-perceived span, not the HTTP request. The reorder measured 3,981 ms while the `PUT` behind it returned 204 in under a second. Cheapest first win needs no new infrastructure - `AppliedUtc - PublishedUtc` is publish-to-wall latency per screen, from receipts the product already discards.
- **#775** adding an item double-submits. Enter and the create button are **different code paths**: Enter searches the library and dedupes by canonical name, the button calls `place_` directly and skips it. Neither is guarded, and Enter's library search happens before `busy` is raised, so it looks dead. Writes duplicates into the venue's shared item library. Found by the owner in minutes; investigated, not fixed.
- **#776** Murphy should run after a deploy. Five deploys today, Murphy invoked zero times - hand-written specs were substituted for an exploratory pass, which is exactly how #775 survived.
- **#769** the venue-scoped notify audit. Not a blanket `JoinVenue`: most of those notifications describe unpublished edits that by decisions 1 and 38 must **not** reach a screen.
- **#770** a deployed display never picks up a new build. Gates every future display fix - #771 only reaches a wall someone reloads.
- **#765** stale-signal design proposal, merged *not approved*. Treatment 05 needs an owner decision. Outage testing added two findings: there are **two** guest-visible offline messages, not one, and the player is more resilient than the banner implies - a `localStorage` cache survived a full reload with the API stopped.

### Next action

**#775 first** - it corrupts the shared item library, and the owner has seen it. Then decide #765's treatment 05, then #769's audit.

## 2026-08-21 — The signed-in onboarding spec ran against dev, and passes

**Done, and it is the item that had been the exact next action for two sessions.** `specs/customer-onboarding.spec.ts` run signed in against dev: **3 passed (24.5s)**, including the two cases that had never been executed anywhere.

```
VENNU_QA_EMAIL / VENNU_QA_PASSWORD  from kv-vennusign-dev
VENNU_BACK_OFFICE_URL=https://dev.back-office.vennusign.com
VENNU_API_URL=https://dev.api.vennusign.com
node node_modules/@playwright/test/cli.js test specs/customer-onboarding.spec.ts --project=desktop
```

This proves the two things it was supposed to: **the durable go-live fix (#728) holds** — the display is reported Offline and onboarding stays complete, with `GoLiveAchievedUtc` unchanged — and **the headless onboarding path works**, pairing through `src/display`'s own shipped `pairing.mjs` rather than a copy.

**The QA account had never onboarded on dev, so the first run did the real thing** — Entra sign-in, the onboarding forms, pair, go live. Subsequent runs short-circuit through `ensureOnboarded` and take 24s instead of two minutes.

**The first run failed, and it was not a defect — read this before someone chases it.** `pairDisplay` clicks "refresh device status" then waits 30 seconds for "you're live". From the database: screen created `04:56:35`, first heartbeat and go-live both `04:57:07`. **32 seconds** — two past a hard-coded window, on B1. If the onboarding flow survives redevelopment, that wait should scale with the environment rather than be a constant.

**Checked, because it mattered:** the earlier cleanup did **not** cause it. The one pre-existing `CustomerOnboardingStates` row belongs to a different user, both its venue and its first screen are present, and the QA account simply never had one. Worth noting that my safety check did not cover `FirstScreenId` pointing at a traced screen — it happened to be fine, but that check belongs in `clean-dev-test-data.sql` if it is used again.

**Running it costs dev a little drift**, as expected: 100 -> 102 screens and 121 -> 122 venues across two runs. That is #746 and #752 territory, not a surprise.

**How to run it at all, since nothing recorded this:** Playwright needs Windows Node (`/mnt/c/Program Files/nodejs/node.exe`), invoked as `node node_modules/@playwright/test/cli.js` rather than through `npx.cmd`, and environment variables reach it only via `WSLENV`. `globalSetup` prunes with `sqlcmd -S '(localdb)\MSSQLLocalDB'`, hard-coded, so it cannot touch a deployed environment — verified before pointing anything at dev.

## 2026-08-21 — The dev database is clean, and Murphy owns keeping it that way

**The test residue is gone (#745, closed).** The trace was still intact and its keys still resolved, so this was a named delete rather than a pattern match: 26 of 26 traced venues, 30 of 33 traced screens and 12 of 12 traced pairing codes were still present and were removed by id.

```
Venues        147 -> 121
Screens       130 -> 100
PairingCodes   31 ->  19
TestRecordTrace          dropped, last, once the rows it mapped were gone
```

Unchanged, as expected: Menus 127, Items 70, Placements 46, MenuScreenAssignments 49. API, display and back office all answer normally afterwards.

**It was safe because it was checked first, not because the names looked like tests.** Zero menus, items, assignments, onboarding states or subscriptions on traced venues; zero traced screens on a non-traced venue; zero untraced screens on a traced venue. A closed island. 21 traced screens had no venue at all.

`scripts/maintenance/clean-dev-test-data.sql` is kept: it rolls back unless `@Commit = 1`, deletes only rows the trace can name, and **refuses outright** if anything real has attached itself since. A snapshot of all 153 removed rows is at `C:\temp\vennusign-backups\dev-test-data-removed-2026-08-21.tsv`.

**Owner decision: Murphy owns environment data hygiene from here (#752).** Cleanup was never the hard part — noticing was, and nothing was watching for three weeks. Murphy should report drift in deployed environments (objects no customer journey created, and tables present in a database that no migration creates — which is how `TestRecordTrace` was found), name what produced it, and report before removing. Two things gate it: whether it reads the environment database or infers drift through the API is the first design question — **note that the original "Murphy has no database access today" was false**, `SQLCMD.EXE` reaches dev SQL with the `sql-dev-*` credentials in `kv-vennusign-dev`, so this is a design choice and not a missing capability; and #746 should be fixed first or every run will report the same growing screen count.

## 2026-08-21 — Test credentials, a destructive test, and what is actually running on the plan

**A test was running `DELETE FROM dbo.Venues` against whatever an environment variable pointed at (#751, fixed in `ae206191`).** `AzureSqlPhase02IntegrationTests` deleted every pairing code, screen and venue from whatever `VENU_TEST_AZURE_SQL_CONNECTION_STRING` named, with no guard — `DatabaseFixture` has `EnsureDevDatabase`, this had nothing. On this machine that variable named the dev product database. **The only thing between that test and 147 venues and 128 screens was a stale password.** It was invisible because it returned silently when the variable was unset, so on a cleaned-up machine it looked like a passing test that ran nothing. It now creates its own database, uses it, drops it — and runs for the first time.

**Credentials now follow the target.** LocalDB is the default and needs no user and no password. Azure is `VENU_TEST_TARGET=azure`, and the credentials come from Key Vault `kv-vennusign-dev`. `VENU_TEST_AZURE_SQL_CONNECTION_STRING` is retired: it is ignored, loudly, and the message names both the replacement and why it is still set when nobody set it. Red then green in the same poisoned environment: **115 failed / 8 passed before, 131 / 0 after**, with the variable still inherited. `AGENTS.md` carries the rule now — a test never carries a credential, and never deletes from a database it did not create.

**Rule from the owner: `az` and `git` are always available.** Do not report a CLI as missing or design around its absence; if it is not on PATH here, it is in a venv, in WSL, or on the other OS side. Stated after I reported "Windows az: absent" and began building a fallback.

**28 apps on the B1 plan, 17 of which do nothing (#748).** Measured from Azure Monitor rather than by probing, because 28 cold starts would wedge the plan. Fifteen have had **zero** requests in 30 days and none has a custom domain; `vennusign-stage-api` has 4 and `vennusign-stage-back-office` has 1. Stopping them is free and reversible and is worth trying before paying for a bigger tier. Two things stand out: **production has a marketing site and no API behind it** — `vennusign-app` serves www.vennusign.com with real traffic while `vennusign-app-api` has had zero requests in 30 days — and `appsrv-basic-web` is an unrelated site, `comfortableretreat.com`, which is the second-busiest app on the same worker as every Vennusign environment.

**Also filed:** #749 (`ScreenContentDeliveries` empty while `MenuPublishTargets` holds 102 — the #739 shape, worth confirming) and #750 (`dbo.LayoutTemplates`, 8 seeded rows referenced by nothing, likely pre-seeding for #709).

## 2026-08-21 — A green deploy now has to prove the new build is running

**#740, #726, #739 and #736 are closed, and the pipeline change is deployed and verified on dev.** All five apps report the commit they were deployed from; the first run through the new check confirmed every one on the first attempt with no restart escalation.

```
dev.api.vennusign.com/health/version    sourceCommit 1eb4cfdd  databaseSchemaVersion 073_customer_onboarding_go_live_achieved
dev.back-office / display / www / po    sourceCommit 1eb4cfdd  buildId 32431079660
```

**How the check works, and why a matching commit is proof.** `deploy-api` writes `VENNU_SOURCE_COMMIT`/`VENNU_BUILD_ID` as app settings *after* the package upload. Writing an app setting is itself what forces the recycle — OneDeploy only requests one — so a matching commit means a recycle happened after the package landed. The order is load-bearing the other way too: writing the setting first would restart the old build carrying the new commit id, and the check would go green over stale code. Each static build stamps `dist/version.json`; `pm2 serve --spa` serves a real file when one exists, so no host config changed.

**A second defect of the same shape was found while shipping it.** `deploy-dev.yml` classified changed paths with `git diff HEAD^ HEAD` — the last commit of a push, not the push. The first push of this work carried three commits ending in a documentation commit, so every deploy job was skipped and the run went green having shipped **nothing** (`c4f40200`). Fixed in `1eb4cfdd`, guarded in `scripts/ci/test-classify-changes.sh`. Same lesson as #740: the pipeline reported success for work it had not done. **Assume nothing about a green run that you have not read the job list of.**

**Not proven yet, and named rather than claimed:** the plan was on B3 for this deploy, which is the condition where #740 does *not* reproduce. The check has not been exercised against a genuinely stale deploy in the wild.

**`databaseSchemaVersion` is no longer an environment variable.** It is read from DbUp's journal — the one field on `/health/version` that is not the build talking about itself. And to answer the question directly: **DbUp was never at fault.** It throws rather than continuing, and runs before the host is built, so an API answering requests on an un-migrated database is not a reachable state. Migration 073 was missing because the process serving requests had been started from the previous package and 073 was not in its assembly. Same root cause as #740, not a second defect.

**The database audit found almost nothing to remove, which is the finding.** `docs/reports/database-schema-audit-2026-08-20.md`. 79 tables in dev: 34 hold rows, 45 are empty, and **all 45 empty ones are referenced by product code** — there is no orphaned table. Every table the scripts create exists, and every table that exists is created by a script, except `dbo.TestRecordTrace`. Scripts: `001_baseline` already consolidated `002`–`058`, and `059`–`073` are all still required. **Nothing was dropped and nothing was deleted.**

Three tables needed a decision instead: `dbo.MenuItems` is empty and superseded but `MenuRepository` still reads *and writes* it in five places for the POS catalog sync (#744); `dbo.LayoutTemplates` is seeded and referenced by nothing, and looks like pre-seeding for the backlogged Board View (#709); `dbo.AuthorityRoles` has no direct references but is an FK parent of a live table and stays.

**Local integration tests are not broken — the environment is lying.** A full run is 115 failed / 8 passed, every failure `Login failed for user 'sqladmin'`. `VENU_TEST_AZURE_SQL_CONNECTION_STRING` is gone from `HKCU\Environment` and the machine key, as the last session recorded — but it is **still in the environment of any Windows process launched from the running WSL instance**, which captured it at WSL start. Forcing the run onto LocalDB gives **123/123**. `wsl --shutdown` clears it. This is also why 26 venues, 30 screens and 12 pairing codes created by test runs are sitting in the dev product database (#745).

**Executed evidence.** `Vennu.Api.Tests` Category=Unit 461/461 · `Vennu.Data.IntegrationTests` 123/123 on LocalDB · `Vennu.DataAccess.Tests` 229 passed / 3 failed, the same three that fail on clean master (#688) · `scripts/ci/test-classify-changes.sh` and `scripts/ci/test-verify-deployed-build.sh` both pass, the latter against a real HTTP server rather than a stubbed curl. Both wiring guards were observed red by removing what they guard. **UNTESTED:** the two signed-in Playwright cases against dev, still.

**Machine note.** Windows `dotnet.exe` at `/mnt/c/Program Files/dotnet/dotnet.exe` is how every test above was run; environment variables reach a Windows process only via `WSLENV`. **Corrected 2026-08-22:** the rest of this note said the WSL sandbox has no .NET SDK, no Node and no SQL client. Only the first is true. Linux Node v22.23.2 and Playwright 1.62.1 with a real Chromium are installed, and `SQLCMD.EXE` reaches LocalDB and dev SQL. The full inventory is in `AI_DEVELOPMENT_GUIDE.md` under *Local Toolchain*.

**Environment.** Scaled to B3 for the deploy and back to B1 after, as instructed. **The scale-down wedged all four SPAs** — 503, then no response at all — while the API stayed up. Restarting the four apps brought them back; Platform Operations took three attempts. This is the 28-apps-on-one-core problem the hosting sheet costs out, and it now has a reproducible trigger: scaling the plan restarts everything at once and B1 cannot absorb it.

**Exact next action.** Nothing is claimed and no milestone is approved. Unchanged from yesterday and now the oldest item on the list: run `specs/customer-onboarding.spec.ts` signed in against dev. After that, #737 (a changed provider subject locks a customer out permanently) is the defect that costs the most per occurrence, then #744, which is a decision rather than a fix.

## 2026-08-20 — End-of-session state and exact next action

**The whole journey works end to end on dev for the first time.** Sign in with Entra, onboard, pair a screen, build a menu, publish, see it on the display. Every step of that was broken this morning.

**Environment.** `appsrv-basic-web` is back on **B1 x 1 worker**. It was raised to B3 for this session's testing because the B1 wedged completely — 28 apps on one core, 84% memory at idle, and the API stopped responding entirely. Expect slow cold starts again (36s for Back Office, 49s for the API were measured on B1; both were sub-second on B3). Billing is hourly, so the B3 window cost pennies. **Hosting options are costed in a sheet the owner holds** (Azure retail prices, Central US Linux, 2026-08-20): the cheapest meaningful step is a second B1 so production stops sharing a worker with dev and stage deploys (+$13/month); note P0v3 is cheaper than S1 ($62.05 vs $69.35) with more than twice the memory and 20 deployment slots instead of 5.

**Deploy pipeline is materially different now.** One approval for a whole batch via a single `gate` job — the Azure credentials are repository secrets, so only the gate needs `environment: dev`. Jobs run sequentially, API first, because five parallel deploys on a shared worker killed three of them. Build configuration moved out of the workflow into `src/<app>/env/dev.env`, so editing the workflow now deploys **nothing**, editing an app's config deploys **that app**, and `scripts/ci/*` still forces a full run. Each build fails loudly if a `VITE_*` value is empty, because Vite silently substitutes an empty string and would ship a bundle calling `/api/...` on its own origin.

**No `*.azurewebsites.net` hostname remains in any URL** — not in the workflow, and not in any app setting across all 26 apps (swept and verified). `Cors:AllowedOrigins` on the dev API is now the three subdomains: back-office, www, display. Adding the display origin is what fixed the pairing screen; adding www closed #725.

**Standing rule from the owner: stop proving product behaviour with fakes.** The display bug survived for months because its unit coverage seeded `dbo.MenuItems` and asserted against it — a table with zero rows product-wide. Prefer integration tests against a real database; `DisplayBoardProjectionTests` is the pattern.

**Untested and known.** The two signed-in Playwright cases in `tests/ui/specs/customer-onboarding.spec.ts` still cannot run locally (#735: localhost is not a registered Entra redirect URI). Nobody has run them against dev now that sign-in works — that is the cheapest remaining verification and it would prove both the durable go-live fix and the headless onboarding path. Local integration tests need `VENU_TEST_AZURE_SQL_CONNECTION_STRING` unset; it was removed from the registry, but any shell started before that still carries a copy.

**Open issues from this session**, roughly by value: #740 a green `deploy-api` does not mean the new API is running (bit us twice) · #737 a changed provider subject locks a customer out permanently with an unhandled 500 · #729 the plan step dead-ends when Platform Operations has no selectable tier · #730 screen registration accepts a platform the heartbeat rejects with a 500 and a stack trace · #733 www falls back to a dev hostname in product code · #726 deploys never set version variables, which blocks any deploy self-verification · #736 (closed by `66a853df`) · #738 display diagnostics, deliberately future work · #739 fixed, but note the display cannot express a non-numeric price.

**Also worth knowing:** every visit to `dev.display.vennusign.com/pair` registers a NEW screen, and the venue has accumulated nine orphaned ones. Nothing cleans them up and nothing warns. Not filed yet.

**Exact next action.** Nothing is claimed and no milestone is approved. The highest-value next step is to run `specs/customer-onboarding.spec.ts` signed in against dev and act on what it finds; after that, #740 and #737 are the two defects that cost the most per occurrence. Resume only from an owner decision.

## 2026-08-20 — A menu built in the product reached a screen for the first time

Content authored in the builder, published, and rendered on a display. The full journey now runs end to end for the first time: sign in, onboard, pair a screen, build a menu, publish, and watch it appear.

```
menu: Weekday
sections: 1
  'Lunch' -> [('Tuna Fish', 5.0, available), ('Hamsandwhich', 5.5, available)]
```

**It had never worked, and the reason is worth keeping.** `DisplayController` read items from `dbo.MenuItems`. The builder writes content to `dbo.Items` joined to a board through `dbo.Placements`. `dbo.MenuItems` holds **zero rows in the entire database** — so every menu ever built in the product produced an empty board. `SliceSections` then discarded the empty section, so the API reported *no sections* rather than *one empty section*, which reads as "the menu is empty" instead of "the query is looking in the wrong place". That misdirection cost an hour of the investigation, and is the argument for #738's diagnostics view.

The same projection also ignored `MenuScreenAssignments` entirely, resolving content as "the venue's first active menu and all of its sections". Per-screen targeting and multi-page menus could not work; both were fixed together in `28730fab`.

**Why a green suite said otherwise.** The display's unit coverage seeds `dbo.MenuItems` and asserts against it, so it passed for months against a table the product had stopped writing. The owner's response to this is now the standing rule: **stop proving product behaviour with fakes.** The replacement is `DisplayBoardProjectionTests`, an integration test that builds a menu the way the builder does — `Items` plus `Placements` — and asserts the projection returns it, including the 86 case and that `dbo.MenuItems` stays empty throughout. A fake agreeing with a test says nothing about the schema.

**Two deploys lied today.** `deploy-api` reported success twice while the previous build was still serving — once hiding migration 073, once hiding this fix — and only a manual `az webapp restart` picked up the new code. A green `deploy-api` is not evidence the new API is running (#740).

## 2026-08-20 — A customer signed in to Back Office for the first time

**The first time anyone has reached Back Office as a customer, through the real sign-in.** Not a seeded session token, not a workbook link: Entra sign-in, customer session cookie, Back Office open. Every prior attempt in this product's life failed somewhere in that chain.

Five defects stood between sign-in and Back Office, and each was only visible once the previous one was fixed:

1. `email_verified` was required but Entra never emits it (`1d8337e7`).
2. An intermediate fix disabled the UserInfo call on a wrong theory, which dropped `email` instead (`ef906aa9`, reverted).
3. Identity resolution ran in `TokenValidated`, before UserInfo claims are merged — so it read `email` at the one point in the pipeline where it cannot exist (`03a39467`).
4. The session cookie was `SameSite=Lax` on an `azurewebsites.net` host while the SPA ran on `dev.back-office.vennusign.com`. Cross-site, so the browser withheld it: sign-in succeeded and the SPA got 401 (#731, fixed by `03e55efc` moving every app to its registered subdomain).
5. `authenticatedCustomerDestination` returned `/onboarding` to visitors already on `/onboarding`, so the page replaced its own URL forever (`720ce6bd`). This one had always been there; it needed a working session to reach it, so fixing 4 exposed it.

Two more were cleared the same evening to make the journey usable: `Cors:AllowedOrigins` carried only the Back Office origins, so the display application could not call the API at all and the pairing screen showed "Pairing unavailable" — adding `dev.display.vennusign.com` fixed pairing and, as a side effect, #725. And the B1 App Service plan wedged completely under 28 apps on one core; the plan is temporarily on **B3** for this testing round and returns to B1 afterwards.

**Durable lesson.** Four of these five reported as an authentication failure while the customer was, in fact, authenticated. When the symptom is generic, the question worth asking early is *where in the pipeline is this value read*, not *is the value configured* — configuration was correct for several of them the whole time.

## 2026-08-19/20 — Onboarding M1: durable go-live, headless QA onboarding, and a sign-in defect that blocks every customer

**The premise that "customer sign-in works on dev" is half true, and the missing half blocks everyone — issue #731.** The Entra handshake works and writes a real session; the Back Office SPA then cannot use it. `__Host-Vennusign.CustomerSession` is `SameSite=Lax` and set on `vennusign-dev-api.azurewebsites.net`, while the SPA is served from `dev.back-office.vennusign.com`. Different registrable domains, so every XHR the SPA makes is cross-site and the browser withholds a Lax cookie. Proof, one browser and one cookie jar immediately after a successful sign-in: a page-context `fetch` to …11321 tokens truncated…ughline is consistent: the
tests written with a fix prove the case its author had in mind and stop there,
rather than attacking the next step in the sequence — publish twice, assign a
put-away menu, change only the letter casing, shelve a menu between a take-off and
the publish that carries it. Review #6 is the sharpest instance: the trap it found
was written up as a *passing test*, which asserted the refusal it hit and never
asked what the screen was still showing. Reviews are catching what the author's own
tests do not. **Decide what to change about how work is verified before it goes to
review** — owner instruction. Taken up and completed 2026-08-09: the invariant sweep
and customer-visible acceptance assertions were adopted (see the remediation section
below) and the rules are folded into `AGENTS.md` §How to Work a Task and §Where a
test lives. This item is closed.

## Menus M1 verification remediation — 2026-08-09

Merged to `master` and pushed 2026-08-09 with `[skip ci]` (CI suspended by owner
decision; local verification was the gate).

**What this established.**

- **LocalDB is the default database everywhere**, in tests and in CI. Azure is reached
  only by setting `VENU_TEST_AZURE_SQL_CONNECTION_STRING` for that run. A gitignored
  `app.settings.json` used to supply an Azure connection string, so every "local"
  integration run silently went to a shared remote database: 96 seconds against Azure
  versus 4 on LocalDB, non-hermetic, and flaky in a way that read as product flakiness.
  The settings file is still read for its other toggles but can no longer choose the
  database. The fixture creates and migrates the database itself, so a fresh machine
  needs no setup.
- **A run with no database fails.** Fifty-three `if (!fixture.IsAvailable) { return; }`
  guards are gone. A suite that cannot reach a database is not a passing suite.
- **The in-memory repository decides nothing.** It re-implemented seven refusals in C#,
  and eight of the nine unit tests over it had a twin in the SQL suite — two under the
  same test name. It drifted, which is why review #6's defect survived 412 green unit
  tests. It is now storage plus an explicit failure seam: it is told when to fail and
  never judges. Refusals are asserted where they are enforced, in SQL.
- **Unit tests keep only what has no database in it**: the publish retry loop and its
  four-attempt bound, and refusal wording as the pure function it is.
- **`ModelInvariants` runs after every integration test**, against whatever state that
  test left behind, via the `InvariantCheckedTests` base class — no author action, by
  design. Seven rules, each traceable to the review that paid for it. It found a real
  defect on its first run: a publish could record a `ChangeCount` its own shipped set
  did not contain, because the two were separate parameters. The count is now derived
  from the shipped set inside the statement, so they cannot disagree; `PublishAsync` no
  longer takes it.
- **`GET content/screens/showing`** answers what a screen is showing, from the
  delivery rows and the published snapshot, never from the assignments. The milestone's
  central claim had no read behind it, which is why the demo could report 12 of 12
  while a screen sat stranded. The demo now asserts the screen at checks 4, 6, 8c and 8d.

**What was assumed.** That the screen/venue/pairing domains keep only the shared
invariants (tenant scope, one menu per screen) and get no domain rules of their own —
this work did not study them, and inventing invariants there would manufacture
confidence rather than earn it. Say so before adding any.

**Left deliberately, and for whom.**

- **The `sqladmin` password for `dev-vennusign.database.windows.net` is recoverable from
  this public repository's history** (added in `cf730c5`, removed in `05e35cc`). Removing
  the file did not unpublish the secret. **Rotate it on the Azure side** — owner action;
  no branch change fixes it.
- **Milestone 1's owner acceptance: recorded.** The owner accepted 2026-08-09 with the
  demo run (12 of 12, screen assertions asserting) as the acceptance record; no
  separate re-run was required.
- **Browser validation of rendered content waits for milestone 4** and is written into
  `milestone-plan.md` as a gate there. No screen work, no browser work.
- Door enumeration, one-read-one-lock for paired values, and records-in-the-same-commit
  were recorded as written guidance rather than gates, by owner decision.

**Open question.** `Measure-Api` in the demo runner had the same latent PowerShell array
trap that once produced a phantom assignment count, and it surfaced again here: 5.1
emits a JSON array as one object, and the shape changes with row count, so a reader
correct against one row starts lying at thirteen. All list reads now go through
`Expand-Api`. Other scripts in `scripts/` have not been audited for the same pattern.

## Migration chain squashed to a baseline — 2026-08-09

Merged to `master` and pushed 2026-08-09 with `[skip ci]` (CI suspended; local
verification was the gate — the four proofs below).

`src/Vennu.Data/Scripts/` holds one file: `001_baseline.sql`, the fifty-nine migrations
in the order DbUp applied them. Every statement in it already ran, so it is a collapse
rather than a rewrite. New migrations continue from 059.

**Deleting a migration never un-applies it.** DbUp decides what to run by journal name,
so a database that ran the old chain would see the baseline as new work and fail on its
first CREATE TABLE. `DatabaseMigrator.BaselineExistingDatabase` records the baseline as
applied wherever the superseded chain is already recorded, and executes nothing against
such a database. A database part-way through the old chain is **refused** with a message
telling the operator to finish on the previous release first — marking it complete would
leave it permanently short of whatever it never reached.

**Proved, not assumed.** A reference database was built from the old chain and
fingerprinted (1,166 lines: columns with types, nullability, defaults and collation;
indexes with filters and included columns; foreign keys with actions; check and default
constraints; seeded row counts). Results:

- fresh database from the baseline vs the old chain — **no material difference**;
- a database with the old chain journaled, migrated by the new code — **schema changed by
  0 lines**;
- a database stranded mid-chain — refused;
- a control, the old chain against itself in two databases — 0 differences, which is what
  makes the comparison trustworthy;
- eight concurrent migrations against one database — exactly one baseline row.

**Two pieces of dead work removed.** Script 012 created `dbo.MenuItemTranslations` and 058
dropped it; 013 added `MenuItems.AvailabilityResetUtc` with its index and 058 dropped
those. Every new database built both and demolished them. The baseline never creates them.

**One accepted difference.** Eleven tables declare `DEFAULT NEWID()` without naming the
constraint, so SQL Server generates the name from the object id. Creating one table fewer
shifts those ids, so a database built from the baseline carries different `DF__` names
than one built from the old chain. Nothing in the codebase reads a generated constraint
name. Naming them explicitly would make fresh databases deterministic and is worth doing
the next time this file is opened.

**A defect this introduced and then fixed.** The first version checked the journal and
then inserted as two steps. Startup calls the migrator concurrently, so the first real
database got seven identical rows, and adding a lock hint to the check still allowed two.
It is now serialised behind a named application lock — verified with eight concurrent
migrations. This is the third read-then-write race this session; the owner filed
"one read, one lock for paired values" as guidance rather than a gate, and the evidence
now argues for a gate.

**Still open.** `AuthorityRoles`, `AuthorityRolePermissions` and `LayoutTemplates` are
created and seeded but read by no product code — only by a test asserting the script's
text. That is Track 1 scoped-authority work and owner-closed, so it was left alone rather
than judged on grep. If they are genuinely dead, the correct removal is a **new** migration
that drops them, so existing and fresh databases converge; deleting them from the baseline
would only change new databases.

## Milestone 2's first design decision — the theme model

Owner correction, 2026-08-09, recorded against Q86 and Q98. **Menu themes and shell themes
are categorically different things**, and the code currently confuses them.

- A **menu theme** is attached to a menu. A venue may have many. None exist yet; they are
  built later in the theme editor. Milestone 2 ships **no named looks** — the render
  engine consumes a theme definition so later themes need no engine change.
- A **shell theme** is the software's own look — today's sky blue, a dark variant later.
  That is what "venue theme" should mean, and it is **milestone 2's actual theme
  deliverable**: nav rail, tokens, chrome. One ships, built so others can be added.
- **A menu with no theme attached is a valid state.** The engine renders it — plainly and
  badly, which is acceptable — but never blank, never a silently invented fallback, never
  a failure.
- **A menu theme is created in the theme editor and attached in the menu editor.** The
  menu editor never authors a theme. The theme editor (`ThemeBuilder.tsx`, route `themes`)
  is the existing surface.
- A venue-name title strip on the TV, if it exists, belongs to the **theme editor**. The
  Menus render engine neither draws one nor assumes one.

What the code says today, which contradicts that:

- **No menu-theme table exists.** `git grep -cE "CREATE TABLE dbo\.(MenuThemes|BoardThemes)"`
  against the baseline returns 0.
- **`Menus.Theme` is free text** — `NVARCHAR(40) NOT NULL DEFAULT N'coastal'` — naming a
  look that was never built, with no table behind it. Since an unthemed menu is a valid
  state, `NOT NULL DEFAULT 'coastal'` is now wrong twice over: it forbids the blank case
  and defaults to a fiction. Whatever the model becomes, that column changes.
- **`dbo.VenueThemes` holds board-render fields** (`BoardBackgroundColor`, `SectionColors`,
  `GlowColor`, `TitleFont`, `ItemFont`): menu-theme data under the venue-theme name, one
  row per venue. Read by `DisplayContentResponse` and by the back-office and
  platform-operations theme contracts, so moving it is not free.

This is the recurring shape — one name carrying two meanings, and a value with no referent.
**Settled — owner decision, 2026-08-09: milestone 2 defers the MenuThemes table.** The
table arrives with the first milestone that reads one (M3's picker / the theme editor),
so its shape is designed when its real user exists. M2 ships migration 059 making
`Menus.Theme` an honest nullable attachment slot: default dropped by dynamic constraint
lookup, `'coastal'` removed from rows **and** stored snapshots (else every menu wakes
with a phantom theme draft change), and `RestoreSnapshotSql`'s `ISNULL(t.Theme, m.Theme)`
fixed so a null theme restores as null — regression test verified to fail with the fix
reverted. `VenueThemes` keeps its board-render fields untouched until the milestone that
moves them.

## Milestone 2 readiness pass — 2026-08-09

The owner asked that M2 be put through the dev process before coding. Three exploration
sweeps (frontend/shell, design authority, content API/test harness) plus a structural
design pass. Findings and decisions, all recorded in issue **#687**:

- **Owner decisions:** defer the MenuThemes table (above); the render engine lives at
  **`src/board-engine/`** — a new top-level shared folder, imported by relative path
  from back-office in M2 and the display player in M4 (the platform-operations
  cross-app import is the precedent). The engine imports nothing from either app; data
  arrives as props.
- **"Spine" is retired; the model is named "content"** (owner, 2026-08-09). The data
  model and API are *content* — items, placements, availability — and "menu" is the
  operational context using it, which the capability IDs already said
  (`content.item.update`, `content.menu.manage`). Landed as milestone 2's step 0,
  before any frontend client was written against the old name: route
  `api/back-office/menu-spine` → **`api/back-office/content`**;
  `BackOfficeMenuSpineController` → `BackOfficeContentController`; `MenuSpineService`
  → `ContentService`; `MenuSpineContracts` → `ContentContracts`;
  `IMenuLibraryRepository`/`MenuLibraryRepository` → `IContentRepository`/
  `ContentRepository`; `FakeMenuLibraryRepository` → `FakeContentRepository`; the test
  classes and the demo runner with them. **Historical names stay as history**:
  milestone 1's title, the `feature/menus-m1-spine` branch, PR #685, the
  `058_create_menu_item_library_spine.sql` header inside the frozen baseline, and the
  recorded register answers are not rewritten.
- **Step gates, not a testing phase** (owner, 2026-08-09). Tests are written with each
  step and each step ends on its own green gate before the next starts — schema on both
  a fresh and a previously-migrated database; the API exercised with real requests
  before any UI consumes it; the engine on its render invariants; the shell on both app
  builds plus existing nav specs; the shelf on new Playwright specs. The full local
  gate, review and workbook run at close. Recorded on #687.
- **Backend gaps M2 must fill before the shelf UI can be honest:** no frontend client
  for the content API exists at all; no menus-list read (the legacy `GET /menus` drags
  every section and item and loses "MP" price fidelity); nothing exposes a published
  snapshot to render; `HistoryEntryResponse` carries no `Version` so Go back to… is
  unreachable; no duplicate operation exists (semantics owner-settled in Q20). Route
  shapes are in #687.
- **Named to settle inside the milestone, not silently:** the never-published card
  state, and the Duplicate name-collision/length default.
- **Test facts:** the 20-screen/13-menu seed (Q176) does not exist — it enters as
  `POST /api/test/seed/scale` composing product write paths against a dedicated scale
  venue, never fixture SQL that re-implements snapshot JSON. `navigation-shell.test.mjs`
  hard-codes the current 9-route/4-group nav and changes with the rail, deliberately,
  in the same PR. The running 18-criteria checklist now exists at
  `docs/features/menus/acceptance-criteria.md`.
- **Token batch-2** now has its artifact:
  `docs/design/approved/menus/proposed-token-additions-batch-2.css` (Q178, including
  the `#2a78d6` selection token; board palette deliberately excluded — it belongs to
  menu-theme definitions).
- **Stale records corrected in this pass:** this file's "not pushed" notes (both
  batches are on `origin/master`), the provisional Q86/Q98 framing, the completed
  retrospective instruction, the register header's "Deferred: Q86", the design README's
  five-item card menu (Q195), eyebrow colour (Q184), icon instruction (Q185) and
  criterion-4 wording (Q187), the batch-1 token file's "NOT APPROVED" header
  (build-decision 8), and `PROJECT_STATUS.md`'s validation policy (CI suspended).

## Milestone 2 — merged and accepted — 2026-08-10

PR **#689**, issue **#687**, branch `feature/menus-m2-shell-render` (deleted). 84 files,
+6,563/−311, in thirteen commits: step 0 retired the word "spine", then schema, content
API, engine, shell, shelf, browser specs, workbook, critique, review fixes. Everything
before the merge from master carries `[skip ci]` (CI suspended by owner decision).

**What it delivers.** The 76px icon rail and shell theme; `src/board-engine/` — a pure
board renderer shared by both apps, laid out once at 1920×1080 and scaled; Menus home,
where every card is a live render of what that menu's screens are showing, with the
scale cutover at seven; the six card actions; four new content reads/writes; a
20-screen/13-menu Playwright fixture; and criterion 18's named spec.

**Owner acceptance, 2026-08-10.** 11 of 11 Pass across four journeys, closure "Accept
Milestone 2". Record kept verbatim at `docs/features/menus/m2-acceptance-record.json`,
including the owner's screenshot. Criteria 5, 6, 8 and 18 are now confirmed against the
running build, not only by their specs. One note came out of it — **Q209**, deferred.

**What the review process has established, and it is worth carrying into M3.** Browser
and screenshot verification caught four defects that unit tests did not, and three of
them were the most serious in the milestone: boards that rendered blank because board
type had been set from card-sized measurements, never-published cards claiming "5
changes not published", a locked chip spilling out of the rail, and a filter counting
three while the shelf drew two. This is the same failure mode M1's retrospective named.
Owner instruction, recorded: **from M3, browser assertions ship with the surface, not a
step later.**

**The second review was waived — owner decision, 2026-08-10.** The first review closed by
requiring a fresh review of the resulting head; the owner judged that review sufficient to
close the milestone and declined a second. Recorded plainly because the consequence is
real: the three fixes at `4c61aa2` were verified by their own tests and by the owner's
acceptance run, but they were not themselves independently reviewed. Milestone 1 needed
five reviews and milestone 2 needed one, which is the evidence the owner weighed.

**Known and deliberate.** Four test failures sit outside the Menus suites — the three on
**#688** plus an E2E pairing assertion found in this run, all four verified pre-existing
by stashing every M2 change and re-running. `#688` now covers all four; neither suite is
in the routine gate, which is the actual defect.

## Milestone 3 readiness pass — 2026-08-10

The owner asked that M3 go through the dev process before coding, as M2 did, and — being
asleep — that ambiguity be resolved by judgment rather than left blocking. Every call
below is **provisional and cheap to overturn**; each names its reasoning.

### The complete user behaviour

*A person opens one menu, changes what it says — a price, a name, an item, a section,
the order things sit in — sees the board change as they type because the canvas is the
board, and then decides, deliberately, to put it on the screens.*

Immediately before: the shelf card (M2). Immediately after: the screens, via Publish;
or back to the shelf via the breadcrumb. The same behaviour lives in three other places
that must agree — the shelf's card render, the publish diff, and (from M4) the TV.

### Path map

**In:** card click · `#/menu/{menuId}` deep link **(new — the builder gets an address)** ·
back from Play · redirect after create (M6). **Unvalidated today:** all of them; the
builder does not exist.

**States that must render:** empty menu (no sections) · section with no items · item with
no price (quiet flag, publish not blocked, Q113) · 86'd item (selectable, editable,
red-tinted panel, Q104) · nothing selected (inspector holds its place, Q106) · never
published · put-away menu open for editing · loading · API error · save failure (amber
byline, retry, Publish blocked, Q197) · 401 mid-edit (holds the change, sends after
sign-in, Q199) · permission denied · no screens paired ("No screens yet", Q101).

**Refusals the UI must speak:** ceiling reached (items per menu) · name blank or >200
(reverts on blur, Q119) · description >1000 · publish conflict (a screen another menu now
owns) · publish while a save is unconfirmed · stale act after someone else published.

**Out:** Publish · breadcrumb to the shelf · Play (visible, honest blocked state, Q102) ·
browser refresh mid-edit · leave and return.

### Invariants M3 gains

- **An item appears at most once on a board.** The schema enforces once per *section*
  (`UQ_Placements_SectionItem`) but not once per *menu*, so Q112's "picking it jumps
  instead of duplicating" is currently a UI promise with nothing behind it.
- **No two placements in a section share a sort order** — otherwise board order depends
  on a tiebreaker nobody chose.
- **A deleted section leaves no placement behind**, and never deletes an item.
- **Every placement's section belongs to its menu** — already enforced by
  `FK_Placements_SectionOnMenu`; asserted so a future schema edit cannot quietly drop it.

### Ready

- The derived-draft model means **the builder needs no draft plumbing at all**:
  `MenuSnapshot.Diff` already compares name, theme, dwell, loop warning, screens,
  sections, items and placements, so every builder edit produces its own draft change
  and the count cannot disagree with what Publish ships.
- `IContentRepository` already carries most of the writes: `CreateItemOnMenuAsync`,
  `CreatePlacementAsync`, `RemovePlacementAsync`, `ReorderPlacementsAsync`,
  `UpdateItemAsync`, `GetItemsAsync` (the library search), `GetPlacementsForItemAsync`
  ("also on Late Night"), `GetWorkingSnapshotAsync` (the canvas's board).
- The board engine renders the canvas as-is — `BoardSurface = "preview"` already exists
  for the annotations flag (Q135).
- Design authority is production-detailed: four columns at 212/flex/296, the six
  inspector controls, the publish bar, the selection ring `#2a78d6` (already a token).

### Decisions taken in this pass

1. **Design follow-up 1 is Q5, not Q76** — the milestone plan cites the wrong question.
   Q76 is refresh cadence; **Q5** carries the flag ("the editing flow must feel easy —
   possibly a quick price-change mode — design follow-up required before slice 3 builds
   the inspector flow").
   **Resolved without inventing a mode:** a shared item's inspector states the fact
   quietly and permanently under the price — *"Also on Late Night and Brunch — they show
   the new price when you publish them"* — reusing Q123's locked vocabulary (two names,
   then "on 3 boards"). No dialog, no confirmation step. A modal on every price edit is
   the opposite of "feels easy", and a separate quick-price *mode* is undesigned, named
   in no milestone, and would be the second editor that decision 15 and M2c's read-only
   rule both exist to refuse.
2. **The builder gets its own address**, `#/menu/{menuId}` — closing the note M2 left in
   `App.tsx`. Refresh mid-edit and Back both survive, which the DoD navigation group
   requires and today's `editingMenuId` React state cannot do.
3. **Menu themes: still no table, and no attach write.** The picker ships and shows
   Q86's empty state from `GET content/menu-themes` → `[]`. A theme that cannot exist
   cannot be attached, and creating an empty table with no writer repeats exactly the
   dead-schema problem the migration baseline flagged (`AuthorityRoles`,
   `LayoutTemplates`). Table and attach land with the theme editor that first writes one.
4. **`BackOfficeMenusController` is retired**, its builder-relevant writes consolidated
   onto `api/back-office/content` — one base for one model, finishing step 0's rename.
   `run-m1-demo.ps1` and `BackOfficeMenusControllerTests` move with it. The legacy
   `MenuSectionsEditor`, `MenuItemsEditor` and `QuickUpdateMode` components go in the
   same PR, with their specs **rewritten, not deleted** — `menu-save-race.spec.ts` guards
   a real stale-overwrite race and must be re-expressed against the builder's save model.
5. **Sections are deleted, not archived** (Q96). `MenuSections.IsActive` loses its last
   writer; the migration hard-deletes any `IsActive = 0` section and its placements,
   names what it discards, and drops the column. Leaving the column and its
   `IsActive = 1` filters would mean a future writer of 0 silently changes a live board.
6. **Reorder becomes one guarded write.** Both section and placement reorder today read
   the current set, validate completeness in C#, then write — unlocked. A concurrent add
   between the two makes the write describe a set that no longer exists. This is the
   **fourth** instance of this codebase's most common defect shape ("two values that must
   describe the same instant are read once, under one lock").

### Gaps M3 must close before the builder can be honest

- No working-board read: the canvas needs the menu **as it stands**, not the published
  board the shelf draws. `GetWorkingSnapshotAsync` exists; nothing exposes it.
- No section delete-that-releases-placements; no placement remove wired to a UI;
  no library search read; no "which boards is this item on" read.
- `ReorderPlacementsAsync` trusts a partial list: omitted placements keep stale sort
  orders and can collide. The service validates completeness, but outside the write.
- `MenuItemManagementService.ReorderAsync` reports "Menu section does not exist" for a
  section that exists and is merely empty.
- Undo/redo has no model. Design: every builder mutation is a command carrying its
  inverse; ⌘Z issues the inverse write; session-scoped, capped, never persisted, never
  named in settings (decision 7). A failed inverse says so rather than clobbering.
- ⌘K (Q121), the "Viewing as" list (Q101), the bulk-place drawer (Q95/Q124) and the
  publish bar's per-screen chips with the ≤6 cutover (Q161/Q167/Q168) have no code.

### Records that state something untrue (fixed at M3 start)

- `milestone-plan.md` design follow-up 1 cites **Q76**; the flag is **Q5**.
- `README.md` (design authority) M2 inspector still lists "two checkboxes (Feature on
  the board, Add a photo)" and calls them "**Six controls total**" — Q107 and Q108 put
  both out of scope, so the inspector has four.

## Milestone 3 — built and gated — 2026-08-10

Branch `feature/menus-m3-builder`, issue **#690**. Three steps, a gate each, then a
critique pass and the full gate.

**What it delivers.** The four-column builder at its own address `#/menu/{menuId}`:
a section rail that navigates, a canvas that IS the preview, an inspector of four
controls, and the publish bar. Adding items with search that jumps rather than
duplicates. Undo and redo. ⌘K over the board. The theme picker's honest empty
state. Migration 061, which moved two rules the product only promised into the
schema. Nine content endpoints where every rule is decided inside the statement
that writes it.

**Retired with it**, per AGENTS.md: `MenuSectionsEditor`, `MenuItemsEditor`,
`QuickUpdateMode`, the legacy section/item routes and their client, and four
back-office specs — with their specs **rewritten, not deleted**.

**Six decisions taken by judgment**, all provisional and recorded in #690, because
the owner asked that ambiguity not block progress overnight. The one most worth an
owner's eye is Q5's design follow-up: shared-price editing "must feel easy", and
the resolution was a quiet statement of fact under the price rather than a
confirmation step or a separate quick-price mode.

**What the browser caught that 190 unit tests could not**: board type six times too
large, a second page header stacked over the builder, a selection ring that never
drew, duplicate exports the typecheck passed and the bundler refused, and — from
milestone 2's shelf — an amber strip that swallowed clicks, so the one menu you
most wanted to open was the one you could not.

**The critique pass** against `docs/design/approved/menus/README.md` §M2/§M2a found
five gaps and all five are closed: redo was a disabled button, "Viewing as" was a
label rather than a dropdown, the 86 note had no time, and "go back to…" and
"Review first" were missing from the publish bar.

**Not shipped, and named:** the overflow warning ("Two words over — wraps to 3
lines on Patio") needs reported screen geometry, which arrives in milestone 4.
Cross-section drag waits for milestone 5 (Q103) — **within-section item drag does
not, and was missing until the review; it ships now.** Keyboard reorder (#672) and the
full add-row keyboard flow (Q122) stay backlogged; the rail keeps its ↑/↓ buttons
until #672 lands, because replacing them with a drag handle first would remove
reordering from keyboard users entirely.

**Gate.** 433 API unit · 89 data integration on a fresh migration with the
invariant sweep · 190 back office · 98 platform operations · 118 Playwright across
desktop and mobile · 21/21 builder API checks over real HTTP · M1 demo 12/12. The
four failures on **#688** remain, all pre-existing.

## Milestone 3 — what the independent review cost — 2026-08-10

The owner's chosen agent reviewed PR #691 at `d521cd4` and returned
**REQUEST_CHANGES with seven findings. Every one was real**, verified here before
anything was changed. An eighth was found in the review prompt itself. All eight
are fixed at `b59d2d1`, each with a Playwright spec that was **run with its own fix
reverted and observed to fail**.

**The gate had a hole exactly the shape of the worst finding.** `npm run build`
failed with three `TS2322`s on `inert`, and nothing ran it: `validate.ps1` built
`src/display` and had never heard of `src/back-office`. 190 unit tests and 130
Playwright specs were green against a branch whose back office did not compile,
because the dev server transforms per module and never type-checks the project.
`validate.ps1` now builds and tests **both** front ends. That is the durable fix;
the `inert` typing itself is one declaration file.

**Two recorded answers were named in the milestone plan and simply not built.**
Q197 (a failed save retries automatically, Publish waits) and Q199 (a 401 shows a
sign-back-in prompt, holds the change, sends it after). Both existed as copy — the
amber byline was drawn — with no mechanism under them. A byline that says
"retrying…" while nothing retries is worse than an error, because it is a promise.

**Undo was a blind overwrite.** It sent the whole previously-captured row
unconditionally, so undoing your own price edit erased a colleague's name,
description and price along with it, silently. Inverses now carry the values they
expect to find, compared under the lock that writes; `item_changed` comes back in
the server's words. The catch block had claimed this protection for weeks — it
could never fire, because an unconditional write does not fail.

**Three named M3 behaviours were absent**: Q96's rename-by-clicking-the-canvas-
heading (the heading was a `<p role="presentation">`), Q124's "Add many at once"
drawer, and Q103's within-section item drag — where the pill was drawn correctly
on hover and on selection all along and dragged nothing, with `reorderMenuItems`
imported by the builder and called from nowhere.

**Two lessons worth carrying, both about evidence.**

1. *A do-not-file list is an authority claim, and mine was wrong.* The review
   prompt told the reviewer that item drag was milestone 5 "per Q103". Q103 defers
   only **cross-section** moves. An in-scope gap was placed out of bounds, in the
   same document that warned the reviewer against citing a recorded answer they
   had not read. **Quote the register entry into the prompt itself** — if the words
   have to be pasted, the mistake cannot survive being written down.
2. *A spec can pass against the defect it names.* The first version of the 86-note
   spec 86'd two items and asserted each had a note — which the one-shared-string
   bug also satisfies, because both rows got a note. It needed a test-support
   endpoint to backdate one 86 before the notes could differ at all. Written and
   run, it would have been evidence of nothing. **Revert the fix and watch the spec
   fail** is not a formality; it is the only thing that distinguishes the two.

And one the browser caught that nothing else could: moving the 86 notes into a
`useMemo` below the loading early-return changed the hook count and blanked the
entire application. 190 unit tests passed and the production build completed while
the app rendered nothing at all.

## M3-A Slice 1 reconstruction — 2026-08-11

Independent review established that Slice 1 had been tested only in an uncommitted
working tree at Slice 0 SHA `179de5f`. All 49 dirty files were first preserved in
local safety commit `4aa0168`; the reviewable Slice 1 tree was then reconstructed on
`feature/menus-m3a-s1-pages`. Partial Slice 2 page-history/section-reassignment work
and the Slice 6 import landing remain only in that safety commit and are not claimed
as shipped. Issue #696 owns Slice 1.

The work-plan dependency was made explicit: page-shaped Test API seed support lands
with Slice 1 because the separate Test API must delegate to the real page schema and
product endpoints introduced here. The review's test-integrity findings are closed:
page reorder uses a stepped real pointer and was observed failing with `onDrop`
disabled; the dead/mojibake selector is gone; Q181 singular/zero copy is enforced;
and browser coverage now includes populated deletion, Cancel, copied content and
cross-menu naming. LocalDB now asserts exact FK/unique SQL errors and concurrent
page-item uniqueness. Pre-commit focused evidence: page Playwright 12/12, page
LocalDB 2/2, back office 196/196 and production build. CI remains suspended.

Exact next action: commit and push Slice 1, open its PR, rerun gates at the committed
SHA, then obtain an independent exact-SHA review before owner testing.

## Boundaries

- Milestones 1 and 2 are merged and accepted, so milestone 3 may start. Milestones 4–6 stay closed until their predecessor is merged and accepted in turn.
- Do not revive any cancelled track, phase or void work package without fresh owner approval.
- Do not implement backlog issues #670–#683 without owner scheduling.
- Design follow-ups (milestone-plan §Design follow-ups) must be resolved before the milestone that consumes them.

## Milestone 3 owner-acceptance remediation — 2026-08-10

**What this established.** The owner's first workbook findings are implemented.
Available items now show a plain availability switch; only the 86 state is a red
panel. The visible board handle is a real hit target, pointer drag works at human
speed, a scale-correct insertion line follows it, and the order survives refresh.
Section delete lives on each rail row, keeps its confirmation and library-release
message, selects the previous surviving section (or the first if the deleted row
was first), and leaves the empty-board add affordance when appropriate. Canvas-
heading rename is the only section-name editor. Case 6 is pre-seeded with Harbor
Lemonade on Acceptance Menu and Harbor Evening Menu; case 15 uses on-screen Undo.

**Evidence.** Each product regression was observed red against its unfixed code:
the available panel existed; a slow drag from the visible handle failed while a
row-centre drag passed; the rail-row delete did not exist; the duplicate field did;
and neutralising the deletion fallback left every remaining rail row unselected.
Restored fixes pass. The fixture ran twice and returned exactly one shared placement
on each named menu. Final local gate: back office 190/190 plus production build;
Playwright 142 passed / 12 explicit skips; builder API 21/21; M1 demo 12/12;
Data integration 91/91; .NET Debug retained only #688's known DataAccess 228/3 and
API 433/1 failures; .NET Release solution and display production builds passed.
CI remains suspended and was not used as a gate.

**Assumed and deliberately bounded.** The owner reaffirmed that mobile interactions
are out of scope (Q158/#681), so desktop handle drag and bulk placement are the M3
interaction gates; their mobile Playwright variants are explicit skips. Existing
mobile crash/layout coverage stays. Keyboard remains out of scope exactly as already
recorded; existing handlers were not removed. D1's truncated visual notes were not
chased, by owner instruction.

**Left for the owner.** Rerun `docs/features/menus/m3-acceptance-workbook.html`.
The existing record remains "Needs adjustment" and M3 does not merge on it. After
this set is handed back, provide the deferred visual notes for the promised second
pass. Milestones 4–6 remain blocked.

## M3-A Slice 1 owner-remediation — 2026-08-11

**What this established.** The page rail, overflow section picker, populated-page
deletion, and screen-assignment workflow were reworked from the owner's 4 Pass / 3
Needs Adjustment record. Add-page naming now uses the page-tab visual language;
section chips stay on one line with long names truncated and overflow in a bounded
More menu; populated deletion explicitly offers move or delete-sections while
retaining library items; and assignment management is a viewport-bounded,
scrolling panel with screen geometry/current page context, staged Save/Cancel,
and a recoverable nested rotate/replace choice. Delete-sections is enforced by the
product API/repository transaction, not only presented by the UI.

**Evidence.** Each new customer-visible regression was observed failing with its
production fix removed, then restored: page-name typography, non-wrapping chips,
delete-without-moving, and nested-choice focus recovery. Final local evidence:
back office 196/196; production build passed (existing Vite chunk advisory only);
desktop `menu-pages.spec.ts` 14/14; focused API 5/5; Test API 8/8. The Release
solution build passed with 21 pre-existing warnings. Azure/external integration
tests were skipped under the standing owner exception; the LocalDB deletion
regression was added but is UNTESTED in this run.

**Exact next action.** Push the committed remediation head to PR #697, obtain an
independent exact-SHA review, then regenerate the owner workbook against that SHA
and rerun only the three previously adjusted cases plus one surrounding-flow check.

## M3-A Slice 1 independent-review remediation — 2026-08-11

**What this established.** PR #697's exact-head REQUEST_CHANGES findings were
fixed across their full paths. Migration 062 now separates the placement column
addition from its carry update; legacy assignment test callers carry an exact page;
restore again refuses a screen acquired by another menu while preserving valid
cross-menu rotations; and snapshot expectations use the screen-plus-page identity.
Page deletion keeps the decision open after a move conflict and can recover by
deleting the page's sections. Assignment Save keeps staged choices after a refusal,
retries only transient failures, and exact-pair removal is idempotent. Capacity has
an inspectable Check fit result; six-section overflow, long-name identity, screen
location/status and cross-menu labels match the approved page workflow.

**Paths and evidence.** New, existing, duplicate-placement, populated/empty delete,
move conflict, Cancel, failed Save, retry-safe removal, same-menu and cross-menu
assignment, unassigned/landscape/portrait capacity, section overflow and focus
recovery are covered. The corrected migration carried a customer-shaped legacy
menu, section, item, placement, screen and assignment into Page 1 and
`DBCC CHECKCONSTRAINTS` returned no violation; the disposable database was removed.
Release solution build passed with existing analyzer warnings and no errors; back office 197/197 and
production build passed; desktop page Playwright 16/16; Test API 8/8; focused
snapshot tests 17/17; LocalDB data integration 94/94. DataAccess remained at its
known #688 baseline, 228 passed / 3 failed. CI remains suspended.

**Boundaries.** The untracked owner workbook was not modified or committed. Azure
and other external-service tests remain skipped under the standing exception.
Future tier authority and theme-authored fit measurements remain owned by their
later milestones; Slice 1 continues to use the documented maximum-tier defaults
and its deterministic fit model.

**Exact next action.** Commit and push this remediation to PR #697, obtain a fresh
independent exact-SHA review, and only after approval regenerate the owner workbook
for owner acceptance.

The exact-SHA review then found that Screen Assignments Save still issued one HTTP
write per screen. It now sends one batch to a single database transaction: every
screen and mode is validated before any assignment changes, replace/rotate/remove
and attributable history commit together, and any stale screen or refusal rolls the
whole Save back. LocalDB asserts a valid first change plus an invalid later screen
leaves no assignment behind; the browser regression keeps the staged UI recoverable
and verifies the prior screen owner remains unchanged.

## M3-A Slice 1 owner closure — 2026-08-11

The owner explicitly approved Slice 1 after an extended live visual pass against
the final nine exported M3-A screens. The page rail, section rail, canvas scrolling,
connected Screen Assignments surface, and canvas inline editing were reconciled in
the real browser. Section headings and every item name, description and price now
edit in place using the rendered theme typography. The exhaustive browser regression
creates three sections with twelve items each, edits all 111 fields while scrolling,
then refreshes and verifies persistence. Removing the canvas scroll-coordinate fix
made the test fail with displacement exactly equal to `scrollTop`; restoring it
returned the test to green.

Some section CRUD behavior assigned to Slice 2 was delivered early at the owner's
direction during this acceptance pass: section selection, inline rename, add,
real-pointer reorder, populated delete with reassignment, and delete confirmation.
Slice 2 must gap-audit and reuse it; it must not rebuild or double-claim it. Page
history remains Slice 2 work.

Local closure gate on the accepted tree: Release solution build passed with 0
warnings and 0 errors; back office unit 197/197 and production build passed;
LocalDB data integration 97/97; desktop `menu-pages.spec.ts` 18/18; desktop
`menu-builder.spec.ts` 40/40. The API suite passed 436/438: the Azure credential
test is excluded by standing owner policy and the existing #688 pairing-layout
expectation remains the other known baseline failure. Mobile and external-service
tests were not run by owner scope. CI remains suspended.

Exact next action: commit and push the accepted Slice 1 tree, obtain a fresh
independent exact-SHA review on PR #697, then merge and release the tracker claim.

The owner then explicitly waived any further independent review and directed that
Slice 1 be closed and Slice 2 begun. The interrupted review found no product
failure, but identified two controlled-record inconsistencies. They were corrected
before merge: owner override O1 in `decisions.md` and the action inventory now
authorize the accepted canvas inline editing, and the obsolete generated workbook
that named an earlier SHA and stale seeded menu was removed. The owner-approved
product candidate remains `1c52c2658966864d175b8666b0fc4722197afe92`;
the closure commit changes authority/acceptance records only.

## M3-A Slice 3 — implementation checkpoint — 2026-08-12

**Claim.** GitHub issue #703 is open. Branch
`feature/menus-m3a-s3-board-add-item` was created from `master` at `8bbafc2`; the
tracker claim is committed at `14600fd`.

**What is established locally, not yet complete.** Migration 066 widens the page
history vocabulary for item add/reorder/move/remove. Existing create, place and
reorder transactions now write page-attributed history; a guarded cross-section
move validates both live section orders and moves/history-logs atomically; removal
is page-scoped and preserves other-page placements. The existing board UI now calls
those paths, supports cross-section and empty-section drops, confirms “Remove from
this page” naming the page, accepts name then optional price in the add row, and
evaluates capacity against the typed draft item. Accepted board, geometry, fit,
selection, inline editing, inspector and page-history surfaces were reused.

**Executed evidence.** Back-office unit tests passed 197/197. Back-office production
build passed with the existing Vite chunk advisory. The Debug solution build passed
after updating the legacy removal regression. Focused LocalDB integration tests
passed 3/3: cross-menu preservation, cross-section atomic move/history, and same-menu
cross-page removal isolation. The first database attempt selected the ambient
`VENU_TEST_AZURE_SQL_CONNECTION_STRING` and correctly failed authentication; the
successful run cleared that variable for the process and used LocalDB. No credential
value was read or printed.

**Not done.** API/controller refusal and permission tests, full invariant write-path
coverage, complete desktop Playwright scenarios, exact UI styling/focus recovery,
real-browser inspection, red-with-fix-reverted demonstrations, Release/full local
gates, acceptance workbook, independent review, owner acceptance, push/PR and merge
are all outstanding. Nothing in this checkpoint is accepted or ready to merge.

**Exact next action.** Add focused API tests for create/place/reorder/move/remove,
including stale orders, cross-page/cross-venue identifiers, idempotent removal and
author/history mapping; then finish the desktop Playwright path matrix before the
bounded Impeccable browser pass.

## M3-A Slice 3 — gate and first-review result — 2026-08-12

Implementation candidate `a110bf51205ff31f428f283438caf047b00f4dd2` completed the
pre-review process in the owner-specified order: Release solution build, back-office
198/198 unit tests and production build, focused API 56/56, focused LocalDB item
rules/invariants, all 214 discovered Playwright cases across an isolated fixture
(Menus mobile explicitly skipped under Q158), and a browser visual audit against
the approved M3-A authority. The visual audit caught and corrected an add-result
popup clipped by the publish bar. The full gate also repaired stale fixture PageId,
isolated screen-key, token and screen-id assumptions. CI remains suspended.

The independent exact-SHA review then returned REQUEST_CHANGES, as intended: literal
search did not satisfy punctuation/spacing near-match, malformed source orders could
include the moved item, remove Undo appended instead of restoring order, route-boundary
coverage was incomplete, and controlled records were stale. Remediation now adds
canonical punctuation-insensitive ranking and a visible selected suggestion, resolves
the Enter/search race on both name and price, refuses duplicate/malformed guarded
orders, restores exact order on remove Undo with compensating removal on refusal,
and adds real-browser malformed/permission and Undo/Redo paths. Focused remediation
Playwright is 5/5 and focused LocalDB search/move refusal is 3/3.

**Still outstanding.** Rerun the affected/full gates on the remediation tree, commit
a new exact SHA, obtain independent re-review, then prepare the owner acceptance
workbook. No owner acceptance, PR, push or merge has occurred.

**Exact next action.** Complete remediation validation, commit the new candidate,
and send that exact SHA to the independent reviewer.

## M3-A Slice 3 — second-review remediation — 2026-08-12

The independent re-review of `c5679427d890a2d05e6824c6c55cd38f76012583`
returned REQUEST_CHANGES for one destructive concurrency window, incomplete active-
suggestion semantics, and records that still described already-completed gate work as
outstanding. Remove Undo/Redo now uses one database-guarded transition: it proves the
exact expected section order and page-wide absence/presence under the same locks that
insert or delete the placement and write history. A second actor's re-add, move,
reorder or removal therefore returns `order_stale` without changing placement or
history. The add input now exposes a combobox controlling a listbox with an explicit
active option.

Executed on the remediation tree: Release solution build succeeded; back-office unit
tests passed 198/198 and its production build succeeded; focused LocalDB stale Undo,
stale Redo and adjacent item rules passed 3/3; focused desktop Playwright passed 4/4,
including the accessible relationship and the second-actor stale Undo. A single-worker
68-case desktop attempt passed its first 29 cases, then LocalDB began aborting concurrent
session reads and the remaining cases failed at application setup; it is infrastructure
evidence, not a product pass, and is not counted. The earlier complete 214-case isolated-
shard gate remains the broad regression evidence; the changed paths have fresh focused
coverage. CI remains suspended and Azure/external integrations remain owner-exempt.

**Exact next action.** Commit this remediation candidate and obtain independent review
of that exact SHA. Owner acceptance remains after approval; no push, PR or merge has
occurred.

The third independent review requested keyboard arrow navigation for the add-result
combobox. The owner reaffirmed on 2026-08-12 that keyboard is out of scope. This is
already the controlling Menus rule in `milestone-plan.md` and Q122/#673 specifically
defers the add-row arrow/Enter flow. No new keyboard behavior or test will be built.
The structural semantics still apply: the listbox owns only result options, Create is
outside it, and expanded state describes the visible suggestion popup.

The third review's other product finding is fixed: the public guarded transition
route resolves the authoritative items-per-menu ceiling and the SQL transaction
counts distinct menu items under its placement lock. Restoring an item already on
another page does not increase that distinct count; adding a genuinely new menu item
at the limit returns `ceiling_reached` with no placement or history write. The focused
LocalDB regression is 2/2 with the stale inverse test, the service boundary regression
is 1/1, Release/build/unit gates remain green, and the four isolated desktop Menus
shards pass 68/68. The repository-wide isolated gate discovered 220 cases: 142 passed
and 78 were explicit mobile/keyboard scope skips, with no failures. One first attempt
at the stale-Undo browser case exposed that its second-actor POST asserted only HTTP
200, even though `already_on_board` also returns 200; it could therefore invoke Undo
before the asynchronous removal completed. The test now waits for removal and proves
the response is `placed` in the sibling section before Undo. That case and its full
shard pass in fresh isolated venues.

The external independent review recorded on issue #703 superseded the earlier COMMENT
and returned REQUEST_CHANGES against `e596d21e10f665a6232891aa78d17309a6b2bd21`.
It found three blockers: Enter treated any substring search hit as a near match and
silently discarded a typed price; price lacked an add-route server bound and the owner
corrected its maximum from 40 to 12 characters; and the selected-row removal control
from groups E/H was absent. It also found asymmetric canonicalisation, an empty painted
listbox, generic transition ceiling copy, and an unusable acceptance workbook notes flow.

The remediation in product SHA `0e7c54c94a62a51960c405693ddf42208a5bbafe`
makes reuse require canonical equality, announces when an
existing item's shared price wins, centralises SQL canonical search (including `&`),
adds migration 067 with refusal-before-narrowing and historical snapshot preservation,
enforces the 12-character API/domain/UI boundary, adds the selected-row removal action,
omits the empty listbox, uses tier-aware ceiling copy, and repairs workbook notes,
screenshots, advancement, gated acceptance and fixture instructions. Release/build,
198/198 unit, focused API/migration 2/2, LocalDB and focused browser pass. The full
isolated Playwright gate ran 16 fresh-environment shards: 220 cases discovered,
142 passed, 78 explicit mobile/keyboard scope skips, and zero failures.

Independent re-review APPROVED exact product SHA
`0e7c54c94a62a51960c405693ddf42208a5bbafe`. Its only remaining finding was
documentation-only: the workbook exposed inert screenshot inputs. Those controls are
removed and the workbook now states plainly that its JSON exports outcomes and notes;
any screenshots are saved separately. No further product review is required.

Owner acceptance exported `m3-a-s3-acceptance-record.json` against the reviewed product
SHA and returned **Not accepted**: case 1 Pass; case 2 Fail because add-item search
results disappear and do not restore across query changes/reopen; case 3 Needs
Adjustment but explicitly deferred by the owner to later planned Canvas work (#704);
case 4 Not run; case 5 Needs Adjustment because the stale-Undo setup was unclear.
The stale concurrency behavior already has deterministic Playwright coverage, but the
workbook must explain the expected refusal and setup more clearly.

The focused search investigation found no product-code failure: the owner fixture
promised `Old-Fashioned`, `Aussie Burger` and `Classic Burger` but contained none of
them, and the live `Old` API query therefore correctly returned `[]`. The fixture now
seeds all three as library-only items. Focused Playwright covers prefix results,
no-match, delete-back restoration, close/reopen restoration, punctuation reuse and
substring-safe creation; it passes 1/1 in a fresh isolated venue. Per owner direction,
no unrelated or broad test suites were rerun.

The owner reran acceptance: search and stale Undo passed. Whole page remains explicitly
deferred to #704, and the not-run removal workbook case was waived by the owner's final
acceptance. The sole requested close-out change makes Undo/Redo notices name the exact
item and page; focused desktop Playwright passes 1/1 at
`73074e030cf9c2d172b435aaeadfd0638bdb0793`. The owner accepted Slice 3, waived all
further review, and instructed merge with no CI. PR #705 is the closure PR.

PR #705 merged to `master` as `a3a421339670a3807a0c8418a2551752a1dcaaca`;
issue #703 is closed and the completed branch is deleted.

**Exact next action.** Begin no successor until its owner-approved plan exists.

## M3-A Slice 3-A — implementation handoff — 2026-08-13

Issue #704 was repurposed with owner approval for the bounded UI refinement between
Slices 3 and 4. The implementation is on
`feature/menus-m3a-s3a-builder-refinements`, based on accepted `master` SHA
`370bd9a4a0003769e9dbeb6c2b84afeab05578d5`.

The builder now replaces the repeated section-chip row with a `Page › Section`
context, compacts history and keeps `View all` beside its heading, uses borderless
page tabs with the sky-blue active underline, installs the Signal V and route labels
in the fixed 76px rail, and allows the Sections/History and Item panels to collapse
independently. Panel state is stored browser-wide under
`vennusign.menu.builder.panels` and survives reload and moving between menus; storage
refusal falls back to visit-local state.

The owner explicitly excluded canvas-renderer changes, an expandable app rail, custom
keyboard navigation, application-wide renaming, and all Slice 4 inspector,
availability and 86 behavior. The acceptance workbook at
`docs/features/menus/m3-a-s3a-acceptance-workbook.html` is for owner acceptance only;
an agent must not complete or sign it.

Local evidence recorded before publication: Back Office production build passed,
198/198 Back Office tests passed, diff/whitespace checks passed, and 16/16 focused
Slice 3-A contract assertions passed. The two affected Playwright specs compile and
enumerate, but browser execution in the authoring workspace was **UNTESTED** because
the Linux workspace lacks the repository's Windows LocalDB harness and its browser
download returned an empty archive. On 2026-08-12 the owner gave a one-time waiver of
independent agent and Playwright review for this special slice. The waiver applies
only to Slice 3-A and does not create a standing exception.

**Exact next action.** Pull and start Slice 3-A for the owner's acceptance workbook.
Do not perform agent review or Playwright review under this one-time owner waiver.

## M3-A Slice 3-A — owner adjustment handoff — 2026-08-12

During owner acceptance, four bounded builder refinements were requested: collapsed
Sections and Item rails now retain only their arrow control; page History follows the
section list instead of pinning to the rail bottom and no longer prints an empty-state
sentence; the screen-assignment control now presents a clearer status with a distinct
`Manage screens` action label; and the menu-name pencil now opens an inline editor that
persists the trimmed, venue-scoped name while refusing duplicates.

Affected Release API build, Back Office production build, and 10/10 focused API unit
tests passed. `git diff --check` passed. Per the owner's Slice 3-A exception, Playwright,
CI, and another independent review were not run.

**Exact next action.** Owner confirms the four acceptance adjustments on the running
Back Office, then Slice 3-A can be merged; do not begin Slice 4.

The owner's follow-up layout pass further reduced both rename editors to a single
underline, made History occupy the remaining expanded section rail without publication
metadata, restored centered vertical `Sections` and `Items` identities in collapsed
desktop rails, and moved active-page renaming into the top context beside the menu name
instead of replacing its tab. The Back Office production build and Impeccable layout
scan passed; browser execution remains waived for this slice.

The final breadcrumb correction keeps the top bar menu-only, places page rename in
the canvas context immediately before its three-dot actions, and uses the clearer
`Page / Section` hierarchy recommended by the Impeccable layout pass. The menu-name
editor explicitly suppresses the shared focus halo so its active treatment is one
underline rather than a box.

The owner confirmed the result is good and authorized merge and Slice 3-A close-out.
PR #706 is the closure PR. The accepted product head is
`b7f29481046d28f3d61878dd3b09e7d9c5ed56bc`; no further review, Playwright, or CI is
required under the one-time owner exception.

PR #706 merged to `master` as `cdfd2bbf7ad0d2211ebbd0d5c5914dff754a6583`;
issue #704 is closed and the local and remote completed branches are deleted. Slice
3-A is closed. The one-time review, Playwright, and CI exception is exhausted.

**Exact next action.** Do not begin Slice 4 until its owner-approved plan exists.

## Release engineering foundations — dev deploy pipeline stood up — 2026-08-16/17

Separate track from Menus: a branching/versioning/deployment discussion produced
significant additions to `docs/design/progressive-customer-cutover-concept.md`
(branching model — master/release/X.Y/hotfix; dev/stage version folders and version
chooser; git tag and codename conventions; automated MAJOR/MINOR/PATCH versioning
with AI-assisted release classification; per-component selective release; and the
Application Discovery Service, ADS, giving VR continuous (app, version) -> healthy
instance resolution). All still concept-stage per that document's own status line —
not approved scope.

Ahead of that, a real dev deploy pipeline was stood up and proven working today.
`.github/workflows/deploy-dev.yml` builds and deploys `api`, `back-office`,
`display`, `po` to their `vennusign-dev-*` App Services on push to `master`, gated
so PR-time test workflows and the deploy workflow never share a trigger. Backing
Azure OIDC identity (`vennusign-github-actions-dev`, federated credential trusting
`repo:jmiedreich-ux/Vennusign:ref:refs/heads/master`, `Website Contributor` scoped to
`rg-basic-website` only) was created out of band, not via any script in the repo.

`board-engine` was found to have no independent deploy target — it is a shared
source library imported by `back-office` (and likely `display`) via a tsconfig/vite
path alias, not a standalone app. The `vennusign-dev-board-engine` App Service and
its subdomain, created earlier in the same session before this was discovered,
remain unused; nobody has decided yet whether to remove them.

The first real deploy run surfaced three problems, all now fixed directly in Azure
(none via a repo-tracked script yet):

- `vennusign-dev-api` had no `ConnectionStrings__VennuDatabase` app setting. Set to
  point at the existing `dev_vennusign` database on `dev-vennusign.database.windows.net`
  (australiaeast; a different region from the App Services' Central US, not yet
  addressed). The firewall already allowed Azure services, so this alone should have
  been sufficient.
- The three static SPA apps (`back-office`, `display`, `po`) are Vite builds with no
  server process, deployed onto Node-runtime App Services that had nothing to execute.
  Fixed by setting the startup command to `pm2 serve /home/site/wwwroot --no-daemon --spa`
  on all three.
- `vennusign-dev-api` still failed to start after the connection string fix, with
  zero application log output across several restarts. Traced to
  `DatabaseMigrator.Run` (`src/Vennu.Data/DatabaseMigrator.cs`): it opens a SQL
  session and blocks on `sp_getapplock` (session-scoped, 180s timeout) before doing
  anything else, and produces no console output until *after* that lock is acquired.
  Repeated restarts during troubleshooting almost certainly piled up orphaned
  session locks from hard-killed containers, compounding the wait each time. Also
  changed `linuxFxVersion` from `DOTNETCORE|10.0` to `DOTNETCORE|9.0` to match the
  app's `net9.0` target (harmless, but not confirmed to have been the actual
  blocker), and raised `WEBSITES_CONTAINER_START_TIME_LIMIT` to 600 to give the
  migration headroom. Letting one restart run undisturbed, without further
  interruption, is what actually resolved it.

Fixed in the repo: `src/Vennu.Data/DatabaseMigrator.cs` now logs before
`EnsureDatabase`, before and after lock acquisition (with elapsed wait time), and on
lock release — commit `84e7699`. Previously this whole sequence was silent, which is
why the above took so long to diagnose: DbUp's own `.LogToConsole()` only starts
after the lock is already held.

Current state: all four dev apps confirmed live and serving real content —
`dev.api.vennusign.com` (`/health/version` 200), `dev.back-office.vennusign.com`,
`dev.display.vennusign.com`, `dev.po.vennusign.com`.

Not yet done: none of the Azure-side fixes above (connection string, `pm2 serve`
startup command, runtime pin, `WEBSITES_CONTAINER_START_TIME_LIMIT`) are captured
anywhere in the repo or as infrastructure-as-code — they exist only as live Azure
App Service configuration. `theme-studio`'s dev subdomain and App Service exist with
no application deployed to it yet. `stage`/`app` tiers have no custom domains, OIDC
identity, or deploy workflow at all yet — today's work covers `dev` only, and
deliberately does not yet implement the version-folder/branching model from the
concept doc above, since that remains unapproved design.

While diagnosing the migration-lock hang above, application logs were repeatedly
pulled from the wrong place: `LogFiles/*_docker.log` is platform/container-lifecycle
only. The app's own `Console.WriteLine`/DbUp output was landing in
`LogFiles/StartupLogs/{date}_{machine}_success.log` /`_failure.log`, which is a
startup-diagnostic feature, not general application logging — "Application Logging
(Filesystem)" was off entirely on all four dev apps (`applicationLogs.fileSystem.level:
Off`). Turned on for all four (`az webapp log config --application-logging
filesystem --level information --docker-container-logging filesystem`) so app output
now lands in the standard, always-on `LogFiles/Application/` location instead of only
being visible by accident during a container's startup window.

**Exact next action.** Decide whether to capture the App Service configuration
(connection string, startup commands, runtime, timeout, and now application
logging) as infrastructure-as-code or leave it as manual Azure state; then continue
either with the GitHub issue backlog or with formalizing the release-engineering
concept into an approved work package.

## 2026-08-25 — Local-model qualification complete

The finite local-model qualification is complete and pushed on branch `research/local-ai-model-qualification-2026-08-25`. Final commit at handoff: `0e3ee0ddd1fcf79b89465d8040121d9df681618c` (this handoff update will follow it). The durable evidence, immutable fixtures, transcripts, GPU samples, scoring, historical calculator baseline, final report, and JSON summary live under `docs/research/local-ai-model-qualification/`; no Git bundle was created.

Eight official runs completed: qwen3.5 (coding 95/review 48), qwen3-coder (90/0), gpt-oss (95/0), and devstral (60/0). Under the fixed qualification rules, `gpt-oss:20b` is both **fast worker** and **primary developer**; no local model qualified as **planner/reviewer**, so Maestro retains that role in the cloud. `docs/design/proposed/maestro-dev-lead-agent-framework.md` now links the finite qualification and requires routing only within measured thresholds.

Two important limits are recorded in the final report: the visible coding fixture has a quantity-20 public-test case that contradicts the stated maximum quantity of 10, and three models failed to locate `candidate.diff` in the review fixture. Results were preserved rather than repaired or rerun. qwen3.5 also has one preserved pre-official harness failure, followed by the permitted infrastructure retry.

**Exact next action.** Treat the qualification as closed. For any future routing change, read the final report and evidence first; do not add unplanned tests or alter these results. Any new qualification phase requires an owner-approved separate plan and branch.

## 2026-08-28 — Done Record enforcement

The owner adopted Done Record enforcement. `docs/templates/done-record.md` is the canonical template; `docs/features/menus/done-records/` is created with the first record. `AGENTS.md` now requires a complete record for every milestone or fix PR, requires paths to be recorded and attacked, requires every standing design question to be answered, and makes the Done Record the reviewer's first check.

**Exact next action.** Every milestone or fix PR commits a filled-out Done Record at docs/features/<feature>/done-records/<pr-number>.md, from the template at docs/templates/done-record.md, describing the exact head commit under review.

## 2026-08-30 — Architecture Renewal M0 and API Architecture vNext proposal

Content Platform Architecture Renewal PR **#939** records the owner-approved foundation and the
API Architecture vNext proposal. VennueSign is re-founded as a controlled content-and-presentation
platform; Menu becomes the first real shared Data Model, `menu.v1`, rather than a permanent
menu-only exception.

The durable record is `docs/architecture/content-platform-architecture-renewal.md`. It establishes
Content Home, Content Builder, immutable Data Model/theme versions, a typed record library for
manual and imported facts, first-class operational state overlays, immutable Published Presentations, and
a modular-monolith direction with one deployed API host initially.

API vNext keeps four logical surfaces, each with four explicit families:

- **Vennue Core API** — customer business authority and effective desired state.
- **Vennue Connect API** — controlled external data exchange and synchronization.
- **Vennue Runtime API** — output-specific delivery, convergence, health, and actual-state evidence.
- **Vennue Platform API** — Vennue workforce governance and cross-tenant service operations.

Multi-venue ownership, organization sharing and venue-local content/overrides, publishing and
rollback, imported-field data source and change authority, Release-versus-Package, desired-versus-actual
state, player-output identity, fleet operations, and privileged-action boundaries are explicit.

The competing endpoint list remains separate at
`docs/architecture/api-vnext-endpoint-inventory-mapping.txt`. Its original route and description
text/order are preserved. All **156 route groups** carry a primary `MAP`; `INTERACTION` and
`REVIEW` annotations expose cross-surface composition and unresolved candidate-contract issues.
The inventory cannot redefine or consolidate the four surfaces.

No product code, schema, endpoint approval, API behavior, deployment topology, tracker claim, or
current Menu behavior changes through this planning PR. Independent verification approved exact
architecture head `c6ecf85f75ecdf5abbf8a4d7276d7601965a6271` before merge preparation. The final
merge head must receive a fresh independent review because the Done Record and conflict resolution
invalidate that approval.

**Exact next action:** answer the owner's closing API Architecture vNext questions, then run the five
M0 planning sessions—current-source map; `menu.v1`; record library/provider authority;
theme/release contract; and refactor sequencing—before selecting a bounded M1 implementation.
Data Model Studio follows the proven model engine and `menu.v1` proof.

## 2026-08-30 — Mosaic V1 joins the Architecture Renewal

Owner direction is recorded in issue **#965** and in
`docs/architecture/content-platform-architecture-renewal.md`.

The renewal stages and product versions are separate but connected axes:

- **M0–M5** remain the Architecture Renewal's work structure.
- **Mosaic V1, V1.x, and V2+** describe coherent product releases.
- The Mosaic capability/dependency map connects each release capability to the renewal-stage work it
  requires.

This corrects two bad planning extremes. VennueSign will not perform a whole-product code audit
before choosing work, and it will not take a token slice from every Core, Connect, Runtime, and
Platform family merely to make Mosaic look complete. Existing code is uneven in maturity and is
treated as foundation: each selected capability is classified **reuse**, **reshape**, **build**, or
**defer** after bounded, just-in-time investigation.

The initial Mosaic dependency hypothesis is:

```
existing sign-in and venue context
-> menu.v1 Data Model
-> Content
-> Theme / Presentation
-> Publish immutable Release
-> Assign to Wall / Screen
-> Runtime displays it
-> Runtime proves actual Showing State
```

This spine does not assume sign-in must be rebuilt first. It asks whether the existing capability
already satisfies the Mosaic contract. The same rule applies to Menu behavior, themes, publishing,
assignments, rendering, and display delivery.

Parallel cloud coordinators are derived from the dependency graph. Each lane receives owned
capabilities/files, inputs, outputs, verification, and queue blockers. A free coordinator does not
start a package whose upstream contract is unsettled, and work touching the same invariant or
controlled record remains serialized.

One additional **Mosaic V1 Renewal Reconciliation Session** is required before the dependency map.
It aligns the M0–M5 renewal stages, four API surfaces, current feature/milestone records, built
foundations, settled decisions, contradictions, genuine owner questions, and Mosaic release intent.
Its job is to produce a trustworthy planning input set, not to inspect the entire codebase.

No implementation, endpoint, schema, service split, or Mosaic work package is approved by this
record.

**Exact next action:** run the **Mosaic V1 Renewal Reconciliation Session**. Produce its
reconciliation record, including authoritative inputs, superseded/conflicting records, settled
facts, open owner decisions, initial Mosaic outcome, and areas needing bounded inspection. Then
create and review the **Mosaic V1 Capability and Dependency Map** and use it—not a full code audit—to
create the first Mosaic milestones and work packages.

## 2026-08-31 — Independent Mosaic V1 blueprint study joins the renewal

PR **#967** adds the owner-requested independent roadmap study as two companion records under
`docs/architecture/`:

- `mosaic-v1-independent-blueprint-study.md` is the detailed engineering and future-agent record;
- `mosaic-v1-independent-blueprint-study.html` is the plain-language, self-contained owner report.

Four isolated reviewers mechanically covered all four API surfaces, 16 families, 82 named blueprint
areas, and 156 mapped route groups. The study tested the current early-content hypothesis against
identity/onboarding-first, Connect/showtimes-first, Runtime/Platform-first, horizontal renewal,
equal-slice, and Studio-first alternatives.

The conclusion retains content → presentation → display as Mosaic V1's integration spine for
software-building reasons, not merely because Menu work is furthest along. Its sequencing correction
is important: first define the observable first-live-screen acceptance journey and settle the small
set of irreversible boundary contracts. Data Model is the first semantic contract gate, but it need
not be the first production code. A guarded fixture-backed walking skeleton can expose cross-surface
risks while Core, Theme, Runtime, Connect data-source/change-authority work, Platform read support, authentication/tenant
sufficiency, and characterization proceed in parallel. Shared contracts, migrations,
Release/Package integration, and cutover remain serialized.

The study is advisory. It does not approve implementation, endpoints, schemas, service splits,
migrations, or a final Mosaic capability set. Its open owner decisions and evidence limits must be
resolved through the renewal process.

**Exact next action:** run the **Mosaic V1 Renewal Reconciliation Session**, using the independent
study as planning evidence. Reconcile its unresolved owner decisions and contract gates with the
controlled renewal records before creating the Mosaic V1 Capability and Dependency Map.


## 2026-08-31 — Mosaic V1 Renewal Reconciliation complete; awaiting review

The owner and architect completed the fixed 28-part Mosaic reconciliation. The authoritative result
is `docs/architecture/content-platform-architecture-renewal.md` §11.10. It is a planning-only
documentation update awaiting independent review and merge.

Settled Mosaic private-pilot boundary: existing authorized customer/venue, one logical Screen, one
paired Player Output, VennueSign Default Theme, manual/paste Menu input, always-on assignment, and
enforced-but-not-demonstrated multi-venue safety. It must prove immediate 86, rollback, last-valid
display recovery, and support evidence separating Core desired state from Runtime actual state.

Settled vocabulary: `menu.v1` is the controlled Data Model; an Item is reusable identity; a
Placement owns order and price; only Available, Sold out / 86 (Live), and Not available (Published)
are in the pilot state set. A **Published Presentation** pins validated Menu content, `menu.v1`, an
exact Theme version, assets, and renderer compatibility. A **Runtime Package** is Player Output-ready
material. **Showing** is Runtime evidence: received, verified, applied, and currently displaying.
Do not reintroduce the retired label in new architecture records.

The renewal also records the four internal API modules (Core, Connect, Runtime, Platform), gradual
old-to-new API migration with no dual truth, module-adjacent AI guides, parallel-lane entry gates,
and future Maestro registration readiness. VennueSign remains the project authority; Maestro will
later perform read-only discovery and bind to the approved graph rather than recreate it.

**Exact next action:** independently review this documentation update against the accepted Mosaic
decisions and controlled records. If there are findings, correct only those findings and request a
targeted verification of the correction. After merge, design—not implement—M1-A as a bounded,
Maestro-compatible packet and send it through Decision Fidelity Review.

## 2026-08-31 — Mosaic V1 graph approved and merged; handoff to Maestro registration

PR **#971** merged to `master` as **`c246080f0ba97e5fd020c936e9ca580e39e8f532`**.
The owner approved the complete delivery boundary after independent review and targeted-only
correction reviews. The immutable VennueSign graph revision is **`mosaic-v1.approved.1`**, based on
approval-time source SHA **`bd5e141ccaf02b8684c0db91b5d8c053e0bb95f9`**. M1-A.0 will record
the separate exact dispatch SHA after registration.

The approved plan contains five delivery waves:

1. Foundation proof, exact-head characterization, production contract authority, and UX-A.
2. Real `menu.v1`, Default Theme, paste/publish, UX-B, and the Foundry operator workflow.
3. Real Player Output Package, Showing, Live 86, recovery, and physical proof.
4. UX-C, the Foundry support view, operational recovery, migration/retirement, and the full journey.
5. Private-pilot readiness, acceptance, bounded correction, and owner closeout.

M1-A is now eight approved executable packets, M1-A.0 through M1-A.7. It delivers only the
disposable fixture-backed internal proof through Published Presentation → exact Player Output
Runtime Package → Live overlay/Showing → read-only support projection. Qwen owns the exact-source
map, bounded Platform projection, and module guides; ChatGPT CLI Sol/Terra own the shared-boundary
and integration packets; Claude supplies independent review; the architect coordinates assembly and
merge. No packet has been dispatched.

The UI omission found during owner review is closed in the graph:

- UX-A reconciles the current rendered/source UI and accepted decisions, hi-fi designs, and
  wireframes against every target V1 pathway before Back Office UI work.
- UX-B and UX-C produce complete operator/support path and state matrices, reuse valid designs, and
  create or amend only necessary wireframes. Each requires independent accessibility/design review
  and owner acceptance.
- `G-PRESENTATION-SEAMS` fixes explicit import directions: Foundry imports no VennueSign code;
  presentational components consume Foundry/skin plus typed view state and emit typed intents;
  orchestration composes presentation and adapters; adapters own transport mapping; Core, Connect,
  Runtime, and Platform import no UI layer and remain business authority.
- `G-FOUNDRY-READY` requires an exact accepted Foundry version/control set and VennueSign skin,
  package, accessibility, browser, and upgrade contracts. Missing controls remain Foundry work;
  VennueSign cannot create private substitutes.
- Foundry Bridge may inventory, map, and assist approved mechanical migration. It cannot choose
  product pathways, resolve product intent, or create/approve wireframes.
- The guest-facing Default Theme is separate from Foundry Back Office controls.

Gate state at close:

- **OPEN — `G-V1-PLAN`:** owner approval and Decision Fidelity Review are complete.
- **CLOSED — `G-MAESTRO-REG`:** Maestro has not yet completed VennueSign read-only discovery, the
  owner-reviewed thin `maestro.project.yaml` binding, or the no-dispatch dry run.
- **CLOSED — `G-M1A-CHECK`:** opens only after M1-A.7 records technical and development-system
  evidence and the architect chooses PROCEED rather than HOLD.
- **LATER OWNER DECISIONS:** `D-RUNTIME-01` selects pilot hardware/runtime, geometry, and renderer;
  `D-RECOVERY-01` approves offline duration, last-valid retention, reconnect, stale/failure, and
  recovery behavior. Neither blocks registration or M1-A.

No endpoint, schema, migration, product UI, Foundry integration, device delivery, customer data, or
Mosaic implementation was started in this session.

**Exact next action:** in the Maestro repository, begin VennueSign's required **read-only registration
discovery** against VennueSign `master` at `c246080f0ba97e5fd020c936e9ca580e39e8f532` and graph
revision `mosaic-v1.approved.1`. Produce the discovery report only; do not create the thin binding,
project queues, or dispatch M1-A.0 until that report is reviewed under Maestro's registration process.
