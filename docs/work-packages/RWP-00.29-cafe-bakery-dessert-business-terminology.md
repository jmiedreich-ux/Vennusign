# RWP-00.29 — Café, Bakery & Dessert Business Terminology

## Status

Complete in this proposed merge state.

## Issue

- #504

## Objective

Define the canonical terminology and language model for Café, Bakery & Dessert so onboarding, navigation, content editing, help text, analytics, starter content, and guest-facing screens can use consistent industry-appropriate wording without coupling terminology to entitlements, permissions, ordering, fulfillment, or implementation.

## Dependency verified

- RWP-00.28 is complete and merged.
- Restaurant remains the canonical inherited baseline.
- The merged Café, Bakery & Dessert industry and subtype model is authoritative.
- No competing branch, pull request, or active claim owned RWP-00.29 when claimed.

## Delivered

- Added a canonical operator-facing and guest-facing terminology glossary to `track0/industries/cafe-bakery-dessert.md`.
- Defined inherited Restaurant terms that remain unchanged.
- Defined bounded terminology for products, categories, sizes, options, modifiers, temperatures, milk or base choices, flavors, add-ins, toppings, batches, freshness, availability, limited quantity, expected return, preorders, custom orders, pickup, collections, specials, and service periods.
- Defined subtype-specific preferred terms and neutral organization-wide fallbacks.
- Defined hybrid-venue and mixed-organization fallback behavior.
- Distinguished object names, action verbs, display labels, state labels, and analytics labels.
- Defined language rules for ambiguous concepts such as item, product, menu, list, option, modifier, batch, fresh, limited, sold out, unavailable, preorder, custom order, pickup, collection, and service period.
- Applied the project-local Impeccable `clarify` guidance to UI-facing terminology planning.
- Updated the Track 0 capability matrix, project status, tracker, and current handoff.

## Canonical language decisions

1. **Content** is the neutral umbrella term across mixed organizations; **menu**, **drink menu**, **bakery case**, **today's selection**, **dessert menu**, **flavor list**, and **collection** are context-specific presentations of content.
2. **Item** remains the neutral operator-facing object. Guest-facing surfaces use the actual product name or a known noun such as drink, coffee, tea, pastry, bread, cake, dessert, flavor, smoothie, bowl, or take-home item.
3. **Category** is the neutral grouping term. Use section, collection, case, menu group, or flavor group only when the guest meaning is clearer.
4. **Size** is the canonical quantity or format choice when the business presents named or measured sizes. Do not use portion, serving, scoop, slice, cup, cone, or pack as universal substitutes; use them only when they describe the actual format.
5. **Option** is the neutral guest-facing choice. **Modifier** is acceptable operator or integration language but should not be the default guest label.
6. **Temperature**, **milk or base choice**, **flavor**, **add-in**, and **topping** are distinct option types. A subtype may suppress irrelevant types without creating a capability gate.
7. **Batch** describes a produced group that becomes available together. **Next batch** or **available again at** may be shown only when the timing is known.
8. **Freshness** describes operator-provided or source-authoritative product guidance. The system must not infer or promise freshness, production time, shelf life, or safety.
9. **Available**, **unavailable**, **sold out**, and **limited** are distinct product states or presentation values. They are not feature flags, plan-access labels, or interchangeable synonyms.
10. **Preorder** means an item or collection may be requested before its pickup or fulfillment time. **Custom order** means guest-specific configuration or production is expected. Neither term authorizes ordering, payment, production, or fulfillment implementation.
11. **Pickup** is the neutral guest collection term. Use **collection** only where a venue's established language requires it, and do not alternate pickup and collection inside one flow.
12. **Service period** is the neutral operator term for a bounded operating interval. Guest-facing copy uses recognizable names such as breakfast, morning, lunch, afternoon, evening, late night, or pickup hours.
13. **Special** is the neutral operator term for a promoted item or offer. Guest copy may use seasonal drink, daily special, featured pastry, limited flavor, collection, or release when context supports it.
14. **Venue** remains the neutral local business unit. Use outlet, counter, stand, shop, or property only when the parent industry or actual operating context requires it.

## Availability and timing language

- **Available:** the item can currently be offered through the represented service context.
- **Unavailable:** the item cannot currently be offered and the more specific reason is unknown, not communicated, or not depletion.
- **Sold out:** the currently sellable quantity or current batch is exhausted.
- **Limited:** quantity or offer duration is intentionally constrained; it does not imply a known remaining count.
- **Next batch:** another produced group is expected; include a time only when it is known and authoritative.
- **Available again:** an expected return is known; do not invent timing.
- **Preorder available / preorder closed:** describes the current preorder window only, not general item availability.
- **Pickup available / pickup paused:** describes the represented pickup channel or instructions only.

Unknown state must remain unknown. Guest copy must not promise exact quantity, freshness, production time, or return timing unless the source is authoritative.

## Subtype terminology result

