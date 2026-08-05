# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.17 merged; RWP-00.18 is next
- Café, Bakery & Dessert: RWP-00.29 merged; RWP-00.30 is next
- Food Truck & Concession: RWP-00.41 complete in this proposed merge state; RWP-00.42 is next
- Hospitality: RWP-00.51 merged; RWP-00.52 is next
- Entertainment & Attractions: RWP-00.63 merged; RWP-00.64 is next

## Food Truck & Concession Terminology Result

The canonical terminology model is documented at `track0/industries/food-truck-concession.md`.

### Neutral terms

Mixed-organization and cross-industry surfaces use:

- organization;
- venue;
- operation;
- content;
- item;
- category;
- availability;
- location;
- event;
- service point;
- service period;
- screen;
- publish;
- restore.

### Operation-scoped terms

Subtype and operating context may select truck, trailer, cart, kiosk, concession stand, stand, stall, pop-up, station, current location, stop, next stop, pitch, host location, event, service window, compact menu, event menu, combo, special, pickup, collection, queue, lane, last orders, service paused, canceled, relocating, or serving again.

### Important distinctions

- **Operation** is the neutral Food Truck & Concession local context; **venue** remains the neutral cross-industry local business unit.
- **Unit** is a neutral physical or operational instance and does not settle entitlement or quantity-limit counting.
- **Service point** is a guest-facing place where service occurs; stand, stall, kiosk, station, counter, or window is used only when accurate.
- **Service window** is a physical service opening; **service period** is a bounded time interval.
- **Current location** is where the operation is serving now; **stop** is a mobile visit; **pitch** is an assigned vendor position; **host location** is the containing property or event site.
- **Host** describes context and does not imply ownership, permissions, sponsor authority, or commercial access.
- **Combo** is a named grouping of items and does not imply ordering, pricing, inventory, or POS integration.
- **Pickup** is the neutral guest collection term; collection may be used consistently where established.
- **Queue** is the neutral waiting-line concept; **lane** is a distinct order, express, pickup, or collection path.
- **Last orders** communicates an authoritative order cutoff; **service ends at** communicates the end of service.
- **Available**, **unavailable**, **sold out**, **limited**, **open**, **service paused**, **closed**, **canceled**, **relocating**, and **serving again** are distinct product or operating states.

Unknown location, destination, timing, quantity, queue, pickup, and reopening information remains unknown. Guest copy must not promise a destination, arrival time, remaining quantity, queue length, wait time, pickup readiness, or reopening time without authoritative data.

### Subtype preferences

- Food Truck: truck or operation, current location, stop, next stop, service window, compact menu, combos, pickup, queue, sold out, relocating, now serving at.
- Food Trailer: trailer or operation, current pitch, setup location, service side or service window, menu, pickup, queue, weather notice, closing, relocating.
- Food Cart: cart or operation, current location, short menu, size or option, queue, pickup, sold out, open, closed.
- Kiosk: kiosk or service point, host location, area or landmark, counter or service window where accurate, menu, pickup, queue, hours, host notice.
- Stadium / Arena Concession: concession stand, stand, section, gate, concourse, event, game or match, event menu, combo, express lane, pickup lane, period-based service, sold out, closed.
- Festival Vendor: vendor or operation, festival or event, zone, pitch, booth or stand where accurate, event menu, service period, queue, weather delay, canceled, closing.
- Market Stall: market stall, stall, market, market day, aisle, row or zone, today's menu or selection, limited, sold out, pickup, closing.
- Pop-Up: pop-up, temporary location, host or collaborator, dates, limited menu, special hours, launch, final day, sold out, closed, moved.
- Catering Concession: service point, station, counter, buffet station where accurate, private or catered event, service period, menu or offerings, dietary guidance, pickup or collection, queue.
- Neutral subtype: operation, venue, service point, current location, event, menu, item, combo, availability, pickup, queue, service period, publish, restore.

## Classification Result

- Industry, subtype, hybrid traits, and terminology preference are product/domain state.
- Terminology changes defaults, labels, starter recommendations, help text, analytics presentation, and guest wording only.
- Terminology does not grant capabilities, change plan access, alter permissions, transfer host or operator authority, increase limits, control rollout, or change commercial access.
- Operation, unit, service-point, current-location, stop, pitch, host-location, event, service-window, service-period, operating-state, availability, queue-context, pickup-context, combo, and last-order values retain product/domain-state treatment where represented.
- Customer-authored names and custom labels must be preserved through future profile, subtype, host, event, or location changes.
- Manual menu and availability editing, operating-location and event communication, closure and relocation communication, screen targeting, publishing, delivery confirmation, offline awareness, and restoration remain core.
- Routing, ordering, payments, inventory, queue measurement, event management, host-venue, location-source, catering, pickup-source, and related synchronization remain later capability and integration-packaging decisions.
- Counts of venues, operations, units, stands, stalls, service points, windows, screens, events, integrations, or transactions remain usage or quantity limits, not terminology or capability grants.

## Impeccable Planning Result

The project-local Impeccable skill and `clarify` guidance were consulted for future onboarding, navigation, forms, Quick Update, location and event controls, state messages, help text, analytics, and guest-facing copy.

Future UI copy must:

- keep one noun and verb for the same concept throughout a flow;
- use specific verb-object actions and name the affected item, combo, service point, stop, location, event, or service period;
- use persistent labels rather than placeholders as labels;
- distinguish first use, no results, filters, permissions, failures, empty content, sold-out state, unavailable state, paused service, closure, cancellation, relocation, and unknown timing;
- explain what failed and how to recover;
- avoid unsupported promises about destinations, arrival time, remaining quantity, queue length, wait time, pickup readiness, reopening, or external synchronization;
- use complete translatable messages;
- align visible labels and accessible names;
- support long organization, host, event, location, service-point, item, combo, and menu names, localization expansion, pluralization, dynamic values, keyboard access, assistive technology, and 200% zoom;
- avoid color-only meaning and unnecessary abbreviations;
- preserve customer-authored terminology;
- preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, localization, analytics, routing, ordering, payment, inventory, event-management, host-venue, catering, pickup automation, or product implementation was authorized or performed.

## Exact Next Food Truck & Concession Action

After RWP-00.41 is merged, verified on `master`, issue #516 is closed, and the claim is released, execute **RWP-00.42 — Food Truck & Concession Operating Characteristics** (#517).

RWP-00.42 must:

- document operating-day and service-period behavior;
- define setup and teardown patterns;
- document routes, stops, pitches, host locations, event schedules, and relocation behavior;
- document weather, delay, cancellation, reopening, and closure considerations;
- document rapid sell-outs, queue surges, last orders, pickup, and collection patterns;
- document intermittent connectivity and recovery considerations;
- distinguish single-window, multi-window, single-stand, and multi-stand operating patterns without deciding limit counting;
- distinguish subtype-specific operating patterns;
- tie each difference to defaults, terminology, content, screen purposes, or capability classification;
- avoid jurisdiction-specific invention;
- update the Track 0 capability documentation;
- remain documentation-only and hand off to RWP-00.43.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, routing, ordering, payments, inventory, event management, host-venue behavior, catering, pickup automation, analytics, localization, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
