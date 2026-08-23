# Atlas Milestone 4 — The feature planning page, second pass

> **This is a plan for work not yet done.** M2, M2.1 and M3 are records written after the fact; this
> one is written before, so the site renders what is coming as well as what happened. Future tense
> throughout is deliberate, and every "will" here is a claim that has not been tested yet.

**Goal:** Close what is still open on #780 against the feature planning page — the corrections the
owner made after seeing it rendered, the two defects that only looking revealed, and the craft pass
that was never run when the page was built.

**Where it will land:** the `Atlas` repository, as a tagged release. Vennusign consumes
`uses: jmiedreich-ux/Atlas@v1`, so moving the major tag forward is the whole of this repository's
side of it — no manifest change, no schema addition, nothing here to migrate. That is worth stating
because M2.1 and M3 both needed a Vennusign PR of their own and this one should not.

**Spec:** GitHub issue jmiedreich-ux/Vennusign#780, **all comments**, where later comments correct
earlier ones and the later one wins; the approved mock linked from it; and
`.agents/skills/impeccable/SKILL.md`, which `AGENTS.md` already requires before any page or screen
changes and which was not loaded when this page was built.

---

## The framing: these are corrections, not new wants

M2.1 rebuilt the page as drawn paths and the structure held. What did not hold is the drawing. Four
of the items below are the owner looking at what shipped and saying it is not what was decided —
most of all the two arrows, which shipped tiled end to end when what was settled is that they
**overlay**. That is not a refinement of M2.1; it is a correction to it, and M2.1's record describes
the tiled behaviour as if it were right. Where this plan and that record disagree, this one is the
later reading of #780 and wins.

The root cause is the same one M2.1 named and did not fully close: the page was built from
structural assertions and rendered late. It has now been rendered — there is a Playwright Chromium
on the machine, and the earlier claim that there was not came from a stale note nobody checked — so
this milestone has no excuse for shipping anything unseen.

---

## Task list

Twelve tasks, plus one that cannot start yet. They are written to be handed over as they stand.
Task 1 comes first and most of the rest are judged against what it draws.

Where a task is obviously shaped for one kind of worker, that is noted. A **Claude-shaped** task can
name the design and rely on the reader working the rest out from #780 and the mock; a
**model-shaped** task states what it is given, what it must produce and what must be true
afterwards, and needs nothing worked out. This is the first real use of that distinction, which M6
introduces as a rule.

### Task 1 · The two arrows become one object, overlaid

**Delivers:** the faint arrow spans the whole recorded length of a feature; the solid arrow is drawn
over it, from the top, as far as the work has actually reached. Each keeps its own head, growing
straight out of its own body, and the solid head sits over the faint body rather than beside it.

**Finished when:** Keystone renders as a faint arrow covering all six milestones with a solid arrow
laid over the stages on top of it, not a solid arrow ending where a faint one begins; no ribbon
anywhere in the fixture tiles end to end; and the existing tests that assert the tiled geometry are
**inverted by name** rather than deleted, so the change is visible in the suite rather than silent.

**Sequencing:** first, and nothing else in the arrow group can be judged before it. This is the
honest reading of #780's earlier note that you cannot have the first arrow without the second — they
are one object seen twice.

*Shape: Claude.* The geometry follows from an argument made across several comments, and the reader
has to reconcile them.

### Task 2 · Centre the ribbon on its feature's name box

**Delivers:** the header box above a feature and the ribbon below it share a centre line. They do
not today.

**Finished when:** the two centres are the same computed value, asserted rather than eyeballed, at
desktop width and at 390px.

*Shape: model.* Given the two elements, produce a shared centre, assert equality.

### Task 3 · A colour for done, and stopped versus in progress restored

**Delivers:** finished work gets its own arrow colour, distinct from in-progress; and the separation
between stopped and in-progress reads as clearly as it did in the approved mock, which is a
regression against the version that was approved rather than a new request.

**Finished when:** the states are distinguishable without consulting the legend, in light and in
dark, and the commit names what the mock did that the build lost.

**Sequencing:** after task 1 — colour is applied to whatever geometry exists, and the geometry
changes.

*Shape: Claude.* It is judged by eye against a mock; there is no assertion that can stand in for
looking.

### Task 4 · Arrowheads stop overlapping the date text

**Delivers:** the fix for the first of the two defects found on first render. A head is wider than
its ribbon and eats into the date column beside it. The text column starts at a fixed offset from
the ribbon's **centre**, which is right for the ribbon and wrong for the head.

**Finished when:** the three cases named in #780 — Beacon M4's `23 Mar 2026`, Tide M2's `6 Apr 2026`
clipped to `6 Apr`, and Reef M5's `11 Jun 2026 → 30 Jun 2026` clipped at the left — render whole at
1600px, and a test pins the relation between the head's half-width and the text offset so the next
head-width change cannot silently re-break it.

