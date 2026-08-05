# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.17 complete in this proposed merge state; RWP-00.18 is next
- Café, Bakery & Dessert: RWP-00.28 merged; RWP-00.29 is next
- Food Truck & Concession: RWP-00.40 merged; RWP-00.41 is next
- Hospitality: RWP-00.51 merged; RWP-00.52 is next
- Entertainment & Attractions: RWP-00.63 merged; RWP-00.64 is next

## Bar, Brewery & Nightlife Terminology Result

The canonical terminology model is documented at `track0/industries/bar-brewery-nightlife.md`.

### Neutral terms

Mixed-organization and cross-industry surfaces use:

- organization;
- venue;
- content;
- item;
- category;
- screen;
- area;
- event;
- service period;
- special;
- availability;
- publish;
- restore.

### Venue-scoped terms

Subtype and operating context may select drink menu, tap list, cocktail list, wine list, current taps, pour size, flight, bottle, can, release, happy hour, game or match, viewing area, doors, entry information, cover, guest list, reservation, table, room, patio, venue zone, last call, sold out, unavailable, or available when the meaning is clear.

### Important distinctions

- **Item** is the neutral operator object; guest copy uses the product name or a known beverage noun.
- **Tap** is a serving source or list position, not the universal beverage noun.
- **Pour** is a serving option; **flight** is a grouped tasting selection.
- **Special** is the neutral offer noun; happy hour, game-day offer, release, tasting, or featured drink are contextual guest labels.
- **Event** is the neutral program noun; game, match, live music, DJ set, trivia, tasting, and release event are contextual labels.
- **Cover** means an admission charge only; use **entry information** when the access model is mixed or unknown.
- **Reservation** is the canonical held-booking term; use booking only when a local convention or integrated source requires it.
- **Service period** is operator language; guest copy uses recognizable names such as evening service, happy hour, late night, doors, or event hours.
- **Available**, **unavailable**, and **sold out** are distinct product states. Return timing is shown only when known.

### Subtype preferences

- Pub: drinks, food, house specials, recurring events, bar, dining area, patio.
- Sports Bar: games or matches, viewing areas or zones, game-day offers, drinks, food.
- Cocktail Bar: cocktails, signatures, classics, seasonal list, spirits, serves or measures.
- Wine Bar: wine list, by the glass, by the bottle, tasting pour, flight, producer, region, varietal, vintage.
- Brewery: house beers, releases, tap list, packaged beer, cans, bottles, take-home, tours.
- Brewpub: tap list, food menu, pairings, releases, kitchen availability, bar and dining areas.
- Taproom: current taps, pour sizes, flights, releases, guest food or food partner.
- Nightclub: event lineup, doors, entry information, cover, guest list, rooms or venue zones, bar menu.
- Lounge: curated drinks, cocktails, wine, reservations, tables, seating areas, events.
- Neutral subtype: drinks, menu, specials, events, reservations where supported, areas, service periods.

## Classification Result

- Industry, subtype, hybrid traits, and terminology preference are product/domain state.
- Terminology changes defaults, labels, starter recommendations, help text, analytics presentation, and guest wording only.
- Terminology does not grant capabilities, change plan access, alter permissions, increase limits, control rollout, or change commercial access.
- Customer-authored names and custom labels must be preserved through future profile or subtype changes.
- Availability, event, reservation, entry, service-period, and area values retain their own product-state classifications.
- Manual editing, availability changes, publishing, delivery confirmation, offline awareness, and restoration remain core.
- Automatic POS, inventory, tap-management, reservation, ticketing, event, or other synchronization remains a later integration-packaging question.

## Impeccable Planning Result

The project-local Impeccable skill and `clarify` guidance were consulted for future onboarding, navigation, forms, editor labels, state messages, help text, analytics, and guest-facing copy.

Future UI copy must:

- keep one noun and verb for the same concept throughout a flow;
- use specific verb-object actions;
- use persistent labels rather than placeholders as labels;
- distinguish first use, no results, filters, permissions, failures, and empty content;
- explain what failed and how to recover;
- use complete translatable messages;
- align visible labels and accessible names;
- support long names, localization expansion, pluralization, dynamic values, keyboard access, assistive technology, and 200% zoom;
- avoid color-only meaning and unnecessary abbreviations;
- preserve customer-authored terminology;
- preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, localization, analytics, or product implementation was authorized or performed.

## Exact Next Bar, Brewery & Nightlife Action

After RWP-00.17 is merged, verified on `master`, issue #492 is closed, and the claim is released, execute **RWP-00.18 — Bar, Brewery & Nightlife Operating Characteristics** (#493).

RWP-00.18 must:

- define late-night hours and business-day boundaries;
- define service periods, happy hour, rotating taps, limited releases, last call, and temporary availability;
- define table, bar, counter, and hybrid service models;
- identify age-restriction and responsible-display considerations without inventing jurisdiction-specific rules;
- define live music, DJ, trivia, sports, entertainment, and event operations;
- define reservations, guest lists, cover, ticketing, and private-event considerations;
- define inventory volatility and rapid-update needs;
- distinguish subtype-specific operating patterns;
- update the Track 0 capability documentation;
- remain documentation-only and hand off to RWP-00.19.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, ordering, payments, inventory, reservations, ticketing, event management, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
