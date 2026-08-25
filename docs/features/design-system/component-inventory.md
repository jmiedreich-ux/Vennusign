# Component inventory — what the back-office actually needs

**Status: for owner review. Nothing built, nothing decided.**

This is a count, not a design. Purpose: turn "247 buttons and no idea where to start" into a bounded list you can
actually hold in your head and reason about.

## The finding that matters

**The 247 buttons are not 247 things. They are about four things, repeated.**

| Role | Written as | Count |
|---|---|---|
| Primary action | `action-primary`, `import-primary`, `upgrade-modal__primary`, `upgrade-sheet__primary` | 15 |
| Secondary action | `action-secondary`, `import-secondary`, `import-back`, `upgrade-modal__later`, `upgrade-sheet__later` | 32 |
| Destructive action | `action-danger`, `danger`, `builder__quiet-danger` | 11 |
| Inline link | `builder__link`, `builder__crumb-link`, `restore-link` | 8 |
| **No styling at all** | *(no class — browser default)* | **97** |
| Screen-specific one-offs | `menu-card__open`, `menus-home__add-tile`, `rail-identity`, others | ~84 |

Four roles, named eleven ways, plus 97 that were never styled at all. That last number is the real story: **40% of
the buttons in this app have no design applied to them whatsoever.** Not drifted — never dressed.

This is why the mock-to-code gap keeps reappearing. A mock is a picture; a picture cannot be reused. Someone reads
the picture and writes an approximation, once per button. Do that 247 times and drift is not a risk, it is
arithmetic. The gap closes when the component becomes the shared object between design and code — one thing, rather
than a picture and a re-implementation of a picture.

## What already exists and works

The pattern is not foreign to this codebase. Nine shared components are already built and genuinely reused:

| Component | Reused in | What it already does |
|---|---|---|
| `DestructiveReviewDialog` | 11 files | Typed-confirmation flow for dangerous actions |
| `VennusignLoader` | 6 | Branded loading state |
| `TransientFeedback` | 5 | Toast notifications, with reduced-motion handling |
| `LoadingSkeleton` | 4 | Placeholder while content loads |
| `SkyIcon` | 4 | The icon set, one consistent stroke and weight |
| `EmptyState` | 4 | "Nothing here yet" presentation |
| `TierBadge`, `EntitlementLockChip`, `LockedNavigationItem` | 2 each | Plan/tier presentation |

The library stopped at the specialized components and never covered the ordinary ones. That is the gap — not a
missing habit.

**And real behaviour is already written, just not packaged.** `useDialogFocus` — focus trap, focus return, Tab
containment — is written once and used **13 times**, wrapped in 17 separately hand-built dialog shells. Alongside it:
18 hand-written Escape handlers, 12 hand-wired scrim dismissals, 99 hand-built label/input pairings. This is exactly
the "functionality that goes into them" worth preserving. A component library built from appearance alone would
throw it away.

## The list — 14 components

Grouped by how much thinking each needs.

### Group 1 — Plain controls (5)

Mechanical. Behaviour is well understood; only appearance is contested.

1. **Button** — four types (primary, secondary, destructive, inline link). Absorbs all 11 current names and dresses
   the 97 undressed ones.
2. **Text field** — 111 inputs, 9 textareas. Label handling already established in 99 places.
3. **Dropdown** — 27 selects, styled ad-hoc per dialog today.
4. **Checkbox** — only 8 in the app, all browser-default. Designed, never built. Needed by the Review and Publish page.
5. **Radio group** — 10 uses, all browser-default. Currently only in the delete-section and delete-page dialogs.

### Group 2 — Controls that already carry behaviour (4)

The valuable ones. Each has working logic that must survive the move.

6. **Dialog** — 17 hand-built shells around one shared focus hook. Packaging this captures focus trapping, focus
   return, Escape, scrim dismissal, and background inerting in one place instead of 17.
7. **Toggle / switch** — the Available and 86 controls. Carries real meaning (86 is immediate and guest-visible;
   Available is a drafted change) that the appearance should probably express.
8. **Dropdown menu** — 3 uses (section actions, page actions, the Actions menu). Click-outside-to-close and keyboard
   handling already written per instance.
9. **Search / typeahead** — 1 use (add-item), but a real one: live results, keyboard selection, "create new" fallback.

### Group 3 — Presentation, no interaction (5)

Already partly shared; mostly needs consolidating.

10. **Status chip** — 36 uses. Online/offline/stale/live/warning states.
11. **Empty state** — 28 uses, component exists, not used everywhere.
12. **Toast** — 16 uses, component exists and is good.
13. **Loading skeleton** — 13 uses, component exists.
14. **Banner / callout** — the capacity warning, sign-back-in prompt, save-failed line. Currently one-off each.

### A fourth family, deliberately out of scope

`EntitlementLockChip`, `LockedNavigationItem`, `LockedSectionPreview`, `SidebarUpgradeNudge`, `TierBadge`,
`UpgradeModal`, `UpgradeSheet` — seven components for showing locked capability and prompting an upgrade.

