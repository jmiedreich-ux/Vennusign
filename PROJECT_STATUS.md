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
- WP-02.12 — Backend Notification Abstraction
- WP-02.13 — Offline Heartbeat Monitor

## Active Work Package

**WP-02.14 — Phase 02 Vertical-Slice Validation**

Status: Complete pending PR CI, review, and merge.

## Phase 02 Validation Evidence

- HTTP E2E coverage creates a venue and screen, creates and claims a pairing code, loads paired display content, and records online heartbeat state.
- The Phase 02 vertical-slice test proves a stale heartbeat transitions the screen from `Online` to `Offline`.
- WP-02.10 display tests prove screen group membership, reconnection, and event handling.
- WP-02.12 notifier tests prove screen and venue group routing for all four verified SignalR events.
- The two-context browser procedure and accepted execution limitation are recorded in `docs/validation/phase-02-vertical-slice.md`.
- The full `./scripts/validate.ps1` suite is delegated to PR CI.

## Blockers

None currently recorded.

## Next Phase

**Phase 03 — Tier System & Feature Flags**

First package: **WP-03.01 — Feature and Tier Core Models**.

Do not begin WP-03.01 until WP-02.14 CI succeeds and the PR is merged.
