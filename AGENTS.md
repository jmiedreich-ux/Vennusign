# Vennusign Development Instructions

## Startup and Source of Truth

Before changing the repository, read only:

1. `AGENTS.md`
2. `ai/handoffs/current.md`
3. `tracker/assignments.json`
4. `PROJECT_STATUS.md`
5. The active feature's records under `docs/features/<feature>/`, when a feature is active
6. Linked issue, branch, PR, comments, and exact-head CI results

Read `AI_DEVELOPMENT_GUIDE.md`, component README files, architecture, or operations documents only when the task touches that area. Content under `docs/archive/`, `ai/handoffs/archive/`, and `track0/` is research-only: do not load it routinely. Repository and GitHub state override chat history and archived material.

## Working Model — Features and Milestones

Adopted 2026-08-07 from the Track 1 retrospective; replaces the earlier phase/track/WP model (and the interim build/slice naming). Historical phase/track/RWP records remain valid as history only.

- The unit of work is a **feature**, named by product area (e.g. Menus). A feature is delivered in numbered **milestones** — small functional vertical pieces that each ship whole.
- **Design before implementation.** A feature's UI work starts only after its design authority is approved and landed in `docs/design/approved/<feature>/`. Where any other document disagrees with that bundle's `decisions.md`, the decisions win. Open design questions are resolved through the feature's question register in `docs/features/<feature>/` before or alongside the affected milestone — never silently.
- Every milestone ships **schema → API → UI → Playwright specs together**; tests are written with the implementation, never after. Each milestone is independently mergeable and leaves `master` releasable.
- Milestone execution follows the GitHub-first discipline: create the milestone issue, record the claim, branch as `feature/<area>-m<n>-<short-name>`, open one PR, pass exact-head CI, obtain independent review, merge, then synchronize records. One milestone at a time; a successor starts only after its predecessor is merged and its owner workbook is accepted.
- **Every milestone ends with a short owner acceptance workbook (5–10 minutes)** before the next milestone starts; a schema-only milestone gets a demo script instead. Hosted-agent subjective QA (the Track 1 pattern) runs on demand when a milestone carries judgment cases deterministic specs cannot assert.
- Keep changes bounded; do not refactor unrelated code or begin future-milestone work. Delete completed branches after merge.

## How to Work a Task

These govern every task, not only milestone work.

- **Fix the behaviour, not the example.** Find everywhere a behaviour exists and fix it everywhere it exists — not only the place the request names. A request that names one location is describing a symptom; the scope is the behaviour.
- **State the whole behaviour before coding.** Say what complete user behaviour this task is part of, what happens immediately before and after it, and where else that same behaviour lives. Then implement the whole known pattern, not the named example.
- **Answer "where else does this apply" with a search, not from memory.** Run an actual search of the codebase and paste the command and its full results into the report. Every location in those results that was not changed is named, with the reason.
- **Evidence is a command someone else can rerun, and its output** — a test run, a request and its response, a case that failed and then passed. "Verified working" is not evidence. Anything not actually executed is marked **UNTESTED**; that is an acceptable answer, a false "done" is not.
- **Read the handoff first, append to it last.** Before starting, read `ai/handoffs/current.md` and honour what earlier tasks established. Before stopping, append what was established, what was assumed, what was deliberately left for later and for whom, and any open questions.

## Architecture

- Target `.NET 9`.
- Keep `Vennu.DataAccess` generic; Vennusign persistence belongs in `Vennu.Data` and shared domain models in `Vennu.Core.Models`.
- Keep HTTP, SignalR, API composition, and hosted services in `Vennu.Api` unless an established boundary requires otherwise.
- Keep `src/display` independent and never add it as a Visual Studio Website project.
- Apply schema changes only through ordered DbUp migrations; a migration that discards data names what it discards.
- `src/Vennu.Data/Scripts/001_baseline.sql` is the collapsed history of the first fifty-nine migrations and is never edited. New migrations start at 059.
- Deleting a migration does not un-apply it: DbUp decides by journal name, so removing one changes only what a *fresh* database gets. Anything already released is removed by a **new** migration, so existing and new databases converge.
- Inspect existing contracts before adding routes, columns, events, payloads, or abstractions.

## Documentation Control

- Treat Markdown as a maintained interface, not a work log. Update an existing authoritative document before creating a new `.md` file.
- The controlled living records are `AGENTS.md`, `PROJECT_STATUS.md`, `ai/handoffs/current.md`, the tracker, the active feature's records under `docs/features/<feature>/`, and affected durable architecture/operations documents.
- Batch living-record updates at milestone completion. Do not edit tracker, status, or handoff after every local commit.
- A change that makes a controlled record **false** updates that record in the same commit. A record that is behind is tolerable; one that states something untrue sends the next session down the wrong path.
- A new Markdown file requires a durable audience and purpose not served by an existing file. Do not create per-experiment, per-prompt, or evidence-only Markdown; completion evidence belongs in the milestone PR and issue.
- Keep historical material under `docs/archive/` or `ai/handoffs/archive/` and read it only for deliberate research.
- Never commit secrets, tokens, connection strings, generated output, runtime logs, or machine-specific configuration.

