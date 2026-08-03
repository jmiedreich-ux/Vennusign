# Phase 04 Super Admin CRM Validation

## Capability Map

| Operator area | Delivered capability | Repeatable non-integration evidence |
| --- | --- | --- |
| Access | Protected internal workspace and session capability discovery | `SuperAdminSessionControllerTests`; authorization matrix |
| Venue support | Search/filter directory, venue context, screens, effective features, overrides, and tier switching | Venue directory/detail/override/tier-switch service tests; admin journey contract |
| Tier management | Create, edit, clone, archive, and Stripe catalog mapping | `TierManagementServiceTests`; admin journey contract |
| Feature management | Tier feature matrix with audited mutations and effective-feature cache invalidation | `FeatureMatrixServiceTests`; admin journey contract |
| Operations | Venue/subscription counts, screen health, and recent commercial events | Dashboard and event-feed service tests; admin journey contract |
| Revenue | Live USD MRR/ARR, tier allocation, daily persistence, and month-over-month trend | Revenue snapshot/trend service tests; admin journey contract |

## Authorization Matrix

`SuperAdminAuthorizationMatrixTests` exercises every Phase 04 Super Admin route without an admin key, including read and mutation endpoints. Every route must return HTTP 401 before controller execution.

## Frontend Validation

- `src/admin/tests/phase04-critical-journeys.test.mjs` verifies the secure shell and the critical operator journey wiring for venue support, tiers, features, health, revenue, trends, and events.
- The admin production build remains required.
- The display heartbeat tests now wait for the async request lifecycle deterministically; all display tests and the production build are required in CI.

## Residual Risks

- Azure SQL migration execution and Stripe provider calls require external infrastructure and credentials. Those integration-type tests are intentionally skipped under the standing repository-owner instruction.
- The internal Super Admin workspace uses the established protected API-key model; per-user identity and role-based access are not part of Phase 04.
- Admin UI journey tests validate source-level routing and API wiring rather than running a browser-rendered end-to-end suite.

## Phase Result

Phase 04 delivers a coherent internal operations workspace for venue support, tier and feature administration, fleet health, commercial activity, live revenue, tier switching, and revenue trends. Phase 05 starts with the persistent menu domain required by the approved Admin CMS roadmap.