At first read these look like they contradict decision 4 ("locked by plan means invisible — no ghost fields, no
reasons, no state"), and `ThemeBuilder.tsx` does render Pro controls greyed rather than absent. They do not.
**Build-decision 9 already settled it:** Menus follows decision 4 strictly, out-of-plan is absent; the app's
existing upgrade and marketing surfaces are untouched, and upgrade discovery is reworked as its own later piece.

Recorded here because the boundary is easy to mistake for drift, and because these seven are a coherent family that
will want its own treatment whenever that rework happens — not folded into this component set now.

## Deliberately excluded

Screen-specific compositions that only appear once and should stay where they are: the menu board renderer, the nav
rail, the publish bar, menu cards, the 86 board's three-column layout, import review tables. These are *screens*,
not components. Making them "reusable" would be inventing a requirement that does not exist.

## What the design project adds — as claims, not answers

Read after the counts above, on the owner's steer that *"that's where the wish starts from."*

**Read every line below as "the design project says," never as "this is true."** Owner, 2026-08-25: *"we can't
trust what's in there because changes have happened after — it's three ways."* The design project, the written
spec, and the code each move independently, and any one of them can be the stale one. Four ways this has already
bitten, all real:

| Direction | Example |
|---|---|
| Code newer than design | The focus ring — design says 3px sky blue, code changed it to 2px dark teal for a contrast fix that sky blue genuinely fails |
| Design newer than code | The identity explorations, which may describe thinking that never reached the app |
| Both stale, superseded by a decision | The Review mock's "unnamed item" section — A11 later made a name required, so it describes something now impossible |
| Reads like drift, actually settled | The entitlement components vs decision 4, resolved by build-decision 9 |

So the items below are leads to verify, not conclusions.

**The project README claims a canonical reference.** It calls `menus/M1 Hi-Fi v2 - Menus home.dc.html` *"the visual
reference every other area matches."* That is a statement in a README of unknown date — not a verified fact about
what ships today. Worth checking against code before treating it as governing anything (T10).

**Theme Studio is designed in the project and unbuilt in the repo.** Five storyboards (TS1–TS5), a theme editor,
identity and rail explorations, a 28-state storyboard and a PSA review exist there; `docs/features/theme-studio/`
holds only a `workstream.json` with zero milestones. *If* those storyboards are still current, a second consumer
with no legacy code is an attractive place to prove skinning — but nothing has confirmed they are current, and
"designed a while ago, never built" is exactly the condition under which later decisions supersede a design without
anyone updating it (T12).

**A stale note, proving the point.** `themes/notes.md` says "Nothing designed yet" while TS1–TS5 sit in the same
folder. The same file flags the theme builder's disabled Pro controls as contradicting decision 4 — which
build-decision 9 had already settled. One short file, two things out of date. This is the strongest available
evidence for not trusting any single source.

**Unread.** The identity explorations (`Identity D`, `E`, `E2`), rail options (`Rail option A`, `B`), and the
Screens hi-fi. None compared against code (T11).

## What this means for the overhaul

Two things follow from the counts, both relevant to "build to now, but allow the new to be used":

**The work is smaller than it looks.** 14 components, 9 of which either exist already or are mechanical. The
frightening number (247) collapses to four button types.

**Skinnable is achievable, and it is the migration strategy.** If each component keeps its behaviour and takes its
appearance from a swappable skin, then a UI overhaul is a second skin over components that already work — not a
rewrite. Screens cross over when their skin is ready. That is what makes "old and new coexisting" practical rather
than a maintenance burden, and it is why building the component layer comes before deciding what the new look is.

**Owner, 2026-08-25 — skinnable is a requirement, not an option.** *"Theme studio will definitely have a different
skin than back office, but will probably use a lot of the same controls."* Two consumers with deliberately different
looks sharing one control set is the case skinning exists for, so this stops being a design choice and becomes a
constraint on every component's API.

Two consequences, both open (T8, T9 on the milestone issue):

- **Is a skin chosen per app, or per context?** Per app, the choice lives in a wrapper and components never know
  about it. Per context, a component has to know where it is rendering. This changes every component's shape, so it
  wants answering before the first one is written.
- **This inventory covered the back-office only.** Theme Studio is now a confirmed second consumer and has not been
  counted. Its shared controls may differ from the fourteen here, and the design project holds storyboards for it
  (TS1–TS5) plus identity explorations that have never been compared against code.

## Suggested first step

**Group 1's Button, applied to the 97 undressed buttons.** It is the highest-visibility change available (40% of the
app's buttons currently have no design at all), it is mechanical rather than contested, and it proves the
skin-swapping approach on something low-risk before anything harder depends on it.

Not proposed yet — this document is the inventory you asked for, so there is something concrete to react to instead
of a blank page.

## Note for whoever works on Atlas next

Two small things found while putting this milestone's tasks into Atlas. Neither blocks anything; both were worked
around. Recorded here because the Atlas session that would have received them has since ended.

- **Task text renders as plain text, so a markdown link shows raw.** The owner asked that each task link to the
  doc section explaining it, which makes this immediately visible — a full URL sits in the middle of the task line.
  Worked around by writing `` `path#anchor` `` instead, which reads acceptably in Atlas and stays clickable in
  GitHub. Rendering markdown links in task text would be a genuine improvement.
- **`depth` rejects `0`.** Hit while adding an M0 ahead of an existing M1–M4, wanting depth to mirror the milestone
  number. Worked around by renumbering 1..n. Arguably depth is ladder position and should not care what the
  milestone is labelled, so `0` may be worth allowing.
