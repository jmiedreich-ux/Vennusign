# VennueSign Theme Studio engineering guide

**Date:** 2026-09-09
**Status:** Working engineering and wireframe guide; owner review remains in progress
**Source baseline:** `master` at `36977cc` plus the owner conversation and wireframes recorded on 2026-09-07 through 2026-09-09
**Audience:** Theme Studio product design, Content Platform, renderer, API, persistence, and implementation reviewers

## 1. Purpose and authority

This guide preserves the complete current Theme Studio discussion in an engineering form. It reconciles the working design bundle merged by `36977cc` with the approved Content Platform direction and records every wireframe made during the follow-on conversation, including rejected and superseded iterations.

This document is not production implementation authority. The entire Theme Studio surface remains a draft while owner review continues. It is the bridge into the later design authority, question register, exact render-definition contract, and milestone plan under `docs/features/theme-studio/`.

When sources conflict, use this order:

1. Approved Content Platform and engineering architecture.
2. Existing accepted customer behavior in the Menus decisions.
3. Owner-confirmed Theme Studio interaction behavior.
4. The newest non-superseded wireframe for placement and visual intent.
5. Earlier mock behavior and exploratory wireframes.

“Current” in this guide means the newest draft iteration. It does not mean owner-approved for implementation.

## 2. Product definition

Theme Studio creates reusable visual definitions for structured content. It does not own content records, record libraries, provider authority, menu items, films, showtimes, or operational truth.

The durable flow is:

```text
Content type
  -> immutable data-model version
  -> content instance and revision
  -> compatible Theme revision
  -> immutable Published Presentation
  -> Runtime Package
  -> Screen and Player Output
```

Theme Studio edits the Theme side of this flow. Content Builder supplies the content revision. The compiler and shared renderer combine them. Saving a Theme revision never claims that a Screen changed.

## 3. Architecture reconciliation

| Area | Bundle at `36977cc` | Current architecture | Required Theme Studio direction |
| --- | --- | --- | --- |
| Product scope | Current mock presents a Menu-oriented Theme workflow | Menu is the first content type, not the permanent platform boundary | Keep Menu as the first fixture, but make the editor, components, binding language, and contracts model-neutral |
| Model identity | Mock speaks generally of `menu.v1` | Data models are immutable and versioned | Every Theme draft and released Theme revision pins an explicit model identity and version |
| Bindings | Fields can be placed into structured components | Themes may bind only to paths declared by the pinned model version | Store explicit collection, field, and state paths; never infer a live binding from display labels |
| Record source | Earlier mock language allows “actual customer data” and component data-source controls | Record libraries and provider queries belong to the Content Platform | Theme Studio may choose a test dataset, but never chooses or edits the production record source |
| Repeated records | Earlier design says Menu item repeater and later drafts temporarily say Collection | A collection can be inline, library-composed, library-referenced, or provider-query driven | The neutral component name is **Repeater**. Its record type is derived from the first compatible dropped field; source ownership remains outside Theme Studio |
| Operational state | Earlier guided workflow treats Sold out as a Variant | Operational state is layered over the Published Presentation and is not a layout variant | `state` is a normal model field type. The model defines values; Theme Style maps them to appearances; Rules may reference them |
| State geometry | Earlier mock does not fully constrain layout response | TS-C1: a state response may not change how much space a record takes or repaginate a wall | State appearance may change paint and visibility only within the reserved geometry unless a later contract explicitly proves an equivalent footprint |
| Theme lifecycle | Mock uses save and publish language in several places | Released model and Theme versions are immutable; a change creates a successor | Drafts are mutable; saving creates/updates draft work; releasing creates an immutable Theme revision; Screen publication remains a separate Content/Presentation act |
| Renderer | Mock proposes one Canvas Render Definition | Editor, validation, static generation, and Players must use one shared renderer | No consumer may reimplement layout, state mapping, capacity, overflow, or typography behavior |
| Static/live/hybrid | Present in the earlier plan | Still valid | Output support belongs to the Theme/renderer contract and publication validation, not an element-level convenience switch |

## 4. Non-negotiable engineering invariants

1. A Theme revision binds to one explicit immutable data-model version.
2. Every binding stores a stable model path or identifier, not a user-facing label.
3. A Released Theme revision is immutable.
4. A Published Presentation pins the content revision, data-model version, Theme revision, renderer compatibility identity, and target assignment set.
5. Theme Studio never edits production content records or chooses their library/provider source.
6. The editor, validation worker, static renderer, live renderer, and Player use the same render semantics.
7. Guided and free-form editing converge on the same valid render definition.
8. Operational state is a normal declared model field type with state semantics; it is not a menu-only feature.
9. State appearance cannot change record capacity or trigger live repagination.
10. Saving a Theme never implies that a Screen is live, current, received, or applied.
11. Invalid drops create nothing. A successful drag is visible, target-specific, and reversible.
12. A wireframe revision supersedes only the screen it revises; distinct interaction states remain separate storyboard screens.

## 5. Model, record, and binding ownership

### 5.1 Data model

A released data-model version defines:

- record and collection types;
- stable field and collection paths;
- field types, including `text`, `number`, `image`, and `state`;
- allowed state values and their semantic meaning;
- validation and formatting metadata;
- compatible Repeater targets;
- editor labels and fixture-generation metadata;
- provider-authority and mutability rules consumed by Content Builder.

The UI may display `menu.v1 · released` as the current model context. The stored contract must use the final stable model identity and version fields once those names are approved.

### 5.2 Record library and sources

The typed record library is the canonical reusable/imported record layer. A model collection may be inline-owned, manually composed from library records, a library reference, or provider-query driven.

Those are Content Platform and Content Builder decisions. Theme Studio receives a resolved content shape and test fixtures. Therefore the Theme inspector must not contain controls such as:

- Records: Items;
- Item Library;
- provider/source selector;
- ordering owned by Content Builder;
- add/edit/delete record actions.

### 5.3 Repeater binding

The owner chose field-first derivation:

1. A neutral Repeater is dropped into a real layout region.
2. It is empty and unbound.
3. The first model field dropped into it determines the compatible repeating record type.
4. The Repeater records that model path and filters later drop targets to compatible fields from the same record type.
5. Content Builder supplies the actual records and order at compile/publication time.

The inspector may explain “derived from first dropped field,” but it must not make that explanation look like a production source picker.

### 5.4 State fields

The owner clarified that Availability is a normal model field whose type is `state`. It must not be represented as a special “Availability component,” a special rule type, or a menu-only editor feature.

The correct responsibilities are:

- **Model:** declares the field, its type, allowed values, and state semantics.
- **Operational system:** supplies the current value when it is an operational fact.
- **Theme Style:** maps each state value to a visual treatment.
- **Rules:** may use a compatible state value as a condition for another element’s conditional behavior.
- **Renderer:** applies the mapping without changing the reserved record footprint.

The sidebar caption “State treatment · no row slot” in several wireframes is therefore not final. The next pass must present the field normally while preserving the operational-state geometry invariant in the render contract.

## 6. Editor information architecture

### 6.1 Shell

- Edit and Test are the canvas modes.
- The top context identifies the pinned model version and target canvas size.
- Theme settings own display-level configuration such as safe area.
- Save language must remain explicit that changes are not live.
- Light mode is the active wireframe direction. Dark mode is retained as an exploration only.
- The ruler expresses canvas coordinates/pixels for precise placement. It is editor chrome and is never rendered on the Screen.

### 6.2 Left rail ownership

| Rail | Owns | Must not own |
| --- | --- | --- |
| Layouts | Supplied structural presets | Styling, conditional Rules, arbitrary custom grid construction in this version |
| Components | Neutral structured components: Title, Repeater, Image, Content block | Menu-specific names, records, or source selectors |
| Fields | Fields from the pinned model version, grouped by model record type | Production record editing or library management |
| Elements | Unbound decorative primitives | Structured model bindings |
| Assets | Fonts, logos, images, licensing/availability | Record libraries |
| Pages | Page order, page-level styling, continuation/overflow ownership | Layout-preset styling |
| Variants | Deliberate presentation variants | Operational state values such as Sold out |

