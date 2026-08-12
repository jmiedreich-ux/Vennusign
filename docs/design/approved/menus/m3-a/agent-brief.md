# Menu Builder — agent brief

For the coding agent (Claude Code or Codex) implementing the Menu Builder and its
connected screens in `src/back-office/`.

**Build target: every capability ON.** Ship the maximum-tier screen. Each gated
control is wrapped in its own capability check so it can be switched off later
without touching layout — the mechanism exists from day one, nothing uses it yet.

## 1 · Source of truth, in order

1. `docs/design/approved/menus/menu-builder-v2/workflow-handoff.md` — routes and behaviour.
2. `menus/Menu Builder - action inventory.md` — **every action, every branch, every end state** (51 actions / 93 paths for the builder, plus rounds 3 and 4 for the connected screens). This is the acceptance criteria list.
3. `menus/Menu Builder Preview.dc.html` — the builder screen, approved, two frames (menu closed and open).
4. `menus/Menu Builder - connected screens.dc.html` — the seven connected screens plus the failure, after, empty, capability, replace-mode and volume states.
5. `docs/design/approved/menus/decisions.md` — everything the above does not override.

Where a screenshot and the inventory disagree, the inventory wins. Eleven
amendments to the register are tabled at the foot of the inventory and are **not
yet applied to `decisions.md`** — read them there, not in the register.

## 2 · Screens and where they are drawn

| Screen | File section |
|---|---|
| Builder (page rail, sections, board, inspector, footer) | Preview, frame 1 (resting) and 2 (Finish menu open) |
| Import landing — four routes | Connected, **A** |
| Add or replace, into an existing menu | Connected, **A2** |
| Spreadsheet upload + reading state | Connected, **A3** |
| Reading states for photo and paste | Connected, **A4** |
| Import review — photo/paste, and spreadsheet | Connected, **B** and **B2** (one component, question set by source) |
| Screen Assignments | Connected, **C** |
| Publishing review | Connected, **D** |
| Menu history | Connected, **E** |
| Failure, after-publish, empty states | Connected, **F** |
| Capability-off, replace mode, history at volume | Connected, **G** |

## 3 · Tokens — no invented values

Everything in the mocks is a literal. Map it, do not copy it. All from
`src/back-office/src/sky-ui-tokens.css`.

| Mock literal | Token |
|---|---|
| `#0f172a` ink, sidebar, toggle track | `--sky-color-ink` / `--sky-sidebar-background` |
| `#87ceeb` sky fill, active tab cap, focus | `--sky-color-primary` |
| `#e0f2fe` selected chip, icon tiles | `--sky-color-secondary` |
| `#e2e8f0` / `#e9eef4` borders, dividers | `--sky-color-border` |
| `#f4f8fc` / `#f8fafc` workspace, page ground | `--sky-color-background-base` / `--sky-color-surface` |
| `#475569` secondary text | `--sky-color-text-secondary` |
| `#64748b` muted text | `--sky-color-text-muted` |
| `#178a52` / `#e0f4e9` / `#18603f` live | `--sky-color-live` / `-live-surface` / `--sky-positive-text` |
| `#b03a33` / `#fbe7e5` / `#8a2929` off, 86 | `--sky-color-off` / `-off-surface` / `--sky-danger-text` |
| `#c9871a` / `#fdf1dc` / `#7d5911` warning | `--sky-color-warning` / `-warning-surface` / `--sky-warning-text` |
| `#991b1b` destructive button | `--sky-danger-solid` |
| radii 8/10/12/999 | `--sky-radius-sm` … `--sky-radius-pill` |

Two exceptions, both recorded, neither to be spread further: `#1d6fb8` for the
primary button (the owner's `#2a90e0` fails 4.5:1 with white; this passes at
5.2:1), and `#d4552a` for the guest-facing SOLD OUT chip, whose contrast becomes
the **theme's** responsibility per V2 correction #8.

**Final visual authority.** The nine exported screens under
`exports/screens/` are the approved visual truth for all of M3-A. They
supersede the earlier in-progress HTML design files for layout, hierarchy,
placement and visual treatment. The action inventory remains authoritative for
behaviour and branch outcomes. Where an exported screen and a superseded HTML
file disagree visually, the exported screen wins.

**Type.** Use the existing application typeface throughout, from
`--sky-font-family`, including every page tab and its inline naming field. Page
tabs may retain their approved uppercase and letterspacing treatment, but they
must not use a special display face or page-only font token.

## 4 · Icons

- **Nav rail (76px):** lucide-react exports named in `navigation.mjs` — House, UtensilsCrossed, Clock, Beer, Monitor, Palette, ArrowLeftRight, CreditCard, ShieldCheck. 15px, stroke 2, label beneath, accessible name from the route label (Q185). The unicode glyphs in the mocks are stand-ins.
- **Everything else:** `SkyIcon.tsx`, eight names only — check, close, screen, search, refresh, key, moon, sun.
- **Six glyphs this screen needs and SkyIcon does not have:** drag handle, rename pencil, remove, chevron, warning, screen mark. **Open decision:** extend `SkyIcon.tsx` with six named paths, or admit lucide into non-rail use. Do not silently do both.

## 5 · Test id contract

