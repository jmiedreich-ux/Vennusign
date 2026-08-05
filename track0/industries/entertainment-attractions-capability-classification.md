# Entertainment & Attractions Capability Classification

## Authority

This documentation-only companion completes RWP-00.69. It consolidates RWP-00.63 through RWP-00.68 and assigns one primary Track 0 classification to every Entertainment & Attractions concern. Relationships in the remaining columns do not replace the primary classification.

## Classification rules

- **Core capability:** essential behavior available for safe daily operation.
- **Permission:** authority to view, change, approve, publish, restore, target, or administer.
- **Product/domain state:** represented customer, venue, content, operational, source, delivery, or configuration value.
- **Tier entitlement:** recurring native Vennusign outcome bundled commercially above core.
- **Independent add-on:** independently purchased integration, managed service, or specialized consumption-backed capability.
- **Usage or quantity limit:** allowance or consumption boundary only.
- **Internal rollout flag:** temporary release control only.

Industry, subtype, terminology, hierarchy, content, operational values, source values, and delivery values are never feature flags. Permissions never grant commercial access. Limits never create capabilities.

## Consolidated classification

| Concern | Primary classification | Related tier/add-on/limit/permission/state relationship |
| --- | --- | --- |
| Industry selection | Product/domain state | Permission controls configuration; does not grant commercial access |
| Primary subtype and descriptive traits | Product/domain state | Seeds recommendations only; does not stack entitlements or limits |
| Terminology preference and neutral fallback | Product/domain state | Authorized manual terminology configuration remains core |
| Customer-authored names and local vocabulary | Product/domain state | Validation and permission control change; preserve on downgrade |
| Venue, campus, district, building, floor, zone, area, auditorium, stage, screen, gate, section, lane, field, court, track, attraction, exhibit, habitat, route, event, session, queue, and admission-window hierarchy | Product/domain state | Edit authority is permission; object counts may be limits |
| Venue local time zone and operating-day boundary | Product/domain state | Required for core schedule presentation |
| Venue, area, attraction, exhibit, admission, event, service, and last-entry hours | Product/domain state | Manual authoring and publication are core |
| Program, production, film, exhibition, collection, attraction, tour, activity, game, match, and event identity | Product/domain state | Manual create/edit/publish remains core |
| Performance, screening, show, session, round, departure, tour, talk, feeding, demonstration, class, timed-entry, and other occurrence | Product/domain state | Manual scheduling and disruption communication remain core |
| Start, end, duration, doors, boarding, seating, check-in, final occurrence, last entry, and closing values | Product/domain state | Must not infer unsupported timing |
| Availability, open, limited, full, sold out, entry paused, delayed, paused, unavailable, closed, canceled, relocated, weather, maintenance, restriction, reopening, resumed, and unknown values | Product/domain state | Authorized manual change and publication remain core |
| Queue state | Product/domain state | Manual queue communication core; live measurement optional |
| Wait-time value, range, qualitative label, freshness, and unknown state | Product/domain state | Manual entry core; sensors/prediction optional |
| Capacity and occupancy state | Product/domain state | Manual aggregate state core; exact live measurement optional |
| Admission, ticket, pass, membership, reservation, credential, guest-list, timed-entry, reserved-seat, standing, participant, and restricted-access context | Product/domain state | Public display must remain privacy-safe; external sync optional |
| Accessibility, language, rating, age, height, seating, participation, arrival, route, and assistance guidance | Product/domain state | Display only when venue-authored or source-authoritative |
| Operational notice, public wording, priority, audience, effective period, expiry, supersession, and restoration point | Product/domain state | Manual notice operation core; high-scope permission applies |
| Source identity, source authority, freshness, connection, conflict, stale, disconnected, overridden, partial, and unknown state | Product/domain state | External connection is add-on; override authority is permission |
| Target venue, area, object, group, purpose, screen, and language | Product/domain state | Targeting operation core; target authority is permission |
| Draft, scheduled, active, published, expired, superseded, corrected, restored, and archived content version | Product/domain state | Core lifecycle operation; retained versions may be limited |
| Accepted, pending, failed, partial, online, offline, outdated, unknown, and last-known-good delivery state | Product/domain state | Basic delivery confidence core; managed monitoring optional |
| Manual venue and visitor information | Core capability | View/edit/publish permissions apply |
| Manual program, schedule, event, show, screening, exhibit, attraction, and session authoring | Core capability | External automation remains optional |
| Manual closure, delay, pause, cancellation, relocation, restriction, and reopening communication | Core capability | Source and effective state remain product values |
| Manual queue, wait, capacity, and admission guidance | Core capability | Dynamic measurement and synchronization remain optional |
| Manual destination-based wayfinding and temporary accessible-route guidance | Core capability | Interactive maps and positioning remain optional |
| Manual notices and safety-related public communication | Core capability | Does not define emergency policy or life-safety behavior |
| Basic manual multilingual content, language labeling, coverage gaps, and fallback | Core capability | Premium translation workflow and automation optional |
| Accessible content authoring and presentation safeguards | Core capability | Premium audit/workflow may be tier; compliance decisions remain external |
| Exact screen targeting and contextual preview | Core capability | Target and publish authority are permissions |
| Immediate or supported scheduled publication | Core capability | Scheduling authority is permission; schedule state is product state |
| Delivery confirmation, failed/partial visibility, offline/outdated awareness, retry, correction, supersession, expiry, unpublish, and restoration | Core capability | Advanced remote monitoring/support optional |
| Source/freshness/conflict awareness and reversible manual override | Core capability | External source connection is add-on; override is permission |
| Content and state preservation through subtype, tier, connection, or terminology change | Core capability | Quantity retention and export policy require owner decision |
| View venue/public content | Permission | Commercial entitlement does not automatically grant audience scope |
| Create and edit content or represented state | Permission | Core capability may exist while user lacks authority |
| Schedule content | Permission | Schedule state remains product/domain state |
| Approve or reject changes | Permission | Advanced multi-step approval workflow may be tier |
| Publish, unpublish, expire, supersede, correct, restore, or retry | Permission | Operation may be core; authority remains permission |
| Target screens, groups, purposes, audiences, areas, venues, and languages | Permission | Target scope remains product/domain state |
| Manage venue hierarchy, terminology, subtype, languages, sources, integrations, templates, and organization configuration | Permission | Commercial access and limits remain separate |
| Access restricted, staff, performer, sponsor, security, member, participant, or sensitive operational information | Permission | Privacy and audience decisions required |
| Dynamic queue, wait, occupancy, capacity, footfall dashboards, rules, coordination, and prediction | Tier entitlement | Sensor, queue, access, footfall, or prediction source is add-on; data limits apply |
| Native interactive map, route, destination, kiosk, and multi-building wayfinding authoring | Tier entitlement | Mapping/positioning/parking/transit connections are add-ons; map limits apply |
| Coordinated screen groups, zones, event moments, takeovers, sequences, and estate rollback | Tier entitlement | Screen/zone/sequence limits apply; high-scope authority is permission |
| Campaign, promotion, membership, sponsorship, fundraising, retail, merchandising, and cross-sell workflow | Tier entitlement | CRM/loyalty/advertising/ecommerce/POS connections add-ons; campaign limits apply |
| Multi-venue sharing, portfolio coordination, inheritance, comparison, and bulk rollback | Tier entitlement | Venue/group/template/user/history limits apply |
| Brand systems, locked regions, advanced templates, asset rights, creative governance, and drift reporting | Tier entitlement | Premium creative services/assets may be add-ons; asset limits apply |
| Multi-step approval, assignment, acknowledgment, escalation, shift handoff, and retained audit workflow | Tier entitlement | External workflow/records systems add-ons; history limits apply |
| Translation workflow, glossary, translation memory, coverage governance, and locale validation | Tier entitlement | Automated translation/vendor connection add-on; language/character limits apply |
| Premium analytics, benchmarking, cohorts, scheduled reporting, BI access, prediction, and optimization | Tier entitlement | External data/BI sources add-ons; data/history/query/export limits apply |
| Enterprise SSO, provisioning, delegated administration, group mapping, domain/session policy, and security export | Tier entitlement | External identity provider or managed setup may be add-on; user/group/domain limits apply |
| Ticketing, box office, admissions, membership, reservation, seat inventory, guest-list, credential, turnstile, and access-control synchronization | Independent add-on | Native workflow may be tier; connection/event/transaction limits apply |
| Cinema, venue, show-control, collection, attraction, event, sports, team, league, promoter, production, and rights-holder synchronization | Independent add-on | Connection, venue, event, session, attraction, and consumption limits apply |
| Queue, footfall, occupancy, access, sensor, camera, or measurement source | Independent add-on | Native dashboards/rules may be tier; device/data limits apply |
| Mapping, indoor positioning, parking, transit, transport, weather, and route source | Independent add-on | Native map workflow may be tier; connection/request limits apply |
| CRM, loyalty, membership, donor, advertising, sponsor, ecommerce, retail, POS, merchandise, and conversion source | Independent add-on | Native campaigns may be tier; connection/transaction limits apply |
| External translation provider or machine translation | Independent add-on | Native localization workflow may be tier; character/request limits apply |
| AI-assisted content, translation, detection, summarization, recommendation, and operational assistance | Independent add-on | May appear in premium bundle; request/token/model limits apply; human review mandatory |
| Managed displays, players, kiosks, mounts, installation, replacement, enrollment, cellular connectivity, monitoring, remote management, support, and service levels | Independent add-on | Device/site/data/support limits apply; basic pairing and health remain core |
| Screen allowance | Usage or quantity limit | Does not grant targeting, publishing, monitoring, or coordination capability |
| Venue, property, campus, district, area, attraction, exhibit, event, session, queue, user, role, group, language, template, asset, campaign, integration, connection, sensor, map, route, report, transaction, history, storage, request, token, device, data, or support allowance | Usage or quantity limit | Counting rules require owner approval; limits do not transfer authority |
| Temporary exposure of unfinished or controlled functionality | Internal rollout flag | Cannot represent availability, entitlement, permission, product state, or limit |

