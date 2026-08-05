# Entertainment & Attractions Optional Capabilities

## Authority

This documentation-only companion completes RWP-00.68. It identifies advanced workflows, integrations, services, and scale capabilities that may be packaged above required core operation. Nothing in this document removes or weakens the manual core defined by RWP-00.67.

## Packaging rules

- Required manual operation remains core in every customer tier.
- A tier bundles recurring customer outcomes available natively in Vennusign.
- An add-on represents an independently purchased integration, managed service, or specialized capability that can vary by venue or organization.
- A limit controls quantity or consumption; it does not create a capability.
- A permission controls authority; it does not determine commercial access.
- Product state and source data must never be implemented as feature flags.
- Industry and subtype change defaults and recommendations only.

## Optional capability families

### 1. Ticketing, admissions, reservations, and access synchronization

Potential integrations include ticketing, box office, admissions, membership, reservation, timed-entry, seat inventory, guest-list, credential, turnstile, and access-control systems.

Possible outcomes:

- synchronized events, performances, screenings, sessions, tours, and entry windows;
- sold-out, limited, full, entry-paused, boarding, seating, or check-in states from an authoritative source;
- public display of safe aggregate admission guidance;
- conflict detection between imported and manual values;
- source health, freshness, override, fallback, and recovery.

**Primary candidate:** independent add-on. A broader coordinated-operations tier may expose workflow around the connection, but the external connection remains independently purchasable. Connection, venue, event, session, transaction, seat, member, credential, and consumption counts are limits.

### 2. Venue, cinema, show-control, collection, attraction, event, and sports synchronization

Potential sources include cinema scheduling, venue management, show control, collection management, attraction management, event management, sports scheduling/scoring, team, league, promoter, production, and rights-holder systems.

Possible outcomes:

- synchronized program and occurrence identity;
- auditorium, stage, room, field, court, track, gate, section, attraction, exhibit, habitat, or route assignment;
- schedule changes, delays, cancellations, relocations, score/event state, attraction availability, and public-safe notices;
- source precedence, manual override, partial synchronization, and restoration.

**Primary candidate:** independent add-on. Connection and consumption limits apply. Imported data remains product/domain state with visible source and freshness.

### 3. Dynamic queue, wait-time, occupancy, capacity, and footfall

Advanced capability may ingest or calculate:

- queue length, wait estimate, throughput, virtual-queue state, and lane distribution;
- aggregate occupancy, capacity utilization, entrance/gate flow, dwell, and visitor movement;
- current and predicted crowding, surge, intermission, halftime, or egress conditions;
- operator thresholds and recommended display changes.

**Primary candidate:** tier entitlement for native dashboards, rules, and coordination; sensor, queue, footfall, access, or prediction connections are add-ons. Data points, venues, zones, sensors, events, retained history, and consumption are limits.

Predictions must show source, confidence, freshness, and uncertainty. Manual state and safe fallback remain core.

### 4. Advanced wayfinding, maps, positioning, and journey guidance

Advanced capability may provide:

- interactive maps and destination directories;
- temporary-route overlays and accessible-route variants;
- entrance, gate, section, auditorium, gallery, attraction, habitat, parking, transport, service, and exit guidance;
- kiosk interaction, QR handoff, mobile continuation, indoor positioning, and location-aware content;
- multi-building, campus, park, district, or event-site navigation.

**Primary candidate:** tier entitlement for native map and route authoring; mapping, positioning, parking, transit, or venue-map connections are add-ons. Map, floor, route, destination, device, request, and storage limits apply.

Manual destination-based wayfinding remains core.

### 5. Coordinated screens, zones, takeovers, and event moments

Advanced coordination may support:

- synchronized screen groups and timed transitions;
- venue-wide or area-specific event moments;
- pre-event, arrival, doors, start, intermission, halftime, post-event, and egress sequences;
- priority overlays, approved sponsor content, temporary takeovers, and restoration;
- conflict, target, delivery, and rollback control across large estates.

