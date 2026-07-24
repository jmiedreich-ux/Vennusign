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

## Active Work Package

**WP-02.11 — Display Heartbeat**

Status: Complete pending review and merge.

## Remaining Phase 02 Packages

1. WP-02.12 — Backend Notification Abstraction
2. WP-02.13 — Offline Heartbeat Monitor
3. WP-02.14 — Phase 02 Vertical-Slice Validation

## WP-02.11 Completion Evidence

Implemented:

- Immediate heartbeat after successful display boot.
- `POST /api/display/{screenId}/heartbeat` every 30 seconds.
- Existing `{ status: "Online" }` API request contract.
- A single guarded loop that prevents overlapping requests.
- Temporary failure handling without crashing the display.
- Timer and in-flight request cleanup during teardown.
- Controlled-timer frontend tests.

Validation:

- Focused Node tests cover URL construction, request contract, interval cadence, overlap prevention, and teardown.
- Display production build, existing heartbeat API tests, and repository validation are delegated to PR CI.

## Blockers

None currently recorded.

## Next Action

Review and merge the WP-02.11 draft PR, then claim `WP-02.12 — Backend Notification Abstraction`.

## Phase Gate

Do not begin Phase 03 until WP-02.14 is complete and its success criteria are recorded.
