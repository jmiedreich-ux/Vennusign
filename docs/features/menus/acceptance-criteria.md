# Menus — Acceptance Criteria, Running Checklist

Required by `milestone-plan.md` §Per-milestone quality gates: the 18 criteria from
`docs/design/approved/menus/README.md` §Acceptance criteria, tracked as a running
checklist. **A criterion flips to "met" only with a named spec, demo check, or review
asserting it** — never by implication. Criterion 4 carries its Q187 rewording. Criteria
11 and 14–17 are stamped "deferred to a later build" (Q194). Criterion 18 is asserted by
a named milestone-2 spec and **re-checked every UI milestone** (Q194).

Created 2026-08-09 at the Milestone 2 readiness pass. Update in the PR that changes a
criterion's status, not after.

| # | Criterion (abbreviated — README is authoritative) | Milestone | Status |
|---|---|---|---|
| 1 | 86 reaches every screen in seconds, no publish, never queued | M1 (API) · M4 (on-screen, 10s pass line Q188) | **met at API** — `run-m1-demo.ps1` checks incl. screen assertions, M1 accepted 2026-08-09; on-screen re-assertion waits for M4 |
| 2 | Publish ships all of that menu's queue, nothing of another's | M1 · re-asserted through UI in M3 | **met at API** — M1 demo + SQL integration suite; UI re-assertion M3 |
| 3 | An item 86'd before a publish is still 86'd after it | M1 | **met** — M1 demo + integration tests + `ModelInvariants` |
| 4 | No screen content changes without a deliberate act — a publish, an accept, an availability toggle, or a confirmed Take off the screens (Q187 wording) | M1 · re-asserted through UI in M3 | **met at API** — M1 demo checks 4/6/8c/8d assert screens via `GET content/screens/showing` |
| 5 | "unpublish" / "supersede" / "restore" / "archive" appear nowhere in the UI (scope per Q179: Menus + rewritten surfaces; elsewhere = copy debt #682) | M2 | **met** — asserted against the rendered shelf and the open card menu (`menus-shelf.spec.ts`), and against the source (`menus-shelf.test.mjs`) |
| 6 | Take off the screens always shows what will replace the menu before confirming | M2 | **met** — `menus-shelf.spec.ts` opens the dialog and asserts the fallback card, the affected screens and the verbatim sentence |
| 7 | Undo is a keystroke; named in no settings page, plan comparison, or marketing surface | M3 | pending |
| 8 | A capability outside the plan renders nothing — no disabled control, no tooltip, no placeholder | M2 (shell gating) · every UI milestone | **met** — named spec in `navigation-and-entitlements.spec.ts`, asserting both halves: outside-the-plan renders nothing anywhere, and a permission refusal still renders and still says so (decisions 4 and 5) |
| 9 | Play renders against device-reported geometry; an unpaired screen is unselectable in Play | M4 (partial) · M5 (complete) | pending |
| 10 | Import surfaces only the rows the parser was unsure about | M6 | pending |
| 11 | Spreadsheet headings any order/case, extra columns tolerated | later build | **deferred to a later build** (Q194) |
| 12 | Import into an existing menu replaces content, preserves layout/theme/86s | M6 | pending |
| 13 | Thirty near-miss matches produce one grouped question | M6 | pending |
| 14 | (Group) Locked field visible with its reason, never hidden | later build | **deferred to a later build** (Q194) |
| 15 | (Group) An 86 at one venue never affects another, never prompts | later build | **deferred to a later build** (Q194) |
| 16 | (Group) A pending group menu never applies on its own | later build | **deferred to a later build** (Q194) |
| 17 | (Group) Demoting trust reverts nothing on a screen | later build | **deferred to a later build** (Q194) |
| 18 | A single-venue account renders the same components with zero venue affordances | M2 named spec · re-checked every UI milestone (Q194) | **met** — the named spec is `tests/ui/specs/single-venue-criterion-18.spec.ts`. **Re-check it at every UI milestone**: add each new surface to its `surfaces` list rather than writing a second spec |
