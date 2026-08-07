# Handoff: Vennusign Back Office — Menus

## Overview

The **Menu** area of the Vennusign back office: where a venue gets its menu into the system, builds the board that appears on their TV screens, checks it, and publishes it. Covers the single-venue experience end to end, plus the group/multi-venue layer that sits on the same screens at a higher tier.

This replaces an existing implementation whose core problems were: too many controls, no way to see what a change would look like, slow 86 (sold-out) updates, and three different save models on one screen. The design resolves those into a single rule set — see `decisions.md`, which is the authoritative document for this feature.

**Read `decisions.md` first.** Thirty-six numbered decisions, each settled with the product owner. They are written as rules, not descriptions, and the wireframes annotate themselves by decision number. Where this README and a decision disagree, the decision wins.

---

## About the design files

The files in this bundle are **design references created in HTML** — prototypes showing intended look and behavior. They are **not production code to copy**.

The task is to **recreate these designs in the target codebase's existing environment** using its established patterns, component library, and conventions. The real product is a React back office (`src/back-office/` in the Vennusign repo) that already consumes `sky-ui-tokens.css`; build there, against those tokens and whatever component primitives already exist.

The HTML uses inline styles throughout. That is an artifact of the design tool, not a recommendation.

---

## Fidelity

**Mixed — read this carefully, it changes what you should infer.**

**High fidelity (3 screens).** Final colors, type, spacing, radii, shadows. Recreate these closely.
- `M1 Hi-Fi v2 - Menus home.dc.html` — Menus home, populated **and** empty states
- `M2 Hi-Fi - Menu builder.dc.html` — the builder
- `M2c Hi-Fi - Play.dc.html` — Play

**Low fidelity (everything else).** `Menus.dc.html` and `Multi-Venue Menus.dc.html` are wireframes: grey boxes, system fonts, hard black borders, handwritten blue annotations. **Do not reproduce their styling.** Take structure, content, states, and behavior from them; take all styling from the three hi-fi screens and the component sheet below.

The wireframes also carry blue Caveat-font annotations and `M1`/`MV2` badges. Those are documentation, not UI. Never render them.

---

## Design tokens

### The existing contract

`sky-ui-tokens.css` is included in this bundle and is already in the repo at `src/back-office/src/sky-ui-tokens.css`. Use it. Two rules that were violated repeatedly during design review and are worth internalising:

- `--sky-color-text-muted: #64748b` is the muted text color **on light surfaces**.
- `--sky-color-text-on-dark-muted: #94a3b8` is **only** for text on the dark ink surfaces (the nav rail, the Play chrome). It fails WCAG AA on white — measured 2.56:1.

Every accessibility defect found in review was this one substitution. Treat `#94a3b8` on anything light as a bug.

Status colors always pair with a label or icon, never color alone.

### Proposed additions — NOT YET APPROVED

`proposed-token-additions.css` is in this bundle. **It has not been approved by the token owner.** The three hi-fi screens use values the current contract does not define, because a dense back office needs finer steps than the existing scale provides (it jumps 12 → 14 → 16 → 20 → 24).

Do not merge that file without the owner's sign-off. Options, in order of preference:
1. Owner approves the additions, they land in `sky-ui-tokens.css`, you consume them as variables.
2. Owner rejects, you snap to the nearest existing step and accept a blunter page.

Either way the tokens stay the single source of truth. Do not hardcode the raw values into components.

---

## Component sheet

Extracted from the three hi-fi screens. These are the entire vocabulary of the Menu area — if you find yourself inventing a fourth button style, something is wrong.

### Buttons

| variant | fill | text | border | radius | padding | size/weight |
|---|---|---|---|---|---|---|
| Primary (dark) | `#0f172a` | `#f8fafc` | none | 12px | 10–11px / 17–20px | 13–13.5px / 600 |
| Accent (sky) | `#87ceeb` | `#0f172a` | none | 10px | 8px / 15px | 13px / 600 |
| Secondary | `#fff` | `#475569` | 1px `#e2e8f0` | 12px | 10px / 16px | 13px / 600 |
| Inline link | none | `#0f172a` | none | — | — | 12.5px / 500, underline, `text-underline-offset: 3px` |

Primary carries `box-shadow: 0 4px 12px rgb(15 23 42 / .2)` when it is the page's main action (the Publish button); no shadow otherwise.

