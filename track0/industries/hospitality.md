# Hospitality Industry Profile

## Identity

- **Industry:** Hospitality
- **RWP range:** RWP-00.51 through RWP-00.62
- **Current status:** Industry definition and venue-subtype model complete; business terminology is next
- **Baseline:** Restaurant
- **Current completed RWP:** RWP-00.52
- **Next sequential RWP:** RWP-00.53 — Business Terminology

## Purpose

This profile covers lodging-led properties whose guest experience depends on accurate arrival, stay, event, amenity, dining, wayfinding, service, and safety information across public areas and changing operating periods.

It inherits the Restaurant baseline. This document records only the meaningful differences needed to establish the Hospitality boundary and guide later terminology, operations, capability, packaging, onboarding, dashboard, and analytics RWPs.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- help guests understand where to go and what is available from arrival through departure;
- keep amenity hours, service availability, event schedules, meeting-room assignments, dining information, transportation, and temporary notices current;
- publish reliably to selected lobby, corridor, elevator, conference, amenity, dining, retail, staff, and in-room or guest-service displays;
- coordinate property-wide brand and operational information while allowing building-, area-, outlet-, event-, and screen-specific differences;
- communicate closures, relocations, delays, changed hours, maintenance, weather, safety, and recovery guidance without rebuilding content;
- support multilingual, accessible, calm, and distance-readable presentation for guests who may be unfamiliar with the property;
- confirm that high-impact information reached every intended display and recover safely from stale, offline, or failed delivery.

## Inherited unchanged from Restaurant

Unless a later Hospitality RWP records a meaningful exception, this industry and every subtype inherit:

- reusable content creation, editing, duplication, archive, restore, preview, and publishing patterns;
- screen pairing and management, explicit targeting, delivery confirmation, offline and outdated detection, and prior-version recovery;
- basic layouts, themes, business hours, venue information, understandable errors, and recovery guidance;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, dayparts, campaigns, coordinated screens, multi-venue sharing, brand controls, approvals, history, analytics, identity, AI, hardware, and integration capabilities.

Restaurant menu, category, item, price, dietary, availability, and special semantics remain inherited for food-and-beverage outlets that use them. They are not assumed to be the primary content model for a lodging property as a whole. Later Hospitality RWPs will define the required lodging vocabulary and content objects without removing Restaurant behavior from mixed properties.

## Meaningful differences from Restaurant

### Property-wide guest journey

Hospitality information supports arrival, orientation, stay, events, services, disruptions, and departure rather than only ordering and service-period decisions. Content priorities can change by time of day, guest location, event program, weather, occupancy pattern, or property condition.

### Property hierarchy and local context

One property may contain buildings, towers, wings, floors, lobbies, meeting rooms, ballrooms, restaurants, bars, pools, spas, retail, transportation points, staff areas, and temporary event spaces. Property, area, outlet, event, and screen context are product/domain state used for organization, defaults, targeting, and presentation. They are not entitlements. Exact hierarchy and future limit counting remain deferred.

### Wayfinding and event changes

Guests frequently lack local knowledge. Manual wayfinding, room assignment, event schedule, amenity, closure, relocation, and changed-hours communication is part of viable daily operation and must not depend on a premium integration. Automatic property-management, event, room-booking, point-of-sale, transport, or guest-service synchronization remains a later packaging question.

### Continuous operation and handoffs

Properties may operate continuously while individual services open, close, relocate, pause, or change ownership across shifts. The product must distinguish property availability from the state of a specific amenity, outlet, event, room, or service and provide clear handoff, delivery, stale, and recovery information.

### Mixed public and operational audiences

Public displays may serve arriving guests, conference attendees, diners, visitors, and local customers. Other displays may support staff operations. Audience, privacy, authorization, and content authority require clearer separation than a typical single-venue restaurant. Hospitality signage must not expose guest-specific or sensitive operational information by default.

### Calm, accessible, multilingual presentation

Guests may be tired, stressed, unfamiliar with the property, carrying luggage, navigating with mobility or sensory needs, or reading in a second language. Wayfinding and service information require strong hierarchy, plain language, restrained motion, non-color status cues, and dependable mobile, portrait, landscape, and distance-reading behavior.

## Content and screen-purpose differences

A Hospitality property may use a combination of:

