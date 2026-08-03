# Phase 05 Admin CMS Core Editing Validation

## Capability Map

| Venue operator area | Delivered capability | Repeatable non-integration evidence |
| --- | --- | --- |
| Menu domain | Venue-scoped menus, ordered sections, items, translations, presentation states, and deterministic reads | repository/service unit tests; migration inventory |
| Menu editing | Section create/rename/order/activation plus inline item text, price, quantity, badges, and availability | menu service tests; Phase 05 frontend contracts |
| Tier-aware controls | Effective-feature capabilities, visible disabled previews, badges, and dismissible prompts | feature-resolution tests; frontend contracts |
| Quick Update | Daily special and one-scroll availability with venue-local midnight restoration | quick-update and reset-worker unit tests; frontend contracts |
| Screen management | Registration URLs, health, name/location editing, one-screen and venue-wide content push | screen-management/targeting tests; authorization matrix |
| Overflow | Fixed density capacities and deterministic visible/overflow item guidance | screen-targeting tests; frontend contracts |
| Video walls | Pro/Business-gated 2x1, 3x1, and 2x2 groups with stable positions | video-wall service tests; migration inventory; frontend contracts |

## Authorization

`SuperAdminAuthorizationMatrixTests` covers every Phase 05 menu, quick-update, screen, targeting, overflow, and video-wall route without an admin key. Each protected route must return HTTP 401 before controller execution.

## Frontend Validation

- `phase05-menu-sections.test.mjs` verifies each bounded Phase 05 feature contract.
- `phase05-critical-journeys.test.mjs` verifies the composed venue-board workflow across menus, Quick Update, screen management, overflow, targeting, video walls, and tier prompting.
- The admin and display production builds and frontend tests remain required in GitHub Actions.

## Data and Migration Validation

- Migration 012 creates the venue-scoped menu domain.
- Migration 013 adds Quick Update persistence.
- Migration 014 adds the `video_wall` feature and Pro/Business defaults.
- The pure migration-resource unit test requires the exact 001–014 embedded script inventory.

## Standing Validation Exception

Azure SQL execution, hosted services, credentials, containers, external services, and other integration-type tests are intentionally skipped under the standing repository-owner instruction. Their omission is not evidence that those external paths were exercised.

## Residual Risks

- Frontend journey coverage validates source-level composition and API wiring rather than a browser-rendered end-to-end suite.
- Azure SQL migration execution remains environment-dependent despite deterministic embedded-resource validation.
- Display layouts consume these Phase 05 contracts beginning in Phase 06; this closure package does not implement Phase 06 templates.

## Phase Result

Phase 05 delivers one responsive venue-board workspace for menu editing, daily operations, screen targeting, overflow guidance, and supported video-wall configuration. The approved Phase 06 breakdown is documented, and WP-06.01 is the next bounded package.

## Validation Evidence

- Local admin frontend contracts passed 17/17.
- GitHub Actions run 225 passed restore, Release build, both frontend production builds/tests, unit-category tests, migration-resource validation, and the explicit integration-test skip on functional head `054f0d4`.
- Final reconciled head `d38545b` preserved the concurrent roadmap update and passed the same required suite in GitHub Actions run 229.
- PR #85 merged as `1232135`.
- Integration-type tests were intentionally skipped.
