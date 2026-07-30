# Vennu Session Handoff

## Work Package

- ID: WP-08.08
- Status: In progress
- Execution mode: Sequential

## Git State

- Branch: `wp/08.08-emergency-broadcast`
- Issue: #169
- Pull request: pending
- CI state: pending

## Completed This Session

- Added scoped emergency broadcast domain, persistence, active selection, and protected API.
- Added tier-visible venue-wide/screen-targeted activation and cancellation.
- Added realtime full-screen preemption with authoritative expiry recovery.

## Decisions

- Screen targets must belong to the venue.
- Targeted broadcasts win over venue-wide broadcasts; cancelled and expired rows are ignored.

## Validation

- Results: GitHub Actions pending.
- Skipped: all integration-type tests by standing owner instruction.

## Remaining Work

- Validate, review, and merge WP-08.08.

## Exact Next Action

Publish WP-08.08 and validate its exact PR head in GitHub Actions.

## Do Not Redo or Reverse

- Do not redo WP-08.07 playlist behavior.
- Do not add date-range promotions or notification integrations.
