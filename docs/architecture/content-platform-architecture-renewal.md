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
    -> immutable Published Presentation
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
| **Published Presentation** | The exact, immutable combination of approved content and Theme that a Screen may use. |
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
last valid Published Presentation
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
| Record library | `LibraryRecords`, `LibraryRecordRevisions`, provider identity and source history | Canonical typed facts such as Toast items, films, showtimes, speakers, or reusable promotions. |
| Tenant content | `ContentInstances`, `ContentRevisions`, composition/reference rows | Venue-scoped content identity, authored drafts, immutable validated revisions, composition/order, and allowed overrides. |
| Operational state | `ContentStateValues` | Venue-scoped state overlays addressed by stable element ID and model state-field key. |
| Themes | `ThemeDefinitions`, `ThemeRevisions`, `ThemeModelBindings` | Immutable design artifacts that bind only to declared model fields and repeaters. |
| Delivery | `PublishedPresentations`, `RenderPackages`, `ContentDeployments` | Exact approved presentation, package evidence, screen targets, requested/received/applied truth. |
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

Toast imports create or update typed menu-item records with explicit data source and change authority. Cinema integrations create films and showtimes as provider-owned records. A showtimes board can then query “active showtimes for this venue today, grouped by film” instead of manually placing every performance.

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
| Integrations | Provider connections, raw input, mapping, sync, source history, retries, last-valid state. |
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
| Connector Platform | Foundation proof | Design around typed provider records, source modes, data source and change authority, retries, and last-valid state. |
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

## 11. Renewal program and Mosaic version planning

**Status:** The Content Platform Architecture Renewal and API Architecture vNext blueprint are
approved through #939. The owner added the Mosaic version-planning direction on 2026-08-30 through
issue #965.

The renewal stages and product versions are two connected planning axes:

| Axis | Meaning |
|---|---|
| **Renewal M0–M5** | The kinds of architecture-renewal work: reconciliation, module seams, shared content foundation, Menu migration, Theme/display foundation, and later attachments. |
| **Mosaic V1, V1.x, V2+** | The coherent product outcomes released to customers and the blueprint capabilities each version completes. |

Mosaic planning joins the renewal; it does not replace it. The Mosaic capability/dependency map
selects which renewal-stage outcomes must happen first, which existing capabilities already satisfy
them, what may run in parallel, and what remains for a later version.

### 11.1 Renewal stages

The stages remain the program's architectural work structure. They are not a rule that every item in
one stage must be completed across the whole product before any work associated with another stage
can begin.

#### M0 — architecture reconciliation and Mosaic dependency map

1. Maintain the Architecture Bible, Roadmap, API blueprint, and affected feature decisions.
2. Run the Mosaic V1 Renewal Reconciliation Session.
3. Define the observable Mosaic V1 outcome.
4. Map the capabilities and dependencies required for that outcome.
5. Inspect existing source, data, contracts, behavior, and tests only for the selected dependency path.
6. Classify each selected capability as **reuse**, **reshape**, **build**, or **defer**.
7. Identify contract gates, coordinator lanes, queue blockers, and later-version boundaries.

**Exit:** one reviewed map identifies the smallest coherent Mosaic release and its first bounded work
packages. M0 changes no product behavior.

#### M1 — engineering base and module seams

- Repair verification only where Mosaic work depends on it.
- Introduce internal ownership seams required by the selected capabilities without changing the
  one-host deployment topology.
- Move or split only code protected by appropriate characterization evidence.
- Make tenant, provider, release, and runtime boundaries explicit where the Mosaic path crosses them.

#### M2 — shared content foundation

- Establish the Data Model, typed-record, content-revision, state-overlay, release, and package
  contracts required by Mosaic.
- Prove the relevant foundation with `menu.v1`.
- Reuse existing working behavior where it satisfies the renewed contract; do not rebuild it for
  architectural symmetry.

#### M3 — Menu on the shared foundation

- Evolve the existing Menu implementation into the first real shared content type.
- Preserve accepted Menu behavior and useful existing code.
- Replace or retire a legacy path only when the selected version requires it and no consumer still
  depends on it.
- Never operate two competing sources of truth.

