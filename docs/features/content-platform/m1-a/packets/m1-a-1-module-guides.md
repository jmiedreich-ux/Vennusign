# M1-A.1 — Module Guides and Static Boundary Check

**Status:** Proposed execution packet  
**Worker:** Local Qwen  
**Planned worktree:** `m1a-1-module-guides`  
**Reviewer:** Claude Sonnet  
**Depends on:** reviewed PASS from M1-A.0  
**May run in parallel with:** M1-A.2  
**File lock:** only the files listed below

## Job

Create the four AI-friendly module guides required by M1-A. They make ownership visible before later code begins to grow around the new seams.

## Allowed files

- `src/Vennu.Api/Core/README.md`
- `src/Vennu.Api/Connect/README.md`
- `src/Vennu.Api/Runtime/README.md`
- `src/Vennu.Api/Platform/README.md`
- one narrowly named static boundary test or check in the test location identified by M1-A.0

The guides must follow the fixed M1-A ownership table. Each states: owned facts, permitted inputs/outputs, forbidden ownership, local terms, data/tests, legacy-path removal condition, and no public-contract claim.

The check must establish only that the four guide files exist and contain the required headings. It must not inspect runtime behavior, add packages, or bind public APIs.

## Hard stops

Do not edit `Program.cs`, existing modules, customer code, fixtures, controllers, database files, migrations, or build configuration. Do not invent a Connect implementation.

## Validation and review

Run the focused check and the smallest affected local build/test command identified by M1-A.0. Record the exact result. Claude reviews only the listed files against the M1-A ownership contract.
