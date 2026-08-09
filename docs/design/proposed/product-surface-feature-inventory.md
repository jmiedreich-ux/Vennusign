# VennuSign Product Surface and Future Feature Inventory

**Audience:** product owner, UX designers, planners, and implementation agents  
**Status:** Proposed design reference; not a release, pricing, tier, or implementation commitment  
**Updated:** 2026-08-07

## Purpose

This inventory translates the Track 0 capability architecture into a screen-design reference. It answers: **what customer-facing product areas, workflows, actions, objects, and states might VennuSign eventually need to support?**

Use it to design information architecture that can grow without exposing every future capability today. The inventory intentionally includes approved core behavior, advanced native candidates, independent add-on candidates, operating states, governance surfaces, and longer-horizon possibilities.

## How to read status

| Status | Meaning for design |
| --- | --- |
| **Core approved** | Essential manual operating outcome approved in Track 0. This does not mean every supporting screen is implemented. |
| **Foundation delivered** | Track 1 established a technical authority or decision foundation; customer UX may remain incomplete. |
| **Advanced candidate** | Plausible native VennuSign capability that may belong to a higher outcome tier. Packaging and release are undecided. |
| **Add-on candidate** | External, metered, hardware, or managed capability that must remain separately attachable and preserve manual fallback. |
| **Policy pending** | The product needs an owner decision before detailed design or implementation. |
| **Long horizon** | A credible future possibility, not a commitment. Reserve conceptual room; do not advertise it. |

## Product-wide design rules

1. The owner is the primary user, often working in interrupted bursts. Current scope, public impact, exceptions, and the next safe action must be visible quickly.
2. Essential manual operation remains available even when automation, an integration, or a paid add-on is unavailable.
3. Preview the intended target before publishing and distinguish request, queued, received, applied, acknowledged, stale, offline, partial, and failed states.
4. Explain the actual reason an action is unavailable. Never collapse permission, commercial access, product state, add-on configuration, disconnection, limits, privacy restrictions, unsupported context, and rollout into one generic lock.
5. Destructive or high-scope actions require consequence preview, exact scope, confirmation, and recovery where possible.
6. Every customer-facing area must support loading, first-use, empty, validation, success, error, denial, stale/offline, long names, localization expansion, keyboard use, screen readers, 200% zoom, mobile, and desktop.
7. Industry and subtype change terminology, defaults, starter material, screen-purpose suggestions, and emphasis—not entitlement.
8. Sky is the established brand direction. Visual experiments may vary composition, but implementation authority requires an approved design package.

## Recommended top-level product architecture

This is a durable grouping for design exploration, not a mandate that every item becomes permanent navigation.

| Product area | Primary customer question | Likely surfaces |
| --- | --- | --- |
| Home | Is the venue okay, and what needs me now? | Operational home, attention queue, activity, assistant |
| Content | What public information are we showing? | Menus, notices, wayfinding, events, services, media |
| Screens | Where is content playing, and is it healthy? | Fleet, screen detail, pairing, groups, walls, diagnostics |
| Plan & Automate | What should happen later or repeatedly? | Schedules, rotations, rules, campaigns, calendar |
| Publish | What will change, where, and did it arrive? | Preview, target selection, release ledger, delivery history |
| Brand & Templates | How should every output look and stay consistent? | Themes, layouts, templates, brand library, localization |
| Sources & Add-ons | Where does authoritative data come from? | Connections, source health, conflict resolution, marketplace |
| Insights | What happened and what deserves attention? | Operational evidence, reports, alerts, comparisons |
| Team | Who can do what, where, and what awaits review? | People, roles, assignments, approvals, audit |
| Organization | How do sites share standards without losing local control? | Venues, hierarchy, inheritance, overrides, portfolio |
| Plan & Billing | What is included, limited, attached, or changing? | Plan, allowances, add-ons, checkout, downgrade review |
| Help & Administration | How do we recover, govern, and stay safe? | Support, diagnostics, security, privacy, developer/admin tools |

## Content and Menu industry deep dive

### Central design conclusion

**Menu is an important content type, but it is too restaurant-specific to remain the permanent umbrella for the product.** The durable top-level area should be **Content**, with an industry-aware starting label and editor:

| Industry | Suggested customer label | Primary content types |
| --- | --- | --- |
| Restaurant | Menus | Food menus, drink menus, specials, service-period menus |
| Bar, Brewery & Nightlife | Drinks & Menus | Tap lists, cocktail lists, wine lists, food menus, releases, event lineups |
| Café, Bakery & Dessert | Products & Menus | Counter menus, product lists, batch boards, seasonal offers, pickup information |
| Food Truck & Concession | Menus & Offers | Compact menus, event/stand menus, combos, specials, pickup and service guidance |
| Hospitality | Guest Information | Amenities, services, outlets, event directories, wayfinding, notices; menus are nested outlet content |
| Entertainment & Attractions | Programs & Visitor Information | Shows, sessions, attractions, admission, queues, wayfinding, safety notices; concession menus are nested content |

Industry and subtype should seed terminology and suggested starting content, not create different entitlement models or incompatible editors.

### Industry-specific editing needs

| Industry | Entries and attributes | High-frequency states | Context that changes the editor |
| --- | --- | --- | --- |
| Restaurant | Items, sections, descriptions, prices, sizes, variants, modifiers, dietary/allergen information, images and service availability | Available, unavailable, sold out, limited, substituted, expected return | Breakfast/lunch/dinner, dine-in/takeout, service point, location, source-controlled fields |
| Bar, Brewery & Nightlife | Beer, wine, cocktails, spirits, food, taps, bottles, cans, pours, flights, ABV/style, releases, specials and entry information | On tap, kicked/unavailable, sold out, limited release, happy hour, kitchen closed, last call | Bar/patio/room, cross-midnight business day, event, viewing zone, age/responsible wording |
| Café, Bakery & Dessert | Products, categories, sizes, options, batches, freshness, seasonal items, custom/preorder and pickup information | Fresh now, next batch, limited, sold out, expected return, preorder closed, pickup ready only when authoritative | Counter/table service, morning/daypart, pickup point, production source and batch timing |
| Food Truck & Concession | Compact/event/stand menus, items, combos, specials, service points, pickup guidance and host/event context | Available, limited, sold out, service paused, closed, canceled, relocated, reopened | Current location, next stop, host venue, service window, event period, queue/pickup lane |
| Hospitality | Amenities, services, outlets, events, meeting sessions, destinations, routes and public notices | Open, closed, limited service, maintenance, relocated, delayed, canceled, unknown | Property/building/floor, guest/public audience, outlet, event, accessible route, privacy-safe wording |
| Entertainment & Attractions | Programs, shows, screenings, sessions, attractions, exhibits, events, admission and visitor guidance | Open, paused, delayed, canceled, relocated, capacity limited, queue unavailable, reopened | Venue/zone, occurrence time, admission/last entry, queue source, accessible route, safety priority |

### Shared content model

All industries can use one extensible content foundation:

1. **Collection** — menu, tap list, program, directory, notice set, visitor guide or other named public-information set.
2. **Section** — a customer-recognizable grouping with order, visibility and optional timing.
3. **Entry** — the item, drink, service, amenity, event, session, attraction, destination or notice.
4. **Attributes** — common fields plus optional industry modules; never force every entry into a restaurant-item schema.
5. **Operating state** — availability, closure, delay, freshness, capacity, relocation or other represented fact.
6. **Timing** — effective dates, service period, schedule, recurrence, expiry and timezone.
7. **Place and audience** — venue, outlet, area, service point, screen purpose, audience and language.
8. **Source** — manual or connected authority, freshness, last-known-good value, conflict and override.
9. **Presentation** — layout, theme, media, accessibility, responsive behavior and target compatibility.
10. **Lifecycle** — draft, review, publish, delivery, correction, supersession, unpublish, undo and restore.

Industry modules should add relevant fields without fragmenting the core model:

- **Food:** allergens, dietary labels, modifiers, sizes and service mode.
- **Beverage:** ABV, style, producer, vintage, pour size, tap and package format.
- **Café/production:** batch, freshness, expected return, preorder and pickup.
- **Event/program:** occurrence, doors, start/end, performer/host, admission and cancellation.
- **Attraction:** session, queue/capacity, accessibility, closure and reopening.
- **Hospitality:** amenity hours, property/building/floor, audience, route and service conditions.

### Recommended Content-area architecture

The Content area should be organized around customer jobs rather than data tables:

```text
Content
├── Library       Find, create, duplicate, archive and restore content
├── Editor        Structure, entries and industry-relevant properties
├── Quick Update  Change high-frequency public states without opening the full editor
├── Plan          Timing, service periods, schedules, rotations and expiry
├── Presentation  Layout, theme, media, languages and accessibility
├── Sources       Authority, freshness, mappings, conflicts and overrides
└── Publish       Exact targets, change review, delivery evidence and recovery
```

The primary editing workspace should normally combine:

- a content/section outline;
- a direct visual preview or canvas;
- a contextual property inspector;
- persistent draft-versus-live information;
- exact publish impact and destination readiness.

Quick Update should remain a separate fast path for interrupted operational work. A user changing “sold out,” “delayed,” or “closed” should not need to navigate the complete design editor.

### Discovery and onboarding behavior

Do not ask a new customer to choose from every content type VennuSign might eventually support. Industry, subtype and stated screen purpose should suggest a small starting set:

- Brewery: Tap List, Food Menu, Releases, Events.
- Café: Counter Menu, Batch Board, Pickup Information.
- Food truck: Today’s Menu, Current Location, Pickup Guidance.
- Hotel: Guest Information, Amenities, Event Directory, Dining Outlets.
- Cinema: Showtimes, Concessions, Visitor Notices.
- Attraction: Today’s Program, Queue/Closure Information, Wayfinding.

Provide **Create another content type** for broader discovery. Suggested types remain editable recommendations, not hidden packages or automatic entitlements.

### Required editor states

Every content type needs first-use, populated, empty-section, validation, unsaved/saved draft, review required, permission restricted, commercially locked advanced tool, source-controlled, stale/conflicted, offline target, partial delivery, publish success/failure, archived and recoverable states. The interface must identify the real reason and preserve the safest next action.

## Complete surface inventory

### 1. Identity, onboarding, and first value

**Likely surfaces:** Sign in, invitation, organization setup, venue setup, industry/subtype, starter content, first-screen pairing, first publish, resumable checklist.

- Account creation, sign-in, recovery, invitation acceptance, session expiry, and secure sign-out — **Core approved**.
- Create or select an organization and establish the first authorized venue/property/operation context — **Core approved**.
- Choose primary industry, subtype, descriptive traits, local terminology, timezone, and screen purposes — **Core approved**.
- Preview the effect of changing industry/subtype without implying a plan change — **Policy pending**.
- Generate starter content, recommended screen purposes, and a neutral fallback appropriate to the venue — **Core approved**.
- Pair or deliberately defer the first screen with clear consequences — **Core approved**.
- Preview, publish, and verify one useful public outcome before forced pricing or integration setup — **Core approved**.
- Save progress, resume after interruption, transfer ownership, and recover an incomplete setup — **Advanced candidate**.
- Guided import from an existing menu, website, document, provider, or prior venue — **Add-on/long horizon**.

### 2. Home and daily operations

**Likely surfaces:** Venue health overview, needs-attention queue, quick update, recent activity, upcoming schedule, publishing status, support assistant.

- At-a-glance venue health: screens online, content current, sources fresh, schedules active, and exceptions — **Core approved**.
- Prioritized attention queue ordered by public impact and safe urgency — **Core approved**.
- Quick manual updates for sold out, unavailable, delayed, paused, closed, relocated, changed hours, next batch, reopening, and comparable states — **Core approved**.
- Pending changes and explicit draft-versus-live status — **Core approved**.
- Recent activity with actor, scope, target, intended result, delivered result, and timestamps — **Core approved**.
- Upcoming schedules, campaigns, expirations, maintenance windows, and review deadlines — **Advanced candidate**.
- Role-aware home emphasis for Owner, Content Editor, Publisher, operational staff, and portfolio operators — **Foundation delivered / UX pending**.
- Contextual recommendations based on real evidence, never invented metrics — **Advanced candidate**.
- Embedded support agent that can understand the current page/session and explain evidence — **Long horizon**.

### 3. Content and information models

**Likely surfaces:** Content library, menu/catalog editor, notices, services, events, wayfinding, media library, history.

- Customer-authored content with local names, descriptions, pricing, availability, timing, imagery, and accessibility text — **Core approved**.
- Menus, products, tap lists, amenities, services, attractions, exhibits, programs, sessions, areas, destinations, routes, gates, notices, and emergency/public guidance — **Core approved as normalized objects; implementation varies**.
- Sections, groups, ordering, duplication, archiving, restoration, and bulk editing — **Core/advanced mix**.
- Effective dates, expiry, supersession, unpublish, undo, restore, and retained versions — **Core approved; retained depth may be tiered**.
- Content validation for missing fields, overflow, unsupported media, unsafe/private material, rights restrictions, and target compatibility — **Core approved**.
- Reusable content blocks, libraries, collections, and organization templates — **Advanced candidate**.
- Import/export, structured data exchange, and content portability — **Advanced/add-on candidate**.
- Collaborative comments, assignments, review status, and content ownership — **Advanced candidate**.

### 4. Menu, catalog, and rapid-update tools

**Likely surfaces:** Menu builder, item inspector, availability board, price change review, batch update, live preview.

- Direct editing of names, descriptions, prices, dietary/allergen information, availability, and placement — **Core approved**.
- Rapid availability and operating-state changes with optional expected return — **Core approved**.
- Draft autosave, explicit publish, cancel/revert, undo/redo, and change history — **Core approved**.
- Show exactly which screens still display the old value and which will update — **Core approved**.
- Bulk price, availability, category, schedule, or destination changes with impact preview — **Advanced candidate**.
- Variants, modifiers, sizes, service points, batches, pickup/preorder context, and source-controlled fields — **Core/industry expansion**.
- Happy hour, specials, promotions, seasonal items, tap rotation, and limited-time menus — **Advanced candidate**.
- Conflict resolution between manual edits and external source values — **Add-on-dependent core recovery**.

### 5. Brand, layout, templates, and presentation

**Likely surfaces:** Brand library, theme editor, layout gallery, template builder, screen preview, responsive simulation.

- Basic readable themes and accessible layout choices — **Core approved**.
- Logo, colors, typography, spacing, media, and reusable brand assets — **Advanced candidate**.
- Layout selection by screen purpose, orientation, resolution, safe area, and information density — **Core/advanced mix**.
- Live preview for exact target and representative device sizes — **Core approved**.
- Reusable templates, locked brand regions, approved components, and controlled local customization — **Advanced candidate**.
- Advanced HTML/custom presentation with validation, sandboxing, fallback, and preview safety — **Advanced candidate**.
- Video walls, coordinated canvases, synchronized regions, and multi-screen storytelling — **Advanced candidate**.
- Accessibility checking for contrast, text size, motion, reading order, and content density — **Advanced candidate**.
- Brand governance, approval, distribution, inheritance, and local override — **Portfolio/Enterprise candidate**.

### 6. Screen fleet, players, and physical topology

**Likely surfaces:** Fleet, screen detail, pairing, replacement, grouping, wall builder, location map, maintenance.

- Add, pair, name, locate, purpose, rename, and safely unpair a screen/player — **Core approved**.
- Generate and expire pairing codes; replace a broken player while preserving content/settings where supported — **Core approved**.
- Show platform, app version, device identity, orientation, resolution, assigned purpose, and current content — **Core approved**.
- Distinguish paired, online, idle, stale, offline, updating, unsupported, and intentionally disabled states — **Core approved**.
- Per-screen current version, intended version, last heartbeat, last acknowledgement, and last successful publish — **Core approved**.
- Groups, reusable target sets, zones, floors, service points, properties, and screen-purpose collections — **Advanced candidate**.
- Video-wall membership, ordering, layout capacity, synchronized playback, and partial-wall failure — **Advanced candidate**.
- Brightness, quiet hours, orientation, restart, cache refresh, app update, and approved remote recovery — **Advanced/hardware-dependent candidate**.
- Hardware inventory, installation, warranty, replacement, connectivity, and managed-service status — **Add-on candidate**.

### 7. Publishing, targeting, delivery, and recovery

**Likely surfaces:** Target picker, preview, publish review, progress, delivery ledger, recovery history.

- Select exact venue/context, object, audience, screen, group, and delivery target — **Core approved**.
- Preview intended output before publication — **Core approved**.
- Explicitly publish, publish to selected targets, or safely publish all authorized targets — **Core approved**.
- Show saved, queued, accepted, received, applied, acknowledged, partial, offline, stale, failed, and unknown states without overstating evidence — **Core approved**.
- Explain per-target impact before publishing and mixed results afterward — **Core approved**.
- Retry, correct, supersede, expire, unpublish, undo, restore, and preserve last-known-good output — **Core approved**.
- Approval gates, scheduled release, staged release, rollout windows, and coordinated transitions — **Advanced candidate**.
- Publication history, comparison, retained evidence, export, and audit — **Advanced candidate**.
- Emergency/high-priority publishing with explicit authority and recovery safeguards — **Policy pending**.

