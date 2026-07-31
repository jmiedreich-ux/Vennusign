# RWP-05.01 — Venue Admin CMS Foundation

## Status

In Review.

## Goal

Restore the planned architecture by introducing a separate venue-facing Admin CMS application instead of continuing to place day-to-day venue workflows inside the internal Super Admin SPA.

## Scope

- Create an independent venue-facing React/Vite SPA under a dedicated source directory.
- Add a protected venue-scoped bootstrap contract and session initialization flow distinct from the Super Admin bootstrap.
- Establish the initial venue-admin shell, navigation, configuration loading, and protected loading/error states.
- Preserve the existing tier-aware UI patterns for visible locked and unlocked capabilities.
- Extend non-integration validation and CI so the new venue-admin SPA builds independently alongside `src/admin` and `src/display`.

## Acceptance Criteria

1. A separate venue-facing SPA exists and builds independently from the Super Admin SPA.
2. Venue-admin bootstrap does not depend on the Super Admin browser workflow or expose the Super Admin secret as the venue login mechanism.
3. The venue-admin shell provides protected bootstrap, route framing, and deterministic unauthorized/error states.
4. Existing tier-aware locked-state patterns remain available for future venue-admin feature migration.
5. Required non-integration API/frontend/build checks pass; integration-type tests remain skipped under the standing exception.

## Boundaries

- No menu-editing migration in this package.
- No screen, theme, scheduling, tap, or pairing workflow migration in this package.
- No mobile app work.
- No billing, checkout, or POS integration changes.

## Dependencies

- RWP-04.01 — Super Admin Venue Provisioning
- WP-05.05 — Tier-Aware Venue Admin Patterns
- WP-05.10 — Phase 05 Validation and Closure

## Implementation Plan

### App Boundary Decision

- Create a new SPA under `src/venue-admin`.
- Keep `src/admin` as the internal Super Admin CRM.
- Keep `src/display` as the player SPA.
- Do not continue adding venue-operator workflows to `src/admin` once the new shell exists.

### Backend Touchpoints

- Add a dedicated venue-admin bootstrap surface in `src/Vennu.Api` rather than reusing Super Admin session endpoints.
- Planned files:
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminSessionController.cs`
  - `src/Vennu.Api/Contracts/VenueAdmin/VenueAdminSessionResponse.cs`
  - `src/Vennu.Api/VenueAdmin/VenueAdminAuthenticationDefaults.cs`
  - `src/Vennu.Api/VenueAdmin/VenueAdminAuthenticationOptions.cs`
  - `src/Vennu.Api/VenueAdmin/VenueAdminAuthenticationHandler.cs`
- Update `src/Vennu.Api/Program.cs` to register the venue-admin authentication/bootstrap path and local-dev CORS for the new SPA.
- Keep this package bounded to bootstrap/session establishment; operational controllers are deferred to WP-05.12 and WP-05.13.

### Frontend Touchpoints

Create the new venue-admin shell and bootstrap files:

- `src/venue-admin/package.json`
- `src/venue-admin/index.html`
- `src/venue-admin/vite.config.ts`
- `src/venue-admin/tsconfig.json`
- `src/venue-admin/tsconfig.app.json`
- `src/venue-admin/tsconfig.node.json`
- `src/venue-admin/src/main.tsx`
- `src/venue-admin/src/App.tsx`
- `src/venue-admin/src/config.ts`
- `src/venue-admin/src/api.ts`
- `src/venue-admin/src/styles.css`

Add only the foundational route frame in this package:

- dashboard/home placeholder
- menu placeholder
- screens placeholder
- settings/support placeholder

### Bootstrap Expectations

- Venue-admin bootstrap must be distinct from `src/admin/src/App.tsx` and `loadSession()`.
- The venue-admin client must call its own session/bootstrap endpoint and receive venue-scoped identity and entitlement context.
- The implementation must not reuse `X-Vennu-Admin-Key` as the venue-operator login mechanism.

### File Plan

Modify:

- `src/Vennu.Api/Program.cs`
- `src/Vennu.Api/appsettings.json` only if a new bounded configuration section is required
- repository validation scripts or workflow definitions only if needed for the new SPA build

Create:

- the new `src/venue-admin` SPA files listed above
- `tests/Vennu.Api.Tests/Controllers/VenueAdminSessionControllerTests.cs`
- `src/venue-admin/tests/bootstrap.test.mjs`
- `src/venue-admin/tests/navigation-shell.test.mjs`

### Non-Integration Test Plan

- API tests for unauthorized venue-admin bootstrap, successful session bootstrap, and deterministic response shape.
- Authentication-handler tests for missing/invalid credentials and valid venue-scoped identity creation.
- Frontend tests for configuration loading, protected bootstrap states, unauthorized messaging, and route-shell rendering.
- Build validation for `src/venue-admin` alongside existing `src/admin` and `src/display` builds.

### Explicitly Deferred From This Package

- Menu editing and Quick Update migration.
- Screen, theme, scheduling, playlist, tap, or pairing workflows.
- Mobile authentication reuse and staff mobile app behavior.
- Any self-service billing or signup path.

## GitHub

- Issue: #254
- Branch: `rwp/05.01-venue-admin-cms-foundation`
- Pull request: pending

## Validation Evidence

- Local Venue Admin tests: 3 passed.
- Local Venue Admin production build: passed.
- Local .NET validation: not run because the SDK is unavailable in this workspace.
- GitHub Actions: pending and authoritative.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, and all other integration-type tests under the standing owner exception.