*Shape: model.* A named set of failing cases, a named cause, a named correct output.

### Task 5 · Balloons attach at the end of the arrow, and their content is decided

**Delivers:** two halves that are easy to mistake for one. The geometry — a balloon attaches at the
**end** of the arrow, not the middle and not elsewhere on the ribbon, because where the arrow ends
is what the balloon is talking about. And the content — what a balloon actually says is a design
decision to be worked and recorded, not a field mapping picked from whatever the manifest happens to
carry. M2.1 shipped a field mapping (`title` for a next step, the workstream `gate` for a feature
still in the stages) and the owner has asked for the question to be worked properly: what would
genuinely be presented back to him and be understandable.

**Finished when:** every balloon's tail lands at an arrow's end; the rules M2.1 established still
hold (no next step means no balloon; fixed width, growing downward, never into a neighbouring
column; the connector stays inside its own column; placement is per feature, not one global pass);
and the content decision is written into this record rather than left in the code.

**Sequencing:** after task 1. Which end a balloon attaches to is undefined until the arrows overlay,
and it is the open question below.

*Shape: Claude.* The content half is a design judgement with no source to copy from.

### Task 6 · The header block reduces to the title alone

**Delivers:** the explanatory paragraph, the drag/order instruction paragraph and the
back-to-the-generated-order button all leave the header block. The page opens on the chart.

**Finished when:** the header is the title; and the two things that block was carrying have honest
homes rather than being dropped — the per-device caveat (*the order is remembered on this device
only*) is stated somewhere a reader will meet it, and the reset control sits beside the feature
headers where the ordering actually happens.

*Shape: model,* with the exception of choosing where the caveat goes.

### Task 7 · Records becomes Library

**Delivers:** the rename, everywhere it appears — page title, nav label, route wording.

**Finished when:** no surface says "Records", and a decision is recorded either way on whether the
old route redirects or simply stops existing. Links to it exist in the owner's browser history and
possibly in issues; a 404 is a legitimate answer, but it should be a chosen one.

*Shape: model,* once the redirect decision is made.

### Task 8 · The Triage page becomes a modal

**Delivers:** clicking a feature's header on the feature planning page opens a modal carrying what
that page carried — what needs you, the position, the gate. The standalone page goes.

**Finished when:** no standalone triage route remains; the modal is dismissible by keyboard, traps
focus while open and returns focus to the header that opened it; and the phone view is untouched.

**Sequencing:** independent of the arrow work.

**The collision, stated rather than left for the reader:** this touches **decision 22**, *three
purpose-built surfaces, not one responsive layout*, whose reasoning was that planning at a desk and
glancing on a phone are different activities. A modal on the desktop page serves the desk. It does
not obviously serve the phone, and #780 leaves that unsettled. This milestone therefore narrows
decision 22 rather than overturning it — triage on the desk becomes a modal, and **the phone keeps
its own surface** — and records the phone question as open below. Removing the phone view on a
reading nobody confirmed would be the expensive mistake here.

### Task 9 · Hide a feature, and bring it back

**Delivers:** a way to take a column off the page and restore it later, per device in
`localStorage`, exactly as the ordering already is.

**Finished when:** a hidden feature is recoverable **without knowing it is hidden** — meaning the
page always says, persistently and visibly, that features are hidden and how many, and restoring
them does not require remembering which ones they were. A page that silently omits a workstream is
worse than one that shows too many, and that is the whole risk of this task. Plus the same hostile
input floor M2.1 set for the ordering: corrupt JSON, a non-array, duplicates, stale slugs, a
throwing accessor and a 10,000-entry array all return every feature exactly once without throwing;
every read and write wrapped in try/catch; and the whole thing keyboard accessible, because drag and
click alone are not enough.

*Shape: Claude.* "Recoverable without knowing it is hidden" is a design constraint, not a spec.

### Task 10 · The phone view stops inventing a next milestone

**Delivers:** the fix for the second defect found on first render. A feature with four milestones,
all done and nothing recorded beyond, correctly gets no balloon on the chart — and the phone view
says **"Next: M5"**. That is `state.json` deriving a milestone from `headAt`/`tipLabel` that no
record supports. An earlier review flagged it as a data-semantics minor; it is now confirmed as
user-visible, with the two surfaces disagreeing in front of the reader.

**Finished when:** a feature whose last recorded milestone is done has no next milestone in
`state.json`, the phone view says nothing rather than naming one, and a test asserts the chart and
the phone view agree on that case — because the defect is not the label, it is the two surfaces
being computed from different readings of the same field.

*Shape: model.* A named wrong output, a named cause, a named correct output.

### Task 11 · The impeccable pass

**Delivers:** the craft pass that was owed when the page was first built. `AGENTS.md` requires the
project-local Impeccable skill before changing a page or screen; the page was built without it.

