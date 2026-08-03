# RWP-08.01 — Scheduling and Live-Control Safety

## Outcome

Back Office scheduling is now a coherent, target-explicit workspace. Operators can understand server precedence, navigate directly to a task, persist priority and enable state, safely manage screen playlists and promotions, and activate or cancel emergency overrides only after reviewing their impact.

## UI and Function Gap Analysis

The analysis used the WAI-ARIA tabs and alert-dialog patterns plus WCAG 2.2 error-identification, input-assistance, and status-message guidance.

| Surface | Goal and navigation | CRUD and lifecycle | Essential states and validation | Destructive/live safety | Accessibility and responsiveness | Required support |
| --- | --- | --- | --- | --- | --- | --- |
| Scheduling workspace | Replace one long stack with task tabs, deep links, and a precedence overview. | Read all scheduling categories; move between overview, meals, happy hour, playlists, promotions, and emergency. | Loading, target-load failure, no screens, empty schedules, and normal-content fallback are explicit. | Overview explains emergency precedence and target requirements. | `tablist`, `tab`, `tabpanel`, selected state, arrow/Home/End keys, visible focus, and horizontally scrollable narrow layout. | Server-authorized venue context, capabilities, and screen list. |
| Meal periods | Make venue-local priority and persistence clear. | Create, edit, enable/disable, reorder, and delete. | Empty list, overlap conflicts, invalid active days, save/order errors, and success notices. | Delete confirmation; ordering is a complete server-validated operation. | Named controls, pressed state, status/error live regions, and textual conflict feedback. | Venue scope, saved order, enabled state, timezone resolver, layout/menu/theme targets. |
| Screen playlists | Require a selected screen and expose the full slide lifecycle. | Create, edit, enable/disable, reorder, remove, select days and local windows. | No screen, no slides, edit/cancel, busy, validation, save/order/remove failure, and success. | Removal confirmation and no implicit target. | Native screen select, labeled controls, day checkboxes, named order buttons, status/error feedback, responsive layout. | Authorized screen membership, entitlement, playlist persistence, venue-local eligibility. |
| Promotions | Explain priority and provide recoverable feedback. | Create, edit, enable, and archive. | Empty/current list, invalid date range, save/archive errors, and success. | Archive confirmation. | Native date/number controls and live status/error text. | Venue-local resolver, deterministic priority, entitlement, realtime venue notification. |
| Emergency overrides | Show target impact, active state, queued delivery, and history. | Activate for venue/screen; cancel; read active and recent history. | No screens, no active override, busy, error, queued outcome, expiry/cancellation history. | Activation and cancellation confirmations repeat target and duration; zero targets fail closed. | Semantic fieldset/legend, native target selector, alerts/status, text-based delivery state, responsive history. | Venue/screen authorization, entitlement, persisted lifecycle, realtime notification. Player acknowledgements are not available and are not inferred. |

## Implementation

- Added additive meal-period current/next status and order contracts with complete-order validation and focused resolver/service tests.
- Added playlist update client support for the existing server endpoint.
- Added task navigation, target states, confirmations, editable playlist fields, day controls, status feedback, and emergency history.
- No database migration was required.

## Validation

- Back Office Node tests: 55 passed locally.
- Back Office production build: passed locally.
- Focused .NET tests are delegated to exact-head affected-area GitHub Actions because the local SDK is unavailable.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests.

## Research References

- [WAI-ARIA tabs pattern](https://www.w3.org/WAI/ARIA/apg/patterns/tabs/)
- [WAI-ARIA alert-dialog pattern](https://www.w3.org/WAI/ARIA/apg/patterns/alertdialog/)
- [WCAG 2.2 error identification](https://www.w3.org/WAI/WCAG22/Understanding/error-identification)
- [WCAG 2.2 input assistance](https://www.w3.org/WAI/WCAG22/Understanding/input-assistance)
- [WCAG 2.2 status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages)

## Completion

- Issue: #346
- Mode: Sequential
- Branch: `rwp/08.01-scheduling-live-control-safety`
- Next approved package after merge and claim release: RWP-09.01 / issue #414
