# Limits, Scope & Inheritance Policy

## Status

RWP-00.79 defines the unified policy used by the proposed software tiers and independent add-ons. It does not choose numeric values, create billing/entitlement records, or implement enforcement.

## Policy principles

1. A limit constrains quantity or consumption; it never grants a capability or permission.
2. Every allowance has a typed unit, attachment scope, counting rule, effective period, consumption source, enforcement mode, and recovery action.
3. Organization commercial access, local product state, actor permission, add-on attachment, and limit consumption are evaluated separately.
4. Inheritance is explicit and observable. Organization defaults may flow down, but local content, authority, sources, state, history, and active-screen safety are not silently overwritten.
5. Limit enforcement protects public output, correction, unpublish, export where approved, and recovery.
6. Negative or magic values such as “-1 means unlimited” may remain compatibility inputs but must normalize to an explicit unbounded policy in the target model.
7. Numeric values, overage prices, and final pooling rules require owner/commercial approval.

## Typed allowance model

Each allowance should conceptually record:

- stable allowance type ID;
- customer-facing name and unit;
- source: tier, add-on, contract, support exception, or grandfathered policy;
- attachment scope: organization, venue/property/operation, object, provider connection, hardware contract, or user;
- counting unit and qualifying state;
- pool scope and whether consumption is shared or local;
- period: instantaneous, monthly, annual, contract term, rolling window, or retained total;
- included quantity, separately purchased extension, consumed quantity, reserved quantity, and remaining quantity;
- enforcement mode: informational, warning, soft stop, hard stop, read-only, provider-metered, or contract-managed;
- warning thresholds and effective dates;
- grace, grandfathering, temporary exception, and expiry;
- remediation actions and safe fallback;
- authoritative source/version and last calculated time.

A string `limitValue` is insufficient as the long-term authority. Browser text may use a formatted value derived from the typed decision.

## Candidate allowance dimensions

### Organization and operating context

- organizations/accounts;
- venues, properties, mobile operations, campuses, or local operating contexts;
- property/venue groups;
- descriptive objects such as outlets, service points, areas, attractions, exhibits, events, sessions, routes, gates, amenities, and services where a commercial count is later approved.

Industry/subtype does not consume or increase an allowance by itself.

### Screens and devices

- active screens;
- registered/archived screens;
- video-wall screens and groups;
- managed devices, replacement pool, connectivity subscriptions, and monitored endpoints;
- simultaneous active targets or high-volume delivery where later approved.

Screen entitlement limits are distinct from layout capacity, overflow, pixel/canvas capacity, or delivery state.

### Users and authority

- active users;
- administrators, approvers, operators, support users, guests, or service accounts where a count is commercially approved;
- enterprise identity/directory seats.

A user count does not grant any role or action permission.

### Content, workflow, and history

- templates, campaigns, schedules, rules, approvals, tasks, retained versions, audit events, reports, exports, saved views, and workflow history;
- storage volume, media size, and retention duration.

Essential correction, active public safety, and bounded export/recovery remain available according to downgrade policy even when optional creation limits are reached.

### Languages, translation, and AI

- manually maintained language variants where an allowance is commercially approved;
- automated translation characters/requests;
- AI tokens, generations, analyses, predictions, or processing units;
- retained generated assets/history.

Basic manually authored accessible language support remains core; automated/metered processing is separate.

### Integrations and sources

- attached add-ons;
- provider connections/accounts;
- source objects, catalogs, properties, venues, events, or locations;
- synchronization frequency, transactions, records, data volume, history, or API consumption.

Commercial attachment, administrator permission, configured state, connection health, source freshness, and limit consumption remain distinct.

### Analytics and data

- reports, scheduled deliveries, exports, retained history, data volume, dashboards, metrics, alerts, recipients, and BI destinations;
- externally sourced measurements or reconciled transactions.

Core publication/delivery/exception/recovery evidence remains available even without advanced analytics.

### Support and managed services

- support incidents, service hours, response class, sites/devices covered, content/localization volume, deployments, installation visits, monitoring endpoints, and managed events;
- HaaS contract term and covered hardware.

