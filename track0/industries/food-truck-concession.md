# Food Truck & Concession Industry Profile

## Identity

- **Industry:** Food Truck & Concession
- **RWP range:** RWP-00.39 through RWP-00.50
- **Current status:** Industry definition complete; subtype definition is next
- **Baseline:** Restaurant
- **Current RWP:** RWP-00.39
- **Next sequential RWP:** RWP-00.40 — Venue Subtypes

## Purpose

This profile covers mobile, temporary, event-based, and concession-led food-service concepts whose guest experience depends on accurate menus, current operating location or event information, rapid sell-out changes, reliable publishing, and readable presentation in compact, outdoor, crowded, or intermittently connected environments.

It inherits the complete Restaurant baseline. This document records only the differences needed to establish the industry boundary and guide later subtype, terminology, operations, capability, packaging, onboarding, dashboard, and analytics RWPs.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- communicate where a unit or stand is operating now, when service is available, and whether the location or event has changed;
- bring a compact menu and one or more screens into service quickly during setup, then confirm that every intended display received the correct content;
- mark items, combos, service windows, or the entire operation sold out, unavailable, paused, reopened, relocated, or closed without rebuilding content;
- keep essential menu, price, availability, pickup, queue, and venue guidance understandable in bright daylight, glare, weather, noise, motion, crowds, and long viewing distances;
- continue safe operation when connectivity is weak or interrupted and recover confidently when publishing or delivery confirmation fails;
- coordinate consistent brand and content while allowing unit-, stand-, event-, and location-specific differences across mixed organizations.

## Inherited unchanged from Restaurant

Unless a later Food Truck & Concession RWP records a meaningful exception, this industry inherits:

- menu, category, item, price, description, image, and dietary-label management;
- manual availability and Quick Update;
- screen pairing, management, explicit targeting, preview, and immediate publishing;
- delivery confirmation, online/offline and outdated status, and recovery to a prior published version;
- basic layouts and themes;
- business hours and venue information;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, campaigns, multi-screen and multi-venue coordination, approvals, history, analytics, identity, AI, hardware, and integration capabilities.

Restaurant-style fixed-location service, table service, broad menus, and stable dayparts remain inherited where a venue actually uses them; they are not assumed to define the primary operating model.

## Meaningful differences from Restaurant

### Operating location and event as first-class context

A mobile or temporary operation may serve from a different stop, market, festival, venue, campus, attraction, stadium, arena, convention, private site, or temporary pitch over time. The current operating location or event is product/domain state that affects guest communication, defaults, screen targeting, and operational context. It is not an entitlement or rollout flag.

### Temporary and changeable service windows

Service may exist only for a short event window, change with permits or venue access, start after setup, end when stock is depleted, or move because of weather, traffic, event conditions, or host requirements. Detailed status vocabulary and scheduling rules belong to RWP-00.41 and RWP-00.42, but later work must not assume a permanent address or a stable weekly schedule.

### Rapid setup, teardown, and relocation

Screens, connectivity, menu state, and local operating information may need to be checked at every stop or event. Setup and teardown must preserve existing content and pairing authority while providing clear readiness, delivery, offline, outdated, and recovery information.

### Compact menus and queue surges

Many units operate with a focused menu, limited preparation capacity, fast sell-outs, and short periods of intense demand. Current availability, price, combo structure, pickup instructions, and queue guidance must be rapidly scannable. Manual availability remains an inherited core capability and cannot be replaced by a premium integration requirement.

### Intermittent connectivity and environmental exposure

Operators may work on cellular, venue Wi-Fi, temporary networks, or no reliable connection. Guest-facing screens may be outdoors, near service windows, exposed to glare, vibration, heat, cold, weather, or dense visual competition. Offline resilience and understandable delivery state are therefore especially important inherited core behaviors.

### Unit and host-venue coordination

A business may operate one truck, several mobile units, a collection of stands, or concessions inside a larger host venue. Shared brand and commercial access remain organization concerns, while operating location, stand identity, local menu state, screen targets, and event context remain venue- or unit-specific.

## Content and screen-purpose differences

A Food Truck & Concession operation may use a combination of:

- compact menu and combo boards;
- sold-out, limited-quantity, last-order, or service-paused communication;
- current stop, event, stand, gate, section, window, or pickup information;
- queue, order-ready, collection, or service-window instructions;
- event-specific promotions and sponsor or host-required content;
- wayfinding, operating hours, temporary closure, relocation, or weather notices;
- multi-stand or multi-unit coordination screens;
- brand or atmosphere content where essential ordering facts remain dominant.

The profile does not presume that every screen is permanently installed, always online, or assigned to one fixed address.

## Industry boundary

### Included as native concepts

The profile is intended to support concepts including:

- food trucks and mobile kitchens;
- food trailers;
- motorized and nonmotorized food carts;
- mobile snack, refreshment, dessert, beverage, and ice-cream units;
- temporary festival, fair, market, street-event, and pop-up food vendors;
- kiosks, booths, stalls, and temporary concession stands where prepared food or drink for immediate consumption is the defining operation;
- stadium, arena, theater, convention, attraction, campus, and similar concession operations where stand- or unit-level food service is the primary Vennusign context;
- related hybrids where mobility, temporary placement, host-venue operation, or concession service is a primary operating identity.

The exact supported subtype catalog, subtype definitions, and hybrid rules belong to RWP-00.40.

### Included through venue-level mixed-industry behavior

A unit or stand may use this business type even when its parent organization has another primary industry. Examples include a mobile unit within a Restaurant group, a pool or lobby concession within Hospitality, a stadium stand within Entertainment & Attractions, or a temporary festival unit operated by a Café, Bakery & Dessert business.

### Outside the canonical boundary

The following are not treated as native Food Truck & Concession concepts unless an included mobile, temporary, or concession-led guest-facing operation is also present:

- fixed-location restaurants whose stable premises and restaurant service model define the product need;
- fixed cafés, bakeries, dessert shops, bars, or retail stores where mobility or concession operation is incidental;
- catering businesses focused on private event delivery or banquet service without a public-facing mobile or concession service point;
- institutional cafeterias or broad food-service contracting operations where stand- or concession-level guest communication is not the primary use case;
- vending-machine operations;
- unprepared produce, packaged-food, merchandise, or nonfood street vending;
- host venues, attractions, stadiums, festivals, or events whose food service is not operated as a distinct venue or unit in Vennusign.

These boundaries determine Vennusign defaults and profile selection only. They are not legal, permit, licensing, food-safety, tax, contractual, or statistical classifications.

## Organization and venue behavior

### Organization primary industry

- An organization may select Food Truck & Concession as its primary industry.
- Primary industry seeds organization-level terminology, recommendations, starter content, and first-unit setup.
- Primary industry is product/domain configuration, not a subscription entitlement.
- Changing primary industry must not silently add or remove commercial access.

### Venue, unit, or stand business type

- Every operational venue, unit, or stand may select its own business type and, later, a supported subtype.
- Business type controls local defaults, labels, screen-purpose recommendations, starter content, and operational guidance.
- Business type does not override organization-level entitlement authority.
- Changing a business type must preserve existing customer content and require explicit review before defaults are replaced.
- A physical vehicle, cart, booth, kiosk, or stand is not automatically the same thing as an entitlement-counting venue; scope and limit policy is deferred to later Track 0 work.

### Operating location and event state

- Current, upcoming, changed, canceled, or completed operating locations and events are product/domain state when represented.
- Manual location, event, service-window, relocation, and closure communication is part of viable daily operation and must remain accessible without requiring external integrations.
- Permissions determine who may change those values.
- Automatic route, host-event, venue, or location synchronization remains a later packaging question.

### Mixed organizations

- Food Truck & Concession, Restaurant, Café, Bakery & Dessert, Hospitality, Entertainment & Attractions, and other venue types may coexist within one organization.
- Shared libraries, brand controls, users, analytics, and commercial access remain organization concerns unless a later approved policy explicitly defines a different scope.
- Unit- and stand-specific terminology, location state, screen targets, menu state, and defaults must remain local.
- Organization-wide views must use neutral language when truck-, stand-, venue-, and event-specific terms would be ambiguous.
- Host-venue relationships must not silently transfer ownership, permissions, commercial access, or content authority between organizations.

## Initial capability-classification rules

RWP-00.39 establishes these rules for later detailed work:

1. Organization primary industry is **product/domain state** that selects defaults and recommendations.
2. Venue, unit, stand, or subtype selection is **product/domain state** that selects local defaults and terminology.
3. Current operating location, event, service window, relocation, closure, and similar operational values are **product/domain state** when represented.
4. Manual menu availability, operating-location communication, closure or relocation communication, screen targeting, publishing, delivery confirmation, offline awareness, and recovery remain **core capabilities** required for viable daily operation.
5. Automatic POS, order, inventory, route, event, host-venue, or location synchronization remains a future integration-packaging question and must not replace the manual core operation.
6. Counts of venues, units, stands, screens, users, integrations, retained history, storage, or AI consumption are **usage or quantity limits**, not capabilities.

Detailed required, optional, packaging, onboarding, dashboard, and analytics classifications are intentionally deferred to their approved RWPs.

## Impeccable planning guardrails

RWP-00.39 is definition work rather than a detailed UI specification. Because no interactive discovery is available during this scheduled planning run, the project-local Impeccable `shape` guidance is applied with these explicit assumptions for later UI-facing RWPs:

- **Job and audience:** an operator often works from a phone or compact counter device during setup or active service and needs to confirm location, readiness, menu state, screen delivery, and recovery quickly; guests need to understand where and when service is available and what they can order now.
- **Modes:** operator surfaces use **Operate** mode; guest-facing menus and operational notices use **Read** mode; selective promotional surfaces may use **Experience** only when ordering facts remain dominant.
- **Primary outcome and proof:** the operator can make a location, service, availability, or publish change and verify the intended unit or screen received it; the guest can immediately understand location or stand identity, open/closed state, current offerings, prices, and collection instructions.
- **Hierarchy:** current operating location or event, open/closed or service status, item availability, product identity, price, combo or option structure, pickup or queue instructions, and publish health outrank secondary promotional content.
- **Material states:** later specifications must cover first-run, no location selected, upcoming, setup, ready, open, paused, limited, sold out, available again, last order, relocated, canceled, closed, teardown, offline, outdated, permission-restricted, publish-failed, delivered, and recovery conditions where applicable.
- **Realistic ranges:** planning must handle one unit and one screen through multi-unit and multi-stand operations; short and long location or event names; one-day and multi-day events; small and larger menus; no-image and image-heavy content; multilingual text; and portrait, landscape, temporary, and permanently installed displays.
- **Responsive and environmental behavior:** mobile-first operation, touch use with limited space, desktop administration, outdoor glare, weather, vibration, crowds, long viewing distances, and intermittent networks are binding design conditions rather than edge cases.
- **Accessibility:** color alone must not communicate location, service, availability, delivery, or error state; essential text must remain legible at distance and under glare; motion must be restrained and never delay ordering information; recovery instructions must be plain and actionable.
- **Feedback and recovery:** high-impact changes require explicit target confirmation, visible delivery state, safe undo or restoration, stale/offline distinction, and guidance for continuing service when a screen or network is unavailable.
- **Visual direction:** preserve the approved Sky Blue direction for Vennusign administrative surfaces. Guest-facing themes may express unit, event, sponsor, or host branding without weakening operational hierarchy.

These guardrails shape planning only and authorize no UI implementation.

## Owner decisions and deferred questions

The following are intentionally carried into RWP-00.40 or later RWPs rather than decided here:

- the exact distinction among food truck, trailer, cart, kiosk, booth, stall, pop-up, market vendor, mobile beverage unit, fixed concession, and event concession subtypes;
- whether a long-term semi-permanent kiosk or concession should use this profile or Restaurant based on operating model rather than physical form;
- how vehicles, stands, service windows, venues, and host locations map to organization, venue, screen, and future limit scopes;
- whether one mobile unit operating multiple simultaneous service points is represented as one venue or multiple venues;
- how shared or rotating screens at host venues preserve content and authority between operators;
- how catering-concession hybrids are distinguished from private catering operations;
- how sponsor, host-venue, and operator content authority should be represented;
- the neutral organization-wide term for venues, units, stands, and locations;
- the default business type for a mixed organization when no single operating model is dominant.

## Reference anchors

These references inform the boundary but do not replace Vennusign's product model:

- [U.S. Census Bureau 2022 NAICS 722330 — Mobile Food Services](https://www.census.gov/naics/?details=722330&input=722330&year=2022) covers preparation and immediate-consumption service from motorized vehicles and nonmotorized carts, including mobile food concession, refreshment, snack, cart, and truck operations.
- [U.S. Census Bureau 2022 NAICS 722310 — Food Service Contractors](https://www.census.gov/naics/?details=722310&input=722310&year=2022) includes food concession contractors at sporting, entertainment, and convention facilities, supporting a distinction between mobile operations and host-venue concession operations.
- [U.S. Census Bureau 2022 NAICS 722320 — Caterers](https://www.census.gov/naics/?details=722320&input=722320&year=2022) distinguishes single-event catering from mobile immediate-consumption service and longer-term contracted food service, informing the catering-concession boundary.

These references are used only as industry-boundary evidence. They do not define Vennusign entitlements, subtype eligibility, legal obligations, or limit counting.

## Validation checklist

- [x] Restaurant inheritance is explicit.
- [x] Only meaningful deltas are documented.
- [x] Initial concerns have one primary classification.
- [x] Essential manual availability, location communication, publishing, offline awareness, and recovery operations remain core.
- [x] Permissions, states, entitlements, add-ons, limits, and rollout flags remain separate.
- [x] Impeccable `shape` guidance was consulted for UI-facing planning.
- [x] Job, audience, hierarchy, states, realistic ranges, accessibility, responsive behavior, feedback, and recovery are documented.
- [x] No product implementation was performed.
- [x] The next sequential RWP is identified as RWP-00.40.
