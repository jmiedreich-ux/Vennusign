# Vennu Development Instructions

## Startup and Source of Truth

Before changing the repository, read only:

1. `AGENTS.md`
2. `ai/handoffs/current.md`
3. `tracker/assignments.json`
4. `PROJECT_STATUS.md`
5. The claimed file under `docs/work-packages/`, when one exists
6. Linked issue, branch, PR, comments, and exact-head CI results

Read `AI_DEVELOPMENT_GUIDE.md`, component README files, architecture, or operations documents only when the task touches that area. Content under `docs/archive/` and `ai/handoffs/archive/` is research-only: do not load it routinely. Repository and GitHub state override chat history and archived material.

## Architecture

- Target `.NET 9`.
- Keep `Vennu.DataAccess` generic; Vennu persistence belongs in `Vennu.Data` and shared domain models in `Vennu.Core.Models`.
- Keep HTTP, SignalR, API composition, and hosted services in `Vennu.Api` unless an established boundary requires otherwise.
- Keep `src/display` independent and never add it as a Visual Studio Website project.
- Apply schema changes only through ordered DbUp migrations.
- Inspect existing contracts before adding routes, columns, events, payloads, or abstractions.

## Issue, Claim, and Package Workflow

- Every change starts from one approved GitHub issue, explicit execution mode, claim in `tracker/assignments.json`, branch, and PR.
- Implementation work maps to one approved WP/RWP. Documentation or bounded maintenance may use its approved issue identifier.
- Check issues, branches, PRs, tracker, status, and handoff before claiming. Stop on ownership conflict.
- Sequential mode owns one queue item until it is merged, closed, and released; never skip ahead.
- Collaborative mode uses one orchestrator-owned claim. Lanes require declared writable/read-only/prohibited paths and may not claim other roadmap work.
- Branches use `wp/<id>-<short-name>`, `rwp/<id>-<short-name>`, or `issue/<number>-<short-name>` as applicable.
- Keep changes bounded; do not refactor unrelated code or begin dependent/future work.
- Delete completed branches unless retention is documented.

## Shared-File and Multi-Agent Safety

- No two agents may modify the same file concurrently.
- Contracts, project files, dependency injection, migrations, package configuration, shared fixtures, workflows, tracker, status, and handoff are orchestrator-owned.
- Contract-dependent lanes start only after the orchestrator freezes the contract.
- Unexpected overlap requires stopping and re-planning.

## Gap and Remediation Governance

- Record discoveries as issues first; testing/review agents do not alter the roadmap.
- Small in-scope defects may remain in the active package only when explicitly linked and approved.
- Substantial or historical gaps require planning-agent promotion to `RWP-<origin-phase>.<sequence>` with evidence, acceptance criteria, dependencies, queue position, and migration/reconciliation impact.
- Every RWP uses one issue, claim, branch, PR, validation record, review, and merge.

## Testing and CI

- GitHub Actions is authoritative; required checks must pass on the exact reviewed PR head.
- Normal WP/RWP work runs only affected Release builds, focused unit tests, static checks, and applicable non-integration migration validation.
- Widen validation for shared contracts, models, authentication, project files, DI, migrations, dependencies, or workflows.
- Do not run unrelated applications, TV packages, or the complete unit suite for ordinary work.
- Full non-integration validation is reserved for phase closure, nightly/manual validation, workflow changes, or an explicit `full-validation` label.
- Documentation-only changes use lightweight repository validation.
- Standing owner exception: skip Azure SQL and all integration-type tests requiring external services, credentials, hosted infrastructure, containers, devices, signing/store access, or cross-system integration. Record skipped tests.
- Add focused non-integration tests for every behavioral change. Local checks supplement but never replace Actions.

## UI Completeness

- Before changing a page or screen, consult the available UX best-practices capability.
- Record goals, hierarchy/navigation, CRUD actions, loading/empty/error/success/permission states, validation, destructive-action safety, feedback, accessibility/responsiveness, and required API/data/auth/entitlement support.
- Resolve required gaps in scope or document an approved exclusion/follow-up. Do not ship necessary actions or states as silent omissions.

## Review and Merge Gate

- ChatGPT reviews every PR, including documentation and emergency changes.
- Review metadata, full diff, changed files, acceptance criteria, architecture/security impact, tests, exact-head Actions, comments, artifacts, secrets, debug code, unrelated changes, branch drift, and documentation accuracy.
- Allowed decisions are `APPROVE`, `REQUEST_CHANGES`, or `COMMENT`.
- New commits invalidate prior approval. Never merge with incomplete/failing required checks or unresolved material comments.
- If GitHub blocks self-approval, record `CHATGPT APPROVED`, reviewed SHA, validation status, and residual risks in a top-level PR comment.

## Completion and Handoff

Before merge, synchronize:

- active package or issue record
- `PROJECT_STATUS.md`
- `tracker/assignments.json`
- `ai/handoffs/current.md`
- a dated immutable handoff in `ai/handoffs/archive/`
- affected architecture, API, database, operational, or CI documentation

The proposed merge state must release the claim and name one exact next action. Completion evidence belongs in the implementation PR when practical; unavoidable evidence-only PRs remain documentation-only.

## Code and Repository Quality

- Follow repository formatting and analyzer configuration; preserve nullable and implicit-using conventions.
- Use async I/O with `CancellationToken`, validate configuration at startup, and document new dependencies.
- Never commit secrets, tokens, connection strings, generated output, runtime logs, or machine-specific configuration.
- Do not overwrite unrelated user changes.
