# Vennu Session Handoff

## Work Package

- ID: WP-08.07
- Status: In progress
- Execution mode: Sequential

## Git State

- Branch: `wp/08.07-playlist-domain-player-rotation`
- Issue: #166
- Pull request: pending
- CI state: pending

## Completed This Session

- Added screen-scoped playlist domain, repository, migration, and protected API.
- Added tier-visible playlist creation and deterministic reordering.
- Added active-window filtering and stable player rotation.

## Decisions

- Playlist windows are evaluated through venue timezone from an injected UTC instant.
- Only menu, image, and message slides are supported; dwell is bounded to 5–120 seconds.

## Validation

- Results: GitHub Actions pending.
- Skipped: all integration-type tests by standing owner instruction.

## Remaining Work

- Validate, review, and merge WP-08.07.

## Exact Next Action

Publish WP-08.07 and validate its exact PR head in GitHub Actions.

## Do Not Redo or Reverse

- Do not broaden playlist slides to video or external media validation.
- Do not add emergency broadcasts or date-range promotions.
