# M1-A.6 — Complete Fixture Journey Proof

**Status:** Proposed integration packet
**Worker:** ChatGPT CLI — Terra
**Reviewer:** Claude Opus
**Base:** `<accepted-assembled-sha>` containing accepted M1-A.4 and M1-A.5
**Planned worktree:** `m1a-6-journey-proof`
**Estimate:** 70–120 code-bearing lines

## One outcome

Prove the complete disposable journey using the accepted contracts without changing or redesigning them:

```text
Published Presentation → exact Player Output Runtime Package → Live overlay + Showing evidence → read-only Platform view
```

## Exact writable path

- `tests/Vennu.Api.Tests/ContentPlatform/M1A/M1AJourneyTests.cs`

No other file may change. All accepted source, focused tests, and guides are read-only inputs.

## Observable assertions

1. The matching fixture journey completes and the support view separates desired presentation from actual Package/Showing facts.
2. One Live value change changes the composed view while the Published Presentation identity remains unchanged.
3. A Published Presentation not named by the desired assignment and wrong organization, venue, and Player Output requests are each refused in the complete path.
4. Static source search finds no M1-A endpoint mapping, persistence, migration, public transport type, Connect implementation, or `Program.cs` registration.
5. The full focused M1-A suite and Release API build pass.

## Required gates

```bash
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release --no-restore
rg -n 'Map(Get|Post|Put|Patch|Delete)|Controller|Hub|DbContext|Migration|Repository|IHostedService|AddSingleton|AddScoped|AddTransient' src/Vennu.Api/{Core,Connect,Runtime,Platform}
git diff --check
git diff --name-only <accepted-assembled-sha>...HEAD
```

The `rg` command must return no M1-A implementation match; README prose matches must be listed and identified as documentation only. The final command must list exactly the one writable path. Create one scoped commit; do not push or merge.

## Hard stops

Do not repair or reinterpret an accepted dependency. Do not add composition registration, a helper, fixture data outside the test, endpoint, persistence, migration, customer behavior, or broader test harness. A dependency conflict returns to architecture.

## Required completion report

Use the M1-A.1 report format, headed `M1-A.6 RESULT`, with five numbered assertion results, exact command results, code-bearing line count, scope check, and blocker. Claude Opus reviews the full assembled M1-A implementation and this packet's one-file diff. One targeted correction may edit only this test unless the finding identifies an accepted dependency defect; a dependency defect stops and returns to architecture.
