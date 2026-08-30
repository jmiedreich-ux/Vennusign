# Vennusign back office — decisions on record

Every decision settled with the owner across the menu work. These govern the wireframes in `Menus.dc.html` (M1–M3), `Menus at Scale.dc.html`, and `Multi-Venue Menus.dc.html` (MV1–MV4b). Wireframe annotations reference these by number.

## Architecture-renewal amendment — 28 August 2026

**Owner ruling.** Menu is the first content type in the shared VennueSign Content Platform. Its
accepted customer behavior remains binding; its current menu-only persistence and API shape do not
become permanent architecture.

The successor implementation is `menu.v1`, an immutable versioned model used by Content Builder,
Theme Studio, publishing, and the renderer. It must preserve these decisions as acceptance
behavior:

- price belongs to the placement, while an item's library value is only its default;
- 86/availability is a venue-scoped operational fact, immediate rather than queued;
- imported/provider data has explicit source authority and never silently changes unrelated menus;
- drafts, history, assignments, publishing, delivery evidence, and lifecycle actions remain
  deliberate and truthful.

A typed record library replaces the Menu-only Item Library as the underlying capability. It can
hold reusable and imported records, including eventual Toast items, films, and showtimes. A model
declares whether a collection is inline, manually composed, a library reference, or provider-query
driven. Theme revisions bind to explicit model fields and define how a state responds visually.

This amendment does **not** authorize an unbounded rewrite or change current customer behavior.
Correctness and closure work may continue. New Menu persistence, theme, render, or API foundations
must follow `docs/architecture/content-platform-architecture-renewal.md` and start only through
a bounded implementation milestone.

## Publishing and state

**1 · Explicit publish, everywhere.** Nothing reaches a screen without a deliberate act. Per-field autosave-to-live is gone at every tier.

**2 · One queue per menu.** Every edit — price, copy, structure, layout — waits in that menu's draft and ships together when you publish it.

**3 · 86 is always immediate.** Availability never queues and never waits for a publish. It is a fact about the world that is already true; everything else is an intention.

**9 · Supersession is never exposed.** It survives only as "replaced by" inside a history entry. It is not an action anyone takes.

**10 · "Unpublish" → Take off the screens.** Never a bare action: it states what replaces it, in the same click.

**11 · "Restore" → Go back to…** Phrased as time, not versions. It produces a draft you then publish — never a second silent path to the screens.

## Tiers and capability

**4 · Locked by plan means invisible.** A capability outside your plan is absent, not disabled — no ghost fields, no reasons, no state. Upgrade discovery is solved separately, later.

**5 · Blocked is not the same as absent.** Permission, disconnection, limits and offline targets are real states and must say exactly what they are. Only these belong in the state vocabulary.

**6 · Every capability independently switchable.** Nothing in Menus may assume a neighbour exists. A static-content screen is its own designed outcome, not the builder with parts removed.

**19 · Menus is itself tier-gated.** A venue whose plan is one screen of static content has no menu to build. The Menu nav item does not render at all — no shelf, no import, no empty state. Its content home is a different, purpose-built screen.

## Undo, history, evidence

**7 · Undo is a keystroke, not a feature.** Session-scoped, quietly capped, never named in a settings page or plan comparison.

**8 · History is a separate capability.** Durable and attributable. The tiered thing is retention depth — how many versions you keep.

**12 · Delivery evidence scales by sentence.** The promise is "you can always tell whether your screens are current." A sentence at one screen, a list at several — never a table that shrinks to a stub.

**13 · Preview only against real screens.** Exact target, from device-reported geometry. There is no representative size — an unpaired screen cannot be previewed against at all.

**14 · Fallback is chosen, not generated.** A venue picks what shows when nothing is published. Until then, a generated logo-and-name card — a real, visible, replaceable object, not plumbing.

## Editing and getting content in

**15 · Quick Update stays a separate path.** Changing "sold out" must not require the full editor. Not a view toggle on the builder.

