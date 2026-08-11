# Menus M3 — owner acceptance findings, work package

**Repo:** `jmiedreich-ux/Vennusign` · **branch:** `feature/menus-m3-builder` · **head:** `93b4ac8`
**Issue:** #690 · **PR:** #691
**Source:** owner acceptance workbook run 2026-08-10 — 11 Pass, 2 Fail, 2 Needs Adjustment, closure **"Needs adjustment"**. Record: `menus-m3-acceptance-2026-08-10.json`.

## Read before starting

Authority order — **these govern, and they override this document**:

1. `docs/design/approved/menus/decisions.md`
2. `docs/design/approved/menus/README.md`
3. `docs/features/menus/open-questions.md` — 209 recorded owner answers. **These govern over older prose, including over the README.**
4. `docs/features/menus/milestone-plan.md` §Milestone 3
5. `AGENTS.md` — §How to Work a Task, §Definition of Done, §Where a test lives

**Two rules, both learned the hard way on this milestone:**

- **Before citing any Q number, open it and quote the recorded answer.** An automated reviewer filed a P0 citing "Q120's recorded answer" when Q120 explicitly backlogs the thing it demanded. Separately, a review prompt asserted Q103 deferred item drag to milestone 5 — Q103 defers only *cross-section* moves. Both were plausible and both were wrong.
- **A spec that passes is not evidence until you have watched it fail.** Revert the fix, run the spec, confirm it fails, restore. Item drag below is exactly this failure: a Playwright spec passed against a feature that does not work when a human uses it.

## Environment

```
scripts/start-ui-test-env.ps1              # start (API 7138, back office 5174, display 5175)
scripts/start-ui-test-env.ps1 -PruneSeed   # prune-only; run when the API smoke script 400s on the menu ceiling
scripts/start-ui-test-env.ps1 -Stop        # stop (required before dotnet build — the API locks its binary)
```

`VENU_TEST_AZURE_SQL_CONNECTION_STRING` must be **unset** per run; it is set persistently at User level on this machine and integration tests will hit Azure and fail login. CI is suspended by owner decision — local verification is the gate.

**Gate (all of it, at the final head):**

```
cd src/back-office && npm test && npm run build   # BOTH — the build is not optional, see note
cd tests/ui && npx playwright test                # desktop + mobile
dotnet test VennuSign.sln -c Debug
scripts/check-m3-builder-api.ps1
scripts/run-m1-demo.ps1
git diff --check
```

Baseline at `93b4ac8`: back office 190/190 · Playwright 144 passed / 0 failed · `Vennu.Api.Tests` 433 (+1 known failure) · `Vennu.Data.IntegrationTests` 91/91 · builder API 21/21 · M1 demo 12/12. **Four pre-existing failures on issue #688** (3 in `Vennu.DataAccess.Tests`, 1 E2E pairing) — verified pre-existing, do not chase them.

`npm run build` is called out because its absence is how this branch reached an independent review without compiling. `scripts/validate.ps1` now covers both front ends.

---

# A. Fix these — clear defects, no decision needed

## A1. The green "On the board" panel (case 7, **FAIL**)

> **Owner:** "I'm not sure where this green box came from, this goes against the design — any items changes are always queued for publishing except for 86 items"

**Where:** `src/back-office/src/menu-builder.css:751-758`

```css
.builder__availability {
  background: #e0f4e9;                        /* green tint */
  box-shadow: inset 0 0 0 1.5px #178a52;      /* green border */
}
.builder__availability.is-off { background: #fdeaea; ... }   /* red — this one is correct */
```

**The defect.** The milestone plan specifies only the *off* state: *"86'd rendering (item selectable and editable, **red-tinted 'Off right now' panel**, Q104)"*. The green panel for the ON state was invented and is not in any authority. Read Q104 in full before changing anything.

Its cost is exactly what the owner names: a permanently highlighted green box on an item in its normal, unremarkable state reads as a live/instant status covering the whole availability area — when in fact only the 86 flip is instant and every other edit on the page waits for Publish.

**Do — owner decision 2026-08-10: "remove it totally".** When the item is ON there is **no box at all** — no tint, no border, no coloured heading, no panel container. Just the availability switch as a plain inspector control, consistent with the four controls around it. This is a removal, not a restyle to a lighter green.

The OFF state keeps its red panel exactly as it is — Q104 specifies it, and it is the only tinted state the design asks for. Keep the verbatim copy in the OFF panel body untouched: it is quoted in §Verbatim copy of the design README and carries the clause that separates 86 from everything else on the page.

