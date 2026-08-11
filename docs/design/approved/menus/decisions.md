# Vennusign back office — decisions on record

Every decision settled with the owner across the menu work. These govern the wireframes in `Menus.dc.html` (M1–M3), `Menus at Scale.dc.html`, and `Multi-Venue Menus.dc.html` (MV1–MV4b). Wireframe annotations reference these by number.

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

**33 · Imported items match the library by name.** Same name updates the item you already have; a new name makes a new one. Near-misses ("Old-Fashioned" vs "Old Fashioned") are surfaced as **one grouped question** in the confirm step — a list with a row each — pre-ticked as *the same item*, since a tidied name is the common case. Thirty near-misses is one question, never thirty.

**34 · Head office menus are only replaced by head office.** A venue cannot import over a group menu at any trust level — 86 and, at Trusted, price and item changes are the ceiling. Venues import freely into **their own local menus**, with the same four routes.

**35 · Time lives in Schedules, not in Menus.** A menu has no hours of its own; Schedules points at menus. Menus owns only what happens when nothing is pointed at a screen.

**36 · One fallback, every empty moment.** The venue fallback covers a scheduled gap, a menu taken off the screens, and anything else that empties a screen — the same object in all cases (decision 14). For now it is the generated logo-and-name card only; authoring a replacement comes later.

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

## Parked

**Replacing the fallback card.** The generated logo-and-name card is the whole of it for now. Uploading or authoring an alternative is a later piece.

**Price requests.** MV3's "Ask for a price change" implies an inbox on corporate's side. Future dev item; the surface it needs doesn't exist yet.

**Scale as a check, not a deliverable.** No parallel set of at-scale screens. Every new screen gets pressure-tested against 20 screens and 13 menus as it is drawn, and the rule stays the same — summarize the normal, name the exception.

**Area managers, and access generally.** Picked up when we do users, permissions and roles as its own piece — who sees which venues, who can send an update, who can move a trust level. Until then "All venues" means all the venues you can see, and Group → People in MV1 is a placeholder.
