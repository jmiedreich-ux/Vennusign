# M1-A Work Graph — API Module Foundation

**Status:** Proposed. This graph becomes executable only after the owner approves the M1-A packet and the Decision Fidelity Review gate has passed.  
**Parent packet:** `../m1-a-api-module-foundation.md`  
**Issue:** #970

## The work, at a glance

| Packet | Work | Agent / model | Depends on | Output | Independent review |
|---|---|---|---|---|---|
| M1-A.0 | Exact-head source map | Local Qwen | owner approval + DFR gate | Evidence map; no production edit | ChatGPT Terra |
| M1-A.1 | Module guides and static boundary check | Local Qwen | M1-A.0 | Four guides + one narrow guard | Claude Sonnet |
| M1-A.2 | Fixture contracts and seam tests | ChatGPT Sol | M1-A.0 | Internal Core/Runtime/Platform contracts and tests | Claude Opus |
| M1-A.3 | Integration and acceptance proof | ChatGPT Terra | M1-A.1 + M1-A.2 | One assembled implementation PR and Done Record | Claude Opus |
| M1-A.4 | Completion decision | Architect coordinator | M1-A.3 review pass | merge or one bounded correction cycle | Targeted Claude Opus review |

M1-A.1 and M1-A.2 run in parallel, in separate worktrees and with no shared files. M1-A.3 is the only packet allowed to combine their work.

## Execution rules

- Every packet runs through the local wrapper lifecycle: isolated worktree, durable evidence, exact commit, validation result, independent review, one targeted correction cycle, and hard stop.
- A review failure blocks its successor. It does not authorize a workaround or a wider rewrite.
- The worker never merges. M1-A.4 is the only merge path.
- A packet cannot silently change its listed files, public contracts, migrations, or customer behavior. Any such need returns to the architect.
- Branch and worktree names are reserved by packet ID; no two packets may claim the same file.
- The fixture is test-only. It never becomes a temporary source of customer truth.

## What this fixes

The M1-A definition is the architectural guardrail. These records are the executable instructions. The graph makes the waiting explicit: Sol and Qwen cannot begin until M1-A.0 is reviewed; Terra cannot begin until both parallel packets are independently approved.