**Primary candidate:** tier entitlement. Screen, zone, sequence, campaign, event, template, retained-version, and concurrent-publication limits may apply. High-scope publish and takeover authority remains permission.

### 6. Campaigns, promotions, membership, sponsorship, and merchandising

Advanced campaigns may coordinate:

- event, exhibition, attraction, membership, pass, season, fundraising, retail, food-and-beverage, merchandise, sponsor, partner, and cross-sell content;
- audience-safe scheduling and targeting;
- reusable campaign structures, variants, approvals, expiry, and reporting;
- attribution only when a permitted authoritative source exists.

**Primary candidate:** tier entitlement. Advertising network, CRM, loyalty, membership, ecommerce, donor, retail, POS, or sponsor-system connections are add-ons. Campaign, audience, template, asset, impression, conversion, storage, and history limits may apply.

Required visitor information cannot be displaced or paywalled by campaigns.

### 7. Multi-venue sharing, portfolio coordination, and content inheritance

Organizations may optionally coordinate:

- shared templates, brand assets, terminology, languages, campaigns, programs, integrations, reporting, and administration;
- venue groups, districts, campuses, touring productions, franchises, museums, parks, cinemas, sports estates, and mixed portfolios;
- approved organization defaults with local override and mixed-state visibility;
- copy, publish, schedule, compare, and rollback across selected venues.

**Primary candidate:** tier entitlement. Venue, group, property, screen, template, language, campaign, user, integration, storage, and retained-history limits apply. Group membership does not imply ownership, authority, privacy scope, or commercial access.

### 8. Brand systems, advanced templates, and creative governance

Optional capability may include:

- organization design systems and locked brand regions;
- template libraries, reusable modules, campaign kits, variants, and controlled local fields;
- asset approval, rights, expiry, usage, and replacement;
- accessibility and environmental validation across screen sizes and placements;
- brand-level comparison and drift reporting.

**Primary candidate:** tier entitlement. Premium creative services or licensed asset libraries may be add-ons. Template, brand, asset, storage, history, and export limits apply.

Basic layouts, themes, accessible presentation, and manual content remain core.

### 9. Approval, assignment, acknowledgment, escalation, and audit workflow

Advanced governance may provide:

- multi-step approval and separation of duties;
- change requests, assignments, acknowledgments, due dates, escalation, and shift handoff;
- event, sponsor, rights-holder, safety, accessibility, language, and brand review paths;
- retained audit, comparison, export, and evidence.

**Primary candidate:** tier entitlement. External workflow, legal, rights, safety, or records systems may be add-ons. Approver, workflow, retained-history, export, and storage limits apply. Authority remains permission.

### 10. Premium localization, translation operations, and terminology governance

Optional capability may provide:

- translation workflow, assignments, review, approval, glossary, translation memory, and coverage reporting;
- locale-specific dates, times, numbers, pluralization, expansion, and right-to-left validation;
- shared terminology across venues with local exceptions;
- external translation vendor or machine-translation connections.

**Primary candidate:** tier entitlement for workflow and governance; automated translation and vendor connections are add-ons. Language, locale, word/character, request, glossary, memory, retained-version, and AI-consumption limits apply.

Basic manual multilingual content remains core.

### 11. Premium analytics, benchmarking, prediction, and optimization

Optional analytics may cover:

- schedule, program, attraction, exhibit, event, queue, wait, capacity, attendance, footfall, screen, campaign, content, delivery, and venue performance;
- multi-venue comparison, trends, cohorts, benchmarking, export, scheduled reports, and external BI access;
- prediction and optimization for queues, content, staffing signals, schedules, campaigns, and screen allocation where data quality permits.

**Primary candidate:** tier entitlement. Ticketing, attendance, footfall, POS, advertising, membership, weather, transport, or BI connections are add-ons. Metric, event, venue, data-source, retained-history, query, export, and consumption limits apply.

