# Theme Studio owner-test handoff

**Stopped:** 2026-08-13  
**Owner status:** Stop for the day after the Appearance step successfully advanced to Variants.  
**Exact next action:** In Variants, select **Sold out**.

## Live mock

https://vennue-theme-studio-draft.jmiedreich.chatgpt.site

Published mock reference:

- Sites project: `appgprj_6a7d30b04df0819191071e15a087eeec`
- Site version: 13
- Source checkpoint: `35c33d1`
- Deployment status at handoff: succeeded

Continue the existing mock. Do not redesign it before finishing the remaining owner test.

## Owner-verified behavior

The owner personally confirmed:

1. **Create new → Menu → Begin blank** reaches the guided design surface.
2. The active guide is prominent in the center working area.
3. The guide uses a distinctive non-blue treatment and is easy to separate from the normal interface.
4. **Two-column menu** advances automatically to Components.
5. **Menu item repeater** visibly follows the pointer and can be dropped into a real left/right column.
6. Dropping the repeater creates an empty container, not the Northside sample menu.
7. Fields opens automatically after repeater placement.
8. **Item name** visibly follows the pointer and can be dropped into the repeater.
9. Item names appear only after the successful Item name drop.
10. Item name settings opens to Style.
11. Changing Weight from 700 to 600 succeeds and automatically opens Variants.

The owner's final comment on the repaired interaction was: **“dragging is working great.”**

## Automated verification completed on the mock

- `npm run lint` passed.
- `npm test` passed, including the production build and rendered HTML test.
- A live agent-preview pass completed both drag/drop transitions.
- The repeater remained empty before the field drop.
- The Item name drop populated sample names and advanced to Appearance.
- The updated Sites deployment reached `succeeded`.

These checks supplement the owner walkthrough; they do not replace the remaining owner test.

## Fixed defects from the walkthrough

| Defect | Resolution | Status |
|---|---|---|
| Guide was too subtle and blended into the editor | Centered prominent purple guide card | Owner verified |
| Guide treatment looked like ordinary blue UI | Purple guide and amber drag feedback | Owner verified |
| Repeater behaved like click-to-add | Guided placement now requires a left/right column drop | Owner verified |
| Repeater populated the complete sample menu | Placement now creates an empty container only | Owner verified |
| Item names appeared too early | Names appear only after Item name is dropped | Owner verified |
| Repeater did not visibly move during dragging | Added pointer-following amber drag preview | Owner verified |
| Item name had the same invisible drag problem | Applied the same visible drag behavior | Owner verified |

## Remaining owner test

Resume with the design already at **Variants** when possible. If a fresh browser session resets the mock, replay the verified steps quickly and stop at Variants.

### 1. Behavior

- Select **Sold out**.
- Expected:
  - confirmation appears;
  - Behavior becomes complete;
  - Test mode opens automatically;
  - the guide asks for the Long text dataset.

### 2. Test

- Open Dataset and select **Long text**.
- Expected:
  - the canvas renders the long-text case;
  - confirmation appears;
  - Test becomes complete;
  - the guide advances to Save;
  - Save theme becomes enabled.

### 3. Save

- Select **Save theme**.
- Expected:
  - the save/review surface opens;
  - language remains explicit that saving a theme does not change a live screen;
  - saving succeeds and returns to the appropriate Theme Studio or Menu Builder context;
  - no screen-publish claim is made.

### 4. Exit and Resume Guide

Run once before final acceptance:

- Exit Guide during an incomplete step.
- Confirm guide card and guide-only highlighting disappear.
- Confirm the canvas work remains.
- Resume Guide.
- Confirm it returns to the same incomplete step and reopens the correct panel.

### 5. Guidance disabled regression

Create another blank theme with guidance disabled.

- Confirm the centered guide, guide-only targeting, and forced transitions are absent.
- Confirm free-form component/field dragging behaves as it did before the guided-mode changes.
- Confirm guided restrictions do not leak into free-form mode.

## Still untested or incomplete

- Remaining Behavior, Test, and Save owner steps.
- Exit/Resume owner validation after the latest drag-preview fix.
- Guidance-disabled free-form regression after the latest fix.
- Browser refresh during the guided workflow.
- Leave-and-return persistence.
- Invalid drop, pointer cancellation, rapid repeated drops, and double-click behavior.
- Keyboard-only, touch, narrow-screen, zoomed, and assistive-technology operation.
- Full dataset matrix and clipping/overflow limits.
- Production integration, persistence, permissions, entitlements, player equivalence, and publishing.

## Restart prompt for a new chat

> Continue the Vennue Theme Studio owner test using the working design bundle at `docs/design/theme-studio/` and the live mock at https://vennue-theme-studio-draft.jmiedreich.chatgpt.site. Owner testing is complete through Appearance: Weight changed from 700 to 600 and Variants opened automatically. Resume at Variants → Sold out, then test Long text, Save theme, Exit/Resume Guide, and guidance-disabled free-form mode. Update the existing mock only if the owner finds a defect; do not redesign it.
