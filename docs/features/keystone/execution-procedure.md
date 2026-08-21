# Keystone — Milestone Execution Procedure

The numbered steps an agent takes to work one Keystone milestone, reconciling AGENTS.md's
existing discipline with the superpowers plugin being trialled on this feature.

**Source column:** `house` = AGENTS.md, and it wins. `sp` = superpowers. `both` = the two agree
and reinforce each other.

**Where they conflict, AGENTS.md wins.** Superpowers is a general-purpose plugin with no
knowledge of this repository's history; every house rule below exists because something went
wrong once. The four genuine conflicts are listed at the end with the resolution stated.

---

## Phase A — Before touching anything

| # | Step | Source |
|---|---|---|
| 1 | Read `AGENTS.md`, `ai/handoffs/current.md`, `tracker/assignments.json`, `PROJECT_STATUS.md`, this feature's records, and the linked GitHub state. Nothing else. | house |
| 2 | Confirm the predecessor milestone is **merged and its owner workbook accepted**. One milestone runs at a time. | house |
| 3 | Confirm the design authority is approved and landed in `docs/design/approved/keystone/`. If it is still in `proposed/`, stop — implementation is not authorized. | house |
| 4 | Check the tracker and open claims. **Stop on ownership conflict and re-plan** — do not rule on it and continue. | house |
| 5 | Create the milestone issue. | house |
| 6 | Record the claim in `tracker/assignments.json`. | house |
| 7 | Create an isolated worktree and branch `feature/keystone-m<n>-<short-name>` from merged `master`. | both |

## Phase B — Before writing code

| # | Step | Source |
|---|---|---|
| 8 | **State the whole behaviour.** What complete user behaviour is this part of, what happens immediately before and after it, and where else that behaviour lives. | house |
| 9 | **Map the paths.** Which path led in, which leads out, and what *other* paths exist — the refusal, the conflict, the empty case, the permission denial, the second person arriving mid-way, the retry, the stale actor acting late. Name each and what validates it. A path with nothing validating it is named as unvalidated, never left implied. | house |
| 10 | **Search for where else this behaviour lives**, before writing anything. Run a real search, and keep the command and its full output. Knowing every location up front shapes the work; discovering them at the end only catches omissions. Step 24 re-runs this against the finished change. | house |
| 11 | **Write down the area's invariants** — the states its model says cannot exist — and arrange to assert them after every integration test in that area. | house |
| 12 | If the milestone changes a page or screen, load `.agents/skills/impeccable/SKILL.md` and follow its routing and bounded verification rules. Applies to M4 only. | house |

## Phase C — Per task, repeated

| # | Step | Source |
|---|---|---|
| 13 | Dispatch a **fresh implementer subagent** for the task, inheriting none of the coordinating session's context. This is not only context hygiene: an implementer that can read only what is written either succeeds or exposes a thin plan, where one carrying the design conversation would coast on remembered intent and never surface the gap. | sp |
| 14 | **Write the failing test first.** No production code without a failing test. Code written before its test is deleted, not adapted. | both |
| 15 | **Run it and watch it fail.** If you did not watch it fail, you do not know it tests the right thing. | sp |
| 16 | Write the minimal implementation. | both |
| 17 | Run the test and watch it pass. | both |
| 18 | For a **bug fix**, additionally revert the fix, run, observe the failure, restore. A regression test not verified to fail with its fix reverted has not closed the finding. | house |
| 19 | Test the rule **where it is enforced**. A refusal enforced in SQL is asserted against a database; an in-memory double stores state and may be told to fail, but never decides. A double that re-implements a rule proves the copy, and the copy drifts. | house |
| 20 | Commit the task. Code commits are frequent; **living records are not touched yet** — see step 36. | both |
| 21 | Dispatch a **task reviewer** subagent for spec compliance and code quality. This is *not* the merge-gate review in step 29. | sp |
| 22 | On findings: fix rounds 1–3 resume the implementer; rounds 4–5 dispatch a fresh implementer on a more capable model. At round 5, adjudicate each open finding. | sp |
| 23 | Log any judgement call as `Ruling: <what> — <why> — <what it costs if wrong>`. **But not for the four things that stop you** — see conflict 2 below. | sp |

## Phase D — Before calling the milestone done

| # | Step | Source |
|---|---|---|
| 24 | **Re-run step 10's search** against the finished change. Paste the command and its full results into the report. Every location in those results that was not changed is named, with the reason. | house |
| 25 | Walk the **Definition of Done** checklist in AGENTS.md — behaviour, data, navigation and persistence, access, integration, display, and the multiplier. Items that do not apply are **named as not applying**, never passed over silently. | house |
| 26 | Run the house gate, not a generic suite: affected **Release** builds, focused unit tests, static checks, applicable non-integration migration validation, and the **Playwright UI gate**. | house |
| 27 | Record skipped tests. Azure SQL and integration-type tests are skipped by standing owner exception — say so rather than reporting a pass. | house |
| 28 | Evidence is **a command someone else can rerun, and its output**. "Verified working" is not evidence. Anything not actually executed is marked **UNTESTED**, which is an acceptable answer where a false "done" is not. | both |
| 29 | Obtain **independent review — never by the author.** Full diff, acceptance criteria, architecture and security impact, tests, artifacts, secrets, debug code, unrelated changes, branch drift, documentation accuracy. | house |
| 30 | Decisions are `APPROVE`, `REQUEST_CHANGES` or `COMMENT`. New commits invalidate prior approval. Never proceed with unresolved material comments. If GitHub blocks self-approval, record the decision, reviewed SHA, validation status and residual risks in a top-level PR comment. | house |
| 31 | Produce the **owner acceptance workbook**, 5–10 minutes. A milestone shipping no UI gets a demo script instead — M1, M2, M3 and M6 are in that category. | house |
| 32 | Acceptance asserts **what the customer sees** — what a screen shows after a sequence, not that an API accepted a request. A workbook can pass every check while the product is visibly broken, and has. | house |
| 33 | Record the outcome durably (`m<n>-acceptance-record.json`). | house |

