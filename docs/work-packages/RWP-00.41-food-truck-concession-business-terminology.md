# RWP-00.41 — Food Truck & Concession Business Terminology

## Status

Complete in this proposed merge state.

## Issue

- #516

## Objective

Define the canonical terminology and language model for Food Truck & Concession so onboarding, navigation, content editing, Quick Update, location and event communication, help text, analytics, starter content, and guest-facing screens can use consistent industry-appropriate wording without coupling terminology to entitlements, permissions, ordering, routing, event management, host authority, or implementation.

## Dependency verified

- RWP-00.40 is complete and merged.
- Restaurant remains the canonical inherited baseline.
- The merged Food Truck & Concession industry and subtype model is authoritative.
- No competing branch, pull request, or active claim owned RWP-00.41 when claimed.

## Delivered

- Added a canonical operator-facing and guest-facing terminology glossary to `track0/industries/food-truck-concession.md`.
- Defined inherited Restaurant terms that remain unchanged.
- Defined bounded terminology for operations, units, service points, locations, stops, pitches, host locations, events, service windows, menus, combos, specials, availability, sell-outs, pickup, collection, queues, lanes, last orders, relocation, cancellation, and service periods.
- Defined subtype-specific preferred terms and neutral organization-wide fallbacks.
- Defined mixed-organization, host-venue, and hybrid-operation fallback behavior.
- Distinguished object names, action verbs, guest display labels, state labels, and analytics labels.
- Defined language rules for ambiguous concepts such as venue, operation, unit, stand, stall, kiosk, window, stop, pitch, location, host, event, menu, combo, special, pickup, collection, queue, lane, service window, and service period.
- Applied the project-local Impeccable `clarify` guidance to UI-facing terminology planning.
- Updated the Track 0 capability matrix, project status, tracker, and current handoff.

## Canonical language decisions

1. **Operation** is the neutral operator-facing noun for one locally managed Food Truck & Concession business context. Use truck, trailer, cart, kiosk, stand, stall, pop-up, station, or concession only when the subtype or actual service point is known.
2. **Venue** remains the neutral cross-industry local business unit. Food Truck & Concession surfaces may use **operation** when mobility, temporary placement, or a concession service point is material. Neither term decides entitlement or quantity-limit counting.
3. **Unit** is a neutral physical or operational instance when the exact form is unknown or mixed. It may refer to a truck, trailer, cart, kiosk, stand, stall, or temporary service point, but it must not silently imply one entitlement-counting venue.
4. **Service point** is a guest-facing place where service occurs. Use it when one operation has one or more counters, stations, windows, stands, or collection points and no more specific guest term is reliable.
5. **Stand** is the preferred term for a bounded concession or event service point. **Stall** is preferred in market contexts. **Kiosk** is used only for the approved Kiosk subtype or a real kiosk. Do not alternate stand, stall, booth, counter, and kiosk for literary variety.
6. **Service window** is a physical guest-facing ordering, payment, pickup, or service opening. **Service period** is a bounded time interval. **Operating window** is acceptable only when the time meaning is explicit. Do not use window without enough context to distinguish place from time.
7. **Current location** is the neutral guest-facing place where an operation is serving now. **Stop** is preferred for a mobile route or scheduled visit. **Pitch** is preferred when an assigned vendor position or setup site is the established local term. **Host location** identifies the larger property or venue containing the operation.
8. **Event** is a bounded program or occasion that shapes the operation. Festival, fair, market, game, match, concert, convention, private event, or catered event may replace it when the specific event type is known.
9. **Host** is the organization or venue providing the larger location or event context. Host is descriptive product/domain context and does not imply ownership, permissions, sponsor authority, or commercial access.
10. **Menu** remains the inherited guest-facing food and drink offering. **Compact menu**, **event menu**, **stand menu**, **station menu**, **today's menu**, or **limited menu** may be used when they clarify scope. Content remains the neutral cross-industry umbrella term.
11. **Combo** is a named grouping of items sold or presented together. Use meal, bundle, package, or deal only when that is the venue's established customer language. A combo label must not imply ordering, discount calculation, inventory linkage, or POS synchronization.
12. **Special** is the neutral operator term for a promoted item or offer. Guest copy may use event special, game-day offer, market special, featured combo, seasonal item, or limited offer when the source supports it.
13. **Available**, **unavailable**, **sold out**, **limited**, **service paused**, **closed**, and **canceled** are distinct states. They are not feature flags, permission labels, or interchangeable synonyms.
14. **Pickup** is the neutral guest term for collecting an order or item. **Collection** may be used where the venue or region consistently uses it. Do not alternate pickup and collection within one flow.
15. **Queue** is the neutral waiting-line concept. **Lane** describes a physically or operationally distinct path such as order, express, pickup, or collection. **Wait time** appears only when authoritative; the system must not infer it from queue wording.
16. **Last orders** or **orders close at** communicates the final accepted-order time when known. **Service ends at** communicates the end of service. Do not use last call as a universal Food Truck & Concession term.
17. **Relocating** means the operation is moving or preparing to move and is not currently serving at the represented location. **Moved to** or **now serving at** may be used only when the destination is known and authoritative.
18. **Service period** is the neutral operator term for a bounded operating interval. Guest-facing copy uses recognizable names such as breakfast, lunch, dinner, event service, market hours, first half, intermission, late service, or pickup hours when the operation actually uses them.