### 6.3 Layouts

This version supplies presets; users do not construct arbitrary page structures. The current Two-column preset creates:

- one Header region;
- equal Left and Right columns;
- empty drop regions.

The Layout inspector shows read-only Structure. It does not show Style or Rules tabs. Page background and outer padding belong under Pages. Theme/display safe area belongs in Theme settings.

### 6.4 Components

Component names must remain neutral:

- Title;
- Repeater;
- Image;
- Content block.

“Collection” is retired as the component label. A collection is a model/data concept; Repeater is the visual component that renders repeated records.

### 6.5 Fields and drop placement

Fields do not decide their own position. The containing component owns its slot arrangement.

- The first dropped field becomes the first slot.
- When a second field is dragged in, the canvas exposes explicit compatible targets such as right or below.
- The system may highlight a likely target but does not silently choose one.
- In the Menu fixture, Item name is the primary slot and Price is placed in the right slot.

## 7. Inspector ownership

### 7.1 Properties

Properties explains what the selected object is and how it is bound.

For a bound field, the current draft shows:

- Field — read-only;
- Repeater — read-only;
- Model version — read-only;
- When empty — currently shown as `Hide field`.

The owner explicitly removed:

- Visible;
- Accessible label.

There is an unresolved placement question: the conversation proposed moving `When empty` to Rules, but the owner did not confirm that move. Until answered, the latest wireframe keeps it in Properties.

For a Repeater, Properties may show its derived repeating path/type and pinned model version. It must not choose the production record source.

### 7.2 Style — text or number field

When a text-like field is selected, Style contains:

- font family;
- font size;
- font weight;
- text color;
- line height;
- letter spacing;
- horizontal alignment;
- text transform.

These controls belong to the selected field, not the Repeater.

### 7.3 Style — Repeater

The Repeater owns shared row treatment:

- arrangement: Inline or Stacked;
- vertical alignment: Top, Center, or Baseline;
- row spacing slider;
- row padding slider;
- optional connector: None, Dot leader, Dashed, or Solid;
- conditional connector appearance when a connector is active: color, thickness, and spacing;
- row divider: None or Line;
- row background: Transparent or Theme surface.

Connector is optional and defaults to None. Connector controls apply only to an inline arrangement. Field typography and a duplicate bound-field map do not belong in Repeater Style.

### 7.4 Style — state field

When the selected model field is type `state`, Style gains a State appearance section. The model supplies the allowed values; the Theme maps each value to a treatment. Conceptually:

| Model state value | Theme appearance |
| --- | --- |
| Available | Normal/default treatment |
| Sold out | Muted, struck, alert-colored, or another approved treatment |
| Unavailable | Approved subdued or hidden treatment within the fixed footprint |

Exact controls and allowed treatment properties remain an owner-review question. State appearance belongs in Style, not in a separate Availability component and not in the generic Rules action list.

### 7.5 Rules

The new discussion uses Rules for conditional element behavior:

```text
WHEN [compatible model field] [operator] [value]
THEN [approved element behavior]
```

A state field is one ordinary compatible condition source. Example: `When Availability is Sold out -> show this Sold out label`.

The earlier implementation plan instead described Rules as required-content, minimum-type-size, maximum-density, and supported-output-mode constraints. Those are validation and protected-definition concerns, not the same thing as per-element conditional behavior. Do not silently combine them. Recommended ownership for the next pass:

- inspector Rules: conditional presentation behavior;
- Diagnostics and Theme settings: safety constraints and actionable validation;
- publication gate: authoritative enforcement.

The exact Rules action set is still open. Show/hide is the only behavior discussed concretely. Arbitrary typography mutation from Rules is not approved; state-conditioned visual mapping belongs in State appearance.

## 8. Accepted interaction sequence that must be preserved

The existing owner-verified guided behavior remains intact:

1. Create new -> Begin blank.
2. Select the Two-column preset.
3. Components opens.
4. Visibly drag Repeater into a real Left or Right column.
5. The dropped Repeater is empty.
6. Fields opens.
7. Visibly drag Item name into the Repeater.
8. Sample names appear only after the successful drop.
9. Item name Style opens.
10. Change Weight from 700 to 600.
11. The guide advances.

