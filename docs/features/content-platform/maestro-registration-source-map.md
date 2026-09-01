# Maestro Registration Source Map — VennueSign

**Status:** Pre-registration project authority map; not a Maestro binding and not registration approval  
**Project:** VennueSign  
**Repository:** `jmiedreich-ux/Vennusign`  
**Default branch:** `master`  
**Approved graph revision:** `mosaic-v1.approved.1`  
**Graph approval commit:** `c246080f0ba97e5fd020c936e9ca580e39e8f532`  
**Approval-time source base:** `bd5e141ccaf02b8684c0db91b5d8c053e0bb95f9`

## Purpose and authority boundary

This is the VennueSign-side index for Maestro's later read-only registration discovery. It makes existing project authority easy to locate without copying that authority into Maestro or pre-creating `maestro.project.yaml`.

VennueSign remains authoritative for product architecture, work graphs, issues, pull requests, engineering policy, acceptance, and merge decisions. Maestro may later observe and project the approved graph into operational state after its own registration gates. This file creates no Maestro project record, queue, lease, task state, implementation permission, or dispatch authority.

Where this index disagrees with a linked authority, the linked authority wins. Repository and GitHub state override stale summaries.

## Registration inventory

| Required area | VennueSign authority | Registration fact |
|---|---|---|
| Identity | This file; repository Git metadata | Project `VennueSign`; repository `jmiedreich-ux/Vennusign`; default branch `master`. Adapter and Maestro process versions are intentionally unset until the reviewed binding step. |
| Project engineering policy | `AGENTS.md` | Governs source boundaries, features and milestones, testing, evidence, shared-file ownership, independent review, documentation control, and completion. A Maestro binding may strengthen but never weaken it. |
| Current handoff | `ai/handoffs/current.md` | Read first for the exact next action and current gates. Historical entries remain context, not current authority. |
| Current status | `PROJECT_STATUS.md` | Human-readable feature, milestone, CI, and approval state. |
| Operational claims | `tracker/assignments.json`; linked GitHub issues and PRs | The tracker is a project claim record, not the Mosaic graph and not a Maestro queue. Live repository/GitHub state wins when it is stale. Registration must not import unrelated Menus assignments as Mosaic packets. |
| Architecture authority | `docs/architecture/content-platform-architecture-renewal.md` §§11.10–11.11 | Approved Mosaic V1 boundary, five-wave graph, stable node IDs, typed dependencies, gates, locks, verification, non-goals, and later owner decisions. |
| Independent planning evidence | `docs/architecture/mosaic-v1-independent-blueprint-study.md` | Advisory evidence used by the approved renewal; it does not independently authorize work. |
| Active milestone boundary | `docs/features/content-platform/m1-a-api-module-foundation.md` | Approved M1-A outcome, ownership boundary, exclusions, acceptance proof, complete M0-D12 quality contract, routing, and review policy. |
| Executable packet graph | `docs/features/content-platform/m1-a/work-graph.md` | Ordered M1-A.0–M1-A.7 packets, serial/parallel order, typed prerequisites, owners, reviewers, writable domains, correction rules, and `G-M1A-CHECK`. |
| Complete worker packets | `docs/features/content-platform/m1-a/packets/` | Full prompts; they must not be shortened or silently rewritten at dispatch. Placeholders are resolved only as each accepted dependency supplies its exact SHA. |
| Approval and review evidence | `docs/features/content-platform/done-records/971.md`; PR #971 | Graph approval, full-review range, targeted correction coverage, owner approval, and residual UNTESTED areas. |
| Registration handoff evidence | `docs/features/content-platform/done-records/972.md`; PR #972 | Exact discovery-only handoff and correction review. |
| Task records | GitHub issue #970 and linked PRs | GitHub remains the actual task and delivery record. Stable graph nodes link to project task records; Maestro must not create a competing backlog. |
| Branch and merge policy | `AGENTS.md` §§Working Model, Review and Merge Gate | GitHub-first branch/issue/PR flow; independent review; exact reviewed-head coverage; owner acceptance; no automatic Maestro merge authority. |
| Verification policy | `AGENTS.md` §§Testing and CI; packet `Required gates` sections | Packet-specific commands are authoritative for M1-A. Broader local gates apply according to affected scope. Every unexecuted check is reported `UNTESTED`. |
| UI and subjective QA | `AGENTS.md` §§UI Completeness; active feature design authority when a UI node opens | The project-local Impeccable skill, approved design authority, Playwright, critique/audit, and owner workbook govern UI work. M1-A contains no UI. |
| Deployment | `.github/workflows/deploy-dev.yml`; `scripts/ci/confirm-deployment.sh`; affected operations authority | Dev deployment is GitHub/Azure-environment controlled and verifies source commit. Mosaic planning does not grant deployment authority. Stage/production and rollback must be discovered as missing or separately authoritative; do not infer them from dev. |
| Environments and secrets | `docs/architecture/tooling-secrets.md`; `AGENTS.md` §§Testing and CI; workflow secret-reference names only | LocalDB is the default test database. Deliberate Azure tests use `VENU_TEST_TARGET=azure` and Key Vault `kv-vennusign-dev`. No credential value belongs in project records or Maestro. |
| Roles and routing | M1-A work graph and packet headers | Routes name Local Qwen, ChatGPT CLI Sol/Terra, Claude independent reviewers, and Architect coordination. Registration observes eligibility; it does not broaden it. |
| Locks and shared domains | `AGENTS.md` §Shared-File and Multi-Agent Safety; approved graph node locks; packet writable paths | Contracts, project files, DI, migrations, shared fixtures/workflows, tracker, status, and handoff retain their declared ownership. Only M1-A.4 and M1-A.5 are approved to run in parallel. |
| Exceptions | `AGENTS.md`; approved graph and packet exclusions | CI is suspended by owner decision; Azure/external integration tests have a standing skip; packet-specific exclusions and hard stops remain controlling. One-off historical waivers are not reusable. |

