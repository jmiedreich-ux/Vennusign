# Content Platform Architecture Renewal Program

**Status:** Owner-approved architecture direction and planning authority<br>
**Issue:** #939<br>
**Decision date:** 2026-08-28<br>
**Audience:** Product owner, engineers, technical designers, reviewers, and engineering agents

---

## 1. Purpose

VennueSign is moving from a menu-shaped product architecture to a controlled **content-and-presentation platform**.

A menu remains a first-class customer experience, but it must no longer be a permanent technical exception. The same foundation must let a restaurant express menus, a cinema express films and showtimes, and a future venue express whatever operational content it needs—without turning VennueSign into an uncontrolled generic CMS.

This program is the bridge between the currently implemented Menu feature and that foundation. It establishes the target architecture, the boundaries for a safe re-foundation, the workstream consequences, and the sequence that must occur before large code moves begin.

It does **not** authorize a broad rewrite in one change, a physical microservice split, customer-defined executable code, or a change to live customer behavior without a bounded implementation milestone.

## 2. The decision

The product is organized around this chain:

```
Industry profile + entitlements + permissions
    -> available content types
    -> versioned data model
    -> content records and content instance
    -> focused Content Builder experience
    -> compatible Theme revision
    -> immutable Content Release
    -> assigned logical screens / players
```

The roles are deliberately distinct:

| Layer | Owns |
|---|---|
| **Content type** | The named customer concept: Menu, Showtimes, Promotion, Event Board, etc. |
| **Data model version** | Meaning, structure, validation, field authority, states, editor hints, and theme-binding paths. |
| **Record library** | Typed, reusable, imported, or provider-owned operational facts. |
| **Content instance** | A venue's composed object, such as “Downtown Lunch Menu.” |
| **Content Builder** | The focused editing workflow appropriate to that type. |
| **Theme Studio** | The reusable visual structure and the response to model state. |
| **Content Release** | The exact, immutable combination approved for delivery. |
| **Display runtime** | Safe rendering, cached continuity, and applied-revision evidence. |

**Content Home** is the customer-facing catalog and lifecycle surface. A Menu appears there as one eligible content type; it is not the definition of the entire product.

**Content Builder** is the shared capability behind focused editors. The product should say “Items” in a menu and “Showtimes” in a cinema board when that is the natural language of the work. It must not force all content types into a generic form called Items.

## 3. Industry, entitlement, and model meaning

Industry is a way to make the product relevant, not a schema boundary.

- **Industry profile** chooses useful defaults and which types are relevant to a venue.
- **Entitlement and rollout** decide whether the organization can use a type or capability.
- **Permission** decides what an actor may do.
- **Data model version** defines the technical meaning and structure of content.

An industry, plan, or individual customer must not silently change the meaning of `menu.v1`. A plan can hide the Menu type or restrict an import capability; it cannot give two customers incompatible definitions of `price` or `availability` under the same model version.

## 4. Data-model contract

### 4.1 What a model defines

A released model is an immutable Vennue-controlled contract. It defines:

- scalar fields: text, rich text, money, number, date/time, media reference, enum, boolean, link;
- nested objects and collections;
- stable element identity requirements;
- validation, defaults, constraints, and localization behavior;
- fields that are venue-entered, inherited, provider-owned, derived, or system-managed;
- operational state fields and valid state values;
- collection source modes;
- editor labels, primary working collection, grouping, quick actions, and allowed operations;
- field paths which Theme Studio may bind to;
- model compatibility, migration, and deprecation rules.

The first released model is `menu.v1`. Its exact field list is a follow-on design deliverable, but it must support the already accepted Menu behavior: nested sections and items; price that belongs to the placement; availability/86 as an operational state; imported content; drafts and publish history; and stable references.

### 4.2 Collections have explicit source modes

A model collection declares one of these modes:

| Source mode | Example | Product behavior |
|---|---|---|
| Inline-owned | A small custom notice list | Rows belong only to this content instance. |
| Manual composition | A restaurant menu section | Operator selects/order records from a typed library and may apply permitted placement overrides. |
| Library reference | A speaker selected for an event session | The content instance references a reusable canonical record. |
| Provider query | Cinema showtimes today | The content resolves a scoped query over imported provider records; an operator does not place every row manually. |

A model may combine modes only when its contract names the relationship. The renderer and Content Builder must never infer provider authority from a loose JSON shape.

### 4.3 State is not layout

A **state** is a fact about a content element at a point in time. It is not an ordinary field slot and it is not a separate Theme Studio variant.

For Menu:

| Layer | Responsibility |
|---|---|
| Model | `availability` is an operational state; allowed values include available, sold-out, and unavailable. |
| State data | The Turkey Club is sold out at this venue now. |
| Theme revision | Keep the row, replace the price with “Sold out,” dim the item name, or use another approved response. |

This preserves the accepted rule that 86 is immediate and does not wait for a content publish. A player resolves:

```
last valid published content release
+ current valid state overlay
+ approved theme state response
= displayed presentation
```

Every state target must have a stable ID. “Third item in the section” is never an acceptable state identity.

### 4.4 Models are released, never edited live

A data model version is drafted, validated, released, and then frozen. A later change creates `menu.v2` or another explicit successor. A migration creates reviewed new content revisions; it never silently changes a live screen.

The same rule applies to theme revisions. A release pins a model version and a theme revision, not a mutable theme name.

## 5. Persistence architecture

### 5.1 Principle

The Data Model Studio does **not** create SQL tables dynamically. It produces a versioned definition stored in a small permanent registry. A content instance stores model-specific nested data, while tenancy, identity, revisioning, assignments, audit, release, and operational state stay relational.

Do not use:

- a table or column per customer-created model field;
- EAV field-value rows as the source of truth;
- a second generic CMS database;
- duplicate legacy and generic copies of the same content;
- mutable JSON that changes a release after publication.

### 5.2 Target relational ownership

Names below are conceptual, not a locked schema or migration contract.

| Area | Concepts | Purpose |
|---|---|---|
| Model registry | `ContentTypes`, `DataModels`, `DataModelVersions`, generated `DataModelFieldIndex` | Registry of immutable contracts, searchable field paths, validation metadata, and compatibility. |
| Eligibility | `ContentTypeEligibility`, capability/rollout policy | Makes a type relevant and permitted without altering model meaning. |
| Record library | `LibraryRecords`, `LibraryRecordRevisions`, provider identity and provenance | Canonical typed facts such as Toast items, films, showtimes, speakers, or reusable promotions. |
| Tenant content | `ContentInstances`, `ContentRevisions`, composition/reference rows | Venue-scoped content identity, authored drafts, immutable validated revisions, composition/order, and allowed overrides. |
| Operational state | `ContentStateValues` | Venue-scoped state overlays addressed by stable element ID and model state-field key. |
| Themes | `ThemeDefinitions`, `ThemeRevisions`, `ThemeModelBindings` | Immutable design artifacts that bind only to declared model fields and repeaters. |
| Delivery | `ContentReleases`, `RenderPackages`, `ContentDeployments` | Exact release tuple, package evidence, screen targets, requested/received/applied truth. |
| Shared infrastructure | `AuditEvents`, `OutboxMessages`, `MediaAssets` | Auditability, reliable workers, and media ownership. |

A `ContentRevision` can hold validated model JSON because collections are naturally nested. Its relational envelope owns organization, venue, instance identity, revision number, status, model version, actor, timestamps, checksum, and relationships. Searchable or operationally significant values gain an approved projection/index only after a real need appears.

### 5.3 The record library

The current Item Library becomes the first example of a **typed record library**.

For `menu.v1`:

```
Library record: Turkey Club
  shared facts: name, description, allergens, media
  operational state: sold out at this venue

Lunch Menu / Sandwiches placement
  reference: Turkey Club
  permitted override: price = $12.50

Late Night Menu / Sandwiches placement
  reference: Turkey Club
  permitted override: price = $13.00
```

The model defines which fields are shared, placement-overridable, provider-owned, or stateful. The current accepted rule remains intact: price belongs to the placement; availability is a venue-scoped operational fact.

Toast imports create or update typed menu-item records with provider provenance. Cinema integrations create films and showtimes as provider-owned records. A showtimes board can then query “active showtimes for this venue today, grouped by film” instead of manually placing every performance.

Changing a library record does not silently alter a live manual composition. The content model decides whether a dependent instance receives a draft refresh, an automatic managed update, or an explicit reconciliation task. Invalid provider input never replaces the last known valid canonical record.

## 6. Publishing and rendering contract

Publishing is a deliberate content action. A successful publish does not claim that a physical screen applied it.

A release pins at least:

```
content instance
+ validated content revision
+ model version
+ theme revision
+ resolved composition/query criteria
+ renderer contract version
+ target assignments
= immutable content release
```

The display runtime receives a safe render package or manifest derived from that release, retains the last valid package, applies valid operational state overlays, and reports requested/received/applied evidence. A state update may advance independently where the model declares it operational; an authored price, description, or structure change follows the normal draft-to-publish boundary.

## 7. Internal tool evolution

The product is built in this order:

1. **Schema registry/compiler** — model definitions may initially be code/config-assisted, but validation is server-side and model versions are explicit.
2. **Fixture and preview lab** — valid/invalid fixtures, representative sizes, state cases, and binding checks.
3. **Theme compatibility checker** — rejects a theme whose fields, repeaters, formatters, or state responses cannot resolve against the bound model.
4. **`menu.v1` vertical proof** — prove that the engine can validate, compose, release, and render the first real shared model.
5. **Data Model Studio** — the staff-only visual tool to construct, test, validate, release, deprecate, and migrate model versions once the engine is proven.
6. **Migration and impact console** — shows content, themes, releases, and integrations affected by a model successor.
7. **Controlled enterprise/partner configuration** — only if proven valuable; never arbitrary customer code.

The Data Model Studio is not a customer-facing schema designer in the initial product.

### 7.1 Planning sessions before implementation

The re-foundation needs several short, decision-focused planning sessions. They establish the engine
once; they do **not** attempt to design every future industry before development starts.

| Session | Decisions to settle | Outcome |
|---|---|---|
| 1. Current-source map | Current Menu data, APIs, UI behavior, database ownership, consumers, invariants, and verification gaps | A migration/acceptance inventory: what must survive and where it lives today. |
| 2. `menu.v1` model contract | Fields, nested collections, stable element IDs, placement overrides, drafts, validation, and operational state | The first real shared model specification and representative fixtures. |
| 3. Record library and provider authority | Manual records, Toast item ownership, film/showtime identity, queries, mappings, refresh, and last-valid behavior | Clear source modes and integration contracts without menu-shaped assumptions. |
| 4. Theme and release contract | Field/repeater bindings, state response, theme revision, compatibility, release/package contents, and player evidence | A renderer-safe contract that Theme Studio and players can share. |
| 5. Refactor and delivery sequence | Module seams, source moves, migrations, characterization tests, and the first vertical milestone | A bounded M1/M2 backlog that keeps one API host and avoids a rewrite-in-place. |

Menu and cinema/showtimes provide enough contrast to prove the foundation. New industries later reuse
the rules; they do not reopen the underlying engine by default.

### 7.2 The Data Model Studio follows the engine

