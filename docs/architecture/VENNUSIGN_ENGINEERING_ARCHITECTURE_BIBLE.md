# VennuSign Engineering Architecture Bible

**Status:** Canonical engineering orientation document  
**Repository:** `jmiedreich-ux/Vennusign`  
**Last consolidated:** 2026-08-28 (amended)  
**Audience:** Engineers, reviewers, technical designers, and engineering agents

---

## 1. Purpose

This document is the single engineering architecture reference for VennuSign.

It consolidates the durable architectural decisions currently distributed across `AGENTS.md`, `AI_DEVELOPMENT_GUIDE.md`, `docs/architecture/`, selected operations records, approved feature records, and explicitly marked proposals. It is intended to answer:

- What VennuSign is and what its major parts are.
- Which component owns each responsibility.
- Where data, identity, authorization, entitlements, and publishing authority live.
- How content reaches screens and how the system behaves when delivery is delayed or unavailable.
- How engineers should change the system safely.
- Which decisions are confirmed, which are implemented foundations, which are proposed, and which remain open.

This document is an orientation and cross-system consistency reference. Feature-level decisions, current code, `AGENTS.md`, exact migration files, and live repository state remain more authoritative for their specific subjects.

---

## 2. Authority and decision status

When two documents disagree, use this order:

1. Current code and database behavior.
2. `AGENTS.md` for engineering policy and workflow.
3. The active feature's `decisions.md` and question register.
4. This architecture bible for cross-system architecture.
5. Living architecture and operations records.
6. Design records that have not yet been promoted into an active feature.
7. Archived plans, historical reports, old handoffs, and conversation history.

A statement in this bible must carry one of these meanings:

- **Confirmed** — an intentional architectural decision.
- **Implemented** — confirmed and present in the current repository, subject to the documented proof boundary.
- **Proposed** — a design direction that is not implementation authority.
- **Open** — deliberately unresolved and requiring a decision before implementation.

Proposed material must not be implemented merely because it appears in this document. An owner-approved design must be placed in the relevant feature directory and reconciled with this bible.

---

## 3. Product definition

VennuSign is a multi-tenant digital signage platform for venues and location-based businesses.

A customer organization manages one or more venues. A venue manages operational content—such as menus, promotions, schedules, and displays—and assigns that content to physical or hosted screens. Screens are connected through a player runtime. The back office is the customer-facing operating surface; Platform Operations is the internal support and release surface.

The product's core promise is **truthful, controllable presentation**:

- the customer sees what the server says is allowed and published;
- the screen shows the last content known to be valid;
- operators can distinguish requested, delivered, applied, stale, and failed states;
- permissions, plan entitlements, capacity, and provider authority are enforced by the server;
- a network or device problem does not silently erase a valid screen presentation.

The primary product surfaces are:

- **Back Office** — customer and venue operations.
- **Platform Operations** — internal organization, support, entitlement, release, and fleet operations.
- **Display** — the hosted player web application that renders published content.
- **TV/platform shells** — native wrappers that launch or host the display player on supported platforms.
- **API and services** — HTTP, SignalR, authentication composition, hosted workers, domain use cases, persistence, and integrations.
- **Test API and acceptance tooling** — non-production tools that exercise real product endpoints without becoming a second domain implementation.

---

## 4. System boundaries

### 4.1 Solution architecture

| Boundary | Responsibility | Must not own |
|---|---|---|
| `Vennu.Api` | HTTP contracts, controllers, authentication composition, SignalR, hosted services, API composition | UI-specific business rules, provider response models, persistence-specific types |
| `Vennu.Core.Models` | Shared provider-neutral domain models and canonical types | HTTP, SQL, RepoDB, provider SDK types |
| `Vennu.Data` | VennuSign repositories, persistence behavior, database scripts, migrations | Generic provider infrastructure |
| `Vennu.DataAccess` | Reusable generic data-access/provider infrastructure | VennuSign-specific domain meaning |
| `src/back-office` | Customer and venue operations UI | Server authority, entitlement decisions, persistence |
| `src/platform-operations` | Internal support and platform operations UI | Customer-facing authorization decisions |
| `src/display` | Hosted display/player SPA and rendering behavior | Visual Studio Website-project coupling, back-office concerns |
| `src/tv` | Native/platform wrappers and distribution packages | The source of truth for display content |
| `Vennu.TestApi` | Environment-scoped deterministic test seeding through product APIs | Direct database/domain mutations or production use |

