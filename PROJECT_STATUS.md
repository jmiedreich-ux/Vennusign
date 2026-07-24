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
- WP-02.09 — Display Boot Flow

## Active Work Package

**WP-02.10 — SignalR Display Connection**

Status: Complete pending review and merge.

## Remaining Phase 02 Packages

1. WP-02.11 — Display Heartbeat
2. WP-02.12 — Backend Notification Abstraction
3. WP-02.13 — Offline Heartbeat Monitor
4. WP-02.14 — Phase 02 Vertical-Slice Validation

## WP-02.10 Completion Evidence

Implemented:

- SignalR connection to `/hubs/vennu` after display content loads.
- `JoinScreen(screenId)` on initial connection and reconnection.
- Automatic reconnect configuration.
- Deterministic handlers for `ContentUpdated`, `ThemeUpdated`, `ItemAvailabilityChanged`, and `SyncTick`.
- Controlled connecting, connected, reconnecting, and degraded states.
- Local display state updates without a full page reload.
- Focused lifecycle and event-handler tests.

Validation:

- Dependency-free Node tests cover event state changes and SignalR lifecycle orchestration.
- Display production build, backend SignalR tests, and repository validation are delegated to PR CI.

## Blockers

None currently recorded.

## Next Action

Review and merge the WP-02.10 draft PR, then claim `WP-02.11 — Display Heartbeat`.

## Phase Gate

Do not begin Phase 03 until WP-02.14 is complete and its success criteria are recorded.
