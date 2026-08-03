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