## Ambiguities resolved

1. **Manual versus automated wait time:** the manual wait value and communication operation are core/product state; measurement or prediction sources are add-ons; advanced native coordination is tier.
2. **Capacity versus sold out:** capacity/occupancy state and saleable admission inventory are separate product values; external inventory synchronization is add-on.
3. **Maps versus wayfinding:** destination-based manual guidance is core; native interactive maps are tier; external map/positioning sources are add-ons.
4. **Approval versus permission:** approval workflow is tier; the authority to approve is permission.
5. **Screen health versus managed monitoring:** basic publication and offline/outdated visibility are core; proactive estate monitoring and service are tier/add-on candidates.
6. **Multilingual versus premium localization:** manual alternate-language content is core; governance workflow is tier; automated translation is add-on.
7. **Analytics versus source data:** core publication and freshness metrics remain core; advanced analysis is tier; external attendance/footfall/ticket data is add-on.
8. **AI versus content state:** AI access is add-on; generated drafts are reviewable product state; publish authority remains permission.
9. **Enterprise identity versus authorization:** enterprise administration is tier; authentication and authorization values remain security/product state and permissions.
10. **Subtype versus packaging:** subtype changes presentation and defaults only; it never grants tier or add-on access.

## Impeccable planning result

Future commercial and administrative surfaces must explain classification through customer outcomes rather than internal labels. They must keep the included manual path visible, distinguish unavailable from not permitted, not purchased, not configured, disconnected, stale, unsupported, limit reached, and internally disabled, and show scope, dependencies, pricing context, limits, outage behavior, downgrade behavior, and recovery without creating a disabled-control maze. Preserve the approved Sky Blue administrative direction.

## Owner decisions carried forward

Final tier bundles, pricing, trials, upgrade/downgrade timing, proration, cancellation, retention, export, counting rules, source precedence, privacy, rights, safety, sponsor, accessibility, advertising, and mixed-portfolio behavior remain for later approved RWPs and owner approval.
