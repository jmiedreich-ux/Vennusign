# Atlas Milestone 4.1 — Feature planning rebuilt again: accordion + spine

> This milestone was not on the roadmap. It exists the way M2.1 does (decision 18: an inserted
> milestone is `M<n>.1`, never a letter, never a bare suffix) — done, and recorded after the fact.

**Goal:** Live use of #780's drawn SVG chart surfaced two problems small fixes kept not solving —
dates and duration cluttering every page, and no honest place for an inserted or renumbered
milestone's real label. Rather than patch the chart a third time, the owner ran a UX exploration
and picked a different shape for the page entirely.

**What shipped:** Feature Planning (desktop) rebuilt as a collapsed accordion, one row per
feature; expanding a row reveals that feature's milestone spine inline — no dates or duration
anywhere, every milestone shows its own real label. "What needs you" (mobile) rebuilt as triage
grouped by blocking state instead of a flat card list. Milestone task checklists, parsed for the
first time from the checklist already inside each milestone's linked GitHub issue (open or
closed), each task carrying an optional trailing owner tag (` — Claude`, ` — ChatGPT`) — an
explicitly open vocabulary, not validated against an enum. Drag-to-reorder reimplemented against
real DOM rows (features, and tasks within an expanded milestone) rather than SVG `transform`.
`src/chart.mjs` deleted in full, along with the per-feature triage modal M4 added.

**Where it landed:** entirely in the `Atlas` repository. `jmiedreich-ux/Atlas` PR #7, tagged
`v1.4.0`.

**Spec:** `docs/superpowers/specs/2026-08-23-feature-planning-rebuild-design.md` and
`docs/superpowers/plans/2026-08-23-feature-planning-rebuild.md`, both on Atlas's `main`. Built
spec → plan → subagent-driven-development, 10 tasks, each with its own implementer, review and
fix loop, then a final whole-branch review that browser-verified the actual behaviour (not just
text assertions) and caught 6 real bugs before merge.

**Acceptance:** full suite (515 tests) green; fixture build verified; visually verified in both
themes against real task/owner data (not just the empty fixture) via a real browser before the PR
opened.

---

## The thing this milestone does NOT settle, and should not be read as settling

M6 ("Tasks are GitHub issues, rendered inside their milestone") already had a considerably fuller
design worked out before this milestone existed: native GitHub sub-issues rather than checkbox
text, an `agent:<name>` label namespace as a closed, build-validated vocabulary for model
assignees, a horizontally-scrolling task-row grid (decision 26), and an explicit argument against
a task shape living anywhere in `workstream.json` or being parsed from free text.

What this milestone actually built is simpler and looser than that design on every one of those
points — checkbox text instead of sub-issues, an open (never-validated) owner-tag string instead
of a closed `agent:` label, a checklist instead of a row grid. It was built this way because it is
what the owner asked for in a live conversation, not because M6's fuller design was reconsidered
and rejected.

**So the real open question this milestone leaves behind is M6's, and it is now sharper than it
was:** reconcile the two designs, let M6's fuller version supersede what shipped here, or keep
both deliberately and say why two shapes of task representation coexist. That decision belongs to
M6, not to this record — this record only states plainly that it exists, so nobody reads M6's
plan later and assumes its "deliberately excluded: a task shape in `workstream.json`" line was
already litigated against what actually shipped. It wasn't.

## Decisions this milestone rests on and creates

It rests on **1** (built from source — the task checklist is parsed from an issue that already
exists, nothing new is authored), **18** (an inserted milestone is `M<n>.1`, which is why this
record exists at all), **20** (not every workstream has the same milestone count; this rebuild's
per-feature spine leans on that directly), and **40** (no layout names a person — `owner` values
are agent identities in practice, never validated as such). It does not create a new numbered
decision on its own; the reconciliation question above is left for whichever future milestone
settles it.
