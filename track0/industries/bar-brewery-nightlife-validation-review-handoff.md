# Bar, Brewery & Nightlife Validation, Review & Handoff

## Result

RWP-00.15 through RWP-00.25 have been reviewed as one coherent Bar, Brewery & Nightlife Track 0 profile. The profile is complete, internally consistent, and ready for owner review and cross-industry consolidation.

No blocking gap, classification collision, subtype-entitlement coupling, permission-entitlement confusion, or silent product-implementation authorization remains. Essential manual daily operation remains core. The remaining questions are owner decisions for cross-industry packaging, prices, trials, numeric limits, add-on strategy, policy/privacy, external data, and later implementation.

## Canonical profile

Restaurant remains the inherited baseline. Bar adds only meaningful differences for beverage-led content, rapid tap and availability changes, cross-midnight service periods, happy hours and releases, events and entertainment, general entry guidance, multiple venue areas, and late-night operation.

The canonical primary subtypes are:

1. Pub
2. Sports Bar
3. Cocktail Bar
4. Wine Bar
5. Brewery
6. Brewpub
7. Taproom
8. Nightclub
9. Lounge

`Unspecified / General Bar` is the neutral fallback. Music, live entertainment, food-led operation, tasting, production, sports emphasis, reservations, private events, and multi-room operation are optional traits or operating emphases. They do not create additional primary subtypes, entitlements, permissions, or limits. Brewery and Brewpub remain distinct.

## Classification validation

### Core capabilities

- Manual drink, tap, cocktail, wine, optional food, special, release, event, entry, venue-information, and screen-content management.
- Quick Update for available, unavailable, sold-out, limited, and current operating communication.
- Current hours and one-off cross-midnight changes.
- Manual event delay, cancellation, relocation, pause, resumption, and public guidance.
- Screen pairing and management, explicit targeting, preview, immediate publication, and target-level delivery confirmation.
- Correction, retry, supersession, undo, restoration, offline/outdated awareness, and current operational recovery evidence.

### Product/domain state

Industry, subtype, traits, terminology, content, prices, serving formats, tap positions, hours, effective periods, availability, venue/service/event state, areas, targets, versions, publication/delivery state, source, freshness, conflict, override, and metric values.

### Permissions

View, edit, controlled wording, approve, publish, restore, screen management, organization/venue administration, connection administration, analytics access/export, commercial administration, and object/area/venue scope.

### Tier candidates

Advanced schedules and recurrence, event series, campaigns, richer presentation and synchronized displays, reusable libraries, multi-venue sharing, brand governance, approvals, advanced audit/history, native analytics, reports, and organization oversight.

### Independent add-on candidates

POS/payment, inventory/keg/tap systems, reservations, ticketing/guest-list/identity/access, sports/event/lineup feeds, footfall and external analytics, metered AI/translation, managed hardware/connectivity/monitoring/support, and custom integrations or data services.

### Limits and rollout

Venues, areas, screens, devices, users, content, taps, events, schedules, campaigns, assets, storage, history, reports, exports, connections, transactions, data, requests, tokens, languages, monitoring, support, and spend are limits. Experiments, staged delivery, compatibility, migration, and emergency disablement are internal rollout controls.

No subtype is a tier. No permission grants commercial access. No product state is a feature flag. No count grants a capability.

## Journey validation

### First value

An authorized operator can select the organization and venue, choose Bar and one canonical subtype or fallback, confirm local time and a minimal service period, create starter or manual content, select a screen purpose, pair/select a screen or deliberately defer, select exact targets, preview, publish, and receive per-target delivery evidence.

Full pricing, tier comparison, and add-on presentation follows a real first screen showing useful content. Pairing deferral preserves work and provides an exact next action; it does not substitute pricing for first value. RWP-13.06 remains paused.

### Daily operation and recovery

The dashboard is venue-time-aware and exception-first. Quick Update, hours, specials, releases, events, entry guidance, screens, publication, and restoration remain visible. Permission, purchase, configuration, connection, limit, unsupported, stale, and rollout states are presented distinctly. Target-level delivery state drives retry, correction, supersession, undo, or restoration.

### Events and privacy

Manual public event, doors, delay, cancellation, relocation, entry, and venue guidance remains core. Personal reservation, ticket, guest-list, payment, identity, or access details require separately authorized systems and audiences and are not exposed by default.

### Upgrade, downgrade, and add-on lifecycle

Upgrade preserves content and permissions. Downgrade preserves customer-authored data, safe current publication, last-known-good recovery, and manual core, without silent deletion. Add-on cancellation defines source fallback, freshness, manual override, retention/deletion, credential removal, service return, and recovery.

## Analytics validation

Core evidence covers screen health, publication, delivery, content freshness, current service, exceptions, and recovery. Advanced native analytics may compare content activity, schedules, events, campaigns, venues, and workflows. Sales, inventory, reservations, attendance, entry, footfall, conversion, or attribution require authoritative external data.

Every metric must define grain, dimensions, source, venue-local time, operating-day treatment, freshness, quality, included/excluded states, units, partial/stale behavior, permission, retention, correction, export, and classification. Publication or delivery evidence must not be presented as commercial impact or audience engagement.

## Accessibility and Impeccable validation

UI-facing planning includes clear hierarchy, one dominant task, explicit state language, keyboard support, visible focus, matching visible/accessibility names, logical headings, error summaries, 200% zoom and reflow, mobile/tablet/desktop adaptation, localization expansion, right-to-left readiness, non-color status, high contrast, reduced motion, low-light/glare/crowded/intermittent-connectivity contexts, safe high-scope confirmation, and actionable recovery. The approved Sky Blue administrative direction remains intact.

## Owner decisions still open

- Final tier count, names, capability boundaries, prices, trials, contracts, and downgrade policy.
- Exact limits, counting scope, overage, archive, and retention behavior.
- Direct versus partner delivery and bundling of integrations, managed services, data, AI, hardware, and support.
- Jurisdictional responsible-content, age/access, privacy, consent, retention, and data-right requirements.
- Cross-industry feature-key, gate, permission, override, limit, and locked-surface normalization.
- Bounded implementation, migration, test, and rollout packages after owner approval.

None of these decisions blocks the Bar profile from consolidation.

## Final handoff

After merge, issue closure, default-branch verification, and claim release:

- Bar, Brewery & Nightlife is complete through **RWP-00.26**.
- No additional Bar RWP is approved.
- Do not start Bar implementation or consolidation.
- RWP-00.75 remains gated until RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all merged and verified.
- Other industry streams continue only from their first unfinished approved RWP without duplicating valid ownership.