# Shared components — reconciliation diff

**Status: awaiting owner ruling. Nothing has been built or changed.**

Purpose: before building a shared component library, establish what each shared component *should* look like —
without silently reverting decisions made after the designs were drawn. Three sources disagree in real ways, and
none of them is automatically right.

**The sources**

| | What it is | Where |
|---|---|---|
| **Design** | The owner-reviewed design project — hi-fi artboards, the original visual intent | Claude Design project `Vennusign screen mockups` (`213b7be7-…`), incl. its own `sky-ui-tokens.css` |
| **Spec** | A text summary extracted from those hi-fis into the repo | `docs/features/menus/README.md` → "Component sheet" |
| **Code** | What actually ships today | `src/back-office/src/*.css`, `styles.css`, `menu-builder.css` |
| **Recent** | Owner feedback from the 2026-08-24 live-testing round | This session; several items already shipped |

**The standing rule: no source is authoritative on its own.** Owner, 2026-08-25: *"we can't trust what's in there
because changes have happened after — it's three ways."* Each source moves independently and any one can be the
stale one. Four ways this has already bitten:

| Direction | Example |
|---|---|
| Code newer than design | The focus ring (row 1) — sky blue genuinely fails contrast, so the code's change was right |
| Design newer than code | Identity explorations that may never have reached the app |
| Both stale, superseded by a later decision | The Review mock's "unnamed item" section, which A11 made impossible |
| Reads like drift, actually settled | Entitlement components vs decision 4, resolved by build-decision 9 |

Practical consequence: **a design file is evidence, never a verdict.** A claim inside one — including "this is the
canonical reference" — carries the date it was written and nothing more. Verify against code and against decisions
made since, before treating it as governing anything.

**How to read a row.** Every conflict is tagged:
- 🟡 **documented reason** — code diverged deliberately, with a recorded rationale. Likely keep, but you should see it.
- 🔴 **undocumented drift** — code differs and nobody wrote down why. Most likely a real defect.
- ⚫ **never built** — spec'd, no implementation exists.

The owner asked to start by ruling on *everything* (option 1), then migrate over time toward auto-accepting
🟡 documented-reason conflicts and only surfacing 🔴 drift. The tags exist so later passes can filter instead of
re-deriving.

---

## 1. Focus ring

🟡 documented reason · ⚠️ three-way conflict

The single most consequential row: it affects every interactive control in every app.

| Source | Value |
|---|---|
| Design | `--sky-focus-color: var(--sky-color-primary)` (#87ceeb sky blue), `--sky-focus-width: 3px` |
| Code | `--sky-focus-color: #096f91` (dark teal), `--sky-focus-width: 2px`, plus a `--sky-focus-halo: none` the design has no concept of |
| Recent | Owner, on dialog fields: remove the ring entirely — "we need to get rid of this circle selecting problem everywhere". Shipped as `outline: none` + border-color change on `.builder__dialog` fields and `.builder__inspector` inputs. |

Code carries its own rationale in a comment: *"One dark-sky ring clears 3:1 on light surfaces without making
ordinary controls look double-bordered."* That is an accessibility contrast fix — sky blue at #87ceeb does not
clear 3:1 against white. **Reverting to the design value would reintroduce a real accessibility defect.**

The recent feedback is narrower than it sounds: it removed the *outline ring* on fields inside dialogs and the
inspector, replacing it with a border-color change. It did not remove focus indication from buttons, links, or
anything else.

**Needs a ruling:** is "quiet border-color change instead of a ring" the rule for *all* text inputs everywhere, or
only inside dialogs/inspector? Buttons and links still use the 2px ring today. A design system needs one answer.

---

## 2. Button variants

🔴 undocumented drift · naming collision

The spec is explicit: *"These are the entire vocabulary of the Menu area — if you find yourself inventing a fourth
button style, something is wrong."* Four variants were specified. The code has **four parallel vocabularies**, and
the same word means different things in each.

| Spec variant | Spec values | Code implementation | Match? |
|---|---|---|---|
| Primary (dark) | `#0f172a` fill, `#f8fafc` text, 12px radius, 13–13.5px/600, shadow `0 4px 12px rgb(15 23 42 / .2)` when the page's main action | `.builder__publish-button` — 12px radius, 13.5px/600, that exact shadow | ✅ exact |
| Accent (sky) | `#87ceeb` fill, `#0f172a` text, 10px radius, 8px/15px padding, 13px/600 | `.action-primary` — correct colors via tokens, but adds `min-height: 42px`, `padding: 9px 13px`, `font-weight: bold`, and `!important` on three properties | ⚠️ diverged |
| Secondary | `#fff` fill, `#475569` text, 1px `#e2e8f0` border, 12px radius | `.action-secondary` | ≈ close |
| Inline link | no fill, `#0f172a`, 12.5px/500, underline, offset 3px | `.builder__link`, `.restore-link`, `.builder__capacity-link`, others | ⚠️ scattered, no single definition |

**The collision:** the spec's "Primary" is dark navy; `.action-primary` in code is *sky blue* (the spec's "Accent").
So "primary button" means two different things depending on which file you're in. This is the mechanism behind the
`.secondary` typo found on 2026-08-24 — a Cancel button used a class name that exists in nobody's vocabulary and
rendered as an unstyled browser default, invisible because no component owns "what secondary means."