The architectural dependency direction is:

```
UI and platform shells -> HTTP/SignalR contracts -> API/use cases -> domain models
                                                -> repositories/infrastructure -> database/providers
```

External provider response types never cross into domain or public contracts. They are translated at the integration boundary into owned VennuSign contracts.

### 4.2 Deployment shape

VennuSign is designed as a modular application deployed through independently versioned components. The initial system may run as a small number of deployable applications and managed services. It is not required to begin as a fleet of microservices.

The governing rule is:

> Start modular; split a component only when scaling, failure isolation, deployment independence, or ownership proves that the split is worth its operational cost.

The architecture supports later separation of workers, platform operations, integrations, and display delivery without making those boundaries mandatory prematurely.

---

## 5. Tenant and ownership model

The ownership hierarchy is:

```
Customer account -> Organization -> Venue -> Screen -> Player/output assignment
```

The exact database entities and names are defined by the current schema and feature records. The conceptual rules are:

- A customer account identifies a person who can access one or more organizations.
- An organization is the commercial and entitlement boundary.
- A venue is the operational boundary for menus, schedules, screens, and venue-local settings.
- A screen is a logical content target. It represents what should be shown, regardless of which physical device currently renders it.
- A player is a physical or runtime device registration.
- A player output is one addressable display output on a multi-output player.
- A physical replacement should preserve the logical screen's content, history, and wall position when the workflow explicitly supports replacement.
- Server-derived organization, venue, screen, player, and output ownership is authoritative. Clients may select a context, but they may not manufacture ownership.

Every read, write, claim, pairing action, and provider callback must be scoped to the correct organization and venue boundary. Cross-tenant access must fail closed.

---

## 6. Identity, authentication, authorization, and entitlements

### 6.1 Identity

VennuSign separates:

- administrative/technical identity used by internal platform operations;
- customer account identity;
- organization membership;
- venue and screen ownership;
- external provider identity;
- device/player identity.

A provider subject change for an already verified customer email must be handled deliberately. Same-provider identity healing and idempotent repeated callbacks are allowed where the identity architecture specifies them. Third-party provider boundaries remain explicit; do not silently merge identities from unrelated providers.

Authentication supports the current customer flows and the repository's configured identity providers. Strong authentication, passkeys, TOTP, recovery, Google signup, local development, and branded authentication experiences are separate concerns that share the account and organization foundations.

### 6.2 Authorization

Authorization is server-authoritative and exists at multiple levels:

1. authenticated account;
2. organization membership;
3. venue/screen ownership;
4. role permission;
5. capability decision;
6. subscription entitlement, add-on, allowance, or rollout;
7. provider-owned data authority.

The UI may predict and explain a refusal, but the API must enforce it on every relevant request. A hidden or disabled control is not a substitute for server enforcement.

### 6.3 Capability decisions

The capability model is the common language for feature access. The current model defines canonical action capabilities across product domains such as screen, publishing, content, account, schedule, organization, branding, analytics, workflow, support, and localization.

Every session resolves a capability into one of four decisions:

- `allowed` — the actor may perform the action;
- `denied` — the actor lacks permission;
- `unavailable` — the capability is not included or enabled for the plan;
- `temporarily-blocked` — the capability exists but is withheld by rollout, timing, or operational policy.

These are different product states and must not collapse into one generic “no.”

The decision payload is structured and includes the capability, decision, reason code, localized message, parameters, correlation identifier, resolution hint, retry information, conditions, and `isAllowed`. The same shape is returned by session resolution and by request refusals.

