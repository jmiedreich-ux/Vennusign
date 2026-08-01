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
- Every change, including documentation-only work and local-only development configuration work, must start from one approved GitHub issue and proceed on its own branch and pull request.
- Check `tracker/assignments.json` before claiming work.
- Every work item must be explicitly claimed in `tracker/assignments.json` before changes begin so another agent can see the active owner, branch, issue, and execution mode.
- Use one integration branch and pull request per work package unless inseparable work is explicitly documented.
- Branch names use `wp/<id>-<short-name>`.
- Commit messages begin with the work-package ID.
- Do not mark a package complete until acceptance criteria pass, required GitHub Actions checks pass, documentation is synchronized, the mandatory ChatGPT review is approved, and the PR is merged.
- Delete the package branch after merge unless a documented exception requires retention.
- Keep `PROJECT_STATUS.md`, `tracker/assignments.json`, the active work package, and `ai/handoffs/current.md` synchronized.
- A completed package may not remain listed as Not Started, In Progress, Review, or the next action anywhere in the repository.

## Execution Modes

- Every WP or RWP must declare exactly one execution mode: `Sequential` or `Collaborative`.
- `Sequential` means the agent executes the package independently within the approved scope, then records results for review.
- `Collaborative` means the user and agent intentionally drive the package together step by step, but the package still uses the normal issue, branch, pull request, validation, documentation, and approval gates.
- Execution mode changes how the work is carried out, not the package type, queue position, ownership rules, validation requirements, or approval requirements.
- Local-only development configuration work also follows the same issue, branch, and PR flow, but secrets and machine-specific values must remain out of the repository and be represented only by safe documentation or placeholder configuration.

## GitHub Actions as the Authoritative Validation Environment

- GitHub Actions is the authoritative validation environment for every implementation pull request.
- Local builds and tests are optional developer-productivity checks; they do not replace required GitHub Actions validation.
- Every implementation PR must run the required non-integration workflows and checks for the affected areas before ChatGPT may approve it.
- Ordinary work uses impact-based validation: run the nearest relevant non-integration checks for the area changed, then widen scope whenever shared contracts, shared models, project files, dependency injection, authentication, migrations, workflow definitions, or other cross-cutting surfaces are touched.
- Phase-closure work packages and equivalent closure PRs must run the broader full-repository non-integration regression suite before approval.
- Required checks must complete successfully against the exact PR head commit being reviewed.
- Missing, skipped, cancelled, stale, or failing required non-integration checks block approval unless the work package explicitly documents why a check is not applicable and ChatGPT accepts that exception during review.
- Owner-approved standing exception: skip all integration-type tests for every work package, including Azure SQL and tests requiring external services, credentials, hosted infrastructure, containers, or cross-system integration. Their omission or failure is never a completion or merge blocker. Restore, Release build, display production build, unit tests, static analysis, and applicable non-integration migration validation remain required.
- If a required workflow does not exist for an affected area, creating or extending the workflow is part of the work package before approval.
- GitHub Actions logs, job results, artifacts, and combined commit status are the source of truth for validation evidence.
- A successful local run may be recorded as supplemental evidence, but approval still requires passing required GitHub checks.
- Any new commit after successful CI requires the applicable checks to run again against the new head commit.
- Work-package completion evidence and the session handoff must record the workflow names, run status, head commit SHA, and any intentionally non-applicable checks.

## Mandatory Pull-Request Review and Approval

- Every pull request must be reviewed by ChatGPT through the connected GitHub workflow before merge.
- This rule applies to all changes, including AI-generated changes, human-authored changes, documentation-only changes, dependency updates, fixes, and emergency corrections.
- The implementation agent or author may prepare the PR but may not declare it approved or merge-ready.
- ChatGPT must inspect the PR metadata, complete diff, changed files, tests, validation evidence, GitHub Actions results, acceptance criteria, architecture boundaries, security impact, and documentation consistency.
- ChatGPT must review unresolved comments and requested changes before issuing a final decision.
- The allowed final review decisions are `APPROVE`, `REQUEST_CHANGES`, or `COMMENT` when the PR is not ready for a final decision.
- A PR may not merge until ChatGPT has explicitly issued `APPROVE` against the latest reviewed commit.
- Any new commit pushed after approval invalidates that approval and requires a new ChatGPT review of the updated head commit and fresh required GitHub Actions results.
- Authors and lane agents must not merge, enable auto-merge, or bypass review before the required approval is recorded and required checks pass.
- Only ChatGPT performs the final code review and approval decision. After that approval is recorded and the required checks are green on the reviewed head commit, the active agent may merge the pull request.
- When GitHub prevents a formal approving review because the reviewing identity is also the PR author, ChatGPT must record an explicit top-level review comment containing `CHATGPT APPROVED`, the reviewed head commit SHA, validation status, and any residual risks. That recorded decision is the required approval gate.
- Approval is invalid if required GitHub Actions checks are failing or incomplete, acceptance criteria are incomplete, unresolved blocking comments remain, documentation is stale, secrets are exposed, or the reviewed commit no longer matches the PR head.

