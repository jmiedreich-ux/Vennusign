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

## Active Work Package

**WP-02.08 — Display Application Foundation**

Status: Not Started

## Remaining Phase 02 Packages

1. WP-02.08 — Display Application Foundation
2. WP-02.09 — Display Boot Flow
3. WP-02.10 — SignalR Display Connection
4. WP-02.11 — Display Heartbeat
5. WP-02.12 — Backend Notification Abstraction
6. WP-02.13 — Offline Heartbeat Monitor
7. WP-02.14 — Phase 02 Vertical-Slice Validation

## Blockers

None currently recorded.

## Next Action

Implement `docs/work-packages/WP-02.08-display-application-foundation.md` without beginning display boot, SignalR event handling, or heartbeat behavior.

## Phase Gate

Do not begin Phase 03 until WP-02.14 is complete and its success criteria are recorded.
