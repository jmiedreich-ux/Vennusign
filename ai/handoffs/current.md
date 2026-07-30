# Vennu Session Handoff

## Work Package

- ID: WP-08.08
- Status: Complete and merged
- Execution mode: Sequential

## Git State

- Branch: `wp/08.08-emergency-broadcast`
- Issue: #169
- Pull request: #170
- Latest reviewed commit: `fb0abf1`
- Merge commit: `aa19c83`
- CI state: GitHub Actions runs #395 and #396 passed

## Completed This Session

- Added scoped emergency broadcast domain, persistence, active selection, and protected API.
- Added tier-visible venue-wide/screen-targeted activation and cancellation.
- Added realtime full-screen preemption with authoritative expiry recovery.

## Decisions

- Screen targets must belong to the venue.
- Targeted broadcasts win over venue-wide broadcasts; cancelled and expired rows are ignored.

## Validation

- Results: Release build, admin/display production builds and tests, migration inventory, and required non-integration tests passed in Actions runs #395 and #396.
- Skipped: all integration-type tests by standing owner instruction.

## Remaining Work

- WP-08.09 — Date-Range Promotions.

## Exact Next Action

Claim and implement WP-08.09.

## Do Not Redo or Reverse

- Do not redo WP-08.08 broadcast targeting, realtime, or expiry behavior.
- WP-08.09 should add bounded promotion precedence without changing broadcast precedence.
