# Done Record

- **Milestone / PR:**
- **Head commit this record describes:**
- **Author:**

Every item below is answered. The allowed answers are:

- **PASS** — followed by what was actually done, in specifics. Name the action taken and what was observed. "Tested" alone is not an answer.
- **N/A** — followed by the reason it does not apply to this change.
- **UNTESTED** — followed by why, and what risk that leaves.

A blank item means this record is incomplete and the PR does not merge. An answer that restates the checklist item is not an answer.

## Paths

List every path this change carries — not just the happy path; a function usually has several legitimate routes through it. For each: how it was attacked, and the result.

| **Path** | **How it was attacked** | **Result** |
| --- | --- | --- |
|  |  |  |

Paths known but not attacked are listed here as UNTESTED with the reason:

## Design questions

Asked of this change as designed, before and after building:

- Does it make sense for this to be reviewed or skipped, given the actions before it?
- Can the user edit it?
- Can the user go back a step?
- Is delete needed? Is edit needed?
- Does it need to talk to another function?
- Is every word shown to the user plain and clear?
- Are there paths that should exist because they'd make this easier to use in context?
- Should this be drag-and-drop?

Answer each:

## The behaviour

- Happy path works end to end:
- Loading, empty, disabled, error and retry states:
- Validation on first entry and when editing an already-saved value:
- Error recovery — retry without losing what was entered:

## The data

- New data and existing saved data, both:
- Empty, invalid, minimum, maximum, very long, duplicate values:
- Partially completed data:

## Navigation and persistence

- Back preserves entered values:
- Browser refresh mid-flow:
- Leave and return; resume an interrupted flow:
- Cancel, and close:
- Edit after completion:
- Double-click and repeated submission:

## Access

- Each role, each tier:
- Permission granted and denied:
- Feature enabled, disabled, unavailable, upgrade-required:
- Role or tier changed after the data was created:

## Integration (where a service is involved)

- Success, failure, timeout, partial response:
- Retry after failure:

## Display (where UI changed)

- Smallest and largest supported widths:
- Long labels and overflow:
- Zero records, one, many, and more than fit:

## The multiplier

- Every location the same behaviour lives — searched, listed, all fixed (paste the search command and results):
- Every consumer of any shared component touched:
- Nothing adjacent broken — quick regression of the surrounding flow:
