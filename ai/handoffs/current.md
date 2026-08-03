# Vennusign Session Handoff

## Current State

- Item: RWP-00.04 — Deployment Component Versioning and Release Manifest / issue #437
- Mode: Sequential
- Branch: not yet claimed
- Status: Approved and first of five queued RWPs

## Approved Queue

1. RWP-00.04 / #437 — deployment component versioning and release manifest
2. RWP-05.07 / #439 — atomic screen replacement and pairing recovery
3. RWP-08.02 / #440 — daylight-saving-safe scheduling resolution
4. RWP-10.02 / #441 — durable player content receipts and delivery reconciliation
5. RWP-00.05 / #442 — affected-screen action completeness and recovery

Each package must be claimed, implemented, validated, merged, and released before the next package is claimed. The scheduled run may complete up to five packages.

## RWP-00.04 Approved Outcome

- One semantic product release version identifies an approved immutable combination of independently versioned components.
- Back Office, Platform Operations, API, deployable services, hosted display SPA, TV shells, native bridge, database schema/procedure contracts, infrastructure, and configuration schema retain the version models defined by the active package.
- A machine-readable release manifest records component versions, changed/carried-forward state, artifact commit/build identity, and compatibility declarations.
- Database evolution remains expand-and-contract compatible while older application versions are supported; incompatible stored-procedure callable contracts receive new versions.
- Shell and hosted-player versions remain separate and declare native-bridge compatibility.

## Boundaries

- Do not broaden RWP-00.04 into the full deployment control plane.
- Do not merge the scopes of the later remediation packages into RWP-00.04.
- Do not resume Phase 14+.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

The Sequential agent may claim RWP-00.04 / issue #437, create `rwp/00.04-deployment-component-versioning`, and implement `docs/work-packages/RWP-00.04-deployment-component-versioning.md`. After merge and claim release, continue in the recorded queue order.
