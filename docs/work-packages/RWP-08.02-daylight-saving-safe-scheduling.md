# RWP-08.02 — Daylight-Saving-Safe Scheduling Resolution

## Outcome

All scheduling resolvers behave deterministically and remain resilient when local wall-clock times are skipped or duplicated by daylight-saving transitions.

## Required Implementation

- Define durable policies for invalid spring-forward times and ambiguous fall-back times.
- Apply the policy to meal periods and audit happy hour, playlists, promotions, and quick-update reset resolvers.
- Prevent one invalid occurrence from failing a complete snapshot or background loop.
- Surface adjustment or ambiguity where operators need it without disrupting ordinary schedules.
- Preserve venue-timezone authority, overnight semantics, priority, and deterministic ordering.

## Acceptance Criteria

- DST transitions cannot by themselves cause scheduling resolution to throw.
- Spring-forward and fall-back behavior is documented and consistent.
- Active, next-occurrence, and background activation results agree.
- Overnight and week-boundary schedules remain correct.
- Focused non-integration tests cover invalid and ambiguous times, multiple zones, overnight windows, next-run resolution, and service resilience.

## Queue and Boundaries

- Issue: #440
- Sequential; follows RWP-05.07 and precedes RWP-10.02.
- Phase 14+ remains paused.
- Integration-type tests remain skipped under the standing owner instruction.

## Implementation policy and audit

- A configured wall-clock time inside a spring-forward gap advances minute-by-minute to the first valid local instant. The resolved occurrence is reported as `AdvancedAfterGap`.
- A duplicated fall-back wall-clock time resolves to the earlier UTC occurrence (the larger UTC offset). The resolved occurrence is reported as `EarlierAmbiguousOccurrence`; active windows remain active through both repetitions of their wall-clock interval.
- Meal-period next occurrences use this policy and expose the adjustment in the administration response. Happy-hour end calculation and quick-update local-midnight calculation use the same resolver.
- Playlist windows share the occurrence-aware active-window policy. Date-range promotions compare venue-local dates and perform no arbitrary local-to-UTC conversion, so they require no conversion change. Overnight and prior-day semantics remain unchanged.
- Scheduled content activation isolates an invalid venue evaluation so one venue cannot stop the rest of the background loop.
- Focused Data/API unit tests cover spring gaps, ambiguous starts and ends, both repeated-hour occurrences, overnight behavior, venue timezones, and per-venue background resilience. Integration-type tests remain skipped.