## Phase E — Merge

| # | Step | Source |
|---|---|---|
| 34 | Push with `[skip ci]` while CI is suspended. When the owner restores CI, delete that note and the suspension note rather than relying on a green tick that never ran. | house |
| 35 | Merge the single PR, delete the branch. **Merge happens on an accepted record** — or on explicit owner instruction to merge without one, which is what Menus M3 required and is never the default. | house |

## Phase F — After merge

| # | Step | Source |
|---|---|---|
| 36 | **Now** synchronize the living records, in one batch: the milestone issue, `PROJECT_STATUS.md`, `tracker/assignments.json`, `ai/handoffs/current.md`, this feature's records, and any affected architecture, API, database or operations document. A change that makes a controlled record false updates it in the same commit. | house |
| 37 | Append to the handoff **last**: what was established, what was assumed, what was deliberately left for later and for whom, and any open questions. Name one exact next action. | house |
| 38 | Record discoveries as GitHub issues. Owner-approved out-of-scope decisions become backlog issues **at the moment of decision**. Only now may the next milestone start. | house |

---

## The four genuine conflicts

**1 · "Continuous execution" stops at the milestone boundary, not after it.**
Superpowers' subagent-driven skill says do not pause between tasks — execute the whole plan
without checking in. That is correct *within* one milestone and wrong across milestones: AGENTS.md
requires an owner acceptance workbook before a successor starts, and one milestone at a time. An
agent reading the skill literally would run M1 through M6 in one go. **Resolution:** continuous
within a milestone plan; a hard stop at step 31.

**2 · "Rulings, not stalls" does not override the house stop conditions.**
The skill tells an agent to decide conflicts itself and log a ruling rather than wait. AGENTS.md
has cases where stopping is required: an **ownership conflict** in the tracker (step 4), anything
that would **refactor unrelated code or begin future-milestone work**, and out-of-scope decisions
that need owner approval before becoming backlog issues. **Resolution:** rule on plan defects and
ambiguities; stop on ownership conflicts, scope expansion, and anything needing owner approval.

**3 · `finishing-a-development-branch` must not merge.**
That skill verifies tests, then offers a three-option menu including merging to the base branch.
AGENTS.md permits merge only after independent review by someone other than the author, with no
unresolved material comments. **Resolution:** use the skill for its verification and cleanup;
ignore its merge option. Steps 29–35 are the only merge path, and step 31 gates it.

**4 · The skill's "full test suite" is not the house gate.**
It says run `npm test` / `pytest` / equivalent. The house gate is narrower and wider at once —
affected **Release** builds, focused unit tests, static checks, migration validation, the
Playwright gate, and Azure/integration explicitly skipped and recorded. **Resolution:** step 26
replaces it.

## What superpowers adds that the house process did not have

- **Fresh subagent per task**, inheriting none of the coordinating session's context.
- **Two-stage review per task** — spec compliance and code quality — sitting *underneath* the
  house's independent PR review rather than replacing it.
- **Fix-round escalation:** rounds 1–3 resume the implementer, rounds 4–5 start fresh on a more
  capable model, round 5 adjudicates.
- **The ledger**, making judgement calls visible as explicit rulings with a stated cost if wrong.
- **Worktree isolation** as a default, which is what AGENTS.md's multi-agent safety rules assume
  but never provide.
- **"If you didn't watch the test fail, you don't know if it tests the right thing"** — a sharper
  statement of why tests-with-implementation matters than the house rule gives.

## What the house process has that superpowers has no equivalent for

Any of these would be silently dropped by an agent following the plugin alone:

- Reading the handoff first and appending to it last.
- The milestone issue and the tracker claim.
- Stating the whole behaviour, and mapping every path with what validates it.
- Area invariants asserted automatically after every integration test.
- "Where else does this apply" answered with a real search, command and full output pasted.
- The Definition of Done checklist, with non-applicable items named rather than skipped.
- The Impeccable skill before changing any page or screen.
- `[skip ci]` while CI is suspended, and the rule for restoring it honestly.
- The owner acceptance workbook, and acceptance asserting what the customer sees.
- Batching living-record updates to milestone completion rather than after every commit.

---

*Scope: written for Keystone, which is where superpowers is being trialled. If it holds up across
a milestone or two, it is a candidate for promotion into AGENTS.md rather than living here.*