The **Data Model Studio** is Vennue's planned internal model-builder: a staff tool, not the
first implementation milestone. Its UI must sit over a proven model engine that already supports `menu.v1`, validation,
binding, release, and migration safety.

Building the visual tool first would create a persuasive editor with no settled meaning behind its
controls. The correct order is:

```
planning decisions -> model engine -> menu.v1 proof -> Data Model Studio
```

Once the engine is proven, the Data Model Studio can safely let authorized Vennue staff define fields,
nested collections/repeaters, source modes, state sets, validation, editor behavior, and
compatibility rules; test them against fixtures and representative screens; then release an
immutable model version. It does not create database tables, alter live models, or accept
customer-written code.

## 8. Modular-monolith API direction

The immediate goal is one deployable API host, not multiple production services. `Vennu.Api` remains the HTTP, SignalR, worker, and composition host; the codebase gains real internal module boundaries.

| Module | Owns |
|---|---|
| Platform | Account, organization, venue, tenancy, authorization, entitlement, capability. |
| Content | Content types, data models, records, composition, revisions, state overlays, publishing contract. |
| Themes | Theme definitions, revisions, model bindings, compatibility validation. |
| Integrations | Provider connections, raw input, mapping, sync, provenance, retries, last-valid state. |
| Display Delivery | Content releases, package generation, deployment requests, applied evidence and reconciliation. |
| Operations | Internal support, release/cutover, fleet and operational workflows. |

A module owns its application services, public contracts, write paths, persistence boundary, migrations, invariants, and tests. Other modules use named application contracts or domain events; they do not reach across the database casually.

This is a **logical API split**, not separate containers. Later extraction is allowed only when scaling, failure isolation, release cadence, ownership, or security evidence justifies the operational cost.

## 9. Source cleanup for people and AI agents

The cleanup is a prerequisite to safe re-foundation, not an independent beautification exercise.

Each module needs a short source map recording:

- purpose and owned business language;
- routes, SignalR messages, hosted jobs, and public contracts;
- tables/migrations and read/write repositories;
- invariants and tests;
- inbound/outbound dependencies;
- tenant, provider-authority, and release boundaries;
- known legacy adapters and deletion conditions.

Refactoring rules:

- one clear responsibility per class/file;
- feature-focused organization of route, contract, use case, validation, persistence, and tests;
- no generic helper hiding a cross-domain write;
- explicit organization and venue scope at every boundary;
- no provider response model outside Integration;
- no persistence model exposed as an HTTP contract;
- preserve stable external routes during internal moves unless a deliberate contract version is approved;
- use characterization tests before moving behavior whose complete rule is not yet known.

The outcome is source that a human or agent can navigate without reconstructing the system from unrelated files.

## 10. Workstream disposition

| Workstream | Renewal position | Immediate consequence |
|---|---|---|
| Menus | First migration target | Preserve accepted behavior; stop extending menu-only persistence/theme/rendering architecture. |
| Theme Studio | Co-foundation | Formalize model bindings, state response, theme revision, compatibility, and renderer contract before display work resumes. |
| Connector Platform | Foundation proof | Design around typed provider records, source modes, provenance, retries, and last-valid state. |
| Screens / display delivery | Compatible consumer | Keep lifecycle/security repairs moving; adopt Content Release rather than a menu-specific payload. |
| Box Player | Later consumer | Must consume render packages and state overlays without assuming a browser-only delivery path. |
| Onboarding | Compatible later | Industry eligibility and entitlements become shared inputs; do not rebuild the flow now. |
| Platform Operations / Keystone | Compatible later | Release/version work remains needed, but PO and cutover are not a reason to split the content API physically. |
| Authentication / test harness / environment hygiene | Stabilize now | Repair verification and security defects before broad refactoring relies on them. |
| Foundry | Separate UI system | Inform future UI composition but do not block the content data/API foundation. |
| Atlas | Living map | Add architecture ownership, dependency, migration, and deployment metadata to its generated view. |

### 10.1 Cross-cutting and candidate-workstream disposition

| Workstream or candidate | Renewal position | Immediate consequence |
|---|---|---|
| Release versioning | Stabilize now; align with Content Release | Keep the existing manifest/version work independent, but ensure it can identify the exact API, renderer, package, model, and theme contract later. |
| Legacy retirement and environment hygiene | Stabilize now, then defer destructive retirement | Inventory dependency and data-retention consequences first; no table/App Service cleanup is assumed safe merely because it looks unused. |
| Test harness integrity | Stabilize now | Local verification is the current gate. Repair trustworthy targeted verification before broad source moves rely on it. |
| Authentication hardening | Stabilize now, separate feature | Security and identity defects do not wait for the content refoundation; preserve its approved authority and module boundary. |
| Display diagnostics | Remain compatible for later | Design it around Content Release/requested/received/applied evidence, not a menu-specific current-state view. |
| Mosaic release | Remain compatible for later | Mosaic is a release codename, not a module. It inherits the Content Platform contract but begins no implementation milestone here. |
| Public site | Defer / unaffected | It is out of the version equation and does not block this foundation. |
| Foundry | Remain separate | Its component system informs future UI composition but is not part of the data/API renewal. |
| Atlas | Migrate into planning support | Its generated map must surface module ownership, migration stage, deployment boundary, and renewal disposition once M0 reconciliation is complete. |

### 10.2 Menus: preserve behavior, replace the foundation

The new Menu implementation must retain these acceptance rules:

- imports and their truthful review/replace lifecycle;
- typed reusable item/library behavior;
- placement-specific price and deliberate multi-placement price updates;
- 86/sold-out as immediate venue-scoped operational state;
- draft, history, restore, publish, assignment, delivery, and fallback truth;
- currently agreed shelf/lifecycle behavior, including the intended filters, twelve-card compacting point, and Recently deleted path;
- provider authority and no silent cross-menu side effects.