Follow-on working behavior adds:

12. Drag Price into the same Repeater.
13. Show explicit right/below drop targets.
14. Drop Price into the right slot.
15. Select Repeater Style to control arrangement, spacing, connectors, divider, and row background.
16. Select Item name to control its own Properties, Style, and Rules.

Do not make the storyboard clickable yet. First complete the feature inventory, panel ownership, and placement decisions.

## 9. Conversation decision ledger

This is the engineering record of the full follow-on conversation, in order.

| Sequence | Discussion | Result |
| --- | --- | --- |
| 1 | Compare the merged Theme Studio bundle with current architecture | Reconcile before the next wireframe pass; do not treat the August mock as implementation authority |
| 2 | Explain the problem plainly | Theme Studio must stop assuming everything is a Menu and bind to explicit versioned models and paths |
| 3 | Complete end-to-end wireframe problem | Build a static storyboard before a clickable prototype; preserve decisions and stop merging unrelated ideas |
| 4 | Initial flow was obvious, not the blocked part | Shift focus from entry flow to editor/canvas features and their logical placement |
| 5 | Light and dark explorations | Keep light mode as the active direction; retain dark mode only as an exploration |
| 6 | Start with Layouts | Define the left rail one area at a time |
| 7 | Page structures | Use supplied presets in this version; no arbitrary custom structure builder |
| 8 | Two-column structure | Header plus empty equal Left and Right columns |
| 9 | Ruler | Canvas pixel coordinates; editor-only |
| 10 | Layout Style and Rules | Remove them from Layouts; page styling moves to Pages and element styling to selections |
| 11 | Final screen delivery | Preserve one ordered final storyboard containing only the latest revision of each screen |
| 12 | Components | Use neutral names across models |
| 13 | Component Properties | Show all controls in the storyboard, even when the happy path does not visit every tab |
| 14 | Screen tracking | Entire set remains draft; distinguish interaction states from revisions; retain only newest revisions in the final deck |
| 15 | Fields rail and Records/Items | Theme Studio does not add records; Content Builder/Menu Builder owns records and order |
| 16 | Collection terminology | Rename the visual component to Repeater |
| 17 | Binding interaction | Dragging the first field into Repeater derives its record type; no Records: Items picker |
| 18 | Additional Repeater properties | Keep source/library functions elsewhere; expose only component-owned properties |
| 19 | Price placement | Drag Price into the Repeater; explicit targets let the user choose right or below |
| 20 | Field alignment | Repeater owns the row pattern; fields do not silently know their alignment |
| 21 | Connector | Optional type; None is the default; inline-only |
| 22 | Row spacing | Make it a slider; remove redundant row-layout and bound-field diagrams from Style |
| 23 | Expanded Repeater Style | Add vertical alignment, padding, divider, background, and conditional connector appearance |
| 24 | Text formatting | Selecting the field opens its Style controls for font, size, weight, color, spacing, alignment, and transform |
| 25 | Visual completeness | Every feature and meaningful tab state must have a picture in the final storyboard |
| 26 | Field Properties | Binding details are read-only and tied to a released model version |
| 27 | Visible property | Removed by owner |
| 28 | Accessible label property | Removed by owner |
| 29 | Rules | Conditional behavior may use compatible model fields; exact action set remains open |
| 30 | Availability | Corrected: Availability is a normal model field of type `state`, not a special component or rule type |
| 31 | State conditional formatting | State-value appearance belongs in Style; Rules may use state as a condition for another element |
| 32 | Inspector summary | Use the actual panel visual language; omit the whole editor shell; organize Layouts, Components, and Fields by Properties, Style, and Rules |

## 10. Verbatim owner-message transcript

The messages below preserve the owner's words from the available conversation context, including
typos and shorthand. The assistant's exact historical prose was not available as an export in the
repository context used to make this guide; it is therefore not reconstructed or presented as a
verbatim transcript. The decisions it produced are captured in Section 9, and every available
visual response is preserved in Sections 11 and 12.