### 8. Scheduling, rotations, rules, and campaigns

**Likely surfaces:** Calendar, schedule builder, daypart editor, rotation, rule builder, campaign workspace.

- Manual hours, service periods, events, sessions, batches, programs, routes, and planned notices — **Core approved**.
- Start/end scheduling, expiry, one-time changes, and timezone-aware preview — **Core approved**.
- Recurrence, dayparts, rotations, conflict detection, exclusions, and coordinated transitions — **Advanced candidate**.
- Reusable campaigns, promotions, content sets, destinations, target groups, and calendars — **Advanced candidate**.
- Rule-based changes driven by time, source state, availability, weather, capacity, or authorized events — **Advanced/add-on candidate**.
- Campaign approval, simulation, launch readiness, partial-target handling, rollback, and results — **Advanced candidate**.
- Calendar/source synchronization with conflict ownership and manual fallback — **Add-on candidate**.

### 9. Sources, synchronization, and conflict resolution

**Likely surfaces:** Connection catalog, connection setup, field mapping, source health, conflicts, overrides, sync history.

- Source identity, authoritative fields, freshness, coverage, last successful refresh, and last-known-good values — **Core whenever a source exists**.
- Distinguish configuration error, unauthorized, disconnected, stale, conflicting, rate-limited, provider incident, and unsupported states — **Core recovery contract**.
- Local override with explicit scope, author, reason, expiry, and reconnect behavior — **Core recovery / advanced governance**.
- Field mapping, object matching, duplicate resolution, initial import, reconciliation, and dry-run preview — **Add-on candidate**.
- Sync direction, cadence, pause/resume, retry, backfill, history, and provider status — **Add-on candidate**.
- Credential setup without exposing secrets; reconnect, revoke, remove, and retain/delete configuration — **Add-on candidate**.
- Data-quality warnings, source precedence, manual fallback, and conflict queues — **Add-on candidate**.

### 10. Commerce and food-operation add-ons

**Likely surfaces:** Provider marketplace, connection setup, catalog mapping, order/status mapping, source monitor.

- POS catalog, price, availability, modifier, and item synchronization — **Add-on candidate**.
- Inventory, production, recipe/supply, batch, sold-out, and expected-return synchronization — **Add-on candidate**.
- Ordering, payment, fulfillment, pickup, order-ready, and service-point information — **Add-on candidate**.
- Tap-management, keg/beer data, rotations, freshness, and availability — **Add-on candidate**.
- Loyalty, supplier, CRM, promotion, and customer-program sources — **Add-on candidate**.
- Manual operation, safe override, conflict visibility, and last-known-good presentation remain available when disconnected — **Core requirement**.

### 11. Hospitality, entertainment, and environmental add-ons

**Likely surfaces:** Connection setup, property/venue mapping, source monitor, privacy review, operational dashboards.

- Property management, room/amenity, booking, conference/event sales, guest service, transport, parking, access, and gaming systems — **Add-on candidate**.
- Ticketing, admissions, access control, queue/wait, occupancy/footfall, cinema, attraction, collection, venue/show control, sports, mapping, and mobility systems — **Add-on candidate**.
- Weather, traffic, government/public feeds, safety alerts, calendars, directories, messaging, data warehouse, and export destinations — **Add-on candidate**.
- Public-screen privacy, rights, age/regulated-content, uncertainty, source coverage, and safe fallback controls — **Policy pending / mandatory when applicable**.
- Guest-specific, child-related, biometric/camera, alcohol, gambling, sponsor, and regulated information requires separately approved governance — **Policy pending**.

### 12. AI, translation, and assisted workflows

**Likely surfaces:** Assistant panel, translation workspace, generation review, usage, source/evidence view, automation settings.

