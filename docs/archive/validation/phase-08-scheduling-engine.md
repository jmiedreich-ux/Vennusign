# Phase 08 — Scheduling Engine Validation

## Result

Phase 08 is ready for closure when the WP-08.10 GitHub Actions run passes. Integration-type tests remain intentionally skipped under the standing repository-owner instruction.

## Acceptance Matrix

| Journey | Evidence |
| --- | --- |
| Venue-local timezone, overnight, and daylight-saving resolution | `MealPeriodScheduleResolverTests`, `HappyHourScheduleResolverTests`, `DateRangePromotionResolverTests` |
| Transition-only scheduled activation | `ScheduledContentActivationServiceTests`, `HappyHourEvaluatorServiceTests`, promotion transition assertions in `phase08-critical-journeys.test.mjs` |
| Meal-period administration and tier visibility | `meal-period-administration.test.mjs`, admin Phase 08 critical journeys |
| Happy-hour automatic/manual state and countdown | `HappyHourScheduleResolverTests`, `happy-hour-administration.test.mjs`, `happyHourBanner.test.mjs` |
| Ordered playlist rotation and recovery | `playlist-administration.test.mjs`, `playlistRotation.test.mjs`, display Phase 08 critical journeys |
| Broadcast target precedence and authoritative expiry | `EmergencyBroadcastServiceTests`, `emergency-broadcasts.test.mjs`, `emergencyBroadcast.test.mjs` |
| Inclusive date-range promotions and overlap precedence | `DateRangePromotionResolverTests`, `date-range-promotions.test.mjs`, `dateRangePromotion.test.mjs` |
| Realtime authoritative reload and offline fallback | `displayRealtime.test.mjs`, `displayCache.test.mjs`, display Phase 08 critical journeys |
| Ordered migration inventory | `MigrationResourceTests`, unit-category `DatabaseMigratorTests` |

## Required Validation

- Dependency restore and complete Release build.
- Admin and display production builds and frontend tests.
- All non-integration unit tests.
- Repository migration inventory validation.
- GitHub Actions review of the exact PR head.

## Explicitly Skipped

- Azure SQL integration tests.
- Tests requiring external services, credentials, hosted infrastructure, containers, or cross-system integration.

## Boundaries

This closure package adds validation evidence only. It does not add scheduling behavior or Phase 09 tap-list functionality.
