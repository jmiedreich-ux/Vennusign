# M1-A.1 — Core Fixture Contract

**Status:** Proposed implementation packet
**Worker:** ChatGPT CLI — Sol
**Reviewer:** Claude Opus
**Base:** `<accepted-base-sha>` from reviewed M1-A.0
**Planned worktree:** `m1a-1-core-contract`
**Estimate:** 100–160 code-bearing lines including tests

## One outcome

Create the smallest internal Core fixture contract that distinguishes stable scope, a frozen Published Presentation, and desired Screen assignment. It does not know about Player Outputs, Runtime Packages, Live state, or Showing.

## Exact writable paths

- `src/Vennu.Api/Core/M1A/CoreFixtureContracts.cs`
- `tests/Vennu.Api.Tests/ContentPlatform/M1A/CoreFixtureContractTests.cs`

No other file may change.

## Required contract

- `M1AScope` carries non-empty organization, venue, and logical Screen IDs.
- `M1APublishedPresentation` carries a non-empty presentation ID, content revision, exact `menu.v1` identity, Theme version, renderer compatibility identity, and immutable asset identities.
- `M1ADesiredAssignment` associates one `M1AScope` with one Published Presentation identity.
- Construction copies caller-supplied asset data so later caller mutation cannot change the Published Presentation.
- Every type is `internal`, carries an obvious `M1A` fixture name, and includes a source comment stating the deletion condition: remove when the accepted minimum `menu.v1` compiler and Default Theme binding path replace it, before Mosaic acceptance.

Use ordinary immutable C# values. Do not create interfaces, dependency injection, serialization attributes, transport DTOs, stores, clocks, services, or extension points.

## Observable assertions

1. A complete scope, frozen presentation, and desired assignment can be created and retain their exact identities.
2. Empty or whitespace identity values refuse construction with a deterministic argument exception.
3. Mutating the caller's original asset collection after construction does not alter the Published Presentation.
4. Desired assignment refers to a logical Screen and contains no Player Output or actual-Showing fact.

## Non-goals and hard stops

No Runtime or Platform source; no guide files; no `Program.cs`; no project/configuration edit; no endpoint, persistence, migration, customer data, Theme design, Menu model, or legacy code change. If the required contract cannot compile within the two files, stop and report the missing prerequisite.

## Required gates

```bash
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A.CoreFixtureContractTests
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release --no-restore
git diff --check
git diff --name-only <accepted-base-sha>...HEAD
```

The final command must list exactly the two writable paths. Create one scoped commit; do not push or merge.

## Required completion report

```text
M1-A.1 RESULT: PASS | BLOCKED
Base SHA: <sha>
Commit SHA: <sha or NONE>
Changed files:
- <exact path>
Code-bearing lines: <number>
Assertions:
1. PASS | FAIL | UNTESTED — <evidence>
2. PASS | FAIL | UNTESTED — <evidence>
3. PASS | FAIL | UNTESTED — <evidence>
4. PASS | FAIL | UNTESTED — <evidence>
Commands:
- <command> — PASS | FAIL — <unedited summary>
Scope check: PASS | FAIL
Blocker: NONE | <exact blocker>
```

One targeted correction is allowed only for defects named by Claude Opus. The correction may edit only the two locked files. A second defect, unchanged return, or boundary expansion stops the packet.
