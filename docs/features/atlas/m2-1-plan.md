# Atlas Milestone 2.1 — The page rebuilt, and reorderable

> **This is a record of work that shipped**, not a proposal. It is written after the fact so the
> site can render what happened; the work was merged and tagged on 2026-08-22.

**A refinement of M2, not a milestone of its own.** M2 delivered Vennusign adopting the generator
and the site going live; M2.1 rebuilt the page that adoption exposed as wrong, and added one
capability the owner asked for. Write-back stayed M3.

The sub-number was deliberate: it leaves a visible mark that further work happened at that point,
rather than the refinement being absorbed invisibly into the original milestone. Decision 18 sets
the form — a milestone is `M<n>` and its parts are `M<n>.1` — and this is its first use.

**Where it landed:** the `Atlas` repository, as PR jmiedreich-ux/Atlas#2, tagged **`v1.1.0`**, 326
tests on Node 22 Linux. Vennusign's side of it is PR #781 (why the gate copy has to stay) and PR
#782 (the `gated` → `blocked` rename in the Keystone manifest).

**Spec:** GitHub issue jmiedreich-ux/Vennusign#780, **all comments**. The design was argued out
across those comments and several of them correct earlier ones; where they disagree, the later one
wins. The approved mock is linked from the issue.

---

## The framing, which sits above everything else

**It was not a chart and they were not grid cells.** M1 implemented the page as an HTML table —
workstreams as columns, ladder rows as `<tr>`, every intersection a `<td>` — and every visual
complaint in #780 followed from that. Cells want borders and labels and equal weight, so the arrow
became a glyph in a box, completed work became a fill with the word "Passed" in it, and future work
needed something in it because a cell looks broken empty.

Fixing those one at a time inside a table would not have got there. The page is now a set of drawn
paths descending the page: the ladder supplies the vertical scale, the ribbons are the drawing, and
nothing occupies a cell because there are no cells.

The root cause is worth keeping: M1's theme was built from structural assertions against generated
HTML and **never once rendered in a browser**. The tokens were adopted by value and the contrast was
computed arithmetically. Matching a palette is not the same as matching a design.

---

## What shipped

**Renamed throughout: Depth → Feature planning.** The old name described the mechanism, not what the
page is for. Page title, nav label, route wording.

**The ladder.** Three stage rows — *Not started · Designing · Planned* (decision 23) — then one row
per milestone depth. Milestone identifiers appear **only** in the ladder column, never in a
feature's own lane. A light horizontal rule at every row, full width, drawn *beneath* the ribbons so
a ribbon crosses a rule rather than being cut by it. Lines, not a grid.

**Four colour-coded bands** — *Not started*, *Designing*, *Planned*, *Execution* — running the full
width, so the phase reads horizontally across every feature at once. Tinted from the Sky contract's
own status surfaces, faint enough that the ribbons stay the subject. The execution band is far the
tallest, so the weight that works for the three short bands had to survive being stretched.

**The arrows.** One continuous ribbon per feature, from the top of the ladder through the stages and
on into the milestones, with no break between them. A **solid** arrow covers what is finished;
where records exist beyond it, a **second, fainter, slightly narrower** arrow covers the remainder.
Each arrow ends in its own head, growing straight out of its own body — nothing floats, and there is
never a gap between body and head. Colour carries state without the legend. No "Passed" labels, and
no marks in a feature's lane at all beyond the ribbon and its dots.

**Nothing renders below the arrowhead, whatever its status.** The rule is positional, not
status-based.

**A skipped milestone is noted, not treated as a wall.** The ribbon leaves its lane, curves around a
crossed circular marker sitting in that milestone's row, and rejoins below, with the reason and
issue number small beside it — and the bar continues to the real edge of finished work.

**Milestones carry dates.** Two additive fields, `started` and `completed`, both stored calendar
days. A closed milestone shows both and how long it took; current and next show the start date only;
future milestones show nothing. Because they are additive, `state.json` stays at version 1 — a new
optional key does not break a reader that understood the previous version.

**Balloons.** A speech balloon at each feature's current or next step, taking its text from the
milestone `title` for a next step and the workstream `gate` for a feature still in the stages.

**`gated` became `blocked`** in the closed status vocabulary, and **the generator now emits its own
`staticwebapp.config.json`**, so a project adopting Atlas is not public by default — the top M2
finding from #780.

---

## The new capability: drag to reorder, remembered per device

The owner wanted to drag features into his own order and have it stick. Reorder by dragging a
feature's column header; the whole lane moves with it. The order persists to `localStorage` as a
list of workstream slugs, with a visible way back to the generated order.

- A slug the stored order does not know goes to the end, in config order; a stored slug that no
  longer exists is ignored. Neither case may throw, and neither may lose a feature.
- Every read and write of `localStorage` is wrapped in try/catch, and the page renders correctly
  when it is empty or throws — private windows and blocked site data both do that.
- Keyboard accessible: a header is focusable and moves with the arrow keys. Drag alone is not
  enough.
- **The page says plainly that the order is remembered on this device only.** It does not follow the
  owner between his PC and his phone; cross-device ordering means write-back, which is M3.
