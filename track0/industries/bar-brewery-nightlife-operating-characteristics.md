# Bar, Brewery & Nightlife Operating Characteristics

## Authority and scope

This document is the canonical operating-characteristics companion to `track0/industries/bar-brewery-nightlife.md` for RWP-00.18. It records only meaningful operating differences from the Restaurant baseline and does not implement UI, workflow, billing, entitlement, legal, inventory, event, reservation, ticketing, or integration behavior.

## Canonical operating model

Bar, Brewery & Nightlife venues operate around fast-changing beverage availability, service periods that may cross midnight, multiple service models, temporary offers, entertainment, admissions conditions, and rapid public communication.

The model separates venue hours, service periods, kitchen hours, door time, event time, last entry, and last call; beverage item, serving format, tap position, release, special, and availability state; table, bar, counter, standing, lounge, patio, viewing-area, and hybrid service contexts; public information, staff-only context, restricted access, and locally approved responsible-content wording; event schedule, event state, entry method, reservation, guest list, cover, ticket, and private-event state; and manual, scheduled, imported, overridden, stale, unknown, delivered, outdated, and restored information.

These values are product/domain state. They do not themselves grant permission, entitlement, or legal authority.

## Operating hours and cross-midnight service

A business day may continue past midnight. Future planning must not assume that the date changes when the service period changes or that every area follows the same hours.

Required distinctions include ordinary venue hours; beverage-service and kitchen-service hours; happy-hour or offer periods; doors, event start, set time, last entry, and event end; locally defined last call; temporary extension, early close, delayed opening, private-event closure, and after-hours state; and the next confirmed service period without promising unconfirmed reopening time.

Manual communication remains core. Advanced recurring schedules, conflict detection, event-linked timing, and automated source synchronization remain later tier or add-on candidates.

## Availability, taps, releases, and temporary offers

High-frequency change is normal. A tap may change, a keg may empty, a limited release may sell out, a cocktail ingredient may become unavailable, or a special may end while the venue remains open.

Core operation requires operators to mark an item available, unavailable, or sold out; distinguish the beverage item from its tap, serving format, package, or pour size; communicate a replacement only when authoritative; show the effective period of happy hour, featured offers, tastings, flights, or releases; remove, supersede, correct, or restore public content quickly; preview intended screens and confirm delivery; and see offline, outdated, failed, partial, and restored delivery states.

Live inventory quantity, depletion prediction, automated tap state, purchasing, margin, or production data requires an authoritative external source and remains an add-on candidate. Manual operation must remain viable without it.

## Service models

One venue may use table service, bar service, counter service, self-directed discovery, and hybrids that vary by area, time, event, or product.

Service model changes terminology, screen-purpose recommendations, placement, and guidance. It is product/domain configuration, not an entitlement. Future surfaces must preserve explicit venue and area scope and must not infer reservation, ordering, payment, or fulfillment capability.

## Age restrictions and responsible presentation

Track 0 does not define alcohol law, licensing, responsible-service policy, age thresholds, health claims, required warnings, or jurisdiction-specific wording.

The planning contract supports locally approved public information with explicit authority and scope, operator- or source-supplied age and access wording, non-promissory availability language, protection against unsupported legal or health claims, accessible non-color-only presentation, and clear effective periods, correction, expiration, supersession, and restoration.

Permissions govern who may edit or publish controlled wording. Jurisdiction and compliance requirements remain product/domain inputs or external policy, not commercial feature flags.

## Entertainment and event operations

Live music, DJs, trivia, karaoke, sports, tastings, release events, watch parties, and other programming may be a primary reason to visit.

Manual event communication remains core and includes event name and type; local date and time; doors, start, and end where known; area or viewing zone; delay, cancellation, relocation, replacement, pause, or resumption; public entry information; approved age or access wording; current screen targets and delivery state; and correction and restoration.

Advanced recurring programming, lineup workflow, sports-feed ingestion, ticketing, guest-list synchronization, performer or rights data, campaign orchestration, and event analytics remain tier or add-on candidates.

## Reservations, guest lists, cover, tickets, and private events

These concepts remain distinct. A reservation is a held table, area, or attendance arrangement. A guest list is an access method or named entry list. Cover is an admission charge. A ticket is a separately issued admission instrument. A private event is a restricted event or venue/area use period.