- arrival, welcome, check-in, concierge, and departure guidance;
- lobby, corridor, elevator, floor, building, parking, and transportation wayfinding;
- meeting-room, ballroom, conference, wedding, group, and event schedules;
- amenity hours, availability, access, closure, relocation, and maintenance notices;
- restaurant, bar, café, room-service, breakfast, retail, spa, pool, fitness, entertainment, and local-experience information;
- weather, transport, shuttle, parking, local-area, and guest-service notices;
- property-wide operational, safety, emergency, and recovery communication;
- staff-facing operational information where permissions and privacy remain explicit;
- brand, atmosphere, destination, loyalty, promotion, and sponsor content where essential guidance remains dominant.

The profile does not presume that every display shares the same audience, hours, content authority, privacy level, or physical environment.

## Industry boundary

### Included as native concepts

The profile is intended to support lodging-led concepts including:

- hotels, resort hotels, and motels;
- boutique and lifestyle lodging properties;
- hostels and guest-house-style traveler accommodation;
- extended-stay hotels and serviced-apartment lodging operations;
- conference, convention, wedding, group, and event-led lodging properties;
- casino resorts where lodging is a primary property function;
- related hybrids where short-term or extended-stay guest accommodation is the primary operating identity.

### Included through venue-level mixed-industry behavior

A Hospitality organization may contain Restaurant, Bar/Brewery/Nightlife, Café/Bakery/Dessert, Food Truck/Concession, Entertainment/Attractions, retail, spa, meeting, and other operational venues. Those venues may use their own approved business type while sharing organization-level brand, users, libraries, analytics, and commercial authority.

### Outside the canonical boundary

The following are not treated as native Hospitality concepts unless a lodging-led guest operation is also present:

- residential apartment, condominium, dormitory, or long-term housing operations without a traveler-lodging service model;
- vacation-rental or property-listing businesses with no managed shared guest-facing property environment;
- hospitals, rehabilitation centers, assisted-living, senior-living, or other care facilities;
- standalone restaurants, bars, attractions, casinos, conference centers, event venues, retail centers, offices, and transportation facilities without a lodging-led property;
- campgrounds, recreational-vehicle parks, cruise ships, and broad travel operations until separately approved;
- private homes or informal accommodation without a managed property-level signage use case.

These boundaries determine Vennusign defaults and profile selection only. They are not legal, licensing, accessibility, safety, tax, lodging, gaming, tenancy, or statistical classifications.

## Canonical venue subtypes

Subtype is local product/domain configuration. It selects defaults, terminology candidates, starter-content suggestions, screen-purpose recommendations, and operating guidance. It is not a tier, entitlement, permission, usage allowance, rollout flag, legal classification, star rating, brand segment, or substitute for the property's real content.

A property may remain **Unspecified / General Hospitality Property** when no supported subtype clearly controls its daily operating rhythm. This is a neutral fallback state rather than a tenth commercial package.