**Finished when:** the skill's own bounded verification has been run rather than an open-ended polish
loop — build fully, inspect once with a batched round covering desktop and mobile together in both
themes, fix everything that round shows in one batch, confirm with at most one more round, stop. The
surface's mode is **Operate**: the owner is completing a task, so scanability and consistency
outrank expression, and the brand lives in precise details.

**Sequencing:** after tasks 1–10. Running it earlier means running it against geometry that is about
to change.

### Task 12 · The promotion path stops being folklore

**Delivers:** a written, findable procedure for promoting an idea from `docs/design` onto the sheet.
Today it is two manual steps — write `docs/features/<slug>/workstream.json`, add the slug to
`atlas.config.json` — and nothing records that it happened or prompts for it.

**Finished when:** the path is documented where the next person will look rather than remembered,
and — proposed rather than assumed — Atlas warns when a `docs/features/<slug>/` directory exists
that `atlas.config.json` does not name. The reverse case already fails the build: a config naming a
directory with no manifest is a broken reference under decision 32. The missing half is the feature
that exists and is not on the page, which is silent today and is the same failure shape as a hidden
feature nobody can find.

### Task 13 · A refresh button that rebuilds the site — *blocked, not scheduled*

**Delivers:** what the owner actually asked for under #780's item 4, once corrected: not
live-updating data, but a button that **rebuilds the site**, so he stops waiting on CI to notice. It
keeps decision 37 completely intact — the repository stays the only source of truth, the page is
still rendered from records, nothing is held anywhere. Mechanically it is cheap: the workflow already
accepts `workflow_dispatch`, so it is a `POST /api/refresh` gated on the `author` role, firing the
workflow through the GitHub App that decision 36 already requires. No new credential and no new trust
boundary.

**Why it is not started:** #780 records that decision 35 lists *triggering work* among the things
that belong elsewhere, and that asking the site to re-read its own records is close enough to that
line to be settled in #784 rather than slipped in. #784 has since established that decision 35's
justification is false — Platform Operations deals with the release process and nothing before it —
but the scope has not been re-decided on correct grounds. New decision 57 records the withdrawal;
this task waits for the replacement.

---

## Still open at the end of this plan

- **Which arrow's end a balloon attaches to.** The owner's instruction is that a balloon attaches at
  the end of the arrow. Once the arrows overlay there are two ends. The honest reading is the
  **solid** arrow's end, because that is where the work has reached and the balloon speaks about what
  is next — but an earlier comment on #780 placed Keystone's balloon beside M1, which is a row below
  where its solid arrow ends. Both readings are on the issue and they do not agree. Task 5 cannot be
  called finished without this settled.
- **Does the phone keep a surface of its own, or does the modal serve both?** #780 asks it and does
  not answer it. Task 8 assumes the phone keeps its surface, which is the reversible choice.
- **What a balloon says.** Deliberately left open: the owner asked for the question to be worked
  through the process documentation rather than answered from the manifest's field list.
- **The refresh button's scope**, pending #784.
- **Whether the old Records route redirects or 404s.**

## Still open on #780 and deliberately not in this milestone

These remain on the issue. None of them is page work, and folding them in would make this milestone
about something else.

- **`docs/work-packages/` is ~370 retired-era files and every one renders.** Whether Atlas should
  read that directory at all is the owner's call.
- **No `workstream:*` labels exist in GitHub**; 46 of 52 open issues carry none. This is M6's first
  task, because M6 cannot start without it.
- **GitHub task lists render as literal `[x]`.** Decision 11 does not enumerate them. It becomes
  urgent only if M6 decides sub-tasks are checkboxes rather than sub-issues.
- **Decision 7 says Microsoft and the only real deployment signs in with GitHub.** M2's record calls
  decision 7 arguably the stale half. Still unreconciled.
- **Menus' eleven milestones all point at the shared `milestone-plan.md`**, and two acceptance
  records are missing.

## Deliberately excluded

- **Any schema change.** M2.1 added `started` and `completed`; this milestone adds no field. Every
  item above is drawing, behaviour or naming.
- **The register, tasks and assignment** — M5 and M6.
- **A drag library, or any new runtime dependency.** M2.1 held the dependency list to
  `@11ty/eleventy` and `markdown-it` and asserted it by test; hiding and restoring is pointer events
  and `localStorage`, same as the ordering.
- **Cross-device ordering or hiding.** Both stay device-local. Making them follow the owner between
  his PC and his phone is a write, and a write into per-device preference is not one of decision 35's
  two endpoints.

## Decisions this milestone rests on and touches

New decisions **48** (the two arrows are one object), **49** (per-device state may hide a feature but
never silently) and **50** (triage is a modal, narrowing decision 22) are created by this approval and
recorded in `docs/design/approved/atlas/decisions.md`. It also depends on **22–25** (the surfaces and
the chart's rules), **28** (Sky tokens and three-state dark), **29** (`state.json`, which task 10
corrects), **32** (fail loudly) and **37** (task 13's constraint).
