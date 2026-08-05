# Unified Tier & Add-On Architecture

## Status

RWP-00.78 proposes a cross-industry commercial architecture for owner review. It defines outcome archetypes and independent add-on families without setting prices, trials, contracts, numeric limits, or implementing billing/entitlement behavior.

## Architecture principles

1. The complete essential manual operating core is included in every software tier.
2. Industry and subtype never determine commercial access.
3. Permissions, product state, privacy/rights, limits, and rollout remain separate from tier entitlement.
4. Advanced native Vennusign workflow may be tiered by customer outcome.
5. External systems, separately metered services, managed services, hardware, and HaaS remain independently attachable add-ons.
6. A tier or add-on grants access to a capability outcome, not authority to every object or action.
7. Current legacy slugs and feature keys may remain compatibility identifiers during migration; final customer names and stable capability IDs are separate decisions.
8. Pricing and integration discovery should not block first value. Onboarding reaches one confirmed useful screen before forced plan comparison or external connection setup.

## Proposed software outcome archetypes

The names below are working architecture names, not approved commercial names.

### Operate

**Customer outcome:** Run one or more allowed venues safely every day with reliable manual control.

Included normalized capabilities:

- industry/subtype-aware terminology, starter recommendations, and neutral fallback;
- manual content, product/service/program/event information, hours, schedules, state, notices, wayfinding, and ordinary public guidance;
- rapid manual availability/operating updates, including sold out, unavailable, limited, delayed, paused, closed, canceled, relocated, changed hours, next batch, and reopening context;
- manually authored language variants and accessibility-ready content;
- screen pairing/selection, purpose, exact targeting, preview, explicit publish, per-target delivery confidence, offline/outdated awareness, correction, supersession, expiry, unpublish, retry, undo, and restore;
- source identity/freshness/conflict/local-override visibility where a source exists;
- essential user and scope permissions;
- core screen, publication, delivery, content-freshness, exception, and recovery evidence;
- billing/security access and safe plan/add-on discovery after first value.

Operate does not imply unlimited venues, screens, users, history, or storage; applicable allowances remain separate limits. It also does not include an external integration merely because manual operation is core.

### Coordinate

**Customer outcome:** Plan and coordinate recurring work, richer presentation, and team workflows within a venue or small group.

Candidate capabilities added to Operate:

- recurring schedules, rotations, planned transitions, conflict detection, and advanced timing rules;
- reusable templates, content sets, promotions, campaigns, advanced layouts, and coordinated multi-screen presentation;
- approvals, assignments, acknowledgments, review queues, advanced history, and operational handoff workflow;
- advanced localization workflow, terminology libraries, quality review, and broader manually managed language portfolios;
- advanced native analytics, comparisons, alerts, and scheduled reports using Vennusign-owned evidence;
- safe bulk actions within an explicitly bounded local scope;
- advanced recovery/audit and reusable source/target configuration.

Coordinate does not include external provider connections by default. External data remains an add-on even when Coordinate provides richer workflow around it.

### Portfolio

**Customer outcome:** Govern and operate multiple venues/properties/operations with shared standards and local control.

Candidate capabilities added to Coordinate:

- organization templates, inheritance, local overrides, effective-value comparison, and safe rollback;
- cross-venue/property/operation campaigns, scheduling, content libraries, screen-purpose standards, and localization governance;
- portfolio exception dashboards, delivery/source health, advanced analytics, comparisons, and scheduled distribution;
- safe bulk actions with preview, mixed-result handling, per-site confirmation, and restoration;
- delegated administration, advanced role/scope policy, approvals, retained audit, and operational governance;
- mixed-industry organization support using canonical capability IDs and local industry terminology;
- cross-site capacity planning and limit visibility.

Portfolio commercial access never grants local authority automatically. User permissions and venue/property scope are evaluated independently.

### Enterprise

**Customer outcome:** Apply enterprise governance, identity, policy, data, assurance, and service controls at scale.

Candidate capabilities added to Portfolio:

- enterprise identity and directory integration administration, advanced role governance, policy controls, and delegated support boundaries;
- configurable retention, export, legal hold candidates, data-region/rights administration, audit assurance, and advanced security reporting;
- enterprise brand/localization governance and approved content controls;
- advanced data/BI administration, governed exports, metric catalogs, data-quality/reconciliation views, and enterprise reporting workflow;
- custom operational policy, advanced approval/escalation, support administration, and service-management visibility;
- negotiated service, deployment, monitoring, and administration features where they are native software outcomes rather than managed services.

Enterprise does not automatically include every external system, hardware bundle, custom service, or unlimited usage.

## Candidate capability placement

| Capability family | Operate | Coordinate | Portfolio | Enterprise |
| --- | --- | --- | --- | --- |
| Essential manual industry-aware operation | Included | Included | Included | Included |
| Pair/select, target, preview, publish, delivery confidence, recovery | Included | Included | Included | Included |
| Manually authored language variants and accessibility | Included | Included | Included | Included |
| Ordinary manual schedules/hours/state | Included | Included | Included | Included |
| Recurrence, rotations, advanced rules, conflict detection | — | Candidate | Included | Included |
| Templates, campaigns, advanced layouts, multi-screen coordination | — | Candidate | Included | Included |
| Approvals, assignment, acknowledgment, advanced history | — | Candidate | Included | Included |
| Advanced localization workflow and terminology governance | — | Candidate | Included | Included |
| Advanced native analytics and alerts | Core evidence only | Candidate | Included | Included |
| Organization inheritance, local override, safe bulk action | Local basics only | Limited candidate | Included | Included |
| Portfolio dashboards and mixed-industry governance | — | — | Included | Included |
| Enterprise identity/policy/data/retention governance | — | — | — | Candidate |

A dash means the advanced outcome is not included, not that the underlying core action disappears.

## Independent add-on catalog

### Commerce & Food Operations

Candidate connections/services:

- POS providers, inventory, production, ordering, payment, fulfillment, pickup/order-ready, tap management, loyalty, supplier, and commerce CRM.

Attachment scope candidates: organization, venue, outlet, operation, service point, menu/catalog, or provider account.

Required states: eligible, attached, configured, connected, synchronizing, stale, conflicted, disconnected, suspended, unsupported, and removed with manual fallback.

### Hospitality Systems

Candidate connections/services:

- property management, room booking, event/conference sales, transport, parking, guest service, amenity systems, access, gaming, and related local property systems.

Guest-specific or restricted data is never exposed on public screens without separately approved privacy/audience controls.

### Entertainment & Venue Systems

Candidate connections/services:

- ticketing, admissions, access control, queue/wait, occupancy/footfall, cinema, venue/show control, collection, attraction, event, sports, route/mapping, and transport systems.

Rights, source authority, coverage, freshness, uncertainty, privacy, and manual fallback are mandatory.

### Data, Environmental & Communication Sources

Candidate connections/services:

- weather, traffic, government/public feeds, safety notifications, calendars, directories, messaging, CRM, footfall, BI/data warehouse, and export destinations.

### Translation, AI & Metered Assistance

Candidate separately metered services:

- automated translation, copy assistance, image/content generation, prediction, recommendations, anomaly detection, optimization, and other AI processing.

Required controls include consumption visibility, source/generated labeling, privacy, review, retention, correction, export, and manual fallback.

### Identity & Enterprise Connections

Candidate external connections:

- identity provider, directory, HR/user provisioning, support/service management, and enterprise governance sources.

Native enterprise administration may be tiered; the external connection remains an add-on candidate.

### Hardware & Managed Services

Candidate independent services:

- HaaS bundles/contracts, purchased hardware, connectivity, installation, replacement, deployment, remote monitoring, managed content, managed localization, managed analytics, custom integration, premium support, and event/temporary deployment services.

HaaS remains a separate contract path and never silently changes software tier access.

## Add-on attachment model

Every add-on needs explicit records for:

- catalog identity and version;
- organization/venue/property/operation/object attachment scope;
- provider/region/rights eligibility;
- commercial status independent of configuration;
- administrator permission;
- provider account/source identity;
- configuration and credential state without exposing secrets;
- connection health, freshness, coverage, conflict, override, and last-known-good state;
- applicable typed limits and consumption;
- privacy/rights/safety obligations;
- manual fallback and disconnect behavior;
- cancellation/downgrade, retention, export, removal, and restoration;
- support responsibility and service status.

A tier may recommend an add-on or bundle commercial offers, but the add-on remains separately identifiable, attachable, removable, observable, and governable.

## Upgrade behavior

- Introduce plan/add-on discovery contextually after a useful first-screen outcome, not as an onboarding gate.
- Show customer outcomes rather than a feature-key list.
- Recheck server-authoritative eligibility immediately before hosted billing continuation.
- Distinguish required tier from required add-on and from a limit increase.
- Show current access, target outcome, gained capabilities, changed allowances, add-on prerequisites, billing path, provider confirmation, and effective timing.
- A provider return never grants access locally; current server/provider state remains authoritative.
- Preserve in-progress work when opening hosted review.

## Downgrade and cancellation behavior

Before a software-tier downgrade, show:

- capabilities moving to read-only, unavailable, or reduced workflow;
- typed usage conflicts and counted objects;
- active screens and public-impact risk;
- organization inheritance/local override impact;
- scheduled work, campaigns, templates, analytics, approvals, history, and exports affected;
- independently attached add-ons and whether prerequisites remain satisfied;
- grace/read-only/export/retention/removal choices defined later by RWP-00.79;
- server recheck and provider-authoritative completion.

Core manual correction, unpublish, active-screen safety, export where approved, and recovery must not be trapped by the downgrade.

Add-on removal must separately explain connection stop, last synchronized data, manual fallback, retained configuration, export/deletion, downstream content, and recovery.

## Mixed-industry organizations

- One organization may have multiple native industries and subtypes.
- Software tier access uses canonical capability IDs and applies according to attachment/inheritance policy, not industry labels.
- Each venue/property/operation preserves local terminology, starter content, objects, state, timezone, sources, and screen purposes.
- Portfolio and Enterprise may add cross-site inheritance/governance without flattening local differences.
- An add-on can attach only where the provider and object context are compatible.
- Neutral fallback presentation is required for shared cross-industry surfaces.

## Legacy migration principles

- Preserve current tier slugs (`starter`, `restaurant_starter`, `pro`, `business`) and feature keys as compatibility aliases until consumer/data migration is verified.
- New stable capability IDs must not embed a tier name or industry.
- Map current keys to normalized capability outcomes; do not infer access from browser catalog metadata.
- Migrate overloaded keys into explicit decisions while maintaining old responses during a bounded compatibility period.
- Do not delete dormant keys until server, data, tests, support, and customer records are verified.

## Impeccable commercial presentation guidance

Plan comparison and locked surfaces must:

- begin with the customer outcome and current context;
- distinguish software tier, independent add-on, limit increase, permission, and unavailable product state;
- show a useful read-only preview only where privacy/rights allow;
- use persistent labels and specific actions such as “Review Coordinate,” “Connect POS,” “Ask an organization owner,” or “Archive 2 screens” rather than generic “Unlock”;
- keep access and pricing secondary during first-value onboarding;
- support keyboard navigation, safe focus, screen readers, reduced motion, long names, localization expansion, mobile layout, and 200% zoom;
- preserve exact intended and effective dates and server-authoritative pending/completed states;
- avoid false urgency, hidden consequences, or implying that an upgrade fixes a permission, source, state, or rollout problem.

## Owner decisions required later

- final tier names and exact placement of candidate capabilities;
- pricing, trials, contracts, annual rules, grandfathering, promotions, and taxes;
- whether any add-ons are bundled or discounted while remaining separately modeled;
- provider, region, rights, prerequisites, service levels, and support responsibility;
- final allowance values and overage/grace policy;
- enterprise/custom capability versus managed-service boundary;
- migration treatment for current tiers, feature keys, overrides, and customer contracts.

## Handoff

RWP-00.79 defines the unified limit, attachment scope, inheritance, override, capacity enforcement, downgrade, and exception-governance policy used by these tier and add-on archetypes.
