# Track 0 Capability Matrix

This matrix is the normalized cross-industry classification record produced by RWP-00.75. Restaurant is the inherited baseline; native industries add only meaningful terminology, object, state, operating-rhythm, default, screen-purpose, dashboard, analytics, and external-system differences.

Each concern has exactly one primary classification. Relationship columns do not replace that classification.

| Capability or concern | Normalized behavior | Primary classification | Tier relationship | Add-on relationship | Limit relationship | Permission/state relationship | Decision status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Manual content and local terminology | Authorized operators create and maintain clear customer-authored public content and neutral or industry-aware terminology. | Core capability | Advanced brand/localization governance may be tier | Translation/AI services may be add-on | Character, language, template, or history allowances may apply | Permission controls authoring/publishing; names and language are product state | Core approved |
| Industry, subtype, and descriptive traits | Seed defaults, labels, starter content, screen-purpose suggestions, dashboard emphasis, and analytics presentation. | Product/domain state | None | None | None | Permission controls configuration; does not change commercial access | Non-commercial approved |
| Organization, venue/property/operation, hierarchy, and context | Represent local operating scope across Restaurant, Bar, Café, Food Truck, Hospitality, and Entertainment. | Product/domain state | Portfolio coordination may be tier | External hierarchy sources may be add-on | Venue, property, operation, object, or group counts may apply | Ownership, authority, privacy, entitlement, and state remain separate | Counting deferred |
| Customer-authored names and content | Preserve local names and content unless invalid, unsafe, privacy-sensitive, rights-restricted, or superseded by an authoritative source. | Product/domain state | Governance workflow may be tier | Managed content may be add-on | Storage/history may apply | Permission and validation govern changes | Approved |
| Availability and operating-state values | Represent available, unavailable, sold out, limited, delayed, paused, closed, canceled, relocated, weather-affected, reopening, unknown, batch, freshness, queue, capacity, admission, service, and comparable bounded values. | Product/domain state | Planned automation may be tier | Authoritative synchronization may be add-on | Object/state history may apply | Authorized core actions mutate state; state is never a feature flag | Approved |
| Manual rapid update | Change ordinary availability, hours, service, event, notice, location, queue/capacity/admission guidance, closure, delay, relocation, and reopening information. | Core capability | Advanced rules and recurring automation may be tier | External automation may be add-on | Resource allowances may apply | Permission controls actors; correction and restore remain available | Core approved |
| Schedule and operating information | Maintain ordinary hours, service periods, events, sessions, batches, programs, routes, and planned public information manually. | Core capability | Recurrence, conflict detection, rotations, and orchestration may be tier | Calendar/event/source synchronization may be add-on | Schedule/event counts may apply | Planned, current, changed, and source states remain distinct | Core approved |
| Targeting and preview | Select exact organization/venue/context, audience, object, screen, and target; preview intended content before publication. | Core capability | Advanced reusable target groups may be tier | None | Screen/target/group counts may apply | Permission, privacy, and target state govern access | Core approved |
| Screen pairing, selection, and purpose | Pair or select a screen, assign a clear purpose, and preserve identity and content across configuration changes. | Core capability | Advanced multi-screen coordination may be tier | Managed hardware/connectivity may be add-on | Screen/device counts may apply | Pairing, online, delivery, and acknowledgement remain distinct state | Core approved |
| Publication and delivery confidence | Publish explicitly and show intended versus accepted/queued/pending/offline/failed/delivered state without inventing player acknowledgement. | Core capability | Advanced approvals/campaigns may be tier | Monitoring/managed operations may be add-on | Publication/history/retention limits may apply | Publish authority is permission; delivery is system/product state | Core approved |
| Source identity, freshness, conflict, and override | Show authoritative source, freshness, stale/disconnected state, local override, conflict, and last-known-good value. | Core capability | Advanced source governance may be tier | External connections are add-ons | Connection/history/data limits may apply | Override permission and represented source state remain separate | Core approved |
| Correction, supersession, expiry, unpublish, retry, undo, and restore | Preserve safe recovery for ordinary operations and partial/failing delivery. | Core capability | Advanced retained history/workflow may be tier | Managed recovery may be add-on | History/retention may apply | Destructive/high-scope actions require permission and review | Core approved |
| Basic accessibility and manually authored language variants | Provide accessible, responsive, keyboard and assistive-technology-compatible operation and manually maintained alternate-language content. | Core capability | Advanced localization workflow may be tier | Automated translation may be add-on | Language/content allowances may apply | Review/publish permission remains separate | Core approved |
| Advanced planning and native automation | Recurring schedules, rotations, rules, conflict detection, coordinated transitions, and advanced planning. | Tier entitlement candidate | Yes | External triggers may be add-on | Schedule/rule/action limits may apply | Permission and product state remain separate | Bundle deferred |
| Campaigns, templates, advanced presentation, and multi-screen coordination | Reusable campaigns/templates, advanced layouts, coordinated canvases, brand libraries, and presentation governance. | Tier entitlement candidate | Yes | Managed design/content may be add-on | Template/layout/campaign/screen limits may apply | Permission and content state remain separate | Bundle deferred |
| Workflow, approvals, assignment, audit, and retained history | Approval chains, assignment, acknowledgment, escalation, governance, and advanced audit/history. | Tier entitlement candidate | Yes | Managed service may be add-on | User/approver/history/retention limits may apply | Permission does not equal tier access | Bundle deferred |
| Multi-venue/property/operation portfolio control | Inheritance, local overrides, safe bulk actions, cross-site dashboards, and enterprise administration. | Tier entitlement candidate | Yes | External portfolio systems may be add-on | Site/user/template/history limits may apply | Group membership does not imply authority or entitlement | Bundle deferred |
| Advanced native analytics | Trends, comparisons, alerts, scheduled reports, portfolio analysis, and optimization using Vennusign-owned evidence. | Tier entitlement candidate | Yes | External data/BI may be add-on | Report/history/export limits may apply | Metric/source/privacy/permission state remains separate | Bundle deferred |
| POS, inventory, production, ordering, payment, fulfillment, tap, loyalty, supplier, and CRM synchronization | Synchronize authoritative food-and-beverage or customer systems while preserving manual fallback. | Independent add-on candidate | May be presented with tiers but remains attachable | Yes | Connections, transactions, sync, storage, or consumption may apply | Source, freshness, privacy, permission, and disconnect state required | Packaging deferred |
| Property, room, event, transport, parking, guest-service, access, and gaming systems | Synchronize Hospitality systems without exposing private guest state by default. | Independent add-on candidate | May be bundled commercially later | Yes | Property/event/connection/transaction limits may apply | Authorization, privacy, source authority, override, and fallback required | Packaging deferred |
| Ticketing, admissions, access control, queue/occupancy/footfall, venue/show control, cinema, attraction, event, sports, and mapping systems | Synchronize Entertainment systems without replacing required manual visitor communication. | Independent add-on candidate | May be bundled commercially later | Yes | Venue/event/session/transaction/source limits may apply | Rights, privacy, freshness, disconnect, and fallback required | Packaging deferred |
| AI, automated translation, prediction, and metered assistance | Provide separately governed generated or processed outputs that remain reviewable. | Independent add-on candidate | Some native workflow may be tier; metered service remains separate | Yes | Consumption, language, token, character, or request limits may apply | Permission, source labeling, privacy, review, and product-state ownership required | Packaging deferred |
| Managed hardware, HaaS, connectivity, installation, monitoring, content, analytics, and support | Deliver separately contracted physical or managed services. HaaS remains separate from software subscriptions. | Independent add-on candidate | No automatic software entitlement | Yes | Device, site, data, support, term, and service limits may apply | Contract/service state and administration permission remain separate | Packaging deferred |
| Venue, screen, user, object, schedule, event, source, integration, history, storage, export, transaction, support, and consumption allowances | Constrain quantity or usage without granting a capability. | Usage or quantity limit | Tier may set allowance | Add-on may extend allowance | Yes | Limit reached differs from permission, lock, state, disconnect, unsupported, and rollout conditions | Exact values deferred |
| Internal release controls | Stage, test, disable, or roll out product behavior temporarily. | Internal rollout flag | Not a customer tier | Not an add-on | Not a customer limit | Must not represent product state, permission, or commercial access | Internal only |