Core screen health, publish result, and current notice freshness remain core analytics.

### 12. AI-assisted content and operations

Optional AI may assist with:

- draft visitor notices, summaries, variants, translations, schedules, templates, image descriptions, and campaign copy;
- detecting conflicting, stale, incomplete, inaccessible, or risky content;
- suggesting target, timing, route, language, or restoration actions;
- summarizing multi-venue exceptions and operational changes.

**Primary candidate:** independent add-on or explicit premium tier component. Requests, tokens, images, languages, retained context, storage, and model access are limits.

AI output is never authoritative by default. Human review, source visibility, permission, privacy, rights, safety, rollback, and manual operation remain mandatory.

### 13. Enterprise identity, provisioning, and administration

Optional enterprise capability may include:

- SSO, SAML/OIDC, SCIM, directory synchronization, group mapping, delegated administration, domain control, conditional access, and session policy;
- venue, team, contractor, promoter, tenant, sponsor, and seasonal-worker administration;
- centralized security and audit exports.

**Primary candidate:** tier entitlement. External identity provider or managed onboarding services may be add-ons. User, group, domain, connection, role, session, and audit-history limits may apply. Authentication state and authorization remain separate from commercial access.

### 14. Managed hardware, connectivity, deployment, and support

Optional managed services may include:

- commercial displays, players, kiosks, mounts, installation, replacement, device enrollment, remote management, cellular connectivity, network monitoring, proactive support, and service levels;
- outdoor, high-brightness, low-light, interactive, protected, or specialty hardware;
- event, touring, temporary, and seasonal deployment services.

**Primary candidate:** independent add-on or managed-service package. Device, site, screen, data, support, installation, replacement, and service limits apply.

Basic pairing, online/offline awareness, outdated detection, publication confirmation, and restoration remain core.

## Optional presentation by subtype

Optional capabilities should be presented by customer outcome rather than an undifferentiated feature list. Examples:

- Cinema: ticketing/cinema synchronization, coordinated lobby/auditorium screens, campaigns, analytics.
- Museum/Gallery: collection/exhibition systems, maps, multilingual workflow, membership/donor campaigns.
- Zoo/Aquarium/Park: attraction state, dynamic waits, maps, weather/route feeds, portfolio coordination.
- Sports/Live Event: ticketing/access, event systems, coordinated venue moments, sponsorship, egress analytics.
- Family Entertainment/Bowling/Arcade: reservations, lane/activity systems, queue/capacity, campaigns, POS or loyalty connections.
- Attraction/Tour: reservations, departure systems, maps, languages, weather, transport, and route coordination.

Subtype presentation does not grant entitlement.

## Impeccable planning result

Future optional-capability selection is an **Operate** and **Persuade** boundary: operators need clear operational impact while buyers need transparent commercial choices. The experience must state what outcome is added, what remains core, what data or external system is required, which venue scope is affected, which permissions remain necessary, what counts against a limit, what happens during outage or downgrade, and how the capability can be removed without losing customer-authored content or manual operation.

Avoid a wall of disabled controls. Present optional capabilities when the customer has enough context to understand them, preserve the approved Sky Blue administrative direction, and cover loading, unavailable, permission, trial, enabled, connection-required, disconnected, stale, limit-reached, downgrade, cancellation, and recovery states.

## Owner decisions carried forward

- final tier names, price points, trial rules, upgrade/downgrade timing, proration, cancellation, retention, and export;
- which integrations are individually purchasable versus included in bundles;
- exact venue, event, screen, user, connection, transaction, data, history, storage, AI, hardware, and support limits;
- source precedence and manual override policy;
- personalized or ticket-specific display privacy;
- legal, safety, rights, sponsor, accessibility, advertising, and records requirements;
- mixed-organization, campus, district, resort, casino, arena, cultural, touring, and franchise behavior.