| Primary subtype | Bounded definition and inclusion rule | Exclusion or neighboring-profile rule | Meaningful defaults and presentation differences |
| --- | --- | --- | --- |
| **Hotel** | A lodging-led property providing short-term guest rooms and a managed arrival, stay, and departure experience. Use when no more specialized approved subtype controls daily operation. | Do not use merely because a brand markets itself as a hotel when Resort, Motel, Extended-Stay, Conference Property, Casino Resort, or another subtype clearly drives operations. | Favor arrival and check-in guidance, lobby orientation, room and floor wayfinding, amenity and outlet hours, guest-service notices, departure information, and balanced property-wide presentation. |
| **Resort** | A lodging-led destination property where recreation, leisure, wellness, entertainment, dining, outdoor activity, or a broad amenity campus materially shapes the guest stay and daily information rhythm. | Use Hotel when accommodation remains primary but destination amenities do not control the guest journey. A standalone attraction, spa, club, or entertainment venue without lodging is not a Resort subtype. | Favor campus and amenity wayfinding, activity schedules, transport, weather, seasonal conditions, family or adult zones, dining and entertainment discovery, closures, relocation, and destination-led presentation. |
| **Motel** | A lodging-led property where direct vehicle access, roadside arrival, exterior room access, parking proximity, or a compact low-rise layout materially shapes guest orientation and service. | Use Hotel when interior corridors, lobby-led circulation, or broader full-service operations control the guest journey. Roadside branding alone is insufficient. | Favor property entrance, parking and building-zone guidance, exterior room ranges, late-arrival instructions, compact amenity and breakfast information, safety notices, and high-legibility outdoor presentation. |
| **Hostel** | A traveler-lodging property where shared accommodation, dormitory-style rooms, communal facilities, social areas, shared kitchens, activities, or community-oriented service materially shapes operations. Private rooms may also exist. | Dormitories, student housing, shelters, care facilities, and long-term residential operations without a traveler-lodging model remain outside. Use Hotel or Boutique Lodging when shared accommodation and communal operation are incidental. | Favor reception and access guidance, room or bed-zone orientation without exposing guest-specific data, shared-facility hours, quiet hours, activity and community schedules, luggage and kitchen guidance, multilingual communication, and compact staff workflows. |
| **Extended-Stay** | A lodging-led property designed for longer traveler stays where weekly or monthly rhythms, kitchenettes, recurring housekeeping, laundry, package, workspace, and resident-like service needs materially shape daily operation. | Ordinary apartments and long-term residential housing remain outside. Use Hotel when long stays are offered but do not change service rhythm; use Serviced Apartment when apartment-style units and residential-style access or building structure dominate. | Favor recurring service schedules, housekeeping and linen cycles, laundry, kitchen and workspace guidance, package and local-service notices, longer-horizon events, and clear distinction between property and individual-service state. |
| **Serviced Apartment** | A managed traveler-lodging operation using apartment-style units with hospitality services, shared guest support, and a property-level arrival, access, wayfinding, or service environment. | Residential apartment management, condominium associations, and listing-only vacation rentals without a managed shared guest experience remain outside. Use Extended-Stay when a hotel-style property with longer-stay service remains the dominant model. | Favor building and unit-zone access, reception or remote-arrival guidance, elevators and floors, housekeeping and maintenance windows, local services, shared amenities, property rules, and calm long-stay communication. |
| **Conference Property** | A lodging-led property where conferences, conventions, meetings, weddings, groups, or multi-room events consistently drive guest arrivals, room assignments, wayfinding, service changes, and display coordination. | A standalone convention center, meeting venue, wedding venue, or event campus without lodging is not Hospitality by this subtype. Use Hotel or Resort when event business exists but does not control daily information needs. | Favor group arrivals, event directories, room and ballroom assignments, schedules, session changes, relocation, sponsor and host content, dining and break guidance, multi-screen synchronization, and rapid event recovery. |
| **Casino Resort** | An integrated lodging property where casino gaming and its associated entertainment, dining, loyalty, event, and high-volume circulation environment materially shapes the stay. Lodging remains a primary property function. | A standalone casino or gaming venue without lodging belongs outside Hospitality. Use Resort when gaming is incidental rather than a defining operating context. | Favor entrances, towers, gaming areas, entertainment and event schedules, dining, loyalty and guest-service guidance, age- or access-sensitive public wording, crowded-floor wayfinding, continuous operation, and clear public-versus-restricted content boundaries. |
| **Boutique Lodging** | A lodging-led property where a smaller-scale, independent, design-led, lifestyle, heritage, local, or highly curated guest experience materially shapes content, service, and presentation. It need not be luxury. | Marketing language, décor, room count, or price alone is insufficient. Use Hotel when the standard hotel operating model should control defaults, or another subtype when destination, conference, gaming, shared-accommodation, or long-stay behavior is more important. | Favor distinctive arrival, local recommendations, curated amenities and experiences, flexible service hours, staff-authored guidance, restrained brand storytelling, and high-quality presentation without weakening operational clarity. |

## Hybrid and ambiguous concepts

Hybrid properties use one primary subtype plus optional descriptive traits. Traits tune recommendations and future terminology; they do not stack entitlements, increase limits, transfer authority, or create multiple commercial identities.

Descriptive traits may record destination or recreation focus, all-inclusive service, wellness, ski or beach setting, conference and wedding emphasis, gaming, heritage, independent or lifestyle positioning, apartment-style units, extended-stay service, campus or multi-building form, seasonal operation, franchise or management relationship, and mixed-use outlets. Traits must not become hidden feature flags.

### Selection rules

1. Choose the subtype that best describes the property's **dominant daily operating rhythm and guest communication need**, not its legal entity, license, brand tier, star rating, ownership, management contract, tax treatment, architecture, room count, or marketing phrase.
2. Consider arrival pattern, length of stay, circulation, shared versus private accommodation, amenity and activity breadth, event cadence, gaming, vehicle access, and the defaults operators need most often.
3. When two models overlap, select the one that should control local terminology and first-run recommendations, then retain the other as a descriptive trait.
4. When no model clearly dominates, remain neutral rather than forcing a misleading choice.
5. Organization primary industry may seed the first suggestion but never overrides a property's local subtype.
6. Subtype does not determine which capabilities are commercially available or how many properties, buildings, rooms, venues, outlets, areas, events, or screens a plan permits.