## Shared-File and Multi-Agent Safety

- No two agents may modify the same file concurrently.
- Contracts, project files, dependency injection, migrations, package configuration, shared fixtures, workflows, tracker, status, and handoff are orchestrator-owned.
- Check the tracker and open claims before starting; stop on ownership conflict and re-plan.

## Discoveries and Backlog

- Record discoveries as GitHub issues first. Owner-approved out-of-scope decisions become backlog issues at the moment of decision.
- Small in-scope defects may be fixed inside the active milestone when explicitly linked; anything larger becomes its own issue and waits for scheduling.

## Testing and CI

- GitHub Actions is authoritative; required checks must pass on the exact reviewed PR head.
- Normal milestone work runs affected Release builds, focused unit tests, static checks, applicable non-integration migration validation, and the Playwright UI gate (`ui-regression.yml`).
- Widen validation for shared contracts, models, authentication, project files, DI, migrations, dependencies, or workflows.
- Documentation-only changes use lightweight repository validation.
- Standing owner exception: skip Azure SQL and all integration-type tests requiring external services, credentials, hosted infrastructure, containers, devices, signing/store access, or cross-system integration. Record skipped tests.
- Add focused non-integration tests for every behavioral change. A milestone that replaces a surface retires or rewrites the legacy specs it obsoletes in the same PR. Local checks supplement but never replace Actions.

### Where a test lives, and what makes it worth having

Adopted 2026-08-09 from the Menus Milestone 1 retrospective, after five consecutive independent reviews each found real defects in work that had just been declared finished.

- **Test a rule where it is enforced.** A refusal enforced in SQL is asserted against a database. An in-memory double stores state and may be *told* to fail; it never decides. A double that re-implements a rule proves the copy, and the copy drifts — that is how a defect survived 412 green unit tests.
- **Unit tests cover what has no database in it**: pure functions, contract mapping, retry loops, refusal wording.
- **LocalDB is the default everywhere**, locally and in CI. Azure is reached only by setting `VENU_TEST_AZURE_SQL_CONNECTION_STRING` for that run — a deliberate act that ends when the run does. Never a file on disk, which is "always, until somebody remembers to delete it".
- **A suite that cannot reach its database fails.** It never reports a pass having asserted nothing.
- **Model invariants run after every integration test**, automatically, against whatever state the test left behind. When a defect turns out to be "the model was in a state it says cannot exist", it becomes a new invariant rather than only a new test.
- **A regression test is verified to fail with its fix reverted** — revert, run, observe the failure, restore. Closing a finding without that check is not closing it.
- **Acceptance asserts what the customer sees**: what a screen shows after a sequence, not that an API accepted a request. A workbook can pass every check while the product is visibly broken, and has.
- **Two values that must describe the same instant are read once, under one lock.** Reading them separately is the most common defect shape this codebase produces.
- **When an invariant is introduced, list every write path that could violate it** and cover each. Finding the doors one at a time costs a review round each.

## UI Completeness

- Before changing a page or screen, load the project-local Impeccable skill at `.agents/skills/impeccable/SKILL.md` and follow its routing and bounded verification rules. Run a critique/audit pass against the approved design authority before a milestone closes.
- Record goals, hierarchy/navigation, CRUD actions, loading/empty/error/success/permission states, validation, destructive-action safety, feedback, accessibility/responsiveness, and required API/data/auth/entitlement support.
- Resolve required gaps in scope or record an approved exclusion/follow-up. Do not ship necessary actions or states as silent omissions.

## Review and Merge Gate

- Every PR gets an independent review — never by its author (Track 1 lesson, issue #659). Review the full diff, acceptance criteria, architecture/security impact, tests, exact-head Actions, artifacts, secrets, debug code, unrelated changes, branch drift, and documentation accuracy.
- Allowed decisions are `APPROVE`, `REQUEST_CHANGES`, or `COMMENT`. New commits invalidate prior approval. Never merge with incomplete/failing required checks or unresolved material comments.
- If GitHub blocks self-approval, record the review decision, reviewed SHA, validation status, and residual risks in a top-level PR comment.

## Completion and Handoff

At milestone completion, synchronize: the milestone issue, `PROJECT_STATUS.md`, `tracker/assignments.json`, `ai/handoffs/current.md`, the feature's records under `docs/features/<feature>/`, and affected architecture, API, database, operational, or CI documentation. The handoff names one exact next action.

## Code and Repository Quality

- Follow repository formatting and analyzer configuration; preserve nullable and implicit-using conventions.
- Use async I/O with `CancellationToken`, validate configuration at startup, and document new dependencies.
- Do not overwrite unrelated user changes.
