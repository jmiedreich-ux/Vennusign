# Vennusign Session Handoff

Updated 2026-08-07 after the Menus planning close-out and repository cleanup.

## Current State

- Working model is **features and milestones** — read `AGENTS.md` first; the phase/track/WP workflow is retired.
- Tracks 0 and 1 are complete and owner-closed. Their records are archival.
- **Menus feature is active; Milestone 1 is implemented and awaiting owner acceptance.** The M1 branch is `feature/menus-m1-spine` (issue #684) with the demo script at `docs/features/menus/m1-demo-script.md`. Design authority: `docs/design/approved/menus/` (`decisions.md` wins conflicts). All 208 register questions are resolved in `docs/features/menus/open-questions.md`; the six-milestone plan in `docs/features/menus/milestone-plan.md` is reconciled with every answer.
- Backlog issues #670–#683 hold the owner's out-of-scope decisions; do not silently implement them.
- The M1 claim is recorded in `tracker/assignments.json`. There is no desktop session lock.

## Read First

1. `docs/features/menus/milestone-plan.md` — milestones, cross-cutting rules, design follow-ups, quality gates.
2. `docs/features/menus/open-questions.md` — recorded owner answers; they govern over older prose.
3. `docs/design/approved/menus/README.md` + `decisions.md` — the design authority.

## Exact Next Action

1. Walk `docs/features/menus/m1-demo-script.md` against the running API and record Pass / Fail / Needs Adjustment.
2. Review the M1 pull request independently (never the author, per issue #659) and confirm exact-head CI.
3. Three items are flagged in the demo for an explicit owner decision: the provisional audit record (Q207), the provisional capability grants (Q24), and the deferred legacy column drops.
4. **Do not merge without owner review.** Milestone 2 starts only after M1 merges and its demo is accepted.

## Boundaries

- Do not start milestones 2–6 until milestone 1 is merged and its demo accepted.
- Do not resume RWP-13.06 unchanged; onboarding returns later as its own build.
- Do not implement backlog issues #670–#683 without owner scheduling.
- Design follow-ups (milestone-plan §Design follow-ups) must be resolved before the milestone that consumes them.
