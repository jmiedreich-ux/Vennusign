# RWP-04.02 — Platform Operations Safety and Support Workflows

Status: Complete in the proposed merge state

Issue: #343

Mode: Sequential

Branch: `rwp/04.02-platform-operations-safety`

## Goal

Make Platform Operations usable for support and commercial administration without changing its protected API boundaries. Operators can move from fleet or revenue signals to a filtered venue list and a venue support record, recover failed reads without losing context, inspect screen health, and review the impact of tier, feature, and archive changes before confirming them.

## Bounded Scope

- Add dashboard refresh, visible freshness, independent error recovery, actionable metrics, and fleet filters.
- Add persistent venue-directory search, tier/status/health filters, result states, and retry behavior.
- Add venue support refresh, screen status/version/last-seen detail, and deliberate review/confirm steps for tier and feature-override changes.
- Add review/confirm steps for bulk feature-matrix changes and tier create/edit/archive actions.
- Preserve the existing protected Platform Operations API, authorization, entitlement, audit-event, and tenant/commercial boundaries.

No new provider integration, background process, billing behavior, or Phase 14 work is included.

## UI and Function Gap Analysis

The analysis used the W3C WAI guidance for [status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages), [error identification](https://www.w3.org/WAI/WCAG22/Understanding/error-identification), [consistent identification](https://www.w3.org/WAI/WCAG22/Understanding/consistent-identification), and native [table patterns](https://www.w3.org/WAI/ARIA/apg/patterns/table/). Confirmation previews remain in-page and non-modal; they therefore do not incorrectly claim the modal-dialog interaction contract.

| Area | Gap | Implemented result |
| --- | --- | --- |
| Goals | Dashboard metrics and fleet signals were informative but not operational entry points. | Metric, screen, and commercial-event controls drill into filtered venue support context. |
| Navigation | Operators had to reconstruct a dashboard signal manually in the directory. | Dashboard-to-directory queries preserve status/health intent, and items with venue identity open the venue record. |
| CRUD actions | Tier, feature matrix, tier switch, and feature override actions committed immediately. | Material create/update/archive, bulk entitlement, tier-switch, and override changes require an explicit impact review followed by confirmation. |
| Essential states | Loading, empty, filtered-empty, stale, partial-failure, and success states were incomplete or indistinguishable. | Refreshing/freshness status, scoped errors, retry controls, result counts, true-empty versus filtered-empty copy, and action feedback are explicit. |
| Validation | Commercial-impact and screen-limit consequences were not visible before confirmation. | Tier review shows screen-limit conflicts and enabled/disabled feature changes; matrix review summarizes changed tiers and entitlement direction. Invalid tier switches are blocked before submission. |
| Destructive actions | Archive and entitlement removal lacked a deliberate review stage. | Archive, bulk entitlement changes, and override removal show their effect and require a second action; server-side audit/reconciliation behavior remains authoritative. |
| Accessibility and responsiveness | Async feedback was not consistently announced, tables lacked an explicit description, and support controls compressed poorly. | `role=status` announces progress/success, `role=alert` identifies failures, native buttons/labels/table semantics remain keyboard operable, the venue table has a caption, and support/filter layouts stack at narrow widths. |
| API/data support | UI recovery and impact preview needed reliable protected data without duplicating commercial rules in an untrusted client. | Existing Platform Operations endpoints continue to supply venue, tier, feature, screen, revenue, and event data. Client previews are advisory; existing server validation, authorization, Stripe mapping, audit, and entitlement enforcement remain final. |

## Implementation Result

- Dashboard reads fail independently, can be retried, show freshness, and retain functional paths when another panel is unavailable.
- Venue filters persist through retries and distinguish no data from no matching results.
- Venue support exposes screen status, version compliance, and last-seen evidence.
- Shared pure impact helpers calculate tier and bulk entitlement consequences and have focused behavior tests.
- Tier and entitlement mutations use review/confirm flows with accessible success and failure feedback.

## Validation

- Platform Operations focused Node tests, including the RWP-04.02 impact-helper and source-contract scenarios.
- Platform Operations production build.
- Exact reviewed PR head validated through affected-area GitHub Actions.

Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests are intentionally skipped under the standing project policy.