## Location, event, and service language

- **Current location:** the authoritative place where the operation is serving now.
- **Next stop:** a future scheduled stop that is known and approved for display.
- **Host location:** the containing property, venue, campus, arena, market, festival, or event site.
- **Stand / stall / station:** the locally recognized service-point identity within a host context.
- **Service window:** the physical place where guests order, pay, receive, or collect.
- **Service period:** the represented time interval during which a menu, service point, or operation is expected to serve.
- **Open:** the represented operation or service point is currently serving.
- **Service paused:** service is temporarily stopped and reopening may or may not be known.
- **Closed:** the represented operation or service point is not serving for the remainder of the applicable period unless a later authoritative update says otherwise.
- **Canceled:** a planned stop, event appearance, or service period will not occur.
- **Relocating:** the operation is moving or preparing to move and is not serving at the represented location.
- **Reopened / serving again:** service has resumed after a pause or closure when that fact is authoritative.

Unknown location, timing, queue, availability, and reopening information must remain unknown. Guest copy must not promise a destination, arrival time, service start, remaining quantity, queue length, wait time, pickup readiness, or reopening time without authoritative data.

## Availability and sell-out language

- **Available:** the item, combo, menu, service point, or operation can currently be offered in the represented context.
- **Unavailable:** it cannot currently be offered and the more specific reason is unknown, not communicated, or not depletion.
- **Sold out:** the currently sellable quantity is exhausted for the represented item, combo, menu, service point, or period.
- **Limited:** quantity or duration is constrained; it does not imply a known remaining count.
- **Service paused:** the operation or service point is temporarily not serving; this is broader than an item sell-out.
- **Last orders:** the final order cutoff is known and approaching or has been reached.
- **Available again / service resumes:** use only when the return or reopening is known and authoritative.

A whole operation must not be labeled sold out when only one item or combo is exhausted. An item must not be labeled closed when the intended state is sold out or unavailable.

## Subtype terminology result

- **Food Truck:** truck or operation, current location, stop, next stop where represented, service window, compact menu, combos, pickup, queue, sold out, relocating, now serving at.
- **Food Trailer:** trailer or operation, current pitch, setup location, service side or service window, menu, pickup, queue, weather notice, closing, relocating.
- **Food Cart:** cart or operation, current location, short menu, size or option, queue, pickup, sold out, open, closed.
- **Kiosk:** kiosk or service point, host location, area or landmark, counter or service window where accurate, menu, pickup, queue, hours, host notice.
- **Stadium / Arena Concession:** concession stand, stand, section, gate, concourse, event, game or match, event menu, combo, express lane, pickup lane, period-based service, sold out, closed.
- **Festival Vendor:** vendor or operation, festival or event, zone, pitch, booth or stand where accurate, event menu, service period, queue, weather delay, canceled, closing.
- **Market Stall:** market stall, stall, market, market day, aisle, row or zone, today's menu or selection, limited, sold out, pickup, closing.
- **Pop-Up:** pop-up, temporary location, host or collaborator, dates, limited menu, special hours, launch, final day, sold out, closed, moved.
- **Catering Concession:** service point, station, counter, buffet station where accurate, private or catered event, service period, menu or offerings, dietary guidance, pickup or collection, queue.
- **Unspecified / General Mobile or Concession Operation:** operation, venue, service point, current location, event, menu, item, combo, availability, pickup, queue, service period, publish, restore.

## Operator actions and state labels

Future operator-facing language should prefer explicit verb-object actions such as:

- `Set current location`;
- `Add next stop`;
- `Change event`;
- `Set service period`;
- `Add service window`;
- `Mark item sold out`;
- `Mark combo sold out`;
- `Mark available`;
- `Pause service`;
- `Resume service`;
- `Mark closed`;
- `Cancel stop` or `Cancel service period`;
- `Set new location`;
- `Add pickup instructions`;
- `Add queue guidance`;
- `Publish menu` or the known subtype-specific content name;
- `Restore previous version`.

Use the affected scope in state-changing actions. Do not label an action `Update`, `Manage`, `Move`, `Close`, `Submit`, or `Save changes` when a more specific outcome is known.

## Mixed-organization, host, and hybrid fallback