`RequireCapability` is the standard API enforcement primitive for protected Back Office actions. New protected routes must inspect existing capability definitions and use the established decision contract rather than inventing an endpoint-specific refusal format.

### 6.4 Roles

Roles are genuinely different authorities, not only labels. The current foundation includes:

- **Organization Owner** — full organizational authority;
- **Content Editor** — edits content but does not publish or manage screens;
- **Publisher** — publishes and recovers content but does not edit content.

The role set may expand, but any new role must define:

- allowed and refused actions;
- visible navigation;
- direct API behavior;
- behavior after the role changes;
- tests for permission denial, repeated requests, and stale sessions.

The UI should reveal predictable role boundaries before a request is sent, while the API remains the final enforcement point.

### 6.5 Entitlements and allowances

Entitlements are server-resolved, time-bounded, revocable, and associated with the organization. Add-ons, allowances, usage, rollout windows, and organization/venue scope all participate in the decision.

The screen pairing allowance blocks only adding or pairing another screen. It does not remove actions from existing screens.

Usage semantics must be explicit. Some allowance usage is computed from authoritative state—for example, active non-archived screens—while other counters are stored. A new allowance must document which model it uses and how reconciliation works.

Localization changes the message, not the reason code or decision semantics. The fallback chain resolves the best available locale while keeping stable machine-readable reason codes.

---

## 7. Domain and data ownership

### 7.1 Core domain objects

The principal concepts are:

- organization and membership;
- venue and venue-local timezone;
- menu, page/section, and menu item;
- theme and display layout;
- screen and screen assignment;
- player and player output;
- pairing/claim and replacement;
- authoritative content revision and applied content revision;
- meal period, playlist, promotion, and emergency broadcast;
- connector, provider connection, mapping, snapshot, delta, and last valid state;
- capability, entitlement, add-on, allowance, rollout, and usage;
- release, component version, migration version, and environment;
- audit/event records where the feature requires durable history.

Each concept should have one owning boundary. Avoid creating duplicate “shadow” models in UI, API, and integration code that disagree on meaning.

### 7.2 Provider authority

When an integration controls a content domain, the provider is authoritative for that domain. VennuSign may transform and present the data, but it must not offer ordinary manual edits that pretend to override the provider.

Provider-controlled data is read-only from the customer surface unless the integration contract explicitly supports writes. The system must preserve enough source identity and synchronization evidence to explain where the displayed value came from.

### 7.3 Optimistic client behavior

The client paints a locally known result immediately when it can compute that result safely. The write is serialized and sent behind the frame.

The server remains the last word on:

- server-assigned identifiers;
- recomputed counts and capacities;
- refusals;
- entitlement and permission;
- availability;
- money;
- what is published;
- facts about the world rather than the client's intent.

On a refusal or conflict, the client reconciles from the authoritative response or a fresh read. Do not freeze the whole surface while waiting for a round trip, and do not re-download state the client just authored when the response already contains the authoritative result.

---

## 8. Content, menus, themes, and publishing

### 8.0 Content Platform architecture renewal

**Confirmed direction, owner-approved 2026-08-28.** VennueSign is a controlled content-and-presentation platform. A Menu is the first content type, not a permanent menu-only architecture.

The durable model is:

```
Content type -> immutable data-model version -> content instance/revision
             -> compatible theme revision -> immutable content release -> screen/player
```

- **Content Home** is the common catalog and lifecycle surface. Industry makes types relevant; entitlement and permission decide availability; neither changes a model's technical meaning.
- **Content Builder** is the shared capability behind focused editors. A Menu still speaks of Items; a cinema board may speak of Showtimes.
- A versioned **data model** defines nested structure, validation, provider authority, editor behavior, state fields, and the paths Themes may bind to.
- The **record library** is a typed canonical layer for reusable and imported facts. A collection may be inline-owned, manually composed from library records, or provider-query driven.
- **Operational state** is first class. A state such as sold out is layered over an approved content release; the Theme revision defines its visual response. It is neither an ordinary layout field nor a separate layout variant.
- **Theme revisions** bind to explicit model versions and field paths. A content release pins the exact content revision, model version, theme revision, renderer contract, and target assignments.
- Model and Theme versions are immutable once released. A change creates a successor version and deliberate migration; it never silently changes live output.
- The product remains a **modular monolith** for now: internally owned API modules, one deployed API host/App Service/container. A physical split requires evidence.

