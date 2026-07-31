# RWP-04.01 — Super Admin Venue Provisioning

## Status

In Review.

## Goal

Allow authorized Super Admin operators to create and provision a new venue from the protected venue workflow without leaving the internal CRM.

## Scope

- Add a protected venue-creation flow to the Super Admin Venues experience.
- Capture the bounded venue profile fields already supported by the core venue API: name, timezone, type, primary language, and optional secondary language.
- Establish a deterministic initial commercial state for new venues using the existing tier and subscription domain behavior.
- Return the operator to the venue directory or the newly created venue detail with clear success and validation states.
- Add focused non-integration API and frontend tests.

## Acceptance Criteria

1. Authorized Super Admin users can create a venue from the Venues route without calling raw APIs manually.
2. Required venue fields are validated consistently between the API and the Super Admin UI.
3. New venues appear deterministically in the venue directory and can be opened immediately in venue detail.
4. The initial tier/subscription state is explicit and consistent with the existing billing domain behavior.
5. Protected API and Super Admin UI checks pass; integration-type tests remain skipped under the standing exception.

## Boundaries

- No public self-service signup.
- No card capture, checkout, or Stripe-hosted subscription purchase flow.
- No venue/customer authentication model changes.
- No migration of day-to-day venue editing into a separate CMS surface.

## Dependencies

- WP-04.02 — Venue Directory
- WP-04.03 — Venue Detail & Support View
- WP-04.10 — Venue Tier Switching

## Implementation Plan

### Backend Touchpoints

- Extend `src/Vennu.Api/Controllers/Admin/SuperAdminVenuesController.cs` with `POST /api/admin/venues` so venue creation stays inside the protected Super Admin surface.
- Reuse `src/Vennu.Api/Contracts/Venues/CreateVenueRequest.cs` and `src/Vennu.Api/Contracts/Venues/CreateVenueResponse.cs` unless a bounded admin-only response shape becomes necessary.
- Add a bounded orchestration service so the controller does not coordinate repository and subscription setup directly:
  - `src/Vennu.Data/Services/IVenueProvisioningService.cs`
  - `src/Vennu.Data/Services/VenueProvisioningService.cs`
- Register the provisioning service in `src/Vennu.Data/Extensions/ServiceCollectionExtensions.cs`.
- Use existing persistence and commercial services rather than introducing new tables:
  - `src/Vennu.Data/Repositories/IVenueRepository.cs`
  - `src/Vennu.Data/Repositories/ISubscriptionTierRepository.cs`
  - `src/Vennu.Data/Services/ISubscriptionManagementService.cs`
- Resolve the initial commercial state by looking up the seeded Starter tier slug and starting the existing trial flow instead of inventing a new subscription path.

### Frontend Touchpoints

- Add a create-venue action to `src/admin/src/VenueDirectory.tsx`.
- Add a create-venue request helper to `src/admin/src/api.ts`.
- Update `src/admin/src/App.tsx` only if needed to auto-open the created venue detail after success.
- Keep the first delivery bounded to the existing Venues route; do not add a new top-level route.

### File Plan

Modify:

- `src/Vennu.Api/Controllers/Admin/SuperAdminVenuesController.cs`
- `src/Vennu.Data/Extensions/ServiceCollectionExtensions.cs`
- `src/admin/src/VenueDirectory.tsx`
- `src/admin/src/api.ts`
- `src/admin/src/App.tsx` if post-create navigation requires state changes

Create:

- `src/Vennu.Data/Services/IVenueProvisioningService.cs`
- `src/Vennu.Data/Services/VenueProvisioningService.cs`
- `tests/Vennu.Api.Tests/Controllers/SuperAdminVenuesControllerTests.cs`
- `tests/Vennu.Api.Tests/Services/VenueProvisioningServiceTests.cs`
- `src/admin/tests/venue-provisioning.test.mjs`

### UI Flow

1. Operator opens `Venues` in `src/admin`.
2. Operator chooses `Create venue` from the directory toolbar.
3. Form captures name, timezone, type, primary language, and optional secondary language.
4. Successful creation posts to the protected admin venue endpoint.
5. The directory refreshes and the new venue can be opened immediately in venue detail.

### Non-Integration Test Plan

- API/controller tests for unauthorized access, validation failures, successful venue creation, and deterministic `201` response shape.
- Service tests for Starter-tier lookup, trial initialization, duplicate-subscription protection, and error propagation.
- Frontend tests for form validation, submit success, error state, and directory refresh/open behavior.

### Explicitly Deferred From This Package

- Self-service customer signup.
- Email invitations, password reset, or venue-user identity management.
- Payment checkout and Stripe-hosted provisioning.
- Any migration of day-to-day venue tools into a separate venue-admin SPA.

## GitHub

- Issue: #253
- Branch: `rwp/04.01-super-admin-venue-provisioning`
- Pull request: pending

## Validation Evidence

- Local Super Admin tests: 73 passed.
- Local Super Admin production build: passed.
- Local .NET validation: not run because the SDK is unavailable in this workspace.
- GitHub Actions: pending and authoritative.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, and all other integration-type tests under the standing owner exception.