The current Menu schema and APIs are a temporary implementation, not the target contract. Menu-specific cleanup that is needed for correctness may continue; new menu-only foundation expansion must not.

## 11. Program sequence

**M0 status: initiated, not complete.** This PR establishes the direction, durable program, and
first record amendments. It does **not** claim to have finished the source/ownership map or every
granular workstream reconciliation. Those are the next planning actions and must be completed
before selecting M1.

### M0 — architecture reconciliation and source map

1. Amend the Architecture Bible, Roadmap, and affected Menu decisions.
2. Establish this program as the durable source for the re-foundation.
3. Reconcile roadmap, project status, feature plans, tracker, and Atlas inputs so agents do not plan from conflicting milestone states.
4. Map existing source and database ownership; identify mixed-responsibility files, duplicate contracts, legacy tables, and verification gaps.
5. Classify every active/outstanding item: **stabilize now**, **migrate into the foundation**, **remain compatible for later**, or **defer**.

**Exit:** one reviewed map identifies the next bounded engineering milestone; no code behavior changes in M0.

### M1 — engineering base and module seams

1. Repair the local verification baseline and capture known exclusions.
2. Introduce internal module ownership without changing deployment topology.
3. Move or split only code covered by characterization tests.
4. Make tenant, provider, and release boundaries explicit.

**Exit:** one API host has traceable module seams and a trustworthy local gate for the touched areas.

### M2 — shared content foundation

1. Implement model registry/version validation.
2. Implement typed record-library identity, revision, provenance, and source modes.
3. Implement content instances/revisions/composition and state overlays.
4. Implement release tuple/package contract and outbox-driven background work.
5. Prove the foundation with `menu.v1` fixtures and binding validation before moving the live Menu editor.

**Exit:** a non-customer-facing vertical proof can validate, compose, release, and render a `menu.v1` package.

### M3 — Menu reimplementation

1. Rebuild Menu data/API behavior on `menu.v1`.
2. Migrate/reseed non-production data deliberately; preserve every accepted behavior through tests and owner workbook.
3. Retire legacy Menu write paths only after no consumer depends on them.
4. Do not run two competing sources of truth.

**Exit:** Menu is genuinely the first shared content type, not an adapter hiding permanent menu-only storage.

### M4 — Theme Studio foundation and display resumption

1. Persist immutable Theme Definitions/Revisions and Model Bindings.
2. Implement compatibility checking, field/repeater pickers, and state response.
3. Bind the Theme Studio manual workflow to `menu.v1`.
4. Resume Menu canvas, Board View, Play, and renderer work only against the real contract.

### M5 — follow-on attachment

Bring integrations, cinema/showtimes, screens, player packaging, onboarding, Platform Operations, and Keystone onto the proven contracts through individually planned milestones.

## 12. Guardrails and non-goals

- Do not use architecture cleanliness alone to justify breaking known customer behavior.
- Do not treat a theme as a mutable name; it is a versioned, bindable artifact.
- Do not make imported/provider data editable unless its provider contract explicitly allows a write.
- Do not let a generated content package claim it is applied without player evidence.
- Do not introduce an all-purpose “Content Builder” UI before two real content types prove the shared interaction pattern.
- Do not deploy separate APIs/containers merely because the source gains modules.
- Do not use chat history as implementation authority after this program is accepted; update the owning records instead.
- Do not start a product implementation milestone until its source map, boundary, acceptance behavior, migration, and verification plan are explicit.

## 13. Decisions still required before implementation

The direction is settled; these design details remain intentionally open:

1. The full first `menu.v1` field, collection, stable-ID, state, and override contract.
2. Which existing Menu behavior is retained unchanged versus deliberately redesigned during reimplementation.
3. Exact content-release/package payload and renderer compatibility rules.
4. Which provider-specific source modes and mapping contracts begin with Toast and cinema feeds.
5. Model migration UX, default migration behavior, and retention policy.
6. The first internal Data Model Studio workflow and required approval roles.
7. Final module namespace/folder conventions after the source map identifies current dependencies.
8. Exact architecture-renewal milestone order after verification-base findings are known.

These questions must be answered in the relevant feature records before the affected implementation milestone starts.

## 14. Definition of success

The renewal succeeds when a second content type can be added without inventing a second content architecture, while the Menu experience remains strong and purpose-built.

A future engineer should be able to answer, from repository records alone:

- what a model means and which version a release uses;
- where an imported fact lives and who owns it;
- which values are content, placement overrides, or operational state;
- which theme revision can render the content;
- exactly what a screen was asked to display and whether it applied it;
- which API module owns the change;
- how an existing milestone connects to the renewal; and
- the next bounded action required to move safely.

## 15. API Architecture vNext proposal

**Status:** Proposed API architecture; captured from the owner review on 2026-08-30.<br>
**Authority boundary:** This section proposes ownership surfaces and vocabulary. It does not approve individual endpoints, route shapes, schemas, migrations, or deployable-service splits. Those require the remaining owner questions and bounded implementation planning.

### 15.1 The four surfaces

VennueSign vNext exposes four purposeful API surfaces inside the modular monolith:

| Surface | Responsibility |
|---|---|
| **Vennue Core API** | Turns organization-level and venue-level customer decisions into controlled, effective desired state for each venue. |
| **Vennue Connect API** | Turns external data into controlled Core changes and sends approved Vennue data outward. |
| **Vennue Runtime API** | Delivers resolved output packages and operational overrides, then proves what each player output is actually showing. |
| **Vennue Platform API** | Lets authorized Vennue staff support, govern, configure, and operate the overall service without bypassing the owning APIs. |

