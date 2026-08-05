# RWP-00.72 — Entertainment & Attractions Default Dashboard

## Status

Complete in this proposed merge state.

## Issue

- #547

## Dependency verification

- RWP-00.71 merged through PR #603.
- Issue #546 is closed.
- RWP-00.72 is the first unfinished approved Entertainment & Attractions item.

## Objective

Define the task-first Entertainment & Attractions default dashboard and starter-menu information architecture. Prioritize current visitor impact, operational exceptions, schedules, closures, queues/wait/capacity, admission, wayfinding, notices, screen/publish health, recovery, role-aware presentation, multi-venue scope, and mobile/desktop behavior.

## Delivered

- Added `track0/industries/entertainment-attractions-default-dashboard.md`.
- Defined a persistent venue/area/experience context header and safe context-switching rules.
- Defined exception-first visibility for failed/partial delivery, offline/outdated screens, stale or conflicting sources, mismatched public state, expired notices, missing language fallback, and blocked actions.
- Defined quick core actions for schedules, attractions, exhibits, events, queues, capacity, admission, wayfinding, notices, publication, correction, retry, and restoration.
- Defined now/today/next, schedule health, queue/wait/capacity/admission, wayfinding, notices, screen/publish health, source freshness, upcoming work, and multi-venue sections.
- Defined role-aware presentation for front-line operators, editors, publishers, venue administrators, portfolio/enterprise administrators, and limited collaborators.
- Defined mobile-first and desktop priorities and complete first-use, empty, permission, tier, add-on, integration, limit, privacy, state, partial-delivery, and recovery coverage.
- Applied project-local Impeccable `shape`, `clarify`, `harden`, and bounded `polish` guidance.

## Validation

- Reviewed against issue #547, RWP-00.63–00.71, `AGENTS.md`, the Track 0 execution packet, and project-local Impeccable guidance.
- Current visitor impact and operational exceptions remain above analytics and promotion.
- Manual schedules, notices, queue/capacity/admission guidance, wayfinding, publication, delivery visibility, retry, and restore remain core.
- Healthy aggregate state cannot hide a failed, outdated, unknown, or excluded target.
- Schedule, wait, capacity, admission, route, accessibility, reopening, and source freshness are never inferred.
- State, permission, tier, add-on, limit, source, privacy, and rollout remain separate.
- Documentation-only scope; no product behavior or implementation.
- Azure SQL and all integration/external-system tests remain skipped.

## Completion checkpoint

Shared living-record updates are queued for reconciliation in the final Entertainment & Attractions completion checkpoint. RWP-00.73 is the exact next item.

## Handoff

After merge, issue closure, default-branch verification, and claim release, execute **RWP-00.73 — Entertainment & Attractions KPIs & Analytics** (#548).