# RWP-00.04 — Deployment Component Versioning and Release Manifest

## Outcome

Vennusign has one product release version that identifies an approved, immutable combination of independently versioned deployment components. Build, deployment, runtime, and future Platform Operations workflows can determine exactly what changed, what carried forward, and which contracts are compatible.

## Scope

| Deployment part | Required version model |
| --- | --- |
| Product release | Semantic version for the approved component combination |
| Back Office | Independent semantic version |
| Platform Operations | Independent semantic version |
| API | Independent semantic implementation version plus API contract major |
| Background services | Independent semantic version per deployable service |
| Hosted display/player SPA | Independent semantic version and fixed versioned deployment identity |
| Android/Fire TV shell | Semantic app version plus monotonically increasing store build number |
| Samsung Tizen shell | Semantic app version plus monotonically increasing platform package/build number |
| LG webOS shell | Semantic app version plus monotonically increasing platform package/build number |
| Native bridge | Explicit major/minor contract version and compatibility range |
| Database schema | Monotonically increasing ordered migration version |
| Stored procedures | New callable suffix/version only for incompatible contract changes |
| Infrastructure | Immutable infrastructure-as-code artifact version |
| Configuration | Configuration-schema version separate from environment values |

## Required implementation

- Establish a canonical machine-readable release manifest and schema.
- Record product version, component versions, commit SHA, pipeline/build identity, compatibility declarations, and whether each component changed or carried forward.
- Validate semantic versions, ordered schema versions, platform build numbers, contract versions, compatibility ranges, and required component coverage.
- Make builds immutable and promote the exact staging-tested artifacts to production; environment-specific values remain configuration.
- Prevent an unchanged component from receiving a new version or being rebuilt/redeployed solely because the product version changed.
- Publish safe product/component/contract version metadata through appropriate health or operational surfaces.
- Preserve expand-and-contract database compatibility while any older supported product version remains deployed.
- Create a new stored-procedure contract version when parameters, result shape, meanings, or behavior become incompatible.
- Track TV shell and hosted-player versions separately and validate their native-bridge compatibility.
- Document how future Platform Operations deployment workflows consume the manifest.

## Acceptance criteria

- Every current independently deployable Vennusign component is represented by the canonical version source and release manifest.
- CI validates a proposed manifest and identifies changed versus carried-forward components.
- Changed artifacts include immutable commit and build identity.
- Missing, malformed, reused, contradictory, or incompatible declarations fail validation with actionable output.
- API, native bridge, database schema, stored-procedure, infrastructure, and configuration compatibility are explicit and machine-validated.
- Runtime/health metadata exposes applicable product, component, shell, hosted-player, and contract versions without secrets.
- Focused non-integration tests cover valid manifests, invalid manifests, compatibility ranges, and version progression.
- Existing builds and local development remain operational.
- Controlled status, tracker, handoff, and durable deployment/versioning documentation are synchronized.

## Boundaries

- This RWP implements the version foundation and release manifest.
- It does not implement customer maintenance schedules, deployment waves, environment provisioning, production cutover, rollback orchestration, or environment decommissioning.
- It does not resume Phase 14 or later.
- Azure SQL, external services, credentials, hosted infrastructure, containers, physical devices, signing/store access, cross-system workflows, and all other integration-type tests remain skipped under the standing owner instruction.

## Dependencies and queue

- Issue: #437
- Mode: Sequential
- Depends on the current component/project structure and completed RWP-10.01 player runtime work.
- Queue position: next approved product item.

## Implementation

- Status: implemented; pending exact-head CI and merge.
- Canonical template and JSON schema: `docs/operations/release/`.
- Manifest validation/materialization: `src/display/scripts/releaseManifest.mjs`.
- Safe API runtime metadata: `GET /health/version`.
- Focused coverage: release-manifest validation/progression tests and API metadata unit test.
- Integration-type validation remains intentionally skipped.