The rejected surface names are **Management API**, **Integration API**, **Delivery API**, and **Player API**. They were too generic and did not express the boundaries. **Vennue API** was refined to **Vennue Core API** once its role as the business authority became clear.

The four short boundary statements are:

> **Core defines and controls desired state.**<br>
> **Connect exchanges and synchronizes external data.**<br>
> **Runtime applies desired state and reports actual state.**<br>
> **Platform governs and operates the Vennue service.**

Across the surfaces:

> **Connect determines what an external change means. Core determines what Vennue should do with it.**

> **Core decides what each venue should show. Runtime safely delivers the resolved package to each output and proves what is actually showing.**

> **Platform can govern and operate every Vennue surface, but it must never bypass the rules or ownership of Core, Connect, or Runtime.**

### 15.2 Deployment and contract direction

The surfaces are logical ownership and contract boundaries, not an instruction to deploy four services now.

- Keep one deployable ASP.NET Core host/App Service/container initially.
- Give every surface and family strict internal module ownership.
- A module owns its use cases, contracts, write paths, persistence boundary, migrations, invariants, tests, and emitted events.
- Modules interact through named application contracts or versioned events; they do not write one another's tables directly.
- Split a surface physically only when scaling, failure isolation, security, release cadence, or team ownership proves that the operational cost is justified.
- Use REST and JSON for public contracts unless a later use case proves another contract is necessary.
- Generate OpenAPI contracts and check the generated contracts into source control so contract changes are explicit and reviewable.
- Version public contracts from the beginning; representative paths use an explicit contract version such as `/api/v1`.
- Prefer business actions over database-shaped CRUD. Examples include validate, publish, rollback, assign, pair, reconcile, and set operational state.
- Use opaque time-sortable public identifiers such as UUIDv7 rather than exposing sequential database keys.
- Derive and enforce organization and venue scope on the server; never trust a client-supplied scope without authorization.
- Use optimistic concurrency through ETags or explicit revision numbers.
- Require idempotency for publishing, imports, pairing, provider callbacks, and other retryable commands.
- Use cursor-based pagination for large collections.
- Return stable error codes, field details, and correlation identifiers.
- Use a transactional outbox for reliable asynchronous work.
- Audit meaningful customer, provider, player, and workforce actions.
- Treat webhooks, events, and realtime messages as versioned contracts.
- Realtime notifications prompt reconciliation; they never replace the authoritative state read.

The API domain term is **Data Model**. **Content Type**, bare **Model**, and **Content Model** were considered and rejected as the primary domain term before settling on Data Model. Representative top-level Core language is `data-models`, `content`, `themes`, `screens`, `players`, `releases`, `assets`, `organizations`, and `venues`. The existing proposed internal authoring-tool name **Data Model Studio** is not renamed by this API proposal.

### 15.3 Multi-venue foundation

Multi-venue support is not an implementation detail. It governs tenancy, authorization, shared content, publishing, scheduling, and runtime delivery from the beginning.

The public conceptual hierarchy is:

```
Organization -> Venue -> Wall -> Screen -> Player Output
```

- An organization may have one or many venues.
- Organization-level objects are shared objects, not uncontrolled copies at every venue.
- A venue owns its local content, local configuration, permitted overrides, screens, walls, assignments, schedules, releases, and operational state.
- Screens and walls are logical targets. A player output is a physical/runtime endpoint bound to a logical screen.
- Organization-level content, models, themes, assets, presentations, brand standards, policies, and access rules may be shared where their owning contracts allow it.
- Each venue retains explicit state for shared material: running, pending, behind, not running, or locally overridden.
- An organization push may automatically apply, become pending, require venue acceptance, or preserve approved local overrides according to policy. The API must represent the policy rather than silently choosing.
- Runtime never resolves organization inheritance or venue override precedence.

Core resolves:

```
Organization content and policy
+ venue configuration
+ permitted local overrides
+ wall/screen assignments and schedule
= effective desired state for one venue
```

The foundational multi-venue rule is:

> **Core combines organization-level decisions, venue configuration, and permitted local overrides into effective desired state for each venue.**

### 15.4 Vennue Core API

The Core API is the business brain of Vennue. It owns what the customer intends the system to do.

Its basic model is:

```
Data Model -> Content ----+
                          +-> Presentation -> Publish -> Release -> Assignment -> Wall/Screen
Theme --------------------+
```

- A **Data Model** defines meaning, fields, relationships, validation, source authority, states, and binding paths.
- **Content** is the actual operational information: items, prices, descriptions, calories, films, showtimes, and other typed data.
- A **Theme** defines visual structure, zones, repeaters, fields, style, and behavior.
- A **Presentation** binds selected Content into a Theme for display.
- **Publishing** validates the complete intended change.
- A **Release** is the immutable approved version owned by Core.
- An **Assignment** says where and when the Release should apply.

Core contains four families and they remain explicit.

#### 15.4.1 Account and Business Structure

- Authentication
- Authorization
- Sessions
- Onboarding
- Organizations
- Venues
- Subscriptions and Billing

This family establishes who the customer is, where they operate, which context they are working in, and what they may use.

- **Authentication** proves human identity through sign-in, identity providers, factors, and token/session establishment.
- **Authorization** determines roles, permissions, organization scope, venue scope, and allowed actions.
- **Sessions** expose the signed-in actor's current organization, venue, and resolved capabilities.
- **Onboarding** coordinates creation of the organization, first venue, and first screen through the owning domains.
- **Organizations** are the commercial and shared-policy boundary.
- **Venues** are the operational boundary. The public term is Venue, not Site.
- **Subscriptions and Billing** expose customer-facing subscription state, usage, checkout, and billing-portal operations. Platform separately owns plan and tier administration.

