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

## Parked

**Replacing the fallback card.** The generated logo-and-name card is the whole of it for now. Uploading or authoring an alternative is a later piece.

**Price requests.** MV3's "Ask for a price change" implies an inbox on corporate's side. Future dev item; the surface it needs doesn't exist yet.

**Scale as a check, not a deliverable.** No parallel set of at-scale screens. Every new screen gets pressure-tested against 20 screens and 13 menus as it is drawn, and the rule stays the same — summarize the normal, name the exception.

**Area managers, and access generally.** Picked up when we do users, permissions and roles as its own piece — who sees which venues, who can send an update, who can move a trust level. Until then "All venues" means all the venues you can see, and Group → People in MV1 is a placeholder.