#### M4 — Theme, presentation, and display foundation

- Bind Theme revisions and Presentations to the Data Model and Release contract.
- Establish compatibility and state-response behavior required by the selected release.
- Resume canvas, Board View, Play, and renderer evolution only against the real shared contract.

#### M5 — follow-on attachment

Attach Connect, additional content types, Screens/player evolution, Onboarding, Platform Operations,
Keystone, and other blueprint areas through individually planned version capabilities. A capability
may enter Mosaic earlier when the dependency map proves the Mosaic outcome needs it.

### 11.2 Mosaic V1 Renewal Reconciliation Session

One additional owner/architect reconciliation session occurs before the capability/dependency map.
It reconciles planning authority, not the entire source tree.

The session must:

- align the Architecture Renewal M0–M5 stages with the four API surfaces and their families;
- reconcile the Mosaic release intent with current Menu, Theme Studio, Screens/player, Connect,
  Platform Operations, Keystone, onboarding, and other relevant feature records;
- distinguish current built behavior, approved design, proposed design, active work, blocked work,
  and unplanned ideas;
- identify decisions already settled, contradictions between controlled records, genuine open
  owner questions, and assumptions that must not enter the map as facts;
- agree on the observable Mosaic V1 outcome and the first dependency hypothesis;
- decide which records the Mosaic map will treat as authority and which are evidence only;
- name areas that require bounded source inspection after the session.

The output is a reconciliation record and a clean input set for the Mosaic map. It does not approve
implementation and must not expand into a whole-product code audit.

### 11.3 Version rule

A product version is a coherent outcome, not a token checklist slice from every blueprint family.

- Do not add a small piece of Core, Connect, Runtime, and Platform merely for symmetry.
- Do not rebuild Authentication, Authorization, venue context, Menu behavior, publishing, or
  delivery merely because they occur early in a conceptual diagram. Reuse them when sufficient.
- Do not require every future feature to be known before planning Mosaic. New discoveries update the
  map and are placed into the earliest version that truly needs them.
- Do not perform a whole-product code audit. Source discovery is just in time and bounded to the
  selected dependency path.
- Every later version names the blueprint capability it completes, deepens, replaces, or adds.

### 11.4 Mosaic V1

**Mosaic V1** is VennueSign's first coherent release of the renewed blueprint.

The initial planning spine is:

```
existing sign-in and venue context
-> menu.v1 Data Model
-> Content
-> Theme / Presentation
-> Publish immutable Release
-> Assign to Wall / Screen
-> Runtime displays it
-> Runtime proves actual Showing State
```

This is a dependency hypothesis to evaluate, not an instruction to rebuild every node. Existing
authentication and venue behavior may already be sufficient. Existing Menu, theme, publishing,
assignment, rendering, and display-delivery code is evidence and potential foundation, not
disposable legacy.

Mosaic includes the smallest set of capabilities that makes this path real, safe, supportable, and
verifiable. Connect and Platform capabilities enter Mosaic only where the outcome needs them;
security, tenancy, authorization, release truth, rollback, and runtime evidence remain mandatory
where the path crosses them.

### 11.5 Mosaic V1 Capability and Dependency Map

The next renewal artifact is the **Mosaic V1 Capability and Dependency Map**. It is completed before
implementation work packages.

For every selected capability, record:

| Field | Required decision |
|---|---|
| Blueprint home | Surface, family, and owning area |
| Renewal stage | M0–M5 work classification involved |
| Mosaic outcome | What must be observably true for V1 |
| Current evidence | Existing code, data, behavior, tests, and deployed proof |
| Disposition | **Reuse**, **reshape**, **build**, or **defer** |
| Dependencies | Capabilities and decisions that precede or constrain it |
| Contracts | Inputs, outputs, invariants, ownership, and version boundary |
| Verification | How the release proves the capability end to end |
| Work ownership | Coordinator lane, files/modules, shared gates, and queue blockers |
| Later completion | What remains for V1.1, V1.2, V2, or later |

The source investigation follows this map far enough to make a sound disposition decision; it does
not inventory unrelated product code.

### 11.6 Independent Mosaic V1 blueprint study

The independent roadmap study requested by the owner is a companion to this renewal:

- [`mosaic-v1-independent-blueprint-study.md`](mosaic-v1-independent-blueprint-study.md) — the
  detailed engineering and future-agent record;
- [`mosaic-v1-independent-blueprint-study.html`](mosaic-v1-independent-blueprint-study.html) — the
  same findings in a plain-language, self-contained owner report.

The study accounts for all four API surfaces, 16 families, 82 named blueprint areas, and 156 mapped
candidate route groups. It independently tested the current dependency hypothesis against credible
alternatives and retained the content-to-presentation-to-display path as the Mosaic V1 integration
spine for software-building reasons, not merely because current work is concentrated in Menu.

Its sequencing correction is authoritative planning input for the coming reconciliation and map:
begin with the observable first-live-screen acceptance journey and the smallest irreversible
boundary contracts. Data Model is the first semantic contract gate, but it need not be the first
production code. A guarded fixture-backed walking skeleton may expose cross-surface risks while
Core, Theme, Runtime, Connect data-source/change-authority work, Platform read support,
authentication/tenant sufficiency, and existing-path characterization proceed in parallel. Shared
contracts, migrations, Published-Presentation/Package integration, and cutover remain serialized.

The study is advisory. It does not approve implementation, endpoint shapes, schemas, service
splits, migration work, or a final Mosaic capability set. Its unresolved owner decisions and
evidence limits must be reconciled before the capability/dependency map authorizes work packages.

### 11.7 Version evolution

- **Mosaic V1** delivers the first coherent vertical path through the renewed blueprint.
- **V1.1, V1.2, and later V1.x releases** deepen or attach specifically named capabilities.
- **V2 and later major versions** introduce larger outcomes or materially complete new blueprint
  areas. A second real content type is an important proof, but its exact version is decided from the
  map rather than assumed here.
- Reusing a mature capability unchanged still appears in the map so its contract and evidence are
  explicit.

### 11.8 Parallel cloud-coordinator model

Parallelism follows the dependency graph, not the number of available agents.

- A coordinator receives a bounded capability lane, explicit file/module ownership, inputs,
  outputs, acceptance evidence, and queue blockers.
- Independent lanes proceed together only when their shared contract is settled or one lane is
  explicitly producing the contract the others await.
- Work changing the same invariant, contract, migration boundary, or controlled record remains
  serialized through its owning coordinator.
- Each coordinator queue may contain several work packages, but a blocked package does not start
  merely because its coordinator is free.
- Likely early lanes include the `menu.v1`/Core content contract, theme and renderer compatibility,
  Runtime package/evidence, and bounded characterization of the existing path. The reviewed map,
  not this example list, authorizes their order.

### 11.9 Planning order and exit

1. Run the Mosaic V1 Renewal Reconciliation Session and settle the authoritative input set.
2. Define the observable Mosaic V1 product outcome and acceptance boundary.
3. Build the capability/dependency map and connect each capability to its renewal stage.
4. Inspect only the existing implementation needed to classify those capabilities.
5. Mark each capability reuse, reshape, build, or defer.
6. Identify shared contract gates and safe parallel coordinator lanes.
7. Place incomplete blueprint facts into V1.1, V1.2, V2, or later.
8. Create renewal milestones and bounded work packages from the reviewed map.

**Planning exit:** the architect can explain why each Mosaic capability is needed, which renewal work
it requires, what existing foundation it uses, what precedes it, how it is verified, what can run in
parallel, and what is deliberately left to the next version.

### 11.10 Final Mosaic V1 Renewal Reconciliation — 2026-08-31

**Status:** Owner-approved planning authority. This section completes the M0 reconciliation
session. It supersedes earlier Mosaic wording in this document where the terms or planning order
conflict. It authorizes no endpoint, schema, migration, service split, implementation packet, or
Maestro operational state.

#### Product boundary

Mosaic V1 is a **private pilot**, not a public launch. It starts with an existing authorized
customer and an existing venue. The pilot proves one logical Screen (a wall of one), one paired
Player Output, one VennueSign-owned Default Theme, manual or paste-imported Menu content, and an
always-on assignment. Multi-venue safety is enforced underneath but does not need a visible
multi-venue demonstration.

