# RWP-05.07 — Atomic Screen Replacement and Pairing Recovery

## Outcome

Replacing a physical player preserves the selected logical screen, its supported configuration and operational history, while rotating device identity safely and recoverably.

## Required Implementation

- Provide a deliberate replacement workflow that selects the existing logical screen and new player pairing code.
- Preserve applicable configuration, targeting, audit/history relationships, and video-wall placement.
- Make pairing claim, ownership checks, capacity handling, and assignment atomic or restart-safe with deterministic reconciliation.
- Handle expired, already-claimed, partial, retried, canceled, unauthorized, and conflicting attempts.
- Keep replacement distinct from new-screen pairing, unpair, archive/restore, and venue transfer.
- Provide impact preview, confirmation, completion feedback, and audit evidence.

## Acceptance Criteria

- Replacement does not create a second logical screen or lose supported screen state.
- Failed assignment cannot irreversibly consume a code without a linked or recoverable player.
- Retries are idempotent and tenant/venue safe.
- Capacity evaluates the resulting active logical fleet.
- Old credentials stop operating the screen after cutover.
- Focused non-integration tests cover success, partial failure, retry, cancellation, stale/claimed codes, ownership, capacity, credential rotation, preservation, and audit behavior.

## Queue and Boundaries

- Issue: #439
- Sequential; follows RWP-00.04 and precedes RWP-08.02.
- Phase 14+ remains paused.
- Integration-type tests remain skipped under the standing owner instruction.
