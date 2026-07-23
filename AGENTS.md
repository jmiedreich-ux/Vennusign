# Vennu Development Instructions

## Mandatory Session Startup

Before making any development change, read these files in order:

1. `AGENTS.md`
2. `ai/handoffs/current.md`
3. `PROJECT_STATUS.md`
4. `tracker/assignments.json`
5. The active work package under `docs/work-packages/`
6. `AI_DEVELOPMENT_GUIDE.md`
7. Any linked GitHub issue, branch, pull request, and CI result

Repository and GitHub state are the source of truth. Chat history is supporting context only.

## Architecture

- Target `.NET 9`.
- Keep `Vennu.DataAccess` generic and reusable.
- Put Vennu-specific repositories and persistence behavior in `Vennu.Data`.
- Put shared domain models in `Vennu.Core.Models`.
- Keep HTTP transport, SignalR hubs, API composition, and hosted services in `Vennu.Api` unless an existing boundary requires otherwise.
- Keep the display SPA under `src/display` and run it independently with npm.
- Never add `src/display` as a Visual Studio Website project.
- Apply database schema changes only through DbUp scripts.

## AI Work-Package Workflow

- Every implementation change must map to one documented work package.
- Check `tracker/assignments.json` before claiming work.
- Use one integration branch and pull request per work package unless inseparable work is explicitly documented.
- Branch names use `wp/<id>-<short-name>`.
- Commit messages begin with the work-package ID.
- Do not mark a package complete until acceptance criteria pass, validation succeeds, documentation is synchronized, and the PR is merged.
- Delete the package branch after merge unless a documented exception requires retention.
- Keep `PROJECT_STATUS.md`, `tracker/assignments.json`, the active work package, and `ai/handoffs/current.md` synchronized.
- A completed package may not remain listed as Not Started, In Progress, Review, or the next action anywhere in the repository.

## Multi-Agent Rules

- Default to sequential execution when ownership cannot be divided cleanly.
- Before parallel work starts, define each lane's writable, read-only, and prohibited files.
- No two active agents may modify the same file.
- Shared contracts, project files, dependency injection, migrations, package configuration, shared fixtures, trackers, and handoffs are orchestrator-owned.
- Lane agents must stop rather than edit outside their declared scope.
- Only the orchestrator integrates lanes, resolves conflicts, updates shared tracking, and decides readiness.
- Unexpected overlap invalidates parallel safety and requires re-planning.

## Documentation Consistency Gate

Before completing or merging a work package, verify:

1. The work-package file has the correct status and completion evidence.
2. `PROJECT_STATUS.md` has the correct active phase, completed packages, next package, and blockers.
3. `tracker/assignments.json` has no stale claim.
4. `ai/handoffs/current.md` describes the actual repository state and exact next action.
5. A dated immutable handoff exists under `ai/handoffs/archive/`.
6. Relevant architecture, API, database, or operational documentation is updated when behavior or boundaries changed.

If any applicable record is stale, the package remains incomplete even when code and tests pass.

## Mandatory Session Handoff

Before ending a development session:

- Run the validation appropriate to the change.
- Update the active work package and all applicable tracking files.
- Replace `ai/handoffs/current.md` using `ai/handoffs/template.md`.
- Add a dated archive copy under `ai/handoffs/archive/`.
- Record branch, commit, issue, PR, validation, changed files, decisions, remaining work, risks, and CI state.
- State one concrete Exact Next Action.
- State what the next agent must not redo or reverse.
- Keep unfinished work on its package branch and use a draft PR for handoff.

“Continue implementation” is not a valid handoff.

## Code Quality

- Follow repository formatting and analyzer configuration.
- Enable nullable reference types and implicit usings in .NET projects unless an existing project has a documented exception.
- Use asynchronous APIs and pass `CancellationToken` through I/O paths.
- Validate configuration at startup.
- Do not add dependencies without documenting the reason.
- Do not commit credentials, tokens, connection strings, or local secret files.
- Do not invent database columns, routes, contracts, event names, or payload shapes.
- Do not refactor unrelated code.

## Testing and Validation

- Add or update tests with every behavioral change.
- Prefer unit tests for business rules and mapping.
- Use integration tests for repository, API, SignalR, and provider behavior.
- Run the narrowest relevant tests during development.
- Before completion, run `./scripts/validate.ps1` and record any intentionally skipped checks.
