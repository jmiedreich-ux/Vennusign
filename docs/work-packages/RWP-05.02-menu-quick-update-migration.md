# RWP-05.02 — Menu and Quick Update Migration

## Status

In Review.

## Goal

Move the core day-to-day menu and service-time workflows into the dedicated venue-facing Admin CMS so venue operators can manage their board without using the internal Super Admin CRM.

## Scope

- Recompose the existing menu sections, menu items, and Quick Update workflows inside the venue-facing Admin CMS.
- Preserve current tier-aware visibility, locked previews, and soft-upgrade affordances for venue operators.
- Add venue-admin navigation and states for menu editing and quick-update tasks.
- Reduce the Super Admin surface to support/CRM responsibilities instead of primary day-to-day editing.
- Add focused non-integration frontend and API regression tests for migrated workflows.

## Acceptance Criteria

1. Venue operators can access menu section management, menu item editing, and Quick Update from the venue-facing Admin CMS.
2. Existing day-to-day editing behavior remains functionally equivalent after migration.
3. Tier-aware locked/unlocked behavior remains visible and deterministic in the venue-facing CMS.
4. The Super Admin CRM no longer serves as the primary venue-operator surface for these workflows.
5. Required non-integration checks pass; integration-type tests remain skipped under the standing exception.

## Boundaries

- No screen management, themes, scheduling, playlist, emergency-broadcast, date-range-promotion, or tap workflows in this package.
- No new menu domain behavior beyond what is needed for the migration.
- No mobile app work.

## Dependencies

- RWP-05.01 — Venue Admin CMS Foundation
- WP-05.03 — Inline Menu Item Editing and Sync
- WP-05.06 — Quick Update Mode

## Implementation Plan

### Source Workflows To Migrate

- `src/admin/src/MenuSectionsEditor.tsx`
- `src/admin/src/MenuItemsEditor.tsx`
- `src/admin/src/QuickUpdateMode.tsx`
- menu-related request helpers in `src/admin/src/api.ts`

### Backend Touchpoints

- Keep the existing menu domain and services authoritative.
- Add venue-admin-facing controller endpoints that reuse the current bounded services instead of sending venue operators through Super Admin endpoints.
- Planned controller touchpoints:
  - current source: `src/Vennu.Api/Controllers/Admin/SuperAdminMenusController.cs`
  - planned target: `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminMenusController.cs`
- Reuse existing request/response contracts where practical:
  - `src/Vennu.Api/Contracts/Admin/MenuSectionRequests.cs`
  - `src/Vennu.Api/Contracts/Admin/MenuEditorSnapshot` shapes already emitted by menu endpoints
- Reuse existing services rather than copying menu logic:
  - `src/Vennu.Data/Services/IMenuSectionManagementService.cs`
  - `src/Vennu.Api/Services/IMenuItemManagementService.cs`
  - `src/Vennu.Api/Services/IQuickUpdateService.cs`

### Frontend Touchpoints

Create venue-admin versions of the day-to-day menu workflows:

- `src/venue-admin/src/MenuSectionsEditor.tsx`
- `src/venue-admin/src/MenuItemsEditor.tsx`
- `src/venue-admin/src/QuickUpdateMode.tsx`
- `src/venue-admin/src/api.ts` menu request helpers
- `src/venue-admin/src/App.tsx` route wiring for menu and quick-update screens

Reduce the Super Admin dependency on these workflows by changing `src/admin/src/VenueDetail.tsx` to link to the venue-admin surface instead of acting as the primary operator UI.

### File Plan

Modify:

- `src/Vennu.Api/Program.cs` if the new venue-admin menu controller requires registration changes
- `src/admin/src/VenueDetail.tsx`
- `src/venue-admin/src/App.tsx`
- `src/venue-admin/src/api.ts`

Create:

- `src/Vennu.Api/Controllers/VenueAdmin/VenueAdminMenusController.cs`
- `src/venue-admin/src/MenuSectionsEditor.tsx`
- `src/venue-admin/src/MenuItemsEditor.tsx`
- `src/venue-admin/src/QuickUpdateMode.tsx`
- `tests/Vennu.Api.Tests/Controllers/VenueAdminMenusControllerTests.cs`
- `src/venue-admin/tests/menu-editor.test.mjs`
- `src/venue-admin/tests/quick-update.test.mjs`

### Migration Sequence

1. Stand up venue-admin menu bootstrap and route shell on top of WP-05.11.
2. Move section list/edit/reorder flows.
3. Move item create/edit/presentation flows.
4. Move Quick Update daily-special and quick-availability flows.
5. Replace Super Admin editing affordances with bounded links into the venue-admin surface.

### Non-Integration Test Plan

- API/controller tests for protected menu snapshot access, create/update/reorder section behavior, item create/update behavior, and Quick Update endpoints.
- Frontend tests for loading/error states, section create/edit, item save, quick availability toggles, daily-special updates, and locked-tier affordances.
- Regression checks that migrated UI still emits the same request shapes expected by the existing services.

### Explicitly Deferred From This Package

- Screen-management and video-wall workflows.
- Themes, schedules, playlists, emergency broadcasts, promotions, and tap lists.
- Any menu-domain redesign beyond what the migration requires.

## GitHub

- Issue: #255
- Branch: `rwp/05.02-menu-quick-update-migration`
- Pull request: pending

## Validation Evidence

- Local Venue Admin tests: 7 passed; production build passed.
- Local Super Admin tests: 73 passed; production build passed.
- Local .NET validation: not run because the SDK is unavailable in this workspace.
- GitHub Actions: pending and authoritative.
- Skipped: Azure SQL, external-service, credentialed, hosted-infrastructure, container, and all other integration-type tests under the standing owner exception.