Authentication and Authorization are intentionally separate. **Access** is not retained as one combined area. Player Authentication also remains separate in Runtime because it proves machine identity, not human identity.

#### 15.4.2 Content and Design

- Data Models
- Content
- Presentations
- Themes
- Assets

This family establishes what information exists and how it should appear.

Data Models are versioned, validated contracts rather than arbitrary JSON. Content remains purpose-built in the customer experience: a Menu can say Items while a cinema presentation can say Showtimes. Presentations are the displayable composition boundary:

```
Content + Theme = Presentation
```

Assets include images, video, fonts, and other reusable media. Themes own visual and behavioral rules; they do not own operational content.

#### 15.4.3 Display Planning

- Screens
- Walls
- Player Administration
- Assignments
- Scheduling

This family establishes where and when a presentation should appear.

- A **Screen** is a logical destination.
- A **Wall** is an ordered group of one or more screens. A standalone screen is a wall of one, allowing one consistent assignment model.
- **Player Administration** covers human decisions such as claiming a code, naming or replacing a player, binding a discovered output to a logical screen, and unpairing it.
- **Assignments** connect approved releases to logical walls/screens.
- **Scheduling** resolves time-based desired state on the server in the venue's timezone.

The preferred assignment concept is:

```
Presentation Release -> Wall
```

Core may present Runtime health and showing state as read-only composed views, but those facts remain Runtime-owned.

#### 15.4.4 Change Control

- Drafts
- Publishing
- Releases
- Operational Overrides
- Audit

This family establishes what is approved, what should go live, and who changed it.

The ordinary path is:

```
Normal authored change -> Draft -> Review -> Publish -> immutable Release
```

A publish operation preserves the full controlled workflow:

1. validate content and theme;
2. validate required fields and screen fit;
3. create an immutable release snapshot;
4. record assignments;
5. emit the desired-state change so Runtime can notify and reconcile the affected players;
6. preserve the previous release for rollback.

Operational state is a separate path:

```
Immediate operational fact -> Operational Override -> Runtime delivery
```

For Menu, an 86 is immediate and venue-wide. A normal authored availability/content change follows publish. An operational overlay remains independent of ordinary publishing until explicitly cleared; exact cancellation behavior must preserve the accepted Menu decisions, including cases where a cancellation is intentionally carried with a publish.

Core records desired state such as:

```
Lobby Wall should show Breakfast Release 7 at 7:00 AM.
```

Runtime records actual state such as:

```
Player 12 downloaded Package 44.
Output 2 applied it at 7:00 AM and is currently showing it.
```

Core owns the first statement. Runtime owns the second.

Core does not own provider transport/synchronization, player manifests/downloads/heartbeats, or Vennue's internal cross-tenant administration.

### 15.5 Vennue Connect API

The Connect API is more than provider-specific import endpoints.

Its basic pipeline is:

```
External system
-> immutable source input/snapshot
-> organization and venue resolution
-> mapping
-> ownership rules
-> validation
-> change set
-> controlled Core change
```

Connect never bypasses Core's ownership, validation, or publishing rules.

#### 15.5.1 Connections and Sources

- Connectors
- Connections
- Data Sources
- Transports
- Webhooks

- A **Connector** is Vennue's provider capability for Toast, Clover, Square, a cinema system, or another integration.
- A **Connection** is one customer's configured link to that provider.
- A **Data Source** is a catalog, menu feed, film feed, showtime feed, or other scoped source.
- **Transports** include pull REST, push REST, webhooks, and SFTP.
- A **Webhook** signals that work is available. It is not unquestioned business truth.

#### 15.5.2 Mapping and Control

- Venue Mapping
- Data Mappings
- Ownership Rules
- Validation Rules

This family determines where external data belongs, what it means, and which system is authoritative.

Representative mapping:

```
Provider account/location 1842 -> Downtown Venue
provider item.name          -> Menu Item Name
provider item.price         -> Default Price
provider item.available     -> operational availability
```

Field or content-area authority may be:

- Vennue controlled;
- integration controlled;
- integration default with a permitted local override;
- integration controlled across selected content/presentations;
- manually reviewed before acceptance.

Every imported field retains provenance sufficient to explain and reconcile the accepted value:

- source system;
- external ID;
- import timestamp;
- mapping version;
- ownership rule;
- last accepted value.

An imported price may deliberately control that price across selected menus/presentations when the configured ownership rule says so. It must never gain cross-content reach accidentally. Film and showtime identity is provider-owned where the connector contract declares it.

#### 15.5.3 Data Movement

- Imports
- Exports
- Sync Runs
- Source Snapshots
- Change Sets

- An **Import** brings external data into the controlled pipeline.
- An **Export** sends approved Vennue data outward.
- A **Sync Run** records one bounded execution.
- A **Source Snapshot** preserves exactly what the provider supplied.
- A **Change Set** records the proposed difference and resulting Core actions.

A sync run reports records received, matched, created, changed, rejected, held for review, Core changes produced, and final status. Paste/upload imports may use the same controlled concepts without pretending that a human-provided file is a live provider connection.

#### 15.5.4 Reliability and Visibility

- Connection Status
- Sync History
- Errors and Alerts
- Retries
- Reconciliation
- Connect Audit

On failure:

- retain the last valid canonical state and display content;
- never partially corrupt Core;
- preserve the failed input and correlation evidence;
- retry idempotently and boundedly;
- alert the appropriate operator;
- support investigation and safe replay;
- fail closed on unknown identity, venue mapping, ownership, or invalid schema.

The established targets remain up to 1,000 locations, urgent updates within seconds, 99.9% monthly availability, and recovery within two hours.