- Draft content, rewrite, summarize, adapt tone, suggest layout, and generate variations — **Add-on/advanced candidate**.
- Automated translation with manual review, terminology library, locale fallback, and per-target language — **Add-on candidate**.
- Predictive recommendations, anomaly detection, scheduling assistance, and optimization — **Long horizon**.
- UI support agent that understands page context, session evidence, delivery history, and safe recovery actions — **Long horizon**.
- Engineering/support diagnostics with trace evidence, correlation IDs, logs, application insights, permissions, and audit — **Long horizon**.
- Every generated result must identify source/status, remain reviewable, respect privacy/rights, expose consumption, and never silently publish — **Required governance**.

### 13. Team, roles, workflow, and audit

**Likely surfaces:** People, invitations, roles, scope assignments, review queue, activity, audit, support access.

- Owners, editors, publishers, local operators, billing administrators, integration administrators, support, and custom scoped roles — **Foundation delivered / expansion candidate**.
- Organization, venue, area, screen, content, event, source, integration, billing, and support scopes — **Foundation delivered**.
- View, create, edit, approve, publish, unpublish, override, restore, delete, export, manage users, manage billing, manage integrations, and sensitive-data actions — **Foundation delivered / UI pending**.
- Invitations, assignment, reassignment, expiration, deactivation, and access review — **Core/advanced mix**.
- Approval chains, review queues, comments, acknowledgment, escalation, and operational handoff — **Advanced candidate**.
- Audit history with actor, reason, scope, before/after, correlation, source, and effective time — **Advanced/Enterprise candidate**.
- Bounded support grants/exceptions with reason, expiry, review, notification, and revocation — **Foundation delivered / UX pending**.

### 14. Multi-venue, portfolio, and enterprise governance

**Likely surfaces:** Organization map, venue switcher, portfolio health, templates, inheritance inspector, bulk action review.

- Authorized switching among venues/properties/operations while preserving local terminology and state — **Core approved**.
- Organization hierarchy, groups, regions, brands, concepts, properties, outlets, areas, and service points — **Advanced candidate**.
- Organization defaults, inheritance, local overrides, exceptions, effective-value comparison, and rollback — **Portfolio candidate**.
- Cross-site templates, campaigns, schedules, content libraries, screen-purpose standards, and localization governance — **Portfolio candidate**.
- Safe bulk actions with scope preview, mixed results, per-site confirmation, and restoration — **Portfolio candidate**.
- Portfolio delivery/source health, exceptions, comparisons, capacity, and scheduled reports — **Portfolio candidate**.
- Enterprise identity/directory, policy, delegated administration, data regions, retention, export, legal hold, and assurance — **Enterprise candidate**.

### 15. Insights, alerts, reports, and evidence

**Likely surfaces:** Operational insights, alert center, report builder, metric definitions, exports, delivery/source evidence.

- Basic operational evidence: screen health, content freshness, pending changes, delivery outcomes, source health, and exceptions — **Core approved**.
- Alerts for offline/stale screens, failed/partial delivery, expiring schedules, disconnected sources, conflicts, limits, and approval delays — **Advanced candidate**.
- Trends, comparisons, scheduled reports, portfolio analysis, and optimization using VennuSign-owned evidence — **Advanced candidate**.
- External analytics/BI, footfall, commerce, ticketing, occupancy, or CRM data — **Add-on candidate**.
- Metric catalog with definition, scope, source, freshness, reconciliation, privacy, and confidence — **Required before claiming metrics**.
- Export destinations, report permissions, retention, redaction, and audit — **Advanced/Enterprise candidate**.
- Never invent revenue, engagement, conversion, attribution, or operational outcomes without trustworthy sources — **Binding rule**.

### 16. Plans, allowances, add-ons, and lifecycle

**Likely surfaces:** Plan overview, capability comparison, usage, add-on catalog, checkout/portal handoff, downgrade review.

- Explain current software outcome, included capabilities, independent add-ons, allowances, product states, and responsible administrator — **Foundation delivered / UX pending**.
- Server-authoritative eligibility, hosted checkout/billing portal, pending provider confirmation, applied/canceled/error states — **Existing foundation**.
- Typed allowances for venues, screens, users, objects, schedules, events, sources, integrations, campaigns, templates, languages, reports, history, storage, exports, transactions, support, hardware, and AI/translation consumption — **Policy pending**.
- Usage detail with included, consumed, reserved, remaining, counted objects, scope, pooling source, and calculation time — **Advanced candidate**.
- Upgrade comparison by complete customer outcome, not isolated feature bait — **Required direction**.
- Downgrade preview covering lost advanced outcomes, allowance conflicts, active screens, scheduled work, history, exports, retention, add-on dependencies, grace, and remediation — **Policy pending**.
- Add-on purchase, attachment scope, prerequisites, provider/region eligibility, support responsibility, removal, reconnect, retention, and manual fallback — **Policy pending**.
- Release, tier, add-on, rollout, and product state remain separate decisions — **Binding rule**.

### 17. Help, support, diagnostics, and managed services

**Likely surfaces:** Help center, contextual help, incident detail, diagnostic trace, support conversation, service status.

- Contextual explanations, quick guides, recovery steps, and links grounded in the current screen/task — **Core UX requirement**.
- Customer-visible causal trace: save, publish, acknowledgement, connection loss, missed heartbeat, player/app failure, recovery — **Advanced candidate**.
- Safe self-service retry, reconnect, pairing-code generation, replacement, correction, and restoration — **Core/advanced mix**.
- Support conversation with explicit consent to inspect scoped session evidence — **Long horizon**.
- Owner/engineering root-cause view with correlation IDs, structured logs, traces, application insights, deployment/app versions, and audit — **Long horizon**.
- Status page, incident communication, maintenance windows, service-level evidence, and managed monitoring — **Add-on/Enterprise candidate**.
- Managed hardware, connectivity, installation, deployment, content, localization, analytics, and support contracts — **Add-on candidate**.

### 18. Account, security, privacy, developer, and internal administration

**Likely surfaces:** Profile, authentication/security, organization settings, privacy/rights, API/developer, internal rollout/support tools.

- Profile, locale, timezone, authentication methods, sessions, password/recovery, and notification preferences — **Core foundation**.
- Organization ownership, security contacts, billing contacts, and authorized administrators — **Core/advanced mix**.
- MFA, SSO/directory, device/session management, security events, and delegated policy — **Advanced/Enterprise candidate**.
- Privacy, rights, consent, retention, deletion, export, redaction, and restricted-content review — **Policy pending**.
- API credentials, webhooks, service accounts, event subscriptions, rate/usage visibility, and integration audit — **Long horizon / Enterprise candidate**.
- Internal rollout, preview, staged release, temporary disablement, support exceptions, and customer-safe explanation — **Internal control; never customer entitlement**.
- Operational configuration, migration, reconciliation, data repair, and support tooling must remain audited and scope-safe — **Internal requirement**.

## Backlog — TBD

The owner canceled former Phases 14, 15 and 16 as numbered phases. Their ideas remain here only as an **unprioritized backlog**. They are not approved, sequenced, promised, scheduled, priced, or available. Any item requires fresh owner approval before it becomes planning or implementation work.

### Mobile operations — formerly Phase 14

**Intent:** give venue staff fast operational control from iOS and Android during service.

- React Native application sharing the existing .NET API, with biometric authentication and push notifications — **Long horizon**.
- Quick unavailable/“86” updates, daily-special publishing, happy-hour overrides, and emergency broadcasts — **Long horizon**.
- Mobile fleet health, offline alerts, last-seen evidence, manual publish, and remote content refresh — **Long horizon**.
- Brewery operations including keg-empty updates, rapid tap-list editing, and “Now Pouring” changes — **Long horizon**.

### AI-assisted work — formerly Phase 15

**Intent:** lower the skill required to produce, translate, arrange, and troubleshoot venue content while keeping people in control.

- Menu-description drafting, bulk description generation, naming suggestions, and human-confirmed allergen suggestions — **Long horizon**.
- Plain-language custom display generation with reusable templates, governed variables, sandboxed execution, usage tracking, and review before publish — **Long horizon / Policy pending**.
- Developer HTML/CSS editing with Monaco, a supported-variable reference, and live venue-data preview — **Long horizon / Policy pending**.
- POS-informed happy-hour suggestions, layout-position advice, and AI-assisted branded photo backgrounds — **Long horizon / Add-on candidate**.
- The page-aware support and engineering diagnostic agents described elsewhere in this inventory fit this phase conceptually, but require explicit privacy, consent, evidence, and access-control decisions — **Long horizon / Policy pending**.

### Analytics and smart automation — formerly Phase 16

**Intent:** use accumulated operational and commerce evidence to demonstrate reliability and help venues make better decisions.

