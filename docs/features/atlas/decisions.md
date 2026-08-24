# Atlas — decisions on record

Status: **approved by the owner, 2026-08-22.** Settled across the brainstorming sittings of
2026-08-21/22 and landed here from `docs/design/proposed/`. This document now governs Atlas
implementation: per `AGENTS.md`, where any other document disagrees with an approved authority, the
authority wins.

**Amended 2026-08-22 with decisions 48–57**, approved by the owner across three areas of future
work: the second pass on the feature planning page (Milestone 4), the register becoming data
(Milestone 5), and tasks becoming GitHub issues (Milestone 6). Where an amendment contradicts an
earlier decision, the amendment says so in its own text rather than leaving two decisions quietly
disagreeing.

**Amended 2026-08-24 with decisions 58–60** (Milestone 9): decision 35 retired on decision 57's
own grounds, the `approve` capability it names shipped, and the split between an approved design
authority and a feature's own tracking (decision 40's `docs/design/` reference) retired in favour
of one directory per feature.

Atlas is the always-current internal site for the Vennusign project. Decisions are numbered and
written as rules. Where any other document disagrees with this one, this one wins. Open questions
live in `docs/features/atlas/open-questions.md` and are never resolved silently.

---

## What Atlas is

**1 · Atlas is built from the repository and GitHub, never maintained.** The site is regenerated
from `master` and the GitHub API; no page is hand-authored. This is the whole point: a site that is
*kept in sync* drifts, and every hand-maintained twin in this repository already has. A site that is
*built from* the source cannot be behind it.

**2 · Atlas is never the record.** It renders records that live elsewhere. Where Atlas and a record
disagree, the record is right and Atlas is broken. Nothing is true because Atlas displays it.

**3 · No artifact without a generator.** Any page, export or view that cannot be regenerated from
source does not ship. A hand-written page is a second truth with a shorter half-life than the
conversation that produced it.

**4 · Atlas is for the owner, on a desk and on a phone.** Not for agents: agents read the repository
directly, because a rendered page is a worse input than the Markdown it came from. The one exception
is decision 29.

---

## Hosting and access

**5 · Azure Static Web Apps, Free tier, at `atlas.vennusign.com`.** A **deliberate exception** to the
repository's one-App-Service-per-app convention, recorded as an exception rather than a precedent.
It is the only option where answering from a phone (decisions 34–37) does not become a second
project, because managed Functions ship in the same deployable behind the same auth.

