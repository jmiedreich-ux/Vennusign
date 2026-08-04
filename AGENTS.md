# Vennusign Development Instructions

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
- Keep `Vennu.DataAccess` generic; Vennusign persistence belongs in `Vennu.Data` and shared domain models in `Vennu.Core.Models`.
- Keep HTTP, SignalR, API composition, and hosted services in `Vennu.Api` unless an established boundary requires otherwise.
- Keep `src/display` independent and never add it as a Visual Studio Website project.
- Apply schema changes only through ordered DbUp migrations.
- Inspect existing contracts before adding routes, columns, events, payloads, or abstractions.

## Issue, Claim, and Package Workflow

- Every change starts from approved scope and one explicit execution mode. Sequential and Mobile Collaborative work require a GitHub issue, claim, branch, and PR before editing; Desktop Collaborative work uses the session lock and local branch model below.
- Implementation work maps to one approved WP/RWP. Documentation or bounded maintenance may use its approved issue identifier.
- Check issues, branches, PRs, tracker, status, and handoff before claiming. Stop on ownership conflict.
- Sequential mode owns one queue item until it is merged, closed, and released; never skip ahead.
- Mobile Collaborative mode preserves the GitHub-first orchestrator workflow: one remotely visible claim, bounded remote branch/PR, and declared writable/read-only/prohibited lanes. It may not claim unrelated roadmap work.
- Desktop Collaborative mode is local-first for an interactive Visual Studio or VS Code session. Before code changes, pause every sequential schedule and publish one visible desktop-session lock identifying the owner, scope, session integration branch, and start time. An active desktop lock blocks all Sequential claims.
- In Desktop Collaborative mode, pull the default branch once, create `collab/desktop/<topic>` as the local session integration branch, and use short-lived local logical branches beneath it. Merge reviewed logical branches locally into the session branch; a logical branch may resolve multiple coherently related issues.
- Do not repeat repository-wide GitHub, tracker, status, or handoff scans between local logical branches. Recheck remote ownership and default-branch drift at publish checkpoints and before final merge.
- At a meaningful Desktop Collaborative checkpoint, reconcile issue links once, update controlled records once, push the session branch, open one coherent PR, and run affected-area Actions once. Publishing a checkpoint does not end the desktop session.
- End Desktop Collaborative mode only on explicit owner direction: publish or preserve remaining work, release the desktop lock, then resume sequential schedules. Never resume them merely because a checkpoint PR merged.
- Published item branches use `wp/<id>-<short-name>`, `rwp/<id>-<short-name>`, or `issue/<number>-<short-name>` as applicable. Desktop session integration branches use `collab/desktop/<topic>`; their logical child branches remain local.
- Keep changes bounded; do not refactor unrelated code or begin dependent/future work.
- Delete completed branches unless retention is documented.

## Documentation Control

- Treat Markdown as a maintained interface, not a work log. Update an existing authoritative document before creating a new `.md` file.
- During Desktop Collaborative work, do not create Markdown per local branch, issue, experiment, prompt, test run, or intermediate handoff. Keep temporary reasoning in the session or issue discussion, not the repository.
- The controlled living records are `AGENTS.md`, `PROJECT_STATUS.md`, `ai/handoffs/current.md`, the tracker, the active approved package when one is required, and affected durable architecture/operations documents.
- Batch living-record updates at publish checkpoints. Do not edit tracker, status, or handoff after every local commit or logical merge.
- A new Markdown file requires a durable audience and purpose not served by an existing file. Work-package records are created only for approved WP/RWP work; architecture/decision/operations records are created only when the durable system contract or procedure truly needs a separate document.
- Archive snapshots are optional, not per-merge output. Create one only for phase closure, a major release or process transition, a durable decision needing audit history, or explicit owner request.
- Keep historical material under `docs/archive/` or `ai/handoffs/archive/` and read it only for deliberate research.

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

- Before changing a page or screen, load the project-local Impeccable skill at `.agents/skills/impeccable/SKILL.md` and follow its routing and bounded verification rules. Use `shape` before substantial new surfaces, `critique` for design review, and the applicable `audit`, `adapt`, `harden`, or `polish` pass before completion.
- The Impeccable hook in `.codex/hooks.json` is an advisory design detector for Codex edits. Address applicable findings or record why a finding is out of scope; it does not replace focused builds, tests, accessibility review, or the GitHub Actions merge gate.
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
- affected architecture, API, database, operational, or CI documentation

For Desktop Collaborative checkpoint PRs, synchronize these records once for the checkpoint and keep the desktop lock active. At final session completion, the proposed merge state must release the lock and name one exact next action. Other modes release their claim at item completion. Completion evidence belongs in the implementation PR; do not create an evidence-only Markdown file or PR when the existing PR, issue, status, or current handoff can hold it.

## Code and Repository Quality

- Follow repository formatting and analyzer configuration; preserve nullable and implicit-using conventions.
- Use async I/O with `CancellationToken`, validate configuration at startup, and document new dependencies.
- Never commit secrets, tokens, connection strings, generated output, runtime logs, or machine-specific configuration.
- Do not overwrite unrelated user changes.