### Segmented control
Track `#f1f5f9`, 3px padding, 10px radius. Selected segment: `#fff`, 8px radius, `0 1px 2px rgb(15 23 42 / .1)`, 12px/600. Unselected: transparent, `#64748b`, 12px/500. Used for Quick update / Build and the icon-button pairs (Undo/Redo).

### Pill toggle (View: One section / Whole board)
Selected: `#0f172a` fill, `#f8fafc` text, 999px radius, 5px/13px padding, 12px/600.
Unselected: `#fff` fill, `#475569` text, 1px `#e2e8f0` border, 12px/500.

### Availability switch (the 86 control)
42×24px, 999px radius, `#178a52` when on, `0 1px 2px rgb(23 138 82 / .4)`, 18px white knob inset 3px. Lives inside a panel: `#e0f4e9` background, `inset 0 0 0 1.5px #178a52`, 12px radius. Title 13px/700 `#18603f`; body 11.5px/1.45 `#267252`.

This control is visually the loudest thing in the inspector on purpose — decision 3.

### Text field
1px `#dbe3ec` border, 10px radius, `#fff`, 9px/11px padding, 13px/1.45. Label above: 10.5px/600, `letter-spacing .07em`, uppercase, `#64748b`, 6px gap.
Inline warning below: 6px dot `#c9871a` + 11.5px/500 `#7d5911`.

### Checkbox
18×18px, 1.5px `#cbd5e1` border, 5px radius. Label 13px `#334155`, 10px gap.

### Menu card (Menus home)
Board render at `aspect-ratio: 16/9`, 14px radius, `box-shadow: 0 1px 2px rgb(15 23 42 / .09), 0 10px 24px rgb(15 23 42 / .10)`. **No border and no card background** — the board is the card.

Below the board, 12px gap, then a row: name 15.5px/600 `letter-spacing -.018em` on the left with a status line under it; section/item counts 11.5px `#64748b` right-aligned.

Status line: 6px dot + 12.5px/500 text.
- On screens, all good → dot `#178a52`, text `#18603f`
- On screens, one offline → dot `#c9871a`, text `#7d5911`
- Not on a screen → dot `#cbd5e1`, text `#64748b`

**⋯ button**, absolutely positioned top-right 12px, 28×28px, 9px radius, `backdrop-filter: blur(6px)`:
- on a **dark** board → `rgba(248,250,252,.16)` fill, `rgba(248,250,252,.24)` border, `#f8fafc` glyph
- on a **light** board → `rgba(15,23,42,.42)` fill, `rgba(15,23,42,.12)` border, `#f8fafc` glyph

Branch on board lightness. A light-glass chip on a cream board is invisible — this was a real defect twice.

**Pending-changes bar.** When a menu has unpublished changes, the board renders **shorter** (`aspect-ratio: 16/7.75`) and an amber strip occupies the reserved space at the bottom of the same rounded container: `#c9871a` fill, `#0f172a` text, 7px/14px padding, 11.5px/700, "3 changes not published" left, "Review →" right. The bar never overlays board content — decision from review.

### Add-a-menu tile
Same grid cell, `aspect-ratio: 16/9`, 1.5px dashed `#cbd9e4`, 14px radius, `#fbfdfe`. Centered: 36px sky rounded square with `+`, then 13.5px/600 "Add a menu", then 11px/1.5 `#64748b` "Photo, paste, spreadsheet / or start blank". Always the last cell.

### Nav rail
76px wide, `#0f172a`. Logo 34px sky rounded square. Items 56px wide, icon 15px over a 9px/500 label, 12px radius, 5px gap. Active: `#87ceeb` fill, `#0f172a` text, label 600. Inactive: `#94a3b8`. A `border-top: 1px solid #1e293b` divider near the bottom, then Settings, then a 32px avatar.

### Publish bar
`border-top: 1px solid #e9eef4`, `#fff`, 13px/22px padding. Left: 14px/700 `#7d5911` count + 11.5px `#64748b` meta line with underlined inline links. Center: per-screen chips. Right: "Review first" inline link + primary Publish button.

