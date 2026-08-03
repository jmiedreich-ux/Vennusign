# Vennusign Session Handoff

## Current State

- Item: RWP-08.01 — Scheduling and Live-Control Safety / issue #346
- Mode: Sequential
- Branch: `rwp/08.01-scheduling-live-control-safety`
- Status: Complete in the proposed merge state

## Result

- Back Office scheduling now has accessible task tabs with a durable deep link, an overview of live precedence, and explicit no-screen/error states.
- Meal-period priority is persisted through an authorized API operation; enable changes persist immediately; delete and ordering outcomes are explicit.
- Playlist administration is bound to a selected screen and supports create, edit, enable, day/window controls, ordering, confirmed removal, and recovery feedback.
- Promotions disclose server priority and venue-local resolution, announce saves, and confirm archive actions.
- Emergency activation/cancellation discloses target impact, requires confirmation, reports queued delivery honestly, preserves recent history, and blocks activation without targets.
- The durable contract and UI/function gap analysis are recorded in `docs/architecture/scheduling-live-control.md` and `docs/archive/work-packages/RWP-08.01-scheduling-live-control-safety.md`.

## Validation

- Back Office Node tests pass locally (55/55).
- Back Office production build passes locally.
- Focused .NET checks are delegated to exact-head affected-area GitHub Actions because local .NET tooling is unavailable.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

After this RWP merges and its claim is released, reassess and claim only RWP-09.01 / issue #414 in Sequential mode if it has no active owner.

## Do Not Redo

Do not flatten scheduling back into one long page, treat browser time as authoritative, allow implicit screen targets, remove live-impact confirmations, report unobserved player delivery as acknowledged, skip the recorded queue, or resume Phase 14+.