## Gap and Remediation Workflow

- Testing, review, and implementation agents record newly discovered gaps as GitHub issues first. They must not create, renumber, reprioritize, or insert work packages directly.
- Use the `gap-found` label for a discovered gap. Add `remediation` only after planning review confirms remediation work is required, plus the affected phase label.
- A small defect that stays within the active package's approved scope may be fixed there only when the issue and package record explicitly link the decision.
- A substantial architectural, cross-package, or historical gap must be promoted by the designated planning agent to a Remediation Work Package (RWP) before implementation.
- RWP IDs use `RWP-<origin-phase>.<sequence>`, such as `RWP-05.02`. They supplement completed roadmap history; never renumber or rewrite completed WPs.
- Promotion requires an approved GitHub issue with problem statement, evidence, affected architecture, acceptance criteria, dependencies, queue position, and migration or reconciliation impact.
- Every RWP uses one claimed issue, one branch named `rwp/<id>-<short-name>`, one PR, required GitHub Actions validation, ChatGPT review, completion evidence, and merge.
- Only the designated planning agent may promote a finding to an RWP or change queue priority. Testing agents create findings and evidence only.
- When remediation changes a surface used by later completed work, create an explicit reconciliation package before continuing dependent roadmap work.

## Multi-Agent Rules

- Default to sequential execution when ownership cannot be divided cleanly.
- Prefer `Collaborative` execution mode when the user explicitly wants to drive implementation interactively with the agent.
- Before parallel work starts, define each lane's writable, read-only, and prohibited files.
- No two active agents may modify the same file.
- Shared contracts, project files, dependency injection, migrations, package configuration, shared fixtures, trackers, handoffs, and GitHub workflow files are orchestrator-owned.
- Lane agents must stop rather than edit outside their declared scope.
- Only the orchestrator integrates lanes, resolves conflicts, updates shared tracking, and decides readiness for ChatGPT review.
- Unexpected overlap invalidates parallel safety and requires re-planning.

## Documentation Consistency Gate

Before completing or merging a work package, verify:

1. The work-package file has the correct status and completion evidence.
2. `PROJECT_STATUS.md` has the correct active phase, completed packages, next package, and blockers.
3. `tracker/assignments.json` has no stale claim.
4. `ai/handoffs/current.md` describes the actual repository state and exact next action.
5. A dated immutable handoff exists under `ai/handoffs/archive/`.
6. Relevant architecture, API, database, operational, or CI documentation is updated when behavior or boundaries changed.
7. Required GitHub Actions checks passed against the current PR head commit.
8. ChatGPT approval is recorded against the current PR head commit.

If any applicable record is stale, the package remains incomplete even when code and tests pass.

## Mandatory Session Handoff

Before ending a development session:

- Run or trigger the applicable non-integration validation appropriate to the change.
- Ensure required GitHub Actions workflows have run against the current PR head.
- Update the active work package and all applicable tracking files.
- Replace `ai/handoffs/current.md` using `ai/handoffs/template.md`.
- Add a dated archive copy under `ai/handoffs/archive/`.
- Record branch, commit, issue, PR, validation, GitHub Actions workflows and results, changed files, decisions, remaining work, risks, CI state, and ChatGPT review status.
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

- Add or update non-integration tests with every behavioral change.
- Prefer unit tests for business rules and mapping.
- Do not run integration-type tests under the standing repository-owner exception.
- Run the narrowest relevant non-integration tests during development when a local checkout is available.
- Use the nearest affected validation by default, but widen scope when shared or cross-cutting surfaces are affected.
- Before completion, ensure the required GitHub Actions workflows execute the authoritative impact-based validation for the affected areas.
- Before phase closure, ensure the broader full non-integration suite executes successfully across the repository.
- `./scripts/validate.ps1` remains the standard local validation entry point and should be used when available, but local execution does not replace required GitHub Actions checks.
- Record any intentionally skipped or non-applicable checks in the PR and work-package completion evidence.
