# M1-A.5 — AI-Friendly Module Guides

**Status:** Proposed documentation packet
**Worker:** Local Qwen
**Reviewer:** Claude Sonnet
**Base:** `<accepted-base-sha>` from accepted M1-A.3
**May run in parallel with:** M1-A.4
**Planned worktree:** `m1a-5-module-guides`
**Estimate:** 80–140 documentation lines total

## One outcome

Write four short module guides that faithfully describe the accepted M1-A ownership seams and clearly distinguish the disposable fixture from future production contracts.

## Exact writable paths

- `src/Vennu.Api/Core/README.md`
- `src/Vennu.Api/Connect/README.md`
- `src/Vennu.Api/Runtime/README.md`
- `src/Vennu.Api/Platform/README.md`

No other file may change. Accepted code and tests may be read but not edited.

## Required headings in every guide

1. `# <Module>`
2. `## Owns`
3. `## Accepts and returns`
4. `## Must not own`
5. `## Local terms`
6. `## Data and tests`
7. `## Legacy retirement condition`

Core, Runtime, and Platform must link to their exact `M1A` source/test paths and label those contracts disposable. Connect must explicitly say M1-A contains no Connect code, parsing, mapping, import, persistence, or direct write to Core. Every guide must say that it creates no public contract.

## Observable assertions

1. All four files contain all seven headings exactly once.
2. Their ownership statements agree with the parent M1-A boundary table.
3. Core, Runtime, and Platform point to the accepted fixture source/tests; Connect claims no implementation.
4. Every guide states the correct legacy-retirement condition and no-public-contract limit.

## Non-goals and hard stops

Do not change source, tests, configuration, parent planning records, or legacy documentation. Do not invent endpoints, storage, Theme behavior, Connect behavior, or production contract names.

## Required gates

```bash
for f in src/Vennu.Api/{Core,Connect,Runtime,Platform}/README.md; do for h in '## Owns' '## Accepts and returns' '## Must not own' '## Local terms' '## Data and tests' '## Legacy retirement condition'; do test "$(grep -Fxc "$h" "$f")" -eq 1 || exit 1; done; done
git diff --check
git diff --name-only <accepted-base-sha>...HEAD
```

The final command must list exactly the four writable paths. Create one scoped commit; do not push or merge.

## Required completion report

Use the M1-A.1 report format, headed `M1-A.5 RESULT`, replacing code-bearing lines with documentation lines and reporting the four assertions. One targeted correction is allowed only for defects named by Claude Sonnet and only in the four locked files.
