# Vennusign Project Status

## Current State

- **Working model: features and milestones** (adopted 2026-08-07 from the Track 1 retrospective; see `AGENTS.md`). The phase/track/WP model is retired; its records are history.
- **Phase 13 is the final numbered phase.** Former Phases 14, 15 and 16 are canceled as phases (owner, 2026-08-07). Their feature ideas survive only in the unprioritized **Backlog — TBD** and require fresh owner approval before planning or implementation.
- **All future tracks are cancelled** (owner, 2026-08-07) ahead of a full planning reset. No Track 2 or later track exists as a plan. The queued future work packages on file — RWP-10.02, RWP-11.03, RWP-11.04, RWP-13.04, RWP-13.05 — are void as plans; their documents remain only as history and require fresh owner approval before any reuse.
- **RWP-13.06 — Trial-First Onboarding: closed as not planned (#466).** Onboarding returns later as its own feature, freshly designed.
- Former `before-track-2` follow-ups **#656–#662: canceled by the owner and closed as `NOT_PLANNED`** (2026-08-07). They are not prerequisites for any later work.
- **Track 0** (industry and product architecture): complete and closed. Primary records under `track0/consolidation/` (research-only).
- **Track 1** (capability model, server decisions, scoped permissions, essential-core gate): complete, merged, exact-head validated, and **closed by the owner 2026-08-07**. Execution record: `ai/handoffs/archive/` and PRs #645–#650, #654. The retrospective report is on PR #667 (open, unmerged); its process changes are already incorporated into the feature/milestone model in `AGENTS.md`.

## The planning reset, and what it has produced

The cancellation above cleared the roadmap deliberately. The reset has since produced the features-and-milestones working model and its first feature.

- **Menus feature — active. Planning complete; Milestone 1 implemented, owner-accepted, and reworked after independent review.**
  - Design authority approved and merged: `docs/design/approved/menus/` (36 decisions, hi-fi M1/M2/M2c, wireframes, tokens).
  - All 208 open questions resolved across four owner sittings: `docs/features/menus/open-questions.md`.
  - Six-milestone plan reconciled with every recorded answer: `docs/features/menus/milestone-plan.md` (merged via PR #669).
  - Owner acceptance was recorded 2026-08-08 (`docs/features/menus/m1-acceptance-record.json`). The independent review then returned REQUEST_CHANGES; those findings are addressed and the branch reworked.
  - **Exact next action: a fresh independent review of PR #685, and a re-run of the acceptance workbook** — the earlier run predates the rework, and its availability and publish checks did not prove the claimed behaviour.
- Backlog from the Menus planning: issues #670–#683 (out-of-scope decisions, copy debt, accessibility debt).

### Planning-reset inputs, not yet approved

- `docs/architecture/built-foundations-spec.md` — what the product actually has today.
- `docs/design/proposed/product-surface-feature-inventory.md` and its searchable HTML companion — 18 customer-facing domains, 133 classified capabilities, a universal state vocabulary, navigation implications, unresolved owner decisions, and a Content and Menu industry deep dive. **A design reference, not a roadmap or an implementation approval.** The owner decides whether it becomes the approved input to the Release–Capability–Tier Matrix and future navigation design.
- `docs/design/customer-support-diagnostic-agent-concept.md` — exploratory product concept.

## Menus Feature Milestone Status

| Milestone | Scope | Status |
|---|---|---|
| 1 | Item library + draft/publish spine + assignment | reworked after review — PR #685, awaiting fresh review and re-run acceptance (#684) |
| 2 | App shell + render engine + Menus home | blocked on 1 |
| 3 | Builder + adding items | blocked on 2 |
| 4 | Display player + geometry + delivery | blocked on 3 |
| 5 | Board view + Play | blocked on 4 |
| 6 | Quick Update + import + confirm step | blocked on 5 |

## Validation Policy

Exact-head GitHub Actions is authoritative (`phase02-tests.yml`, `ui-regression.yml`). Azure SQL, live Stripe/providers, physical devices, hosted infrastructure and integration/external-system tests remain skipped by standing owner exception. Tests are written with each milestone's implementation; a milestone retires the legacy specs it obsoletes. Every milestone ends with an owner acceptance workbook (milestone 1: demo script and HTML workbook); hosted-agent subjective QA runs on demand.

## Historical Reference

Phases 1–13 and Tracks 0–1 are complete or canceled as described above. Their deliverables, execution records, and closure evidence live in `docs/archive/`, `ai/handoffs/archive/`, `track0/`, and the merged PR history. Do not load them routinely.