### Canonical ambiguous cases

- **Full-service city hotel with substantial meetings:** Conference Property when group and event schedules consistently control daily guest communication; otherwise Hotel with a conference trait.
- **Destination hotel with spa, pools, activities, or ski access:** Resort when the amenity campus and destination journey control the stay; otherwise Hotel with destination or wellness traits.
- **Roadside property branded as an inn or hotel:** Motel when vehicle access and exterior circulation control orientation; otherwise Hotel or Boutique Lodging according to the operating model.
- **Inn, bed-and-breakfast, guest house, lodge, or heritage property:** Boutique Lodging when curated, local, small-scale, or heritage experience controls operation; Resort when destination amenities dominate; Hotel when a general lodging model is most accurate. Informal private accommodation without a managed property environment remains outside.
- **Aparthotel or residence-style hotel:** Serviced Apartment when apartment-style units and residential-style access dominate; Extended-Stay when longer-stay hotel service rhythms dominate; Hotel when neither materially changes operation.
- **Vacation club, branded residence, or mixed ownership property:** select Resort, Serviced Apartment, Extended-Stay, or Hotel only when a managed traveler-lodging and shared guest-service environment exists. Ownership model remains separate from subtype and authority.
- **Casino hotel with limited resort amenities:** Casino Resort when gaming materially shapes circulation and guest information. A standalone casino remains outside Hospitality.
- **Hostel with many private rooms:** Hostel when shared facilities, community programming, and hostel operating rhythm remain material; Boutique Lodging or Hotel when they do not.
- **Convention center attached to a hotel:** Conference Property for the lodging property when conference operation dominates. The convention facility may be modeled as an area, event venue, or separate local business type according to later hierarchy decisions.
- **Resort containing restaurants, bars, cafés, retail, spa, golf, attractions, or concessions:** the property keeps Resort while distinct outlets may use their own approved local industry and subtype.
- **One campus with several lodging buildings or towers:** use one property subtype when a unified guest journey and authority model exists; retain building and tower structure as product state. RWP-00.52 does not decide future property or venue counting.
- **Management company operating different brands:** each property chooses its own subtype. Management, ownership, franchise, brand, and commercial authority remain separate organization concerns.

## Restaurant capability inheritance by subtype

Every subtype inherits the Restaurant baseline. The table records only where inherited capabilities are emphasized or starter recommendations differ.

| Subtype | Restaurant capabilities most visibly inherited | Additional emphasis, not a separate capability |
| --- | --- | --- |
| Hotel | venue information, hours, reusable content, targeting, publishing, screen health, recovery | arrival, check-in and departure guidance; lobby, room and floor wayfinding; amenity and outlet state |
| Resort | all Hotel inheritance plus multi-venue content and schedules | campus navigation, activities, transport, weather, seasonal services, broad amenity discovery and closures |
| Motel | venue information, hours, immediate publishing, outdoor screens, recovery | parking, exterior room ranges, drive-up orientation, late arrival, compact services and outdoor legibility |
| Hostel | venue information, schedules, reusable notices, permissions, publishing | shared-facility state, quiet hours, activities, multilingual guidance, community spaces and privacy-safe room-zone information |
| Extended-Stay | venue information, schedules, reusable content, service-state updates | recurring housekeeping, laundry, kitchen, package, workspace, longer-horizon and resident-like service communication |
| Serviced Apartment | venue information, wayfinding, schedules, permissions, publishing | building and unit-zone access, remote arrival, housekeeping and maintenance windows, shared amenities and local services |
| Conference Property | schedules, multi-screen coordination, targeting, publishing, delivery confirmation, recovery | event directories, room assignments, relocations, group arrivals, sponsor or host content and rapid schedule changes |
| Casino Resort | hours, multi-screen targeting, publishing, delivery confirmation, recovery | continuous operation, crowded-floor wayfinding, gaming-area context, entertainment, dining, loyalty and public/restricted boundaries |
| Boutique Lodging | themes, reusable content, venue information, publishing, local promotion | curated arrival, local recommendations, flexible service, distinctive presentation and staff-authored guidance |
| Neutral fallback | full Restaurant and Hospitality inheritance | neutral property, area, service, event and guest-information language until a subtype is selected |