**6 · Atlas stays off the `appsrv-basic-web` B1 plan.** That plan already carries 28 apps on one
worker, seventeen of them idle, and restarts them all together (#748). Atlas adds nothing to it.

**7 · Nothing is anonymous.** SWA's built-in Microsoft provider with role invitations; every route
requires a role. The Free tier's 25-invitation ceiling is accepted as sufficient.

**8 · Atlas is environment-independent.** One instance, like `qa.vennusign.com`. Internal records are
not versioned per environment, so `dev.`/`stage.`/`app.` would be three copies of one truth.

---

## Generator

**9 · Eleventy with markdown-it, on Node 22.** Node 22 is the only JavaScript runtime CI uses;
markdown-it reproduces how GitHub renders the corpus today, which matters because 609 Markdown files
already render correctly there and none carry frontmatter.

**10 · Standalone HTML is copied byte-for-byte, never templated.** Thirty-two `.html` files under
`docs/` are complete documents with their own styles and scripts, ten of them loading a sibling
`support.js`. A generator that runs them through a template engine mangles them.

**11 · The corpus renders as GitHub renders it.** Two- and three-space nested lists, inline HTML,
`<sub>` citations, pipe tables. Atlas adapts to the records; the records do not adapt to Atlas.

**12 · No framework runtime and no second hosting model.** Eleventy emits static files. Anything that
needs a server belongs in decisions 34–37 or nowhere.

---

## Content model

**13 · The spine is roadmap → workstream → milestone.** Every page hangs off that.

**14 · Position lives in a manifest, not in prose.** Each workstream carries
`docs/features/<workstream>/workstream.json`:

```json
{ "codename": "Keystone",
  "what": "Progressive-cutover thin layer",
  "stage": "planned",
  "position": "Designed, not approved",
  "gate": "Owner approval of the authority; tier-and-cost before deployment",
  "label": "workstream:keystone",
  "design": [{ "name": "cutover/Cutover Architecture v2", "where": "design-project" }],
  "milestones": [
    { "id": "M1", "label": "M1", "depth": 1, "title": "TenantContext contract and library",
      "status": "next", "plan": "m1-plan.md", "issue": null, "pr": null,
      "acceptance": { "kind": "demo-script", "record": null } }
  ] }
```

**15 · Markdown is the authority for content; the manifest is the authority for position.** Decisions,
registers, plans, handoff and procedure render from the files they already are. The manifest says
only where a thing stands and where it lives.

**16 · The roadmap's tables become generated.** `ROADMAP.md` keeps its narrative — the eras, why a
workstream is paused, the design queue — and its workstream and milestone tables become output. Prose
that duplicates a manifest is prose that will drift.

**17 · Historical milestone ids are preserved; only display labels normalise.** Twenty-one Menus files
carry ids in their names and are durable acceptance evidence cited in merged PRs. The manifest
carries both `id` (unchanged, forever) and `label` (normalised for display). Nothing is renamed.

**18 · Going forward, a milestone is `M<n>`, and its parts are `M<n>.1`, `M<n>.2`.** Never a letter,
never a bare suffix. Menus' irregular history stays a historical artifact rather than a convention
anyone must learn.

**19 · `M` always means Milestone.** Spell "Milestone 6.3" in headings and reserve the bare `M6.3` for
chips and cells, because a workstream named *Menus* beside ids `M1`–`M6` invites the misreading.

**20 · Not every workstream has the same number of milestones.** The ladder is the union of all of
them; depth rows are numbered positions, and each column shows its own ids. No workstream's numbering
is imposed on another.

**21 · The Claude Design project and Google Drive are named links, never rendered.** CI cannot reach
either, and "anything only in Design is by definition not approved" is already the rule.

---

## Surfaces

**22 · Three purpose-built surfaces, not one responsive layout.** Planning at a desk and glancing on a
phone are different activities, and a squeezed desktop layout serves neither.

**23 · Desktop · project depth.** Workstreams as columns; one shared ladder down. Three stages sit
**above** the first milestone — *Not started · Designing · Planned* — because that is where most
workstreams live, and a chart starting at M1 would show six of seven as empty.

**24 · The bar covers what is complete; the head points at what is next.** Both ends come from the
manifest, so the chart cannot disagree with the records.

**25 · Every column carries a note at its tip saying why it stopped there.** "Ten deep, then stopped."
"Blocking Menus." A depth without a reason is a number nobody can act on.

**26 · Desktop · milestone grid.** Inside one milestone: tasks as columns, the per-task cycle as rows,
with the milestone-level phases as a strip above and below. It answers which task is at which step.

**27 · Mobile is sorted by what needs the owner**, not alphabetically: one card for the decision
waiting on them, then moving, blocked, designing, not started. Depth is a filled track; the gate is
always the last line, because on a phone the question is never "how far" but "why has it stopped".

**28 · Look: Sky UI tokens, Segoe UI typography.** The product's own locked palette, so Atlas reads as
a sibling of the back office and Platform Operations. Segoe needs no webfont. Three-state dark mode —
bare `:root`, a `prefers-color-scheme` block guarded against an explicit light choice, and a
`[data-theme="dark"]` block.

**29 · The build emits `state.json` beside the pages.** The same data, machine-readable. This is the
one thing Atlas offers agents: a session's orientation read becomes one guaranteed-current file
instead of six.

---

## Build

**30 · Three triggers: push to `master`, manual dispatch, and a six-hourly schedule.** Issues change
without merges, so build-on-merge alone would show stale issue panels for days.

**31 · Its own workflow, with no `environment:` gate.** A documentation merge must never cost a deploy
approval.

**32 · The build fails loudly on a broken reference.** A manifest pointing at a missing plan, a
milestone with no title, a workstream with no gate — all fail the build rather than rendering a blank
cell. This is what makes decision 1 structural instead of aspirational.

**33 · `src/atlas/**` is added to `scripts/ci/classify-changes.sh`'s allow-list as a no-deploy class**
before the first Atlas commit. Any path outside that list trips the fail-safe and redeploys all five
applications.

---

## Write-back — Milestone 2, not Milestone 1

**34 · Atlas is read-only in Milestone 1.** Everything actionable links out to the file or the issue.

**35 · Milestone 2 writes register answers and acceptance results. Nothing else.** Creating issues,
approving milestones and triggering work belong to Platform Operations; two consoles that both act is
how they diverge.

**36 · Writes go through a GitHub App, not `GITHUB_TOKEN`.** Pushes made with the Actions token do not
trigger workflows, so the site would never rebuild after its own write.

**37 · A write lands as a commit to the record, and the page is then rebuilt from it.** An answer
submitted on a phone becomes a commit to `open-questions.md`; the page reloaded afterwards is rendered
from that file. Atlas never keeps state of its own.

---

## Repository structure

**38 · Atlas is a generator, not a site that lives in one project.** A second project may use it, so
Atlas embedded in Vennusign would be a fork waiting to happen. It is a tool that any project
repository runs against itself.

**39 · The generator is its own repository, versioned, consumed as a composite GitHub Action.** One
line in a project's workflow: `uses: jmiedreich-ux/atlas@v1`. It holds the build, the layouts, the
theme and the manifest schema, and no project content whatsoever.

**40 · A project repository provides a fixed convention and nothing else.** `atlas.config.json` at the
root, `ROADMAP.md`, `docs/features/<workstream>/workstream.json`, the feature records beside it, and
the authorities under `docs/design/`. A project that follows the convention needs no code — only the
config and a workflow. **Amended by decision 60:** a design authority once approved lands under
`docs/features/<workstream>/` too, not a separate `docs/design/approved/` tree — only `proposed/`
work stays under `docs/design/`.

**41 · One site per project, never one site across projects.** Vennusign builds its own to
`atlas.vennusign.com`; a second project builds its own to its own host. No cross-repository reads, no
credentials to hold, and access control stays per project — one project's records must not be visible
to another's readers.

**42 · Documentation stays co-located with the project it describes.** Across repositories, "a change
that makes a record false updates it in the same commit" stops being enforceable. `docs/` needs
reorganising, not relocating.

**43 · Atlas builds from `master` and does not force a branch model.** The trunk-plus-`release/X.Y`
question belongs to release versioning (#754). Atlas works under either.

**44 · Atlas needs no standing agent.** Building it is CI, validating it is decision 32, and
developing it is ordinary milestone work under `docs/MILESTONE_EXECUTION.md`. A records steward that
maintains the manifests at milestone completion is a plausible future agent, but it is about the
process rather than Atlas, and it earns its place only with evidence that manifest upkeep actually
drifts.

---

## Prerequisites owned outside Atlas

**45 · Repository visibility.** The repository is public, so every record Atlas would render — tenant
identifiers, application registration ids, Key Vault names, unfiled security findings — is already
world-readable, and gating the rendered site protects nothing until that changes. *(Owner, 2026-08-22:
the repository will be made private.)*

**46 · The generator's release tags live in the generator's repository, not in any project's.**
`uses: <owner>/atlas@v1` resolves against the `atlas` repository: a `v1.0.0` tag on each release plus
a `v1` major tag moved forward to the newest compatible release, which is the GitHub Action
convention. Task 8 of Milestone 1 creates the first of them. A project consuming Atlas needs no tags
of its own.

**47 · Murphy cannot reach Atlas.** Static Web Apps role invitations are for people; there is no
non-interactive credential path, so the QA agent that tests every other deployed surface cannot smoke-
test this one. Either Atlas is exempt from Murphy, or the write-back work in decisions 34–37 supplies
a service identity. Not a Milestone 1 blocker; recorded so it is not discovered late.

---

## The feature planning page, second pass — Milestone 4

**48 · The two arrows are one object, overlaid — not two arrows end to end.** The faint arrow runs
the whole recorded length of a feature; the solid arrow is drawn **over** it, from the top, as far as
the work has actually reached. They are one object seen twice, which is the honest reading of the
earlier note that you cannot have the first arrow without the second. **This corrects what Milestone
2.1 shipped**, which tiles them — the solid arrow stops and the faint one starts below it — and it
corrects that milestone's own record, which describes the tiled behaviour as though it were right.
Keystone is the reference case: a faint arrow the full six milestones, with the solid one over the
stages on top of it.

**49 · Per-device state may hide a feature, but never silently.** Ordering and hiding both live in
`localStorage` and both stay device-local. A hidden feature must be **recoverable without knowing it
is hidden**: the page always says, persistently, that features are hidden and how many, and restoring
one does not require remembering which. A page that silently omits a workstream is worse than one
that shows too many, because the first is wrong and looks right. The same rule governs any future
per-device state that can remove something from view.

**50 · Triage is a modal on the feature planning page, not a page of its own.** Clicking a feature's
header opens a modal carrying what the standalone Triage page carried — what needs you, the position,
the gate — and the standalone page goes. **This narrows decision 22 rather than overturning it.**
Decision 22 says three purpose-built surfaces and not one responsive layout, on the grounds that
planning at a desk and glancing on a phone are different activities; the modal serves the desk, and
the phone keeps a surface of its own. Whether the phone should instead be served by the same modal is
raised in #780 and is **not settled here**; the phone view stays until it is, because keeping it is
the reversible choice.

---

## The register as data — Milestone 5

**51 · A question register is structured data, and its readable document is generated from it.** Per
question: an id, the question, why it was asked, its options, which one is recommended, which one was
chosen, and whether the chosen one was **offered or written in**. An answer is *choose one of these,
or supply one that should have been there*, and the written-in choices matter most, because they are
the ones the question failed to anticipate — so the flag that records them is signal, not
bookkeeping. This is decision 3 applied to the register rather than an exception to it: Keystone's
hand-built `open-questions-workbook.html` was the right shape built the wrong way, and it drifted from
its register within a day. That drift is what produced decision 3 in the first place. The consequence
is that `POST /api/answer` writes the record and not the document, since after this the document is
build output and a write into it would be overwritten by the next build.

**52 · The register index is built from the files it indexes.** Registers need finding once there are
many, and the index is ordered by **what is waiting** rather than by name. It is assembled from the
presence of the register files themselves, so adding one makes it appear and deleting one makes it
vanish, with no list edited anywhere. An index assembled from a hand-maintained list would be a second
truth about which registers exist — the failure decision 1 exists to prevent, in miniature.

---

## Tasks, assignment and observation — Milestone 6

**53 · Tasks are GitHub issues, not manifest data.** Atlas is *a simplified easy view over GitHub*, so
a milestone's tasks and sub-tasks are the issues Atlas already fetches, rendered — not a new shape in
`workstream.json`. **This supersedes #780's item 3**, which asked for a task list in each milestone
and assumed it needed a manifest shape and a record behind it. An issue *is* a record; copying issues
into a manifest would create the hand-maintained twin decision 1 exists to prevent. The division:
milestones, gates and position live in records; tasks and sub-tasks live in GitHub; **questions and
their options live in records, because GitHub cannot express a recommendation.** That last clause is
why decision 51 goes the other way, and it is a statement about what each system can hold rather than
a preference.

**54 · People are GitHub assignees; models are an `agent:<name>` label.** Human assignment already
works and nothing is gained by inventing a parallel notion. A local model cannot hold a GitHub
account, so it is a label namespace instead of a fictional user. **An issue carrying two `agent:`
labels fails the build by name**, as every other closed vocabulary does under decision 32. This is
deliberately unlike `workstream:*`, where two labels are legitimate and the fetch handles them on
purpose: two workstreams on one issue means the work touches both, which is true and useful, while
two agents on one task means nobody knows who is doing it — an unanswered question in the costume of
an answer.

**55 · A sub-task states what it is given, what it must produce, and what must be true afterwards.**
A task written for Claude can say *"resolve the tenant from the path"* and rely on the reader working
it out from the authority, the code and three prior conversations; a local model has none of that.
Once decision 54 lets a task be assigned to a model, how it is written stops being style and becomes
whether it can be done at all. The `agent:` label and this shape are two halves of one thing:
assigning work to a model you have not written a task for is assigning it to fail.

**56 · A planner may observe and ask. It may never decide.** A scheduled watcher over `state.json`
that *notices* things — a milestone open far longer than its siblings, a gate that has not moved, an
authority sitting in `docs/design/proposed/` blocking milestones, records that contradict
`ROADMAP.md` — writes **questions into a register**, which is what write-back exists to answer. It
never writes a plan, a status or a manifest. If it writes plans rather than observations, the site
stops being generated from records and starts being generated from a model's opinion, which is
decision 1 inverted. **Deferred, not scheduled:** the owner triggers work verbally today and that is
his deliberate answer, so decision 44's *Atlas needs no standing agent* stands unchanged for now.
This decision records the constraint in advance, so that if the planner is ever built it is built
inside it.

**57 · Decision 35's exclusions no longer stand on their stated grounds.** Decision 35 scopes
write-back to register answers and acceptance results and justifies it by handing creating issues,
approving milestones and triggering work to Platform Operations. **Platform Operations does not do
those things** — it deals with the release process and nothing before it — so the justification is
withdrawn (#784), and with it the sentence *two consoles that both act is how they diverge*, which
described a division of labour that does not exist. The two-endpoint limit is not thereby widened:
some of those exclusions may well survive on grounds that are actually true, and a write path into a
manifest is a genuinely bigger surface than a write path into a prose record. What is settled is that
the old reason is void and the scope must be re-decided on correct ones. Until it is, two things wait
on it: the refresh button that rebuilds the site (#780), and anything that would author an issue
rather than render one.

## Write-back re-decided, and one directory per feature — Milestone 9

**58 · Decision 35 is retired.** Its justification — that approving milestones and editing
manifests belong to a separate operations console — was withdrawn by decision 57 (Platform
Operations does not do those things). Decision 57 did not itself widen write-back's scope; this
decision does not either. What it removes is the CLOSED-COUNT posture: a future write-back
capability is its own decision, on its own stated grounds, the same way decision 59 is — not
something that needs a fixed "exactly N" test updated by amendment every time. `api/lib/handlers.mjs`'s
"exactly three handlers" test is retired to a named-list test for the same reason: a closed, named
list still catches a stray export nobody decided on; a fixed count does not do more than that.

**59 · `POST /api/approve` moves a proposed design into its feature's own directory and scaffolds
its first milestone, in one commit.** Decided on its own grounds, per decision 57's instruction to
re-decide rather than infer: a human has already reviewed the design (decision 35's own "repository
presence does not constitute design approval" still holds — nothing here approves a design that has
not been looked at; it acts on a decision a person already made) and wants it to become a tracked
feature. A genuinely bigger write surface than the three before it — it creates or extends a
manifest rather than editing a record a manifest names — built on its own atomic-commit mechanism
(`createTreeClient`, the Git Data API) rather than the single-record one, because that surface is
bigger. Recognizes a proposal in either shape a project actually uses: a slug directory
(`docs/design/proposed/<slug>/`), or — the ordinary case for a real proposal, one `.md` file with
maybe a sibling image or wireframe — a loose-file group under `docs/design/proposed/` sharing a
filename stem. An existing manifest is never a refusal: a feature the generator already tracks (this
project seeded seven of them directly, before `approve` ever existed) can still have a design
genuinely un-landed, and `approve` moves it in without disturbing what is already on record. Does
not reopen the rest of what decision 35 excluded: creating an arbitrary GitHub issue, setting a
milestone's `status` directly, or any other manifest EDIT is still undecided and still not built.

**60 · An approved design authority and a feature's own tracking share one directory.** The split
this project used — `docs/design/approved/<feature>/` for the authority, `docs/features/<feature>/`
for everything else — was confusing enough in practice that the owner asked for it retired: "we need
to be organized and have a clean process." `docs/design/proposed/` is unaffected; only where an
ALREADY-approved design lands changed. `AGENTS.md`'s design-before-implementation gate amended to
match. Real content moved for every workstream that had a separate authority at the time: `atlas`
(the decisions log this very file is), `menus` (57 files), and `authentication` (4 files) —
scaffolded as a tracked feature in the same move, having had an approved authority but no workstream
ever carrying it forward.

**61 · `POST /api/refresh` triggers the project's own rebuild workflow, and commits nothing.**
Decision 57 named this, alongside `approve`, as waiting on write-back's scope being re-decided;
decisions 58–60 did that re-decision, so this is settled deliberately here rather than inferred.
Unlike every write-back endpoint before it, this one is not a commit at all — it reads
`atlas.config.json`'s `"workflow"` field (a filename, not a secret, so it lives beside everything
else a project already names about itself rather than in a new application setting) and asks
GitHub to dispatch that workflow, through the same installation token every write endpoint already
holds. No new credential, no new trust boundary, no state: decision 37 stays completely intact —
the repository is still the only source of truth, this only stops a reader waiting on CI to notice
a change already landed.
