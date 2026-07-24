# Vennu Project Status

## Current Phase

**Phase 02 — Core Backend and Real-Time Engine**

## Milestone

A screen can boot, fetch content, connect to SignalR, send heartbeats, receive real-time updates, and transition offline when heartbeats stop.

## Completed

- Core projects, models, repositories, migrations, and data-access abstractions
- Venue and screen creation endpoints
- Pairing code creation, status, and claim endpoints
- Display content endpoint
- Display heartbeat endpoint
- SignalR hub scaffolding at `/hubs/vennu`
- Initial unit, integration, and E2E coverage for backend flows
- WP-02.08 — Display Application Foundation
- WP-02.09 — Display Boot Flow (pending PR review and merge)

## Active Work Package

**WP-02.09 — Display Boot Flow**

Status: Complete pending review and merge.

## Remaining Phase 02 Packages

1. WP-02.10 — SignalR Display Connection
2. WP-02.11 — Display Heartbeat
3. WP-02.12 — Backend Notification Abstraction
4. WP-02.13 — Offline Heartbeat Monitor
5. WP-02.14 — Phase 02 Vertical-Slice Validation

## WP-02.09 Completion Evidence

Implemented:

- Fetch display content once from `GET /api/display/{screenId}/content`.
- Deterministic loading, not-found, API-error, and ready states.
- Minimal board rendering using only the established API response contract.
- Focused frontend tests for URL construction, success, 404, server failure, and network failure.

Validation:

- Node test command added without introducing another package dependency.
- Display production build and repository validation are delegated to PR CI.

## Blockers

None currently recorded.

## Next Action

Review and merge the WP-02.09 draft PR, then claim `WP-02.10 — SignalR Display Connection`.

## Phase Gate

Do not begin Phase 03 until WP-02.14 is complete and its success criteria are recorded.
