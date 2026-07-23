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

## Active Work Package

None. WP-02.09 is next.

## Remaining Phase 02 Packages

1. WP-02.09 — Display Boot Flow
2. WP-02.10 — SignalR Display Connection
3. WP-02.11 — Display Heartbeat
4. WP-02.12 — Backend Notification Abstraction
5. WP-02.13 — Offline Heartbeat Monitor
6. WP-02.14 — Phase 02 Vertical-Slice Validation

## WP-02.08 Completion Evidence

Implemented:

- `/display/{screenId}` route parsing and page exposure
- Centralized API and SignalR endpoint configuration
- Top-level application error boundary
- Independent display install, build, preview, and run documentation

Validation and review:

- Existing `phase02-tests` run passed .NET restore, Release build, unit tests, and integration tests before the branch rebuild.
- Full PR diff, scope, architecture, configuration, and documentation review completed.
- GitHub suppressed fresh Actions runs for connector-authored commits; this remains a documented residual validation limitation.

## Blockers

None currently recorded.

## Next Action

Claim and implement `docs/work-packages/WP-02.09-display-boot-flow.md` after PR #7 is merged.

## Phase Gate

Do not begin Phase 03 until WP-02.14 is complete and its success criteria are recorded.