Additional undeclared vocabularies in live code: `import-primary`/`import-secondary` (menu import),
`customer-entry__provider--primary` (onboarding), `builder__quiet-danger` (menu builder).

**Needs a ruling:** what are the canonical variant names, and which spec variant does each map to?

---

## 3. Text field

✅ no conflict

| Source | Value |
|---|---|
| Spec | 1px `#dbe3ec`, 10px radius, `#fff`, 9px/11px padding, 13px/1.45. Label above: 10.5px/600, `.07em`, uppercase, `#64748b`, 6px gap |
| Code | `.builder__inspector input/textarea` — 1px `#dbe3ec`, `--sky-radius-sm-plus` (10px), `#fff`, 9px 11px, 13px, 1.45 |

Matches exactly. The one open question is the focus treatment (row 1).

Note: `styles.css` also has a global `:where(input, select, textarea)` rule using tokens rather than the literal
hex. Same result today; worth collapsing to one definition when components land.

---

## 4. Availability switch

🟡 documented reason · the 86 control

| Source | Value |
|---|---|
| Spec | 42×24px, 999px radius, **`#178a52` (green) when on**, `0 1px 2px rgb(23 138 82 / .4)`, 18px knob inset 3px. Panel: `#e0f4e9` bg, `inset 0 0 0 1.5px #178a52`, 12px radius |
| Code | 42×24, 999px, 3px padding — geometry exact. But **on-state is `--sky-text-primary` (#0f172a, near-black)**, shadow `rgb(15 23 42 / .3)` |

Deliberate: changed when the Available/86 split shipped (PR #831). The control's meaning changed — it is no longer
one green "on sale" switch; there are now two independent toggles (Available, drafted; and 86, immediate), and green
would have implied the old single-meaning semantics.

The spec's own note — *"This control is visually the loudest thing in the inspector on purpose — decision 3"* —
still holds for **86**, which is immediate and guest-visible. It arguably does not hold for **Available**, which is
an ordinary drafted change like name or price.

**Needs a ruling:** should the two toggles look different from each other — 86 loud (red/green), Available quiet
(neutral)? Today they are visually identical, which may under-sell how differently they behave.

---

## 5. Segmented control

⚫ never built

Spec: track `#f1f5f9`, 3px padding, 10px radius; selected segment `#fff`, 8px radius, `0 1px 2px rgb(15 23 42 / .1)`,
12px/600; unselected transparent `#64748b` 12px/500. Specified for *"Quick update / Build and the icon-button pairs
(Undo/Redo)."*

**No implementation exists** — no `.segmented`, `.segment`, or equivalent anywhere in the CSS. The Quick update
control that does exist (`MenusHome.tsx:560`) is a plain unstyled `<button>`. Undo/Redo are separate buttons.

**Needs a ruling:** build it as spec'd, or is this a design that got superseded and should be dropped from the
component set? (V2's builder navigation changed substantially after this spec was written.)

---

## 6. Checkbox

⚫ never built

Spec: 18×18px, 1.5px `#cbd5e1` border, 5px radius; label 13px `#334155`, 10px gap.

No implementation. The only trace is a token comment in `sky-ui-tokens.css:153`
(`--sky-color-border-control: #cbd5e1; /* checkbox border; … */`) — the value was reserved, the component never
built. Checkboxes in the app today are bare `<input type="checkbox">` with browser-default rendering.

Relevant: decision A14's fit-overflow acknowledgment (planned for the Review & Publish page, M7.1) needs a real
checkbox. This is a dependency, not just tidiness.

---

## 7. Pill toggle

⚠️ needs review

Spec: *"View: One section / Whole board"* — selected `#0f172a` fill + `#f8fafc` text, 999px, 5px/13px, 12px/600;
unselected `#fff` + `#475569`, 1px `#e2e8f0` border.

The "One section / Whole board" control this describes no longer exists in that form — V2 replaced it with the
page-tab + breadcrumb navigation (`.builder__page-tab`). What *is* called a pill in code
(`.builder__assignment-pill`) is a different thing entirely: the screen-assignment summary button, using
`--sky-radius-md`, not 999px.

**Needs a ruling:** is there still a pill-toggle component, or does this spec entry retire with the navigation it
described?

---

## 8. Undefined tokens

🔴 undocumented drift · live defects

Not a design conflict — straightforwardly broken. **45 usages of CSS custom properties that are never defined
anywhere.** 15 have no fallback, so the property is dropped and the element renders unstyled.

| Token | Uses | No fallback |
|---|---|---|
| `--sky-color-surface-muted` | 28 | 10 |
| `--sky-selection` | 6 | 0 |
| `--sky-color-warning-soft` | 3 | 3 |
| `--sky-action-shadow` | 2 | 2 |
| `--sky-color-danger-text` | 2 | 0 |
| `--sky-accent` | 2 | 0 |
| `--font-display`, `--font-sans` | 2 | 0 |

Several look like near-misses for tokens that *do* exist: `--sky-color-surface-muted` vs the real
`--sky-color-fill-subtle`/`--sky-color-surface-tint`; `--sky-color-warning-soft` vs the real
`--sky-color-warning-surface`; `--sky-selection` vs `--sky-color-selection`; `--sky-color-danger-text` vs
`--sky-danger-text`.

This is exactly the class of bug fixed on 2026-08-24 in the capacity banner (`--sky-color-warning-border` /
`--sky-color-warning-soft`, both undefined — the banner had been rendering with no background and a default dark
border, silently, for an unknown period).

**No ruling needed** — these are defects. The question is only whether a build-time check should make this class of
bug impossible going forward (recommended; nothing catches it today, and `docs/**` and CSS are outside the current
CI's reach).

---

## 9. Token source duplication

🔴 structural

`sky-ui-tokens.css` exists as **two hand-maintained copies plus two relative-path importers**:

| Consumer | How it gets tokens |
|---|---|
| back-office | owns `src/back-office/src/sky-ui-tokens.css` (145 definitions) |
| www | its **own copy** — header says *"Keep the two files in sync by hand"* |
| platform-operations | `@import "../../back-office/src/sky-ui-tokens.css"` |
| board-engine | references back-office's copy |
| Design project | its own copy, already **behind** — has the pre-accessibility-fix focus values (row 1) |

Values in the two app copies are currently identical; only header comments differ. The design project's copy has
already drifted. Owner has approved consolidating to one shared source.

---

## Summary — what needs a ruling

| # | Component | Tag | Question |
|---|---|---|---|
| 1 | Focus ring | 🟡 | Is "quiet border-color, no ring" the rule for all inputs, or only dialogs/inspector? |
| 2 | Button variants | 🔴 | Canonical names, and which spec variant each maps to? ("primary" currently means two different things) |
| 4 | Availability switch | 🟡 | Should Available and 86 look different from each other? |
| 5 | Segmented control | ⚫ | Build as spec'd, or retire it? |
| 6 | Checkbox | ⚫ | Build as spec'd (A14's fit acknowledgment needs one) |
| 7 | Pill toggle | ⚠️ | Still a component, or retired with the navigation it described? |

Rows 3 (text field), 8 (undefined tokens) and 9 (token duplication) need no ruling — 3 is already correct, 8 are
defects, 9 is already approved.

## What happens after the ruling

1. Consolidate to one token source; fix the undefined tokens.
2. Build the agreed components.
3. Ship `#/design` — a route in the real back-office rendering every component, every variant, every state, using
   the real shipped CSS. Fits the app's existing hash routing (`#/menu/{id}`, `#/menu/quick-update`). This becomes
   the thing you review against, and it cannot drift from what ships because it *is* what ships.
4. Migrate opportunistically: menu-builder surfaces first, others as they're touched.
