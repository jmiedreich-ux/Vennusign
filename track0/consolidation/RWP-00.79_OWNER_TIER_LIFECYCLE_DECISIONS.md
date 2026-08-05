# RWP-00.79 Owner Tier, Screen, Trial & Lifecycle Decisions

## Status

This document records owner decisions made during and immediately after RWP-00.79. It supplements `LIMITS_SCOPE_INHERITANCE_POLICY.md`, `TIER_AND_ADDON_ARCHITECTURE.md`, and the final Track 0 owner handoff. It does not set prices or authorize product implementation.

## 1. Commercial model

Vennusign separates:

- software tier;
- active screen capacity;
- independent add-ons;
- permissions;
- product/domain state;
- trials;
- promotions and specials;
- internal rollout controls.

The governing rule is:

> The software tier determines what outcomes the customer can achieve. Screen capacity determines how much they can operate.

These concepts remain separate in the product model even when presented as one clear customer total.

## 2. Tier ladder

The working tier ladder is:

1. Free
2. Operate
3. Coordinate
4. Portfolio
5. Enterprise

Names remain editable commercial presentation. Stable capability identifiers must not embed tier or industry names.

## 3. Free tier

The Free tier provides one complete, useful outcome:

- one organization;
- one venue;
- one user;
- one active screen;
- one active static image;
- image upload, fit/crop, preview, manual publish, delivery/status visibility, replacement, and removal;
- no forced credit card;
- no expiration while the account remains eligible under the future inactivity policy.

The Free tier excludes video, playlists, scheduling, advanced layouts, menu workflows, Quick Update, multiple users, approvals, integrations, AI, advanced analytics, white label, advanced support, and long retained history.

The Free tier must not be intentionally broken. The full signup-to-one-confirmed-screen loop must work safely.

Suggested positioning:

> One screen. One image. Easy to update.

## 4. Paid tier outcomes

### Operate

Complete daily manual signage operation, including industry-aware content, ordinary schedules/hours/state, rapid manual updates, screen pairing/selection, exact targeting, preview, publish, delivery confidence, correction, expiry, unpublish, retry, undo, restore, basic team permissions, and core operational evidence.

### Coordinate

Operate plus recurring schedules, rotations, advanced timing, campaigns, reusable templates, advanced layouts, coordinated multi-screen presentation, approvals, assignments, review queues, localization workflow, advanced history, alerts, scheduled reports, and advanced native analytics.

### Portfolio

Coordinate plus organization inheritance, explicit local overrides, safe bulk actions, cross-location libraries and campaigns, delegated regional administration, portfolio exception dashboards, cross-site comparisons, shared standards, mixed-industry organization support, and capacity planning.

### Enterprise

Portfolio plus enterprise identity, policy, retention, data governance, security and audit assurance, legal/data-region controls, governed export and BI administration, enterprise brand/localization governance, advanced support administration, and contractual service visibility.

Enterprise does not automatically mean unlimited.

## 5. Industry overlay and recommendation

Industry does not determine entitlement. It influences the recommended starting tier, terminology, examples, starter content, dashboard emphasis, and suggested add-ons.

Default first paid-tier recommendations:

- Restaurant: Operate;
- Café, Bakery & Dessert: Operate;
- Bar, Brewery & Nightlife: Operate;
- Food Truck & Concession: Operate;
- Hospitality: Coordinate;
- Entertainment & Attractions: Coordinate.

Any industry may use Free for one static image on one screen. Recommendation moves upward based on location count, recurring schedules, team workflow, shared templates, multiple zones/screens, governance, and external systems. Customers may select a different tier from the recommendation.

## 6. Screen-capacity model

Screens and software tiers are separate commercial dimensions.

- Free includes one active screen with no separate charge.
- Every paid tier includes a base screen allowance.
- Additional active screen capacity may be sold as simple packs, pooled allowances, or committed-volume bands.
- Coordinate and above should support organization pooling where approved.
- Portfolio should support organization-wide allocation and reallocation.
- Enterprise may use committed volume, negotiated bands, true-up rules, or approved site/network structures.
- Effective marginal screen cost should decline at scale.

Billable capacity should be based on active managed endpoints. Archived screens, approved cold spares, setup/test screens, replacement-pending devices, and temporary offline state should not automatically count as billable active capacity.

Hardware purchases, HaaS, connectivity, installation, monitoring, replacement, and managed service remain independent add-ons.

Internally the system keeps tier entitlement, screen allowance, screen extension, add-on, and contract distinct. Externally the customer sees one understandable total.

## 7. Free trials

Free and paid trials coexist.

- A customer may remain permanently on Free.
- A customer may activate an eligible trial of Operate, Coordinate, or another approved paid outcome.
- Trial configuration is independent of the base tier definition.
- Trial configuration includes target tier/version, duration, temporary capabilities, temporary limits, screen/venue allowance, add-ons, card requirement, eligibility, warnings, conversion behavior, and fallback behavior.
- Trial expiry must not remove the customer’s only safe public output.
- At expiry, the customer may upgrade, select another paid tier, or fall back safely to Free.
- When multiple screens were used, the customer selects which one remains active on Free.
- Unsupported trial content becomes read-only, archived, exportable, or otherwise handled under the future downgrade policy; automatic destructive deletion is not the default.

Pricing and integration discovery must not block first value. The first useful screen should be confirmed before forced plan comparison or external-system setup.

## 8. Promotions and specials

Promotions are overlays on a subscription or eligible cohort, not silent edits to a sold tier version.

Examples include:

- additional screen capacity for a defined period;
- temporary Coordinate capabilities on Operate;
- a free add-on trial;
- AI or translation credits;
- partner, region, seasonal, or grandfathered offers;
- temporary discounts or service bundles.

