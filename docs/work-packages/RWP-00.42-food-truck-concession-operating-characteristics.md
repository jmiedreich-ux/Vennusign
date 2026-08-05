# RWP-00.42 — Food Truck & Concession Operating Characteristics

## Status

Complete in this proposed merge state.

## Issue

- #517

## Objective

Define the operating characteristics that distinguish Food Truck & Concession from the inherited Restaurant baseline, including mobility, temporary locations, event calendars, intermittent connectivity, rapid setup and teardown, compact menus, queue surges, sell-outs, weather and location changes, shared concessions, and subtype differences. Tie each characteristic to product defaults, terminology, content and screen-purpose guidance, and Track 0 capability classification without implementing product behavior.

## Dependency verified

- RWP-00.41 is merged, verified, closed, and released.
- Restaurant remains the canonical baseline.
- The merged Food Truck & Concession industry, subtype, and terminology model is authoritative.
- The existing RWP-00.42 claim and branch were valid and had no competing Food Truck pull request.

## Operating model

Food Truck & Concession is not defined by one physical form. Its distinguishing operating model is the combination of one or more of the following:

- service may move between stops, pitches, events, hosts, or temporary locations;
- service may exist for a bounded event or market window rather than a permanent weekly schedule;
- setup readiness, teardown, utilities, network access, and screen delivery may be checked repeatedly;
- menus are often compact, capacity-constrained, and changed quickly during service;
- demand may arrive in short surges tied to gates, breaks, intermissions, periods, sessions, weather, or venue programming;
- items, combos, windows, stands, or whole operations may sell out or pause independently;
- operators may depend on cellular or temporary venue connectivity and still need safe manual operation;
- host, sponsor, promoter, caterer, property, and operator responsibilities may overlap without transferring authority automatically.

These characteristics tune defaults and presentation. They do not create entitlements, permissions, limits, or rollout flags.

## Operating-day and service-period lifecycle

The canonical lifecycle is descriptive product/domain state, not a workflow engine requirement:

1. **Planned:** a stop, event, market day, service period, or host engagement is known but not yet in setup.
2. **Traveling / not on site:** relevant for mobile units only when the operator deliberately represents it. It must not imply live vehicle tracking.
3. **Setup:** the unit or service point is preparing equipment, menu, location, screens, utilities, and guest guidance. Setup does not mean open.
4. **Ready:** local content, screen targets, and represented location or event have been checked. Ready does not guarantee every external system or physical dependency.
5. **Open / serving:** the represented operation or service point is serving.
6. **Service paused:** service has temporarily stopped. Reopening remains unknown unless an authoritative time is supplied.
7. **Limited / constrained:** service continues with a reduced menu, limited quantity, reduced window, altered queue, or other represented constraint.
8. **Relocating:** the operation is moving or preparing to move and is not serving at the represented location.
9. **Closed:** service has ended for the represented service period unless a later authoritative update reopens it.
10. **Canceled:** a planned stop, appearance, or service period will not occur.
11. **Teardown:** service has ended and the operation is being packed down or handed back. Guest-facing content should normally show closed, moved, or the next authoritative state rather than an internal teardown label.
12. **Recovered / serving again:** service has resumed after a pause, outage, relocation, or temporary closure when the operator confirms it.

A later implementation may support only a bounded subset initially. The lifecycle does not authorize schema, automation, routing, or live tracking.

## Mobility, routes, stops, and temporary locations

- Current location, stop, pitch, host location, event, zone, section, stand, stall, or station is product/domain state when represented.
- A route is a planned sequence or pattern, not proof of live location or arrival time.
- A next stop may be shown only when approved for guest display.
- Changing the current location must not silently change ownership, subtype, commercial access, screen authority, or the host relationship.
- Relocation must preserve menus, availability, screen pairing, targeting, history, and customer-authored names.
- A location change should prompt review of screens, host notices, event context, pickup guidance, queue guidance, dates, and service periods.
- Unknown location, arrival, departure, or reopening information remains unknown.

Defaults should favor current-location visibility for Food Truck, Food Trailer, Food Cart, Festival Vendor, Market Stall, and Pop-Up subtypes. Kiosk and Stadium / Arena Concession should favor host area, gate, section, concourse, level, or landmark. Catering Concession should favor the event and service-point identity.

## Event, market, host, and service calendars

The product may represent multiple calendar concepts without treating them as one capability:

- ordinary business hours;
- one or more service periods in a day;
- stops or route visits;
- market days;
- event appearances;
- host-venue schedules;
- event phases such as pre-open, gates open, intermission, halftime, post-event, or private service;
- temporary start and end dates for pop-ups;
- setup and teardown windows;
- last-order and service-end cutoffs.

Manual hours and bounded service-period communication remain core. Advanced recurring routing, event ingestion, host calendar synchronization, conflict detection, and cross-unit scheduling remain optional capability or integration candidates for later classification.

