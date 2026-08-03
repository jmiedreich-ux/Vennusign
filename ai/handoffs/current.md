# Vennusign Session Handoff

## Current State

- Item: RWP-00.04 — Deployment Component Versioning and Release Manifest / issue #437
- Mode: Sequential
- Branch: not yet claimed
- Status: Approved and next in queue

## Approved outcome

- One semantic product release version identifies an approved immutable combination of independently versioned components.
- Back Office, Platform Operations, API, deployable services, hosted display SPA, TV shells, native bridge, database schema/procedure contracts, infrastructure, and configuration schema retain the version models defined by the active package.
- A machine-readable release manifest records component versions, changed/carried-forward state, artifact commit/build identity, and compatibility declarations.
- Database evolution remains expand-and-contract compatible while older application versions are supported; incompatible stored-procedure callable contracts receive new versions.
- Shell and hosted-player versions remain separate and declare native-bridge compatibility.

## Boundaries

- This package implements the version foundation and release manifest, not the broader deployment control plane.
- Do not implement customer maintenance schedules, rollout waves, environment provisioning/decommissioning, or full Platform Operations deployment orchestration.
- Do not resume Phase 14+.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

The Sequential agent may claim RWP-00.04 / issue #437, create `rwp/00.04-deployment-component-versioning`, and implement the active package in `docs/work-packages/RWP-00.04-deployment-component-versioning.md`.

## Do Not Redo

Do not reopen completed remediation, redesign player runtime behavior, or broaden this RWP into customer cutover orchestration.
