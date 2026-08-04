# RWP-04.03 — Platform Operations Mobile and Console Polish

## Outcome

Platform Operations remains usable on mobile, explains access failures safely, keeps revenue values readable without hover, and preserves table context during long support workflows.

## Required implementation

- Expose the existing sign-out behavior in the mobile header when the desktop identity control is hidden.
- Distinguish rejected or expired keys, permission failures, and service unavailability without exposing key values or internal details.
- Show formatted MRR values, month, percent change, and active-subscription context directly in the trend chart and its accessible name.
- Keep table headings visible while long support tables scroll; preserve the Feature Matrix frozen first column.
- Add focused source-contract tests and run the affected Platform Operations test/build suite.

## UI and function gap analysis

- **Goal and hierarchy:** mobile operators can end a privileged session from the page header. Access failures lead with recovery. Revenue values and months remain the visual chart hierarchy, while percent change stays supporting context.
- **Navigation and required actions:** all routes remain unchanged. Desktop and mobile sign-out call the same session-clearing function. Retry is performed by entering a current protected key and reopening the workspace.
- **Essential states:** valid/loading session behavior is preserved. Invalid or expired, permission denied, API unavailable, and aborted requests are distinguished. Trend values remain visible for every returned month; existing loading and error states remain unchanged.
- **Validation:** access errors are derived from the existing HTTP status contract. The access input is marked invalid and references the alert guidance. Focused guards require the mobile action, visible trend values, accessible chart list, and sticky table headings.
- **Destructive actions:** sign-out clears only session-scoped key state and is immediately recoverable by signing in again. No persistent or destructive system operation is introduced.
- **Feedback:** the access form presents an inline alert beside the affected key field. Trend data is no longer hidden in a title tooltip. RWP-00.09 retains ownership of general transient success feedback.
- **Accessibility:** the access error uses `role="alert"`, `aria-invalid`, and `aria-describedby`. The chart exposes a labeled list with a complete per-month accessible name, while decorative bars are hidden. Sticky headings retain semantic table markup.
- **Responsiveness:** mobile sign-out appears at the existing 760px shell breakpoint. Chart points use a wider minimum width and horizontal scrolling. Tables constrain vertical height while retaining two-axis scrolling.
- **API, data, authorization, and entitlement support:** no endpoint, payload, persistence, authorization, tenant, billing, or entitlement change is needed. Existing session and revenue responses remain authoritative.

## Acceptance evidence

- The mobile header renders `Sign out` and invokes the same protected-session cleanup used by the desktop identity control.
- HTTP 401 and 403 access failures provide separate recovery messages; generic outages remain distinct.
- Every trend point visibly renders formatted MRR and exposes month, MRR, subscription count, and change to assistive technology.
- Generic support-table headings and the Feature Matrix corner heading remain sticky during scrolling.
- Focused Platform Operations tests pass and the production build succeeds.

## Validation

- Platform Operations Node tests: 93 passed locally.
- Platform Operations production build: passed locally.
- Patch whitespace validation: passed.
- Exact-head affected-area GitHub Actions is authoritative before merge.

## Skipped integration testing

Hosted browser/mobile rendering, live Stripe, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Queue and boundaries

- Issue: #453.
- Branch: `rwp/04.03-platform-operations-polish`.
- RWP-00.09 / #454 becomes next only after this PR merges, issue #453 closes, `master` is verified, and the claim is released.
- RWP-13.06 / #466 remains held; Phase 14+ remains paused.