- Screen uptime, downtime timelines, content impressions, POS-sales correlation, item-performance scoring, happy-hour ROI, and multi-location reporting — **Long horizon / Add-on candidate**.
- A/B testing for layouts and item positions, including measured comparison and an owner-approved winning configuration — **Long horizon / Policy pending**.
- Time-of-day and rules-based pricing, with demand-based pricing reserved for explicitly governed use cases — **Long horizon / Policy pending**.
- Context-driven experiences using foot traffic, weather, reviews, pairing suggestions, and live sports data — **Long horizon / Add-on candidate**.
- Outbound webhooks, automation-platform connections, and spreadsheet-driven synchronization with authentication, audit, failure handling, and manual fallback — **Long horizon / Add-on candidate**.

The former phase numbers are historical references only and no longer communicate planned sequencing. These concepts must be reconsidered against current architecture, privacy and AI policy, provider availability, pricing, and owner priorities before any can enter an approved track.

## Universal state vocabulary

Every designed control or surface should identify which of these conditions applies and offer the appropriate explanation or recovery:

| State family | Examples | Appropriate response |
| --- | --- | --- |
| Included and available | Ready, configured, permitted | Show the action normally. |
| Permission restricted | User lacks an action at this scope | Name the missing authority and who can resolve it. |
| Commercially locked | Advanced native outcome not included | Explain the outcome and offer plan review without false urgency. |
| Add-on not attached/configured | External capability absent or incomplete | Explain prerequisites, scope, setup, price path, and manual fallback. |
| Product/domain state | Sold out, closed, paused, canceled, relocated | Show the real operating state and authorized update/recovery. |
| Source condition | Stale, disconnected, conflicted, rate-limited | Show source, freshness, last-known-good value, override, and reconnect. |
| Delivery condition | Queued, partial, offline, failed, unacknowledged | Show affected targets, evidence, retry, correction, and safe persistence. |
| Limit condition | Warning, reached, reserved, pooled | Show unit, scope, calculation, consuming objects, and least-destructive remedy. |
| Privacy/rights/safety restriction | Sensitive, regulated, unsupported audience | Give the safe alternative or responsible review path; never upsell around it. |
| Rollout/temporary condition | Preview, staged, maintenance, disabled | Explain availability and timing without presenting it as a purchase decision. |

## Navigation implications

- Do not create one permanent navigation item per capability. Use stable customer jobs with contextual secondary navigation.
- Keep **Content**, **Screens**, and **Publish** conceptually distinct even when a workflow crosses them.
- Let **Home** surface exceptions and safe next actions rather than duplicate every management screen.
- Keep **Sources & Add-ons** separate from ordinary manual editing so loss of a provider never implies loss of core operation.
- Present **Team**, **Organization**, and **Plan & Billing** according to authority; keep their underlying concepts distinct.
- Advanced or future capability should have a truthful state and architectural home, but it should not clutter navigation before it is available and useful.

## Decisions still required

- Complete V1, V1.1, V2, and Later outcomes.
- Final capability names and stable identifiers.
- Final tier placement, add-on bundling, trials, prices, contracts, and regions.
- Exact allowances, pooling, grace, overage, retention, and grandfathering.
- Supported providers, hardware platforms, service levels, and responsibility boundaries.
- Privacy, rights, safety, regulated-content, AI, and data-governance policy.
- Final top-level navigation and which future surfaces stay hidden until available.

## Source authority

This design inventory synthesizes:

- `track0/CAPABILITY_MATRIX.md`
- `track0/consolidation/CROSS_INDUSTRY_MODEL.md`
- `track0/consolidation/TIER_AND_ADDON_ARCHITECTURE.md`
- `track0/consolidation/CUSTOMER_JOURNEY_VALIDATION.md`
- `track0/consolidation/EXISTING_PRODUCT_INVENTORY.md`
- `track0/consolidation/OWNER_APPROVAL_AND_IMPLEMENTATION_HANDOFF.md`
- the five native-industry required/optional capability sets under `track0/industries/`
- `docs/work-packages/RWP-01.06-track-1-lessons-learned-retrospective.md`
- `docs/archive/research/roadmaps/Vennu_Roadmap_v5.md` as historical provenance for the canceled Phase 14–16 groupings only

Where this document is broader than an approved source, it labels the item **Long horizon** rather than presenting it as committed product scope. Repository and owner decisions remain authoritative.