- Organization-wide and cross-industry surfaces use organization, venue, operation, content, item, category, availability, location, event, service point, service period, screen, publish, and restore.
- Operation-scoped surfaces may use truck, trailer, cart, kiosk, stand, stall, pop-up, station, or concession when the subtype and real context are known.
- Host-scoped labels must distinguish the operator's service point from the containing property or event.
- A host relationship must not silently rename the operator's customer-authored service point, menu, location, or event.
- Hybrid traits may influence suggestions but must not silently transform existing terminology.
- Copying content between unlike subtypes preserves source names and presents destination terminology as reviewable suggestions only.
- Local custom labels remain authoritative until an authorized user changes them.
- Neutral labels do not settle ownership, permission, sponsor, approval, commercial-access, or quantity-limit questions.

## Analytics terminology

Core operational views use neutral dimensions such as venue, operation, subtype, service point, current location, host location, event, service period, content type, item, combo, availability state, operating state, screen, and publish state. Subtype-specific drill-downs may use stop, pitch, stand, stall, section, gate, concourse, zone, aisle, station, service window, pickup lane, or queue context when the data actually exists. Analytics labels must not imply route accuracy, live inventory, remaining quantity, queue length, wait time, order status, ticket status, host authority, or paid access that the source data does not support.

## Impeccable planning result

The project-local Impeccable skill and `clarify` guidance were consulted because terminology will appear in future onboarding, navigation, forms, Quick Update, location and event controls, state messages, help text, analytics, and guest-facing screens.

The specification requires future UI copy to:

- keep one noun and one verb for the same concept throughout a flow;
- use specific verb-object actions and name the affected item, combo, service point, stop, location, event, or service period;
- use persistent labels and examples rather than placeholders as labels;
- distinguish first use, no results, filters, permissions, failure, empty content, sold-out state, unavailable state, paused service, closure, cancellation, relocation, and unknown timing;
- explain what failed and how to recover without exposing internal codes as the primary message;
- avoid promising a destination, arrival time, remaining quantity, queue length, wait time, pickup readiness, reopening time, or external synchronization without authoritative data;
- use complete translatable messages rather than concatenated fragments;
- keep visible labels and accessible names aligned;
- support long organization, host, event, location, service-point, item, combo, and menu names, localization expansion, pluralization, dynamic values, keyboard access, assistive technology, and 200% zoom;
- avoid color-only meaning and unnecessary abbreviations;
- preserve customer-authored terminology through subtype, host, event, or location changes;
- preserve the approved Sky Blue administrative direction.

This is planning only. No UI strings, components, routes, schema, API, analytics implementation, routing, ordering, pickup automation, event management, or localization resources were changed.

## Classification decisions

1. Industry, subtype, hybrid traits, and terminology preference are **product/domain state**.
2. Operation, unit, service-point, current-location, stop, pitch, host-location, event, service-window, service-period, operating-state, availability, queue-context, pickup-context, combo, and last-order values retain **product/domain state** treatment where represented.
3. Terminology changes defaults, labels, starter recommendations, help text, analytics presentation, and guest wording only.
4. Terminology does not grant capabilities, alter permissions, transfer host or operator authority, increase limits, control rollout, or change commercial access.
5. Manual menu and availability editing, operating-location and event communication, closure and relocation communication, screen targeting, publishing, delivery confirmation, offline awareness, and recovery remain inherited core capabilities.
6. Automatic POS, ordering, payment, inventory, route, queue, event, host-venue, location, catering, pickup-source, or related synchronization remains a later capability and integration-packaging decision.
7. Customer-authored names and custom labels must be preserved through future profile, subtype, host, event, or location changes.
8. Counts of venues, operations, units, stands, stalls, service points, windows, screens, events, integrations, or transactions remain usage or quantity limits, not terminology or capability grants.

## Validation

Documentation-only review confirmed:

- every issue-listed terminology area is defined or assigned a bounded fallback;
- Restaurant inheritance is explicit and not restated as new capability access;
- operator-facing and guest-facing language are distinguished;
- subtype overrides do not create hidden packages, permissions, or ownership transfers;
- mixed organizations, host venues, and hybrid operations have neutral fallback language;
- ambiguous physical-place and time-window terms have context rules;
- Impeccable language, accessibility, localization, error, state, and recovery guidance is recorded;
- no product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, routing, ordering, payment, inventory, event-management, analytics, localization, or integration implementation was introduced;
- integration and external-system tests were not applicable and remain skipped under the standing owner instruction.

## Handoff

The next sequential item is **RWP-00.42 — Food Truck & Concession Operating Characteristics** (#517).

RWP-00.42 must document operating-day and service-period behavior, setup and teardown, routes and stops, event and host schedules, relocation, weather and cancellation, rapid sell-outs, queue surges, last orders, pickup patterns, intermittent connectivity, multi-window or multi-stand operation, and subtype-specific operating differences. It must tie each difference to defaults, terminology, content, screen purposes, or capability classification; remain documentation-only; avoid jurisdiction-specific invention; and must not begin until RWP-00.41 is merged, verified on `master`, issue #516 is closed, and the claim is released.
