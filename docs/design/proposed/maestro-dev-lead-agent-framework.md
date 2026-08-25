# Maestro — Dev-Lead Agent Framework

**Status:** Proposed — not approved or scheduled. Adding this document does not authorize implementation.

## Purpose

Maestro is a dev-lead agent that executes an owner-approved Atlas milestone through the existing Vennue development process. It is not a replacement for `AGENTS.md`; that document remains authoritative and wins if there is a conflict.

Maestro's job is to turn one approved milestone into one controlled, evidence-backed pull request. The owner remains the approval, acceptance, and merge point.

## Operating boundary

- One run handles one owner-approved, unclaimed milestone.
- One run produces one draft PR on the normal feature branch.
- Maestro never starts the next milestone by itself.
- Tests prove behavior against real running targets; tests are not created merely to inflate coverage or recreate implementation logic with doubles.
- Any work outside an assigned ownership boundary is rejected and returned to the worker.

## Run lifecycle

1. **Kickoff** — Poll Atlas/GitHub for an owner-approved, unclaimed milestone. No inbound endpoint is needed.
2. **Orient** — Read only the normal startup context: `AGENTS.md`, current handoff, assignments, project status, active feature records, and linked GitHub state.
3. **Claim** — Claim the milestone, create the standard feature branch, and open one draft PR.
4. **Decompose** — Convert the approved plan into bounded work packets. Each packet names one owner, the files it may change, acceptance criteria, and the commands that provide evidence.
5. **Dispatch** — Sequence work by the milestone's dependencies, normally schema → API → UI → specifications/Playwright. Each worker operates in an isolated worktree.
6. **Integrate** — Maestro owns shared orchestration files: contracts, projects, DI, migrations, package configuration, fixtures, workflows, tracker, status, and handoff.
7. **Verify** — Run the existing local gate and retain verbatim output as PR evidence.
8. **Review** — Use an independent reviewer, ideally from a different model vendor than the packet author. Review outcomes are `APPROVE`, `REQUEST_CHANGES`, or `COMMENT`.
9. **Done ledger** — Mark every Definition-of-Done item as `PASS`, `N/A (reason)`, or `UNTESTED`; do not silently omit a check.
10. **QA** — Run the existing development-environment QA script. Findings become GitHub issues rather than unreviewed fixes.
11. **Sync and stop** — Update the repository records, mark the PR ready, provide the owner acceptance workbook, and stop.

## Work packets and roles

Packets are area-focused, not general-purpose assignments. Proposed architecture areas are:

- Data and migrations
- API
- Back-office UI
- Platform-operations UI
- Display/player
- Specifications and Playwright
- Reviewer

A role describes the area's conventions, invariants, allowable files, and where tests belong. The model is selected separately per packet:

- Local models: mechanical work such as scaffolding and documentation sync.
- Mid-tier hosted/local CLI models: bounded single-area implementation.
- Stronger models or Maestro: contracts, migrations, cross-cutting work, and design judgment.
- Reviewer: independent of the packet author and preferably a different vendor.

This routing is configuration, not a hard-coded property of a role. It can be tuned later from real results.

## Verification target

The expected gate remains the existing process: Release build, focused tests, integration/model-invariant tests, Playwright, QA, and owner demo/acceptance.

The long-term Linux gate should use a disposable SQL Server 2022 Linux Docker container per run. This gives the gate a real, hermetic database with server-level permissions for all integration tests, avoids shared Azure-dev collisions, and can support the same shape in GitHub Actions later.

That work includes a container test target, routing the nine server-level tests through it, container-aware catalog safeguards, API connection-string environment overrides, and a Linux replacement for the Windows-shaped UI test environment script. These are test and tooling changes, not intended product behavior changes.

## Where it runs

The intended steady state is the Linux AI box:

- Maestro and gate runner
- Local models
- Disposable SQL database containers
- Build and Playwright/Chromium tooling

Maestro polls Atlas/GitHub, so no inbound service must be exposed. Tailscale is useful for remote human access but is not a dependency of the pipeline.

Because database containers, builds, Playwright, and inference share CPU, RAM, and GPU resources, a verification gate must be serialized against local-model work until actual capacity data supports safe parallelism.

## Delivery plan

### v1 — prove the control loop

- One hosted worker only
- One bounded milestone at a time
- One draft PR and completed done ledger
- Existing verification executed over SSH on the Windows dev box
- Owner acceptance and merge unchanged

v1 intentionally avoids work packets, local-model routing, and Linux gate implementation. The objective is a reliable, visible dev-lead loop—not a large automation project.

### v2 — controlled delegation

- Work packets and file ownership enforcement
- Area-focused role definitions
- Model-routing configuration
- Local models used only for suitable bounded tasks

### v3 — mature autonomous execution support

- Independent review with a defined owner escalation cap
- QA hook, process facts, and retrospective issues
- Disposable SQL Docker target and Linux-native verification gate

## Reporting and improvement

For each packet, record the role, selected model, cost/tokens, elapsed time, review rounds, rework count, gate result, and `UNTESTED` count. At run level, record QA escapes found after merge.

Start by collecting these facts, not by optimizing against elaborate metrics. Once there is enough history, watch first-pass review acceptance, cost per merged milestone, and local-model completion share. Any recommended process change is proposed as a GitHub issue; it does not silently rewrite `AGENTS.md`.

## Open decisions

1. Final name: Maestro, Foreman, or another choice.
2. Maximum review rounds before owner escalation.
3. Whether to build an Azure-dev-database bridge or proceed directly from v1's Windows SSH verification to the disposable container target.
4. Location and ownership of the model-routing configuration.
5. Resource policy for gate runs versus local-model dispatch on the AI box.

## Related repository material

- `AGENTS.md` — authoritative engineering process
- `AI_DEVELOPMENT_GUIDE.md` — architecture map and local toolchain notes
- `docs/features/atlas/` — milestone planning and approval surface
- `tests/Vennu.Data.IntegrationTests/Fixtures/TestDatabaseTarget.cs` — existing test-target resolver to extend for container use
