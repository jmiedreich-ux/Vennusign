# Vennusign Development Instructions

## Startup and Source of Truth

Before changing the repository, read only:

1. `AGENTS.md`
2. `ai/handoffs/current.md`
3. `tracker/assignments.json`
4. `PROJECT_STATUS.md`
5. The active build's records under `docs/builds/<build>/`, when a build is active
6. Linked issue, branch, PR, comments, and exact-head CI results

Read `AI_DEVELOPMENT_GUIDE.md`, component README files, architecture, or operations documents only when the task touches that area. Content under `docs/archive/`, `ai/handoffs/archive/`, and `track0/` is research-only: do not load it routinely. Repository and GitHub state override chat history and archived material.

## Working Model — Builds and Slices

Adopted 2026-08-07 from the Track 1 retrospective; replaces the earlier phase/track/WP model. Historical phase/track/RWP records remain valid as history only.

- The unit of work is a **build**, named by product area (e.g. Menus). A build is delivered in numbered **functional vertical slices**.
- **Design before implementation.** A build's UI work starts only after its design authority is approved and landed in `docs/design/approved/<build>/`. Where any other document disagrees with that bundle's `decisions.md`, the decisions win. Open design questions are resolved through the build's question register in `docs/builds/<build>/` before or alongside the affected slice — never silently.
- Every slice ships **schema → API → UI → Playwright specs together**; tests are written with the implementation, never after. Each slice is independently mergeable and leaves `master` releasable.
- Slice execution follows the GitHub-first discipline: create the slice issue, record the claim, branch as `build/<area>-s<n>-<short-name>`, open one PR, pass exact-head CI, obtain independent review, merge, then synchronize records. One slice at a time; a successor starts only after its predecessor is merged and its owner workbook is accepted.
- **Every slice ends with a short owner acceptance workbook (5–10 minutes)** before the next slice starts; a schema-only slice gets a demo script instead. Hosted-agent subjective QA (the Track 1 pattern) runs on demand when a slice carries judgment cases deterministic specs cannot assert.
- Keep changes bounded; do not refactor unrelated code or begin future-slice work. Delete completed branches after merge.

## Architecture

- Target `.NET 9`.
- Keep `Vennu.DataAccess` generic; Vennusign persistence belongs in `Vennu.Data` and shared domain models in `Vennu.Core.Models`.
- Keep HTTP, SignalR, API composition, and hosted services in `Vennu.Api` unless an established boundary requires otherwise.
- Keep `src/display` independent and never add it as a Visual Studio Website project.
- Apply schema changes only through ordered DbUp migrations; a migration that discards data names what it discards.
- Inspect existing contracts before adding routes, columns, events, payloads, or abstractions.

## Documentation Control

- Treat Markdown as a maintained interface, not a work log. Update an existing authoritative document before creating a new `.md` file.
- The controlled living records are `AGENTS.md`, `PROJECT_STATUS.md`, `ai/handoffs/current.md`, the tracker, the active build's records under `docs/builds/<build>/`, and affected durable architecture/operations documents.
- Batch living-record updates at slice completion. Do not edit tracker, status, or handoff after every local commit.
- A new Markdown file requires a durable audience and purpose not served by an existing file. Do not create per-experiment, per-prompt, or evidence-only Markdown; completion evidence belongs in the slice PR and issue.
- Keep historical material under `docs/archive/` or `ai/handoffs/archive/` and read it only for deliberate research.
- Never commit secrets, tokens, connection strings, generated output, runtime logs, or machine-specific configuration.

## Shared-File and Multi-Agent Safety

- No two agents may modify the same file concurrently.
- Contracts, project files, dependency injection, migrations, package configuration, shared fixtures, workflows, tracker, status, and handoff are orchestrator-owned.
- Check the tracker and open claims before starting; stop on ownership conflict and re-plan.

## Discoveries and Backlog

- Record discoveries as GitHub issues first. Owner-approved out-of-scope decisions become backlog issues at the moment of decision.
- Small in-scope defects may be fixed inside the active slice when explicitly linked; anything larger becomes its own issue and waits for scheduling.

## Testing and CI

- GitHub Actions is authoritative; required checks must pass on the exact reviewed PR head.
- Normal slice work runs affected Release builds, focused unit tests, static checks, applicable non-integration migration validation, and the Playwright UI gate (`ui-regression.yml`).
- Widen validation for shared contracts, models, authentication, project files, DI, migrations, dependencies, or workflows.
- Documentation-only changes use lightweight repository validation.
- Standing owner exception: skip Azure SQL and all integration-type tests requiring external services, credentials, hosted infrastructure, containers, devices, signing/store access, or cross-system integration. Record skipped tests.
- Add focused non-integration tests for every behavioral change. A slice that replaces a surface retires or rewrites the legacy specs it obsoletes in the same PR. Local checks supplement but never replace Actions.

## UI Completeness

- Before changing a page or screen, load the project-local Impeccable skill at `.agents/skills/impeccable/SKILL.md` and follow its routing and bounded verification rules. Run a critique/audit pass against the approved design authority before a slice closes.
- Record goals, hierarchy/navigation, CRUD actions, loading/empty/error/success/permission states, validation, destructive-action safety, feedback, accessibility/responsiveness, and required API/data/auth/entitlement support.
- Resolve required gaps in scope or record an approved exclusion/follow-up. Do not ship necessary actions or states as silent omissions.

## Review and Merge Gate

- Every PR gets an independent review — never by its author (Track 1 lesson, issue #659). Review the full diff, acceptance criteria, architecture/security impact, tests, exact-head Actions, artifacts, secrets, debug code, unrelated changes, branch drift, and documentation accuracy.
- Allowed decisions are `APPROVE`, `REQUEST_CHANGES`, or `COMMENT`. New commits invalidate prior approval. Never merge with incomplete/failing required checks or unresolved material comments.
- If GitHub blocks self-approval, record the review decision, reviewed SHA, validation status, and residual risks in a top-level PR comment.

## Completion and Handoff

At slice completion, synchronize: the slice issue, `PROJECT_STATUS.md`, `tracker/assignments.json`, `ai/handoffs/current.md`, the build's records under `docs/builds/<build>/`, and affected architecture, API, database, operational, or CI documentation. The handoff names one exact next action.

## Code and Repository Quality

- Follow repository formatting and analyzer configuration; preserve nullable and implicit-using conventions.
- Use async I/O with `CancellationToken`, validate configuration at startup, and document new dependencies.
- Do not overwrite unrelated user changes.
