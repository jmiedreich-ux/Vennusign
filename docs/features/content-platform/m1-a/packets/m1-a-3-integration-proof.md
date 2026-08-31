# M1-A.3 — Integration and Acceptance Proof

**Status:** Proposed execution packet  
**Worker:** ChatGPT CLI — Terra  
**Planned worktree:** `m1a-3-integration-proof`  
**Reviewer:** Claude Opus  
**Depends on:** M1-A.1 and M1-A.2 independently approved  
**File lock:** integration-only files, test assembly, Done Record, status/handoff only after proof passes

## Job

Assemble the independently reviewed guide and fixture-contract branches into one coherent M1-A implementation PR. Do not redesign either input.

## Required proof

1. Four guides are present and structurally complete.
2. The Core → Runtime → Showing path works in-process.
3. Wrong Player Output, organization, and venue are refused in Package, Live overlay, and Platform composition tests.
4. A Live change leaves the Published Presentation identity unchanged.
5. Platform has no writable store or command path.
6. Search of changed paths finds no endpoint mapping, migration, persistence, or public transport contract.
7. Local build and focused tests are recorded. Any unavailable validation is honestly marked UNTESTED with reason.

## Allowed changes

Only the minimum composition/test assembly justified by M1-A.0, a Done Record at `docs/features/content-platform/done-records/<pr-number>.md`, and the required status/handoff update after proof passes.

## Hard stops

Do not absorb a failed review by changing scope. Do not add a public endpoint, database behavior, customer-facing behavior, real Player Output delivery, or an implementation outside the two reviewed inputs. Any conflict between inputs returns to the architect.

## Handoff

Record base/head SHAs, exact worker and reviewer, test evidence, remaining exclusions, and whether the fixture deletion condition remains intact. Submit to M1-A.4; do not merge.
