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

## Controlled local-model qualification

The finite local-model qualification at `docs/research/local-ai-model-qualification/` evaluates four local models under fixed fixtures and configuration. Maestro routes only bounded work to models that meet the measured role thresholds in that report; cloud models remain responsible for work outside those thresholds. The qualification report maintains the full evidence and final routing table.

## Cloud-to-local task dispatch plan

The recommended control plane is GitHub. Cloud Maestro creates and reviews bounded assignments; the Linux AI box makes outbound GitHub requests to claim and execute them. Normal dispatch does not require an inbound endpoint, Remote Desktop, SSH, or Tailscale. Tailscale remains an owner maintenance path only.

### Decision authority

- The owner approves milestones, acceptance, merge, and deployment.
- Cloud Maestro owns interpretation, decomposition, architecture, risk classification, acceptance criteria, model-routing class, and final review.
- Local models implement bounded repository work but do not approve their own work, change architecture, merge, deploy, or contact the owner directly for product decisions.
- Ambiguous, security-sensitive, production, identity, database-strategy, cross-application, or credential-bearing work remains cloud-only unless Cloud Maestro first reduces it to an explicitly approved implementation packet.

### Measured routing configuration

Cloud assignments name an execution class rather than a hard-coded model so the model mapping can change without changing the protocol.

| Execution class | Current route | Intended work |
| --- | --- | --- |
| `fast` | `gpt-oss:20b` | Mechanical edits, documentation, scaffolding, focused tests, and straightforward fixes |
| `developer` | `qwen3-coder:30b` | Multi-file implementation, difficult defects, concurrency, idempotency, and substantial bounded feature slices |
| `fallback` | `qwen3.5:9b-q4_K_M` | Lightweight work when the normal fast model is unavailable or unnecessary |
| `cloud-only` | Cloud model | Planning, architecture, security/identity judgment, unresolved ambiguity, final review, and owner-facing decisions |

`devstral:24b` is not routed. Qualification showed Qwen Coder passing all 14 hidden coding checks, GPT-OSS providing the fastest high-scoring coding run, and no local model establishing sufficient final-review evidence. Only one local inference job runs at a time until capacity evidence supports concurrency.

### Assignment eligibility

A local packet must be repository-contained, reversible through Git, limited to named paths, based on settled requirements, verifiable with explicit commands, and free of production credentials and customer data. Cloud Maestro may delegate implementation of a high-risk design only after retaining the design decision and expressing the remaining work as a bounded packet.

### GitHub job contract

Each assignment is a GitHub Issue created from a fixed template and pinned to an exact base commit. The issue contains:

- Job ID, repository, base branch, base commit, execution class, risk, and priority.
- Goal, business context, requirements, explicit non-goals, and known risks.
- Allowed and forbidden paths.
- Exact acceptance commands and expected evidence.
- Time budget, maximum revision cycles, network and production-access policy, and merge authorization.
- Cloud-review expectations and Definition of Done.

The local worker snapshots and hashes the assignment before execution. Recommended labels are `maestro:ready`, `maestro:claimed`, `maestro:running`, `maestro:testing`, `maestro:review`, `maestro:revision`, `maestro:blocked`, `maestro:complete`, and `maestro:cancelled`, plus routing and risk labels.

Normal state flow:

`READY -> CLAIMED -> RUNNING -> TESTING -> REVIEW -> COMPLETE`

Failure paths return to `BLOCKED`, `REVISION`, `CLOUD-ONLY`, or owner escalation without silently widening scope.

### Local worker behavior

A small `maestro-worker` service on the AI box starts automatically after reboot and polls GitHub approximately every 20–30 seconds. It processes one job at a time:

1. Atomically claim one ready issue and record the worker identity and lease.
2. Verify the pinned base commit and create a clean isolated worktree.
3. Map the execution class to the configured Ollama model.
4. Launch OpenCode with the immutable assignment and repository instructions.
5. Enforce time, path, network, and resource limits while recording model, timing, process, and GPU evidence.
6. Run the exact acceptance commands without weakening or rewriting tests.
7. Push a predictable branch such as `local-agent/<job-id>/<slug>`.
8. Open a draft PR linked to the issue and move the issue to cloud review.
9. Wait for `APPROVE`, `REQUEST_LOCAL_REVISION`, `TAKE_OVER_IN_CLOUD`, or `RETURN_TO_OWNER`.

The worker never commits to `master`, force-pushes, merges, deploys, guesses through merge conflicts, reuses a dirty worktree, or includes unrelated changes.

### Evidence and repository records

Product changes stay on the task branch. Durable execution evidence is written to an append-only `maestro-runs` branch under `runs/<job-id>/`, including the assignment snapshot and hash, model tag and digest, base and result commits, timing, exit status, tests, changed files, generated diff, GPU samples, retry and revision counts, and final disposition. PR and issue comments link to that record. Logs are secret-scanned before push.

### Cloud review and loop limits

Cloud review verifies requirements, authorized scope, test integrity, architecture, security and identity boundaries, error handling, concurrency/idempotency where relevant, migrations, and repository conventions. The local worker receives at most one cloud-requested revision cycle. A second failure moves the work to the cloud or owner; it does not create an endless local repair loop.

### Failure handling

- Infrastructure failures are preserved and may receive a bounded clean retry.
- Failed tests or misunderstood requirements are model outcomes, not infrastructure retries.
- `fast` work may use the qualified fallback model when the normal model cannot load; `developer` work does not silently downgrade.
- Timeouts stop safely, preserve evidence, and block the job.
- Stale bases or conflicts return to Cloud Maestro for replanning.
- An offline worker leaves ready work queued; Cloud Maestro may wait, execute it in cloud, or reassign it.
- Ambiguity returns to the cloud instead of being guessed locally.

### Security boundary

The service runs as a dedicated non-root user with a fine-grained GitHub credential restricted to approved repositories, issues, branches, pull requests, and run records. It has no production Azure credentials, customer data, automatic deployment permission, repository-administration permission, or `sudo`. No inbound internet port is required. A dedicated GitHub App should replace a personal token when the control loop is proven.

### Operational health

The worker publishes a small heartbeat containing worker ID, idle/busy state, current job ID, Ollama/GPU health, and last-seen time without publishing private prompt contents. Local SQLite state supports leases and crash recovery.

### Implementation sequence

1. **Run-once proof** — Process one prepared GitHub job end to end: claim, route, worktree, model execution, tests, branch, draft PR, and evidence.
2. **Automatic worker** — Add polling, leases, local state, crash recovery, time limits, heartbeat, and `systemd` startup.
3. **Cloud integration** — Standardize assignment, risk, review, revision, and escalation prompts.
4. **Hardening** — Add GitHub App authentication, stronger sandbox/resource limits, secret detection, stale-job recovery, and multiple-repository support.

GitHub Issues and PRs are the initial operating interface; a separate dashboard is not required.

### Operational Definition of Done

The first operational version is complete when a cloud agent can publish a bounded GitHub assignment; the AI box receives it without manual prompt or file transfer; the correct model executes in an isolated branch; evidence and tests are retained; a draft PR is created; cloud review can request at most one revision; nothing merges or deploys without authorization; and the worker recovers after reboot.

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