Public signage may communicate approved instructions, availability statements, access information, and private-event closures. It must not claim that a person is admitted, eligible, reserved, paid, verified, or on a guest list without an authoritative system and appropriate privacy controls.

Manual general information remains core. Reservation, guest-list, ticketing, payment, identity, and access-control synchronization remain independent add-on candidates where external capability is required.

## Inventory volatility and rapid update rhythm

Operators may make repeated changes during one shift. Quick Update stays universally core. The current effective state and last successful publish must be prominent; changes must identify venue, area, list, item, event, special, and target scope; mixed states must be visible before bulk actions; stale imported state must not silently override newer approved manual state; and recovery must support retry, correction, supersession, and restoration.

Advanced approval chains, multi-user coordination, source-conflict workflow, scheduled change sets, and deep audit history may be tier candidates. Basic history sufficient for confirmation and restoration remains core.

## Subtype operating differences

| Subtype | Dominant operating rhythm | Default emphasis |
| --- | --- | --- |
| Pub | community service, recurring events, mixed drinks and optional food | house specials, drinks, food, recurring events, clear late service periods |
| Sports Bar | fixture-led peaks and multiple viewing zones | games, viewing areas, game-day offers, rapid schedule and zone changes |
| Cocktail Bar | curated made-to-order lists and seated/bar service | signature and seasonal lists, ingredient availability, low-light readability |
| Wine Bar | by-glass, by-bottle, flights, tastings, reservations | serving format, producer context, tastings, table/area guidance |
| Brewery | house portfolio, releases, tours, packaged product | release status, tours, tap and package formats, take-home guidance |
| Brewpub | coordinated beverage and kitchen operation | tap list, food menu, pairings, separate kitchen/bar hours |
| Taproom | rotating taps, pours, flights, frequent keg changes | current taps, pour sizes, flights, releases, fast availability changes |
| Nightclub | doors, admission, DJs/live programming, late-night zones | lineup, entry information, cover, guest list, areas, last entry, bar menu |
| Lounge | reservations, table/area service, curated atmosphere | tables, seating areas, curated lists, events, premium but readable presentation |
| Unspecified / General Bar | mixed or uncertain operating model | neutral drinks, specials, events, areas, service periods, screen health |

Subtype changes defaults and recommendations only. It does not grant commercial access, permissions, limits, legal status, or automatic integrations.

## Capability-presentation implications

Future Bar Operate surfaces should prioritize the active job: opening or shift start; live service; event surge; last-call period; and close or handoff. Every view must make venue and area scope, effective local time, current state, source, target screens, publication result, and recovery path explicit.

The project-local Impeccable guidance applies to future UI planning: Operate mode, explicit task and scope, progressive disclosure, first-use/empty/loading/permission/validation/stale/conflict/offline/partial-delivery/success/undo/restoration states, phone and desktop layouts, long names, local-time clarity, 200% zoom, keyboard and assistive-technology access, non-color-only status, restrained motion, distance readability, and the approved Sky Blue administrative direction.

## Classification decisions

1. Hours, service periods, tap position, serving format, availability, release, special, event, entry, reservation, guest-list, cover, private-event, area, source, freshness, publication, and delivery values are **product/domain state** where represented.
2. Manual content, availability, hours, specials, event information, responsible wording, targeting, preview, publishing, confirmation, offline/outdated awareness, correction, supersession, and restoration are **core capabilities**.
3. Permissions control who may view, edit, approve, publish, restore, manage restricted wording, or act for a venue or area.
4. Advanced schedules, campaigns, approvals, cross-venue sharing, coordinated event operations, deep history, and premium analytics are **tier-entitlement candidates**.
5. POS, inventory, tap-management, reservation, guest-list, ticketing, identity, access, sports, event, AI, managed hardware, and other automatic synchronization are **independent add-on candidates** where external capability is required.
6. Counts of venues, areas, screens, users, lists, items, taps, events, schedules, campaigns, integrations, storage, history, transactions, and AI use are **usage or quantity limits**.
7. Experiments, migrations, compatibility controls, emergency disable controls, and staged delivery are **internal rollout flags**.

## Boundaries and handoff

Documentation and planning only. No product behavior was implemented. RWP-13.06 and Phase 14+ remain paused.

RWP-00.19 owns the required-capability inventory and must preserve the essential manual operating core defined here.