- No drag library — pointer events. Runtime dependencies stayed exactly `@11ty/eleventy` and
  `markdown-it`, asserted by a test.

Hostile stored values were executed against it — corrupt JSON, non-arrays, duplicates, stale slugs,
throwing accessors, a 10,000-entry array — and every case returns all features exactly once without
throwing.

---

## Decisions that were overturned on the way, which is the part worth keeping

Six positions were taken and then corrected inside #780. Each correction is recorded because the
first version of each is the one somebody will otherwise reinvent.

1. **"Drop the *Passed* labels, and move the theme test to requiring the milestone id instead."**
   Corrected: the ids belong in the ladder column **only**, and repeating them in each feature's
   cells was the actual problem. The Sky contract's "pair a status colour with a label or icon" rule
   governs status chips, not chart cells — it had been cited against a thing it does not govern. The
   test requiring an id-or-"Passed" in every coloured cell was wrong and went, rather than moving.

2. **"Nothing renders for milestones that have not started; blocked and parked still earn a mark."**
   Corrected against a real case: Keystone's six milestones are all blocked with the arrowhead at
   M1, so "blocked earns a mark" would mark every row while nothing had started. **The rule is
   positional, not status-based.** Status still drives the chip on the workstream card and the phone
   view; it just does not put marks in future chart rows.

3. **"Milestones carry a start date and a days-open figure."** Days-open must be derived at build
   time, which would end the byte-identical guarantee. Settled the other way: **current and next
   show the start date only**, so nothing on the page is ever derived from today's date and the
   determinism test stands unchanged.

4. **"Show expected depth as a second, lighter arrow."** Then: "the two arrows are one object, so an
   expected depth is what makes the arrow drawable at all." Then, superseding both: **the arrow runs
   to the last milestone that has a record.** That drops the expected-depth field entirely — no new
   schema field, no owner judgement to state, nothing hand-maintained. A feature's length is how many
   milestones it has records for, which the manifest already carries. It also answered the earlier
   open question of how to show how many milestones a feature is expected to have: the arrow's
   length *is* that answer, with no count anywhere.

5. **"A feature with no milestones gets no arrow."** Corrected: the ladder's top three rows are
   stages, and progress through the stages is progress. **Every feature gets an arrow**, including
   the four that exist only in Design.

6. **The balloon spec was corrected twice.** First pass: a balloon at every feature, tail pointing at
   the arrowhead, placement solved globally across the page. Corrected to — no next step means **no
   balloon** ("nothing is next" is noise); a balloon points at **the step it describes**, not the end
   of the arrow; it **never expands into a neighbouring column** (fixed width, growing downward); its
   connector **stays inside its own column**, because the earlier gutter routing still ran between
   columns and crossed things; and **balloons do not share placement rules** — one global layout pass
   produced worse results than placing each for its own feature.

Two further inversions came out of the same issue and are not corrections but reversals of M1's own
behaviour:

- **The contiguous-run rule in `src/depth.mjs` was inverted.** M1 stopped a bar at the first gap —
  its comment said so outright — so Menus, with M5 parked and M6 through M6.3 complete, read as four
  milestones in when nine were done. Several tests asserted the contiguous behaviour by name and
  inverted with it. Completion is computed once and consumed by the chart, the phone view and
  `state.json`, which continue to agree.
- **The status vocabulary lost a word.** `gated` became `blocked`, because "gate" belongs to the
  workstream's own `gate` field — the thing the owner holds — and what a milestone records is simply
  that it cannot start. The phone view's triage vocabulary already said `blocked`, so the product now
  has one word where it had two that nearly meant the same thing. In this repository the change
  arrived as a **build failure**: v1.1.0 refused the Keystone manifest by name rather than rendering
  six blank chips, which is the closed vocabulary working exactly as decision 32 intends. PR #782.

---

## Found after it shipped

The page was **rendered in a browser for the first time on 2026-08-23** — there was a Playwright
Chromium on the machine all along, and the claim that there was not came from a stale note that was
never checked. Screenshots at 1600px light, 1600px dark and 390px phone. The structure held, and two
defects that only looking reveals did not:

1. **Arrowheads overlap the date text.** A head is wider than its ribbon and eats into the date
   column beside it — three cases visible in the fixture. The text column starts at a fixed offset
   from the ribbon's centre, which is right for the ribbon and wrong for the head.
2. **The phone view claims a next milestone that does not exist.** A feature with four milestones,
   all done and nothing recorded beyond, correctly gets no balloon on the chart — and the phone view
   says "Next: M5". That is `state.json` inventing a milestone from `headAt`/`tipLabel`, flagged in
   an earlier review as a data-semantics minor and now confirmed as user-visible, with the two
   surfaces disagreeing in front of the reader.

Both are cheap, neither is structural, and both remain open in #780.

---

## Deliberately excluded

- **Write-back of any kind** — M3. This is also why the reorder is device-local: cross-device
  ordering needs a write.
- **The phone view's own layout**, beyond keeping it correct and agreeing with the chart.
- **GitHub task lists rendering as literal `[x]`.** Decision 11 does not enumerate them, so it was
  deferred rather than fixed in a frozen file at the end of a milestone. Still visible, still open.
- **A drag library.** Pointer events only.