> An existing authorized operator at one existing venue can create or paste-import a Menu, use the
> VennueSign Default Theme, publish an immutable Published Presentation, assign it always-on to one
> logical Screen, pair one Player Output, and prove the correct Runtime Package is actually showing.
> They can immediately 86 an item without republishing, recover through a failure using the last
> valid display, roll back safely, and see support evidence that separates desired state from actual
> state.

#### Frozen `menu.v1` meaning for the pilot

`menu.v1` is a centrally controlled, versioned Data Model. A customer, venue, Theme, or import
cannot redefine its meaning. A future breaking change is `menu.v2`.

| Element | Pilot meaning |
|---|---|
| Menu | Named container for ordered Sections. |
| Section | Name, optional description, and order. |
| Item | Reusable identity: name, optional description, optional image/media reference. |
| Placement | The Item's occurrence in a Menu/Section. It owns order and price. Any size label belongs with its price entry. |
| Stable identity | State targets stable Item ID plus venue; display position is never an identity. |
| States | **Available**; **Sold out / 86** (Live); **Not available** (Published). No speculative state catalogue belongs in `menu.v1`. |
| Paste import | One-time creation or replacement of ordinary draft content. Ambiguous material is asked about; a later paste never silently changes live content. Existing active 86 state remains active. Source history is retained. |

An Item may appear in many places. A price never belongs to the reusable Item: it belongs to its
Placement. `featured` remains a compatibility question, not an automatic V1 field. There is no
generic extra-fields mechanism; material future fields require a successor Data Model.

#### Theme, publication, and runtime vocabulary

| Name | Meaning |
|---|---|
| **Mosaic V1** | The product version: this private pilot. |
| **Deployment** | A VennueSign software version installed in an environment. |
| **Default Theme** | VennueSign's product-owned, ready-to-use visual system for one Data Model version. For `menu.v1`, it contains complete visual style, layout, bindings, and state responses. |
| **Published Presentation** | Frozen approved answer to what a Screen is allowed to show: validated Menu revision, `menu.v1`, exact Theme version, referenced assets, and renderer compatibility. It excludes Live state overlays. |
| **Runtime Package** | Player Output-ready result of a Published Presentation, created for a specific Player Output. |
| **Showing** | Runtime's evidence that a Player Output received, verified, applied, and is currently displaying a Runtime Package. |

The flow is:

```
Draft Menu + Default Theme -> Publish -> Published Presentation
-> assign to Screen -> Runtime Package -> actually Showing
```

The Default Theme is designed once the minimum `menu.v1` fields, state semantics, and bindings are
stable. Its design and renderer work may proceed in parallel with Core and Runtime, but the exact
version is compatibility-checked and pinned before the pilot leaves fixtures. A newer Default Theme
never silently changes a live Published Presentation.

Publishing creates a new immutable Published Presentation; the previous one is preserved for
rollback. Assignment is separate and states where it applies. Live 86/Sold Out is not republished:
it is a venue-level Runtime-applied overlay. A Player Output keeps its last known valid Runtime
Package during a connection failure and reconciles safely after recovery.

#### API transition and ownership

VennueSign remains one deployed application while it gains strict internal modules:

| Module | Owns for Mosaic |
|---|---|
| Core | Customer/venue scope, `menu.v1`, draft content, Published Presentation, and desired assignment state. |
| Connect | Paste/import boundary and data-source/change-authority rules. |
| Runtime | Player Output identity, Runtime Package delivery, Live state application, and Showing evidence. |
| Platform | Read-only support views composed from owned facts. |

New API paths are introduced capability by capability. An old path remains only until each consumer
has moved to its named new home, then it is removed. There is never a second writable truth or a
permanent adapter. Each meaningful module has a concise Markdown guide beside its source describing
ownership, public contracts and terms, allowed calls, data and tests, legacy replacement, and exact
retirement condition.

#### Approved work graph and safe parallelism

The first graph node is **M1-A — API module foundation and fixture-backed first-live-screen
skeleton**. It is not an implementation packet yet. Its purpose is to establish the module seams,
neutral fixtures, and observable path without claiming that fixtures are production authority.

