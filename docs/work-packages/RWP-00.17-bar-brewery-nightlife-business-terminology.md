# RWP-00.17 — Bar, Brewery & Nightlife Business Terminology

## Status

Complete in this proposed merge state.

## Issue

- #492

## Objective

Define the canonical terminology and language model for Bar, Brewery & Nightlife so onboarding, navigation, content editing, help text, analytics, starter content, and guest-facing screens can use consistent industry-appropriate wording without coupling terminology to entitlements, permissions, or implementation.

## Dependency verified

- RWP-00.16 is complete and merged.
- Restaurant remains the canonical inherited baseline.
- The merged Bar, Brewery & Nightlife industry and subtype model is authoritative.
- No competing branch, pull request, or active claim owned RWP-00.17 when claimed.

## Delivered

- Added a canonical operator-facing and guest-facing terminology glossary to `track0/industries/bar-brewery-nightlife.md`.
- Defined inherited Restaurant terms that remain unchanged.
- Defined bounded terminology for drinks, menus, tap lists, pours, flights, bottles, cans, cocktails, specials, events, cover, reservations, tables, sections, and service periods.
- Defined subtype-specific preferred terms and neutral organization-wide fallbacks.
- Defined hybrid-venue and mixed-organization fallback behavior.
- Distinguished object names, action verbs, display labels, state labels, and analytics labels.
- Defined language rules for ambiguous concepts such as menu, item, tap, pour, flight, cover, reservation, section, service period, sold out, unavailable, doors, and last call.
- Applied the project-local Impeccable `clarify` guidance to UI-facing terminology planning.
- Updated the Track 0 capability matrix, project status, tracker, and current handoff.

## Canonical language decisions

1. **Content** is the neutral umbrella term across mixed organizations; **menu**, **drink list**, **tap list**, **wine list**, and **event lineup** are context-specific presentations of content.
2. **Item** remains the neutral operator-facing object; guest-facing surfaces use the actual product name or a subtype-appropriate noun such as drink, cocktail, wine, beer, bottle, can, pour, or flight.
3. **Available**, **unavailable**, and **sold out** describe product state. They are not feature flags or commercial access labels.
4. **Tap** identifies a serving source or list position only when that distinction is useful. It must not be used as the universal noun for a beverage item.
5. **Pour** describes a serving option or measure; **flight** describes a grouped tasting selection. Neither is a separate entitlement or product package.
6. **Special** is the neutral operator term for a time-bound promoted offer. Guest copy may use happy hour, featured drink, game-day offer, release, tasting, or house special when context supports it.
7. **Event** is the neutral program term. Subtype copy may use game, match, live music, DJ set, trivia, tasting, release, or private event.
8. **Cover** means an admission charge only. Use **entry information** when the charge, ticket, guest-list, age, or access model is mixed or not yet known.
9. **Reservation** means a held booking for a table, area, or experience. Use **booking** only where an external or local business convention requires it; do not alternate the two terms in one flow.
10. **Section** means a named operational or guest area. Use **viewing area**, **room**, **bar**, **patio**, **table area**, or **venue zone** when the physical meaning is known.
11. **Service period** is the neutral operator term for a bounded operating interval. Guest-facing copy should use recognizable names such as lunch, evening service, happy hour, late night, doors, or event hours.
12. **Venue** is the neutral local business unit across the industry. Use **property**, **outlet**, **stand**, or another industry term only when the parent profile requires it.

## Subtype terminology result

- **Pub:** drinks, food, house specials, recurring events, bar, dining area, patio.
- **Sports Bar:** games or matches, viewing areas or zones, game-day offers, drinks, food, event schedule.
- **Cocktail Bar:** cocktails, signature cocktails, classics, seasonal list, spirits, serves or measures, bar seating.
- **Wine Bar:** wine list, by the glass, by the bottle, tasting pour, flight, producer, region, varietal, vintage.
- **Brewery:** house beers, releases, tap list, packaged beer, cans, bottles, take-home, tours, release events.
- **Brewpub:** tap list, food menu, pairings, releases, kitchen availability, bar and dining areas.
- **Taproom:** current taps, pour sizes, flights, releases, guest food or food partner, taproom.
- **Nightclub:** event lineup, doors, entry information, cover, guest list, rooms or venue zones, bar menu.
- **Lounge:** curated drinks, cocktails, wine, reservations, tables, seating areas, events.
- **Unspecified / General Bar:** drinks, menu, specials, events, reservations where supported, areas, service periods.

## Impeccable planning result

The project-local Impeccable skill and `clarify` guidance were consulted because terminology will appear in future onboarding, navigation, forms, editor labels, state messages, help text, analytics, and guest-facing screens.

The specification requires future UI copy to:

- keep one noun and one verb for the same concept throughout a flow;
- use specific verb-object actions such as `Add drink`, `Mark unavailable`, `Publish tap list`, and `Restore previous version`;
- use persistent labels and examples rather than placeholders as labels;
- distinguish first use, no results, filters, permissions, failure, and empty content;
- explain what failed and how to recover without exposing internal codes as the primary message;
- use complete translatable messages rather than concatenated fragments;
- keep visible labels and accessible names aligned;
- support long product names, venue names, event names, localization expansion, pluralization, and 200% zoom;
- avoid color-only meaning and unnecessary abbreviations;
- preserve customer-authored terminology rather than silently renaming existing content after a subtype change;
- preserve the approved Sky Blue administrative direction.

This is planning only. No UI strings, components, routes, schema, API, analytics implementation, or localization resources were changed.

## Classification decisions

1. Industry, subtype, hybrid traits, and terminology preference are **product/domain state**.
2. A terminology preference affects defaults, labels, starter recommendations, help text, and presentation only.
3. Terminology does not grant capabilities, alter permissions, increase limits, control rollout, or change commercial access.
4. Availability, event, reservation, entry, service-period, and area values retain their own product-state classifications.
5. Manual editing, availability changes, publishing, delivery confirmation, and recovery remain inherited core capabilities.
6. Automatic POS, inventory, tap-management, reservations, ticketing, event, or other synchronization remains a later integration-packaging decision.
7. Customer-authored names and custom labels must be preserved through future profile or subtype changes.

## Validation

Documentation-only review confirmed:

- every issue-listed term is defined or assigned a bounded fallback;
- Restaurant inheritance is explicit and not restated as new capability access;
- operator-facing and guest-facing language are distinguished;
- subtype overrides do not create hidden packages or permissions;
- mixed organizations and hybrid venues have neutral fallback language;
- ambiguous terms have context rules;
- Impeccable language, accessibility, localization, error, state, and recovery guidance is recorded;
- no product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, analytics, or integration implementation was introduced;
- integration and external-system tests were not applicable and remain skipped under the standing owner instruction.

## Handoff

The next sequential item is **RWP-00.18 — Bar, Brewery & Nightlife Operating Characteristics** (#493).

RWP-00.18 must define late-night hours and business-day behavior, service periods, happy hour, rotating taps, limited releases, last call, service models, age and responsible-display considerations, entertainment and event operations, reservations, guest lists, cover and ticketing considerations, inventory volatility, and subtype-specific operating differences. It must remain documentation-only and must not begin until RWP-00.17 is merged, verified on `master`, issue #492 is closed, and the claim is released.