Screen chip: 1px border, 10px radius, 8px/11px padding. Online → `#dbe7de` border, `#fff`, `#178a52` dot. Offline → `#c9871a` border, `#fdf1dc`, `#c9871a` dot, `#7d5911` sub-text. Name 12.5px/600, sub 11.5px.

### Board rendering (the simulated TV)
This is menu content, not chrome. Typography is deliberately different from the UI.
- Family: `'Playfair Display', Georgia, serif`
- Venue/board title: 15px/600, `letter-spacing .3em`, uppercase, `#1a2b4a`, 1px `#ccd6e2` bottom rule
- Section heading: 11.5px/700, `letter-spacing .22em`, uppercase, `#8a6a2a`
- Item name: 15–19px/600, `letter-spacing .055em`, uppercase, `#1a2b4a`
- Price: same size, 500 weight
- Description: italic 13.5px/1.45, `#475569`
- Surface: `#faf8f2` (Coastal theme). Dark themes use `linear-gradient(#14202c, #0b1219)` with `#e2e8f0` items and `#87ceeb` section headings.

**Product annotations overlaid on a board** ("BERRY FIZZ IS 86'D — NOT RENDERED", "PAGE 3 OF 5", "1 OF 2") are **not board content**. 11.5px, `#64748b` or `#475569`, never the board's muted ink.

---

## Screens

Eight screens in the single-venue set (`Menus.dc.html`), eight in the group set (`Multi-Venue Menus.dc.html`).

### M1 — Menus home
**Hi-fi:** `M1 Hi-Fi v2 - Menus home.dc.html` (both states).
**Purpose:** see every menu, know whether the screens are current, open one, or add one.

Layout: 76px rail + content. Content is 30px/40px padded. Header block: venue name 11px/600 uppercase `#94a3b8` (not clickable — switching lives under the avatar), then the status headline at 29px/600 `letter-spacing -.028em` max-width 800px, then a 13px `#64748b` sub-line. Right: "Check the screens" secondary + "Add a menu" primary.

Grid: `repeat(4, 1fr)`, 26px gap, `align-items: start`. Always 4-up. Menus in order, then the Add-a-menu tile as the last cell.

Below, a "Not in use" strip: 11px/600 uppercase label, then pill chips (999px radius, `#f4f7fa`, 34×21px thumb + name + date).

**The headline is decision 12 doing real work.** It is a sentence naming what is current and what is not — never a green all-clear, never a status table. One screen → a sentence. Many screens → "18 current · 2 offline". It scales by summarising the normal and naming the exception.

**Empty state (zero menus):** the grid is replaced entirely. Centered, max-width 860px: "Let's get your menu in." at 34px/600, sub-line, then three route cards in a row (Photo highlighted with 2px `#87ceeb` and `#f2fbff`), then "or start from a blank board" as an underlined link. **Onboarding is the empty state of this screen, not a wizard** — decision 17. There is nothing to fall out of and nothing to re-enter.

**POS variant:** when the POS add-on is attached, a fourth route appears and leads. When it is not, there is no trace of it — decision 4.

### M1a — Import
**Lo-fi.** Four routes, one ending.

- **Photo:** drop zone → reading state that names what it found as it finds it ("7 sections found", "41 items", progress on descriptions) → confirm. Camera is a peer of upload, not hidden behind it.
- **Paste:** a textarea that parses live and reports "Looks like 2 sections and 6 items" before you commit. A caps line becomes a section; no syntax to learn.
- **Spreadsheet:** we publish the headings we read — **Item** and **Price** required, Description / Section / Sold out / Size optional. Any order, any case, extra columns allowed. A template is offered, never required. Matched columns are shown as facts; unmatched columns are the only question. **There is no mapping step** — decision 31.
- **POS:** pick categories, not items. States up front that the POS becomes the price source and that a hand-typed price wins until cleared.

**All four converge on one confirm step** (decision 30):
- Headline names the ratio: "We read 41 of 45 items. Four need you, plus a name check."
- One card per unclear item, each with a **crop of the original beside it** so the operator answers without hunting for the paper menu.
- Question shapes: fill a price, confirm a spelling, one-item-with-sizes vs two items, no-price (show "MP" / type one / leave it out).
- **Near-misses are one grouped question, never N questions** — a list with a row per item, each pre-ticked as *the same item*, plus "Untick all". A reprint with 30 tweaked names must not produce 30 questions (decision 33).
- The 41 correct rows are not shown, not counted down, not paged through. "See all 45" exists and is not the default (decision 18).
- Actions: "Done — open in the builder" primary, "Skip these for now" inline. Skipping flags them on the canvas.

