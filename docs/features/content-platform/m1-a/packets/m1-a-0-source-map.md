# M1-A.0 — Exact-Head Source Map

**Status:** Proposed execution packet  
**Worker:** Local Qwen  
**Planned worktree:** `m1a-0-source-map`  
**Reviewer:** ChatGPT Terra  
**Depends on:** owner-approved M1-A packet and passing Decision Fidelity Review  
**Changes allowed:** one evidence document only

## Job

Inspect the exact current `master` source. Produce an evidence map that tells later packets where to add their work without guessing or moving legacy code.

## Required inspection

- `src/Vennu.Api/Program.cs`: composition and registration points.
- `src/Vennu.Api/Menus/`: existing Menu/import seam; list only relevant touchpoints.
- `src/Vennu.Api/PlatformOperations/`: existing support boundary; list only relevant touchpoints.
- `src/Vennu.Api/Release/`: distinguish product-version code from the renewed presentation terms.
- the existing applicable API test projects and their test discovery/fixture patterns.
- existing solution/project references needed to compile a focused internal-contract test.

## Deliverable

Create only:

`docs/features/content-platform/m1-a/evidence/source-base-map.md`

It must name the exact base SHA, inspected paths, likely test home, candidate composition point, conflicts to avoid, and a **PASS / BLOCKED** recommendation for M1-A.1, M1-A.2, and M1-A.3.

## Hard stops

Do not create production source, rename existing code, edit the parent packet, add an endpoint, migration, fixture database row, or client change. If the map finds that M1-A needs one of those, mark **BLOCKED** and explain why.

## Validation and review

- Confirm the report is based on its recorded exact SHA.
- Reviewer independently samples every claimed path and checks that no conclusions were invented.
- A reviewed PASS unblocks M1-A.1 and M1-A.2. A BLOCKED result returns to the architect.
