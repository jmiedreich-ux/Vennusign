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
- WP-02.10 — SignalR Display Connection
- WP-02.11 — Display Heartbeat

## Active Work Package

**WP-02.12 — Backend Notification Abstraction**

Status: Complete pending review and merge.

## Remaining Phase 02 Packages

1. WP-02.13 — Offline Heartbeat Monitor
2. WP-02.14 — Phase 02 Vertical-Slice Validation

## WP-02.12 Completion Evidence

Implemented:

- `IScreenUpdateNotifier` application abstraction.
- SignalR-backed implementation using `IHubContext<VennuHub>`.
- Screen and venue routing with existing group conventions.
- Content, theme, item availability, and synchronization notifications.
- Dependency injection registration.
- Unit tests for every notifier method and routed payload.

Validation:

- API tests, solution build, and repository validation are delegated to PR CI.

## Blockers

None currently recorded.

## Next Action

Review and merge the WP-02.12 draft PR, then claim `WP-02.13 — Offline Heartbeat Monitor`.

## Phase Gate

Do not begin Phase 03 until WP-02.14 is complete and its success criteria are recorded.
