# M1-A Work Graph — API Module Foundation

**Status:** Approved on 2026-08-31 after Decision Fidelity Review. No implementation packet is dispatchable until `G-MAESTRO-REG` opens.
**Parent boundary:** `../m1-a-api-module-foundation.md`
**Tracking issue:** #970

## Ordered packets

| Packet | One bounded outcome | Worker / model | Depends on | Writable paths | Expected size | Reviewer |
|---|---|---|---|---|---:|---|
| M1-A.0 | Confirm the exact source and test homes | Local Qwen | `G-V1-PLAN` + `G-MAESTRO-REG` | no repository files; structured issue comment only | no code | ChatGPT Terra |
| M1-A.1 | Add the Core fixture identity and frozen-presentation contract | ChatGPT CLI / Sol | reviewed PASS from M1-A.0 | one named Core file and one named test file | 100–160 code-bearing lines | Claude Opus |
| M1-A.2 | Add output-specific Runtime Package creation | ChatGPT CLI / Terra | M1-A.1 accepted | one named Runtime file and one named test file | 90–140 code-bearing lines | Claude Sonnet |
| M1-A.3 | Add Live overlay and Showing evidence | ChatGPT CLI / Sol | M1-A.2 accepted | two named Runtime files and one named test file | 120–180 code-bearing lines | Claude Opus |
| M1-A.4 | Compose the read-only Platform support view | Local Qwen | M1-A.3 accepted | one named Platform file and one named test file | 80–130 code-bearing lines | Claude Sonnet |
| M1-A.5 | Write the four module guides from accepted contracts | Local Qwen | M1-A.3 accepted | four named README files only | 80–140 documentation lines | Claude Sonnet |
| M1-A.6 | Prove the complete fixture journey and exclusions | ChatGPT CLI / Terra | M1-A.4 + M1-A.5 accepted | one named end-to-end test file only | 70–120 code-bearing lines | Claude Opus |
| M1-A.7 | Complete records and merge the reviewed implementation | Architect coordinator | M1-A.6 approved | Done Record, tracker, status, handoff, issue/PR state | no product code | targeted reviewer if corrected |

M1-A.4 and M1-A.5 may run in parallel because their file locks do not overlap. All other implementation packets are sequential. A packet may be queued early, but it remains **Blocked** until every dependency is accepted.

## Packet completeness rule

The packet file is the complete prompt delivered to the worker. No worker receives a shortened restatement. Every implementation packet fixes:

- exact prerequisites and base commit;
- exact writable paths and an estimated code-bearing range;
- one outcome and explicit non-goals;
- three to five observable assertions;
- exact command gates;
- one required scoped commit;
- an exact completion report format;
- one targeted correction limit and a stop condition.

At dispatch, the coordinator replaces each packet's `<accepted-base-sha>` placeholder with the actual accepted dependency commit. No other packet wording may be silently changed. A required path or behavior change returns to architecture.

## Branch and assembly model

- Each implementation packet starts from its accepted dependency commit in an isolated worktree.
- M1-A.1 through M1-A.3 form the shared contract chain.
- M1-A.4 and M1-A.5 branch from the accepted M1-A.3 head. The coordinator assembles both accepted commits before dispatching M1-A.6.
- Workers never merge and never update controlled project records.
- M1-A.7 is the sole merge and controlled-record path.

## Correction and review rule

Every packet receives independent review by a reviewer who did not author it. A review finding produces one correction prompt containing only the named defects, affected assertions, and permitted files. The same reviewer then checks only those corrections. An unchanged return, a second implementation defect, a new scope need, or any edit outside the file lock stops the packet and returns it to the architect.

## Milestone ceiling

M1-A remains a disposable in-process skeleton. It creates no endpoint, migration, persistence, customer record, Theme, real Player Output delivery, or production Runtime contract. Passing M1-A does not claim that a live screen exists.

## Foundation Wave checkpoint — G-M1A-CHECK

After M1-A.7 merges, the architect verifies that its post-merge issue comment is complete and records one gate decision: **PROCEED** or **HOLD**. Only **PROCEED** opens `G-M1A-CHECK` and makes M1-B eligible for its own planning/dispatch gates.

The checkpoint records two separate outcomes:

1. **Technical:** which boundary assertions passed, failed, or remain UNTESTED.
2. **Process:** packet elapsed time, actual model route, correction count, review impact, retained work, scope violations, integration friction, and whether each packet was appropriately sized.

Evidence may change future eligible routes, size ceilings, or packet splits through a reviewed graph revision. It does not retroactively change an accepted packet or allow Maestro to author new project scope. A missing result, unresolved material review finding, or **HOLD** decision keeps the gate closed.
