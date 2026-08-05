# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.17 merged; RWP-00.18 is next
- Café, Bakery & Dessert: RWP-00.29 complete in this proposed merge state; RWP-00.30 is next
- Food Truck & Concession: RWP-00.40 merged; RWP-00.41 is next
- Hospitality: RWP-00.51 merged; RWP-00.52 is next
- Entertainment & Attractions: RWP-00.63 merged; RWP-00.64 is next

## Café, Bakery & Dessert Terminology Result

The canonical terminology model is documented at `track0/industries/cafe-bakery-dessert.md`.

### Neutral terms

Mixed-organization and cross-industry surfaces use:

- organization;
- venue;
- content;
- item;
- category;
- option;
- availability;
- service period;
- screen;
- publish;
- restore.

### Venue-scoped terms

Subtype and content context may select drink menu, coffee menu, tea menu, bakery case, today's selection, dessert menu, current flavors, juice and smoothie menu, size, temperature, milk choice, base, flavor, add-in, topping, batch, next batch, freshness guidance, limited, sold out, preorder, custom order, pickup, seasonal item, or guest-recognizable service-period names.

### Important distinctions

- **Item** is the neutral operator object; guest copy uses the product name or a known product noun.
- **Category** is the neutral grouping term; section, collection, case group, menu group, or flavor group is used only when clearer.
- **Size** is the canonical quantity or format choice. Portion, serving, scoop, slice, cup, cone, and pack remain contextual formats.
- **Option** is the neutral guest-facing choice. Modifier is operator or integration language, not the default guest label.
- **Temperature**, **milk or base choice**, **flavor**, **add-in**, and **topping** are distinct option types.
- **Batch** is a produced group that becomes available together. Next-batch timing is shown only when known.
- **Freshness guidance** must be venue-authored or source-authoritative. Vennusign must not infer freshness, shelf life, safety, or production time.
- **Available**, **unavailable**, **sold out**, and **limited** are distinct product states or presentation values.
- **Preorder** means a request before pickup or fulfillment; **custom order** means guest-specific configuration or production. Neither authorizes ordering, payment, production, or fulfillment implementation.
- **Pickup** is the neutral guest collection term. Use collection only where the venue's established language requires it, and do not alternate both terms in one flow.
- **Service period** is operator language; guest copy uses recognizable names such as morning, breakfast, lunch, afternoon, evening, late night, or pickup hours.

### Availability and timing language

- Available: currently offered through the represented service context.
- Unavailable: cannot currently be offered and the more specific reason is unknown, not communicated, or not depletion.
- Sold out: the current sellable quantity or current batch is exhausted.
- Limited: quantity or duration is constrained; it does not imply a known remaining count.
- Next batch or available again: use only when the return is known and authoritative.
- Preorder available or closed: describes the preorder window only.
- Pickup available or paused: describes the represented pickup context only.

Unknown state remains unknown. Guest copy must not promise exact quantity, freshness, production time, return timing, pickup readiness, or preorder acceptance without authoritative data.

### Subtype preferences

- Café: menu, drinks, food, daily or seasonal specials, service periods, pickup, venue information.
- Coffee Shop: coffee menu, espresso drinks, brewed coffee, hot or iced, size, milk choice, extra shot, flavor or syrup, seasonal drinks, pastries.
- Tea Shop: tea menu, tea base or style, hot or iced, size, sweetness, ice, toppings, add-ins, seasonal drinks.
- Bakery: today's selection, bakery case or display case, bread, pastry, baked goods, batch, next batch, sold out, preorder, pickup.
- Patisserie: pastry or cake collection, flavor, size or servings where known, custom order, preorder, pickup, seasonal collection, limited availability.
- Bakery-Café: bakery case, beverage menu, breakfast or lunch where used, service period, daily specials, pickup, available and sold-out state.
- Dessert Shop: dessert menu, portion or format, flavor, toppings, add-ins, combinations, made to order, seasonal or limited item.
- Frozen Dessert Shop: current flavors, scoop or serving format, size, cup or cone, toppings, take-home, limited or rotating flavor.
- Juice & Smoothie Bar: juice and smoothie menu, size, base, ingredients, add-ins, boosts where locally used, bowls, seasonal produce, pickup.
- Neutral subtype: menu, item, category, size, options, availability, special, preorder where supported, pickup, service period.

## Classification Result

- Industry, subtype, hybrid traits, and terminology preference are product/domain state.
- Terminology changes defaults, labels, starter recommendations, help text, analytics presentation, and guest wording only.
- Terminology does not grant capabilities, change plan access, alter permissions, increase limits, control rollout, or change commercial access.
- Batch, freshness, limited-quantity, expected-return, availability, preorder-window, pickup-context, service-period, size, and option values retain product/domain-state treatment where represented.
- Customer-authored names and custom labels must be preserved through future profile or subtype changes.
- Manual item editing, manual availability changes, publishing, delivery confirmation, offline awareness, and restoration remain core.
- Ordering, payment, production management, fulfillment, inventory, POS, pickup-source, and related synchronization remain later capability and integration-packaging decisions.

## Impeccable Planning Result

The project-local Impeccable skill and `clarify` guidance were consulted for future onboarding, navigation, forms, editor labels, state messages, help text, analytics, and guest-facing copy.

Future UI copy must:

- keep one noun and verb for the same concept throughout a flow;
- use specific verb-object actions;
- use persistent labels rather than placeholders as labels;
- distinguish first use, no results, filters, permissions, failures, empty content, sold-out state, unavailable state, and unknown timing;
- explain what failed and how to recover;
- avoid unsupported promises about freshness, quantity, production time, return time, pickup readiness, or preorder acceptance;
- use complete translatable messages;
- align visible labels and accessible names;
- support long product, category, option, venue, and collection names, localization expansion, pluralization, dynamic values, keyboard access, assistive technology, and 200% zoom;
- avoid color-only meaning and unnecessary abbreviations;
- preserve customer-authored terminology;
- preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, localization, analytics, ordering, payment, production, fulfillment, inventory, pickup automation, or product implementation was authorized or performed.

## Exact Next Café, Bakery & Dessert Action

After RWP-00.29 is merged, verified on `master`, issue #504 is closed, and the claim is released, execute **RWP-00.30 — Café, Bakery & Dessert Operating Characteristics** (#505).

RWP-00.30 must:

- document early hours and business-day boundaries;
- define service periods and rotating daily-product behavior;
- document batch production, freshness windows, sell-outs, and expected returns;
- document preorder, custom-order, pickup, and collection operating considerations without implementing ordering or fulfillment;
- document seasonal demand and temporary availability;
- distinguish counter, table, mixed, and pickup-led service patterns;
- distinguish subtype-specific operating patterns;
- tie each difference to defaults, terminology, content, screen purposes, or capability classification;
- avoid jurisdiction-specific invention;
- update the Track 0 capability documentation;
- remain documentation-only and hand off to RWP-00.31.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, ordering, payments, production, fulfillment, inventory, pickup automation, analytics, localization, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