These may be contract/service allowances rather than software entitlement limits.

## Attachment scope

### Software tier

The organization owns one effective software-tier relationship unless later commercial policy explicitly allows site-specific tiers. Canonical capability access is organization-derived and then evaluated against local compatibility, permissions, limits, and state.

A software tier does not imply authority over every venue or object.

### Add-on

An add-on attaches at the narrowest valid scope:

- organization when one provider/account serves the organization;
- venue/property/operation when a local provider/account or contract applies;
- outlet/service point/menu/catalog/event/source when the provider requires finer scope;
- hardware/device when it is a managed physical service;
- user when it is an individually licensed external service.

Attachment scope is not inferred from industry. Provider eligibility and object compatibility must be explicit.

### Limit

An allowance attaches to its commercial source and counting scope. Examples:

- organization tier supplies a shared screen pool;
- add-on supplies provider connections per venue;
- HaaS contract supplies covered devices for one site;
- support exception temporarily extends one allowance for one organization or venue.

The target model must show which source supplies each effective allowance.

## Inheritance precedence

Effective values follow a visible precedence order:

1. mandatory system/safety/privacy/rights restriction;
2. active support/commercial exception or grandfathered contract, within its approved scope and dates;
3. independently attached add-on or managed-service policy;
4. organization software-tier capability and allowance;
5. organization default configuration;
6. approved venue/property/operation local override;
7. industry/subtype recommendation or neutral fallback;
8. product default.

This order is conceptual and must be made explicit per concern. A higher item does not automatically overwrite unrelated product content.

## Organization and local inheritance

- Organization tier capabilities are inherited by eligible local contexts.
- Local permissions determine who can act.
- Industry/subtype controls presentation/defaults, not commercial inheritance.
- Organization templates/defaults may seed local values; local overrides are explicit and reversible.
- Removing a local override reveals the inherited value.
- Copying or bulk applying content never transfers ownership, permission, privacy scope, source authority, add-on attachment, or integration credentials.
- Mixed-industry organizations use canonical capability IDs while preserving local terminology and object models.
- Effective-value views should show source, inherited value, local override, exception, and effective result.

## Pooling policy

Pooling is defined independently per allowance type.

Candidate modes:

- **organization shared pool** — all eligible local contexts consume one quantity;
- **local fixed allocation** — each venue/property/operation receives a defined amount;
- **organization pool with local reservations** — shared capacity plus protected local allocation;
- **non-poolable attachment** — provider connection, hardware contract, or regulated service remains attached to one context;
- **provider-metered** — provider usage is authoritative and reported back;
- **contract-managed** — service term/coverage is governed by the separate contract.

The UI must not imply a pool where the commercial contract is local, or a local allowance where consumption is shared.

## Counting rules

Every counted object needs:

- unique canonical identity;
- qualifying state, such as active versus archived;
- scope and time boundary;
- de-duplication rule;
- reservation/commitment rule;
- transition timing for activation, archive, deletion, transfer, or restore;
- reconciliation and audit source.

Current screen and venue downgrade evaluation counts active screens and organization venues. RWP-00.79 preserves this behavior as a legacy starting point but requires explicit definitions before migration.

## Enforcement modes

### Informational

Show consumption and forecast without blocking.

### Warning

Allow action while showing threshold, impact, and next steps.

### Soft stop

Stop creation or expansion but preserve current operation, correction, unpublish, export, and recovery.

### Hard stop

Used only where contractual, provider, security, privacy, rights, safety, or technical capacity requires it. Explain the authoritative reason and safe alternatives.

### Read-only/grace

Preserve viewing, export, correction required for public safety, and orderly remediation for a defined period.

### Provider/contract-managed

Provider or separate service contract is authoritative; Vennusign shows status and avoids inventing access.

The same allowance may use different modes before, at, and after downgrade.

## Limit-reached behavior

A limit-reached state must show:

- counted unit and scope;
- included, consumed, reserved, and remaining quantities;
- which objects consume the allowance;
- authoritative calculation time;
- whether the restriction blocks creation, activation, publication, retention, or provider processing;
- actions such as archive, reassign, reduce, export, purchase an extension, review another tier, repair data, or contact an administrator/support;
- public-output and recovery protections.

