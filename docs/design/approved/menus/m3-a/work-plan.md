# M3-A — work plan for the implementing agent

Scope: the Menu Builder and the screens it reaches. Runs **after M3, before M4**.
Everything here is designed, answered and recorded — see `menus/Menu Builder - agent brief.md`
for conventions, `menus/Menu Builder - action inventory.md` for acceptance criteria,
and the amendments at the foot of `decisions.md`.

**Build target: maximum tier, every capability on**, each gated control wrapped in
its own capability check (amendment A16).

Every slice follows `AGENTS.md`: **schema → API → UI → Playwright specs in the same
PR**, independently mergeable, `master` releasable at each step. CI is suspended by
owner decision, so the gate per slice is the affected Release builds, focused unit
tests, the Playwright gate and the owner demo, each reported with its output.

---

## Slice 0 · Foundations (no user-visible change)

Small, and everything else assumes it.

- **Extend `SkyIcon.tsx` with six named paths** — drag handle, pencil, remove, chevron, warning, screen mark. Settled: one in-house set. Lucide stays where `navigation.mjs` already uses it, in the rail, and nowhere else. Same 24px viewBox, stroke 1.8, round caps, `aria-hidden`.
- **Add the Playfair display token** — one token, page tabs only. Font loaded once, no other back-office use.
- **Amend the banned-words array** in `tests/ui/specs/menus-shelf.spec.ts`: remove `restore`, keep `unpublish`, `supersede`, `archive` (amendment A8). Ship this in whichever PR first renders the Finish menu if not done here.
- **Three config values** with defaults, read at runtime, never hardcoded in a view: import file-size limit (tiered, starts at 5 MB), publish-retry-silence threshold (venue setting, ~30s), history retention depth (per tier).
- **Capability check helper** for the gated controls, defaulting to on. The tier infrastructure does not exist yet and is not built here — the point is that each gated control is written so it *can* be switched off without touching layout.
- **Seed API foundation** — treated as a product surface, not test scaffolding: explicit timestamps for history entries, a seeded item library with known names, and forced screen states (online, offline, never paired, has-not-taken-this-yet). Page-shaped seed inputs land in Slice 1 with the page schema and product endpoints they must exercise; putting them here would require a test-only implementation ahead of the product API and contradict the rule below that the Test API does nothing itself.
- **Move the seed onto its own deployable.** It lives inside the product API today as `POST /api/test/seed`, Development-only. Owner decision: it should not ship. Compiling it out of Release is the cheap option and it fails the staging requirement — staging runs a Release build, so a seeder excluded from Release cannot seed the environment it is meant to serve. Instead: a **separate deployable referencing the same data and domain layer**, deployed to local and staging and never to production, with its own auth secret. **One hard rule: it hands every call off to the product API and does nothing itself.** No direct writes, no domain calls, no SQL. Every seed then exercises real endpoints, so seeding is itself an API test and a data test — if the seeder breaks, the API broke, and a spec cannot pass against a state the product could never create.

  That leaves two states with no obvious endpoint, and each has an honest answer that needs no back door.

  **Screen states — a test player, not a hook.** A screen's state is reported by the screen, never set by the back office. So the seeder acts like one: register a player against the screen key and have it check in, stop checking in, or acknowledge a publish late. Offline really is "has not checked in", never-paired really is "never paired", and "has not taken this yet" really is an unacknowledged publish. Real behaviour, reached the real way, no product change.

  **Time-dependent states — the product API owns it, the test API is the only caller allowed.** Settled: the product API gains the ability to write a history entry at a given time, and that ability is exercised **only when the caller is the test API**, gated by identity and scope rather than by a build flag. So it is a real endpoint with a real authorisation boundary — a permission only the seeder holds — rather than a hidden door that ships and hopes nobody finds it. The behaviour is the product's, the privilege is not.

  Two things this must not become: a parameter any authenticated caller can pass, and a code path that skips the product's own validation. It writes the same entries by the same rules, only with the time supplied.

  Tiny configured windows in the test environment remain the cheaper route for the grouping threshold, where no dating is needed at all — real actions in one run cross a threshold of two.

  The cost of all of this is **specs that wait**: the retry threshold waits its real 30 seconds, retention waits its configured minute. A slower suite in exchange for evidence rather than assertion.

**Done when:** the tokens and glyph set exist, the three settings resolve from config, the separately deployed seed delegates its supported shapes to real product endpoints, and the amended banned-words spec passes. Page-specific shapes are Slice 1's schema/API/seed vertical.

---

## Slice 1 · Page rail and page header

Roughly 15 branch paths (inventory groups B and C).