**Owner amendment, 2026-08-13 — the 86 board.** Quick Update is a three-column operational surface: bold rail labels are on-screen menus and their indented labels are authored sections; the center shows available placement tiles for the selected section; the right panel shows every currently 86'd on-screen placement across menus. A shared item is repeated once per placement so location stays visible, but availability remains one venue-scoped fact: every tile action requires confirmation, taking it off affects every published placement, and **Back on sale** restores it everywhere. Search is limited to items in currently published menus assigned to screens. Offline and stale screens never block the fact change and are named honestly in the result. Carryovers are items still off from an earlier venue day; Review focuses them and never restores automatically. Items on no published screen do not appear. Guest copy is **Sold out**; staff copy is **86**. The separate **New menu / Start blank** flow remains in the same milestone but does not live inside the 86 board. Planning-sheet decisions are `VennuSign Planning` Q12–Q20; these IDs are not the feature question-register Q12–Q20.

**16 · Source-controlled fields are deferred.** POS-authoritative pricing with override, freshness and last-known-good is an advanced add-on, designed later.

**17 · Getting a menu in is permanent.** Photo, paste and spreadsheet import live on the Menus home forever — not in a signup wizard. POS import is an add-on and appears only when attached.

**18 · Confirm only what we were unsure of.** After any import, surface the rows the machine could not read — never the ones it could. Governs photo, spreadsheet and POS sync alike.

## Scale

**The governing rule.** Summarize the normal, name the exception. Never truncate, never paginate a list of screens. Twenty screens becomes "18 current · 2 need you", and only those two are drawn. One screen becomes a sentence. The length of a list tracks what is wrong, not what exists — which is what lets the same component serve a 1-screen café and a 40-screen stadium without a redesign.

## Multi-venue

**20 · A menu is one object, not twelve copies.** Group menus are authored once. Each venue holds a *state* on that menu — running, pending, behind, overridden. Venues can also author their own local menus, which sit in the same list tagged **Local**.

**21 · Default view is all venues.** Nobody switches venue to see the state of the world. The list shows every menu across the group; filters narrow it. A single-venue account sees the identical component with the venue column collapsed to nothing.

**22 · Corporate pushes, venues accept.** Publishing to selected venues creates a pending change at each. The operator accepts on their own schedule — never mid-service, never a surprise mid-shift.

**23 · Not accepting is loud, not silent.** Day 1 a badge, day 3 a banner they must dismiss, day 7 a daily email to the operator and their manager. Nothing auto-applies. A venue that never accepts becomes **Behind** on corporate's row — a visible number, not a hidden one.

**24 · Permission is a trust level, not 72 checkboxes.** Venues sit on named levels — **New**, **Standard** and **Trusted** ship as defaults, and a group can create their own, up to **eight in total**. A level is always defined as a difference from an existing one, so it stays describable in a sentence. Eight is a real ceiling, not a technical one — past that nobody can say what a level means without looking it up.

**25 · 86 is venue-level, always.** An 86 never leaves the venue that made it — group menu or local, it's the same instant toggle and toast as decision 3, with no prompt and no scope question. When a supplier fails group-wide that isn't an 86 at all: head office removes the item from the menu and sends an update.

**26 · An override is a fact on the row, never a fork.** A Trusted venue changing a price does not fork the menu. Their row reads "running · 2 local changes", corporate can see exactly what they are, and the next group publish says which of them it would overwrite.

**27 · Every venue has a default user.** A named person, required at venue setup — the address the escalation goes to and the name on "Behind 9 days". A venue without one can't be added to the group; a shared bar login is a device, not a person.

**28 · Apply it for them exists, and can be switched off.** Corporate's last resort when a venue never accepts. A group setting, so a franchise whose agreement forbids it turns it off once and nobody sees it again.

**29 · Multi-venue is a tier, and it's invisible below it.** Decision 4 applied to the group dimension: single-venue plans get no Venues nav, no trust levels, no venue chip — the menu list is the identical component with the venue column collapsed to nothing. Not self-serve either: a group account is set up with them, not bought in a modal.

**30 · Import ends where it begins.** Photo, paste, spreadsheet and POS differ only in what they ask for. All four converge on the same confirm step and land the same way — a draft on the shelf, nothing on a screen.

**31 · Spreadsheet headings are published, order is free.** We name the headings we read (**Item** and **Price** required; Description, Section, Sold out, Size optional), accept them in any order, any case, alongside any extra columns. A template is offered, never required. There is no mapping step — a heading either is one of ours or it is theirs, and only theirs is worth a question.