**Verify:** Playwright — assert no panel/tint element exists when the item is available, and that the red treatment and its copy appear when off. Existing 86 specs must stay green.

## A2. Item drag does not work, and shows nothing when it does (case 13, **FAIL**)

> **Owner:** "The drag item did not work in one section view and there was no 'placeholder' line to show where to drop it."

**This is the important one.** Two separate problems.

### A2a. It does not actually drag

`tests/ui/specs/menu-builder.spec.ts` has a passing spec (`"an item is dragged to a new place on its own section (Q103)"`) using Playwright's `dragTo`. It passes. The feature does not work for a human. Treat the existing spec as **not evidence** and do not trust it.

**Prime suspect — `BoardStage` re-renders continuously during the drag.** `src/back-office/src/MenuBuilder.tsx`, the `BoardStage` component:

```ts
useEffect(() => {
  const measure = () => { ...; setScale(next); setHeight(inner.current.scrollHeight * next); };
  measure();
  if (typeof ResizeObserver === "undefined") return;
  const observer = new ResizeObserver(measure);
  ...
  return () => observer.disconnect();
});   // <-- NO dependency array: runs after EVERY render
```

`measure()` calls `setScale`/`setHeight` on every render with no dependency array. If `scrollHeight * scale` jitters by a sub-pixel the component re-renders in a loop, React re-creates the `<li>` that is mid-drag, and Chromium cancels the drag. A fast synthetic `dragTo` can complete inside that window; a human drag cannot. Confirm with a render counter or a `dragstart`/`dragend` log before fixing.

Second suspect if that is not it: the board is inside `transform: scale(...)`; HTML5 drag hit-testing inside a scaled container is unreliable in Chromium. If so, drop HTML5 DnD for pointer events (`pointerdown`/`pointermove`/`pointerup` with capture), which is scale-safe.

**Reproduce in a real browser first.** Open the builder, one-section view, and drag with an actual mouse. Do not begin from the spec.

### A2b. There is no drop indicator

No drop-placeholder was built at all. Add a visible insertion line showing where the row will land, tracking the pointer across `dragover`. It must be sized against the board scale — `--board-scale` is published by `BoardStage`, and both `.board-item-note` and the `⠿` pill already divide by it. A fixed pixel size renders at roughly a third of its intended size inside a board drawn at ~0.35 and is invisible.

**Scope:** within-section reorder only. Q103 defers cross-section moves to milestone 5, and a drop onto another section already refuses in words — keep that.

**Verify:** a real-browser check plus a spec that fails against the current code. If you cannot make a spec fail against the broken version, the spec is not testing the defect.

## A3. Deleting a section leaves the canvas empty (case 10, Pass with note)

> **Owner:** "Deleting should reverse back the previous section, to avoid nothing displaying"

**Where:** the delete-section path in `src/back-office/src/MenuBuilder.tsx` (`confirmDelete` → `deleteMenuSection`).

After a delete the builder holds a `place.sectionId` that no longer exists and the canvas renders nothing. Select the **previous** section (or the first remaining one if the deleted section was first), and clear `selectedItemId`. If no sections remain, show the existing empty-board state — Q96: *"an empty board shows just the add affordance."* Read Q96 before implementing.

**Verify:** extend the existing delete-section spec — after deleting, a section is selected and the canvas is not blank.

---

# B. Decided by the owner — implement as written

These were raised as conflicts with recorded answers and the owner has now ruled on each, 2026-08-10. **Where a decision overrides a recorded answer that is stated explicitly below — record it, do not bury it.** The register entry gets a dated note; the decision does not get to look like a slip later.

## B1. "Delete this section" moves into the Sections list — **overrides Q96**

> **Owner:** "maybe have a little red x or garbage can next to each section"

**Do:** a small red ✕ or bin icon on each row of the left-hand Sections list, acting on that row's section. It keeps the existing confirmation dialog — the delete is irreversible and the dialog names how many items go back to the library, which is the part Q96 actually protects.

**Remove** the `Delete this section` link from under the board.

**This overrides Q96**, which reads: *"rename by clicking the canvas heading and typing over it; **a quiet delete control with the heading**; deleted sections release their items back to the library."* It also makes the Sections list an editor as well as a navigator, which the design README currently rules out (*"a navigator, not a second editor"*). Both were put to the owner and this is their answer.

