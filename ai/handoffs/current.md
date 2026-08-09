# Vennusign Session Handoff

Updated 2026-08-09 after the Menus Milestone 1 rework that answers independent review #2.

## Current State

- Working model is **features and milestones** — read `AGENTS.md` first; the phase/track/WP workflow is retired.
- **All future tracks were cancelled by the owner on 2026-08-07** ahead of a full planning reset. Phase 13 is the final numbered phase; former Phases 14–16 are canceled, their ideas held in the unprioritized Backlog — TBD. RWP-13.06 is closed as not planned (#466), and #656–#662 are closed as `NOT_PLANNED`. The queued RWP-10.02, 11.03, 11.04, 13.04 and 13.05 packages are void as plans and survive only as history.
- Tracks 0 and 1 are complete and owner-closed. The Track 1 retrospective report sits on **PR #667 (open, unmerged)**; its process changes are already folded into `AGENTS.md`.
- **The planning reset produced the Menus feature.** Design authority: `docs/design/approved/menus/` (`decisions.md` wins conflicts). All 208 register questions are resolved in `docs/features/menus/open-questions.md`; the six-milestone plan in `docs/features/menus/milestone-plan.md` is reconciled with every answer.
- **Milestone 1 is implemented, owner-accepted, and then reworked twice after independent reviews returned REQUEST_CHANGES.** Branch `feature/menus-m1-spine`, issue #684, PR #685. It is deliberately **not merged**.
- **The save model is settled: the draft is derived, not authored** (owner decision, milestone-plan §The save model). The live rows are the working state; the screens show the last published snapshot; the draft is the computed difference. Migration 058 creates no draft table, and the legacy editor now writes through `Items`/`Placements` so no path can change a screen without a publish.
- Backlog issues #670–#683 hold the owner's out-of-scope decisions; do not silently implement them.
- Not yet approved, and inputs to any further planning: `docs/architecture/built-foundations-spec.md`, and the proposed product-surface inventory under `docs/design/proposed/` (Markdown plus a searchable HTML companion). Design references only.

## Read First

1. `docs/features/menus/milestone-plan.md` — milestones, cross-cutting rules, design follow-ups, quality gates.
2. `docs/features/menus/open-questions.md` — recorded owner answers; they govern over older prose.
3. `docs/design/approved/menus/README.md` + `decisions.md` — the design authority.

## Exact Next Action

1. **A fresh independent review of PR #685** — never by its author (issue #659). Every commit invalidates the previous review, and the branch has been reworked substantially since the last one. Review #2's findings are all addressed; the review comment on the PR names each one and how.
2. **Owner acceptance re-run.** `scripts/run-m1-demo.ps1` was rewritten for the derived model and reported 10 of 10 locally on 2026-08-09 against real LocalDB; `docs/features/menus/m1-demo-workbook.html` carries the matching checks. The owner still records the judgment.
3. The three owner decisions from the first acceptance still stand: audit record kept as is (#677), legacy columns kept, and the three menu capabilities to become separately grantable (#686).
4. **Do not merge PR #685** until the fresh review and re-run acceptance both pass, and exact-head CI is green — no CI ran for the 2026-08-09 push. Milestone 2 starts only after it merges.

## Local verification on 2026-08-09 (no CI run)

Run locally against real LocalDB and a running product, all green: 401 unit tests;
38 data integration tests (fresh database, migration applied from scratch); 109
back-office and 98 platform-operations UI tests; 56 Playwright specs (2 skipped);
and the owner demo at 10 of 10. Local execution caught two defects green CI would
have missed — a phantom assignment count from PowerShell's empty-array handling in
the demo script, and a stale migration-script list that had been failing in the
integration project since script 052.

## Boundaries

- Do not start milestones 2–6 until milestone 1 is merged and its demo accepted.
- Do not revive any cancelled track, phase or void work package without fresh owner approval.
- Do not implement backlog issues #670–#683 without owner scheduling.
- Design follow-ups (milestone-plan §Design follow-ups) must be resolved before the milestone that consumes them.
