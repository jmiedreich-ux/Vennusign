# RWP-00.05 — Affected-Screen Action Completeness and Recovery

## Outcome

Screens affected by the completed remediation round expose the necessary deliberate actions and accurately distinguish loaded, empty, failed, dirty, saving, and saved states.

## Required Implementation

- Add an explicit selected-screen Preview action and safe preview surface for supported layouts.
- Add Save and Cancel/Revert for screen name/location edits with clear dirty and failure states.
- Keep failed edits visibly unsaved and retryable.
- Separate Account Security loading, failed, confirmed-empty, and loaded states; add Retry.
- Add Retry/Reload for Theme Builder initial-load failure.
- Complete a bounded action matrix for screens changed by the 13-RWP round and resolve only verified omissions.
- Preserve authorization, venue scope, passkey lockout safety, targeting, and boundaries of RWP-05.07 and RWP-10.02.

## Acceptance Criteria

- Explicit Preview works only for a valid selected active authorized screen and never pushes content.
- Screen identity edits provide Save and Cancel/Revert; failed saves do not appear committed.
- Account Security never reports no passkeys when inventory is unknown and provides Retry.
- Theme Builder can retry initial loading without a browser refresh.
- The action matrix records primary, cancel, destructive-confirmation, retry, refresh, and navigation coverage plus approved exclusions.
- Focused frontend tests exercise state transitions and controls rather than source-string assertions alone.

## Queue and Boundaries

- Issue: #442
- Sequential; follows RWP-10.02 and is fifth in the scheduled queue.
- Phase 14+ remains paused.
- Browser/device/Azure and other integration-type tests remain skipped under the standing owner instruction.
