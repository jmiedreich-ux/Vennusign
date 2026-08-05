# Entertainment & Attractions Subscription Tier Mapping

## Authority and status

This documentation-only proposal completes RWP-00.70. It maps the authoritative RWP-00.69 classification to customer-outcome tier archetypes without approving final tier names, prices, allowances, trials, contracts, or implementation.

Industry and subtype remain product configuration and never determine price or access. Required core capabilities remain available in the base operating tier. Independent integrations, managed services, and usage limits remain separate from tier entitlement.

## Tier design principles

1. A customer with one venue and one working screen can perform every essential daily visitor-communication task in the base tier.
2. Higher tiers add coordination, governance, scale, and insight—not the ability to communicate accurate schedules, availability, disruptions, wayfinding, notices, delivery state, or recovery.
3. Ticketing, admissions, access, cinema, venue, queue, footfall, map, event, sports, translation, AI, identity-provider, hardware, connectivity, and managed-service connections remain independent add-ons.
4. Quantity and consumption allowances are limits, not capabilities.
5. Permissions determine authority, not commercial inclusion.
6. Industry and subtype tune terminology, starter content, recommendations, screen purposes, and presentation only.
7. Upgrade and downgrade must preserve customer-authored content, current public operation, source/freshness context, last-known-good content, and recovery.
8. Organization or portfolio inheritance must not silently overwrite venue-local operational truth.

## Proposed tier archetypes

Working names are descriptive placeholders and require owner approval.

### Tier 1 — Operate

**Customer outcome:** Run accurate, accessible visitor communication for a venue every day.

Includes the complete RWP-00.67 required core:

- venue, area, attraction, exhibit, event, session, queue, admission, route, and visitor-context information;
- manually authored programs, schedules, shows, screenings, performances, events, tours, activities, occurrences, and continuously available experiences;
- closures, delays, pauses, cancellations, relocations, restrictions, reopening, and recovery communication;
- manual queue, wait, capacity, sold-out, full, limited, entry-paused, admission, boarding, seating, and check-in guidance;
- manual destination-based wayfinding and temporary accessible-route guidance;
- notices and bounded safety-related public communication;
- basic manually authored language variants and accessible content;
- exact screen targeting, contextual preview, immediate publication, and supported scheduling;
- delivery confirmation, online/offline/outdated visibility, failed and partial delivery, retry, correction, expiry, supersession, unpublish, undo, and restore;
- source, freshness, stale, conflict, disconnect, partial synchronization, manual override, and last-known-good awareness;
- ordinary roles and permissions, privacy-safe public audiences, and authority boundaries;
- all required first-use, empty, loading, validation, permission, responsive, accessibility, failure, success, and recovery states.

Operate must remain useful without a paid integration. It may have limits on venues, screens, users, languages, templates, schedules, history, or storage, but RWP-00.70 does not choose values.

### Tier 2 — Coordinate

**Customer outcome:** Coordinate richer visitor journeys, events, teams, screens, languages, and content workflows within one or several closely managed venues.

Adds candidate Vennusign-authored capabilities:

- coordinated screen groups, zones, sequences, event moments, takeovers, and estate rollback;
- recurring schedules, conflict detection, blackout periods, event phases, and content calendars;
- advanced approval, assignment, acknowledgment, escalation, separation of duties, and shift handoff;
- native interactive maps, route variants, directories, kiosk flows, and mobile handoff;
- campaign, promotion, membership, sponsorship, fundraising, merchandise, and cross-sell workflow;
- advanced localization workflow, terminology libraries, translation assignment, review, and coverage governance;
- reusable brand and venue libraries, controlled local fields, asset rights, and versioning;
- advanced Vennusign dashboards and scheduled reports using Vennusign-owned operational and delivery data;
- native rules and coordination around queue, wait, capacity, crowding, events, and screen priorities when underlying data exists.

Coordinate does not include external ticketing, queue, footfall, map, translation, AI, identity, or managed-service connections by default. Those remain add-ons.

### Tier 3 — Portfolio

**Customer outcome:** Govern and coordinate multiple venues, campuses, districts, parks, cinemas, museums, sports estates, touring operations, franchises, brands, or operating groups while preserving local control.

Adds candidate Vennusign-authored capabilities:

- venue groups, regions, brands, districts, campuses, touring groups, and centralized exception views;
- shared standards, templates, brand libraries, language portfolios, campaigns, schedules, programs, and recovery playbooks;
- centralized preparation with venue-local review, override, opt-out, and mixed-state visibility;
- safe cross-venue copying and bulk actions with explicit included, excluded, and incompatible targets;
- portfolio-level screen, source, delivery, outdated, language, queue, event, campaign, and content oversight;
- multi-venue comparison, benchmarks, trends, exports, scheduled reporting, and data-quality views;
- delegated administration, venue/group role templates, broader retained history, and audit workflow;
- pooled or allocated limits where the approved commercial model permits.

Portfolio does not make every external system inclusive. Connections remain separately selectable add-ons and retain venue-specific source, privacy, rights, and authority boundaries.

### Tier 4 — Enterprise

**Customer outcome:** Operate Entertainment & Attractions signage under enterprise identity, governance, audit, risk, service, and complex-portfolio requirements.

Adds candidate Vennusign-authored capabilities:

- enterprise administration, access reviews, delegated governance, domain and session policy, and audit export;
- organization-wide role templates and controlled administration boundaries across owners, operators, promoters, tenants, sponsors, teams, performers, rights-holders, contractors, and seasonal staff;
- advanced retention, compliance, legal-hold, rights, sponsor, accessibility, safety-review, and administrative workflow where approved;
- enterprise analytics governance, approved data-sharing controls, and external BI administration;
- service-management views, coordinated change windows, migration, portfolio onboarding, and operating-governance workflow;
- complex mixed-industry, campus, district, resort, casino, arena, cultural, touring, and franchise administration where owner decisions permit.

SSO, SCIM, directory synchronization, identity-provider connections, managed hardware, connectivity, monitoring, installation, and premium service levels remain independent add-ons even when commonly purchased with Enterprise.

## Independent add-on catalog

| Add-on family | Candidate scope | Tier relationship |
| --- | --- | --- |
| Ticketing and admissions | Box office, ticketing, timed entry, reservations, membership, seat inventory, guest list, credentials, turnstiles, access control | Independent; manual operation remains core |
| Venue and experience systems | Cinema, venue, show control, collection, attraction, event, sports, team, league, promoter, production, rights-holder systems | Independent |
| Queue, occupancy, capacity, and footfall | Sensors, queue systems, camera or measurement sources, access counts, prediction sources | Independent; native dashboards and rules may be tiered separately |
| Maps and visitor journey | Third-party maps, indoor positioning, parking, transit, transport, weather, route, and venue-map sources | Independent; native map workflow may be tiered separately |
| Campaign and customer systems | CRM, loyalty, membership, donor, advertising, sponsor, ecommerce, retail, POS, merchandise, conversion sources | Independent; native campaigns may be tiered separately |
| Translation and language services | Machine translation, language provider, managed localization | Independent; native localization workflow may be tiered separately |
| AI services | Drafting, translation, detection, summarization, recommendation, image description, operational assistance | Independent consumption-backed add-on; bounded workflow may vary by tier |
| External analytics and BI data | Attendance, ticketing, queue, footfall, campaign, POS, membership, weather, transport, or BI sources | Independent; native analytics workflow may be tiered separately |
| Enterprise identity | SSO, SCIM, directory, identity-provider connection | Independent; enterprise administration may be tiered separately |
| Managed hardware and service | Displays, players, kiosks, mounts, installation, replacement, enrollment, monitoring, cellular connectivity, remote management, support levels | Independent |

Each add-on must define source, authority, rights, privacy, freshness, fallback, conflict, limits, metering, failure, cancellation, retention, export, deletion, and restoration behavior.

## Limits remain separate

Candidate limit families include:

- organizations, brands, groups, regions, campuses, districts, venues, buildings, floors, zones, areas, attractions, exhibits, habitats, events, sessions, queues, routes, gates, sections, stages, screens, lanes, fields, courts, tracks, and admission windows;
- screens, players, kiosks, devices, users, roles, approvers, audiences, languages, integrations, sources, identity providers, sensors, maps, and managed-service sites;
- templates, assets, campaigns, schedules, programs, reports, dashboards, exports, variants, sequences, event moments, automation rules, and coordinated publications;
- retained history, storage, transactions, tickets, seats, credentials, attendance records, impressions, conversions, messages, requests, characters, words, tokens, images, data volume, refresh frequency, support hours, incidents, replacements, and spend.

RWP-00.70 does not choose numeric values. A limit must not hide a capability, change authority, imply source access, alter privacy, or become a feature flag.

## Venue-group inheritance model

### Inheritance candidates

An organization, brand, region, campus, district, venue group, franchise, touring group, promoter, or operating company may offer inherited:

- approved templates, components, design libraries, terminology, language variants, campaigns, programs, schedules, destinations, integration configurations, reporting definitions, and governance policies;
- tier entitlements and purchased add-ons only where the commercial contract explicitly scopes them;
- default permissions and role templates without silently assigning users;
- limit allocations where the approved model supports pooled or venue-level allowances.

### Local-control requirements

Every venue must be able to identify:

- source and version of an inherited item;
- whether it is mandatory, recommended, copied, linked, or locally overridden;
- local identity, schedule, attraction, exhibit, event, queue, route, language, audience, target, source, and operational state affected;
- what changes before adoption;
- what remains local and what restores on rollback;
- mixed states and excluded or incompatible venues.

Current operational truth, urgent local notices, safety-related communication, source authority, privacy, rights, screen recovery, and last-known-good content cannot be silently replaced by inheritance.

## Upgrade behavior

An upgrade should:

- preserve all current content, venues, screens, users, roles, targets, sources, languages, schedules, delivery history, versions, and recovery points;
- present newly available customer outcomes rather than a feature dump;
- avoid changing active content, inheritance, target scope, source precedence, or public behavior automatically;
- support guided setup, preview, sample data, and later completion;
- keep independent add-ons separately selected and priced;
- explain limits, pooled allowances, dependencies, permission needs, and outage behavior;
- allow a customer to activate one advanced outcome at a time.

Industry or subtype change is not an upgrade and must not trigger commercial changes.

## Downgrade behavior and owner questions

A downgrade must never immediately stop essential visitor communication. Before final packaging, the owner must decide:

1. What grace period applies to coordinated screens, campaigns, approvals, maps, localization, portfolio scope, analytics, and enterprise administration?
2. Are advanced configurations frozen read-only, converted to simpler core objects, exported, or removed after the grace period?
3. How are active event sequences, recurring schedules, approval chains, campaigns, maps, inherited libraries, and cross-venue targets reduced safely?
4. Which venue owns copied or inherited content and state?
5. How are pooled limits redistributed?
6. What happens to history, audit, reports, dashboards, translations, analytics, and prediction retention?
7. Can customers retain advanced rendering while losing editing workflow, and for how long?
8. How are independent add-ons handled when companion native workflow is downgraded?
9. What impact reports, previews, notifications, and administrator acknowledgments are required?
10. How are active public screens protected from blank, stale, private, rights-expired, unsafe, or mis-targeted content?

Recommended default: preserve customer data and current safe delivery, stop creation of new advanced objects after a communicated grace period, provide export or conversion, retain manual fallback, and require explicit review before destructive cleanup.

## Pricing and upgrade presentation

Pricing should not interrupt initial venue setup or appear before the customer understands the manual core. Consistent with the accepted product direction, upgrade prompts should become contextual after the customer has established a viable venue and preferably activated a first screen. Earlier pricing access may remain available deliberately, but onboarding must not force plan comparison before operational value is clear.

Future surfaces should present outcomes first:

- **Operate:** communicate accurately every day;
- **Coordinate:** coordinate events, teams, screens, journeys, languages, and campaigns;
- **Portfolio:** govern multiple venues and brands;
- **Enterprise:** meet enterprise identity, audit, governance, and service requirements.

They must show what is included, which add-ons remain separate, which limits apply, what permission or connection is missing, what happens during outage and downgrade, and what manual fallback remains. Avoid disabled-control grids. Preserve the approved Sky Blue administrative direction, keyboard access, assistive technology, 200% zoom, localization expansion, right-to-left readiness, long-name handling, and phone through large desktop.

## Owner decisions required before approval

- final tier names and number of tiers;
- exact placement of advanced queue, maps, campaigns, localization, analytics, coordination, portfolio, and enterprise capabilities;
- numeric limits, pooling, overage, trial, grandfathering, and contract treatment;
- add-on prerequisites and minimum-tier administration requirements;
- inheritance and local-override policy;
- downgrade grace, read-only, conversion, export, retention, and deletion behavior;
- personalized, ticket-specific, member, participant, performer, sponsor, staff, security, and sensitive display privacy;
- source precedence, rights, safety, accessibility, sponsor, advertising, and records requirements;
- enterprise identity, managed hardware, connectivity, and service commercial structure;
- customer-facing pricing timing and upgrade presentation.

## Boundaries

This proposal is not final commercial approval. RWP-00.71 owns the Entertainment & Attractions onboarding experience and must not force a tier choice before the customer understands venue setup and has a viable path to a first active screen.