1. “VennuSign commit 36977cc merged the Theme Studio working design bundle while owner review remains in progress. Compare that bundle with the current engineering architecture, especially versioned data models, record-library sources, operational state fields, and Theme revisions bound to explicit model versions and paths. Help me identify any menu-specific assumptions in the current Theme Studio design that should change before the next wireframe pass, while preserving already accepted interaction behavior.”
2. “Ok I don’t know what this is saying can you tell me plainly and shortly”
3. “Yes agree”
4. “The biggest problem I have now, is I can’t get any tool or AI to properly wireframe the complete them horney, end to end, displaying all the ui features and how they are used. Some add additional good ideas then merge existing ideas wrong. I had research done on all tools in this area done, not so we can duplicate, so we can understand layouts and what these tools generally feel like. I’m at a loss.”
5. “Ok”
6. “The mermaid doesn’t render”
7. “Still no good in phone”
8. “I think that start is common sense and is clear and is not what is stuck”
9. “I don’t think we should go clickable yet. We need to conclude what features the editor and canvas have and where the placement of these features in there best logical easy to use places”
10. “Ok”
11. “What is the next steps”
12. “Ok let’s do it”
13. “I love this, could I see this is darkmode too?”
14. “ok, lets continue the wireframe in the light mode you first gave me. what is next?”
15. “I think we are jumping the gun, should we start with layouts on the left and how we imagine that to be?”
16. “how do the page structures get created?”
17. “In this version we will come up with the presets for the user”
18. “ok show the screen with a two column structure chosent”
19. “What does the ruler represent?”
20. “What would display in the styles and rules tabs”
21. “Does it make sense to move those styles function to another screen?”
22. “Okay we should update the graphic then and these designs notes”
23. “In the end of this process, how will I get all these screens ?”
24. “Ok let’s imagine what the next screen will be”
25. “Ok so the important thing here is the components need to be neutral when it comes to there names”
26. “Yes, can I see the first draft”
27. “Would any of these components need any properties on the right”
28. “Yes that makes sense but for our story board we need to show all the controls how we see it”
29. “Yes, and can you tell me how you are keeping track of the screens we have approved At least for now?”
30. “I would say the entire thing is a draft but decisions were made, I just now that there could be multiple irritations of screen and I just want to be sure we only include the last one. Like there will be many components screens, a, b etc.”
31. “Ok great”
32. “Ok do it”
33. “This screenshot shows us on the fields rail”
34. “On the right it says records with item inside, what exactly is that referring to”
35. “So a collection is a repeater?”
36. “Ok I think we should call the component a repeater then”
37. “Wait”
38. “I feel like dragging the fields into the repeater is better, how does that change the properties on the right”
39. “Ok, does that then expose other properties or they are reserved for other functions”
40. “Ok let’s see this screen”
41. “Ok so on the menu builder individual items get added to the repeater”
42. “Ok then what do we show next”
43. “Ok do just the next screen”
44. “Ok next”
45. “So how do the fields know how to align in the repeater”
46. “The type of style connector the dots should be optional type”
47. “Should we see that now?”
48. “What is row spacing”
49. “We should turn it into a slider, and I don’t think we need row layout text and field again in style”
50. “Seems there should be more style options”
51. “Ok let’s see it”
52. “How do we change the text color and font and font size and all those things”
53. “We should have pictures that show that as well”
54. “What does the property do?”
55. “Everything needs to be visually recorded so give me all the screens needed”
56. “We don’t need visible”
57. “Accessible label can be removed”
58. “What is under rules for each element”
59. “I want to clarify that available field is a normal model that is of type state”
60. “Correct”
61. “I think it’s fair if you dedicate a full screen of all the different variations of the property styles and rules panels so we can see a summary so far of what all the options are?”
62. “Everything”
63. “No that’s a confusing way of looking at it”
64. “No, I want them to look how they do now, row 1 is layouts, then the three row 2 components, then the 3, then rows 4 fields or do it by column , layout, then the three vertical underneath, etc.”
65. “Not the whole shell just the panels”
66. “Does the rules allow for conditional formatting on the state fields?”
67. “Ok now, take this entire conversation so far and create a new theme studio engineering guide with today’s date and put all this chat and pictures into it. Then have an independent fidelity review done on this chat to make sure the information is correct and correctly uploaded to GitHub and merged into master”

