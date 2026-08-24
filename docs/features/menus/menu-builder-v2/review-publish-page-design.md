# Review & Publish page — design write-up (draft, awaiting owner review)

**Status:** paused for review. Nothing implemented yet. Sub-milestone M7.1 in `workstream.json`.

Scopes the full-page "Review & Publish" piece from `mock-fidelity-polish-plan.md`'s Bigger Pieces list — replacing
the current small `reviewOpen` modal in `MenuBuilder.tsx` with the full page shown in
`assets/15-publishing-review-page.png`.

## Already decided (recorded, not reopened here)

Confirmed during this write-up's discussion:

- **The fit-overflow warning is a real gate, not just informational.** Publish stays disabled until the operator
  ticks "I understand these will not show" — matches decision A14 exactly.
- **The "Where it goes" sidebar goes to full mock fidelity** — page assignment, refresh cadence, online/offline/stale
  per screen — not the footer's simpler screen chips.
- **"Publish later…" is out of scope.** `milestone-plan.md`'s scope guardrails exclude scheduling entirely; the
  mock's hand-off to Schedules has nothing to hand off to yet. Build "Publish now" only; leave "Publish later" absent
  per decision 4 (a capability outside scope is absent, not disabled) rather than a dead button.

## Findings from reading the actual code (not assumptions)

1. **Most of the change-list is already built.** The current modal already renders `data.changes` — an array of
   structured, field-level `{targetKind, targetId, field, before, after}` objects, the exact shape Q12 asked for —
   through existing formatter functions `changeSentence(change, board)` / `changeValues(change)`
   (`builderModel.mjs`). The new page's core list is mostly "give this existing render a fuller layout and group it
   by page," not new data plumbing.
2. **The mock's "1 item has no name yet" section doesn't map onto this app's real data model.** A11 requires a name
   to create an item at all — there's no `isMissingName` anywhere in the code, only `isMissingPrice`. This section of
   the mock predates A11 being settled. Treating **missing price** as the real equivalent (see issue #853).
3. **The fit-overflow acknowledgment (A14) isn't wired to publishing at all today.** `fitOpen`'s "Check fit" dialog
   is read-only info, reachable from the capacity banner, with no connection to the publish flow — you can publish
   over-capacity content today with zero warning. This is genuinely new work, not a re-arrangement.
4. **Refresh-cadence correction** (I initially got this wrong and want it on record): there's no per-screen
   configurable cadence anywhere in the backend. The display app has one **global** constant
   (`SERVER_REFRESH_INTERVAL_MS`). The mock's per-screen-looking cadence text ("Front Left: updates immediately",
   "Front Right: every 30s", "Lobby: offline 2h — takes this when it reconnects") actually decomposes into: the same
   global interval for every online screen, plus the *already-real* online/offline/stale screen-state tracking for
   the reconnect case. Full fidelity is achievable, just from that corrected source — not real per-screen data that
   doesn't exist.
5. **No client-side router exists in this app.** Navigation is entirely internal component state (`onBack` callback
   prop, `place.view` toggling the builder's main content). This directly shapes the structural-approach question
   below.

## Open questions — filed as issues, need your review before implementation starts

- **#852 — structural approach.** New `place.view === "review"` state (recommended, matches the app's own existing
  navigation pattern) vs. a bigger version of today's modal (rejected — can't cleanly reproduce the mock's own
  top-bar/breadcrumb, keeps this as an overlay rather than a real page).
- **#853 — missing-price placement.** Fold it inline into the new Review page (matches A11's actual wording, mock's
  layout) vs. keep today's separate post-click `confirmPublishMissingPrice` popup. This is a real behavior change
  (removes a click) either way, worth an explicit call.

## Not yet scoped (deliberately stopped here)

Once #852/#853 are settled, the remaining work is: the exact per-page grouping of the change list, the fit-gate's
checkbox/state wiring, composing the "Where it goes" sidebar from existing screen/assignment data plus the corrected
cadence source, and the page's own copy (governed by the same verbatim-copy rules as the rest of Menus). None of that
is blocked on anything outside this doc — it's implementation detail once the structural question is answered, not a
second design pass.

## Downstream dependency

Per A15, the full history page's expandable per-publish detail is **the same summary text produced here, stored at
publish and replayed later** — not recomputed. Whatever this page's change-list formatting settles into should be
built as something the publish path can persist, since the history page's own design (not yet started) depends on
it.
