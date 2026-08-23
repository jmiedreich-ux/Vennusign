# Atlas Milestone 6 — Tasks are GitHub issues, rendered

> **This is a plan for work not yet done.** It is written before the work rather than after it, so
> the site can render what is coming; future tense throughout is deliberate.

**Goal:** A milestone shows the tasks inside it, and those tasks are the GitHub issues that already
exist rather than a second list somebody keeps.

**Where it will land:** the `Atlas` repository for the rendering, and this repository for the labels
and the issues themselves — which, unusually, is the larger half.

**Spec:** the owner's framing, which decides the whole milestone; GitHub issue #780 item 3, which
this supersedes; and decision 26, which described a milestone grid three milestones ago and has
never been built.

---

## The framing, which decides everything below

The owner's words: Atlas is **"a simplified easy view over GitHub."**

So milestone tasks are **not** a new shape in `workstream.json`. #780's item 3 asked for a task list
in each milestone and noted that "milestones currently carry no tasks in the manifest, so this needs
a shape in `workstream.json` and a record behind it, like everything else." That reading is wrong,
and it is wrong in an instructive way: it treated *record* as a synonym for *file in this
repository*. An issue is a record. Atlas already fetches them — `fetchProjectIssues` has bucketed
them by workstream label since M1 — and duplicating them into a manifest would create precisely the
hand-maintained twin that decision 1 exists to prevent.

**The division, stated once so it does not have to be re-derived:**

| Lives in records | Lives in GitHub |
|---|---|
| Milestones | Tasks |
| Gates | Sub-tasks |
| Position | Assignees |
| Questions, their options and which is recommended | — |

The last row is the load-bearing one, and it is why M5 goes the other way. **GitHub cannot express a
recommendation.** An issue can hold a question and a discussion, but there is no field for *these
are the options, this one is recommended, this one was chosen, and it was written in rather than
offered*. So questions stay records, and tasks become issues, and the difference between them is not
taste — it is what each system can actually hold.

---

## What has to happen before any of it: the labels do not exist

`workstream:*` is a namespace nobody has created. **46 of 52 open issues carry no label at all**,
which M2's record already flags as an unresolved finding: issues are collected but not bucketed.

This has a consequence for the three plan records that carry task lists — M4, M5 and this one.
**Those lists cannot become GitHub issues yet.** An issue created today would be unbucketed and
therefore invisible to Atlas, which is a worse outcome than a prose list, because a prose list is at
least findable in the record it belongs to. So the task lists live in the plan records until this
milestone lands the labels and the rendering, and then they migrate. That is a sequencing fact, not
an oversight, and it is stated here so nobody reads those lists as a failure to follow this
milestone's own rule.

Task 1 below is creating the namespace, for that reason.

---

## Task list

Six tasks. Task 1 unblocks everything, including work outside this milestone.

Shapes are noted where clear: a **Claude-shaped** task can name the design and rely on the reader
working the rest out; a **model-shaped** task states what it is given, what it must produce and what
must be true afterwards. Task 6 is where that distinction stops being a note in a plan and becomes a
rule the repository follows.

### Task 1 · Create the `workstream:*` label namespace

**Delivers:** one label per workstream, matching the `label` field each of the seven manifests
already declares — `workstream:menus`, `workstream:atlas`, `workstream:keystone` and so on — and the
existing open issues labelled.

**Finished when:** every manifest's declared label exists in GitHub; no manifest declares a label
that does not exist, asserted by the build rather than by inspection; and the issue panels on the
site stop being empty. An issue may legitimately carry two workstream labels and appear under both —
M1's fetch was built for that case deliberately, because the roadmap places some issues in two
clusters.

**Sequencing:** first, and it blocks the entire milestone. It also unblocks migrating M4's and M5's
task lists into issues.

*Shape: model.* A named list of labels from a named set of files, applied.

### Task 2 · Attribute an issue to a milestone, not only a workstream

**Delivers:** the missing link. `fetchProjectIssues` buckets by workstream label; a task row inside a
milestone needs to know which milestone an issue belongs to, and nothing carries that today.

**Finished when:** an issue resolves to exactly one `(workstream, milestone)` pair; an issue naming a
milestone the manifest does not have **fails the build by name**, as every other broken reference
does under decision 32; and an issue with a workstream but no milestone is still collected rather
than dropped — M1 established that principle for unlabelled issues and it holds here for the same
reason.

**The choice, which is open:** GitHub's own milestones, or a `milestone:<id>` label. GitHub's
milestones are the native fit and carry due dates and progress for free. Against them: the milestone
namespace is flat and repository-wide, while ids repeat across workstreams by design — decision 20
says every workstream numbers its own milestones, so seven features all have an `M1`. That needs a
naming rule (`atlas M2.1`) which is a convention nobody can enforce, whereas a label namespace can be
validated the same way task 1's is. The label is the safer answer and the milestone is the more
honest one; this is a decision to take, not a coin to flip.

### Task 3 · The task row

**Delivers:** the rendering. A **horizontally scrolling row across the top of a milestone**, one
column per task, with that task's sub-tasks inside it.

**Finished when:** the row scrolls inside its own container and the page never scrolls sideways;
state comes through without needing a legend, the way the chart's ribbons do; and a milestone with no
issues renders **no row at all** rather than an empty one — the same positional rule M2.1 settled for
the chart, where nothing is drawn that no record supports.

**Sequencing:** after tasks 1 and 2. There is nothing to render before them.

