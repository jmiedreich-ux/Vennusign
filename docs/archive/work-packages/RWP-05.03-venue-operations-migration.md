# RWP-05.03 — Remaining Venue Features Migration

## Status

Complete.

## Goal

Complete the venue-facing Admin CMS by moving the remaining venue-scoped operational workflows out of Super Admin and back into the customer-facing day-to-day management surface.

## Scope

- Move screen-management, theme, meal-period, happy-hour, playlist, emergency-broadcast, date-range-promotion, and tap-list administration into the venue-facing Admin CMS.
- Preserve feature-gated visibility using the existing entitlement and override model.
- Leave Super Admin focused on support context, tiering, overrides, and internal operational CRM duties.
- Add bounded navigation or deep-link handoff between Super Admin support context and venue-admin operational editing where needed.
- Add focused non-integration frontend and API regression coverage for migrated workflows.

## Acceptance Criteria

1. Venue-scoped operational workflows are available in the venue-facing Admin CMS rather than only inside the Super Admin SPA.
2. Existing feature gating remains deterministic and consistent with the current resolution model.
3. Super Admin retains support visibility without remaining the primary editing surface for customer day-to-day operations.
4. Deep links or equivalent bounded navigation allow internal operators to move from support context to the venue-admin surface when appropriate.
5. Required non-integration checks pass; integration-type tests remain skipped under the standing exception.

## Boundaries

- No new display layouts, POS integrations, mobile-app work, or TV packaging work.
- No replacement of the current feature-resolution, billing, or scheduling domain behavior outside what the migration requires.
- No customer self-service billing workflow.

## Dependencies

- RWP-05.01 — Venue Admin CMS Foundation
- RWP-05.02 — Menu and Quick Update Migration
- WP-07.09 — Hero Rotation and Administration
- WP-08.09 — Date-Range Promotions
- WP-09.09 — Pairing Code Registration Completion

## Implementation Plan

### Source Workflows To Migrate

- `src/admin/src/ScreenManagement.tsx`
- `src/admin/src/ThemeBuilder.tsx`
- `src/admin/src/MealPeriodAdministration.tsx`
- `src/admin/src/HappyHourAdministration.tsx`
- `src/admin/src/PlaylistAdministration.tsx`
- `src/admin/src/EmergencyBroadcastAdministration.tsx`
- `src/admin/src/DateRangePromotionAdministration.tsx`
- `src/admin/src/TapListAdministration.tsx`
- venue-detail composition in `src/admin/src/VenueDetail.tsx`

### Backend Touchpoints

- Add venue-admin-facing controller surfaces that reuse the existing operational services and repositories rather than duplicating logic.
- Planned controller files:
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminScreensController.cs`
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminThemesController.cs`
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminMealPeriodsController.cs`
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminHappyHourController.cs`
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminPlaylistsController.cs`
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminEmergencyBroadcastsController.cs`
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminDateRangePromotionsController.cs`
  - `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminTapListController.cs`
- Current Super Admin source endpoints to mirror are:
  - `src/Vennu.Api/Controllers/Admin/SuperAdminScreensController.cs`
  - `src/Vennu.Api/Controllers/Admin/SuperAdminThemesController.cs`
  - `src/Vennu.Api/Controllers/Admin/SuperAdminMealPeriodsController.cs`
  - `src/Vennu.Api/Controllers/Admin/SuperAdminHappyHourController.cs`
  - `src/Vennu.Api/Controllers/Admin/SuperAdminPlaylistsController.cs`
  - `src/Vennu.Api/Controllers/Admin/SuperAdminEmergencyBroadcastsController.cs`
  - `src/Vennu.Api/Controllers/Admin/SuperAdminDateRangePromotionsController.cs`
  - `src/Vennu.Api/Controllers/Admin/SuperAdminTapListController.cs`

### Frontend Touchpoints

Create venue-admin operational counterparts under `src/venue-admin/src/`:

- `ScreenManagement.tsx`
- `ThemeBuilder.tsx`
- `MealPeriodAdministration.tsx`
- `HappyHourAdministration.tsx`
- `PlaylistAdministration.tsx`
- `EmergencyBroadcastAdministration.tsx`
- `DateRangePromotionAdministration.tsx`
- `TapListAdministration.tsx`
- route wiring in `App.tsx`
- request helpers in `api.ts`

Refocus `src/admin/src/VenueDetail.tsx` on support context and bounded links rather than full day-to-day editing.

### File Plan

Modify:

- `src/admin/src/VenueDetail.tsx`
- `src/venue-admin/src/App.tsx`
- `src/venue-admin/src/api.ts`
- `src/Vennu.Api/Program.cs` only if new venue-admin controller/auth wiring requires it

Create:

- the venue-admin controller files listed above
- the venue-admin component files listed above
- `tests/Vennu.Api.Tests/Controllers/VenueAdminOperationalControllerTests.cs` or equivalent bounded controller suites by area
- `src/venue-admin/tests/screen-management.test.mjs`
- `src/venue-admin/tests/theme-builder.test.mjs`
- `src/venue-admin/tests/scheduling-suite.test.mjs`
- `src/venue-admin/tests/tap-list-suite.test.mjs`

### Migration Sequence

1. Move screen-management and theme flows first because they anchor the operator shell.
2. Move scheduling flows next: meal periods, happy hour, playlists, emergency broadcast, date-range promotions.
3. Move tap-list administration last because it depends on later-phase venue capabilities.
4. Replace embedded operational editing in Super Admin with deep links or open-in-venue-admin actions.

### Non-Integration Test Plan

- API/controller tests for protected venue-admin access to each operational area and bounded success/not-found/validation behavior.
- Frontend tests for feature-gated visibility, loading/error states, save flows, and deep-link navigation from support context.
- Regression tests that the Super Admin venue detail still shows support context correctly after editing tools are removed.

### Explicitly Deferred From This Package

- New display layouts.
- POS synchronization changes.
- Native TV app packaging.
- Mobile-app operator flows.
- Any billing self-service beyond existing entitlement visibility.

## GitHub

- Issue: #256
- Branch: `rwp/05.03-venue-operations-migration`
- Pull request: #263

## Validation Evidence

- Local Venue Admin tests: 12 passed; production build passed.
- Local Super Admin tests: 73 passed; production build passed.
- Local .NET validation: not run because the SDK is unavailable in this workspace.
- GitHub Actions run #562: passed against reviewed head `a225dc078a31a920cf4ee7ebfda6b6064fe0ecf3`.
- Merge commit: `16a477cf80e9f90711706dde364d4f67de6dde26`.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, and all other integration-type tests under the standing owner exception.