**Importing into an existing menu replaces it** — decision 32. Not a merge. The board keeps its layout and theme; anything 86'd stays 86'd.

### M1b — The named actions
**Lo-fi.** The card ⋯ menu and the dialogs behind it.

Menu items, in order: **Open** · **Quick update** · — · **Go back to…** · **Duplicate** · — · **Take off the screens** (danger, `#8a2929`).

Nobody ever sees the words *unpublish*, *supersede*, *restore* or *archive* — decisions 9, 10, 11. This is **verbatim copy**; do not paraphrase.

**Take off the screens** is never a bare action. The dialog states what replaces it, with a picture of the fallback, before you confirm: "It stays on your Menus home and keeps its history. You can put it back at any time." Then "What people will see instead" showing the venue fallback and which screens are affected.

**The fallback** is a generated logo-and-name card, one per venue, used for **every** empty moment — taken off the screens, a scheduled gap, anything (decisions 14, 36). Authoring a replacement is out of scope; the UI says so plainly rather than offering a dead link.

### M2 — Menu builder
**Hi-fi:** `M2 Hi-Fi - Menu builder.dc.html`.

Four columns inside the rail: section rail 212px, canvas (flex), inspector 296px; publish bar across the bottom. **Set `box-sizing: border-box`** — the fixed widths are outer widths.

Top bar 58px: breadcrumb ("Menus / Summer Menu", "Menus" is the way back), Quick update / Build segmented control, Undo/Redo pair, "Viewing as **Bar** · 1920×1080 ▾", "▶ Play" accent button.

**Section rail** `#fbfcfe`: uppercase "Sections" heading with a `+`, then rows — drag handle `⠿` (`#64748b`, must be legible; it is the reorder affordance), name, item count. Selected row: `#e0f2fe` with `inset 0 0 0 1px #87ceeb`. It is a **navigator, not a second editor** — items live only on the canvas.

**Canvas** `#f4f7fa`: View toggle (One section / Whole board) then the board card at 14px radius on `#faf8f2` with a strong drop shadow. The board is the real render. Selected item gets `0 0 0 2px #2a78d6` plus a blue drag pill hanging off its left edge. An 86'd item renders at 42% opacity, name struck through, with "86'd 6:40pm — hidden on all screens right now" in `#8a2929`.

**There is no Preview button** — the canvas *is* the preview (decision from M2c). Adding one would imply the thing you are editing is not real.

**Inspector**: item name + ✕, then the availability panel, then Name / Description / Price, then two checkboxes (Feature on the board, Add a photo), then a footer line pointing at the theme. **Six controls total.** No Content/Style tabs, no Advanced, no per-item currency. The only warning shown is one that matters: "Two words over — wraps to 3 lines on Patio".

**Publish bar**: "3 changes not on your screens" (14px/700 `#7d5911`), "Draft saved 10:42am by Alex · go back to… · discard draft", the three screen chips, "Review first", "Publish 3 changes".

### M2a — Adding items
**Lo-fi.** One affordance for "new" and "existing" — you type, and it offers both library matches and "Create '<typed>' as a new item". Shows where an existing item already appears ("Also on Late Night"). A bulk path exists for adding many at once ("Place 2 in Non-Alcoholic").

All three routes end in the same place: a draft change in the publish bar.

**Note:** M2a references an item library that is **out of scope for this feature** and is being designed separately. Build against whatever the library API turns out to be; do not invent a library UI here.

### M2b — Board view
**Lo-fi.** The zoom level between editing an item and watching the cycle: where each section actually sits on the board, how it paginates, per-screen differences, and per-screen overrides ("Override it for Patio →"). Pages are a **consequence of content, not a setting**.

### M2c — Play
**Hi-fi:** `M2c Hi-Fi - Play.dc.html`.

Full-bleed dark (`#0b1220`). Top bar: "Playing · Summer Menu" with a live dot, an amber "Draft — includes your 3 unpublished changes" pill, screen picker chips (active = sky fill; offline = dimmed), ✕.