**What this does to decision 26.** Decision 26 describes a desktop milestone grid: *tasks as columns,
the per-task cycle as rows, with the milestone-level phases as a strip above and below*. This
fulfils the first half. The per-task cycle as rows is **not** what has been asked for now, and it is
recorded here as not built rather than quietly dropped, so that anyone reading decision 26 later
finds out from this record why the surface looks different from what it describes.

### Task 4 · Decide what carries a sub-task

**Delivers:** a decision between GitHub's native sub-issues and task-list checkboxes in an issue body,
and the rendering that follows from it.

**Finished when:** the choice is recorded with its reason, and sub-tasks render inside their task.

**The complication worth naming:** if sub-tasks are checkboxes, then **#780's open defect that GitHub
task lists render as literal `[x]` text becomes this milestone's problem** and has to be fixed first.
It is currently deferred on the grounds that decision 11 does not enumerate task lists. It is the same
question wearing a different hat, and choosing sub-issues makes it go away — which is an argument for
sub-issues that has nothing to do with sub-issues being better.

### Task 5 · Assignment: people are assignees, models are labels

**Delivers:** who owns a task, rendered.

- **Human assignees stay GitHub assignees.** They already work, they already have avatars, and
  nothing is gained by inventing a parallel notion.
- **Models are a label namespace, `agent:<name>`** — because a local model cannot hold a GitHub
  account, and the alternative is a fictional user or a convention hidden in the issue body.

**Finished when:** an issue's assignees and its `agent:` label both render on its task card; the set
of known agent names is closed and lives somewhere the build reads (`atlas.config.json` is the
proposal, since it is already the project's declaration of what exists); an unknown agent name fails
the build by name; and **an issue carrying two `agent:` labels fails the build**, as every other
closed vocabulary does under decision 32.

That last rule is deliberately **unlike** `workstream:*`, where two labels are legitimate and M1's
fetch handles them on purpose. Two workstreams on one issue means the work touches both, which is
true and useful. Two agents on one task means nobody knows who is doing it, which is not information
— it is an unanswered question wearing the costume of one.

### Task 6 · Write the sub-task shape down

**Delivers:** a stated shape for a sub-task — **what it is given, what it must produce, and what must
be true afterwards** — documented where an issue author will meet it, which means an issue template
rather than a page nobody opens.

**Why this survives:** it was originally justified by a dispatcher that would consume it, and that
dispatcher is not being built (see below). The reason it still matters is task 5. Once a task can be
assigned to a model, the way it is written stops being style and starts being whether it can be done
at all. A task written for Claude can say *"resolve the tenant from the path"* and rely on the reader
working it out from the design authority, the surrounding code and three prior conversations. A local
model has none of that: it needs what it is given, what it must produce, and what must be true
afterwards, stated. So the `agent:` label and the sub-task shape are two halves of one thing —
assigning work to a model you have not written a task for is assigning it to fail.

**Finished when:** the shape is documented, one real milestone's tasks are rewritten to it, and the
difference is visible — a reader can tell which tasks were written for a model and which were not,
without being told.

**Sequencing:** after task 5, and it is the task most likely to be skipped, because nothing breaks
if it is.

*Shape: Claude.* It is a writing standard, judged by whether the tasks written to it turn out to be
doable.

---

## Still open — the planner, which is a question and not a milestone

The owner raised, and has **deferred**, an AI planner: something that watches `state.json` on a
schedule and **notices** things.

- A milestone that has been open far longer than its siblings.
- A gate that has not moved.
- An authority sitting in `docs/design/proposed/` that is blocking milestones.
- Records that contradict `ROADMAP.md`.

It would be the first real consumer of decision 29's `state.json`, which exists precisely so an agent
reads one guaranteed-current file rather than six.

**And the constraint that makes it safe, which is the whole of why it is worth recording:** it may
**observe and ask. It may never decide.** What it produces is a question written into a register —
which is what write-back exists to answer, and what M5 gives a shape to. If it writes plans rather
than questions, the site stops being generated from records and starts being generated from a model's
opinion, which is decision 1 inverted. That is not a slippery slope argument; it is the same failure
as every hand-maintained twin in this repository, with a faster author.

It also connects to M5's own open question — *where do questions come from* — since a planner would
be the first thing that generates one, and today nothing does.

**It is not planned, and there is no milestone for it.** The owner's current answer is that he
triggers work verbally, the way he does today, and that is a deliberate choice rather than a gap
waiting to be filled. Recorded here so the reasoning survives if he changes his mind.

## Deliberately excluded

- **Dispatch to local models.** There is no scheduled runner and no dispatch machinery. Marking who
  owns a task is not sending it to them, and sending it is not being built: the owner keeps
  triggering work verbally.
- **A task shape in `workstream.json`.** The framing at the top of this record is the reason, and it
  is the single most important thing to get right here.
- **Writing issues from Atlas.** Creating an issue is a write, and it is one decision 35 excluded. New
  decision 57 records that decision 35's justification has been withdrawn, but the scope has not been
  re-decided, and this milestone renders issues rather than authoring them.
- **The per-task cycle as rows** — decision 26's second half, task 3.

## Decisions this milestone rests on and creates

It creates **53** (tasks are GitHub issues, not manifest data), **54** (people are assignees, models
are an `agent:<name>` label), **55** (a sub-task states what it is given, what it must produce and
what must be true afterwards) and **56** (a planner may observe and ask, never decide — deferred). It
rests on **1** (built, never maintained — the reason a task list in a manifest was refused), **20**
(every workstream numbers its own milestones, which is task 2's whole complication), **26** (the
milestone grid it half-fulfils), **29** (`state.json`) and **32** (fail loudly on a closed
vocabulary, which tasks 2 and 5 both lean on).