A promotion records eligibility, scope, included capabilities, added limits, add-ons, start/end dates, renewal and expiry behavior, precedence, public visibility, and whether it applies to new or existing customers.

## 9. Tier mutability and deletion

Before a tier version has any customer subscription or historical commercial assignment, authorized operators may edit or delete it subject to validation.

Once a tier version has ever been sold or assigned:

- it cannot be physically deleted;
- its stable ID and version cannot be reused;
- commercial fields become immutable except for approved non-contractual copy corrections;
- it may be hidden from the public catalog;
- it may be stopped from new sales;
- it may be retired while existing subscribers remain;
- it may be archived after no active subscribers remain, while retained for billing, audit, refund, dispute, reporting, and legal retention.

The Tier Manager replaces Delete with actions such as Stop selling, Hide from catalog, Create new version, View subscribers, Offer migration, and Archive when eligible.

## 10. Tier versioning

A logical tier may have multiple immutable commercial versions, for example Operate v1, Operate v2, and Operate v3.

Each sold subscription references the exact tier version it purchased. A version retains:

- stable tier and version IDs;
- internal slug/compatibility aliases;
- provider product and price references;
- billing cadence and contract metadata;
- capability assignments;
- typed allowances;
- add-on eligibility/prerequisites;
- trial rules where applicable;
- effective and retirement dates;
- historical revisions and audit records.

Creating a new version does not move existing customers.

## 11. Promoting a tier version

“Promote tier version” means make that version the default for new sales.

Promotion does not automatically migrate existing subscribers. The operator may:

- publish the new version for new sales only;
- hide or retire the previous version from new sales;
- offer an optional migration;
- schedule an approved mandatory migration;
- perform a tightly controlled administrative migration for an approved contract correction or support case.

The Tier Manager must preview affected catalog behavior and must not silently alter existing subscriptions.

## 12. Billing continuity

The Billing Manager bills from the customer’s assigned commercial record, not from the currently public catalog.

Billing authority includes:

1. assigned tier version;
2. active screen-capacity package or committed allowance;
3. active independent add-ons;
4. promotions and discounts;
5. contract-specific overrides or grandfathering;
6. migration effective date and billing policy;
7. provider-confirmed subscription/price state.

A retired or hidden tier version remains billable for existing subscribers. The system must never replace it with a newer version automatically.

Public catalog visibility does not determine billing authority. Provider return pages do not grant access; server/provider-confirmed state remains authoritative.

## 13. Customer migration campaigns

Existing-customer movement is explicit and uses a Tier Migration Campaign.

A campaign records:

- source tier/version;
- target tier/version;
- included customer cohort;
- upgrade or downgrade direction;
- voluntary or mandatory mode;
- offer and notice dates;
- effective date;
- billing/proration/credit policy;
- trial, promotion, grace, or grandfathered terms;
- capability and allowance changes;
- add-on compatibility and prerequisites;
- notification state;
- execution, failure, retry, rollback, and audit state.

Cohorts may target all customers on a version or selected organizations, regions, contracts, billing cadences, renewal windows, usage bands, or add-on combinations.

## 14. Upgrade behavior

Upgrades may take effect immediately with approved proration, at the next cycle, at renewal, or on a scheduled date.

Before confirmation the system shows:

- current and target tier versions;
- gained outcomes;
- changed screen and other allowances;
- add-on prerequisites and compatibility;
- current and target recurring charges;
- prorated charge or credit where applicable;
- effective date;
- provider/server pending and completed state.

In-progress work must be preserved while hosted billing is reviewed.

## 15. Downgrade behavior

Downgrades require a current authoritative conflict evaluation covering:

- active screens and venues;
- users and authority scopes;
- schedules, campaigns, templates, layouts, approvals, and workflow;
- integrations and add-on prerequisites;
- storage, history, reports, exports, and retention;
- organization inheritance and local overrides;
- active public content and safety/recovery needs.

Possible outcomes include:

- allowed immediately;
- scheduled for billing-cycle or renewal date;
- allowed after conflicts are resolved;
- allowed with a defined grace/read-only period;
- blocked when active-public-output or regulated-content risk cannot be safely resolved;
- escalated for an approved exception.

If the target allowance is lower, the customer may archive/reassign objects, purchase a capacity extension, choose another tier, schedule the downgrade, or request an exception.

Essential correction, unpublish, active-screen safety, approved export, and recovery remain available. Automatic destructive deletion is not the default.

## 16. Security, legal, safety, and capability deprecation

A sold tier version is not rewritten to handle platform-wide security, legal, safety, or broken-feature requirements.

Use explicit platform mechanisms such as mandatory policy, capability deprecation, security restriction, legal restriction, replacement capability, notification, and controlled migration. Historical commercial records remain intact.

## 17. Tier Manager requirements

The future Tier Manager must support:

- create logical tier and draft version;
- edit/delete unsold versions;
- publish/promote a version for new sales;
- hide, stop selling, retire, and archive sold versions;
- assign stable capabilities and typed allowances;
- configure included screen capacity and eligible extensions;
- configure add-on eligibility and prerequisites;
- configure trials independently;
- create promotions and specials;
- view current and historical subscribers;
- create, preview, schedule, pause, cancel, execute, retry, and audit migration campaigns;
- show downgrade conflicts and remediation;
- preserve provider/server billing authority.

The manager must use stable capability and allowance IDs rather than tier-named feature flags.

## 18. Implementation boundary

These decisions affect future implementation planning but do not set prices, numeric allowances, exact trial duration, contract terms, tax behavior, provider commitments, or final tier names.

Implementation must be split into bounded packages for commercial catalog/versioning, screen capacity, trials/promotions, billing continuity, migration campaigns, downgrade conflict evaluation, and customer-facing access-state presentation. No implementation is authorized by this decision record alone.
