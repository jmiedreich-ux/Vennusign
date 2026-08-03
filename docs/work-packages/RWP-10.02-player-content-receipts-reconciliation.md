# RWP-10.02 — Durable Player Content Receipts and Delivery Reconciliation

## Outcome

Operators can distinguish a queued push from content actually received and applied by a player, and offline players converge on the latest authoritative revision after reconnecting.

## Required Implementation

- Add a monotonic or immutable authoritative content revision.
- Include target revision in realtime reload events and content snapshots.
- Durably report received/applied status with screen identity, player/shell version, timestamps, and safe failure detail.
- Persist and reconcile requested/applied revisions through idempotent tenant-safe APIs.
- Show pending, received, applied, stale, offline, failed, superseded, and recovered states.
- Compare authoritative and applied revisions after startup/reconnect without replaying obsolete pushes.
- Define retention and audit behavior without storing content bodies or secrets.

## Acceptance Criteria

- Operators can verify which authoritative revision a selected screen applied.
- Offline/reconnecting players converge and acknowledge application.
- Late, duplicate, reordered, forged, or cross-screen receipts cannot regress or cross tenant boundaries.
- Player, hosted SPA, shell, and API compatibility aligns with the RWP-00.04 release manifest.
- Focused non-integration tests cover issuance, snapshot/event consistency, authentication, idempotency, ordering, recovery, supersession, UI states, and retention.

## Queue and Boundaries

- Issue: #441
- Sequential; follows RWP-08.02 and precedes RWP-00.05.
- Phase 14+ remains paused.
- Integration-type tests remain skipped under the standing owner instruction.
