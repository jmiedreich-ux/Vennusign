# Atlas Milestone 7 — a design becomes a tracked feature

**Goal:** a feature whose design is approved (`docs/design/approved/<slug>/`) but has never been
scaffolded onto Atlas's own sheet sits at "Designing / No milestones yet" indefinitely, with no
documented path from an approved design to a tracked `workstream.json`. This closes that gap.

**What shipped:** `src/scaffold.mjs`, a **local-only CLI** — never write-back, never reachable
from the deployed site (decision 35 already excludes manifests and plan files from what write-back
may touch). It checks three preconditions (the design is approved, not still `proposed/`; the
workstream isn't already scaffolded; the slug is a real directory name) and writes a schema-valid
starter `workstream.json` plus a first milestone plan with unmissable placeholder text, promoting
the new slug into `atlas.config.json` so it actually renders rather than triggering
`unnamedFeatureDirs`'s warning silently. A companion SOP addition, step 2a in this repository's
`docs/MILESTONE_EXECUTION.md`, gates what has to be true before something is even written into
`docs/design/proposed/` in the first place — the upstream half of the same gap.

Two of the four things originally asked for turned out to already exist and were not rebuilt: the
approval process (`docs/design/proposed/README.md`'s per-entry convention, and
`MILESTONE_EXECUTION.md` step 3) and the "stuck at Designing" signal (M4.1's "No milestones yet"
text already says so).

**Where it landed:** entirely in the `Atlas` repository. `jmiedreich-ux/Atlas` PR #9, part of
`v1.5.0`.

**Spec and plan:** `docs/superpowers/specs/2026-08-23-m7-feature-onboarding-design.md` and
`docs/superpowers/plans/2026-08-23-m7-feature-onboarding.md`, both on Atlas's `main`.

**Acceptance:** 527 tests passing at the time this milestone closed; a real end-to-end run
(`scaffold.mjs` against a fresh fixture, then `build.mjs`) produced actual rendered pages carrying
the placeholder text. Independently reviewed by a fresh agent with no stake in the implementation
before merge: decision 35 confirmed respected by grep (zero references to `scaffold` anywhere in
`api/`), decision 1's gap (the scaffold is a template, not generated from the design's own content)
confirmed real but deliberately scoped out in the spec, not an oversight.

**Worth recording:** the execution fork for this milestone did every implementation and review
step itself rather than dispatching fresh implementer/reviewer subagents per task, a real deviation
from this project's normal subagent-driven-development shape. Caught only by an independent review
dispatched afterward, not by the process itself. Low risk here given the plan's small size, but the
pattern is worth watching for on a larger milestone.
