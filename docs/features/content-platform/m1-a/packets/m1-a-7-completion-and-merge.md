# M1-A.7 — Completion, Targeted Correction, and Merge

**Status:** Approved coordinator packet; waiting for clean M1-A.6 review
**Owner:** VennueSign Architect Lead
**Depends on:** clean M1-A.6 independent review

## One outcome

Assemble the exact accepted commits, complete the controlled records, obtain exact-head review, and merge one implementation PR without adding product behavior.

## Required inputs

- owner approval of the M1-A graph;
- owner approval of the complete Mosaic V1 wave graph;
- reviewed M1-A.0 PASS comment;
- accepted commits and review decisions for M1-A.1 through M1-A.6;
- exact changed-file lists and command evidence from every packet;
- confirmed fixture deletion condition.

## Coordinator-owned writes

- `docs/features/content-platform/done-records/<implementation-pr-number>.md`
- `tracker/assignments.json`
- `PROJECT_STATUS.md`
- `ai/handoffs/current.md`
- issue #970 checklist/status and the implementation PR body/comments

No product source or test file may be authored in this packet.

## Merge gate

1. Every packet commit is present exactly once and no file falls outside the union of its locks.
2. The Done Record describes the exact implementation PR head and answers every item PASS, N/A with reason, or UNTESTED with risk.
3. Focused M1-A tests, Release API build, changed-path exclusion search, and `git diff --check` are rerun on the assembled head.
4. Claude Opus independently reviews the full assembled head against the parent boundary and every packet assertion.
5. The owner-approved fixture deletion condition remains explicit.

## Exact pre-merge gates

Run these commands on the assembled implementation head:

```bash
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~ContentPlatform.M1A
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release --no-restore
rg -n 'Map(Get|Post|Put|Patch|Delete)|Controller|Hub|DbContext|Migration|Repository|IHostedService|AddSingleton|AddScoped|AddTransient' src/Vennu.Api/{Core,Connect,Runtime,Platform}
git diff --check origin/master...HEAD
git diff --name-only origin/master...HEAD
git log --format='%H %s' origin/master..HEAD
```

Account for every `rg` match and every changed file. The changed-file list must equal the union of the accepted packet locks plus the coordinator-owned records. Commit the completed records before requesting exact-head review.

## Fixed pre-merge report

Post this on the implementation PR before merge:

```text
M1-A.7 PRE-MERGE RESULT: PASS | BLOCKED
Implementation PR: <number>
Base SHA: <sha>
Reviewed head SHA: <sha>
Packet commits:
- M1-A.1: <sha> — <worker/model> — <review decision>
- M1-A.2: <sha> — <worker/model> — <review decision>
- M1-A.3: <sha> — <worker/model> — <review decision>
- M1-A.4: <sha> — <worker/model> — <review decision>
- M1-A.5: <sha> — <worker/model> — <review decision>
- M1-A.6: <sha> — <worker/model> — <review decision>
Commands:
- <exact command> — PASS | FAIL — <unedited summary>
Changed-file union: PASS | FAIL — <details>
Done Record exact-head check: PASS | FAIL
Full independent review: APPROVE | REQUEST_CHANGES | COMMENT
Targeted correction count: <0 or 1>
UNTESTED: NONE | <item and residual risk>
Merge gate: OPEN | BLOCKED
```

If review names corrections, create one bounded correction prompt listing only the defect, affected assertion, exact writable path, and rerun gates. The same reviewer checks only those corrections. A second implementation defect, unchanged correction, scope expansion, or shared-contract change stops and returns to architecture.

## Post-merge issue comment

Only after GitHub confirms the merge, post this on issue #970:

```text
M1-A MERGED
Implementation PR: <number>
Reviewed head SHA: <sha>
Merge SHA: <sha>
Validation: PASS | <residual UNTESTED items>
Review: <decision>; targeted corrections: <0 or 1>
Fixture deletion condition: CONFIRMED
Architecture result: <plain technical outcome>
Development-system result: <packet sizing, routing, review, and integration outcome>
Foundation checkpoint G-M1A-CHECK: PROCEED | HOLD — <reason>
Newly unblocked nodes: <exact nodes or NONE>
Still blocked nodes: <exact nodes or NONE>
Next action: <one exact action>
```

Do not claim a real screen, production Runtime contract, M1-B approval, or Mosaic acceptance.
