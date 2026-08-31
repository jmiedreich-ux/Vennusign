# M1-A.4 — Read-Only Platform Support View

**Status:** Approved implementation packet; waiting for accepted M1-A.3
**Worker:** Local Qwen
**Reviewer:** Claude Sonnet
**Base:** `<accepted-base-sha>` from accepted M1-A.3
**Planned worktree:** `m1a-4-platform-view`
**Estimate:** 80–130 code-bearing lines including tests

## One outcome

Compose a read-only support view from accepted Core and Runtime facts without creating an independently stored or writable copy of truth or a command path.

## Exact writable paths

- `src/Vennu.Api/Platform/M1A/SupportViewFixture.cs`
- `tests/Vennu.Api.Tests/ContentPlatform/M1A/SupportViewFixtureTests.cs`

No other file may change. Accepted Core and Runtime files may be read but not edited.

## Required contract

- `M1ASupportViewComposer.Compose(...)` accepts Core scope, desired assignment, Runtime Package, Live overlay, and Showing evidence.
- The result exposes desired Published Presentation separately from actual Package/Showing facts.
- Composition refuses any organization, venue, logical Screen, Package, or Player Output identity mismatch.
- The result is an immutable projection only. The Platform fixture exposes no write, update, store, retry, or device-command method.
- The fixture deletion condition is present in source.

## Observable assertions

1. Matching facts produce one view with desired and actual identities visibly separate.
2. Each named scope/identity mismatch refuses composition.
3. The Live value is visible without being copied into the Published Presentation.
4. Public reflection over the fixture surface finds no mutation, persistence, or command method.

## Non-goals and hard stops

No support endpoint or UI, authorization, persistence, new package, DI, `Program.cs`, project/configuration change, or edits to accepted dependencies. If an accepted contract is insufficient, stop and report it.

## Required gates

```bash
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A.SupportViewFixtureTests
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release --no-restore
git diff --check
git diff --name-only <accepted-base-sha>...HEAD
```

The final command must list exactly the two writable paths. Create one scoped commit; do not push or merge.

## Required completion report

Use the M1-A.1 report format, headed `M1-A.4 RESULT`, with four numbered assertion results, exact command results, code-bearing line count, scope check, and blocker. One targeted correction is allowed only for defects named by Claude Sonnet and only in the two locked files.
