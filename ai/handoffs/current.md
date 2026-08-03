# Vennu Session Handoff

## Work Package
- ID: Issue-395
- Status: Complete through PR #396
- Execution mode: Collaborative

## Git State
- Branch: `master`
- Issue: #395
- Pull request: #396
- CI state: all 12 required checks passed on reviewed head `07ea82d`; PR #396 merged

## Completed This Session
- Added a registered trusted customer frontend origin and safe configured-origin plus local-path callback return.
- Rejected external, scheme-relative, overlong, HTTP, path-bearing, and user-info redirect inputs.
- Switched Development Control API/Venue Admin customer-authentication paths to HTTPS.
- Added Vite basic SSL, testable service catalog coverage, migration 051, binding/security tests, and operator documentation.
- Trusted the ASP.NET development certificate and configured the local Development frontend origin without committing values.
- Verified actual HTTPS API/Venue Admin startup and a Google code+PKCE challenge with the exact localhost callback.

## Validation
- API focused tests: 16/16 passed.
- Development Control tests: 8/8 passed.
- Venue Admin tests: 39/39 passed; production build passed.
- Migration inventory tests: 3/3 passed.
- Actual HTTPS challenge returned Google host, exact callback, code response type, and PKCE.
- GitHub Actions pending.

## Remaining Work
- Operator must enter the Google web client ID and secret through Super Admin, enable Google, restart API, and complete live consent.

## Known Risks or Blockers
- Live Google consent requires operator-owned credentials and is intentionally not run in CI.
- Vite's local certificate requires one browser acceptance before callback navigation.

## Exact Next Action
- Configure Google OAuth origin/callback and credentials as documented in `docs/architecture/google-customer-signup.md`, restart API, and complete live consent.

## Do Not Redo or Reverse
- Do not accept arbitrary absolute OAuth return URLs.
- Do not revert API/Venue Admin customer authentication to mixed HTTP/HTTPS origins.
- Do not commit Google credentials or the unrelated local `UserSecretsId` change.