Do not present a limit as a missing permission or generic feature lock.

## Upgrade and limit increase

- A software-tier upgrade may change several allowances.
- A separately purchasable allowance extension remains a distinct commercial item if offered.
- An add-on may include its own allowance without changing unrelated software-tier limits.
- Eligibility and usage are rechecked server-side before hosted billing continuation.
- Pending provider state never changes effective allowance locally.
- The customer sees effective date, changed quantities, pooling/scope, prerequisites, and downstream consequences.

## Downgrade policy

Before downgrade:

1. calculate target capability and allowance decisions using current authoritative usage;
2. list conflicts and the exact consuming objects;
3. protect active public screens and essential operation;
4. explain advanced workflow, template, campaign, analytics, history, and governance effects;
5. show independent add-ons and whether their prerequisites remain satisfied;
6. provide least-destructive remediation choices;
7. recheck immediately before hosted billing action;
8. apply access only after provider/server confirmation.

Candidate conflict outcomes, selected later by owner policy:

- require remediation before downgrade;
- schedule downgrade after remediation;
- temporary grace/read-only state;
- automatic archival only with explicit reviewed consent and restoration path;
- retain data for a defined period with export;
- cancel downgrade when active-screen or regulated-content risk cannot be safely resolved.

Automatic destructive deletion is not an acceptable default.

## Add-on cancellation/removal

Removal must separately address:

- commercial cancellation effective date;
- provider disconnect and credential revocation;
- synchronization stop and final freshness timestamp;
- manual fallback;
- dependent schedules/content/analytics;
- retained configuration and reconnect path;
- data export, retention, deletion, and rights obligations;
- unresolved conflicts or overrides;
- hardware return, contract term, support, and service state where applicable.

Removing an add-on does not remove customer-authored content or the core capability to operate manually.

## Exceptions and grandfathering

An exception record requires:

- target capability or allowance type;
- organization/local scope;
- source: support, contract, migration, promotion, incident, or legal/commercial decision;
- granted or restricted value;
- actor and approving authority;
- reason and customer-safe explanation;
- start, expiry, review, and revocation dates;
- precedence and interaction with tier/add-on policy;
- audit history;
- notification and expiry behavior.

Exceptions must not represent product operating state. A sold-out item, closed venue, disconnected source, or temporary delivery failure is not an entitlement exception.

## Capacity and public-output protection

- Active screens, current public notices, urgent content, correction, unpublish, expiry, and recovery receive explicit protection during limit enforcement and downgrade.
- Layout overflow/capacity is a product/presentation issue, not a subscription limit.
- A screen over plan allowance may require archive/reassignment before downgrade, but existing public output must not disappear without an explicit safe transition.
- Partial delivery or source failure does not consume or release commercial capacity unless the counting policy explicitly says so.

## Impeccable presentation guidance

Limit and inheritance surfaces should use a task-first hierarchy:

1. effective access and scope;
2. current usage and counted objects;
3. impact on the attempted action;
4. inherited/default/override/exception source;
5. least-destructive actions;
6. commercial review only where relevant.

Use specific actions such as “Archive 2 screens,” “Move connection to Downtown,” “Remove local override,” “Export history,” or “Review Portfolio allowance.” Support keyboard/focus behavior, accessible tables and names, long dynamic values, localization, mobile layout, reduced motion, 200% zoom, error/retry/reconciliation, and mixed-result states.

## Owner decisions

- exact allowance values and which are tiered, add-on extensions, contracted, or unbounded;
- pooling mode per allowance;
- warning thresholds, overage, grace, grandfathering, and enforcement mode;
- site-specific versus organization-wide software tier policy;
- data retention/export/deletion periods;
- active-screen downgrade protection duration;
- exception approval roles and maximum durations;
- provider-metered reconciliation and dispute handling;
- contract/service limit presentation.

## Handoff

RWP-00.80 validates representative customer journeys against the normalized capability, tier/add-on, limit, scope, inheritance, permission, state, and recovery policies.