- **Schema/API:** pages as first-class children of a menu — create, rename, duplicate, delete, reorder; page-to-screen assignment moves to the page.
- **Seed API:** pages, multi-section pages and per-page assignments, added only now that the real product schema and endpoints exist.
- **UI:** the horizontal tab rail with the folder join and uppercase Playfair names; `+ Add page` with inline naming; drag reorder; horizontal scroll past the width. Page header with name, live item count, page-actions `⋯` (rename, duplicate, delete), viewing chips (`Whole page` plus each section, collapsing behind `More` past five), and the assignment pill carrying the count and the route to Screen Assignments.
- **Rules:** a menu always keeps one page. Delete offers to move contained sections. Duplicate copies sections and items but not assignments (A1, C3, C4). Capacity is only ever reported against assigned screens.
- **Specs:** tab switch reloads sections, header, assignment and board; delete-with-sections offers the move; last page cannot be deleted; chips collapse at six; capacity never names an unassigned screen.

---

## Slice 2 · Sections and page history inside the panel

Roughly 14 paths (groups F and G).

- **Schema/API:** sections belong to a page; reorder within a page; delete with item reassignment.
- **UI:** the sections column inside the page panel, selection, inline rename via pencil, drag reorder, `+ Add section` with inline naming, delete with the *move these items to…* choice inside the confirmation. Page-scoped history beneath it, with menu-level facts and the *Menu history* route at its foot.
- **Rules:** an empty page is legal and is flagged at review, not blocked. History rows are read-only in M3-A — no restore, no diff (A2, A15, F4).
- **Specs:** rename updates rail, board heading and history; delete-with-items offers the move; history shows only this page's events and never a publish entry.

---

## Slice 3 · Board and add-item

Roughly 14 paths (group E).

- **API:** item create/reorder/move-between-sections/remove-from-page; library name matching.
- **UI:** the live board at the page's real geometry with the capacity banner above it; item selection; cross-section drag; `Remove from this page` with its confirmation; the inline add row at the end of the section, caret in the name, Tab to price; the library near-match suggestion with the existing item preselected.
- **Rules:** nothing else is editable on the board (A6). A name is required, a price is not. Abandon-blank discards silently. Capacity recalculates on the keystroke (A10, A11, A12).
- **Specs:** add lands at the section end; blank abandon leaves no item; near-match offers the existing item; cross-section drag moves it; remove confirms and names the page; capacity warns while typing.

---

## Slice 4 · Inspector, availability and the 86

Roughly 13 paths (group H).

- **Schema/API:** availability and 86 as separate facts with provenance; 86 immediate, its cancellation queued with the hide (A4).
- **UI:** Basics fields with live board updates and draft autosave; the two coupled toggles on one ink track; the boxed 86 message with time and author; the inert 86 on an unpublished item **with its reason shown**; More details behind its tab.
- **Rules:** prices render exactly as typed (Q115/Q190). Available off clears the 86 and both land on the same publish. 86 wording is staff-side; the board says Sold out, drawn by the theme (A3, A5).
- **Specs:** typing updates the board; price `MP` survives untouched; Available off with an active 86 defers both to publish; a never-published item cannot be 86'd and says why; the board renders the theme's sold-out treatment, not a hardcoded one.

---

## Slice 5 · Footer, publish and history

Roughly 20 paths (group I plus screens D and E).

- **API:** publish with the review summary **stored on the publish record**; per-screen delivery state; retry with the venue threshold; restore producing a draft.
- **UI:** the footer's draft state and publish time, theme control, the single `Finish` menu (review & publish, save & exit, discard, restore); the discard confirmation naming every change and protecting live 86s; the publishing review with the overflow tick and *fix the fit*, the unnamed-item list with drop-all, where-it-goes and the tier-gated *Publish later…*; the confirmation that names screens and persists until the next edit; menu history with grouping above five of a kind a day and expandable stored summaries.
- **Rules:** overflow publishes, named and acknowledged, never silently (A14). Offline screens never block and catch up. A half-failed publish is silent until the threshold, then names the screen that has not taken it. No diff view (A7, A15).
- **Specs:** publish updates only online screens and names the offline one; discard preserves an 86; overflow lists both items and records the acknowledgement; the confirmation survives until an edit; grouping starts at six of a kind; history expands to the stored summary.

---

## Slice 6 · Import landing — the routes exist, and do nothing yet

Small. Owner decision: the options are present, the imports are not built here.

- **UI:** the landing with its four route cards, reached from *Create menu* and from *+ Add content*. **Photograph it, Paste text and Spreadsheet are inert** — present, described, not wired. POS is absent entirely until a till is attached (decisions 4 and 17).
- **Start blank is the one that works** — confirmed by the owner — because it is not an import: it creates the page, names it, and opens the builder. It is load-bearing; without it there is no route into the builder in M3-A.
- **Not built here:** upload, reading states, review for either source, add-or-replace, the five failure messages, the section picker, the page-name field. All are designed and approved — sections A2, A3, A4, B, B2 and F of the connected screens file — and become their own slices in a later milestone.
- **Specs:** the landing renders four routes with POS absent; Start blank reaches the builder with a named page; the three import routes are inert and say so rather than failing.

