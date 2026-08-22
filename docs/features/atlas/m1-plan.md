# Atlas Milestone 1 — The Generator

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.
>
> **Note on style:** this plan describes behaviour, names files and states what each test must prove, rather than listing code, at the owner's standing instruction. Every task still writes a failing test first and gives an exact verification command.

**Goal:** Build the Atlas generator — a versioned, reusable tool that turns any project repository following the Atlas convention into a static site, holding no project content of its own.

**Architecture:** A new repository, `atlas`, containing an Eleventy build with markdown-it, a manifest schema and validator, a depth-chart computation, a GitHub data fetch, and the Sky-token theme. It is published as a composite GitHub Action so a project consumes it in one line. Vennusign adopting it is Milestone 2, in the Vennusign repository; this milestone ships nothing to Vennusign.

**Tech Stack:** Node 22, Eleventy, markdown-it, `node:test` (matching the repository's existing front-end convention — no vitest, no jest). No framework runtime.

**Spec:** `docs/design/approved/atlas/decisions.md` (47 decisions, approved 2026-08-22).

## Milestone discipline

Per `docs/MILESTONE_EXECUTION.md`. Note the deviations forced by this milestone creating a new repository:

- Steps 1–7 run against the **new** `atlas` repository, not Vennusign. The milestone issue and the
  tracker claim still live in Vennusign, because that is where the authority and the roadmap are.
- **Step 22's** house gate does not apply — there is no `Vennu.Api`, no LocalDB and no Playwright
  suite here. The gate for this milestone is `npm test` plus a build of the fixture project.
- **Step 30's `[skip ci]`** does not apply. That convention exists because Vennusign's CI is
  suspended; the `atlas` repository has its own CI from task 8 and it must run.
- **Step 2** has nothing to confirm: Atlas M1 is the first milestone of a new workstream.
- Acceptance is a **demo script**, not a workbook: this milestone ships no customer-facing UI.

## Governance gate

**Met.** The design authority was approved by the owner on 2026-08-22 and is at
`docs/design/approved/atlas/decisions.md`. This plan is cleared to execute.

**Repository visibility.** Decision 46 records that Vennusign is public and the owner will make it
private. The `atlas` repository holds no project content (decision 40), so it may be public or
private independently — but it must not be created public if any fixture in it quotes real records.

## Global Constraints

- **Node 22.** The only JavaScript runtime this project's CI uses.
- **`node:test` for tests**, run by `npm test`, matching `src/www` and `src/back-office`.
- **The generator holds no project content** (decision 40). Any Vennusign string in this repository
  outside a clearly-labelled test fixture is a defect.
- **Renders as GitHub renders** (decision 11): two- and three-space nested lists, inline HTML,
  `<sub>`, pipe tables. No frontmatter is required or expected — the corpus has none.
- **Standalone `.html` is copied byte-for-byte** (decision 10), never passed through a template
  engine, including any sibling `support.js`.
- **The build fails loudly** (decision 32). A missing file, an unknown status, a milestone with no
  title — all exit non-zero. Never render a blank.
- **Sky UI tokens, Segoe UI, three-state dark mode** (decision 28): bare `:root`, a
  `prefers-color-scheme` block guarded against an explicit light choice, and a `[data-theme="dark"]`
  block.
- **No new hosting model and no framework runtime** (decision 12).

## File structure

| File | Responsibility |
|---|---|
| `package.json` | Node 22, `node:test`, Eleventy and markdown-it as the only runtime dependencies |
| `src/schema.mjs` | The manifest and config contract, and the validator. The single source of what a project must provide. |
| `src/config.mjs` | Load and normalise `atlas.config.json`; resolve paths against a project root |
| `src/markdown.mjs` | markdown-it configured to match GitHub; `.md` link rewriting; anchor ids |
| `src/depth.mjs` | Manifests → ladder rows, per-column depth, arrow extents. Pure. |
| `src/github.mjs` | Fetch issues and PRs, bucket by workstream label. Injectable fetch. |
| `src/state.mjs` | Assemble `state.json` from the same inputs the pages use |
| `src/build.mjs` | Eleventy wiring, passthrough copy, page generation |
| `theme/` | Sky-token CSS, layouts, the depth chart, the mobile view |
| `fixture/` | A miniature project that follows the convention — the build's own test subject |
| `action.yml` | The composite GitHub Action wrapper |
| `tests/` | One test file per `src/` module |

`schema.mjs` is deliberately first and standalone: it is the contract between the generator and every
project that will ever use it, and decision 41 makes it the thing a project must satisfy.

---

### Task 1: The repository, and the manifest contract

**Files:** `package.json`, `src/schema.mjs`, `tests/schema.test.mjs`, `README.md`, `.gitignore`

**Produces:** `validateWorkstream(obj)` and `validateConfig(obj)`, each returning
`{ ok: true, value }` or `{ ok: false, errors: [{ path, message }] }`. Never throws; never returns a
partially-valid object.

Create the repository, then define the contract before anything consumes it. The schema covers the
manifest shape in decision 14 — codename, what, stage, position, gate, label, design links, and a
milestone array of `{ id, label, depth, title, status, plan, issue, pr, acceptance }`.

**Tests must prove:** a well-formed manifest validates; an unknown `stage` or `status` is rejected by
name, because decision 32 requires a closed vocabulary rather than a free string that renders as a
blank chip; a milestone missing `title` or `depth` is rejected; `id` and `label` are both required and
may differ, which is decision 17's whole point; `issue`, `pr` and `acceptance.record` are nullable
because a planned milestone has none; and every error names the path that failed, so a project author
can fix it without reading the generator.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail** — `npm test`
- [ ] **Step 3: Implement the schema and validators**
- [ ] **Step 4: Run and confirm they pass** — `npm test`
- [ ] **Step 5: Commit** — `feat(schema): the manifest and config contract`

---

### Task 2: Config loading and project resolution

**Files:** `src/config.mjs`, `tests/config.test.mjs`, `fixture/atlas.config.json`

**Consumes:** `validateConfig` from Task 1.
**Produces:** `loadConfig(projectRoot)` returning a normalised config with absolute paths, and
`resolveWorkstreams(config)` returning validated manifests in declaration order.

Also create the fixture project — a miniature repository following decision 41's convention, with two
workstreams of different depths so that decision 20 is exercised from the first task that can.

**Tests must prove:** a config naming a workstream directory that does not exist fails with that path
in the message, not a stack trace; the fixture's two workstreams load in declaration order; a manifest
that fails Task 1's schema aborts the whole load rather than being skipped, because a silently-omitted
workstream is exactly the drift Atlas exists to prevent; and paths resolve relative to the project
root, never the generator's own directory — the generator must work identically from any checkout.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement, and build the fixture**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(config): load a project and its workstreams`

---

### Task 3: Markdown that renders as GitHub renders it

**Files:** `src/markdown.mjs`, `tests/markdown.test.mjs`, fixture markdown samples

**Produces:** `renderMarkdown(text, { hrefBase })` returning HTML, and `headingAnchors(text)`
returning `{ id, text, level }[]` for a table of contents.

**Tests must prove:** two- and three-space nested lists nest, because the corpus uses both and never
four; a pipe table becomes a table wrapped in a horizontally scrollable container, so a wide table
never makes the page scroll sideways; inline `<sub>` survives, since the question registers use it for
citations; a relative `[text](other.md)` link rewrites to the generated page's URL while an absolute
link is untouched; a link to a `.html` file, including one with spaces in its name, resolves to the
copied file; a fenced block keeps its language; and a heading produces a stable anchor id, so
`#d15`-style deep links into a decisions document work.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(markdown): render the corpus as GitHub does`

---

### Task 4: Depth computation

**Files:** `src/depth.mjs`, `tests/depth.test.mjs`

**Consumes:** validated manifests from Task 2.
**Produces:** `computeLadder(workstreams)` returning `{ rows, columns }` — `rows` being the pre-
milestone stages followed by numbered depth positions, and each column carrying `{ codename, stage,
barTo, headAt, tipLabel, note }`.

This is the chart's whole logic, and decision 24 says both ends come from the manifest so the chart
cannot disagree with the records.

**Tests must prove:** the ladder is the **union** of all workstreams' depths, so a six-milestone stream
and an eleven-milestone stream coexist and neither imposes its numbering (decision 20); three pre-
milestone stages sit above the first milestone (decision 23); the bar covers every completed stage and
the head points at the next one — the off-by-one this rule exists to prevent is a real bug caught in
review, so assert a completed final milestone puts the head *beyond* it, not on it; a workstream at
`designing` with no milestones produces a bar of two stages and no milestone rows; a workstream with
nothing produces no bar at all; and `tipLabel` is the column's **own** id, never a ladder row label.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(depth): compute the ladder, bars and arrowheads`

---

### Task 5: GitHub issues, bucketed by workstream

**Files:** `src/github.mjs`, `tests/github.test.mjs`, `tests/fixtures/issues.json`

**Produces:** `fetchProjectIssues({ repo, token, fetchImpl })` returning
`{ byLabel: Map, unlabelled: [], prs: [] }`.

Inject `fetchImpl` so tests need no network. One list call bucketed client-side, not one call per
label — 47 issues fit a single page and the search endpoint has a lower rate limit.

**Tests must prove:** issues bucket by their `workstream:*` label; an issue carrying two workstream
labels appears under both, because the roadmap deliberately places one issue in two clusters; pull
requests are excluded from the issue buckets, since the REST issues endpoint returns them; unlabelled
issues are collected rather than dropped, because 42 of 47 are unlabelled today and silently hiding
them would misrepresent the backlog; and a failed fetch degrades to empty buckets with a warning
rather than failing the build — GitHub being unreachable must not stop the site rendering the
repository, which is the part that matters.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(github): fetch and bucket issues by workstream`

---

### Task 6: The theme and the pages

**Files:** `theme/tokens.css`, `theme/base.njk`, `theme/depth.njk`, `theme/mobile.njk`,
`theme/workstream.njk`, `theme/milestone.njk`, `theme/document.njk`, `tests/theme.test.mjs`

**Consumes:** Tasks 3 and 4.
**Produces:** the layouts `build.mjs` renders in Task 7.

Sky UI tokens with Segoe UI, three-state dark mode. Three surfaces per decision 22: the desktop depth
chart, the mobile view sorted by what needs the owner, and the document pages.

**Tests must prove** — these are assertions against the generated HTML, not visual checks, because
this environment has no browser: every colour used resolves to a token defined on bare `:root`, since
a colour defined only inside a media or `[data-theme]` block is the classic unreadable-artifact bug;
`body` sets an explicit background from a token; the mobile view orders workstreams by state, not
alphabetically (decision 27); every status chip carries a text label and not colour alone; and no
generated page contains a hard-coded project name, which is decision 40 enforced by test.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement the theme and layouts**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(theme): Sky tokens, three surfaces, three-state dark`

---

### Task 7: The build, and `state.json`

**Files:** `src/build.mjs`, `src/state.mjs`, `.eleventy.js`, `tests/build.test.mjs`,
`tests/state.test.mjs`

**Consumes:** everything above.
**Produces:** `build(projectRoot, outDir, options)`; and `state.json` in the output.

**Tests must prove:** building the fixture produces a page per workstream and per milestone, plus the
depth chart and the mobile view; the fixture's standalone `.html` file and its sibling `support.js`
are copied **byte-identical**, asserted by hash, because decision 10 is the one rule a template engine
would silently break; a manifest referencing a missing plan file **fails the build non-zero** with the
path named — the single most important assertion in this milestone, since it is what makes decision 1
structural rather than aspirational; an unknown status likewise fails; `state.json` contains the same
workstreams, milestones and issue buckets the pages were rendered from, so the agent-facing output
cannot drift from the human-facing one; and a second build over the same output is byte-identical,
because a generator whose output varies run to run cannot be trusted to be current.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement the build and the state emitter**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Full milestone verification**

```bash
npm test
node src/build.mjs fixture/ .out/
```

Expected: all tests pass; `.out/` contains the fixture's pages, its copied HTML, and `state.json`.
Then deliberately break the fixture — point a milestone at a missing plan — and confirm the build
exits non-zero naming that path. Restore it.

- [ ] **Step 6: Commit** — `feat(build): render a project, emit state.json, fail on a broken reference`

---

### Task 8: Package as a versioned Action

**Files:** `action.yml`, `README.md`, `.github/workflows/test.yml`

**Produces:** a composite action consumable as `uses: <owner>/atlas@v1` with inputs for the project
path, output directory and GitHub token.

**Tests must prove:** the action's own CI runs `npm test` and the fixture build on Node 22 and
ubuntu-latest, so the generator is verified by the same runner a consuming project will use. The
README documents the convention a project must follow — decision 41's file list and the config shape —
because that is the generator's actual public interface.

- [ ] **Step 1: Write the workflow and confirm it fails** (no action.yml yet)
- [ ] **Step 2: Implement `action.yml` and the README**
- [ ] **Step 3: Confirm the workflow passes on a pushed branch**
- [ ] **Step 4: Tag `v1.0.0` and move the `v1` major tag to it**

Per decision 46, these tags live in the **`atlas` repository**, not in any project that consumes it.
A project needs no tags to use Atlas.

- [ ] **Step 5: Commit and tag** — `feat(action): publish the generator as a composite action`

---

## Excluded from this milestone

- **Vennusign's site.** Its config, its manifests, its workflow, its Static Web App and its custom
  domain are **Milestone 2**, and they live in the Vennusign repository.
- **Write-back.** Decisions 34–37, now Milestone 3.
- **Search.** Pagefind is a candidate for a later milestone once there are pages worth searching.
- **`classify-changes.sh`.** Decision 33's allow-list entry belongs to Milestone 2, since it is a
  change to Vennusign's CI, not to the generator.

## Self-review

**Spec coverage.** Decisions 9–12 are Tasks 3, 6 and 7. Decision 14's manifest is Task 1. Decisions
20, 23, 24 and 25 are Task 4. Decision 27 is Task 6. Decision 29's `state.json` is Task 7. Decisions
32 and 40 are asserted by test in Tasks 7 and 6 respectively. Decision 41's convention is documented
in Task 8. Decisions 1, 2 and 3 are properties the tasks enforce rather than features: the build
failing on a broken reference (Task 7) is what makes decision 1 true.

**Decisions with no task here, by design:** 5–8 (hosting), 13 and 15–19 (Vennusign's content), 21, 26,
30, 31, 33 — all Milestone 2 — and 34–37, Milestone 3. Decisions 43–46 are owner or prerequisite
items, not build work.

**Placeholders.** None. Every task states what its tests must prove; no task defers a decision to the
implementer.

**Type consistency.** `validateWorkstream` / `validateConfig` return the same `{ ok, value | errors }`
shape in Tasks 1 and 2. `computeLadder` returns `{ rows, columns }` in Tasks 4, 6 and 7.
`fetchProjectIssues` returns `{ byLabel, unlabelled, prs }` in Tasks 5 and 7.

**Known risk, stated rather than hidden.** Task 6 asserts against generated HTML because this
environment has no browser, so the theme is verified structurally and never visually. Contrast,
overflow at 390px and the dark-mode swap are exactly the failures a structural test misses. The
milestone's demo script must therefore include the owner opening the fixture build on a phone and in
both themes — that is the only visual gate this milestone has.
