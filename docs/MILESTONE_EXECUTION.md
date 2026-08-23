# Milestone Execution — Standard Operating Procedure

The numbered steps an agent takes to work one milestone, in any feature area. This is the
ordered procedure; `AGENTS.md` remains the authoritative policy and states the rules these steps
carry out. Where this document and `AGENTS.md` disagree, `AGENTS.md` wins and this document is
wrong and gets fixed.

It also reconciles that policy with the **superpowers** plugin, so that using the plugin does not
quietly drop house discipline.

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
| 3 | Confirm the design authority is approved and landed in `docs/design/approved/<feature>/`. If it is still in `proposed/`, stop — implementation is not authorized. | house |
| 4 | Check the tracker and open claims. **Stop on ownership conflict and re-plan** — do not rule on it and continue. | house |
| 5 | Create the milestone issue. | house |
| 6 | Record the claim in `tracker/assignments.json`. | house |
| 7 | Create an isolated worktree and branch `feature/<area>-m<n>-<short-name>` from merged `master`. | both |
| 7a | **Any two pieces of git work that can be in flight at overlapping times get separate worktrees — no exception for "this one is small."** A controller's own one-file fix, run in the same directory as an active subagent's multi-hour task, is the collision this step exists to prevent; judging a task "too small to bother isolating" is the failure, not a reasonable shortcut. Atlas M2.1 and M3 collided this way once, self-corrected mid-run, and left a report saying so — the report was read and the lesson was not applied, and the same collision then deleted an entire shipped milestone (M3's write-back, ~4,400 lines) from a commit that was never told to touch it. If a task cannot say for certain that no other process holds the same working directory, it gets its own worktree before it gets a single command. | house |
| 7b | **After every commit made outside a reviewed task loop — a controller's own fix, a docs edit, anything not going through steps 13–19 — run `git show --stat` on it and compare the file list and line counts against what was intended, before doing anything else.** This is the one check that would have caught 7a's incident in seconds: a commit meant to touch three files with small comment edits instead showed 37 files and −4,407 lines, and nobody looked. `git commit` succeeding is not evidence the tree it produced is the tree that was intended — it is only evidence the command ran. | house |

## Phase B — Before writing code

| # | Step | Source |
|---|---|---|
| 8 | **State the whole behaviour.** What complete user behaviour is this part of, what happens immediately before and after it, and where else that behaviour lives. | house |
| 9 | **Map the paths.** Which path led in, which leads out, and what *other* paths exist — the refusal, the conflict, the empty case, the permission denial, the second person arriving mid-way, the retry, the stale actor acting late. Name each and what validates it. A path with nothing validating it is named as unvalidated, never left implied. | house |
| 10 | **Search for where else this behaviour lives**, before writing anything. Run a real search, and keep the command and its full output. Knowing every location up front shapes the work; discovering them at the end only catches omissions. Step 20 re-runs this against the finished change. | house |
| 11 | **Write down the area's invariants** — the states its model says cannot exist — and arrange to assert them after every integration test in that area. | house |
| 12 | If the milestone changes a page or screen, load `.agents/skills/impeccable/SKILL.md` and follow its routing and bounded verification rules. | house |
| 12a | **Write the cross-cutting conventions into the plan's Global Constraints, before the first task is dispatched.** Signatures are already covered — the plan's Interfaces block names what each task consumes and produces, which is why tasks rarely get a function shape wrong. What goes unguarded is everything that is not a signature: how a failure message is phrased, whether it names an absolute or a repository-relative path, exit codes, vocabulary, log style. Each task is reviewed against its own brief, so a convention stated nowhere is violated by nobody and drifts anyway. State it once here and it becomes every task reviewer's attention lens. | house |
| 12b | **Where a constraint refers to something that already exists, cite its path — never a description of it.** A plan that says "use the product's locked palette" hands the implementer a phrase; a plan that says `src/back-office/src/sky-ui-tokens.css` hands it the authority. Atlas M1 shipped an invented palette through six clean reviews for exactly this reason: the decision named a real file, the plan restated it in prose, and every reviewer checked the work against a brief that also named no file. The rule covers palettes, schemas, contracts, vocabularies, message formats and API shapes — anything the repository already holds. | house |

## Phase C — Per task, repeated

| # | Step | Source |
|---|---|---|
| 13 | Dispatch a **fresh implementer subagent** for the task, inheriting none of the coordinating session's context. This is not only context hygiene: an implementer that can read only what is written either succeeds or exposes a thin plan, where one carrying the design conversation would coast on remembered intent and never surface the gap. | sp |
| 14 | **Run the test cycle.** One motion, four beats: write the failing test; **run it and watch it fail**; write the minimal implementation; run it and watch it pass. No production code without a failing test — code written before its test is deleted, not adapted. Watching it fail is the cheapest, highest-value moment in this procedure: if you did not see it fail, you do not know it tests the right thing. | both |
| 15 | Two constraints govern *what* you write at step 14, not what happens after it. **Test the rule where it is enforced** — a refusal enforced in SQL is asserted against a database; an in-memory double stores state and may be told to fail, but never decides, and a double that re-implements a rule proves the copy, which drifts. And for a **bug fix**, the cycle gains a beat: revert the fix, run, observe the failure, restore. A regression test not verified to fail with its fix reverted has not closed the finding. | house |
| 15a | **Where a step 12a convention is mechanical, the task that establishes it also writes the test that enforces it** — one assertion in the first task guards every task after it, at no recurring cost. "Every failure message names a repository-relative path" is a test, not a hope. This is what makes a convention hold across a milestone nobody is reading end to end: Atlas M1's rule that no generated page may carry a project name was asserted in the task that built the theme and enforced automatically from then on, while its path-spelling convention was written down nowhere and diverged between two modules by the next task. | house |
| 16 | **Implementer self-reviews before handing off.** Re-read the task's own requirements against what was written; check the test asserts the behaviour rather than the implementation; look for anything the plan asked for and did not get. A finding caught here costs a minute in context that already exists; the same finding at step 18 costs a reviewer dispatch, a findings package and a resume. | sp |
| 17 | Commit the task. Code commits are frequent; **living records are not touched yet** — see step 32. | both |
| 18 | Dispatch a **task reviewer** subagent for spec compliance and code quality. **Scope this to risk, not to task count:** a task that only adds a project file, a reference or scaffolding folds into the next substantive task's review rather than earning its own. Fold, never skip — "it looks simple" is the rationalization this procedure exists to defeat. This is *not* the merge-gate review in step 25. | sp |
| 19 | On findings: fix rounds 1–3 resume the implementer; rounds 4–5 dispatch a fresh implementer on a more capable model. At round 5, adjudicate each open finding. Log any judgement call as `Ruling: <what> — <why> — <what it costs if wrong>` — **but not for the four things that stop you**, see conflict 2 below. | sp |

### What to subagent, and what it actually buys

**Subagenting is not a speed optimization.** For a single serial task it usually costs time:
constructing the brief, the agent reading its way in, and the handoff back can exceed a five-minute
task. Judge it on wall-clock for one task and it will lose, and that is the wrong measure.

It pays in three other currencies:

- **Context capacity.** Across a multi-milestone feature the coordinating session would otherwise
  fill up and start making worse decisions. This is the largest benefit and it is invisible until
  it bites.
- **Independence.** `AGENTS.md` requires review "never by its author." A fresh agent that never
  watched the code being written is the cheapest way to actually satisfy that rather than
  approximate it.
- **Plan audit.** An implementer that can read only what is written either succeeds or proves the
  plan was thin. See step 13.

**Parallelism is the only one that buys wall-clock**, and only where tasks are genuinely disjoint.

**The deciding test.** Can the task's brief be written without saying "as we discussed"?

- **Yes** → subagent it. Its inputs are expressible in writing, its output is verifiable by a
  command, and it needs no mid-flight negotiation.
- **No** → either write that context down properly, which makes it subagentable and improves the
  plan, or keep it inline. The test doubles as a plan-quality check.

**Never subagent:**

- Anything touching orchestrator-owned files — contracts, project files, dependency injection,
  migrations, package configuration, shared fixtures, workflows, tracker, status, handoff.
- Decisions that belong to the owner: scope, ownership conflicts, out-of-scope calls.
- Exploratory debugging, where the trail matters more than the result and a summarized handoff
  loses it.

**Running tasks in parallel.** Tasks whose files do not overlap may run concurrently, with
`dispatching-parallel-agents`. The guard is already in `AGENTS.md`: no two agents modify the same
file, and the orchestrator-owned list above stays with the orchestrator. Where a plan's tasks
build on each other's interfaces they are serial, and running them in parallel only produces
conflicts — most milestones are serial chains, and pretending otherwise costs more than it saves.

## Phase D — Before calling the milestone done

| # | Step | Source |
|---|---|---|
| 20 | **Re-run step 10's search** against the finished change. Paste the command and its full results into the report. Every location in those results that was not changed is named, with the reason. | house |
| 21 | Walk the **Definition of Done** checklist in AGENTS.md — behaviour, data, navigation and persistence, access, integration, display, and the multiplier. Items that do not apply are **named as not applying**, never passed over silently. | house |
| 22 | Run the house gate, not a generic suite: affected **Release** builds, focused unit tests, static checks, applicable non-integration migration validation, and the **Playwright UI gate**. | house |
| 23 | Record skipped tests. Azure SQL and integration-type tests are skipped by standing owner exception — say so rather than reporting a pass. | house |
| 24 | Evidence is **a command someone else can rerun, and its output**. "Verified working" is not evidence. Anything not actually executed is marked **UNTESTED**, which is an acceptable answer where a false "done" is not. | both |
| 24a | **Dispatch a whole-branch review with fresh eyes, over the entire diff at once.** Every task was reviewed in isolation against its own brief, so nothing has yet looked at the milestone as one thing. This step asks what that structure cannot: has logic been duplicated and then diverged; is an interface used differently by two consumers; did error styles, path spellings or vocabulary drift between modules; is there dead code, or a test file duplicating another's coverage; has a module grown past what it should hold. Close with the plain question — if a competent engineer inherited this branch tomorrow with no context, what trips them up first. Give the reviewer every finding parked during execution and make it rule ship-or-fix on each; a finding deferred task by task has had no other moment to be judged as a whole. This is not step 25 and does not replace it: this one hunts drift, step 25 gates the merge. | both |
| 25 | Obtain **independent review — never by the author.** Full diff, acceptance criteria, architecture and security impact, tests, artifacts, secrets, debug code, unrelated changes, branch drift, documentation accuracy. | house |
| 26 | Decisions are `APPROVE`, `REQUEST_CHANGES` or `COMMENT`. New commits invalidate prior approval. Never proceed with unresolved material comments. If GitHub blocks self-approval, record the decision, reviewed SHA, validation status and residual risks in a top-level PR comment. | house |
| 27 | Produce the **owner acceptance workbook**, 5–10 minutes. A milestone shipping no UI gets a demo script instead. | house |
| 27a | **The owner may waive step 27. The agent runs the checks anyway.** A waiver removes the owner's *time*, never the *verification*: work every case the workbook would have contained, against the running product, exactly as the owner would have, and report each result with its evidence. Marking a case "waived" when nobody executed it is how a milestone merges on no one having looked. | house |
| 28 | Acceptance asserts **what the customer sees** — what a screen shows after a sequence, not that an API accepted a request. A workbook can pass every check while the product is visibly broken, and has. | house |
| 29 | Record the outcome durably as `docs/features/<feature>/m<n>-acceptance-record.json`. Where step 27 was waived, record that it was waived, by whom, and the agent's own results in its place — so the record shows what was actually verified rather than implying an acceptance that never happened. | house |

## Phase E — Merge

| # | Step | Source |
|---|---|---|
| 30 | Push with `[skip ci]` while CI is suspended. When the owner restores CI, delete that note and the suspension note rather than relying on a green tick that never ran. | house |
| 31 | Merge the single PR, delete the branch. **Merge happens on an accepted record** — or on explicit owner instruction to merge without one, which is what Menus M3 required, and is never the default. | house |

## Phase F — After merge

| # | Step | Source |
|---|---|---|
| 32 | **Now** synchronize the living records, in one batch: the milestone issue, `PROJECT_STATUS.md`, `tracker/assignments.json`, `ai/handoffs/current.md`, this feature's records, and any affected architecture, API, database or operations document. A change that makes a controlled record false updates it in the same commit. | house |
| 33 | Append to the handoff **last**: what was established, what was assumed, what was deliberately left for later and for whom, and any open questions. Name one exact next action. | house |
| 34 | Record discoveries as GitHub issues. Owner-approved out-of-scope decisions become backlog issues **at the moment of decision**. Only now may the next milestone start. | house |

---

## The four genuine conflicts

**1 · "Continuous execution" stops at the milestone boundary, not after it.**
Superpowers' subagent-driven skill says do not pause between tasks — execute the whole plan
without checking in. That is correct *within* one milestone and wrong across milestones: AGENTS.md
requires an owner acceptance workbook before a successor starts, and one milestone at a time. An
agent reading the skill literally would run M1 through M6 in one go. **Resolution:** continuous
within a milestone plan; a hard stop at step 27.

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
ignore its merge option. Steps 25–31 are the only merge path, and step 27 gates it.

**4 · The skill's "full test suite" is not the house gate.**
It says run `npm test` / `pytest` / equivalent. The house gate is narrower and wider at once —
affected **Release** builds, focused unit tests, static checks, migration validation, the
Playwright gate, and Azure/integration explicitly skipped and recorded. **Resolution:** step 22
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

## Status

Adopted as general standard operating procedure (owner, 2026-08-21). It applies to every feature
area, not only the one it was written during.

Steps 12a, 12b, 15a and 24a were added on 2026-08-22, after Atlas M1 showed the gap they close. Eight
tasks were each implemented and reviewed against their own brief, and none was wrong against it —
yet two modules still ended up reporting failure paths in different styles, because the convention
existed in no brief at all. The three steps attack that at each of the points it can be caught:
state the convention before the first dispatch, enforce it by test where it is mechanical, and look
at the whole branch once with fresh eyes before merge. The last of the three is the only one that
catches a *semantic* drift, where two modules both look correct and mean different things — no
mechanical check finds that, which is why the step exists even though it is the least automatable.

Steps 7a and 7b were added on 2026-08-23, after the collision they name actually destroyed shipped
work rather than merely blurring a convention. Atlas M2.1 and M3 had already collided once in one
shared working directory and self-corrected mid-run; the report documenting it was read and its
lesson was not carried forward, and the same collision then produced a commit — meant to touch
three files — whose actual tree deleted an entire milestone. It went unnoticed for hours because
nothing checked the commit against what was intended, and it was found only when an unrelated
review read the diff instead of the message. 7a removes the judgment call that let it happen a
second time; 7b is the one-command check that would have caught it in seconds rather than hours.

`AGENTS.md` is the policy and this is the procedure. A change that makes one false updates the
other in the same commit — if a house rule changes, the step carrying it changes with it.

The superpowers reconciliation is current as of plugin version 6.3.0. If the plugin is upgraded,
re-read `subagent-driven-development`, `test-driven-development`,
`verification-before-completion` and `finishing-a-development-branch`, and check the four
conflicts above still describe what those skills say.