Body: the board at real proportions on the left, a 268px sidebar on the right holding (a) any problem the run surfaced — "Wine splits across 2 pages / 12 items don't fit the right column. A guest sees six, waits 8 seconds, then sees six more." with "Fix it in Board view →", and (b) the **Readable from** panel: a large number (32px/600) in feet, then a plain comparison to the room, then the provenance line — "Measured from what this screen reported: 55″ at 1920×1080, last seen 2m ago."

Transport bar: prev / pause / next, then a proportional timeline of pages where each block's width is its dwell time, the current page in sky, and a **dashed sky block for a page that only exists because something overflowed**. Right: "8s each · 40s a full cycle".

**Play is read-only by design.** The moment you can edit inside it, it becomes a second editor with a different layout.

**Play uses real screen geometry reported by devices — never a generic aspect ratio** (decision 13). An unpaired screen cannot be previewed against at all. The picker is "show me this board as Bar / Patio / Lobby", not "pick an aspect ratio"; an operator should never need to know their own screen's resolution.

### M3 — Quick Update
**Lo-fi.** The other half of Menus, and deliberately **not** a view of the builder (decision 15).

No canvas, no sections, no inspector. A search field, filter chips (Everything / Off right now · 2), and a flat list of items grouped by section, each with exactly one control: an availability toggle. Toggling shows a toast — "Berry Fizz is off on all 3 screens." + Undo.

**A toggle here is live** — it never joins the draft queue, never waits for a publish (decision 3). This is the 11pm-on-a-Friday path and must be reachable in one tap from the card ⋯ menu.

---

## Multi-venue (tier-gated)

`Multi-Venue Menus.dc.html`, screens MV1–MV4b. **All lo-fi.**

This is **not a second design**. It is the same screens with one extra dimension. A single-venue account gets the identical components with the venue parts absent — not disabled, absent (decisions 21, 29). Below the tier there is no Venues nav, no trust levels, no venue chip, and no upgrade UI beyond a single "Talk to us" prompt at the moment someone tries to add a second venue. It is not self-serve.

