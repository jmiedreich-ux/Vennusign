# M1-A.2 — Output-Specific Runtime Package

**Status:** Approved implementation packet; waiting for accepted M1-A.1
**Worker:** ChatGPT CLI — Terra
**Reviewer:** Claude Sonnet
**Base:** `<accepted-base-sha>` from accepted M1-A.1
**Planned worktree:** `m1a-2-runtime-package`
**Estimate:** 90–140 code-bearing lines including tests

## One outcome

Create a deterministic internal compiler that produces one Runtime Package for one exact Player Output from the accepted Core fixture values.

## Exact writable paths

- `src/Vennu.Api/Runtime/M1A/RuntimePackageFixture.cs`
- `tests/Vennu.Api.Tests/ContentPlatform/M1A/RuntimePackageFixtureTests.cs`

No other file may change. M1-A.1 files may be read but not edited.

## Required contract

- `M1APlayerOutputTarget` carries non-empty organization, venue, Player ID, and Player Output ID.
- `M1ARuntimePackage` carries a non-empty deterministic Package identity, the Published Presentation identity, renderer compatibility identity, and its exact Player Output target.
- `M1ARuntimePackageCompiler.Create(...)` accepts a non-empty Package identity, the Core desired assignment, Published Presentation, and requested target.
- Creation refuses when the supplied Published Presentation identity does not equal the desired assignment's Published Presentation identity.
- Creation refuses an organization or venue mismatch between Core scope and target.
- `M1ARuntimePackage.RequireTarget(...)` refuses every Player Output identity other than the package's exact target, including another output on the same Player.
- The Package contains no logical-Screen assignment, Live-state value, Showing claim, persistence behavior, or delivery behavior.
- The fixture deletion condition is present in source.

## Observable assertions

1. Matching desired assignment and scope produce a Package carrying the exact Package, Published Presentation, and Player Output identities.
2. Empty Package identity and a Published Presentation not named by the desired assignment refuse creation.
3. A different organization or venue refuses Package creation.
4. A different Player Output—including another output on the same Player—is refused, and Package data contains no Live overlay or actual-Showing state.

## Non-goals and hard stops

No Showing or Live-state implementation; no Platform source; no networking, cache, pairing, device, endpoint, persistence, DI, `Program.cs`, project file, or guide change. If the accepted Core API must change, stop and return to architecture.

## Required gates

```bash
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A.RuntimePackageFixtureTests
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A.CoreFixtureContractTests
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release --no-restore
git diff --check
git diff --name-only <accepted-base-sha>...HEAD
```

The final command must list exactly the two writable paths. Create one scoped commit; do not push or merge.

## Required completion report

Use the M1-A.1 report format, headed `M1-A.2 RESULT`, with four numbered assertion results, exact command results, code-bearing line count, scope check, and blocker. One targeted correction is allowed only for defects named by Claude Sonnet and only in the two locked files.
