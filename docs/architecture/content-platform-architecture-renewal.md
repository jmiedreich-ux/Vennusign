# Content Platform Architecture Renewal Program

**Status:** Owner-approved architecture direction and planning authority  
**Issue:** #939  
**Decision date:** 2026-08-28  
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