Subtype-specific screen purposes are recommendations using inherited or later-classified capabilities. They are not separate entitlements.

## Screen-purpose and presentation emphasis by subtype

- **Hotel:** lobby welcome, check-in and departure, directory, elevator and floor wayfinding, amenity hours, dining, guest services, transport, and operational notices.
- **Resort:** campus map, activities, pools and beach, ski or recreation conditions, spa and wellness, transport, dining, entertainment, weather, seasonal and closure notices.
- **Motel:** entrance and parking, building or room ranges, reception and late arrival, breakfast and compact amenities, safety and exterior operational notices.
- **Hostel:** reception, room or bed zones, shared kitchen and facilities, quiet hours, social events, local guidance, luggage, access and multilingual notices.
- **Extended-Stay:** arrival, recurring housekeeping, laundry, kitchen and workspace, package and local services, long-stay events, maintenance and amenity notices.
- **Serviced Apartment:** building access, reception or remote check-in, floor and unit-zone wayfinding, housekeeping, maintenance, shared amenities, local services and property rules.
- **Conference Property:** arrival and group welcome, event directories, meeting-room and ballroom schedules, wayfinding, break and dining guidance, sponsors, changes and relocation.
- **Casino Resort:** tower and property directories, gaming-area and entertainment wayfinding, dining, events, loyalty, transport, safety and high-volume operational notices.
- **Boutique Lodging:** distinctive welcome, curated amenities, local recommendations, flexible service, events, wayfinding and brand storytelling that never obscures operational information.
- **Neutral fallback:** general property welcome, directory, wayfinding, hours, events, amenities, services, notices, publishing and recovery.

## Organization and property behavior

### Organization primary industry

- An organization may select Hospitality as its primary industry.
- Primary industry seeds organization-level terminology, recommendations, starter content, and first-property setup.
- Primary industry is product/domain configuration, not a subscription entitlement.
- Changing primary industry must not silently add or remove commercial access.

### Local subtype selection

- Each Hospitality property selects one primary subtype or the neutral fallback independently of organization primary industry.
- Selection changes local defaults, terminology candidates, starter recommendations, screen-purpose suggestions, and planning guidance only.
- A subtype change must preserve customer-authored content, screens, pairing, targeting, themes, schedules, publication history, current property and service state, custom terminology, authority boundaries, and commercial access.
- A future change flow must preview effects and require explicit confirmation before replacing defaults.

### Mixed properties and organizations

- Hospitality and other approved industry types may coexist within one organization and one property.
- A restaurant, bar, café, concession, attraction, spa, retail outlet, conference venue, or other local operation may use its own approved business type where that produces more accurate defaults and terminology.
- Shared brand controls, users, libraries, analytics, and commercial access remain organization concerns unless a later approved policy defines another scope.
- Property-, building-, area-, outlet-, room-, event-, amenity-, service-, and screen-specific state and authority remain local.
- Management, franchise, ownership, operator, tenant, concession, brand, host, and sponsor relationships must not silently transfer permissions, commercial access, content authority, or guest data.

### Multi-property and copied content behavior

- Different properties in the same organization may use different Hospitality subtypes.
- Organization-wide templates and shared content may be copied or targeted across subtypes only when the operator explicitly chooses the destinations and reviews local terminology, hierarchy, service, privacy, and screen-purpose differences.
- Copying content does not copy property state, guest-specific data, room assignments, authority, integrations, entitlements, or quantity allowances.

## Classification decisions

1. Organization primary industry, primary property subtype, neutral subtype state, and optional descriptive traits are **product/domain state**.
2. Subtype may affect terminology candidates, starter content, recommendations, screen-purpose suggestions, and operating guidance only.
3. Subtype does not grant capabilities, change plan access, alter permissions, increase limits, control rollout, transfer authority, or act as a subscription entitlement.
4. Property, building, tower, wing, floor, area, outlet, room, event, amenity, service window, closure, relocation, and similar values keep their product/domain-state classification independent of subtype.
5. Subtype-specific screen purposes are recommendations using inherited or later-classified capabilities.
6. Manual guest information, wayfinding, event, amenity, service, closure, relocation, changed-hours, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
7. Counts of properties, buildings, rooms, venues, outlets, areas, events, screens, users, integrations, storage, retained history, or AI consumption remain independent usage or quantity limits.
8. Automatic property-management, event, room-booking, point-of-sale, transport, guest-service, access, gaming, or related synchronization remains a later integration-packaging question and cannot replace manual core operation.
9. Guest-specific, reservation-specific, room-specific, member-specific, or sensitive operational information requires later privacy and authorization decisions and is not assumed to be public signage content.
10. Subtype, brand, star rating, franchise, ownership, management, and marketing language must not become hidden feature flags.