## Cross-industry inheritance rules

- Restaurant is the fallback baseline.
- Organization primary industry and local subtype affect presentation and recommendations only.
- Mixed-industry organizations preserve local terminology, timezones, authored content, screens, history, authority, sources, entitlements, add-ons, and limits.
- Organization defaults may seed but never silently overwrite local values.
- Local overrides are explicit, visible, reversible, and scoped; removing an override reveals the inherited value.
- Industry/subtype changes preview effects and preserve content and authority unless a separately reviewed migration is approved.

## State-presentation rule

Future product surfaces must distinguish:

- included and available;
- commercially locked;
- permission-restricted;
- unavailable or closed product state;
- unconfigured add-on;
- disconnected or stale source;
- unsupported context;
- usage or quantity limit reached;
- privacy/rights restriction;
- rollout-controlled or temporarily unavailable behavior.

Each state needs a truthful explanation, appropriate action, accessible semantics, and recovery path. Locked UI never becomes the authority; server-resolved permission and entitlement remain authoritative.

## Completed native-industry profiles

- Bar, Brewery & Nightlife — RWP-00.26
- Café, Bakery & Dessert — RWP-00.38
- Food Truck & Concession — RWP-00.50
- Hospitality — RWP-00.62
- Entertainment & Attractions — RWP-00.74

## Consolidation status

RWP-00.75 normalized the matrix and removed duplicate industry restatements. Final tier placement, add-on catalog, exact limits, inheritance policy, downgrade behavior, and implementation order remain pending RWP-00.76 through RWP-00.81 and explicit owner approval.

## Matrix rules

- One concern has one primary classification.
- Relationship columns never replace the primary classification.
- Essential manual operation remains core.
- Industry and subtype are not entitlements.
- Permission is not commercial access.
- Product state is not a feature flag.
- External systems and separately delivered services are add-on candidates.
- Counts and consumption are limits, not capabilities.
- Rollout flags remain internal.
