# Atlas — decisions on record

Status: **proposed, not approved.** Settled with the owner on 2026-08-21/22. Moving this bundle to
`docs/design/approved/atlas/` is the owner's act and has not happened. Until it does, this document
governs nothing.

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
config and a workflow.

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
convention. Task 8 of Milestone 1 creates the first of them.

This is **not** coupled to Vennusign's own tag situation. Vennusign has zero tags and needs
`v{productVersion}` tags for the release model, but that is #754's problem and release versioning's
to solve — Atlas neither needs nor produces them, and a project consuming Atlas needs no tags at
all.

**47 · Murphy cannot reach Atlas.** Static Web Apps role invitations are for people; there is no
non-interactive credential path, so the QA agent that tests every other deployed surface cannot smoke-
test this one. Either Atlas is exempt from Murphy, or the write-back work in decisions 34–37 supplies
a service identity. Not a Milestone 1 blocker; recorded so it is not discovered late.
