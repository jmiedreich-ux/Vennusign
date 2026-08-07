# Vennusign Session Handoff

Updated 2026-08-07 after the Menus planning close-out and repository cleanup.

## Current State

- Working model is **features and milestones** — read `AGENTS.md` first; the phase/track/WP workflow is retired.
- Tracks 0 and 1 are complete and owner-closed. Their records are archival.
- **Menus feature is active with planning complete and implementation not started.** Design authority: `docs/design/approved/menus/` (`decisions.md` wins conflicts). All 208 register questions are resolved in `docs/features/menus/open-questions.md`; the six-milestone plan in `docs/features/menus/milestone-plan.md` is reconciled with every answer.
- Backlog issues #670–#683 hold the owner's out-of-scope decisions; do not silently implement them.
- No agent holds a claim. `tracker/assignments.json` is empty and there is no desktop session lock.

## Read First

1. `docs/features/menus/milestone-plan.md` — milestones, cross-cutting rules, design follow-ups, quality gates.
2. `docs/features/menus/open-questions.md` — recorded owner answers; they govern over older prose.
3. `docs/design/approved/menus/README.md` + `decisions.md` — the design authority.

## Exact Next Action

Execute **Milestone 1 — the spine** (item library + draft/publish save model + menu↔screen assignment) per the milestone plan:

1. Create the milestone issue; record the claim in the tracker.
2. Branch `feature/menus-m1-spine`; implement schema, migration (names what it discards), API, and focused tests together.
3. Open one PR; exact-head CI green; independent review (never the author).
4. Deliver the demo script for the owner's acceptance. **Do not merge without owner review.**

## Boundaries

- Do not start milestones 2–6 until milestone 1 is merged and its demo accepted.
- Do not resume RWP-13.06 unchanged; onboarding returns later as its own build.
- Do not implement backlog issues #670–#683 without owner scheduling.
- Design follow-ups (milestone-plan §Design follow-ups) must be resolved before the milestone that consumes them.