## 11. Wireframe register

All images made in the conversation are retained below. Rejected and superseded images are evidence of the decision path and must not be used as the final implementation reference.

| No. | Image | Status | Meaning |
| --- | --- | --- | --- |
| 01 | [Light editor concept](wireframes/2026-09-09/01-light-editor-concept.webp) | Exploration | Established the light shell and editor/canvas direction |
| 02 | [Dark mode exploration](wireframes/2026-09-09/02-dark-mode-exploration.webp) | Exploration | Requested comparison; not the active wireframe direction |
| 03 | [Two-column with Layout Style/Rules](wireframes/2026-09-09/03-layout-two-column-with-style-rules-superseded.webp) | Superseded | Incorrectly put Style and Rules under Layouts |
| 04 | [Two-column Layout current](wireframes/2026-09-09/04-layout-two-column-current.webp) | Current draft | Supplied preset and read-only Structure only |
| 05 | [Neutral Components with Collection](wireframes/2026-09-09/05-components-neutral-collection-superseded.webp) | Superseded | Neutral shelf direction, but Collection term was rejected |
| 06 | [Collection Properties](wireframes/2026-09-09/06-collection-properties-superseded.webp) | Superseded | Records picker and Collection terminology are incorrect |
| 07 | [Repeater with first field bound](wireframes/2026-09-09/07-repeater-first-field-bound.webp) | Current interaction draft | First field derives the repeating type |
| 08 | [Item name Style weight](wireframes/2026-09-09/08-item-name-style-weight.webp) | Current interaction draft | Field-specific typography and 700 -> 600 sequence |
| 09 | [Price drag target](wireframes/2026-09-09/09-price-drag-target.webp) | Current interaction draft | Compatible Repeater target is visible during drag |
| 10 | [Repeater Style first draft](wireframes/2026-09-09/10-repeater-style-first-draft.webp) | Superseded | Still duplicated row-layout/binding information |
| 11 | [Repeater Style simplified](wireframes/2026-09-09/11-repeater-style-simplified.webp) | Superseded by expanded panel | Introduced direct row-spacing slider and removed duplication |
| 12 | [Repeater Style expanded](wireframes/2026-09-09/12-repeater-style-expanded.webp) | Current draft | Current Repeater style grouping and Dot leader controls |
| 13 | [Item name Text Style](wireframes/2026-09-09/13-item-name-text-style.webp) | Current draft | Complete text-style ownership |
| 14 | [Item Properties with Visible and accessible label](wireframes/2026-09-09/14-item-properties-visible-accessible-superseded.webp) | Superseded | Both controls were subsequently removed |
| 15 | [Item Properties without Visible](wireframes/2026-09-09/15-item-properties-accessible-superseded.webp) | Superseded | Accessible label was subsequently removed |
| 16 | [Item Properties current](wireframes/2026-09-09/16-item-properties-current.webp) | Current draft | Read-only binding plus When empty; location of When empty remains open |
| 17 | [Panel reference first draft](wireframes/2026-09-09/17-panel-reference-first-draft-rejected.webp) | Rejected | Grouped by option type instead of the existing panels |
| 18 | [Whole-shell inspector storyboard](wireframes/2026-09-09/18-whole-shell-inspector-grid-rejected.webp) | Rejected | Too much shell; not the requested summary form |
| 19 | [Panel-only reference format](wireframes/2026-09-09/19-panel-reference-current-format.webp) | Current format, content corrections required | Correct summary format; next revision must remove Layout Style/Rules, add State appearance, and retain only confirmed Rules actions |

## 12. Full visual appendix

### 12.1 Shell and layout direction

![Light editor concept](wireframes/2026-09-09/01-light-editor-concept.webp)

![Dark mode exploration](wireframes/2026-09-09/02-dark-mode-exploration.webp)

![Superseded layout tabs](wireframes/2026-09-09/03-layout-two-column-with-style-rules-superseded.webp)