**Consequence for enumeration:** the import paths do not need turning into branch tables for M3-A. What does is Screen Assignments, publishing review, menu history and the after/empty/capability states — roughly 30-35 paths, not the 60-70 that included import.

---

## Test readiness — the eight prerequisites

Real tests against real data. Owner answers, 11 Aug 2026.

| # | Prerequisite | Answer |
|---|---|---|
| 1 | Seed has no pages | The seed API grows with the product; pages, multi-section pages and per-page assignments are added to it. It becomes the backbone of automated testing in staging later |
| 2 | History needs backdated data | The **product API** gains timed history writes, exercised **only when called by the test API** — a real endpoint with an authorisation boundary, not a build-flagged back door. Grouping needs no dating: a tiny configured threshold does it |
| 3 | Testing capability-off | Everything ships on. Controls are written so they can be switched off; the tier infrastructure does not exist yet, so absence is not asserted in M3-A |
| 4 | Forcing screen states | Through the seed API — online, offline, never paired, has-not-taken-this-yet. POS is an add-on and out of scope |
| 5 | Import fixtures | Not needed. The routes are inert in M3-A; fixtures land with the import slices in a later milestone |
| 6 | Deterministic overflow | See below — researched, and it needs building |
| 7 | Retry threshold | Specs wait the real ~30 seconds. No shortcut, no injected state |
| 8 | Seeded item library | Seed a library with known names so a near-match is a real match |

### Board-fit capacity — what the research found

`tests/ui/specs/screen-capacity.spec.ts` is about the **screen pairing allowance**
(`screen-quota`, `allowance.reached`), not about whether a page fits a board.
Board-fit capacity does not exist in the codebase at all: no engine, no endpoint, no
test. It is new work in this milestone, and it is the one piece of M3-A that cannot
be asserted against something already true.

So it needs building as a testable unit, not as a number inside a component:

1. **The fit engine computes a limit** from content, theme, screen geometry and orientation, and **exposes it** — `data-capacity="fits|nearly-full|overflowing"` plus the computed limit and the names of anything dropped.
2. **One golden spec pins the limit itself**: Midnight, 1920 × 1080 landscape, a fixed set of items → an exact expected limit. If the engine's arithmetic changes, that spec fails loudly rather than every other capacity test quietly re-baselining.
3. **Every other capacity spec derives from the engine**: seed `limit + 2` items and assert the two named overflow items, rather than hardcoding "14".
4. **A second geometry** — the 3840 × 2160 portrait screen already in the fixture — proves the limit moves with the screen rather than being a constant.

**One thing not to confuse with this.** The existing `screen-quota` / `allowance.reached`
work is the **pairing allowance**, which is a tier question and belongs to the tier
ladder, not to Menus. Board-fit capacity is a different subject entirely: how much
content a given theme can draw on a given screen. Capacity specs may seed whatever
screen count is convenient — the allowance is irrelevant to whether a page fits, and
the two must not share a fixture, a name or a test id.

Until step 1 exists, the overflow tick, the publish-with-overflow path and the
capacity banner's three states are all untestable, which makes the fit engine the
critical path item of slice 1.

## Exit criteria for M3-A

- All 93 builder paths have a passing spec, plus the ~30-35 paths for Screen Assignments, publishing review, menu history and the after/empty states once enumerated. Import paths are excluded — the routes are inert.
- No literal colour, radius or spacing outside `sky-ui-tokens.css`; Playfair only on page tabs.
- The three config values are read, not hardcoded.
- Every capability-gated control has its check in place and is on.
- The fit engine exposes its computed limit, with the golden spec pinning it.
- Owner demo covers: start a menu blank, add sections and items, 86 something, publish with an overflow acknowledged, and see it in history.

## What M4+ may have to adjust

- **Import, in its own slices** — upload, reading, review for photo/paste and spreadsheet, add-or-replace, the five failure messages, the section picker and the page-name field. All designed and approved already; sequence them as one milestone rather than spread across others.
- **Menus home** — menu-level duplicate and delete have no home; the card also needs to carry an import waiting for review, and the no-menus-at-all empty state.
- **Restore and history depth** — restore-from-history (*Go back to this*) is deferred; the diff view stays deliberately unbuilt.
- **Quick Update** is the first mobile candidate and the only surface where the 86 lives elsewhere; it must not diverge from A3's vocabulary.
- **Schedules** takes *Publish later*, and already owns timing; the theme owns rotation, so neither belongs to Menus later either.
- **Multi-venue** (decisions 20-34) touches import (a venue cannot replace a group menu), history (whose change was it) and publishing (corporate pushes, venues accept). The group-menu blocked state is deliberately out of scope here.
- **Size as an item field** — decision 31 lists it as an optional import heading with nowhere in the item model to land.