**32 · Importing into an existing menu replaces it.** Not a merge. A reprint is the new menu, so the file wins outright — no row-by-row reconciliation to design or explain. The board keeps its layout and theme, and anything currently 86'd stays 86'd (decision 3: availability is a fact about tonight, not about the menu). Same four routes, same confirm step, queued as a draft like any other edit.

**33 · Imported items match conservatively.** An exact name, including normalization limited to case, punctuation and spacing, may match automatically. Anything semantic—including an obvious-looking typo—is an operator identity decision. Near-misses are surfaced as **one grouped question**, never thirty separate questions, and no row is preselected. The fast path may accept a bounded set of safe normalization matches; ambiguous rows remain unanswered until the operator chooses **Same item**, **New item**, or another close library candidate.

**34 · Head office menus are only replaced by head office.** A venue cannot import over a group menu at any trust level — 86 and, at Trusted, price and item changes are the ceiling. Venues import freely into **their own local menus**, with the same four routes.

**35 · Time lives in Schedules, not in Menus.** A menu has no hours of its own; Schedules points at menus. Menus owns only what happens when nothing is pointed at a screen.

**36 · One fallback, every empty moment.** The venue fallback covers a scheduled gap, a menu taken off the screens, and anything else that empties a screen — the same object in all cases (decision 14). For now it is the generated logo-and-name card only; authoring a replacement comes later.

## Paste-import amendments — 13 August 2026

Approved storyboard and supporting artifacts live in `paste-import/`. These decisions supersede Q83, Q84, Q92, Q93 and Q94 where those earlier answers conflict.

**37 · Final confirmation is the only menu mutation.** Parsing and review persist an import session, not menu working rows. The operator resolves every required question, chooses create or replace after review, and then confirms once. Confirmation is atomic and idempotent; a refusal rolls everything back while preserving the session and its valid answers.

**38 · Completion tells the publishing truth.** A successful import creates or replaces unpublished working state only. Screens and the published snapshot remain unchanged until a later Publish. Both outcomes say **Not live yet** and offer **Review draft in builder** or **Done for now**; the product never auto-navigates or implies that a screen changed.

**39 · Review answers survive only while their dependencies do.** Refresh preserves answers whose source line and candidate identity remain unchanged. A changed target, parser result, permission, allowance or candidate invalidates only affected answers, and the UI names what was cleared and why.

**40 · Replacement explains and preserves.** The server computes a structured delta against the target's published snapshot and returns a plain-language unpublished-change breakdown. Replacement preserves menu identity, theme, assignments, published snapshot and active availability/86 state. Before replacement, the transaction records a restorable version of the complete unpublished working state.

**41 · Imported items is one predictable fallback.** Every pasted line remains traceable and appears exactly once. Content that cannot be placed confidently lands in one customer-facing **Imported items** section. Parser-cause metadata is retained for plain-language explanation and diagnostics, not used to create multiple automatic sections. An eligible unreadable line becomes a section only through an explicit reversible operator action.

**42 · Retention is configuration and tier policy.** Preserve all historical replacement snapshots. A centralized configuration table resolved by subscription tier controls snapshot scope, retention, restore eligibility, tier limits and import-session lifetime. Only a successful user mutation may renew an import session; passive reads do not. The UI shows the resolved absolute expiry. Expired raw paste and derived review data are deleted together.

**43 · Paste import has no silent cross-menu price effect.** A pasted price change is menu-scoped. If the persistence model shares price globally, confirmation must create an explicit menu-specific override/copy or refuse until the operator knowingly chooses a broader action. An import never silently changes another menu.

## Amendments — 11 August 2026

Settled with the owner during the Menu Builder V2 session. Each one changes
something already on this list or in the V2 handoff; the numbered decisions above
stand except where named here. Governs `menus/Menu Builder Preview.dc.html`,
`menus/Menu Builder - connected screens.dc.html` and the action inventory beside
them.

**A1 · Sections live inside the page, not in an outer rail.** V2 §4 put the
section list in the left rail. It now sits inside the page panel, one column left
of the board, so everything belonging to a page is contained by the tab that
represents it. The rule's intent is unchanged: sections are scoped to the selected
page, and the list is never a second page navigator.