Calendar presentation must distinguish planned from current, canceled from closed, and service start from setup time. It must not imply that a host schedule, ticketed event, or external calendar has synchronized unless source authority and freshness are known.

## Rapid setup and teardown

Setup defaults should help an operator confirm:

- the intended operation, event, host, and current location;
- the active menu or content set;
- local availability and sell-out state;
- service period and last-order information;
- screen pairing, target assignment, online/offline state, and last delivered version;
- pickup, collection, queue, and wayfinding guidance;
- weather, closure, relocation, sponsor, or host notices;
- whether older content remains visible on any intended screen.

Teardown must preserve content, screen identity, publication history, and restore points. Removing equipment from a site must not automatically delete screens, revoke authority, close another service period, or erase a future schedule.

Setup and teardown checklists are presentation and workflow candidates. The underlying ability to pair, target, publish, confirm delivery, identify outdated screens, and restore content remains core.

## Compact menus, capacity, and sell-outs

- Compact menu is a presentation preference, not a separate capability.
- Item, combo, menu, service-point, and whole-operation availability must remain independently scoped.
- Sold out means sellable quantity is exhausted for the represented scope; unavailable is used when the reason is not depletion or is unknown.
- Limited indicates a constraint without claiming a known count.
- A whole operation must not be marked sold out because one item, combo, or window is exhausted.
- A service pause or closure must not silently mark every item unavailable or destroy prior item state.
- Manual rapid availability changes remain core even when POS, inventory, production, or order integrations are absent or disconnected.
- Expected return, next batch, remaining quantity, preparation time, and pickup readiness appear only when authoritative.

Defaults should prioritize Quick Update, recent changed items, high-impact combos, current menu scope, and a clear publish result. Advanced inventory-driven automation remains an add-on candidate and may not replace manual operation.

## Queue surges, pickup, and service windows

Queue and pickup information may be represented manually as content or product state. The profile does not assume live queue measurement or order tracking.

Core presentation may include:

- which service point or window is open;
- order, express, pickup, collection, accessible, or other locally defined lanes;
- temporary lane closure or rerouting;
- pickup or collection instructions;
- last orders and service-end information;
- simple queue guidance such as “order here” or “pickup at window 2”;
- a manual service-paused or limited-capacity notice.

Live queue length, wait-time prediction, order-ready status, digital ordering, payment, capacity optimization, and sensor or footfall data remain optional capabilities or external-data candidates. Unknown wait and pickup status must not be inferred.

## Weather, venue conditions, and cancellation

Weather, traffic, permits, utilities, host direction, venue access, safety conditions, stock, staffing, and equipment may affect operation. Track 0 does not create legal or safety policy.

Future surfaces should support clear manual communication of:

- delayed setup or opening;
- weather-affected service;
- changed location or service side;
- reduced menu or limited service;
- temporary pause;
- canceled stop, appearance, or service period;
- early closure;
- moved or now serving at an authoritative new location;
- reopening or service resumption.

A weather or host notice is product content/state. Automated weather, traffic, permit, venue, or safety feeds remain integration candidates. Any automation must preserve manual override, source identity, freshness, and safe fallback.

## Intermittent connectivity and delivery confidence

Food Truck & Concession depends heavily on inherited offline resilience:

- operators must see whether a screen is online, offline, outdated, or has an unknown delivery state;
- the last successfully delivered version and time should remain understandable;
- a failed publish must identify affected targets and recovery actions;
- retry must not duplicate content or silently retarget screens;
- previously delivered guest content should remain stable during temporary connectivity loss where the player supports it;
- manual local operation must remain possible without purchasing an integration;
- reconnecting must not overwrite newer approved content or hide conflicts;
- unknown source freshness must not be presented as current.

Advanced network monitoring, managed connectivity, cellular plans, remote support, and managed hardware may be tier or add-on candidates. Basic publish confirmation, outdated awareness, and restoration remain core.

## Shared concessions, host venues, and authority

A host may provide location, event, screen, sponsor, safety, or schedule context while an operator controls menu and availability. Authority must be explicit by object and scope.

- Host relationship is product/domain state.
- Permissions control who may edit or publish.
- Commercial access is an entitlement decision.
- Quantity allowances are limits.
- Source authority and approval state are separate from all four.
- Shared or rotating screens must identify the target and current content owner before publishing.
- Host-required content must not silently overwrite operator content.
- Operator changes must not remove mandatory host or safety content without permission.
- Copying content between stands or events must preserve source content and require local review of location, event, sponsor, service, and screen context.

Detailed approval workflows, sponsor management, event management, and host-system integration remain later capability candidates.

## Multi-unit, multi-stand, and multi-window behavior

One organization may operate several mobile units, stands, stalls, kiosks, or stations. The profile requires:

- clear organization, operation, service-point, event, host, location, and screen scope;
- local overrides without breaking shared brand or commercial access;
- bulk changes only with explicit selection and preview;
- mixed-state visibility when some targets are online, open, sold out, paused, closed, or outdated;
- protection against publishing one location or event’s content to another unintentionally;
- neutral organization-wide terminology when physical forms differ.

