# Atlas Milestone 5 — The register becomes data

> **This is a plan for work not yet done.** It is written before the work rather than after it, so
> the site can render what is coming; future tense throughout is deliberate.

**Goal:** A question register stops being prose and becomes a record with a shape, and the readable
document is generated from it.

**Where it will land:** mostly the `Atlas` repository — the register's contract sits beside the
manifest contract in `src/schema.mjs`, and the generated document is build output. Vennusign's side
is the registers themselves, which are content, and whatever migration task 6 decides on.

**Spec:** GitHub issue jmiedreich-ux/Vennusign#784 and its comment on the register; decision 3 (*no
artifact without a generator*), which this milestone is an application of rather than an exception
to; and the two registers that actually exist — `docs/features/menus/open-questions.md`, which is
the corpus this shape has to survive contact with.

---

## Why this is structural and not a UI change

**The register is prose, and the answering pattern is not.** Every register in practice has offered
**multiple choice with a recommendation**, and the owner routinely **adds choices of his own** that
were not on the list. So an answer is neither free text nor a pick from a fixed set. It is *choose
one of these, or supply one that should have been there*, and the supplied ones carry as much weight
as the offered ones — more, in the sense that matters, because they are the ones the question failed
to anticipate.

For that to work, a register has to hold, per question: the options, which one is recommended, which
one was chosen, and whether the chosen one was **written in** rather than offered.
`open-questions.md` holds none of that. It is prose, and `POST /api/answer` writes prose into it.

**The precedent is already on the record.** Keystone's `open-questions-workbook.html` was a
hand-built multiple-choice workbook; it drifted out of sync with its register **within a day**, and
that drift is what produced decision 3. So the workbook shape was right and the hand-building was
what was wrong. The answer is not to stop building workbooks — it is to generate them, from a
register that is data.

---

## Task list

Six tasks. Tasks 1 and 2 are the milestone; 5 is what makes it more than a refactor; 6 is the one
that decides what happens to two years of existing questions.

Shapes are noted where clear, on the same distinction M6 sets out: a **Claude-shaped** task can
state the intent and rely on the reader working it out; a **model-shaped** task states what it is
given, what it must produce and what must be true afterwards.

### Task 1 · The register's contract

**Delivers:** a validated shape for a question, beside the manifest contract in `src/schema.mjs`,
carrying per question:

| Field | What it holds |
|---|---|
| `id` | The durable identifier — `Q1`, `Q209`. Never reused, never renumbered, for the same reason decision 17 preserves milestone ids. |
| `question` | The question itself, as asked. |
| `why` | Why it was asked — what could not be settled on the spot, and what breaks if it is guessed wrong. |
| `options` | The offered choices. |
| `recommended` | Which option is recommended. Exactly one. |
| `chosen` | Which option was chosen, or null while it is open. |
| `chosenWasOffered` | Whether the chosen option was on the list or written in. |

**Finished when:** the schema rejects an unknown value by name rather than rendering a blank
(decision 32); a recommendation that names an option that does not exist fails; a written-in answer
validates without having to be smuggled in as an option that was never offered; and a hand-written
fixture register carrying at least one write-in and at least one still-open question validates.

**What the shape must also survive, or the migration loses information.** The registers that exist
carry three things the list above does not, and they are not decoration:

- **Severity** — `BLOCKING`, *important*, *minor*. Menus uses it to decide what a slice can start
  without.
- **`defer` as a third valid answer.** Menus' register states it outright: accept the recommendation,
  answer your own way, or decide later; deferrals stay tracked, and an unanswered question is treated
  as a deferral rather than as silent acceptance. A shape with only *chosen* and *not chosen* cannot
  express that, and would quietly convert every deferral into an unanswered question.
- **Citations.** `<sub>` footnotes naming the files and artifacts a question was raised from —
  decision 11 exists partly to keep them rendering.

*Shape: model.* A contract, a fixture and a test.

### Task 2 · The document is generated from the register

**Delivers:** the readable register — the thing that is `open-questions.md` today — becomes build
output rendered from task 1's data, not a file anybody edits.

**Finished when:** the generated document shows, for every question, the options, the recommendation,
the chosen answer and whether it was written in; the register renders on the site the way the corpus
does; and a hand edit to the generated file is lost on the next build. That last one is the point
rather than a side effect — decision 3 is only true if the generator wins.

**Sequencing:** after task 1.

### Task 3 · The write-in answers are surfaced, not buried

**Delivers:** the write-ins made visible as a group, on the register's own page — because they are
the ones the question failed to anticipate, and a register whose answers are mostly write-ins is a
register whose questions were guessing.

