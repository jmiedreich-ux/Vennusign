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

- **Menus feature — active. Planning complete; Milestones 1 and 2 merged (PR #685/#684 on 2026-08-09, PR #689/#687 on 2026-08-10).**
  - Design authority approved and merged: `docs/design/approved/menus/` (36 decisions, hi-fi M1/M2/M2c, wireframes, tokens).
  - All 208 open questions resolved across four owner sittings: `docs/features/menus/open-questions.md`.
  - Six-milestone plan reconciled with every recorded answer: `docs/features/menus/milestone-plan.md` (merged via PR #669).
  - Five independent reviews (#2 through #6) each returned REQUEST_CHANGES and each found real defects. All findings are closed, every one with a regression test verified to fail with its fix reverted. Merged on 13 green exact-head checks at `2977bc3`.
  - **Accepted by the owner 2026-08-09.** Milestone 1 shipped no new UI, so its acceptance is the demo script rather than a workbook walk: `scripts/run-m1-demo.ps1` passes 12 of 12, including assertions of what each screen is actually showing. `docs/features/menus/m1-acceptance-record.json` remains **superseded** — signed 2026-08-08 against the earlier authored-draft implementation — and is kept as history.
  - **Milestone 2 merged and accepted 2026-08-10** (PR #689, issue #687). Owner ran the acceptance workbook: 11 of 11 Pass, closure "Accept Milestone 2", record at `docs/features/menus/m2-acceptance-record.json`. One independent review; its three blocking defects are fixed. It asked for a second review of the resulting head and the owner waived that, judging the first sufficient — milestone 1 took five reviews, milestone 2 took one.
  - **Milestone 3 is merged and closed** — PR #691 merged to `master` as `6bf0f75` on 2026-08-11, branch deleted, issue #690 closed. The builder: four columns, the canvas as the preview, adding items, the bulk drawer, item drag, undo/redo and the publish bar. **It did not close on a green acceptance record.** The owner's 2026-08-10 record stands at "Needs adjustment" (11 Pass, 2 Fail, 2 adjust); its findings were remediated and the owner instructed merge without a rerun. Both Fails were verified independently before merge rather than accepted on report, because both had previously passed their tests while broken. Six readiness-pass decisions remain provisional and recorded in #690.
  - **The independent review returned REQUEST_CHANGES with seven findings, and every one was real** — including a back office that did not compile in production mode, because `validate.ps1` built `src/display` and had never heard of `src/back-office`. An eighth was found in the review prompt itself, which had put an in-scope gap (Q103's item drag) on the do-not-file list. All eight are fixed at `b59d2d1`, each with a spec run against its own reverted fix and observed to fail. `validate.ps1` now builds both front ends.
  - **The owner ran the acceptance workbook 2026-08-10 and returned "Needs adjustment"** — 11 Pass, 2 Fail, 2 Needs Adjustment across fifteen checks. Record at `docs/features/menus/m3-acceptance-record.json`. **M3 does not merge on this record.** The two Fails: an invented green "On the board" panel that the design never specified (only the red 86 panel is, Q104), and item drag that does not work under a real mouse with no drop indicator — the latter passing its Playwright spec, which is the third green-spec-over-broken-feature in this milestone. Owner decisions taken on the back of it: the delete control moves into the Sections list (overriding Q96, recorded there), the duplicate section-name field goes, and keyboard is out of scope for the build.
  - **The acceptance findings are worked and locally gated.** The green availability panel is gone; human handle-origin drag works with a visible insertion line; deletion selects the previous/first surviving section; delete lives on each Sections row; the duplicate name field is gone; the workbook uses on-screen Undo; and the fixture pre-seeds one shared item on two menus. Every product regression was observed red with its fix absent. Mobile interactions remain out of scope (Q158/#681), reaffirmed by the owner.
  - **Exact next action: obtain independent re-review of Menus M3-A Slice 3 product SHA `0e7c54c`, then run owner acceptance.** The external #703 findings are remediated. Release/build, 198/198 frontend units, focused API/LocalDB, and the 16-shard Playwright gate pass; the latter discovered 220 cases, with 142 passed and 78 explicit scope skips. Push, PR, merge and issue/tracker closure still wait for review and owner acceptance. Page-lifecycle history and idempotent section-create retries remain tracked in #701 and #702.
  - The register has one open question again — **Q209**, deferred at M2 acceptance: the ⋯ card actions cover guest content now that Q98 removed the venue-name strip. It ships on its provisional default until settled.
- Backlog from the Menus planning: issues #670–#683 (out-of-scope decisions, copy debt, accessibility debt).

### Planning-reset inputs, not yet approved

- `docs/architecture/built-foundations-spec.md` — what the product actually has today.
- `docs/design/proposed/product-surface-feature-inventory.md` and its searchable HTML companion — 18 customer-facing domains, 133 classified capabilities, a universal state vocabulary, navigation implications, unresolved owner decisions, and a Content and Menu industry deep dive. **A design reference, not a roadmap or an implementation approval.** The owner decides whether it becomes the approved input to the Release–Capability–Tier Matrix and future navigation design.
- `docs/design/customer-support-diagnostic-agent-concept.md` — exploratory product concept.

## Menus Feature Milestone Status

Menus M3-A Slices 1 and 2 are owner-approved. Slice 2's first independent review
returned REQUEST_CHANGES; both blockers were remediated with observed red/green
regression evidence. The owner accepted the product and explicitly waived any
further independent review. CI remains suspended, so local evidence was the gate.

| Milestone | Scope | Status |
|---|---|---|
| 1 | Item library + draft/publish spine + assignment | **complete** — merged and accepted 2026-08-09 (PR #685, #684) |
| 2 | App shell + render engine + Menus home | **complete** — merged and accepted 2026-08-10 (PR #689, #687) |
| 3 | Builder + adding items | **complete** — merged to `master` as `6bf0f75` 2026-08-11 (PR #691, issue #690); closed on a remediated "Needs adjustment" record at the owner's instruction |
| 4 | Display player + geometry + delivery | blocked on 3 |
| 5 | Board view + Play | blocked on 4 |
| 6 | Quick Update + import + confirm step | blocked on 5 |

## Validation Policy

**CI is suspended by owner decision, 2026-08-09** (see `AGENTS.md` §Testing and CI): local verification is the gate — affected Release builds, the unit and LocalDB integration suites with the invariant sweep, the Playwright gate, and the owner demo/workbook — and pushes carry `[skip ci]`. When the owner restores CI, exact-head GitHub Actions (`phase02-tests.yml`, `ui-regression.yml`) is authoritative again. Azure SQL, live Stripe/providers, physical devices, hosted infrastructure and integration/external-system tests remain skipped by standing owner exception. Tests are written with each milestone's implementation; a milestone retires the legacy specs it obsoletes. Every milestone ends with an owner acceptance workbook (milestone 1: demo script and HTML workbook); hosted-agent subjective QA runs on demand.

## Historical Reference

Phases 1–13 and Tracks 0–1 are complete or canceled as described above. Their deliverables, execution records, and closure evidence live in `docs/archive/`, `ai/handoffs/archive/`, `track0/`, and the merged PR history. Do not load them routinely.
