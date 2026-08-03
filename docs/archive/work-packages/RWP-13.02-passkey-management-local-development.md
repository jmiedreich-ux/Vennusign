# RWP-13.02 — Passkey Enrollment, Management, and Local Development

## UI and function gap analysis

| Concern | Completed behavior |
| --- | --- |
| Goal and navigation | Account & Security is a first-class Back Office destination; legacy venue links explain that customer sign-in is required. |
| List/add/rename/remove | Safe metadata is listed; recently authenticated customers can name and add a credential, rename it inline, or deliberately remove it after confirmation. |
| Essential states | Loading, empty, unsupported, waiting, success, cancellation/timeout, missing credential, expired/failed verification, stale recent-auth, removal conflict, retry, and recovery guidance are explicit. |
| Validation | Passkey names are required and bounded in browser and server; WebAuthn challenges remain user-bound, protected, five-minute, and single-use. |
| Destructive/lockout safety | Removal confirms the named credential, requires recent authentication, soft-revokes the user-owned row, and blocks loss of the last passkey without verified email recovery. |
| Accessibility/responsiveness | Labeled controls, native forms/confirmation, visible focus, status/alert announcements, device-independent copy, and stacked narrow-screen actions follow [W3C status-message guidance](https://www.w3.org/WAI/WCAG22/Understanding/status-messages) and [accessible authentication guidance](https://www.w3.org/WAI/WCAG22/Understanding/accessible-authentication-minimum.html). |
| Browser capability | WebAuthn remains top-level and secure-context only; browser errors map to actionable, non-sensitive alternatives consistent with [MDN PublicKeyCredential guidance](https://developer.mozilla.org/en-US/docs/Web/API/PublicKeyCredential). |
| API/data/authentication | Management returns no public key/handle/counter; customer-session authorization and recent-auth enforcement are server-side; email, OIDC, TOTP, and recovery-code boundaries remain separate. |
| Local development | Development explicitly uses HTTPS localhost frontend/API and localhost RP/origin; production exact-domain configuration fails closed. No credentials or relaxed defaults are committed. |

## Validation

- Back Office tests (61/61) and production build pass locally.
- Focused API configuration, repository ownership/soft-delete, route, browser-error, and UI contract tests are included for exact-head Actions.
- Azure SQL, live browser/authenticator, external identity/email, hosted infrastructure, credentials, containers, physical devices, signing/store, cross-system, and all other integration-type tests are skipped.

Completion evidence remains in the implementation PR. Issue: #420. The approved product queue returns to paused; Phase 14+ remains unauthorized.