#### 15.5.5 Multi-venue Connect behavior

A connection may be organization-scoped or venue-scoped.

```
Provider Account
├── Location 101 -> Venue A
├── Location 102 -> Venue B
└── Location 103 -> Venue C
```

Organization-level mappings and ownership policies may be reused, with explicit permitted venue-specific rules. External locations must resolve to a known venue before a change can reach Core.

The result of a valid external change depends on policy:

- a normal price change may become a Core draft;
- a trusted showtime feed may auto-publish a managed update when policy explicitly permits it;
- an urgent imported availability fact may become a Core operational override;
- an ambiguous identity match may wait for human review;
- a rejected field leaves the last valid value unchanged.

### 15.6 Vennue Runtime API

Runtime turns Core's effective desired state into what each player output actually shows, then reports the result.

Its basic model is:

```
Core desired state -> approved Release -> output-specific Package
                   -> Runtime delivery -> Player Output -> actual-state evidence
```

Runtime does not choose content, resolve organization inheritance, decide assignments, or calculate business precedence.

#### 15.6.1 Player Identity and Topology

- Player Enrollment
- Player Authentication
- Pairing Status
- Output Discovery

- **Player Enrollment** establishes machine identity before or while a player is attached to a venue. Vennue-shipped hardware may use a one-time bootstrap token.
- **Player Authentication** uses restricted, rotatable, revocable machine credentials. A player can report only its own topology/state and retrieve only its assigned material.
- **Pairing Status** covers the player requesting and displaying a short code, polling whether it was claimed, and learning its assigned context.
- **Output Discovery** reports connector identity, EDID corroboration, resolution, geometry, orientation, connection state, and health.

The pairing boundary is split deliberately:

- Runtime: request code, show it, poll status, establish machine identity.
- Core: staff claims the code, approves the player, binds an output to a logical screen, replaces or unpairs the player.

One player can drive several physical outputs:

```
Player
├── Output 1 -> Lobby Screen
└── Output 2 -> Menu Screen
```

Port/connector identity and EDID identify the physical output. Neither is the wall position. Core owns the logical binding.

#### 15.6.2 Desired State and Delivery

- Desired State
- Package Delivery
- Asset Delivery
- Override Delivery

The desired-state contract is already resolved per authenticated output and may contain:

- package identity;
- Core release identity;
- screen and wall position;
- required assets and integrity hashes;
- schedule and activation time;
- operational overrides;
- fallback rules;
- minimum compatible runtime version.

**Release** and **Package** are different:

- A **Release** is the approved immutable presentation version owned by Core.
- A **Package** is the output-specific material delivered by Runtime.

One Core release can produce several packages because screens can have different geometry or positions within a wall. Players verify complete package integrity before replacing the currently working presentation. Assets are immutable and content-addressed where practical.

#### 15.6.3 Convergence and Evidence

- Package Status
- Showing State
- Synchronized Activation
- Reconciliation Notifications

Package evidence distinguishes:

```
Requested -> Downloading -> Received -> Verified -> Applied -> Showing
```

It also records failed, recovered, superseded, and stale. **Assigned**, **downloaded**, **applied**, and **currently showing** are not synonyms.

For a synchronized wall:

1. every required output downloads and verifies its package;
2. Runtime records readiness;
3. the server issues a shared activation time;
4. outputs switch on that clock;
5. excluded, late, stale, or failed outputs remain visible in evidence.

Push/SignalR notification is only a prompt:

> **Notifications prompt; desired-state reconciliation decides.**

A restart or reconnect performs an authoritative desired-state comparison. Missing a notification can never permanently prevent convergence.

#### 15.6.4 Health and Lifecycle

- Health Reports
- Diagnostics
- Runtime Updates
- Recovery

Health reports include runtime version, operating system, output state, showing package, storage, recent failures, and last successful reconciliation. Diagnostics expose support evidence without secrets or customer-administration authority.

Vennue Platform defines approved runtime versions, rollout rings, canaries, minimum versions, required updates, pause, and rollback policy. Runtime delivers and installs the selected update and reports the outcome.

Recovery retains the current working package, last-known-valid package, required cached assets, machine identity, and output bindings. A network, provider, or service failure must not create a blank screen. The player continues the last valid presentation and reconciles when connectivity returns.

### 15.7 Vennue Platform API

Platform is Vennue's internal operator API. Customers, external integrations, and players do not receive Platform authority.

Platform may observe and administer all surfaces through their supported commands, but it must not silently edit their databases.

#### 15.7.1 Customer and Commercial Operations

- Customer Support
- Organization Administration
- Venue Administration
- Subscription Support
- Plan and Tier Administration
- Revenue Reporting

Customer Support composes organization, venue, subscription, screen/wall, player health, publishing/showing, connector, failure, and audit information. Support access is role-restricted, reason-recorded, time-limited when elevated, audited, and read-only by default.

Core exposes customer subscription usage, checkout, and billing portal actions. Platform investigates failures, reconciles provider state, manages the plan/tier catalog, and applies approved commercial exceptions. Organization and Venue Administration include controlled recovery, suspension/reactivation, and structural correction use cases through the owning Core commands. Revenue Reporting includes trial conversion, subscription movement, and billing reconciliation failures as well as recurring-revenue views. Vennue retains billing identifiers and commercial state, not payment-card data.

#### 15.7.2 Entitlements and Configuration

- Feature Management
- Entitlement Policies
- Customer Exceptions
- Platform Configuration
- Environment Configuration
- Configuration History

Effective capability resolves:

```
Plan/tier
+ organization allowance
+ venue allowance
+ approved exception
= effective capability
```

