# RWP-09.01 — Tap-List Lifecycle and Operational Scale

## Outcome

Tap-list administration now supports safe lifecycle work and large operational lists without changing the server authorization or persistence boundary.

## UI and Function Gap Analysis

The review applied WCAG 2.2 error-identification, input-assistance, status-message, keyboard, and responsive-control guidance.

| Area | Goal/navigation and CRUD | Essential states and validation | Destructive/high-impact safety | Accessibility/responsiveness | Required support |
| --- | --- | --- | --- | --- | --- |
| Categories | Create, edit, enable, reorder, delete, and expose group size. | Empty list, dependency count, save/order/delete errors, success, retry. | Populated categories fail closed; empty deletion repeats the category name and permanence. | Named move controls, native inputs, text dependency state, alerts/status; rows collapse on narrow screens. | Venue scope, category membership, complete ordering, realtime venue notification. |
| Tap items | Create/edit descriptions and existing fields; search/filter by category; reorder canonical list. | Empty/filter-empty, 1,000-character description, validation error, saved/push feedback, retry. | Delete repeats tap name and exact display position. | Search input, native group filter, labeled controls, visible focus/native keyboard, responsive grid. | Existing description persistence/display, validation, venue category authorization, canonical order. |
| Placement/bulk | Preview visible/overflow positions and change availability for selected rows. | Capacity summary, per-row position, selected count, 25-row cap, clear, success/error/retry. | Selection fails closed at 25 and affects only named selected rows. | Checkbox selection, text not color-only, native buttons and live regions, mobile stacking. | Server-authorized item updates and queued realtime venue notifications; no acknowledgement is inferred. |

## Validation

- Back Office Node tests: 55 passed locally.
- Back Office production build: passed locally.
- Added focused data-service coverage for description normalization and maximum length.
- Focused .NET validation is delegated to exact-head affected-area GitHub Actions because the local SDK is unavailable.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests.

## Research References

- [WCAG 2.2 error identification](https://www.w3.org/WAI/WCAG22/Understanding/error-identification)
- [WCAG 2.2 input assistance](https://www.w3.org/WAI/WCAG22/Understanding/input-assistance)
- [WCAG 2.2 status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages)

## Completion

- Issue: #414
- Mode: Sequential
- Branch: `rwp/09.01-tap-list-lifecycle-scale`
- Next approved package after merge and claim release: RWP-10.01 / issue #423
