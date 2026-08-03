# RWP-05.06 — Back Office Organization and Venue Context

## Outcome

Every authenticated Back Office screen now identifies the active organization and venue separately from the signed-in customer account. Accounts with multiple authorized venues can switch deliberately through a persistent native selector; the choice is remembered only after the server validates current membership and manage-content permission.

## UI and Function Gap Analysis

The analysis followed the WAI-ARIA select-only combobox guidance, WCAG 2.2 consistent identification, error identification, input assistance, and status-message guidance. A native `select` preserves familiar keyboard, touch, zoom, and assistive-technology behavior without recreating combobox semantics.

| Area | Goal and navigation | CRUD/lifecycle actions | Essential states and validation | Destructive/high-impact safety | Accessibility and responsiveness | Required support |
| --- | --- | --- | --- | --- | --- | --- |
| Persistent context | Make the active organization, venue, and signed-in account visible on every Back Office route. | Read active context; choose another authorized venue when more than one is available. No organization or venue creation/editing is introduced. | Initial loading, single-context, multi-context, switching, success, billing-refresh degradation, missing/stale saved venue, revoked membership, unauthorized venue, and empty authorized set fail closed or recover to the server default. | Switching always asks the operator to save unfinished changes and confirm the destination. The previous context remains active when confirmation or authorization fails. | Semantic headings/labels and a native select support keyboard and screen readers; `role=status` announces outcomes; focus has a visible outline; long names truncate with a title; controls stack at narrow widths. | Customer-session identity, organization membership, venue membership, manage-content capability, effective features, and venue ownership are resolved server-side on every request. Browser storage is continuity only and never grants access. |
| Tenant-bound screens | Prevent data from one venue remaining visible after a switch. | Remount menu, POS, screen, theme, schedule, and tap surfaces for the accepted venue. | Previous billing and route data are cleared/reloaded; unauthorized and removed contexts do not become active. | A confirmed switch is the only state transition; stale browser selection is removed before safe fallback. | Status feedback is textual and live-region announced rather than color-only. | All Back Office requests send the accepted venue selector and retain controller-level venue scope checks. |

## Server and Compatibility Contract

- `GET /api/back-office/session` returns account identity, active organization/venue names, capabilities, and the account's authorized manageable contexts.
- `X-Vennusign-Venue-Id` requests a context; authentication accepts it only after venue ownership, active organization/venue membership, and `ManageVenueContent` checks.
- `IBackOfficeContextRepository` returns only active organization memberships where organization owner/admin or venue manager/editor grants content management.
- Saved browser context is written only from a successful session response. A stale saved selection is cleared and retried through the server-selected onboarding venue.
- Legacy configured sessions expose one bound context and cannot switch tenants.
- Existing identity and membership tables support the query; no schema migration is required.

## Validation

- Back Office Node tests: 51 passed locally.
- Back Office production build: passed locally.
- Added focused repository coverage for active membership and content-management role filtering, API bootstrap assertions, and frontend coverage for identity separation, safe switching, stale recovery, announcements, keyboard behavior, and narrow layouts.
- Focused .NET tests are delegated to exact-head affected-area GitHub Actions because the local .NET SDK is unavailable.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests.

## Research References

- [WAI-ARIA select-only combobox example](https://www.w3.org/WAI/ARIA/apg/patterns/combobox/examples/combobox-select-only/)
- [WAI-ARIA combobox pattern](https://www.w3.org/WAI/ARIA/apg/patterns/combobox/)
- [WCAG 2.2 consistent identification](https://www.w3.org/WAI/WCAG22/Understanding/consistent-identification)
- [WCAG 2.2 error identification](https://www.w3.org/WAI/WCAG22/Understanding/error-identification)
- [WCAG 2.2 input assistance](https://www.w3.org/WAI/WCAG22/Understanding/input-assistance)
- [WCAG 2.2 status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages)

## Completion

- Issue: #419
- Mode: Sequential
- Branch: `rwp/05.06-back-office-context`
- Next approved package after merge and claim release: RWP-08.01 / issue #346
