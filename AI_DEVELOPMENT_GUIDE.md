# Vennu AI Development Guide

## Purpose

Vennu is a venue display platform. Phases 02 and 03 are complete. Phase 04 — Super Admin CRM is active, beginning with its protected API and independent admin web application foundation.

`AGENTS.md` is the authoritative operating policy for AI development. This guide provides the concise implementation context used after mandatory session startup.

## Mandatory Read Order

1. `AGENTS.md`
2. `ai/handoffs/current.md`
3. `PROJECT_STATUS.md`
4. `tracker/assignments.json`
5. The active file under `docs/work-packages/`
6. This guide
7. `NEXT_STEPS.md` only when the package requires historical Phase 02 context
8. `Vennu_Roadmap_v5.md` only when broader product context is required
9. Linked GitHub issue, branch, pull request, and CI state

Repository and GitHub state are the source of truth. Do not reload the full roadmap when the active work package provides enough context.

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

- Every implementation change must map to one documented work package.
- Every change starts from one approved GitHub issue and proceeds on its own branch and pull request, including documentation-only work and local-only development configuration work.
- Check and claim the package in `tracker/assignments.json` before modifying code.
- Record every active work item in `tracker/assignments.json` before modifying code so the current owner, branch, issue, and execution mode are visible to other agents.
- Implement one work package at a time.
- Use branch format `wp/<id>-<short-name>` and begin commit messages with the work-package ID.
- Use one integration branch and pull request per package unless inseparable work is explicitly documented.
- Do not refactor unrelated code.
- Do not begin dependent packages before their dependency is complete.
- Preserve public contracts unless the package explicitly authorizes a contract change.
- Prefer the smallest change that satisfies the acceptance criteria.
- Add or update tests in the same package as the behavior.
- Delete package and lane branches after merge unless a documented exception requires retention.

## Execution Modes

- Each WP or RWP uses exactly one execution mode: `Sequential` or `Collaborative`.
- `Sequential` is the default when the agent performs the work independently inside the approved scope.
- `Collaborative` is used when the user and agent intentionally progress through the work together step by step.
- Execution mode does not create a new package type; it only describes how the approved WP or RWP is executed.
- Local-only development configuration work still follows the same issue, branch, PR, validation, and approval flow, but secrets must stay out of committed files.

## Multi-Agent Efficiency

- Default to sequential execution when file ownership cannot be divided cleanly.
- Prefer `Collaborative` execution mode when the user explicitly wants hand-in-hand implementation.
- Parallel work requires an orchestrator and explicit writable, read-only, and prohibited paths for every lane.
- No two agents may modify the same file concurrently.
- Shared contracts, project files, dependency injection, migrations, package configuration, shared fixtures, trackers, and handoffs remain orchestrator-owned.
- Contract-dependent lanes may begin only after the orchestrator freezes and integrates the contract.
- Lane agents stop rather than edit outside scope.
- Only the orchestrator integrates branches, resolves conflicts, updates shared tracking, and decides readiness.

## Required Completion Report

For every completed package, record:

1. Files changed
2. Behavior implemented
3. Tests added or updated
4. Validation commands executed
5. Results and CI state
6. Remaining risks or blockers
7. Branch, commit, issue, and pull request
8. One exact next action
9. What the next agent must not redo or reverse

## Documentation Consistency Gate

A work package is not complete until all applicable records agree:

- The active work-package document
- `PROJECT_STATUS.md`
- `tracker/assignments.json`
- `ai/handoffs/current.md`
- A dated archive under `ai/handoffs/archive/`
- Relevant architecture, API, database, or operational documentation

No repository record may continue to identify a completed package as Not Started, In Progress, Review, or the next action.

## Definition of Done

A work package is complete only when:

- All acceptance criteria pass.
- Required tests exist and pass.
- The repository builds for the affected projects.
- No unrelated behavior was changed.
- Validation and CI state are recorded.
- Documentation and tracking records are synchronized.
- The pull request is merged.
- The package branch is deleted unless retention is documented.

- After ChatGPT approval is recorded and the required non-integration GitHub checks pass on the reviewed head commit, the active agent may merge the pull request.

## Validation

Run the narrowest relevant tests during development. Before marking a package complete, run:

```powershell
./scripts/validate.ps1
```

Use `-SkipDisplay` or `-SkipIntegration` only when the package does not affect those areas, and record the skipped checks and reason in the handoff.

## Code Quality

- Follow repository formatting and analyzer configuration.
- Enable nullable reference types and implicit usings unless a documented project exception exists.
- Treat enforced compiler, analyzer, and code-style warnings as errors when repository configuration supports it.
- Use asynchronous APIs and pass `CancellationToken` through I/O paths.
- Validate configuration at startup.
- Do not add dependencies without documenting why.
- Do not commit credentials, tokens, connection strings, or local secret files.

## Prohibited Assumptions

- Do not invent database columns, repository contracts, routes, event names, or payload shapes.
- Inspect existing implementations before adding parallel abstractions.
- Do not replace working code solely for style consistency.
- Do not add Phase 04 behavior without a documented and claimed bounded work package.
- Do not use chat history as the sole authority when repository state differs.
