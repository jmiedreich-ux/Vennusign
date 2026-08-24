# Mock-fidelity polish round — plan and status

Tracks the live-testing feedback round against the shipped #797 follow-on work (PRs #831–#833), comparing dev against
the "New reference set" mockups (`page-examples.md`, assets 12–16). Two GitHub tracking issues cover it:
**#834** (bugs, design-system standard, item panel, bottom bar — closed out) and **#842** (dialog design passes, the
two full-page pieces, background/capacity polish — in progress).

## Shipped

| # | What | PR |
|---|---|---|
| 1 | Two crash-to-white-screen bugs (Move-to-page, Delete-section), "Rename does nothing" | #836 |
| 2 | Standard dialog field styling — real border, radius, focus ring instead of the raw browser default | #837 (decision O3) |
| 3 | Section actions (Rename/Move/Delete) — centered modal → anchored dropdown | #838 |
| 4 | Add-item panel: focus ring, "Add many at once" as a pill, persistent "+ Add item" button while editing | #839 |
| 5 | Bottom bar: dropped the duplicate middle pill, "Last published…" wording, blue Actions button, theme picker moved into the footer, Actions dropdown flush to the bar | #840 |
| 6 | Focus ring made quiet everywhere (border-color change, no outline) after live feedback that even O3's refined ring still read as "a circle"; doubled rail divider; delete-section button says "Delete and move items" / "Delete and keep items" instead of generic "Delete section" | #844 |
| 7 | Capacity banner — `--sky-color-warning-border`/`--sky-color-warning-soft` were referenced but never defined anywhere in `sky-ui-tokens.css`; pointed at the real `--sky-color-warning`/`--sky-color-warning-surface` tokens | #847 |
| 8 | "Check fit" restyled as a plain link (was a bordered button); background corner blob made visible (anchored tighter to the corner, more saturated, than the app-wide `--sky-page-gradient`) | #848 |
| 9 | Real design pass on Move/Delete/Remove dialogs — found and fixed: Remove-item's Cancel button used an undefined `.secondary` class (unstyled), Escape/click-outside didn't close that dialog at all; added a selected-state highlight to both dialogs' radio choice rows | #849 |
| 10 | Connecting line through the on-screen history timeline circles | #850 |

## Cleanup list (found along the way, not yet fixed)

- **#803 — stray default "Section 1"**: a fresh menu always carries an empty default section alongside whatever
  section the seed actually created. Causes widespread Playwright failures unrelated to whatever's being tested
  (wrong section selected by default, `board-item` not found, strict-mode violations on ambiguous locators). Confirmed
  via direct A/B against unmodified `master` multiple times this round — real, pre-existing, not caused by any of the
  above.
- **Background corner blob / tint not visible on the live deployed site**, despite #846 and #848 both being verified
  via local Playwright screenshots. Reopened in #842. **Needs a Playwright check against the actual live dev URL**
  (`dev.back-office.vennusign.com`), not just the local dev server, before attempting another fix — something is
  different between what a local screenshot shows and what the reporter sees live, and guessing blind at another CSS
  tweak risks the same result.

## Remaining on #842

### Two full-page pieces

Both depend on the same underlying capability: a per-publish, field-level "what changed" list (what, before → after).
This is **not** a new backend build — `MenuSnapshot.Diff` (`src/Vennu.Api/Services/MenuSnapshot.cs`) already computes
exactly this shape (`SnapshotChange`: kind, id, field, before, after) for menu/theme/page/section/item/placement, and
it already backs today's "N changes" count. The work is formatting that into the change-list UI and, for history,
persisting the formatted text at publish time.

Design authority for both, in order of specificity:
1. `page-examples.md`'s "New reference set" notes (assets 15–16) — what's confirmed vs. still open for these
   *specific* mockups.
2. `../decisions.md` — A11 (unnamed items), A14 (fit-overflow acknowledgment), A15 (no diff view; history replays
   the review-time summary verbatim), A2 (history is page-scoped where shown, menu-level where kept).
3. `../open-questions.md` Q12 — "one change-list sheet (what, before → after, who, when + target screens + Publish)…
   No visual diffing this build." Governs the shape of the "what changed" list itself: plain before → after text
   rows, never a visual/side-by-side diff renderer.

#### Review & Publish (mock 08 / asset 15)

Full routed page, replacing the current small modal (`reviewOpen` dialog in `MenuBuilder.tsx`). Per
`page-examples.md`: **"not yet scoped against the current modal's content"** — before building, reconcile what the
current modal already covers against what the mock adds:
- Fit-overflow section with the acknowledgment checkbox and "Fix the fit" route (A14) — the app already has
  `fitOpen`/`Check fit` elsewhere; reuse rather than re-derive.
- Unnamed-item section with "Name it" / "Drop it" (A11).
- The per-field change list (see above) grouped by page, using `MenuSnapshot.Diff`.
- "Already live, and staying that way" — 86'd items, unaffected by publish.
- "Where it goes" sidebar — per-screen breakdown (page assigned, refresh cadence, online/offline/stale). Screen
  state and cadence data already exist for the bottom bar's screen chips; this is a new arrangement of existing
  data, not new data.
- "Publish now" / "Publish later…" — the mock's "Publish later" hands off to Schedules. **Schedules integration is
  out of scope** per `milestone-plan.md`'s scope guardrails ("Out of scope entirely: … scheduling"). Build "Publish
  now" only; either omit "Publish later" or leave it visibly absent per decision 4 (a capability outside scope is
  absent, not disabled) until Schedules exists.

#### Full history screen (mock 09 / asset 16)

Full routed page, replacing the "View all" modal (`viewAllOpen`) and the "go back to…" picker. Per
`page-examples.md`, **confirmed cuts from the mock**: drop the "People editing" stat (menus are single-editor in this
product — the stat doesn't apply), defer "86s this week" to a future release. What remains in the "THIS MENU" sidebar:
on-screens-since, published-all-time count.
- Per A15, each publish's expandable detail is the **same summary text shown at Review time**, stored at publish and
  replayed — not recomputed. This means Review & Publish should be built first (or their shared summary-formatting
  function extracted first), since History's detail view depends on what Review produces and stores.
- Filters shown in the mock ("All pages ▾", "Everyone ▾") — "Everyone" implies multi-editor attribution filtering,
  which the single-editor cut above already rules out; treat as **out of scope** alongside "People editing" unless
  told otherwise.
- 30-day retention footer note — matches decision 8 ("History is a separate capability... tiered on retention
  depth"); use whatever retention value the tier config already resolves, don't hardcode 30.

### Also outstanding

- Dialog "real design pass" items are done (#849) — Move/Delete/Remove all got selected-state highlighting and the
  Remove-item bug fixes above.
- Background tint/corner blob — see Cleanup list above; reopened, needs live-site Playwright investigation before
  another attempt.

## Suggested next step

Scope the Review & Publish page first (it's the smaller of the two full-page pieces, and History's detail view
depends on its summary format per A15) as its own brainstorming/design pass before implementation — this is
architectural work (new routed page, new data flowing through the stack), not a same-session polish fix.