![Current two-column layout](wireframes/2026-09-09/04-layout-two-column-current.webp)

### 12.2 Components, Repeater, and binding

![Superseded Collection shelf](wireframes/2026-09-09/05-components-neutral-collection-superseded.webp)

![Superseded Collection properties](wireframes/2026-09-09/06-collection-properties-superseded.webp)

![Repeater first field bound](wireframes/2026-09-09/07-repeater-first-field-bound.webp)

![Item name style weight](wireframes/2026-09-09/08-item-name-style-weight.webp)

![Price drag target](wireframes/2026-09-09/09-price-drag-target.webp)

### 12.3 Repeater Style iterations

![Repeater Style first draft](wireframes/2026-09-09/10-repeater-style-first-draft.webp)

![Repeater Style simplified](wireframes/2026-09-09/11-repeater-style-simplified.webp)

![Repeater Style expanded](wireframes/2026-09-09/12-repeater-style-expanded.webp)

### 12.4 Field Style and Properties iterations

![Item name Text Style](wireframes/2026-09-09/13-item-name-text-style.webp)

![Superseded Item Properties with Visible](wireframes/2026-09-09/14-item-properties-visible-accessible-superseded.webp)

![Superseded Item Properties with accessible label](wireframes/2026-09-09/15-item-properties-accessible-superseded.webp)

![Current Item Properties](wireframes/2026-09-09/16-item-properties-current.webp)

### 12.5 Inspector-summary iterations

![Rejected first panel summary](wireframes/2026-09-09/17-panel-reference-first-draft-rejected.webp)

![Rejected whole-shell summary](wireframes/2026-09-09/18-whole-shell-inspector-grid-rejected.webp)

![Current panel-only summary format](wireframes/2026-09-09/19-panel-reference-current-format.webp)

## 13. Screens still required before a clickable prototype

The following screens remain to be designed or corrected. Their absence is explicit; do not infer their behavior from the mock.

1. Corrected panel-only inspector summary.
2. Field Rules full-size screen.
3. State-field Properties screen showing a normal field with type `state`.
4. State appearance Style screen with model-defined values.
5. Repeater Rules full-size screen after the exact action set is approved.
6. Title Properties, Style, and Rules.
7. Image Properties, Style, Rules, and missing-image behavior.
8. Content block Properties, Style, and Rules.
9. Elements rail and representative primitive selection states.
10. Assets rail, font/image availability, and missing/licensing states.
11. Pages rail, page-level style, order, continuation, and overflow ownership.
12. Variants rail without operational Sold out as a layout variant.
13. Test mode for typical, long, empty, maximum, missing-image, and operational-state fixtures.
14. Diagnostics navigation from issue to exact component/path.
15. Save revision and immutable release boundary.
16. Revision/history view showing explicit model and renderer compatibility.
17. Menu-first/Content-first handoff without moving record ownership into Theme Studio.
18. Permission, entitlement, conflict, retry, refresh, leave-and-return, keyboard, touch, and narrow-screen states.

## 14. Open owner decisions

1. Does `When empty` remain a simple Property or move into Rules?
2. Is the first Rules release limited to Show/Hide, and which targets may it affect?
3. Which State appearance controls are allowed while preserving geometry?
4. Can a State appearance hide paint while retaining reserved space, and how is that explained?
5. What exact stable model/path identifiers appear in the UI versus only in the stored contract?
6. Are per-page Theme token overrides allowed, and at which scopes?
7. Which output modes ship in the first Theme Studio milestone?
8. What are the exact Canvas Render Definition field names and compatibility policy?
9. Which supplied layout presets ship first?
10. What are the final Properties, Style, and Rules controls for Title, Image, and Content block?

## 15. Engineering handoff

Before implementation:

1. Resolve the owner decisions above in a Theme Studio question register.
2. Correct the inspector-summary wireframe and finish the required screen inventory.
3. Land owner-approved design authority under `docs/features/theme-studio/`.
4. Freeze the minimum data-model identity, binding-path, State appearance, Theme revision, and renderer compatibility contracts.
5. Reconcile the old Canvas Render Definition plan against Published Presentation and Runtime Package terminology.
6. Produce a bounded milestone plan where schema, API, editor, renderer, tests, and owner acceptance ship together.

