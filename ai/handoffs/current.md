# Vennu Session Handoff

## Work Package
- ID: Issue-395
- Status: In Review
- Execution mode: Collaborative

## Git State
- Branch: `issue/395-google-signup-local-https`
- Issue: #395
- Pull request: #396
- CI state: pending

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
- Validate, review, and merge PR #396.
- Operator must enter the Google web client ID and secret through Super Admin, enable Google, restart API, and complete live consent.

## Known Risks or Blockers
- Live Google consent requires operator-owned credentials and is intentionally not run in CI.
- Vite's local certificate requires one browser acceptance before callback navigation.

## Exact Next Action
- Open the Issue #395 PR and run affected API, Venue Admin, data, and Windows tooling checks.

## Do Not Redo or Reverse
- Do not accept arbitrary absolute OAuth return URLs.
- Do not revert API/Venue Admin customer authentication to mixed HTTP/HTTPS origins.
- Do not commit Google credentials or the unrelated local `UserSecretsId` change.
