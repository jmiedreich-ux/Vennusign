# Vennusign Session Handoff

Updated 2026-08-09 after the Menus Milestone 1 rework that answers independent review #6.

## Current State

- Working model is **features and milestones** — read `AGENTS.md` first; the phase/track/WP workflow is retired.
- **All future tracks were cancelled by the owner on 2026-08-07** ahead of a full planning reset. Phase 13 is the final numbered phase; former Phases 14–16 are canceled, their ideas held in the unprioritized Backlog — TBD. RWP-13.06 is closed as not planned (#466), and #656–#662 are closed as `NOT_PLANNED`. The queued RWP-10.02, 11.03, 11.04, 13.04 and 13.05 packages are void as plans and survive only as history.
- Tracks 0 and 1 are complete and owner-closed. The Track 1 retrospective report sits on **PR #667 (open, unmerged)**; its process changes are already folded into `AGENTS.md`.
- **The planning reset produced the Menus feature.** Design authority: `docs/design/approved/menus/` (`decisions.md` wins conflicts). All 208 register questions are resolved in `docs/features/menus/open-questions.md`; the six-milestone plan in `docs/features/menus/milestone-plan.md` is reconciled with every answer.
- **Milestone 1 is implemented, owner-accepted, and then reworked five times after independent reviews #2 through #6 each returned REQUEST_CHANGES.** Branch `feature/menus-m1-spine`, issue #684, PR #685. It is deliberately **not merged**. **CI has not run on the current head** — the owner has not approved it, and the commits carry `[skip ci]`. The newest green checks belong to `ca187f0`, several commits back, and are not evidence about the head.
- **The save model is settled: the draft is derived, not authored** (owner decision, milestone-plan §The save model). The live rows are the working state; the screens show the last published snapshot; the draft is the computed difference. Migration 058 creates no draft table, and the legacy editor now writes through `Items`/`Placements` so no path can change a screen without a publish.
- Backlog issues #670–#683 hold the owner's out-of-scope decisions; do not silently implement them.
- Not yet approved, and inputs to any further planning: `docs/architecture/built-foundations-spec.md`, and the proposed product-surface inventory under `docs/design/proposed/` (Markdown plus a searchable HTML companion). Design references only.

## Read First

1. `docs/features/menus/milestone-plan.md` — milestones, cross-cutting rules, design follow-ups, quality gates.
2. `docs/features/menus/open-questions.md` — recorded owner answers; they govern over older prose.
3. `docs/design/approved/menus/README.md` + `decisions.md` — the design authority.

## Exact Next Action

1. **A seventh independent review of PR #685** — never by its author (issue #659). Every commit invalidates the previous review. Reviews #2 through #6 are all fully addressed: #2 and #3 have response comments on the PR, and for #4, #5 and #6 the commit messages are the author's account of each finding and how it was closed.
2. **Owner acceptance re-run.** `scripts/run-m1-demo.ps1` walks the derived model, including take-off through to publish and put-away; `docs/features/menus/m1-demo-workbook.html` carries the matching checks. `m1-acceptance-record.json` is **superseded** — it was signed against the authored-draft implementation and is kept as history only.
3. The three owner decisions from the first acceptance still stand: audit record kept as is (#677), legacy columns kept, and the three menu capabilities to become separately grantable (#686). The screen-conflict rule was settled on 2026-08-09: a screen another menu now owns is never touched by a stale act, and the conflict is always named — publish leaves it alone and reports it, restore refuses outright.
   The shelf rule was settled the same way: nothing puts a menu on a screen except a deliberate, ceiling-checked put-back, and nothing takes a menu off the shelf while a screen is still showing it. "Still on a screen" means the **published** snapshot names one that no other menu has since been given — not merely that an assignment row exists — so putting away requires take off, publish, then put away. A shelved menu stays editable and its draft stays discardable; only a restore that would put a screen back is refused.
4. **Do not merge PR #685** until the fresh review and the re-run acceptance both pass, and the owner has approved CI on the exact head and it is green. Milestone 2 starts only after it merges.

## Verification

**The head is unverified by CI.** Exact-head GitHub Actions were green (13 checks)
at `ca187f0`, and the third review confirmed that independently, but several
commits have landed since and the owner has not approved a run on them. Do not
cite the PR's check status as evidence about the current head.

Everything the branch claims is verified locally against real LocalDB and a
running product: unit tests, the data integration suite on a database migrated
from scratch, both UI suites, the Playwright specs and the owner demo.

Local execution and independent review together have now caught defects green CI
missed, including a phantom assignment count from PowerShell turning an empty JSON
array into `$null`, a migration-script list test failing since script 052, a
publish that recorded a shipped set from a different reading of the menu than the
snapshot it committed, a torn read of the published snapshot and its version, and
a menu that could be shelved with its take-off still pending — leaving a screen
showing content no remaining act could clear. Every one is fixed with a regression
test **verified to fail with its fix reverted**; that check is now part of closing
a finding, not an optional extra.

## After Milestone 1 — a retrospective item the owner named

Five consecutive independent reviews (#2 through #6) each found real defects in
work that had just been declared finished, and the throughline is consistent: the
tests written with a fix prove the case its author had in mind and stop there,
rather than attacking the next step in the sequence — publish twice, assign a
put-away menu, change only the letter casing, shelve a menu between a take-off and
the publish that carries it. Review #6 is the sharpest instance: the trap it found
was written up as a *passing test*, which asserted the refusal it hit and never
asked what the screen was still showing. Reviews are catching what the author's own
tests do not. **Decide what to change about how work is verified before it goes to
review** — owner instruction, to be taken up once M1 merges, not during it.

## Boundaries

- Do not start milestones 2–6 until milestone 1 is merged and its demo accepted.
- Do not revive any cancelled track, phase or void work package without fresh owner approval.
- Do not implement backlog issues #670–#683 without owner scheduling.
- Design follow-ups (milestone-plan §Design follow-ups) must be resolved before the milestone that consumes them.