| Lane | Entry gate | Must prove before it can join the pilot |
|---|---|---|
| Core | Shared IDs, `menu.v1`, state, and Published Presentation definitions | Validated draft-to-publish desired state. |
| Runtime | Shared Player Output and Runtime Package definitions | Received, verified, applied, Showing evidence and last-valid recovery. |
| Default Theme / renderer | Stable `menu.v1` bindings | Normal, long-content, Sold Out, and Not Available rendering. |
| Paste import | Core's controlled draft boundary | Reviewed one-time input with no live-link side effect. |
| Platform support | Runtime evidence vocabulary | Desired and actual facts visibly separated. |
| API migration | Each module's replacement contract | Consumer move and old-path retirement condition. |

The architect owns shared IDs, field paths, state names, Screen/Player Output meaning, data-source
and change-authority rules, Published Presentation, Runtime Package, and Showing vocabulary. A lane
may be queued before its gate opens, but remains Blocked until its named contract is ready. Core plus
Theme create a Published Presentation; Runtime then proves Showing; paste import and support evidence
join after those meanings are stable.

#### Maestro registration readiness

VennueSign and Maestro are independent projects. VennueSign remains authoritative for product
architecture, code, decisions, plans, engineering rules, and GitHub history. When Maestro is ready,
it will first perform its required **read-only discovery** and propose a thin
`maestro.project.yaml` binding. That binding will point to this Renewal, the current handoff, project
rules, branch/PR/review policy, validation commands, specialist overlays, environments, and declared
exceptions. It will not duplicate the Renewal, create a second project plan, or grant Maestro product
authority.

Before any graph becomes dispatchable through Maestro, VennueSign must commit an owner-approved graph
revision with source base, authority references, node outcomes and non-goals, dependencies, shared
boundary locks, specialist/reviewer routes, acceptance proof, and bounded quality contracts. Maestro
then projects that graph into operational queues; it does not edit it. No manifest, queue state, or
implementation packet is created by this reconciliation.

#### Immediate next controlled action

Independently review this reconciliation against the accepted decisions and current controlled
records. After any required correction has targeted verification, merge the documentation update.
Only then design M1-A as a bounded, Maestro-compatible implementation packet; its Decision Fidelity
Review must precede dispatch.

## 12. Guardrails and non-goals

- Do not use architecture cleanliness alone to justify breaking known customer behavior.
- Do not treat a theme as a mutable name; it is a versioned, bindable artifact.
- Do not make imported/provider data editable unless its provider contract explicitly allows a write.
- Do not let a generated content package claim it is applied without player evidence.
- Do not introduce an all-purpose “Content Builder” UI before two real content types prove the shared interaction pattern.
- Do not deploy separate APIs/containers merely because the source gains modules.
- Do not use chat history as implementation authority after this program is accepted; update the owning records instead.
- Do not start a product implementation milestone until its source map, boundary, acceptance behavior, migration, and verification plan are explicit.
- Do not turn Mosaic into a whole-product audit or make it wait for every blueprint family.
- Do not rebuild a mature capability without a version dependency that its current contract cannot satisfy.
- Do not distribute one unresolved shared invariant across parallel coordinators.

## 13. Decisions still required before implementation

The direction is settled; these design details remain intentionally open:

1. The full first `menu.v1` field, collection, stable-ID, state, and override contract.
2. Which existing Menu behavior is retained unchanged versus deliberately redesigned during reimplementation.
3. Exact content-release/package payload and renderer compatibility rules.
4. Which provider-specific source modes and mapping contracts begin with Toast and cinema feeds.
5. Model migration UX, default migration behavior, and retention policy.
6. The first internal Data Model Studio workflow and required approval roles.
7. Final module namespace/folder conventions after the source map identifies current dependencies.
8. The exact Mosaic V1 capability set and dependency order after the bounded map evaluates current evidence.
9. Which incomplete blueprint capabilities belong in V1.1, V1.2, V2, or later.

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

Every imported field retains source history sufficient to explain and reconcile the accepted value:

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

A customer-facing composed read may show desired and actual facts together. Composition does not transfer authority: the response must retain enough source-and-ownership detail to distinguish Core desired state from Runtime actual state.

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
