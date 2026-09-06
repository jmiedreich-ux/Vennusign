# Theme Studio working decisions

These decisions govern the current mock unless the owner changes them. Owner testing is incomplete, so this folder is a working design bundle rather than final implementation authority.

## Product boundaries

1. **Theme Studio edits reusable design definitions.** Menu content remains separate from the theme.
2. **Saving a theme does not publish a screen.** Menu Builder uses the latest saved theme; screens change only through the menu publishing flow.
3. **The current data model is Menu.** Future models may include Cinema, Tap board, Bakery, and other structured display types.
4. **Theme definitions are intended for the shared renderer.** The editor canvas and the eventual player should consume the same definition semantics.
5. **Output remains dual-mode.** Mostly static screens may publish a high-definition rendered image; genuinely dynamic screens continue through live or hybrid rendering.

## Guided mode and free-form mode

Guided mode is an optional hand-holding layer, not a second editor.

- When enabled for **Create new → Begin blank**, it identifies one recommended action at a time.
- It automatically opens the panel needed for the next action.
- It highlights the exact source control and exact destination.
- It confirms completed actions and advances without making the user hunt for the next panel.
- **Exit Guide** removes guided presentation without deleting work.
- **Resume Guide** returns to the current incomplete step.
- Guidance-disabled free-form mode keeps its existing behavior unchanged.

## Guide presentation

- The active guide card belongs prominently in the center working area.
- Guide callouts use a distinctive non-blue treatment so they cannot blend into the normal Sky UI.
- The current guide card uses purple; active drag previews use amber.
- The guide should visually dominate the moment without obscuring the control or target it describes.
- Targets remain highlighted until the required action succeeds.

## Guided Begin blank workflow

| Stage | Required owner action | Required result | Automatic transition |
|---|---|---|---|
| Structure | Select **Two-column menu** | Empty left and right regions appear | Open **Components** |
| Content 1 of 2 | Drag **Menu item repeater** into either column | One empty repeater is created in the chosen column | Confirm placement; open **Fields** |
| Content 2 of 2 | Drag **Item name** into the repeater | Sample item names appear only after the drop | Confirm binding; open **Item name settings → Style** |
| Appearance | Change a style value, such as Weight 700 → 600 | Appearance is marked complete | Open **Variants** |
| Behavior | Select **Sold out** | Sold-out behavior is confirmed | Open **Test** mode |
| Test | Select **Long text** from Dataset | The difficult content case renders and testing is marked complete | Advance guide to **Save** |
| Save | Select **Save theme** | Reusable theme is saved | Do not claim any live screen changed |

## Drag behavior

The following rules apply to both guided drags:

- The object must visibly follow the pointer while held.
- The source row should look held, and the pointer should use a grabbing state.
- A compact amber preview identifies the exact object being moved.
- The preview disappears on drop or cancellation.
- Dropping on an invalid area must not create content.
- The visible drag treatment must not replace or alter free-form native dragging when guidance is disabled.

### Menu item repeater

- It must be dragged into the left or right column.
- Clicking it is not a substitute for placement.
- Placement creates only an empty container.
- The chosen column is the repeater's actual location.
- No Northside sample title, menu section, names, descriptions, prices, or images appear from placing the repeater.

### Item name

- It must be dragged into the existing repeater.
- Names do not appear before a successful drop.
- A successful drop binds `item.name`, displays sample names, and advances to Style.

## State and feedback invariants

- The guide never completes a step merely because its source control was clicked.
- Each automatic panel transition follows a successful action, not pointer-down.
- Confirmation names what changed and, where relevant, where it was placed.
- Diagnostics distinguish incomplete setup from a ready design.
- Save stays unavailable until content, appearance, behavior, and test requirements are complete.
- A theme save never silently updates a menu or screen.
- Exiting and resuming guidance preserves completed canvas work.

## Rejected behavior

The following observed behaviors were design defects and must not return:

- a subtle guide that blends into the editor;
- the active guide instruction living away from the working area;
- blue guide callouts that look like ordinary interface chrome;
- click-to-add substitution for the two guided drag actions;
- invisible pointer dragging;
- placing the repeater and immediately showing the complete Northside sample menu;
- showing item names before Item name is dropped;
- leaving the user in an unrelated panel after a successful step;
- changing free-form behavior while fixing guided mode.

## Broader implementation requirements not yet represented fully by the mock

Production Theme Studio should eventually:

- prevent saving or publishing a template that clips required content;
- test typical, maximum, long-text, missing-image, sold-out, and overflow data states;
- offer a controlled auto-fix path through the development-agent API for definition-level rendering errors;
- preserve exact equivalence between editor preview and player output;
- support static high-definition image generation alongside live/hybrid output;
- define permissions, entitlements, versioning, rollback, and audit history;
- meet keyboard, touch, responsive, accessibility, refresh, leave-and-return, and error-recovery requirements.
