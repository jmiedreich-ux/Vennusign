# RWP-05.05 — Screen, Theme, and Pairing Lifecycle Recovery

## Outcome

Back Office operators can now recover screen and pairing failures without deleting device identity or relying on hidden support actions. Screens support recoverable archive/restore, stale connection reset, and unpair-for-replacement; active capacity excludes archived records; video walls support deliberate edit/removal; and themes state their venue-wide scope, selectable preview target, reset action, and readability feedback.

## UI and Function Gap Analysis

The analysis followed the WAI-ARIA modal/alert dialog patterns and WCAG 2.2 guidance for input assistance and status messages. Native confirmation dialogs are used for the bounded destructive actions so focus, cancellation, keyboard use, and announcement remain browser-managed.

| Area | Goal and navigation | CRUD/lifecycle actions | Essential states and validation | Destructive safety | Accessibility and responsiveness | Required support |
| --- | --- | --- | --- | --- | --- | --- |
| Screen fleet | Find the correct player by name, location, health, or platform without leaving Screens. | Create, edit, push, archive, restore, reset connection, and unpair for replacement. | Loading, empty, filtered-empty, online/offline/stale/archived, last-seen, platform/version, quota, queued-online, queued-offline, and failure feedback. Pairing distinguishes not-found, expired, duplicate/limit, pending, and general failure recovery. | Archive is recoverable; unpair and reset require explicit confirmation; permanent delete and direct venue transfer remain excluded. | Search and filter are labeled; failures use alerts, progress/outcomes use status messages; native confirmation supports keyboard/focus; controls wrap through the existing responsive form/list layout. | Venue-scoped authorization, ownership checks, entitlement limits, stable device identity, heartbeat authority, and realtime notification remain server-side. |
| Video walls | Re-enter an existing wall configuration from the Screens workspace. | Create, edit membership/order/layout, cancel edit, remove. | Loading, empty, unavailable entitlement, validation, saving, success, and failure states. Archived screens cannot be selected. | Removal requires confirmation and returns screens to independent layouts; edit can be cancelled without persistence. | Buttons have explicit types and labels; errors/status are announced; ordered membership remains visible at narrow widths. | Feature resolution, venue ownership, unique screen validation, archived-screen exclusion, and venue notification remain authoritative. |
| Theme | Understand that saved themes affect the venue while preview selection affects only the preview. | Edit basic/advanced values, apply preset, select preview target, save, reset defaults. | Loading, no-screen preview, save/reset failure, saved/queued outcome, and contrast ratios for basic and title palettes. | Reset requires confirmation and restores deterministic server defaults. | Scope is text, not color alone; preview selector is labeled; contrast warnings target at least 4.5:1; outcomes are announced. | Server validates colors/fonts/ranges, persists one venue theme, and broadcasts theme updates to active players. |

## Server and Compatibility Contract

- `PUT /api/back-office/venues/{venueId}/screens/{screenId}/lifecycle` archives or restores an owned screen.
- `POST /api/back-office/venues/{venueId}/screens/{screenId}/reset` clears stale heartbeat state without changing device identity.
- `DELETE /api/back-office/venues/{venueId}/screens/{screenId}/pairing` releases venue ownership for replacement while preserving the durable screen key. It is intentionally not a permanent delete or direct transfer.
- Archived screens remain queryable for recovery but cannot heartbeat, receive manual pushes, count toward active entitlement, appear in video-wall targets, or serve display content.
- `DELETE /api/back-office/venues/{venueId}/theme` restores deterministic venue-wide defaults and publishes the authoritative theme update. The Platform Operations compatibility surface exposes the same reset contract.
- Existing `Screen.Status`, nullable `VenueId`, wall membership, and heartbeat fields support these transitions; no schema migration is required.

## Validation

- Back Office Node tests: 48 passed locally.
- Back Office production build: passed locally. Focused .NET unit tests are delegated to exact-head affected-area GitHub Actions because the local .NET SDK is unavailable.
- Added focused coverage for lifecycle ownership/transitions, archive heartbeat protection, capacity exclusion, archived video-wall rejection, display blocking, pairing guidance, safe video-wall actions, theme reset, scope, preview selection, and contrast feedback.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, live player, and all other integration-type tests.

## Research References

- [WAI-ARIA modal dialog pattern](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/)
- [WAI-ARIA alert dialog example](https://www.w3.org/WAI/ARIA/apg/patterns/alertdialog/examples/alertdialog/)
- [WCAG 2.2 input assistance](https://www.w3.org/WAI/WCAG22/Understanding/input-assistance)
- [WCAG 2.2 status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages)

## Completion

- Issue: #345
- Mode: Sequential
- Branch: `rwp/05.05-screen-theme-pairing-recovery`
- Next approved package after merge and claim release: RWP-05.06 / issue #419