- **Café:** menu, drinks, food, daily or seasonal specials, service periods, pickup, venue information.
- **Coffee Shop:** coffee menu, espresso drinks, brewed coffee, hot or iced, size, milk choice, extra shot, flavor or syrup, seasonal drinks, pastries.
- **Tea Shop:** tea menu, tea base or style, hot or iced, size, sweetness, ice, toppings, add-ins, seasonal drinks.
- **Bakery:** today's selection, bakery case or display case, bread, pastry, baked goods, batch, next batch, sold out, preorder, pickup.
- **Patisserie:** pastry or cake collection, flavor, size or servings where known, custom order, preorder, pickup, seasonal collection, limited availability.
- **Bakery-Café:** bakery case, beverage menu, breakfast or lunch, service period, daily specials, pickup, available and sold-out state.
- **Dessert Shop:** dessert menu, portion or format, flavor, toppings, add-ins, combinations, made to order, seasonal or limited item.
- **Frozen Dessert Shop:** current flavors, scoop or serving format, size, cup or cone, toppings, take-home, limited or rotating flavor.
- **Juice & Smoothie Bar:** juice and smoothie menu, size, base, ingredients, add-ins, boosts where locally used, bowls, seasonal produce, pickup.
- **Unspecified / General Café:** menu, item, category, size, options, availability, special, preorder where supported, pickup, service period.

## Operator actions and state labels

Future operator-facing language should prefer explicit verb-object actions such as:

- `Add item`;
- `Add size`;
- `Add option`;
- `Mark sold out`;
- `Mark available`;
- `Mark unavailable`;
- `Add next-batch time`;
- `Set expected return`;
- `Add preorder instructions`;
- `Add pickup instructions`;
- `Publish menu` or the known subtype-specific content name;
- `Restore previous version`.

Use menu-specific verbs only when the object is truly a menu. Do not label an action `Update`, `Manage`, `Submit`, or `Save changes` when a more specific outcome is known.

## Mixed-organization and hybrid fallback

- Organization-wide and cross-industry surfaces use organization, venue, content, item, category, option, availability, service period, screen, publish, and restore.
- Venue-scoped surfaces may use subtype terminology when the subtype and content type are known.
- Hybrid traits may influence suggestions but must not silently rename customer-authored content.
- When a venue combines equal concepts, use the term that describes the immediate task rather than forcing one subtype noun across the entire venue.
- Copying content between unlike subtypes preserves source names and presents destination terminology as reviewable suggestions only.
- Local custom labels remain authoritative until an authorized user changes them.

## Analytics terminology

Core operational views use neutral dimensions such as venue, content type, item, category, availability state, service period, screen, and publish state. Subtype-specific drill-downs may use drink type, product type, size, option type, batch state, preorder state, pickup context, flavor, or format when the data actually exists. Analytics labels must not imply inventory precision, freshness guarantees, production timing, fulfillment status, or paid access that the source data does not support.

## Impeccable planning result

The project-local Impeccable skill and `clarify` guidance were consulted because terminology will appear in future onboarding, navigation, forms, editor labels, state messages, help text, analytics, and guest-facing screens.

The specification requires future UI copy to:

- keep one noun and one verb for the same concept throughout a flow;
- use specific verb-object actions and name the affected item or content;
- use persistent labels and examples rather than placeholders as labels;
- distinguish first use, no results, filters, permissions, failure, empty content, sold-out state, unavailable state, and unknown timing;
- explain what failed and how to recover without exposing internal codes as the primary message;
- avoid promising freshness, quantity, production time, return time, pickup readiness, or preorder acceptance without authoritative data;
- use complete translatable messages rather than concatenated fragments;
- keep visible labels and accessible names aligned;
- support long product, category, option, venue, and collection names, localization expansion, pluralization, dynamic values, and 200% zoom;
- avoid color-only meaning and unnecessary abbreviations;
- preserve customer-authored terminology rather than silently renaming existing content after a subtype change;
- preserve the approved Sky Blue administrative direction.

This is planning only. No UI strings, components, routes, schema, API, analytics implementation, ordering, pickup automation, or localization resources were changed.

## Classification decisions

1. Industry, subtype, hybrid traits, and terminology preference are **product/domain state**.
2. A terminology preference affects defaults, labels, starter recommendations, help text, analytics presentation, and guest wording only.
3. Terminology does not grant capabilities, alter permissions, increase limits, control rollout, or change commercial access.
4. Batch, freshness, limited-quantity, expected-return, availability, preorder-window, pickup-context, service-period, and option values retain product/domain-state treatment where represented.
5. Manual item editing, manual availability changes, publishing, delivery confirmation, offline awareness, and recovery remain inherited core capabilities.
6. Ordering, payment, production management, fulfillment, inventory, POS, pickup-source, or other synchronization remains a later capability and integration-packaging decision.
7. Customer-authored names and custom labels must be preserved through future profile or subtype changes.

## Validation

Documentation-only review confirmed:

- every issue-listed terminology area is defined or assigned a bounded fallback;
- Restaurant inheritance is explicit and not restated as new capability access;
- operator-facing and guest-facing language are distinguished;
- subtype overrides do not create hidden packages or permissions;
- mixed organizations and hybrid venues have neutral fallback language;
- ambiguous terms have context rules;
- Impeccable language, accessibility, localization, error, state, and recovery guidance is recorded;
- no product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, ordering, payment, production, inventory, analytics, or integration implementation was introduced;
- integration and external-system tests were not applicable and remain skipped under the standing owner instruction.

## Handoff

The next sequential item is **RWP-00.30 — Café, Bakery & Dessert Operating Characteristics** (#505).

RWP-00.30 must document early hours, business-day and service-period behavior, batch production, freshness windows, rotating daily products, sell-outs, preorders, pickup, seasonal demand, table and counter service, and subtype-specific operating differences. It must tie each difference to defaults, terminology, content, screen purposes, or capability classification; remain documentation-only; avoid jurisdiction-specific invention; and must not begin until RWP-00.29 is merged, verified on `master`, issue #504 is closed, and the claim is released.