**Finished when:** a reader can see how many answers were written in and read them together, without
scrolling a two-hundred-question document looking for them.

*Shape: Claude.* What "surfaced" looks like is a design judgement.

### Task 4 · An index of registers, built from the files

**Delivers:** registers need finding when there are many. An index that is **built from the presence
of the files themselves**, discovered under `docs/features/<slug>/`, ordered by **what is waiting**
rather than by name.

**Finished when:** adding a register makes it appear with no list edited anywhere; deleting one makes
it vanish; a feature with no register is absent rather than shown empty; and the ordering is by open
and blocking count, so the register with the owner's attention in it is first. An index assembled
from a hand-maintained list would be a second truth about which registers exist, which is exactly
what decision 1 exists to prevent.

*Shape: model.* Given a directory convention, produce an ordered index; true afterwards when no list
of registers exists in any source file.

### Task 5 · `POST /api/answer` writes the record, not the document

**Delivers:** M3's write endpoint retargeted. Today it applies a minimal text edit to
`open-questions.md` — reading the file and its SHA, placing the answer between marker comments, and
`PUT`ting it back. After task 2 that file is **generated output**, so a write into it is overwritten
by the next build: the answer would appear, then vanish, which is worse than not accepting it.

**Finished when:** an answer commits to the structured register; the rebuild regenerates the
document from it; and every guard M3 paid for still applies — markup refused flatly rather than
sanitised (the C1 stored-XSS finding), setext headings refused, the block placed between one open
marker and *its own* close, the repository pinned, and a stale SHA answered with a 409 rather than an
overwrite. The guards now have to cover a **written-in option** as well as an answer, because a
write-in is caller-supplied text that lands in a record and renders for every reader — the same
exposure by a different door.

**Sequencing:** after tasks 1 and 2. This is the task that turns M5 from a refactor into a
capability: without it the register is structured and still unanswerable.

### Task 6 · Migrate the existing registers, or decide not to

**Delivers:** a decision, with its cost named, about the registers that already exist. Menus' is 209
questions across four sittings, with severities, deferrals, provisional defaults flagged into
acceptance workbooks, owner deviations called out by number, and `<sub>` citations throughout. It is
the largest single record in the corpus and it is genuinely load-bearing.

**Finished when:** either it is converted with nothing lost — every severity, every deferral, every
citation, every "accepted recommended" and every owner deviation surviving the round trip — or the
decision to leave historical registers as prose is recorded, along with what that costs: two shapes
of register in the repository, and an index (task 4) that has to render both.

*Shape: Claude.* It is a judgement about a real corpus, and the wrong answer is expensive in both
directions.

---

## Still open

- **Where questions come from.** Today an agent writes them during a design conversation, when
  something cannot be settled on the spot. Nothing generates them, and Atlas does not know a question
  is a question — it renders the file like any other record. #784 names this and does not settle it:
  if the register becomes structured, that is also the moment to decide whether question-writing
  stays a hand act or gains a shape of its own. This plan does not decide it, and task 1's contract
  is deliberately about the question's *content* rather than its *origin* so that either answer still
  fits.
- **Whether the answering surface belongs in this milestone.** #784 is explicit that multiple choice
  is a structural change and not a UI one, so this plan scopes M5 to the data, the generated document
  and the index. But M3 shipped write-back with no interface, and a structured register with no form
  to answer it from leaves that still true. The answering surface is the obvious next thing and is
  not assumed here.
- **Whether the generated document keeps the name `open-questions.md`.** Generating a file at the
  same path an author used to edit is the most confusable outcome available; a different name, or a
  different directory, is worth considering.

## Deliberately excluded

- **Tasks, assignment and issue rendering** — M6.
- **Any change to how decisions are recorded.** Approved authorities under `docs/design/approved/`
  stay Markdown that a human wrote. A register is a form being filled in; an authority is prose being
  argued. Only the first is data.
- **Backfilling a `why` onto questions that never had one.** Inventing the reason a question was
  asked is exactly the drift this milestone exists to stop.

## Decisions this milestone rests on and creates

It creates **51** (a register is structured data and its document is generated from it) and **52**
(the register index is built from the files it indexes). It rests on **3** (no artifact without a
generator, which it is a direct application of), **2** (Atlas is never the record — the register data
is the record and the page is a rendering of it), **11** (the corpus renders as GitHub renders it,
which is why citations must survive), **32** (fail loudly on a closed vocabulary) and **37** (a write
lands as a commit to the record, which task 5 is the reason to restate).
