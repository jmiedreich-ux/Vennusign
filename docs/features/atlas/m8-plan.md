# Atlas Milestone 8 — real deployment stages

**Goal:** `WORKSTREAM_STAGES` carried one catch-all value, `shipping`, for anything past
`planned` — true of a feature still in a dev environment and one genuinely released, which is not
one fact. Replace it with real deployment stages, and let a stage transition be recorded (never
triggered) through write-back.

**What shipped:** `WORKSTREAM_STAGES` is now `not-started / designing / planned / development /
staging / release` — staging is optional, a feature may go straight from development to release.
A new write-back endpoint, `POST /api/deployment-transition`, appends `{ stage, note }` (no dates —
an ordered sequence, not a timeline) to a per-workstream log; it never writes `workstream.json`
directly and never triggers anything — the deployment agent that would someday act on a recorded
transition is explicitly out of scope, a separate Vennusign-wide concern for later. **This amends
decision 35 on the record:** write-back is now exactly three things (a register answer, an
acceptance result, a deployment-transition record), not two — both of the guard tests that used to
assert exactly two write handlers were updated to assert exactly three. The ordered transition
history renders on a feature's own row, reusing the milestone spine's existing node visual
language rather than inventing a second one. The two real manifests still on the old `shipping`
value (`atlas`, itself; `menus`) were migrated to `development` as a conservative floor in a
separate, ordering-constrained commit that landed only after this milestone's new vocabulary was
already live — landing it first would have failed the build against the old vocabulary.

**Where it landed:** the vocabulary, endpoint, wiring and UI in the `Atlas` repository
(`jmiedreich-ux/Atlas` PR #10, part of `v1.5.0`); the two-manifest migration in this repository
(`jmiedreich-ux/Vennusign` PR #827).

**Spec and plan:** `docs/superpowers/specs/2026-08-23-m8-real-deployment-stages-design.md` and
`docs/superpowers/plans/2026-08-23-m8-real-deployment-stages.md`, both on Atlas's `main`.

**Acceptance:** 574 tests passing, fixture build succeeds. A genuine final whole-branch review
found 3 real Important issues (a vocabulary-validation gap in the log-read path, cross-surface
stage disagreement between `computeLadder` and the override, README drift) and one fix wave
resolved all of them plus 4 minor cleanups; a scoped re-review confirmed every fix. Independently
re-verified afterward by a fresh reviewer, focused on the two highest-stakes claims rather than a
full re-review: the decision-35 amendment is deliberate and exact-match enforced (not weakened to
"at least N"), and no code path anywhere — the handler, the shared GitHub client, the on-screen
trigger — calls a deploy or CI API, `workflow_dispatch`, or shells out to anything. Both PASS with
file:line citations, not taken on the executing fork's own word.

**Deliberately excluded, and still open:** the deployment agent itself — a dedicated, Murphy-shaped
actor that would receive a recorded transition and actually act on it, for all Vennusign features,
not only Atlas. Not scoped here on the owner's own instruction ("that handles that... we will spec
this out further later").

**Worth recording, a process note this milestone's own existence proves:** M7 and M8 were both
given fresh top-level numbers while M5 and M6 — already fully planned — sat untouched, without a
stated reason for leapfrogging them. Not undone after the fact (milestone ids are durable once
shipped), but the rule going forward: no new top-level number while an already-planned one is
unstarted, unless a reason is stated; when there is one, it is a sub-milestone of the current
active thread, the way M2.1 and M4.1 were.
