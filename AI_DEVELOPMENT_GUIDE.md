# Vennu AI Development Guide

## Purpose

Vennu is a venue display platform. The active milestone is completion of Phase 02: a screen can boot, fetch content, connect to SignalR, send heartbeats, and receive real-time updates.

## Read First

1. `PROJECT_STATUS.md`
2. The active file under `docs/work-packages/`
3. `NEXT_STEPS.md`
4. `Vennu_Roadmap_v5.md` only when broader context is required

Do not reload the full roadmap when the active work package provides enough context.

## Architecture Boundaries

- Target `.NET 9`.
- Keep `Vennu.DataAccess` generic and reusable.
- Put Vennu-specific repositories and persistence behavior in `Vennu.Data`.
- Put shared domain models in `Vennu.Core.Models`.
- Put HTTP contracts, controllers, API infrastructure, SignalR hubs, and hosted services in `Vennu.Api` unless an existing project boundary clearly requires otherwise.
- Keep the display SPA under `src/display` and run it separately with npm.
- Do not add `src/display` as a Visual Studio Website project.
- Apply database schema changes only through DbUp scripts.

## Work-Package Rules

- Implement one work package at a time.
- Do not refactor unrelated code.
- Do not begin dependent packages before their dependency is complete.
- Preserve public contracts unless the package explicitly authorizes a contract change.
- Prefer the smallest change that satisfies the acceptance criteria.
- Add or update tests in the same package as the behavior.
- Update `PROJECT_STATUS.md` when a package starts or completes.

## Required Completion Report

For every completed package, report:

1. Files changed
2. Behavior implemented
3. Tests added or updated
4. Validation commands executed
5. Results
6. Remaining risks or blockers

## Definition of Done

A work package is complete only when:

- All acceptance criteria pass.
- Required tests exist and pass.
- The repository builds for the affected projects.
- No unrelated behavior was changed.
- Documentation and `PROJECT_STATUS.md` reflect the result.

## Validation

Run the narrowest relevant tests during development. Before marking a package complete, run:

```powershell
./scripts/validate.ps1
```

Use `-SkipDisplay` or `-SkipIntegration` only when the package does not affect those areas, and record the skipped checks in the completion report.

## Prohibited Assumptions

- Do not invent database columns, repository contracts, routes, event names, or payload shapes.
- Inspect existing implementations before adding parallel abstractions.
- Do not replace working code solely for style consistency.
- Do not start Phase 03 until WP-02.14 is complete.
