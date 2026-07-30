# Vennu Session Handoff

## Work Package

- ID: WP-08.07
- Status: Complete and merged
- Execution mode: Sequential

## Git State

- Branch: `wp/08.07-playlist-domain-player-rotation`
- Issue: #166
- Pull request: #167
- Latest reviewed commit: `1acb4a7`
- Merge commit: `08561dc`
- CI state: GitHub Actions runs #390 and #391 passed

## Completed This Session

- Added screen-scoped playlist domain, repository, migration, and protected API.
- Added tier-visible playlist creation and deterministic reordering.
- Added active-window filtering and stable player rotation.

## Decisions

- Playlist windows are evaluated through venue timezone from an injected UTC instant.
- Only menu, image, and message slides are supported; dwell is bounded to 5–120 seconds.

## Validation

- Results: Release build, admin/display production builds and tests, migration inventory, and required non-integration tests passed in Actions runs #390 and #391.
- Skipped: all integration-type tests by standing owner instruction.

## Remaining Work

- WP-08.08 — Emergency Broadcast.

## Exact Next Action

Claim and implement WP-08.08.

## Do Not Redo or Reverse

- Do not redo WP-08.07 playlist persistence, administration, or rotation.
- WP-08.08 should preempt playlists deterministically and recover them after expiry.