## Impeccable planning guardrails

RWP-00.52 is planning rather than UI implementation. Because no interactive discovery is available during this scheduled run, the project-local Impeccable `shape` guidance is applied with these explicit assumptions for a future subtype selection and change experience:

- **Job and audience:** an owner, administrator, or authorized property manager selects the model that best matches a property's daily guest journey and later may change it without losing content or authority.
- **Mode:** the surface is an **Operate** experience; guests do not select the subtype.
- **Outcome and proof:** the operator can compare bounded “best when” definitions, choose one primary subtype or remain neutral, understand what defaults will change, and verify that content, screens, authority, and plan access remain intact.
- **Hierarchy:** dominant operating rhythm, guest journey, arrival and circulation pattern, length of stay, amenity and event breadth, and example screen purposes outrank brand, star rating, room count, architecture, ownership, management, and marketing terms.
- **Material states:** later specifications must cover first-run, neutral selection, recommended match, no clear match, existing selection, change preview, permission-restricted, validation failure, interrupted save, success, restoration, and mixed-property conditions.
- **Realistic ranges:** the flow must handle a small single-building property through multi-building resorts, conference campuses, casino resorts, management groups with different brands, long names, localized descriptions, many outlets, and overlapping traits.
- **Interaction and feedback:** show one primary subtype, optional descriptive traits, changed terminology and starter recommendations, preserved content, and explicit confirmation. Permit safe cancellation and restoration. Do not imply that selection performs integrations, migrates physical property structure, changes privacy scope, or purchases features.
- **Responsive and accessible behavior:** phone and desktop layouts must remain scannable, progressively disclose comparison detail, support keyboard and assistive technology, tolerate 200% zoom and localization expansion, and avoid color-only distinctions.
- **Visual direction:** preserve the approved Sky Blue direction for Vennusign administrative surfaces.

No UI, API, schema, migration, privacy, limit-counting, or product implementation is authorized by this brief.

## Owner decisions and deferred questions

The following remain intentionally deferred to RWP-00.53 or later RWPs:

- canonical operator and guest terminology for organization, property, building, tower, wing, floor, area, outlet, room, event, amenity, service, guest, stay, arrival, departure, wayfinding, notice, and closure;
- exact hierarchy and which levels count toward future limits;
- representation of franchise, ownership, management-company, tenant, concession, third-party event, brand, and sponsor authority;
- public, guest-only, staff-only, event-only, and sensitive information scopes;
- room-specific, reservation-specific, member-specific, or personalized display behavior;
- detailed operating-day, shift, housekeeping, activity, event, gaming, and service-state transitions;
- packaging, onboarding, dashboard, KPI, analytics, and integration decisions.

## Reference anchors

The existing RWP-00.51 references remain boundary evidence only. The subtype catalog is a Vennusign product model and does not claim to reproduce legal, licensing, tax, accessibility, gaming, tenancy, brand, quality-rating, or statistical classifications.

## Validation checklist

- [x] RWP-00.51 is complete and merged.
- [x] Restaurant inheritance is explicit and not duplicated as a new commercial model.
- [x] Hotel, Resort, Motel, Hostel, Extended-Stay, Serviced Apartment, Conference Property, Casino Resort, and Boutique Lodging have bounded definitions.
- [x] A neutral fallback is defined.
- [x] Inclusion, exclusion, neighboring-profile, hybrid, and ambiguous-case rules are documented.
- [x] Subtype differences map to defaults, terminology candidates, starter content, screen purposes, operating emphasis, or presentation guidance only.
- [x] Subtype, traits, property structure, authority, commercial access, permissions, and limits remain separate.
- [x] Mixed-property, multi-property, subtype-change, and copied-content behavior are documented.
- [x] Impeccable `shape` guidance covers job, audience, outcome, hierarchy, states, realistic ranges, interaction, responsiveness, accessibility, feedback, and recovery.
- [x] No product implementation was performed.
- [x] The next sequential RWP is identified as RWP-00.53.