No Theme Studio implementation should begin from the pictures alone.


## 16. Buildability contract and Foundry alignment

Theme Studio wireframes are not standalone instructions for AI agents. Each approved storyboard screen must be accompanied by a screen-and-behavior contract and a Foundry mapping. Together, these three artifacts are the implementation authority for that screen:

| Artifact | Answers |
| --- | --- |
| Wireframe | What is visible, where it appears, and the intended interaction sequence |
| Screen-and-behavior contract | What happens, what is stored, validation, non-happy paths, and acceptance evidence |
| Foundry mapping | Which established design-system primitives, tokens, states, and accessibility behavior must be used |

### 16.1 Foundry status

Foundry is VennueSign’s shared design system. It is being established and is not yet a complete catalogue. Theme Studio must therefore use the following status labels in every work packet:

| Status | Meaning | Implementation rule |
| --- | --- | --- |
| Existing Foundry pattern | The required primitive and states already exist in Foundry | Use it as-is; do not restyle or recreate it in Theme Studio |
| Foundry addition | A reusable primitive or interaction is required but not yet defined in Foundry | Specify it as a Foundry addition before implementation; do not create a Theme Studio one-off |
| Theme Studio composition | A Theme Studio-specific arrangement of existing Foundry primitives | Build the composition using the stated Foundry parts; preserve the product-specific behavior contract |

A missing Foundry part does not block Theme Studio design. It becomes a named dependency in the work packet. An implementation agent may not silently substitute a new control, interaction, token, or state treatment.

### 16.2 Required mapping for the current Repeater interaction

The first binding flow in the current storyboard must be mapped as follows. The named Foundry primitive identifiers are placeholders until Foundry publishes its canonical names; they are not permission to invent substitutes.

| Theme Studio part | Required status | Foundry responsibility | Theme Studio responsibility |
| --- | --- | --- | --- |
| Editor shell, left rail, inspector shell | Existing pattern or Foundry addition | Shell layout, responsive behavior, focus and panel states | Rail ownership and selected-context content |
| Tabs, buttons, selects, number inputs | Existing pattern | Control anatomy, keyboard behavior, disabled/error states | Labels, values, validation meaning |
| Field row in Fields rail | Existing pattern or Foundry addition | Search/list-row anatomy, focus state, drag affordance | Model label, type, compatible/incompatible state |
| Empty Repeater drop target | Foundry addition | Valid/invalid/active drop-state treatment and accessible feedback | Compatibility from the pinned model and derived record type |
| Drag preview and target indicators | Foundry addition | Held-source, target, cancel, valid/invalid visual and keyboard/touch conventions | Field identity and explicit right/below placement semantics |
| Inspector Properties / Style / Rules panels | Existing pattern or Foundry addition | Panel layout, tab behavior, controls, focus management | Binding details, panel ownership, model-aware choices |
| Row-spacing slider and color/font controls | Existing pattern | Control interaction, value presentation, input validation | Repeater or field-specific style mapping |
| Inline warnings and save failure | Existing pattern | Warning/error treatment, announcement, retry affordance | Domain message and non-live-save semantics |

### 16.3 Work-packet minimum

Every Theme Studio implementation packet must contain:

1. A link or identifier for the latest non-superseded wireframe only.
2. The visible controls and their panel/rail placement.
3. The user behavior, stored data, model/version/path constraints, and error/cancel states.
4. A Foundry mapping table using the status labels above.
5. Explicit open decisions marked **Do not implement or infer**.
6. Acceptance checks that prove both UI behavior and stored-contract behavior.

The packet must never say merely “match the wireframe.” It must name the relevant Foundry dependency or composition and state what an agent must do when the dependency is not available.

### 16.4 Explicitly carried-forward decisions

The following remain open and are deliberately carried forward rather than left for an implementation agent to decide: Repeater Rules action set, empty-preview policy, undo semantics, and touch/keyboard drag behavior. They remain subject to Section 14 and must be marked **Do not implement or infer** in any packet that reaches them.