The approved program, target persistence shape, source-cleanup rules, workstream disposition, and phased implementation sequence are maintained in `docs/architecture/content-platform-architecture-renewal.md` (#939). That program is planning authority; implementation still requires a bounded feature milestone with schema, API, UI, tests, and owner acceptance.

### 8.1 Menus

The menu model is conceptually:

```
Menu -> pages/sections -> items
```

A menu item may include name, description, price, availability, dietary information, translations, and other approved fields. The exact current feature contract wins for field names and UI structure.

Menu editing must protect against:

- slow saves overwriting newer edits;
- duplicate or repeated submissions;
- incomplete and invalid values;
- long labels and overflow;
- role or entitlement changes during editing;
- browser refresh or leaving and returning;
- a provider becoming authoritative after local data already exists.

Per-item draft revision tracking is the established pattern for preventing a slow save from overwriting newer edits.

### 8.2 Themes and display layouts

Themes and layouts define how operational content is rendered, but they do not own the content itself.

The Theme Studio design separates:

- theme configuration and reusable visual structure;
- data fields and repeaters;
- layout placement;
- guided authoring;
- draft/revision state;
- final menu publishing.

Guided authoring is a product experience, not a server-authority exception. A repeater placed into a column and fields placed into that repeater must map to a valid renderable contract. Free-form editing and guided editing must converge on the same valid output model.

### 8.3 Publishing

Publishing is distinct from editing.

The server must track what is authoritative and what is published. A person may have permission to edit a draft without permission to publish it. Publishing must account for:

- role and capability;
- plan and allowance;
- validation;
- screen targets;
- provider ownership;
- current revision;
- concurrent changes;
- delivery status after publication.

A publish action must not claim that a display has applied content merely because the publish request succeeded.

---

## 9. Screens, players, outputs, and display delivery

### 9.1 Logical screen versus physical player

A **screen** is the logical destination for content and operational history.

A **player** is the physical or runtime device that connects to VennuSign.

A **player output** is one addressable output slot on a player. A single Windows or Linux box may drive multiple monitors. The output slot is not the same thing as a wall position:

- output identity answers “which connector/display is this?”;
- wall position answers “where is it installed in the venue?”

Keep those values separate.

The current and proposed models support:

```
Player (one registration/claim)
  -> PlayerOutput 1 -> Screen
  -> PlayerOutput 2 -> Screen
  -> ...
```

### 9.2 Pairing, claiming, and replacement

The established screen workflow supports expiring, single-use pairing codes, pre-registration, unpairing, reset, archive/restore, and physical player replacement.

A replacement workflow should preserve the logical screen when that is the declared behavior. It must not accidentally create a second screen, lose content history, or orphan the previous device without an auditable state transition.

Any future box-level claim workflow must make the claim boundary explicit: the box is claimed when the server has accepted the claim, established ownership, and can associate its output inventory with the correct organization/venue context.

### 9.3 Delivery truth

Display delivery is modeled as evidence, not as a single boolean.

At minimum, distinguish:

- authoritative revision;
- requested;
- received;
- applied;
- recovered;
- superseded;
- offline;
- failed.

“Applied” may be claimed only when the applied revision equals the authoritative revision.

Heartbeats provide operational liveness and stale detection. A preview or observer view must not falsely heartbeat a real screen Online.

When a screen reconnects, it catches up from authoritative state. Re-pushing content is unnecessary when the revision reconciliation contract already guarantees convergence.

### 9.4 Display runtime

The hosted display player is the rendering authority on the device side. Platform shells launch or host it. The display runtime should:

- maintain the current valid presentation;
- consume server-approved content and theme contracts;
- use cached content when delivery is delayed;
- report health and applied revision;
- distinguish attention states from emergency states;
- avoid presenting a blank or corrupt surface merely because a request failed.

The current architecture does not require a native player rewrite. Native shells and browser-hosted display behavior remain separate deployment components.

### 9.5 Proposed multi-output box architecture

The Windows/Linux multi-output box design is **proposed**, not active implementation authority.

Its intended shape is:

- one supervisor process per box;
- one isolated display runtime per output;
- supervisor-owned cloud connection, cache control, local IPC, health monitoring, update coordination, and runtime lifecycle;
- each output runtime receives its operational connection from the supervisor;
- per-output version isolation when safe cutover requires it;
- warm/drain handover so a replacement runtime takes over only after it is ready;
- composite output identity using connector/port identity with EDID corroboration;
- future support for physical commissioning and camera-assisted screen mapping as separate features.

The proposal deliberately leaves some choices open: exact IPC schema, thresholds, packaging, compositor behavior, artifact storage, and Linux implementation detail. Do not treat the proposal as a reason to change the current player model until the owner approves an implementation milestone.

---

## 10. Scheduling and live control

Scheduling is resolved by the server in the venue's timezone. The browser is never the final scheduling authority.

The scheduling model includes:

- meal periods;
- happy hour;
- playlists and player rotation;
- date-range promotions;
- emergency broadcasts.

Emergency broadcast is an explicit override with higher priority than ordinary scheduled content. Scheduled activation must be deterministic around timezone and daylight-saving transitions.

A live-control action must identify:

- the target screen or scope;
- the authoritative content/revision;
- the reason and actor;
- the expected precedence;
- the expiry or return behavior;
- delivery and recovery evidence.

Do not solve scheduling by sprinkling local browser time comparisons across clients.

---

## 11. Integrations and connector architecture

VennuSign integrations support both:

- **Pull** — VennuSign retrieves external data on a schedule or on demand.
- **Push** — an external system sends data through REST, webhook, or SFTP.

The connector architecture uses one shared semantic pipeline:

```
Transport -> raw immutable input -> identity/schema validation
          -> mapping -> canonical VennuSign type
          -> scoped snapshot/delta -> last valid state
          -> publishable operational data
```

The transport is not the business meaning. REST pull, REST push, and SFTP may differ in acquisition but converge into canonical, versioned VennuSign types.

Required integration properties:

- asynchronous event-driven processing where appropriate;
- at-least-once delivery with idempotent effects;
- immutable raw input for diagnosis;
- scoped snapshots and deltas;
- constrained declarative mapping rather than arbitrary executable customer code;
- fail closed on unknown identity or invalid schema;
- fail safe on display by retaining the last valid state;
- alert and retry when a synchronization cycle fails;
- no payment-card data;
- provider-owned data remains read-only where specified;
- integration health and provenance are observable.

Current provider foundations include Clover, Square, and Toast behind the content synchronization capability. The established webhook worker pattern logs and retries a failed cycle without terminating the host. Database event claiming must use explicit transaction/isolation behavior and must not leak connection-level isolation changes through pooled connections.

Target scale and reliability requirements recorded by the connector design are up to 1,000 locations, urgent updates within seconds, 99.9% monthly availability, and recovery within two hours. These are architecture targets; implementation proof must be recorded separately.

---

## 12. API, realtime, and background execution

### 12.1 HTTP API

The API owns transport and contract concerns:

- request validation;
- authentication and authorization composition;
- capability enforcement;
- organization/venue scoping;
- mapping owned DTOs to domain use cases;
- stable refusal and error contracts;
- non-secret version/health information.

Before adding a route, inspect existing contracts, actions, events, and refusal shapes. A new endpoint must not duplicate a current behavior under a different name.

### 12.2 SignalR and realtime notifications

SignalR is a notification and coordination path, not a replacement for authoritative reads.

Realtime messages should tell a client that something changed, needs reconciliation, or requires attention. The client then applies the established state contract. Reconnection must be safe, and missed messages must not permanently lose the ability to converge.

### 12.3 Hosted workers

Hosted services handle asynchronous work such as:

- provider webhook processing;
- retries and reconciliation;
- delivery notifications;
- scheduled activation;
- operational telemetry;
- future update orchestration.

Workers must survive an individual bad message or failed provider cycle. Work must be idempotent, observable, bounded, and safe to retry.

---

## 13. Persistence, migrations, and compatibility

Azure SQL is the authoritative production data store. The exact repository data-access implementation remains behind `Vennu.Data` and the reusable `Vennu.DataAccess` boundary.

Database changes use ordered DbUp migrations.

Rules:

- `src/Vennu.Data/Scripts/001_baseline.sql` is the collapsed history of the first fifty-nine migrations and is never edited.
- New migrations start at the next ordered version.
- Removing a migration file does not un-apply it.
- Existing released databases are changed by a new migration.
- Additive and expand-and-contract changes are required while older application releases remain supported.
- Incompatible stored-procedure behavior receives a new callable contract version.
- A migration that discards data must name exactly what it discards.
- Existing and fresh databases must converge to the same intended schema.
- Test a migration against both new and existing-shaped databases where compatibility matters.
- Never let SQL/RepoDB types leak into domain, contracts, or UI boundaries.

Two values that must describe the same instant must be read once under one lock or one authoritative query. Separate reads create race windows and inconsistent decisions.

---

## 14. Deployment and release architecture

Every independently deployable component has its own semantic version. The product version identifies one approved combination of component versions.

The release manifest is the canonical product/component version source. A release pipeline:

1. builds a component from a known source commit;
2. tests and validates it;
3. records the exact artifact and build identity;
4. composes the release manifest;
5. promotes the same immutable artifact;
6. never rebuilds a staging-approved component for production.

A carried-forward component preserves both its version and artifact identity. TV shells have separate increasing platform build numbers and remain separate from the hosted player. Shells and the hosted player declare the native-bridge range they support.

Runtime exposes non-secret version facts such as product version, component version, API contract major, source commit, build ID, database schema version, and configuration schema version through the established version health endpoint.

Database compatibility is part of release compatibility. Product rollout, customer schedules, migration waves, rollback orchestration, and component retirement are operational workflows built on this foundation; they must not silently redefine the manifest contract.

---

## 15. Environments and operations

The project distinguishes local, development, staging, and production concerns. Names and deployment topology may evolve, but an engineer must always identify the target environment before running a migration, seed, test, or deployment action.

Operational principles:

- customer data and production credentials never enter tests, documentation, or repository files;
- secrets are supplied through supported environment/configuration providers and managed secret storage;
- the test API is not deployed to production;
- production deployment promotes tested artifacts;
- operational actions are auditable;
- stale or failed state is visible rather than silently normalized;
- maintenance windows and cutovers are explicit;
- a failed new release does not erase the last known good component or customer presentation.

The platform operations surface is the internal control plane for future release inventory, rollout health, customer versions, organization support, venue registration, schedules, and cutover status. Its UI must not be treated as proof that the underlying deployment actually succeeded; it must display backend-derived state and evidence.

---

## 16. Observability and recovery

Observability is part of the architecture, not an afterthought.

At the request and workflow level, preserve:

- correlation identifiers;
- actor and tenant scope;
- source and target revisions;
- provider/source identity;
- attempt and retry count;
- current status and last transition;
- error category;
- timestamps;
- component and source versions.

Recovery rules:

- retain last valid display content during transient failures;
- retry boundedly and make repeated processing idempotent;
- distinguish attention from emergency;
- do not call a state “applied” without matching authoritative evidence;
- preserve evidence when a worker or runtime restarts;
- surface stale data and blocked capability states honestly;
- prefer a safe degraded presentation over an unexplained blank display.

Future observability and performance telemetry remains a proposed design until promoted into the relevant implementation feature.

---

## 17. Security and secrets

Security boundaries are enforced in code and deployment configuration:

- no secrets, tokens, connection strings, or generated credentials in Git;
- no provider credentials in public contracts or logs;
- least privilege for service identities;
- tenant scope checked server-side;
- external identity merging follows the defined provider boundary;
- production access is not granted to local model workers or test tooling;
- test fixtures never contain live customer data;
- logs are secret-scanned before durable publication;
- support and platform actions are auditable;
- invalid identity, schema, signature, or ownership fails closed.

Local development may use supported environment providers and local certificates. This does not authorize committing local secrets or weakening production validation.

---

## 18. Frontend architecture and UX engineering rules

Back Office and Platform Operations are applications over server contracts, not alternate business-rule engines.

A complete UI change considers:

- loading, empty, disabled, error, and retry states;
- new, saved, invalid, minimum, maximum, duplicate, and long values;
- refresh, leave-and-return, cancel, close, repeated submission, and concurrent actors;
- each role, tier, capability decision, and denial;
- small and large supported widths;
- long labels, overflow, keyboard focus, localization, and mobile behavior;
- every path into the state and every path out of it.

The UI should:

- make role identity and current organization/venue context visible;
- explain why an action is unavailable, denied, or temporarily blocked;
- avoid controls that look identical while representing different “no” states;
- preserve entered work where safe;
- paint client-computable results immediately;
- reconcile to server truth on refusal or conflict;
- use established design tokens and shared components;
- keep destructive actions explicit and recoverable.

The display UI has a different job: it prioritizes legibility, continuity, safe degraded behavior, and correct application of published state over administrative density.

---

## 19. Testing and evidence

A feature is complete when its paths are covered, not when its happy path works.

Every behavioral change should include the smallest meaningful tests at the layer where the rule is enforced:

- pure functions and mapping at unit level;
- SQL/database rules against a real database;
- API authorization and refusal contracts at API level;
- browser-visible workflows through Playwright;
- owner acceptance for what the customer sees;
- provider and hosted infrastructure tests only when the environment and credentials are intentionally available.

Standing repository rules include:

- LocalDB is the default local test target.
- Azure SQL tests require an explicit target selection and managed credentials.
- A suite that cannot reach its database must fail; it must not pass while asserting nothing.
- A test must not delete from a database it did not create.
- Model invariants run after integration tests in areas that use them.
- Regression tests are verified against the defect with the fix reverted when practical.
- Integration, Azure SQL, hosted infrastructure, credentialed, physical-device, and cross-system tests are skipped only under the owner's current exception and must be reported as skipped or `UNTESTED`.
- CI is currently suspended by owner decision; local verification is the gate until that policy changes.
- Documentation-only changes use lightweight repository validation.

Acceptance validates customer-visible behavior. It must not be replaced by a green API call or a workbook that never observes the rendered result.

---

## 20. Engineering workflow and agent boundaries

The repository uses feature-based milestones. Each milestone is a small vertical slice that ships schema, API, UI, and Playwright/specification work together when applicable.

The normal sequence is:

```
approved design -> bounded milestone -> schema -> API -> UI -> specifications
                 -> local verification -> independent review -> owner acceptance
                 -> merge -> synchronized records
```

The owner remains the approval, acceptance, merge, and deployment authority.

Engineering agents must:

- read the startup records required by `AGENTS.md`;
- inspect the tracker and claims before changing shared files;
- state the complete behavior before implementation;
- search for every location where the behavior exists;
- preserve ownership and tenant boundaries;
- keep changes bounded;
- record evidence that another person can rerun;
- identify unvalidated paths instead of implying coverage;
- update living records when a change makes them false.

### Proposed Maestro control loop

The Maestro dev-lead agent framework is **proposed**.

Its intended boundary is:

- cloud-controlled interpretation, decomposition, risk classification, review, and escalation;
- local models limited to bounded repository-contained implementation;
- one isolated worktree and one branch per job;
- no local commit to `master`, merge, deploy, architecture change, credential access, or owner-facing product decision;
- GitHub Issues and PRs as the initial job interface;
- one local inference job at a time until capacity evidence supports concurrency;
- at most one cloud-requested local revision before escalation;
- durable, secret-scanned execution evidence;
- cloud-only handling for ambiguity, identity, security, database strategy, production access, and cross-application design judgment.

This proposal is separate from the product runtime and must not change the product architecture until approved.

---

## 21. Current architecture gaps and open decisions

The following items require explicit promotion or decision before they become implementation commitments:

1. Exact production deployment topology and component split as scale grows.
2. Final Windows/Linux multi-output player packaging and low-level display implementation.
3. Supervisor/display-runtime IPC schemas, health thresholds, and handover tests.
4. Linux implementation and packaging for the proposed box player.
5. Exact artifact storage/CDN choice for large display assets and update packages.
6. Controlled pre-staging of large runtime updates.
7. Coordinated Screen moves between outputs.
8. Full Back Office visual system and detailed role/permission matrix.
9. Completion of the Release–Capability–Tier Matrix.
10. Final name and operating scope for the proposed Maestro system.
11. Maximum review rounds and owner escalation policy for automated work.
12. Whether the verification path moves from Windows-hosted execution to disposable SQL Server Linux containers.
13. Location and ownership of model-routing configuration.
14. Resource policy between local-model inference and verification gates on the Linux AI workstation.
15. Any feature or design currently under `docs/design/proposed/` that has not yet been promoted to an active feature.

Open items are not failures. They are boundaries that keep the current system honest while preserving a path to scale.

---

## 22. Source map

Use this document first to orient. Then read only the source record relevant to the task.

### Governing engineering policy

- `AGENTS.md`
- `AI_DEVELOPMENT_GUIDE.md`
- `docs/README.md`

### Cross-system architecture

- `docs/architecture/built-foundations-spec.md`
- `docs/architecture/content-platform-architecture-renewal.md`
- `docs/architecture/capability-entitlement-authority.md`
- `docs/architecture/administrative-identity.md`
- `docs/architecture/phase-13-identity-tenancy-foundation.md`
- `docs/architecture/phase-13-customer-authentication.md`
- `docs/architecture/phase-13-customer-onboarding.md`
- `docs/architecture/phase-12-pos-foundation.md`
- `docs/architecture/configuration-platform.md`
- `docs/architecture/player-delivery-reliability.md`
- `docs/architecture/scheduling-live-control.md`
- `docs/architecture/tooling-secrets.md`

### Feature authority

Read the active feature's records under `docs/features/<feature>/`, especially:

- `decisions.md`
- `README.md`
- milestone plans
- question registers
- acceptance records and workbooks

Feature decisions win for that feature when they are more specific than this bible.

### Proposed design

- `docs/design/proposed/box-player/architecture.md`
- `docs/design/proposed/box-player/interaction-flows.md`
- `docs/design/proposed/box-player/milestone-plan.md`
- `docs/design/proposed/maestro-dev-lead-agent-framework.md`
- `docs/design/proposed/camera-based-physical-commissioning.md`
- `docs/design/proposed/observability-and-performance-telemetry.md`

### Operations and release

- `docs/operations/DEPLOYMENT_VERSIONING.md`
- `docs/operations/release/release-manifest.template.json`
- `docs/process/RELEASE_POLICY.md`
- `docs/process/SHARED_FILE_WRITE_PROTOCOL.md`

### History and research

- `docs/archive/`
- `ai/handoffs/archive/`
- `track0/`

Historical records explain how the system arrived here. They do not override current code or current authority.

---

## 23. Maintenance rule

Update this bible when a durable cross-system decision changes, when a new component boundary is introduced, or when a source document would otherwise make this document materially misleading.

Do not use it as a work log. Do not copy feature-level acceptance steps into it. Do not duplicate detailed schemas, migration text, or UI specifications that belong in their owning records.

A change that makes this bible false must update it in the same change set as the architectural change.
