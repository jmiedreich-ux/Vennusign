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

## Completed
- Added trusted HTTPS customer frontend-origin configuration and safe local-path callback returns.
- Switched local API and Venue Admin customer authentication to HTTPS.
- Added migration 051, Vite basic SSL, testable development-service endpoints, tests, and exact Google Console documentation.
- Trusted the local ASP.NET certificate and configured Development frontend origin as `https://localhost:5174` outside Git.
- Verified actual API/Venue Admin HTTPS startup.
- Verified Google challenge redirects to `accounts.google.com` with callback `https://localhost:7138/signin-customer-google`, authorization code flow, and PKCE using ephemeral fake process-only credentials.

## Validation
- API focused tests: 16/16 passed.
- Development Control tests: 8/8 passed.
- Venue Admin tests: 39/39 passed; production build passed.
- Migration inventory tests: 3/3 passed.

## Remaining Work
- Validate, review, and merge PR #396.
- Operator must enter Google credentials through Super Admin, enable Google, restart API, and complete live consent.

## Exact Next Action
- Open the Issue #395 PR and run affected checks.

## Do Not Redo or Reverse
- Do not accept arbitrary absolute OAuth return URLs.
- Do not revert customer authentication to mixed HTTP/HTTPS origins.
- Do not commit Google credentials or the unrelated local `UserSecretsId` change.
