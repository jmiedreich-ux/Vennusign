# Vennusign Project Status

## Current State

- **Working model: builds and slices** (adopted 2026-08-07 from the Track 1 retrospective; see `AGENTS.md`). The phase/track/WP model is retired; its records are history.
- **Track 0** (industry and product architecture): complete and closed. Primary records under `track0/consolidation/` (research-only).
- **Track 1** (capability model, server decisions, scoped permissions, essential-core gate): complete, merged, exact-head validated, and **closed by the owner 2026-08-07**. Execution record: `ai/handoffs/archive/` and PRs #645–#650, #654. The retrospective's process changes are incorporated into the build/slice model.
- **Menus build — active, planning complete, implementation not started.**
  - Design authority approved and merged: `docs/design/approved/menus/` (36 decisions, hi-fi M1/M2/M2c, wireframes, tokens).
  - All 208 open questions resolved across four owner sittings: `docs/builds/menus/open-questions.md`.
  - Six-slice plan reconciled with every recorded answer: `docs/builds/menus/slice-plan.md` (merged via PR #669).
  - **Exact next action: execute Slice 1** (item library + draft/publish spine) per the slice plan — issue, claim, branch, PR, demo script. Do not merge without owner review.
- Backlog from the Menus planning: issues #670–#683 (out-of-scope decisions, copy debt, accessibility debt).
- RWP-13.06 — Trial-First Onboarding: held and must not resume unchanged; onboarding is expected to be redesigned as its own build later.
- Legacy `before-track-2` issues #656–#662: under review against the new model.

## Menus Build Slice Status

| Slice | Scope | Status |
|---|---|---|
| 1 | Item library + draft/publish spine + assignment | not started |
| 2 | App shell + render engine + Menus home | blocked on 1 |
| 3 | Builder + adding items | blocked on 2 |
| 4 | Display player + geometry + delivery | blocked on 3 |
| 5 | Board view + Play | blocked on 4 |
| 6 | Quick Update + import + confirm step | blocked on 5 |

## Validation Policy

Exact-head GitHub Actions is authoritative (`phase02-tests.yml`, `ui-regression.yml`). Azure SQL, live Stripe/providers, physical devices, hosted infrastructure and integration/external-system tests remain skipped by standing owner exception. Tests are written with each slice's implementation; a slice retires the legacy specs it obsoletes. Every slice ends with an owner acceptance workbook (slice 1: demo script); hosted-agent subjective QA runs on demand.

## Historical Reference

Phases 1–13 and Tracks 0–1 are complete. Their deliverables, execution records, and closure evidence live in `docs/archive/`, `ai/handoffs/archive/`, `track0/`, and the merged PR history. Do not load them routinely.
