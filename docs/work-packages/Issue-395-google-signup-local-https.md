# Issue-395 — Google Signup Local HTTPS

## Status

In Review

## Execution Mode

Collaborative

## UX Guidance and Flow Gap Analysis

Social Login and Sign Up Flow guidance applies. Existing Google action labels remain familiar and no additional form is introduced.

- Goal: start Google signup, consent with Google, establish a Vennu customer session, and return to onboarding.
- Navigation: `/signup` or `/signin` ? HTTPS API challenge ? Google ? fixed HTTPS API callback ? configured trusted Venue Admin origin plus validated local path.
- Feedback: disabled providers continue returning a non-redirecting 503; provider callback failures remain provider/authentication errors; successful callbacks return to the existing onboarding state handling.
- Safety: arbitrary external return URLs remain rejected; credentials remain write-only configuration; correlation, nonce, and session cookies stay secure.
- Accessibility/responsiveness: no visual component changes; existing labeled provider buttons and error states remain in use.
- Data/authorization: Google must return a verified email; external identity resolution, session persistence, and onboarding authorization remain unchanged.
- Exclusions: live Google credentials and consent are operator-provided and are not committed or exercised in CI.

## Scope

- Trusted customer frontend origin configuration and redirect construction.
- Local HTTPS API and Venue Admin development hosting.
- Exact Google Console callback/origin setup documentation.
- Focused return-path, options, tooling, and frontend validation.

## Validation

- Customer authentication/provider-binding tests: 16/16 passed.
- Vennu Development Control tests: 8/8 passed.
- Venue Admin production build passed; tests: 39/39 passed.
- Migration inventory tests: 3/3 passed.
- ASP.NET HTTPS development certificate is trusted.
- Actual Venue Admin HTTPS startup returned HTTP 200.
- Actual API HTTPS startup and migration 051 passed.
- Challenge validation with ephemeral non-secret test credentials returned HTTP 302 to `accounts.google.com`, callback `https://localhost:7138/signin-customer-google`, authorization code response type, and PKCE.
- Development `CustomerAuthentication:FrontendOrigin` is configured locally as `https://localhost:5174`; no credential was committed.
- GitHub Actions pending.
