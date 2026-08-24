repo: Jmiedreich-UX/Vennusign
branch: master
path: src/back-office

## Last sync

date: 2026-08-06T23:58:00Z

### Updated in this project

- Added a **Decisions on record** panel (16 owner-settled rules) governing all Menus wireframes.
- Added **A5** — "Take off the screens", "Go back to…", and the one-screen publish bar.
- Removed the assumed-size / unpaired-screen preview from A3: preview is exact-target only.
- Publish strip now reports *readiness* before publishing and *delivery* after, keeping three states rather than the inventory's eleven.
- Read `uploads/product-surface-feature-inventory.md` (owner-supplied design reference, not repo content) and reconciled it against A1–A4.

## Menus decisions on record

1. Explicit publish everywhere — nothing reaches a screen without a deliberate act; per-field autosave-to-live is gone at every tier.
2. One draft queue per menu; all edits ship together on publish.
3. 86 / availability is always immediate — never queued.
4. Locked by plan = invisible. No ghost fields, no disabled controls, no reason at reach-time. Upgrade discovery solved separately.
5. "Blocked" (permission, disconnection, limits, offline target) is a real state and must name its actual reason. Only these belong in the state vocabulary.
6. Every capability independently switchable; a static-content screen is its own designed outcome.
7. Undo = keystroke, session-scoped, quietly capped, never named as a feature.
8. Change history is a separate capability; retention depth (version count) is the tiered thing.
9. Supersession never exposed — survives only as "replaced by" in a history entry.
10. "Unpublish" → **Take off the screens**, always showing what replaces it.
11. "Restore" → **Go back to…**, phrased as time, producing a draft.
12. Delivery evidence: "you can always tell whether your screens are current" — a sentence at one screen, a list at several.
13. Preview against real device-reported geometry only; no representative sizes.
14. Venue fallback is chosen manually; a generated logo-and-name card until then, as a real visible object.
15. Quick Update stays a separate fast path from the builder.
16. Source-controlled (POS-authoritative) fields deferred to an advanced add-on.

Open: whether A1 (builder) and S2 (quick update) share a route; Content vs Menu as the durable top-level label.

## Screen map

| Screen | Source files |
| --- | --- |
| A1 — Menu Builder, adopted | `MenuSectionsEditor.tsx`, `MenuItemsEditor.tsx`, `navigation.mjs`, `sky-ui-tokens.css`, uploads/menu editor.png |
| A2 — Adding items | `MenuItemsEditor.tsx` |
| A3 — Play | `src/display`, `src/tv` (render + geometry reporting) |
| A4 — Board view | `MenuSectionsEditor.tsx` |
| A5 — Named actions / one screen | new; no current source equivalent |
| S2 — Menu route landing | `MenuSectionsEditor.tsx`, `QuickUpdateMode.tsx` |

## Repo notes

- Solution: .NET API (`src/Vennu.Api`, `Vennu.Core.Models`, `Vennu.Data`, `Vennu.DataAccess`) plus four front ends.
- Front ends: `src/back-office` (React + TS + Vite, main admin), `src/platform-operations`, `src/display`, `src/tv`.
- Nav is data-driven from `src/back-office/src/navigation.mjs`; routes carry `capabilityId` + `upgradeFeature` for tier gating.
- Design system: "Sky UI" tokens in `src/back-office/src/sky-ui-tokens.css`. Locked palette — sky `#87ceeb` is a fill/focus accent only, never text on light.
- Planning docs live in `track0/`, `docs/work-packages/`, `docs/acceptance/`.

## Sync history

- 2026-08-06 — scoped wireframes to Menus; corrected sidebars against `navigation.mjs`; A1 marked draft approved.
- 2026-08-06 — initial association; read `PROJECT_STATUS.md`, `MenuItemsEditor.tsx`, copied Sky UI tokens.
