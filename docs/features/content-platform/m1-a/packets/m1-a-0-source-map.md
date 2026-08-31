# M1-A.0 — Exact-Head Source Map

**Status:** Proposed discovery packet
**Worker:** Local Qwen
**Reviewer:** ChatGPT Terra
**Depends on:** owner-approved Mosaic V1 wave graph, owner-approved M1-A graph, and passing Decision Fidelity Review
**Repository writes:** none

## One outcome

Confirm that the exact current `master` can support M1-A in the already named source and test homes without changing a project file, public composition root, legacy module, or build configuration.

## Required inspection

- `src/Vennu.Api/Vennu.Api.csproj`
- `src/Vennu.Api/Properties/AssemblyInfo.cs`
- `src/Vennu.Api/Program.cs`
- `src/Vennu.Api/Menus/`
- `src/Vennu.Api/PlatformOperations/`
- `src/Vennu.Api/Release/`
- `tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj`
- two representative focused unit-test files under `tests/Vennu.Api.Tests/`

Search the repository for `Core`, `Connect`, `Runtime`, `PublishedPresentation`, `RuntimePackage`, `ShowingEvidence`, and `M1A`. Account for every collision that could affect the named packet paths.

## Required output

Post one issue #970 comment using exactly these headings:

```text
M1-A.0 SOURCE MAP
Base SHA: <40-character master SHA>
Inspected paths:
- <path> — <relevant fact>
Search command: <exact command>
Search results:
<unedited output or NONE>
Confirmed test command: <exact command>
Packet path verdicts:
- M1-A.1: PASS | BLOCKED — <reason>
- M1-A.2: PASS | BLOCKED — <reason>
- M1-A.3: PASS | BLOCKED — <reason>
- M1-A.4: PASS | BLOCKED — <reason>
- M1-A.5: PASS | BLOCKED — <reason>
- M1-A.6: PASS | BLOCKED — <reason>
Conflicts to avoid:
- <path and reason, or NONE>
Overall: PASS | BLOCKED
```

Do not summarize command output as “clean.” Include it. This packet has no commit because it is a read-only preflight; the issue comment is its sole deliverable.

## Acceptance assertions

1. The comment names the exact 40-character base SHA.
2. Every required path is inspected and every search term is run.
3. The existing `Vennu.Api.Tests` reference and internal-access pattern are identified from source rather than assumed.
4. Every later packet receives a separate PASS/BLOCKED verdict against its exact file lock.

## Hard stops

Return **BLOCKED** if any named file already exists with another meaning, if the tests cannot access the proposed internal contracts without a project/configuration change, or if M1-A would require `Program.cs`, a public endpoint, persistence, migration, legacy move, or new dependency. Do not propose or make the repair.

## Review

Terra samples every claimed path and reruns the search at the same SHA. A reviewed overall PASS unblocks M1-A.1. Any inaccurate claim receives one comment-only correction; a second defect returns to architecture.
