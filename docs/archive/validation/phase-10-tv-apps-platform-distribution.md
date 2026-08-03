# Phase 10 — TV Apps & Platform Distribution Validation

## Result

Phase 10 is ready for closure when the WP-10.10 GitHub Actions run passes. Integration-type and external store/device tests remain intentionally skipped under the standing repository-owner instruction.

## Acceptance Matrix

| Journey | Evidence |
| --- | --- |
| Shared platform launch contract and authoritative React player parity | `platformLaunch.test.mjs`, display Phase 10 critical journeys |
| No-keyboard pairing, durable launch state, and automatic recovery | `pairing.test.mjs`, `androidTvShell.test.mjs`, display Phase 10 critical journeys |
| Android/Fire lifecycle, boot, kiosk, operator escape, and distribution variants | `androidTvShell.test.mjs`, `androidDistribution.test.mjs`, unsigned Android profile builds in GitHub Actions |
| Samsung Tizen hosted-player package, metadata, remote exit, and static safety | `tizenPackage.test.mjs`, Tizen validation script, display Phase 10 critical journeys |
| LG webOS hosted-player package, lifecycle, remote exit, and static safety | `webosPackage.test.mjs`, webOS validation script, display Phase 10 critical journeys |
| HaaS pre-registration, token hashing, expiry, one-time claim, and bridge-only delivery | `HaasPreRegistrationServiceTests`, `provisioning.test.mjs`, admin/display Phase 10 critical journeys |
| Platform/app-version heartbeats and current/outdated/unknown fleet health | `displayHeartbeat.test.mjs`, `OperationalDashboardServiceTests`, admin Phase 10 critical journeys |
| Migration inventory and pre-registration storage constraints | `MigrationResourceTests`, unit-category `DatabaseMigratorTests`, admin Phase 10 critical journeys |
| Signing, credentials, proprietary SDKs, generated packages, and player forks remain excluded | platform package tests, display Phase 10 critical journeys, reviewed PR diff |

## Required Validation

- Dependency restore and complete Release build.
- Admin and display production builds and frontend tests.
- Unsigned Google TV and Fire TV debug builds.
- Samsung Tizen and LG webOS static package validation.
- All non-integration unit tests.
- Repository migration inventory validation.
- GitHub Actions review of the exact PR head.

## Explicitly Skipped

- Azure SQL and all other integration tests.
- Tests requiring external services, credentials, hosted infrastructure, containers, or cross-system integration.
- Store enrollment, signing, submission, certification, simulator, and physical-device tests.

## Boundaries

This closure package adds validation evidence only. It does not add platform behavior, billing UX, store operations, integration infrastructure, or later-phase functionality.
