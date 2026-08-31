# M1-A.3 — Live Overlay and Showing Evidence

**Status:** Proposed implementation packet
**Worker:** ChatGPT CLI — Sol
**Reviewer:** Claude Opus
**Base:** `<accepted-base-sha>` from accepted M1-A.2
**Planned worktree:** `m1a-3-live-showing`
**Estimate:** 120–180 code-bearing lines including tests

## One outcome

Add the two Runtime facts deliberately excluded from the frozen Published Presentation: a venue-scoped Live state overlay and actual Showing evidence for the Package's exact Player Output.

## Exact writable paths

- `src/Vennu.Api/Runtime/M1A/LiveStateOverlayFixture.cs`
- `src/Vennu.Api/Runtime/M1A/ShowingEvidenceFixture.cs`
- `tests/Vennu.Api.Tests/ContentPlatform/M1A/LiveAndShowingFixtureTests.cs`

No other file may change. Accepted M1-A.1 and M1-A.2 files may be read but not edited.

## Required contract

- `M1ALiveStateOverlay` targets a non-empty stable Item ID and exact organization/venue scope; its value is separate from the Published Presentation.
- Overlay resolution refuses a different organization or venue and returns the applicable value without changing the Published Presentation identity.
- `M1AShowingEvidence` records the exact Runtime Package and Player Output identities plus four actual Runtime facts: received, verified, applied, and currently showing.
- Showing transitions are monotonic: verified requires received; applied requires verified; currently showing requires applied. Impossible sequences refuse.
- Showing evidence refuses a different Player Output and never derives actual state from Core's desired assignment.
- Both fixture types carry the deletion condition.

## Observable assertions

1. Changing the Live overlay value leaves the Published Presentation identity unchanged.
2. Cross-organization and cross-venue overlay resolution refuses.
3. The valid received → verified → applied → currently-showing sequence records all four actual facts.
4. Skipped or reversed Showing transitions and a mismatched Player Output refuse.
5. Showing evidence contains no desired-assignment substitution.

## Non-goals and hard stops

No Theme rendering rule, Published-state implementation, real state persistence, device acknowledgement, transport, retry, cache, endpoint, Platform source, accepted-file edit, project/configuration change, or legacy behavior change. If M1-A.1 or M1-A.2 requires modification, stop.

## Required gates

```bash
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A.LiveAndShowingFixtureTests
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A.RuntimePackageFixtureTests
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release --no-restore
git diff --check
git diff --name-only <accepted-base-sha>...HEAD
```

The final command must list exactly the three writable paths. Create one scoped commit; do not push or merge.

## Required completion report

Use the M1-A.1 report format, headed `M1-A.3 RESULT`, with five numbered assertion results, exact command results, code-bearing line count, scope check, and blocker. One targeted correction is allowed only for defects named by Claude Opus and only in the three locked files.