This RWP does not decide whether a unit, stand, service point, window, event, or host relationship consumes a commercial allowance. Those are quantity-limit decisions for later owner approval.

## Subtype operating differences

| Subtype | Dominant operating rhythm | Default emphasis |
| --- | --- | --- |
| Food Truck | travel, stop setup, serving, relocation, repeated stops | current location, stop, compact menu, service window, queue, connectivity, relocation |
| Food Trailer | transport or seasonal pitch, setup utilities, service, teardown | pitch, setup readiness, service side, weather, utilities-related notice, relocation |
| Food Cart | rapid setup, very compact service, immediate sell-outs | short menu, immediate location, availability, queue, open/closed |
| Kiosk | host-controlled stable or semi-stable service point | host area, landmark, hours, counter/window, pickup, host notices |
| Stadium / Arena Concession | event-day setup, gates, bursts, periods, intermission, close | event, stand/section, express menu, lanes, sell-outs, host/sponsor content |
| Festival Vendor | bounded event, setup, surge periods, weather, teardown | event, zone/pitch, dates, weather, queue, cancellation, limited menu |
| Market Stall | recurring market day, stall setup, limited selection, close | market schedule, stall/aisle, today’s menu, limited quantity, pickup |
| Pop-Up | temporary residency, launch, special hours, final day | host/collaborator, dates, location, limited menu, launch/final-day states |
| Catering Concession | contracted event setup, attendee service window, teardown | event/station, service period, dietary guidance, collection, host approval |
| Neutral | mixed or uncertain operating rhythm | operation, service point, location, event, menu, availability, screens |

Subtype changes defaults only. They do not unlock workflow, scheduling, integration, analytics, or commercial access.

## Capability-presentation implications

Future Operate surfaces should prioritize the operator’s current task rather than expose the whole capability catalog:

- **Before service:** setup readiness, location/event, menu, screen health, and publish confirmation.
- **During service:** Quick Update, sell-outs, service state, pickup/queue guidance, screen health, and recovery.
- **During disruption:** affected scope, authoritative state, safe guest message, publish result, and next recovery action.
- **At relocation or close:** location/state update, target review, publish confirmation, and preservation of future schedules and content.
- **Across units:** exceptions first, clear scope, mixed states, and safe bulk actions.

The project-local Impeccable `shape` and `harden` guidance were applied as planning principles: explicit task and scope, hierarchy, first-use/empty/error/success/permission/offline/outdated/conflict states, mobile and desktop ranges, long names, 200% zoom, keyboard and assistive-technology access, non-color-only status, confirmation for high-scope changes, actionable recovery, and preservation of the approved Sky Blue administrative direction.

No UI or product implementation is authorized by this planning result.

## Classification decisions

1. Current location, stop, pitch, host location, event, service period, service window, operating state, availability, queue context, pickup context, and represented source freshness are **product/domain state**.
2. Manual menu, availability, operating-state, location, event, queue/pickup guidance, targeting, publishing, confirmation, offline awareness, and restoration are **core capabilities**.
3. Permissions determine who may change or publish each scope; permissions do not grant commercial access.
4. Advanced route and event scheduling, cross-unit orchestration, approval workflows, live queue/order information, optimization, and advanced monitoring are **tier entitlement candidates** until later mapping.
5. POS, order, payment, inventory, production, route, event, host, weather, traffic, queue, pickup, and external-calendar synchronization are **independent add-on candidates** where integration is required.
6. Counts of operations, units, stands, service points, windows, screens, events, schedules, integrations, retained history, or transactions are **usage or quantity limits**.
7. Internal staged delivery is an **internal rollout flag** and must never be shown as customer availability.
8. Industry, subtype, physical form, operating rhythm, host relationship, product state, permission, entitlement, add-on, and limit remain separate concepts.

## Validation

Documentation-only review confirmed that:

- every issue-listed operating characteristic is covered;
- inherited Restaurant behavior remains inherited rather than duplicated as new access;
- manual daily operation remains core;
- mobile, event, host, weather, surge, sell-out, setup, teardown, and connectivity differences are tied to defaults and capability presentation;
- subtype differences tune recommendations only;
- authority, permission, entitlement, state, add-on, limit, and rollout distinctions remain explicit;
- no product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, analytics, routing, ordering, payment, inventory, event-management, host-venue, catering, pickup, or integration implementation was introduced;
- integration and external-system tests were not applicable and remain skipped under the standing owner instruction.

## Handoff

The next sequential item is **RWP-00.43 — Food Truck & Concession Required Capabilities** (#518).

RWP-00.43 must define the smallest viable core capability set for menu and availability management, current location and event communication, explicit screen targeting, publish confirmation, offline and outdated awareness, recovery, queue and service guidance, rapid updates, permissions, and required states. It must remain documentation-only and must not begin until this RWP is merged, verified on `master`, issue #517 is closed, and the claim is released.