- **MV1** — the same shelf, with venue status folded onto each card. Filter row gains "Waiting on a venue", "Has local changes", "Local menus", "What venues can change".
- **MV1a** — Add a menu, same dialog plus one question: **Who owns it** (a group menu vs one venue's own). Marked "Group tier only". Everything else identical.
- **MV2** — one group menu's rollout across venues: who's running it, who's behind and by how long, who has local changes, and who isn't running it at all (with "Add them").
- **MV2a** — Send an update: pick venues, see conflicts *before* they happen ("your Old Fashioned price would replace theirs"), and note which venues will run it for the first time. **This screen both updates and assigns** — sending is how a menu reaches a venue.
- **MV3** — the venue's side: a pending menu on day 3, with locked fields shown greyed **with the reason**, never removed. An operator who can't see the price can't tell a guest why the board is wrong.
- **MV4 / MV4a / MV4b** — trust levels as a comparison table, creating a level, moving a venue between levels.

**Key rules:** corporate pushes and venues accept on their own schedule; nothing auto-applies; not accepting escalates badge → banner → daily email; an override is a fact on the row, never a fork; **86 is always venue-level** with no prompt and no scope question; trust levels are named, capped at 8, and always defined as a difference from an existing level. Decisions 20–29.

---

## Interactions & behavior

### Save model — this is the spine of the feature
**One save model per surface.** The old build mixed onBlur autosave, per-row Save buttons and dialogs on a single screen.

- **Availability (86) commits instantly**, always, everywhere. It never queues, never waits for a publish, and survives a publish. Toast with Undo.
- **Everything else is a draft**, queued **per menu**, and ships together on an explicit Publish. Price, copy, structure, layout — all of it.
- Nothing reaches a screen without a deliberate act.

### Undo
`⌘Z` / `Ctrl+Z`. Session-scoped, quietly capped. **Never named in a settings page or plan comparison** — it is a keystroke, not a feature (decision 7). History is a separate, durable, attributable capability; the tiered part is retention depth.

### Drag to reorder
Everywhere. Sections, items, taps, categories, meal periods, playlist slides. Every ↑/↓ button pair in the existing codebase becomes a drag handle.

### Navigation
- Card body → builder (M2), opening where you left off. The board **is** the door; there is no Open button on the card.
- Card ⋯ → the actions menu (M1b).
- Breadcrumb "Menus" → M1, from M2, M2b and M3.
- "▶ Play" → M2c. ✕ returns to the builder.
- "Fix it in Board view →" (M2c) → M2b.
- Quick update / Build segmented control switches between M3 and M2 for the **open menu**. It does **not** appear on the shelf — there is no menu selected yet, so it has nothing to switch.

### Loading and blocked states
Real states must say exactly what they are (decision 5): permission, disconnection, limits, offline targets. An offline screen says "offline — updates when it reconnects", not "error".

Capability outside your plan is **absent** — no ghost fields, no disabled controls, no explanatory tooltips (decision 4).

### Responsive
The hi-fis are 1440px desktop. Mobile has not been designed and is explicitly out of scope. Quick Update (M3) is the screen most likely to be used on a phone and should be treated as the first mobile candidate — flag it, don't guess it.

---

## Data model

Not previously written down; implied by the rules. Confirm with the owner before building.

**Venue** — id, name, logo, fallback card (generated), trust level (group accounts only), default user (**required**; the address escalations go to — decision 27).

**Screen** — id, venue, name ("Bar"), reported geometry (width, height, physical size), last-seen timestamp, online/offline, optional overscan correction. **Geometry is device-reported, never user-entered.**

**Menu** — id, name, owner (venue | group), theme, sections[], published version, draft queue, history. A group menu is **one object**; each venue holds a *state* on it, never a copy (decision 20).

**VenueMenuState** (group only) — venue, menu, status (running | pending | behind | not-running), accepted version, days behind, local overrides[].

**Section** — id, name, order, items[].

**Item** — id, name, description, price(s), sizes[], featured, photo, source (manual | pos), library ref.

**Placement** — item on a section of a board, with order. Items live in a library and get *placed*; the same item can appear on several boards.

**Availability (86)** — item × venue, boolean, timestamp, who. **Never part of a draft.** Scoped to the venue always (decision 25).

**DraftChange** — menu, field, before, after, author, timestamp. Grouped per menu; published as a set.

**PublishEvent** — menu, version, author, timestamp, target screens or venues, per-target delivery state.

---

## Acceptance criteria

Checkable restatements of the rules. These are the things most likely to be got wrong.

1. Toggling availability updates every screen showing that item within seconds, **without** a publish, and does not add anything to the draft queue.
2. Publishing a menu ships **all** queued changes for that menu and no changes belonging to another menu.
3. An item 86'd before a publish is still 86'd after it.
4. No screen content changes without a deliberate publish or accept.
5. The strings "unpublish", "supersede", "restore" and "archive" appear nowhere in the UI.
6. "Take off the screens" always shows what will replace the menu before confirming.
7. Undo is bound to the keystroke and appears in no settings page, plan comparison or marketing surface.
8. A capability outside the account's plan renders **nothing** — no disabled control, no tooltip, no placeholder.
9. Play renders against a screen's device-reported geometry; an unpaired screen cannot be selected in Play.
10. Import surfaces only rows the parser was unsure about; correctly-read rows are not shown by default.
11. A spreadsheet with the published headings in any order, any case, plus unknown extra columns, imports successfully and asks only about the extra columns.
12. Importing into an existing menu replaces its content, preserves its layout and theme, and preserves active 86s.
13. Thirty near-miss name matches produce **one** grouped question, pre-ticked as *same item*.
14. (Group) A venue below Trusted cannot change a price on a group menu, and the locked field is **visible with its reason**, not hidden.
15. (Group) An 86 at one venue never affects another venue, and never prompts.
16. (Group) A pending group menu never applies on its own, at any age.
17. (Group) Demoting a venue's trust level does not revert anything currently on a screen.
18. (Group) A single-venue account renders the same components with zero venue affordances.

---

## Verbatim copy

Wording **is** the design in several places. Do not paraphrase, shorten, or "improve" these.

- Card menu: **Open** / **Quick update** / **Go back to…** / **Duplicate** / **Take off the screens**
- "It stays on your Menus home and keeps its history. You can put it back at any time."
- "Turning this off hides it on all 3 screens immediately — not part of your draft."
- "86'd 6:40pm — hidden on all screens right now"
- "3 changes not on your screens"
- "offline — updates when it reconnects"
- "Draft — includes your 3 unpublished changes"
- "Measured from what this screen reported: 55″ at 1920×1080, last seen 2m ago."
- "Let's get your menu in." / "Pick whatever's easiest. You can fix anything later."
- (Group) "Behind 9 days", "running · 2 local changes", "set by head office"

Numbers in these strings are illustrative and should come from data. The sentence shapes should not change.

---

## Out of scope

Do not build, and do not invent UI for:

- **The item library** — its own feature, designed separately. M2a references it.
- **Replacing the fallback card** — the generated logo-and-name card is the whole of it for now.
- **Price requests** — MV3's "Ask for a price change" implies a corporate inbox that does not exist. Parked.
- **Users, permissions and roles across a group** — area managers, who can send an update, who can move a trust level. Parked; MV1's Group → People is a placeholder.
- **Source-controlled POS pricing** — freshness, last-known-good, override semantics. Deferred (decision 16).
- **Mobile layouts.**
- **Other nav areas** — Screens, Schedules, Themes, Tap list, Billing. Menus references them; they are not designed.
- **Time and scheduling.** A menu has no hours of its own; Schedules points at menus (decision 35). Menus owns only what shows when nothing is pointed at a screen.

---

## Assets

No image assets. Everything in the designs is CSS.

- **Fonts:** Inter (UI, per the token contract) and Playfair Display (board rendering only, loaded from Google Fonts in the hi-fi files). Confirm Playfair against the brand — it was chosen to make the simulated boards read as restaurant menus rather than as UI, and it appears nowhere in the chrome.
- **Icons:** the hi-fis use Unicode glyphs (⌂ ☰ ◷ ⛁ ▢ ◐ ⇄ ⚙ ⋯ ⠿ ✕ ▶ ❚❚ ◀). **Replace all of them** with the codebase's icon set. They are placeholders.
- **Board thumbnails** are live DOM renders of menu content, not images. In production they should render from real menu data at 16:9 — that is the point of them.

---

## Files

**Read first**
- `decisions.md` — 36 numbered decisions plus the parked list. Authoritative.

**Hi-fi (recreate closely)**
- `M1 Hi-Fi v2 - Menus home.dc.html` — M1 populated + empty
- `M2 Hi-Fi - Menu builder.dc.html` — M2
- `M2c Hi-Fi - Play.dc.html` — M2c

**Lo-fi wireframes (structure, states and behavior — never styling)**
- `Menus.dc.html` — M1, M1a, M1b, M2, M2a, M2b, M2c, M3, annotated
- `Multi-Venue Menus.dc.html` — MV1–MV4b, annotated
- `Menus at Scale.dc.html` — the same surfaces at 20 screens and 13 menus. Not a separate design; it is the scaling check. Every screen must summarise the normal and name the exception rather than growing a longer list.
- `Menus Home - Compare.dc.html` — the two candidate M1 layouts side by side, with the trade-offs written out. The shelf won. Included because the reasoning constrains what M1 may become later.

**Superseded, included for context only**
- `M1 Hi-Fi v1 - Menus home.dc.html` — the first hi-fi pass: tinted gradient page, bordered cards, 4-up small thumbnails. **Rejected.** Kept so the direction is not accidentally rebuilt. Build `M1 Hi-Fi v2`.

**Tokens**
- `sky-ui-tokens.css` — the existing contract, already in the repo
- `proposed-token-additions.css` — **not approved**; see Design tokens above

**Screenshots**
- `screenshots/M1-menus-home.png` — M1 hi-fi, populated + empty
- `screenshots/M2-menu-builder.png` — M2 hi-fi
- `screenshots/M2c-play.png` — M2c hi-fi
- `screenshots/wireframes-menus.png` — the full single-venue wireframe sheet
- `screenshots/wireframes-multi-venue.png` — the full group wireframe sheet

Screenshots are for orientation. The `.dc.html` files are the reference — open those for real measurements.

**Also included**
- `README.html` — this document, formatted for reading and printing
- `github.md` — the source repo this project tracks

**Runtime dependencies — do not delete**
- `support.js` — the wireframe files will not render without it
- `BackOfficeNav.dc.html` — the shared nav the wireframes pull in. Not a deliverable in itself; the nav's production spec is the rail described under Components.

Open any `.dc.html` in a browser to view it.
