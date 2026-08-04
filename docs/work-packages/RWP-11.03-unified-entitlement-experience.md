# RWP-11.03 — Unified Entitlement Experience

## Outcome

Back Office now uses one entitlement lock chip and one upgrade sheet across locked navigation, section previews, inline hints, and the sidebar upgrade prompt. Every entry point names the blocked feature and required tier consistently while preserving server-authoritative entitlements and Stripe-hosted checkout.

## Accepted Scope

- Introduce one semantic lock chip that combines the non-color lock cue, feature name, and shared tier badge.
- Reuse that chip in locked navigation, locked section previews, inline feature hints, and sidebar upgrade opportunities.
- Replace the former upgrade modal with one responsive upgrade sheet that owns tier value, billing interval, price, error, pending, and hosted-checkout launch states.
- Remove competing upgrade buttons from the surrounding surfaces while preserving their dismiss or defer actions.
- Preserve the established opportunity ordering, dismissal storage, effective-feature checks, hosted Checkout allowlist, and webhook-authoritative entitlement refresh.

## UI and Function Gap Analysis

| Area | Required behavior | Implemented result |
| --- | --- | --- |
| Goals | Operators need one recognizable explanation and path forward whenever a capability is locked. | Every supported context renders the same lock chip and opens the same upgrade sheet with the blocked feature and required tier. |
| Navigation and hierarchy | A locked destination must remain discoverable without creating a second upgrade hierarchy beside the main task. | Locked navigation becomes the shared chip; previews, hints, and nudges use that same primary upgrade entry while keeping only their contextual dismiss action. |
| Required actions | Operators must be able to review tier value, choose monthly or annual billing, defer, close, and continue to secure checkout. | The upgrade sheet centralizes those actions and retains the existing hosted Checkout request and bounded return flow. |
| Essential states | Locked, selected opportunity, monthly/annual, price, pending, error, dismissed, and authoritative post-checkout states must remain explicit. | The chip exposes the locked feature/tier; the sheet owns interval, price, pending and inline error states; existing dismissal and webhook-authoritative refresh behavior is unchanged. |
| Validation and feedback | The browser must not grant access optimistically or accept an unapproved checkout destination. | Existing effective-feature checks, server responses, checkout-origin allowlist, pending feedback, and post-return refresh remain authoritative. |
| Destructive actions | Upgrade entry points must not imply an irreversible local change. | No destructive action is introduced; closing or deferring is always available, and Stripe performs the final commercial review before confirmation. |
| Accessibility | Lock meaning cannot depend on color; dialog focus, name, description, keyboard exit, busy state, and return focus must be reliable. | The chip has a decorative lock plus feature/tier accessible label. The sheet is a named modal dialog, focuses its close action, restores prior focus, supports Escape and backdrop close when idle, and blocks dismissal during submission. |
| Responsiveness | The same experience must work in navigation, content cards, sidebar, desktop overlays, and narrow mobile viewports. | Compact and contextual chip variants share wrapping/focus behavior; the sheet uses a centered desktop panel and a full-width mobile bottom-sheet treatment. |
| API, data, authorization, and entitlements | The consolidation must not add endpoints, trust browser state, weaken tenant isolation, or change tier authority. | No API, schema, persistence, authorization, or entitlement contract changes are included. Existing venue-scoped bootstrap data, `effectiveFeatures`, opportunity definitions, Checkout, and webhook reconciliation are reused. |

## Validation

- Back Office Node tests: 102 passed.
- Back Office production build: passed.
- Git whitespace validation: passed.
- Exact-head affected-area GitHub Actions remains authoritative before merge.

## Skipped Integration Testing

Azure SQL, live Stripe, hosted-browser end-to-end, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Completion

This package, `PROJECT_STATUS.md`, `tracker/assignments.json`, and `ai/handoffs/current.md` describe the proposed merge state. Completion still requires exact-head Actions, review, merge, issue closure, default-branch verification, and claim release.