**A2 · History is page-scoped where it is shown, menu-level where it is kept.**
Decision 8 stands — history is durable, attributable, and tiered on retention
depth. What changes is placement: the page's own history sits inside its panel,
filtered to that page, with menu-level facts and the route to full history at its
foot. Menu events never appear under a page heading.

**A3 · 86 is staff vocabulary; Sold out is guest vocabulary.** The control, the
message and the history entry all say 86, because that is what a kitchen says. The
board says **Sold out**, drawn by the theme (V2 correction #8). One word for the
people who set it, another for the people who read it.

**A4 · An 86 never queues, but its cancellation can.** Decision 3 is unchanged for
the act itself. Switching *Available* off cancels an existing 86, and that
cancellation rides with the hide's publish rather than firing immediately —
otherwise the item goes back on sale between the toggle and the publish, which is
the opposite of what the operator just said.

**A5 · Imports never carry availability.** *Sold out* is recognised as a
spreadsheet heading and then deliberately dropped, listed with the columns that do
not come across. This amends decision 31, which had it among the optional headings
we read. A printed menu cannot say what ran out tonight and neither can a file — an
86 is a person's statement, and no import may change what guests can order.

**A6 · Nothing is edited on the board except a new item's name.** Q118 allowed
in-place price editing. It is withdrawn: adding is the only inline act on the
board, and every edit happens in the panel. One rule instead of an exception.

**A7 · Publish, exit, discard and restore live behind one menu.** V2 §9 lists the
routes without ranking them. They now sit behind a single *Finish* control in the
footer, with the change count removed from the bar and stated inside the menu. The
count returns to the bar if operators start publishing late.

**A8 · "Restore" is permitted wording again.** Decision 11 banned it because it
read as version control. With restore-from-history deferred past this milestone and
the Finish menu action producing an ordinary draft that still needs publishing,
the word is accurate: **Restore an earlier version**. It leaves the banned-words
list; *unpublish*, *supersede* and *archive* stay on it, and the assertion in
`tests/ui/specs/menus-shelf.spec.ts` is amended in the same PR.

**A9 · Viewing scope is chips, collapsing past five.** *Whole page* plus every
section, visible rather than hidden behind a select. Past five sections the extras
collapse behind *More*; the row never scrolls or wraps.

**A10 · Items move between sections by drag.** V2 §7 described reordering within a
section only. Dragging across sections moves the item, including into an empty one.

**A11 · Adding an item, in six parts.** It lands at the end of the section in
scope, with the caret in its name. A name is required and a price is not — blank
and `MP` are both legal. Abandoning it blank discards it silently. An unnamed item
is listed at Review & publish to finish or drop, and does not block the other
changes. An 86 is unavailable until the item has been published once, and says why
rather than sitting inert.

**A12 · Removal has one name.** *Remove from this page* on both the board and the
inspector, with a confirmation saying the item stays in the library and on any
other page using it. *Delete* is not used on this screen, because nothing here
destroys an item.

**A13 · The theme owns the rotation interval.** Menus sets the order of pages on a
screen and nothing more; Screen Assignments displays the interval read-only. This
also settles two menus sharing one screen — neither of them argues about timing.

**A14 · Unresolved overflow publishes, named.** Capacity may not be silently
clipped (V2 §7), and it may not block a publish either. Every dropped item is named
at review with where it sits, acknowledged by a tick, and a *fix the fit* route
sits beside the tick.

**A15 · History shows what review showed.** There is no diff view. Each publish
stores the summary displayed at review and the row expands to show it back, so
history can never disagree with what the operator saw when they pressed publish.
Above five events of a kind in a day, rows group; publishes never group.

**A16 · Everything ships on, gated individually.** The build target is the
maximum-tier screen with every capability present and working, each wrapped in its
own capability check. Decision 4 still governs what absence looks like — a control
outside the plan is gone, not disabled — but the ladder can move without touching
layout.

## Owner overrides — 11 August 2026

**O1 · Existing board content may be edited inline.** This supersedes A6 and
action-inventory E2. Clicking a section heading edits its name in place. Clicking
an item's name, description, or price edits that field in place using the rendered
theme typography; the corresponding inspector field receives a quiet location cue.
The inspector remains a complete alternative editing route. Inline editing must
remain aligned while the canvas scrolls, preserve prices exactly as typed, save by
the same product endpoint as the inspector, and allow Escape to cancel without
changing the saved value. The owner approved this interaction after live testing.

## Owner overrides — 24 August 2026

**O2 · The consolidated footer menu is "Actions," not "Finish."** Corrects A7's
control name. "Finish" reads as completing the menu itself, not as opening a menu
of actions on it — Review & publish, Save & exit, Discard and Restore are things
you can do at any point, not steps toward a done state. A7's content and
consolidation stand: same four actions, same one control, same reasoning. Only the
label changes.

**O3 · One field style for every dialog, using the app's own existing tokens.**
Several dialogs (Move section to a new page, Delete section's destination picker,
Delete page's destination picker) had a bare `<input>`/`<select>` with no border,
radius, or focus treatment of its own — falling back to the raw browser default,
which read as an unstyled, "thick blue" ring dropped into an otherwise designed
surface. The fix is not a new visual language: `sky-ui-tokens.css` already defines
`--sky-focus-ring` / `--sky-focus-color` / `--sky-input-radius` /
`--sky-color-border-input`, used elsewhere in the app (`destructive-review-dialog`,
`menus-home__search`); the dialogs here simply hadn't adopted them. The standing
rule going forward: any text `<input>` or `<select>` inside `.builder__dialog`
gets a real 1px border in `--sky-color-border-input`, `--sky-input-radius`
corners, and `--sky-focus-ring` on focus, declared once and inherited by class —
not reinvented per dialog. Radio inputs take `accent-color: var(--sky-color-primary)`
rather than the browser default. This does not redesign any one dialog's layout
or copy; those remain their own listed pieces of work.

## Amendment — 27 August 2026

**A19 · A price belongs to a placement, not to an item.** Q5 settled on 2026-08-07 that one item
carries one shared price across every menu it sits on, with per-menu variation named as a future
feature. Withdrawn by the owner on 2026-08-27: a dish may cost different amounts on different
menus. `Items.Price` remains, demoted to the default a dish carries when it is placed somewhere
new — never a fact one menu can change underneath another.

The evidence that settled it was a real menu. It carries the same dish in two sections, and it
prices whole sections per protein — *Chicken $11.95, Beef $12.95, Shrimp $13.95* — which the model
could not hold at all, so the import took the first price and printed the rest in the description.
That is a workaround for a model answering the wrong question.

Decision 3 is untouched: **86 stays item-level and venue-wide.** Availability is a fact about
tonight; price is a fact about a menu. Nothing here changes what an 86 does or reaches.

**A20 · A price change that could mean two things asks which.** Owner ruling, 2026-08-27, settling
the question A19 left open: when a dish already sits on several menus, does changing its price
change one menu or all of them?

Neither, on its own. **It asks.** *"Pad Thai is on 3 menus. Change the price here only, or on all
3?"* Answering for the operator was rejected in both directions — silently changing every menu is
the behaviour A19 withdrew, and silently changing one leaves the other menus wrong with nothing
said.

Three things follow, and they are the whole of it:

- **The question is asked only when there is something to be unsure about.** One placement is not
  ambiguous. Asking anyway is the noise A18 and decision 18 rule out — confirm only what we were
  unsure of.
- **"Here only"** writes that placement and nothing else, which is what the builder already does.
- **"On all of them"** writes the library default *and* every placement of that dish, so the answer
  is true straight away rather than only for the menus that happen to carry no price of their own.

A19 is unchanged: a price still belongs to the placement. A20 decides who may change several
placements at once, not where a price lives. **Q112 stays overturned.**

**A21 · Two candidates that look the same are two candidates.** Owner ruling, 2026-08-27. A venue
library can hold the same dish twice at the same price — an older import split it — and the review
screen offered both as *"Use the one you already have — Pad Thai $12.95"*, twice, identically.

Merging them silently was rejected. The screen names what makes them different — which menus each
is on, and when it was made — and the operator chooses on that basis. A duplicate the venue can
see is a duplicate the venue can deal with; one the product quietly resolves is one nobody ever
finds out about.

## Proposed — 27 August 2026 (awaiting owner sign-off)

Nothing in this section is settled. It is written up because the owner raised it directly and the
answer changes behaviour that A21 already shipped.

**A22 (proposed) · An ambiguous match is not an uncertain one, and only one of them is a question.**

The owner, looking at a real review screen: *"there should not even be a question here."* He is
right, and the decisions already in this file say so — they were simply not applied to this case.

What he was shown: two rows under **one grouped question**, each offering the same four choices,
and the first two choices reading identically — *"Pad Thai · Possible match · $11.95 · On New menu
· Added Aug 27"*, twice. The pasted lines were `Pad Thai` (line 52) and `Pad Thai $11.95` (line
111). The library holds `Pad Thai` twice, at the same price, on the same menu, created the same
day.

**Why this should never have been asked.** Decision 33 says an exact name match — normalization
limited to case, punctuation and spacing — *may match automatically*, and reserves operator
identity decisions for anything **semantic**. Decision 18 says confirm only what we were unsure
of. `Pad Thai` against `Pad Thai` is an exact match. Nobody was unsure of anything.

**What actually happened** is a different failure wearing the same clothes. `MenuPasteParser`
auto-answers when exactly one candidate matches and the price agrees; with more than one candidate
it always asks, whatever the candidates are. So the screen was not reporting *uncertainty about the
pasted line* — it was reporting *ambiguity inside the library* and asking the operator to resolve
it. Those are not the same question, and only the first belongs on the review screen.

A21 was written for the second failure and does not reach this case. It adds a provenance line —
which menus each candidate is on, when it was made — but only *prints* that line; it never checks
the lines came out different. When the duplicates agree on menus and date, A21 renders the same
sentence twice and the choice is exactly as unanswerable as before, with more words on it.

**Proposed, in three parts:**

- **Do not ask.** Where every exact-name candidate is indistinguishable to the operator — same
  name, same price — and the pasted price agrees, answer it the way a single candidate is
  answered: bound to one of them deterministically (oldest by creation, then by id), recorded, and
  listed under *Review all N pasted lines* so it can be found and changed. This is not the silent
  merge A21 rejected: both library items survive untouched, and only the line's link is decided.
- **When a question must still be asked, do not pretend it distinguishes.** If the pasted price
  differs, the operator does have something to decide — but the identical candidate labels remain
  a lie. The screen should say the candidates cannot be told apart, rather than print the same
  provenance twice.
- **The duplicate is the real defect, and it has nowhere to go.** Two `Pad Thai` rows exist
  because an earlier import created the dish twice. There is no library surface in the product —
  no route lists library items; the library is reachable only as a search box inside the builder —
  so a duplicate a venue can see is still a duplicate it cannot do anything about. A21's reasoning
  ("a duplicate the venue can see is a duplicate the venue can deal with") assumes a screen that
  does not exist.

**What is not proposed here.** Price stays where decision 43 put it: a pasted price change is
menu-scoped and never silently reaches another menu, and the *which did you mean* price question
is A20's, asked in the builder. A differing price does not earn an identity question on this
screen either.

**The open question for the owner**, and the reason this is a proposal and not a fix: deciding a
line's link deterministically means the product picks one of two items the operator cannot tell
apart. That is the right outcome only if duplicate library items are eventually visible and
mergeable somewhere. If they never are, this hides the duplicate instead of dealing with it — and
the honest alternative is to stop the import creating the second `Pad Thai` at all.

## Parked

**Replacing the fallback card.** The generated logo-and-name card is the whole of it for now. Uploading or authoring an alternative is a later piece.

**Price requests.** MV3's "Ask for a price change" implies an inbox on corporate's side. Future dev item; the surface it needs doesn't exist yet.

**Scale as a check, not a deliverable.** No parallel set of at-scale screens. Every new screen gets pressure-tested against 20 screens and 13 menus as it is drawn, and the rule stays the same — summarize the normal, name the exception.

**Area managers, and access generally.** Picked up when we do users, permissions and roles as its own piece — who sees which venues, who can send an update, who can move a trust level. Until then "All venues" means all the venues you can see, and Group → People in MV1 is a placeholder.