Follow the conventions already in `tests/ui/specs` — kebab-case, semantic, no
component prefix, with state on `data-*` attributes rather than in the id
(`menu-card` + `data-menu-id`, `shelf-grid` + `data-at-scale`). Add these:

`menu-builder` · `page-tab` (+`data-page-id`, `data-active`) · `add-page` ·
`page-actions` · `page-menu` · `section-row` (+`data-section-id`, `data-selected`) ·
`add-section` · `viewing-chip` (+`data-scope`) · `assignment-pill` ·
`capacity-banner` (+`data-capacity="fits|nearly-full|overflowing"`) · `check-fit` ·
`board` (+`data-board-surface="preview"`) · `board-section` · `board-item`
(+`data-item-id`, `data-sold-out`) · `add-item-row` · `item-inspector` ·
`item-name` · `item-price` · `available-toggle` · `eightysix-toggle`
(+`data-inert`) · `eightysix-state` · `remove-from-page` · `page-history` ·
`menu-history-link` · `draft-state` · `theme-select` · `finish-menu` ·
`review-publish` · `save-exit` · `discard` · `discard-dialog` · `restore` ·
`publish-confirmation` (+`data-screens`) · `import-landing` · `import-route`
(+`data-route="photo|paste|spreadsheet|blank"`) · `add-or-replace` ·
`upload-dropzone` · `reading-card` (+`data-stage`) · `import-review` ·
`review-question` (+`data-question`) · `section-picker` · `page-name` ·
`import-error` (+`data-cause`) · `screen-assignments` · `screen-row`
(+`data-screen-id`, `data-state="online|offline|unpaired"`) · `publish-review` ·
`overflow-ack` · `unnamed-item` · `history-entry` (+`data-kind`, `data-grouped`) ·
`empty-state` (+`data-context`).

Existing ids stay as they are: `nav-item`, `menus-home`, `menu-card`,
`card-actions`, `venue-fallback`.

## 6 · Slices, in dependency order

Each is independently mergeable and leaves `master` releasable, with its
Playwright specs in the same PR (`AGENTS.md`: schema → API → UI → specs together).

1. **Page rail and page header** — tabs, add/rename/duplicate/delete page, viewing chips with `More ▾` past five, assignment pill, capacity banner. ~15 branch paths.
2. **Sections and history inside the panel** — the sections column, inline rename, reorder, delete-with-move, page-scoped history. ~14 paths.
3. **Board and add-item** — selection, cross-section drag, remove-with-confirm, the inline add row, library near-match. Nothing else is editable on the board. ~14 paths.
4. **Inspector, availability and the 86** — fields, the two coupled toggles, the boxed 86 message, inert-until-published with its reason. ~13 paths.
5. **Footer, publish and history** — Finish menu, discard dialog, restore, publishing review with the overflow tick and unnamed list, confirmation that names screens and persists, menu history with grouping. ~20 paths.
6. **Import** — landing, add-or-replace, upload, reading states, review for both sources, the five failure messages. ~17 paths.

## 7 · Test conventions to reuse

- `openAs(page, role, route)` and `openMenuEditorAs` from `tests/ui/fixtures.ts`; roles are `owner`, `editor`, `publisher`, `scale`.
- Seed per spec via `POST /api/test/seed` / `scaleSeed` so specs run parallel.
- Menus is a desktop surface: skip non-desktop projects, as `menus-shelf.spec.ts` does.
- Assert **what a person sees**, not that an API accepted a request — that rule came out of the milestone 1 retrospective and is why these specs exist.
- Keep the banned-words assertion, **with "restore" removed from the list**: "unpublish", "supersede" and "archive" must still appear nowhere a user can read. Restore is now permitted vocabulary — the Finish menu says **Restore an earlier version** — so the existing assertion in `menus-shelf.spec.ts` needs its array amended in the same PR that ships the Finish menu, or it fails on a string that is now correct.

## 8 · Do not

- Restyle the board renderer or the sold-out presentation — the theme owns both.
- Add a colour, radius or spacing value that is not a token.
- Introduce or use a special page-tab font. Page tabs use the existing
  application typeface like the rest of the back-office UI.
- Reformat a price. Prices render exactly as typed; `MP` stays `MP`, `9.5` stays `9.5` (Q115/Q190).
- Import a *Sold out* column. It is recognised and deliberately dropped — an 86 is a person's statement about tonight.
- Put a rotation interval control anywhere in Menus. The theme owns it; Screen Assignments displays it read-only.
- Let an 86 queue. It is immediate — except its *cancellation* when Available is switched off, which rides with that edit's publish.
- Invent error copy. The five import failures are drawn in section F, each naming its fix.
- Ship a diff view. History shows the summary stored at publish time; nothing is recomputed.

## 9 · Values that come from config, not from the mock

| Value | Source |
|---|---|
| Import file size limit | Tiered setting, starting at 5 MB |
| When a failing publish stops being silent | Venue setting, default ~30 seconds |
| History retention depth | Per-tier config, read at runtime — never hardcode "30 days" |
| History grouping threshold | More than five events of a kind in a day |

## 10 · Open before or during the build

- The six missing glyphs (§4).
- Menu-level duplicate and delete have no home — parked with the Menus home revision.
- A venue replacing a group menu: out of scope this milestone (decision 34 stands).
- Mobile: out of scope. Quick Update is the first mobile candidate.