Customer exceptions require reason, approver, effective time, expiration, and audit evidence. Platform Configuration covers safe operational thresholds, provider configuration references, renderer versions, retention rules, and feature defaults. Configuration supports concurrency protection, preview/diff, history, rollback, non-secret export, and reviewed environment application. Secrets are referenced and health-checked; their values are never returned.

#### 15.7.3 Fleet and Delivery Operations

- Fleet Monitoring
- Runtime Version Management
- Rendering Operations
- Package Delivery Monitoring
- Connector Fleet Monitoring
- Maintenance and Incidents

Runtime owns individual actual-state reports; Platform aggregates cross-tenant fleet health. Connect owns individual connection/sync records; Platform aggregates provider-wide health. Connector Fleet Monitoring makes provider-wide signals visible, including failure rate, stalled runs, expired credentials, webhook backlog, reconciliation failures, and provider outage.

Rendering Operations observes queue depth, compile/Wall Planner failure, renderer version, artifact-cache health, and package generation. A recompile after renderer change may regenerate packages without inventing a customer content revision.

Maintenance and Incidents cover planned maintenance, cutovers, provider outages, paused deployments, customer-impact evidence, recovery state, and incident timelines.

#### 15.7.4 Governance and Safety

- Workforce Authentication
- Workforce Authorization
- Privileged Actions
- Platform Audit
- Compliance Evidence
- Test Automation

Workforce access uses a separate audience and authority from customer authentication even if the identity provider is shared. It requires strong authentication, explicit environment access, short privileged sessions, and no automatic production authority.

Privileged actions require explicit capability, reason, target preview, confirmation where appropriate, and durable audit. Emergency/break-glass authority expires automatically.

Platform Audit records actor, action, target, previous state, new state, reason, time, correlation identity, and result. Core Audit records customer business changes; Platform Audit records Vennue administrative and operational actions.

Test Automation is authenticated, fully logged, test-environment-only, and technically absent or unreachable in production. It cannot mutate real customer information.

### 15.8 Cross-surface ownership rules

| Concern | Authority | Interaction |
|---|---|---|
| Human identity and customer permissions | Core | Platform uses separate workforce identity and authority. |
| Player machine identity | Runtime | Core owns staff approval and logical binding. |
| Customer desired state | Core | Connect proposes controlled changes; Runtime consumes the resolved result. |
| Provider meaning and field authority | Connect | Core decides the resulting draft, managed update, or operational override. |
| Published approval | Core Release | Runtime receives one or more output-specific Packages. |
| Physical output topology | Runtime | Core binds discovered outputs to logical Screens. |
| Screen/wall configuration | Core | Runtime reports actual delivery/showing state. |
| Individual player health | Runtime | Platform aggregates fleet health. |
| Individual connector health | Connect | Platform aggregates provider/connector fleet health. |
| Plan purchase/use | Core | Platform defines catalog/policy and supports exceptions. |
| Runtime rollout policy | Platform | Runtime delivers and reports the update. |
| Customer audit | Core/Connect/Runtime owner of the action | Platform Audit records Vennue workforce actions. |

A customer-facing composed read may show desired and actual facts together. Composition does not transfer authority: the response must retain enough provenance to distinguish Core desired state from Runtime actual state.

### 15.9 Endpoint-inventory mapping

The earlier projected endpoint inventory remains a separate candidate list. It is not consolidated into this proposal and its wording/order are preserved.

The companion record is:

- `docs/architecture/api-vnext-endpoint-inventory-mapping.txt`

Every candidate route group is annotated:

```
MAP -> primary Surface -> Family -> Area
INTERACTION -> another owner involved without transferring primary ownership
REVIEW -> a boundary issue that remains visible until explicitly decided
```

No endpoint is approved, discarded, renamed, or moved merely because its original section changes. The current mapping intentionally flags:

1. The content-entry state route must not conflate the immediate 86/restore operational contract with an authored **Not available** change that requires publish.
2. Presentation-list responses that say what screens show may compose Core presentation data with Runtime Showing State; the contract must preserve the distinction.
3. The screen-list response mixes Core desired configuration with Runtime actual state and device-reported output geometry; the composed contract must preserve ownership.
4. The screen-assignment response says what each screen is showing; assignment intent remains Core-owned while actual Showing State remains Runtime-owned.
5. Wall delivery state is Runtime-owned even when surfaced in a Core wall view.
6. The candidate Runtime display route is keyed by logical `{screenId}` even though delivery is authorized and resolved per authenticated Player Output; that key must be corrected or its translation proven safe.
7. The candidate diagnostics route cannot provide a centralized unauthenticated Runtime surface; it must be strictly local and non-sensitive or appropriately authenticated.
8. Stripe billing webhook ownership is not part of the customer-data Connect domain.

The inventory is a use-case and candidate-contract register beneath this architecture. It cannot redefine the four surfaces or their families.

### 15.10 Proposal guardrails

- Do not implement this section merely because it is present in the owner-approved renewal program; it is explicitly a vNext proposal pending the owner's remaining questions.
- Do not turn the four surfaces into four production services without evidence.
- Do not collapse Authentication and Authorization back into a generic Access area.
- Do not replace Venue with Site in public API language.
- Do not use Device where the established concept is Player.
- Do not put generic Operations back inside Core; customer desired-state control and Vennue service operation have different authorities.
- Do not call a Runtime Package a Core Release or claim that publish means showing.
- Do not let Connect write published Core tables directly.
- Do not let Platform bypass owning commands, authorization, invariants, or audit.
- Do not hard-code restaurant-only scheduling or promotion concepts into the shared model when Data Models can express the durable meaning.
- Do not expose dynamic customer SQL schema, EAV truth, arbitrary executable mapping, or customer-authored code.