**Record it:** add a dated owner-override note under Q96 in `docs/features/menus/open-questions.md`, and correct the rail's description in `docs/design/approved/menus/README.md` §Component sheet so the authority stops saying something the product no longer does. Per `AGENTS.md` §Documentation Control, a record that states something untrue is a defect.

## B2. Remove the section-name field from under the board

> **Owner:** "what is the purpose of the section name at the bottom of the screen?"

**Do:** remove it. Renaming is done by clicking the canvas heading (Q96), built at `93b4ac8`, and the field is now a second way to do the same thing.

The objection raised was that this field is the only keyboard-reachable rename. **The owner has ruled keyboard out of scope for this build — see B3 — so that objection does not stand.** Remove the field.

With B1 and B2 both done, the strip under the board is empty except for the add-item row — remove the now-empty container rather than leaving it collapsed.

## B3. Keyboard is out of scope for this build — owner ruling, reaffirmed

> **Owner:** "I keep telling you keyboard is out of scope completely."

**Treat this as settled and stop raising it.** Do not build keyboard affordances, do not hold work for keyboard reachability, and do not file keyboard findings against this milestone. Q202 (canvas reachability) and Q120 (keyboard reorder) are already backlogged to **#672**; this ruling extends that to the build as a whole.

**Consequence for the workbook — do this:** case 15 currently instructs the owner to press `Ctrl+Z`. Change the step to use the on-screen **Undo** button in the top bar (`data-testid="undo"`). An acceptance step must not depend on something ruled out of scope.

**One thing left alone, stated once and not to be re-litigated:** the `Ctrl+Z` / `⌘K` handlers already built stay in the code. "Out of scope" means nothing further is built or tested for keyboard — it does not mean deleting working behaviour that `milestone-plan.md` §Milestone 3 names (*"Undo/redo keystroke, session-scoped"*, and Q121 for ⌘K). If the owner wants them physically removed they will say so; do not remove them on your own reading, and do not ask again.

---

# C. Workbook and fixture, not product

## C1. Case 6 could not be run (Needs Adjustment)

> **Owner:** "Was not able to test this as only 1 screen is paired"

Case 6 is about an item on more than one **board** (menu) — nothing to do with screens. The owner read "board" as "screen", and the case asks them to set the condition up themselves by duplicating a menu first.

**Do both:**
- Reword the case so it cannot be read as a screens case — say "on two menus" and drop the setup step.
- Pre-seed the condition in `docs/acceptance/track-1-owner-fixture.sql` so a shared item already exists on two menus when the fixture is applied. An acceptance case the owner has to construct before running is a case that will keep getting skipped.

Workbook conventions live in `docs/features/menus/milestone-plan.md` §Acceptance workbook conventions — read them, and add anything you learn.

---

# D. Deferred by the owner — do not chase

## D1. The truncated final notes

The record's `finalNotes` reads, in full: **"A few visual notes - We need"** — cut off mid-sentence. There are visual findings the owner intended to give that the record does not contain.

**Owner decision 2026-08-10: these come after this set is done.** Do not ask for them, and do not treat their absence as a blocker. Finish A, B and C, then hand back and the owner supplies the visual notes as a second pass.

---

# Definition of done for this package

- **A1, A2, A3, B1, B2 implemented**, each with a spec **verified to fail against the unfixed code** — revert the fix, run it, watch it fail, restore. A passing spec written after the fix proves nothing; that is how the drag defect in A2 shipped.
- **B1's override of Q96 recorded** in `open-questions.md` with a dated note, and the rail's description corrected in the design README.
- **B3 applied to the workbook** — case 15 no longer asks for a keystroke.
- **C1** done in both the workbook and the fixture.
- **D1 left alone** until this set is handed back.
- Full gate green at the new head; the four #688 failures unchanged; `npm run build` included.
- `PROJECT_STATUS.md`, `ai/handoffs/current.md`, issue #690 and PR #691 updated to say what changed and what is still open.
- The owner re-runs the acceptance workbook afterwards — closure was **"Needs adjustment"**, so M3 does not merge on the existing record.

## What not to do

- Do not re-open B1, B2 or B3. They were raised as conflicts, put to the owner, and answered. Implement them.
- Do not file keyboard findings.
- Do not trust a green spec you have not seen fail.
- Do not resolve a conflict with a recorded answer by judgment. Quote the register entry and hand it back — except for the three above, which are already settled.