## Graph ingestion boundary

The graph release available for discovery is `mosaic-v1.approved.1`. Discovery may verify that every node exposes its stable ID, rank or serial position, typed dependencies, change domain and locks, owner/route, output, checks, non-goals, gate state, authority reference, and source-base rule.

Only M1-A has complete worker packets. `G-V1-PLAN` is open. `G-MAESTRO-REG` is closed until Maestro completes and reviews discovery, the owner approves the later thin binding, and a no-dispatch dry run succeeds. `G-M1A-CHECK` remains closed until M1-A.7 records technical and process evidence and the architect chooses `PROCEED`.

Registration must preserve these meanings:

- approved or packet-complete does not mean dispatchable;
- a planned queue entry may be visible while blocked;
- GitHub issues and PRs remain VennueSign task records;
- a material graph change creates a reviewed superseding revision;
- Maestro never fills a missing product or architecture decision with an operational default.

## Known discovery findings

1. `tracker/assignments.json` is stale and contains unrelated Menus execution state from 2026-08-26. It is not a source for Mosaic graph nodes. Registration must report the staleness rather than projecting those entries.
2. CI is suspended by `AGENTS.md`, but workflow comments still describe PR checks as if they gate every merge. The project policy wins. Registration must record workflows as declared verification routes that are currently non-gating, not as proof that CI ran.
3. Dev deployment authority exists. A complete stage/production deployment and rollback authority is not identified by the approved Mosaic records and must be reported missing rather than inferred.
4. Adapter version, Maestro process version, binding schema, notification policy, and Maestro-side operational resource identifiers are deliberately absent. They belong to the later reviewed binding, not this source map.
5. `D-RUNTIME-01` and `D-RECOVERY-01` remain later owner decisions. They do not block registration or M1-A, but their dependent graph nodes remain blocked.

## Registration quality contract

| Field | Constraint |
|---|---|
| Protected outcome | Maestro can locate and faithfully project approved VennueSign authority without creating a second plan or converting stale records into work. |
| Operating / failure model | Read-only discovery against one exact default-branch SHA; records can be stale, duplicated, missing, or internally inconsistent, and linked GitHub state may be newer. |
| Explicit exclusions | No binding file, Maestro database write, project queue, lease, dispatch, implementation, issue mutation, PR mutation, environment change, secret read, or deployment. |
| Practical assurance level | Documentary and structural confidence sufficient to propose a later thin binding; no claim that Maestro execution or product behavior works. |
| Sufficient acceptance proof | Discovery names the exact VennueSign SHA and graph revision, resolves every inventory row to an existing authority or explicit missing fact, reports all conflicts, and proposes no operational default for missing authority. |
| Permitted boundary and complexity | Read repository/GitHub metadata and produce one bounded discovery report. Follow linked authority only as needed; do not perform a whole-codebase audit. |
| Proportionality ceiling | One registration inventory, one conflict/missing-fact list, and one later binding proposal after review. No source refactor merely to ease ingestion. |
| Stop / escalation rule | Stop when authority is contradictory, a required fact would be guessed, the exact source head differs materially from the approved graph base, or discovery would require a write or secret. Return the finding to VennueSign Architecture/Owner before binding. |

Passing that proof is enough for discovery. It does not open `G-MAESTRO-REG`; the later owner-reviewed binding and no-dispatch dry run are separate gates.

## Exact next action

Maestro performs read-only discovery against the then-current VennueSign `master`, recording the observed SHA and comparing it with graph revision `mosaic-v1.approved.1` and its approval base. Its deliverable is the discovery report only. Binding creation, operational projection, and M1-A.0 dispatch remain prohibited until the report is reviewed and the later gates are explicitly opened.
