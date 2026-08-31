# VennueSign Mosaic V1 Independent Blueprint Study

**Study date:** 2026-08-30<br>
**Repository evidence:** Vennusign `master` at `40a03e677a6aa8316614e77922b57c95b56822ad`<br>
**Purpose:** Test the proposed Mosaic V1 direction against the complete API vNext blueprint, independently of the earlier recommendation.<br>
**Decision status:** Study recommendation only. This report does not authorize implementation, endpoint shapes, schemas, migrations, deployable-service splits, or work packages.

## Direct answer

**Yes—the content-to-presentation-to-display path is the right principal roadmap direction for Mosaic V1.** It should be retained because it is the smallest useful customer journey that crosses VennueSign's most important renewed boundaries: business meaning, presentation, controlled publication, logical assignment, physical delivery, recovery, and proof of what is actually showing.

It was unquestionably influenced by the concentration of recent Menu and Theme work, but it does not survive this study merely because that work already exists. It survives because the credible alternatives either rebuild mature prerequisites without proving the signage product, depend on contracts that the content path must establish first, or produce infrastructure without a customer outcome.

The original path needs one important sequencing correction:

> **Start with the observable first-live-screen acceptance journey and settle the minimum irreversible boundary contracts. Then build an executable walking skeleton while the real `menu.v1`, Theme, Release, Package, Player Output, and evidence implementations proceed in parallel.**

This means **Data Model is an immediate semantic contract gate, but not necessarily the first production code**. A walking skeleton may use frozen neutral fixtures or an explicitly disposable legacy adapter to expose cross-surface risks early. No production Package contract may be approved until the minimum `menu.v1` and Theme-binding semantics are frozen. If the team cannot keep that adapter disposable and prevent a second source of truth, the safer fallback is to complete the minimum `menu.v1` compiler before Runtime production implementation.

Authentication is not the V1 spine. Existing human identity, session, organization, venue, role, and entitlement foundations should be reused and tested only along the selected Mosaic path. Any security or tenancy blocker discovered there must be repaired before the affected capability proceeds.

## The observable Mosaic V1 outcome

An authorized operator at a venue can create or use one Menu presentation, publish an immutable Release, assign it to one logical wall-of-one Screen, pair the correct authenticated Player Output, and see evidence that the output received, verified, applied, and is currently showing the correct Package. The operator can apply and clear an immediate 86 without publishing. The display retains its last-known-valid Package through a connection failure and reconciles after recovery. A customer or support view clearly distinguishes Core desired state from Runtime actual state.

The first acceptance journey may use one organization, one venue, one wall, one screen, one output, one content type, and a fixed or basic Theme. The underlying contracts must still enforce organization and venue scope and prove refusal of cross-tenant and cross-venue access. A single-venue demonstration must not create a single-venue architecture.

Mosaic V1 is complete only when this is supportable and recoverable—not merely visible in a demo.

## Independent method

Four independent lenses examined the same current blueprint:

1. a dependency and architecture lens;
2. an adversarial roadmap challenge;
3. a parallel-workstream and integration lens;
4. a bounded current-foundation evidence inspection.

The synthesis used evidence rather than majority vote. It mechanically accounted for:

- all **4 API surfaces**;
- all **16 API families**;
- all **82 named blueprint areas**;
- all **156 mapped candidate route groups**;
- all **51 route-level interaction annotations**; and
- all **8 unresolved route-level review annotations**.

The route inventory divides into 103 Core, 14 Connect, 12 Runtime, and 27 Platform primary route groups. Those counts describe the current candidate API inventory—not architectural importance or implementation order. For example, Connect Mapping and Control has **zero** primary routes, yet its Venue Mapping, Data Mappings, Ownership Rules, and Validation Rules are essential whenever imported data enters Core. Runtime has only 12 primary groups, yet a signage product cannot safely claim success without delivery, recovery, and showing evidence.

This was not a whole-product code audit. The foundation lens inspected only sources and tests relevant to the proposed dependency path. It found substantial reusable nuclei, but the API vNext proposal itself still does not approve endpoints or schemas.

### Evidence references

| Evidence | Exact revision |
|---|---|
| Vennusign `master` | `40a03e677a6aa8316614e77922b57c95b56822ad` |
| Architecture Renewal document blob | `eedc732ccdc90bc72d6d7bad4ff4800ee4e9e27e` |
| API endpoint inventory blob | `09430888193c87f00fc79eb7cfeeb9bd566569e3` |
| `PROJECT_STATUS.md` blob | `34b2c3a3399af82f79cb17d9d01249cefd056651` |
| Current handoff blob | `5bf3fc857bcefe56f36940fd62e37f644ab6d405` |
| Assignment tracker blob | `cf7c59728f54ea7a1b2157d367a114d3d51f52fd` |

The bounded executable checks reported by the foundation lens were 253/253 Back Office deterministic Node tests, 98/98 Platform Operations deterministic Node tests, and 190/190 applicable Display deterministic tests. Four workflow-file inspections were excluded because the sparse study checkout lacked the referenced workflow file. .NET was unavailable in that study runtime, so current API tests were inspected but not executed. Live identity providers, Stripe, POS providers, Azure SQL, physical devices, and cross-system behavior remain unproven.

The same bounded evidence lens classified every mapped route group by the current implementation nucleus:

| Surface | Routes | Proven/reusable nucleus | Reshape | Absent | Uncertain |
|---|---:|---:|---:|---:|---:|
| Core | 103 | 52 | 36 | 10 | 5 |
| Connect | 14 | 10 | 0 | 0 | 4 |
| Runtime | 12 | 3 | 5 | 3 | 1 |
| Platform | 27 | 23 | 1 | 3 | 0 |
| **Total** | **156** | **88** | **42** | **16** | **10** |

This is evidence of an uneven starting point, not an approval of routes or an estimate of effort. The apparent maturity is concentrated in identity, Menu behavior, Screens/scheduling, existing provider foundations, Platform administration, and display heartbeat/cache/receipts. The largest renewed gaps sit at the center: generic Data Models, Presentations, complete Core Releases, output-specific Runtime Packages, Player Outputs, organization sharing, and generalized Connect authority/provenance.

The study explicitly rejects these dangerous equivalences:

- a current Menu publish event is not yet the complete Core Release;
- a Content revision is not a Runtime Package;
- a Screen is not a Player Output;
- the current venue theme is not the future immutable Theme revision;
- current POS catalog mappings are not generalized field provenance and authority;
- existing POS import behavior is not proof of the complete Connect pipeline;
- a Platform API key is not attributable workforce governance;
- anonymous diagnostics are not automatically a safe centralized Runtime contract;
- existing tenancy does not prove organization-shared inheritance and venue override policy; and
- passing deterministic tests does not prove live integrations or physical hardware.

## Agreement, disagreement, confidence, and limitations

### What all four lenses agreed on

- The content-to-display path is the right Mosaic integration spine.
- Existing customer sign-in and venue foundations should be reused, not rebuilt first.
- The path is incomplete without tenant enforcement, Release-versus-Package separation, Player Output identity, showing evidence, rollback, last-known-valid recovery, and minimum operational visibility.
- The four API surfaces are logical modular-monolith boundaries, not four deployment units.
- Menu is a valuable first proof and migration asset, but its current persistence shape must not become the generic architecture.
- Core, Runtime, and Theme work can proceed in parallel against frozen contracts and fixtures.
- Shared identifiers, migrations, public contracts, invariants, and cutover must be serialized.
- Live providers, full Data Model Studio, full Theme Studio, broad Platform renewal, and complete onboarding are not automatically V1 requirements.

### The important disagreement

One view placed the `menu.v1` Data Model implementation first because downstream Theme bindings, Connect mappings, Release composition, and renderer meaning depend upon it. The adversarial view argued that this risks spending too long inside Core before discovering the hardest cross-surface problems: logical Screen versus Player Output, Release versus Package, requested versus showing, package compatibility, and offline recovery.

The adversarial objection is better supported **for implementation order**, while the Data Model-first claim remains correct **for semantic authority**. An acceptance journey and contract gates expose the intended product outcome first. A fixture-backed walking skeleton then attacks the risky seams early. In parallel, the model lane builds the real semantic foundation. The safeguards are non-negotiable:

- use neutral, versioned fixtures or an explicitly disposable adapter;
- freeze minimum `menu.v1` identities, state, placement, and binding paths before approving production Package schemas;
- carry model, Theme, renderer, Release, and output compatibility identities from the start;
- name the adapter's deletion condition;
- do not create two sources of truth; and
- replace the fixture/legacy adapter before Mosaic is accepted.

If these safeguards cannot be enforced, the strongest objection wins completely and the minimum model/compiler must finish before Runtime production implementation.

### Confidence

**Overall confidence: 0.83 (high, conditional).** The product-direction conclusion is strong. Exact work-package ordering remains conditional on the required reconciliation session, the unresolved owner decisions, a bounded exact-head source inspection, and physical/live-system proof where the release intends to make those claims.

### Limitations

- No complete source audit was performed or recommended.
- The 156 routes are candidates, not approved contracts.
- Some built-foundation statements predate newer Menu work.
- Current tracker and project-status records disagree about whether M6.11 files remain actively claimed; this must be reconciled before assigning those files.
- CI is suspended, .NET tests were not executable in the study runtime, and external systems/devices were not tested.
- This report cannot decide product-market scope, release audience, or hardware commitments that the owner has not specified.

## Selection-bias test

### Evidence that bias exists

- Menus has the deepest accepted behavior and feature records.
- Theme Studio has substantial design attention.
- The renewal was discovered through Menu-shaped architectural limits.
- Core owns 103 of the 156 candidate route groups; Content and Design alone owns 35.
- Existing tests and acceptance work are concentrated in Menu, Back Office, Screens, pairing, and display behavior.

This concentration can bias vocabulary, storage, and packages toward Menu-specific tables and browser-player assumptions.

### Why the recommendation survives the bias test

Menu is also the only existing domain rich enough to prove nested content, reusable records, placement-specific values, draft versus immediate operational state, imports, publish history, rollback, assignment, rendering, offline continuity, and actual-state evidence in one journey. It lowers delivery risk without removing architectural pressure.

The correct bias control is therefore not to choose a less coherent V1. It is to:

- treat Menu as the first adapter, not the universal schema;
- test model fixtures against a contrasting future type such as cinema showtimes;
- define Connect provenance even if no live provider ships in V1;
- define Player Output even if acceptance begins with one output;
- preserve organization/venue boundaries even if the first journey uses one venue; and
- prohibit current Menu payloads from silently becoming the renewed Package contract.

## Credible alternative V1 directions

| Alternative | Strongest argument | Study decision |
|---|---|---|
| Identity, tenancy, and onboarding first | Every action needs an actor, tenant, venue, permission, and entitlement; mistakes are security risks. | **Reject as the spine.** Reuse and verify the existing foundation on the selected path. Repair blockers. Defer full onboarding and authentication expansion unless Mosaic must acquire new customers. |
| First live screen via a fixture-backed walking skeleton | Exposes Release/Package, Screen/Output, showing evidence, compatibility, and recovery risks earlier than a long Core build. | **Retain as the starting implementation pattern**, guarded by frozen semantic contracts and a disposable adapter. |
| Connect/showtimes first | Best protection against a Menu-only architecture; forces provenance, provider identity, query source modes, and last-valid behavior. | **Reject for V1, retain as strongest V1.x/V2 proof.** Too many simultaneous external and semantic unknowns. Use showtime fixtures now. |
| Runtime and Platform operations first | A signage product that cannot deliver, prove, and recover what is showing is not operable. | **Retain as a major parallel lane, reject as the whole outcome.** It lacks customer authoring/publishing value by itself. |
| Horizontal module/API renewal first | Clean seams may improve future parallelism and reduce later refactoring. | **Reject.** This is architecture-renewal convenience, not a coherent product release. Add only the seams the Mosaic path crosses. |
| Full Theme Studio or Data Model Studio first | The authoring tools could define the future platform directly. | **Reject.** Build the engine, compatibility rules, and one real proof before the complete visual studios. |

## Customer V1 needs versus renewal convenience

### Customer-release requirements

- sufficient existing customer identity, authorization, organization, and venue context;
- one useful Menu content and presentation path;
- controlled publish and immutable Release;
- rollback and immediate 86/restore;
- logical assignment and authenticated physical-output delivery;
- Package integrity, cache, failure continuity, and reconciliation;
- requested/received/verified/applied/showing evidence;
- enough support visibility to diagnose the journey; and
- cross-tenant and cross-venue refusal.

### Architecture work required only because V1 crosses it

- minimum `menu.v1`, stable identity, state, and binding contracts;
- explicit Core, Connect, Runtime, and Platform ownership at crossed seams;
- Release/Package and Screen/Player Output separation;
- contract fixtures and characterization of reused behavior;
- one source of truth, migration/cutover rules, and adapter deletion conditions;
- versioned contracts and generated OpenAPI where a public contract is introduced.

### Renewal convenience that must not drive V1

- completing every sign-in method or onboarding branch;
- four separately deployed APIs;
- broad source cleanup or module moves outside the selected path;
- full Data Model Studio or Theme Studio;
- every scheduling mode, promotion, playlist, wall, or multi-output feature;
- all providers, exports, SFTP, cinema feeds, or provider-fleet operations;
- full revenue, catalog, configuration, maintenance, incident, or compliance administration;
- a second content type before the first coherent journey works.

## Sixteen-family V1 disposition summary

The appendix provides all 82 area decisions. This table states the family-level intent.

| Surface / family | V1 direction | Why |
|---|---|---|
| Core — Account and Business Structure | **Reuse; bounded hardening** | Required context exists. Verify tenant and permission gates; avoid an identity rebuild. |
| Core — Content and Design | **Build/reshape; central V1** | Shared Data Model, Content, Presentation, Theme revision, and minimum Assets are the semantic center. |
| Core — Display Planning | **Reuse/reshape; minimum V1** | One logical screen/wall, output binding, and assignment are required; broad scheduling is not. |
| Core — Change Control | **Reshape/build; central V1** | Draft, publish, immutable Release, rollback, operational override, and audit make the journey trustworthy. |
| Connect — Connections and Sources | **Preserve/contract; mostly defer** | Keep useful adapters, but do not make a live provider a default V1 gate. |
| Connect — Mapping and Control | **Contract now; build only as used** | Zero routes does not reduce its importance; imported meaning and authority must be settled early. |
| Connect — Data Movement | **Reshape selected import path** | Preserve accepted paste/import workflow behind snapshots and Change Sets; defer export/broad sync. |
| Connect — Reliability and Visibility | **Minimum for selected path** | Honest errors, last-valid behavior, provenance, and bounded retries are required only where Connect runs. |
| Runtime — Player Identity and Topology | **Reuse/reshape/build; V1 gate** | Machine identity and Player Output must stop being conflated with logical Screen. |
| Runtime — Desired State and Delivery | **Build; central V1** | Output-specific Package and asset delivery are absent and mandatory. |
| Runtime — Convergence and Evidence | **Reshape; central V1** | Existing evidence is useful, but must become Package/showing truth. |
| Runtime — Health and Lifecycle | **Reuse/reshape; minimum V1** | Heartbeat and recovery matter; diagnostics security must be corrected; full updates may wait. |
| Platform — Customer and Commercial Operations | **Reuse; thin support only** | Customer support for the journey matters; revenue and broad commercial administration do not. |
| Platform — Entitlements and Configuration | **Reuse** | Mature foundations can support Mosaic; add only required renderer/package values. |
| Platform — Fleet and Delivery Operations | **Thin build/reshape** | Minimum package, render, and showing visibility is necessary to operate Mosaic. |
| Platform — Governance and Safety | **Reuse/repair where crossed** | Test automation helps; workforce identity is required before Platform writes, but read-only support reduces scope. |

## Contract gates

| Gate | Decision that must be frozen | Unblocks |
|---|---|---|
| **G0 — Reconciliation authority** | authoritative records, stale claims, owner outcome, included capabilities, release audience | all implementation |
| **G1 — Identity and tenancy** | actor, organization, venue, Player Output identity, authorization provenance, refusal rules | all writes and Runtime delivery |
| **G2 — `menu.v1` semantics** | fields, collections, stable IDs, source modes, placement overrides, state values, binding paths | Content, Theme, Connect, overrides |
| **G3 — Revision and migration** | immutable model/content/Theme versions, compatibility, adapters, backfill, one truth | schema and migration work |
| **G4 — Theme and renderer** | field/repeater/state bindings, geometry, screen-fit validation, renderer contract version | Presentation, publish, package generation |
| **G5 — Core Release** | immutable tuple, publish validation, idempotency, assignment capture, audit, rollback | assignments, Packages, support truth |
| **G6 — Player Output** | enrollment, machine authentication, output identity, Screen binding, geometry provenance | manifest authorization and evidence |
| **G7 — Runtime Package** | Release/output identity, artifacts/hashes, fallback, minimum runtime, activation | delivery and recovery |
| **G8 — Evidence** | requested/downloading/received/verified/applied/showing/stale/failed/recovered transitions | acceptance and dashboards |
| **G9 — Connect change** | snapshots, venue mapping, field authority, provenance, validation, Change Set, Core command | imported content implementation |
| **G10 — Composed read** | desired-versus-actual provenance and customer/workforce authority | support and customer status views |
| **G11 — Cutover** | coexistence limit, migration order, adapter retirement, rollback, no dual truth | production migration |

The eight route-level `REVIEW` decisions remain part of these gates: 86 versus authored Not available; Presentation desired versus Showing State; Screen configuration versus actual/device geometry; assignment versus showing; Runtime ownership of wall delivery; Player Output authorization rather than casual `{screenId}` delivery; secure/local diagnostics; and Stripe billing webhook ownership outside customer-data Connect.

## Parallel cloud-coordinator plan

| Lane | May start | Owns | Exact blockers |
|---|---|---|---|
| **A. Contract and migration authority** | After G0 | shared IDs, G1–G11, OpenAPI/public contracts, migration order, cross-lane invariants, cutover | owner decisions and reconciliation |
| **B. Existing-path characterization** | After G0; planning can begin now | bounded source maps and tests for auth, Menu, import, 86, publish, assignment, pairing, delivery, recovery, multi-venue refusal | stale tracker claims; shared harness changes remain orchestrator-owned |
| **C. Core Data Model, Content, Change Control** | fixtures after G0; implementation after G2/G3 | `menu.v1`, typed records, revisions, state overlays, Presentation, publish, Release, rollback, Core audit | G2, G3, G4, G5, migration owner |
| **D. Theme and renderer compatibility** | design against draft G2; implementation after G2/G4 | immutable Theme revisions, bindings, state responses, compatibility checker, fixture/preview lab, renderer contract | field paths, states, geometry, renderer version |
| **E. Runtime identity, delivery, evidence** | topology/evidence characterization after G0; implementation after relevant gates | enrollment, machine auth, Output discovery, Package/assets, state machine, health, recovery | G5–G8; Player Output key correction |
| **F. Connect provenance and import adaptation** | source characterization after G0; implementation after G2/G9 | snapshots, mapping, ownership, provenance, Change Sets, retained paste/import adaptation | G2, G9, Core controlled command; no direct Core-table writes |
| **G. Platform support, fleet read, tests** | UX/query design after G0; implementation after G8/G10 | read-only support, fleet/package/render visibility, desired/actual composition, needed test operations | telemetry vocabulary, G8, G10; workforce authority for any write |
| **H. Account/tenant sufficiency** | After G0 | prove current Authentication, Authorization, Sessions, Organizations, Venues; smallest security repairs only | selected acceptance path and identity audiences |

### Safe concurrency

- Bounded characterization and contract drafting can begin together after reconciliation.
- Core persistence can run with Runtime topology/evidence work after their respective contracts freeze.
- Theme compatibility can run against frozen `menu.v1` fixtures while Core storage is built.
- Connect snapshot/provenance work can run with Theme work; Core integration waits for G9 and the controlled Core command.
- Runtime evidence fixtures and Platform read-view design can proceed together against G8/G10.
- Account/tenant sufficiency can proceed independently of feature implementation.
- Lane-local tests may proceed together when they do not modify shared harnesses or fixtures.

### Unsafe concurrency and required serialization

- one owner for Data Model identifiers, field paths, source/state vocabulary, Release, Package, Player Output, and evidence meanings;
- one migration sequence and one cutover owner;
- no independent Core/Runtime manifest designs;
- no independent Data Model/Theme binding vocabularies;
- no independent Core/Connect authority rules;
- no independent interpretation of available, unavailable, sold-out, and 86;
- no independent Screen/Player pairing and binding models;
- no Platform definition of applied/showing separate from Runtime;
- no concurrent edits to `Program.cs`, project files, dependency injection, generated OpenAPI, shared configuration, invariant fixtures, tracker, status, handoff, or architecture authority;
- no legacy Menu and generic-content migrations against the same tables without one cutover owner; and
- no source moves mixed with uncharacterized behavior changes.

A free coordinator is not permission to start a blocked package.

## Recommended integration order

1. **Reconciliation record:** settle authority, stale claims, owner decisions, release boundary, and the eight route reviews.
2. **Acceptance and contract package:** define the observable journey; freeze G1–G10 with valid and invalid fixtures.
3. **Characterization package:** prove what existing identity, Menu, import, 86, Screen, pairing, delivery, rollback, and recovery behavior can be reused.
4. **Executable walking skeleton:** use frozen fixtures/disposable adapters to prove tenant → Release → assignment → authenticated output → Package → Showing → recovery → support evidence.
5. **Migration/cutover package:** backfill, adapters, coexistence limit, deletion conditions, rollback, and one source of truth.
6. **Parallel foundation merges:** Core model/content; Theme compatibility; Runtime identity/evidence; Connect source/provenance; Platform read/test support.
7. **Core composition:** Content + Theme → Presentation; publish → immutable Release/rollback; Release → assignment.
8. **Runtime integration:** Release/assignment → output Package; assets, overrides, recovery, and evidence.
9. **Connect integration:** source → mapping/authority → Change Set → controlled Core command, if the selected V1 import path requires it.
10. **Platform composition:** desired/actual support and fleet views, read-only unless privileged controls are separately approved.
11. **Mosaic acceptance:** cross-tenant refusal, Menu/manual or selected import, Theme compatibility, publish/rollback, authenticated output delivery, offline last-valid, showing evidence, immediate 86/restore, and support visibility.

## Unresolved owner decisions that could materially change V1

1. Is Mosaic V1 a private pilot, limited production release, or broad general availability?
2. Must Mosaic onboard a brand-new customer, or may the first release begin with existing/seeded organizations and venues?
3. Must V1 prove a physical player, and which operating system/hardware target is authoritative?
4. Is one Player Output sufficient for V1, or must multi-output topology and synchronized activation ship?
5. Is a fixed/basic preset Theme sufficient, or is customer Theme authoring part of the release promise?
6. Is existing paste/import required in V1? Is any live POS or cinema/showtime provider required?
7. Must multi-venue behavior be demonstrated end to end, or is enforced structural support plus refusal evidence sufficient?
8. What offline duration, retention, rollback, and recovery targets does V1 promise?
9. Which scheduling behavior is required beyond always-on assignment or an existing reusable schedule?
10. What minimum customer/support/fleet view is required to operate the release?
11. What exact Menu behaviors are preserved unchanged versus deliberately redesigned?
12. What are the exact `menu.v1`, Release, Package, Theme compatibility, and Player Output contracts?
13. What migration window and adapter-retirement rule are acceptable for existing Menu/Screen data?
14. Does any Platform mutation enter V1, thereby requiring production-grade workforce authorization and privileged-action audit?

These are targeted reconciliation questions, not reasons to perform a whole-product audit.

## Complete blueprint coverage ledger: 82 named areas

Legend: **Reuse** = retain and prove; **Reshape** = adapt existing behavior to renewed contracts; **Build** = new V1 capability; **Contract** = settle now, implement only if the V1 path uses it; **Defer** = not required for the stated V1 outcome.

| # | Surface / family | Named area | Mosaic V1 disposition |
|---:|---|---|---|
| 1 | Core / Account and Business Structure | Authentication | **Reuse** and characterize selected sign-in path; do not rebuild first. |
| 2 | Core / Account and Business Structure | Authorization | **Reuse/reshape** only to prove organization/venue permissions and refusal. |
| 3 | Core / Account and Business Structure | Sessions | **Reuse** actor, organization, venue, and capability envelope. |
| 4 | Core / Account and Business Structure | Onboarding | **Defer** renewed flow unless owner requires new-customer V1. |
| 5 | Core / Account and Business Structure | Organizations | **Reuse/reshape** shared/effective-state envelope only as V1 crosses it. |
| 6 | Core / Account and Business Structure | Venues | **Reuse** as mandatory operational and tenancy boundary. |
| 7 | Core / Account and Business Structure | Subscriptions and Billing | **Reuse** necessary entitlement; **defer** commercial expansion. |
| 8 | Core / Content and Design | Data Models | **Build/reshape** minimum immutable `menu.v1` contract/compiler. |
| 9 | Core / Content and Design | Content | **Reshape** Menu into typed records, revisions, composition, and state. |
| 10 | Core / Content and Design | Presentations | **Build/reshape** as Content-plus-Theme composition. |
| 11 | Core / Content and Design | Themes | **Build/reshape** minimum immutable revisions, bindings, and state responses. |
| 12 | Core / Content and Design | Assets | **Build/reshape** minimum immutable/hash-addressed assets used by Packages. |
| 13 | Core / Display Planning | Screens | **Reuse/reshape** logical lifecycle; keep separate from physical Output. |
| 14 | Core / Display Planning | Walls | **Reuse/reshape** wall-of-one; defer advanced planner. |
| 15 | Core / Display Planning | Player Administration | **Reshape** staff claim/bind/replace/unpair around Runtime Output identity. |
| 16 | Core / Display Planning | Assignments | **Reshape** to Presentation Release → Wall/Screen. |
| 17 | Core / Display Planning | Scheduling | **Reuse** only minimum current compatible activation; **defer** broad suite. |
| 18 | Core / Change Control | Drafts | **Reshape** around immutable content/Theme revisions. |
| 19 | Core / Change Control | Publishing | **Build/reshape** controlled validation and idempotency path. |
| 20 | Core / Change Control | Releases | **Build** immutable authority with prior Release and rollback. |
| 21 | Core / Change Control | Operational Overrides | **Reuse/reshape** immediate 86/restore separate from authored changes. |
| 22 | Core / Change Control | Audit | **Reuse/reshape** meaningful customer/system action evidence. |
| 23 | Connect / Connections and Sources | Connectors | **Preserve/reuse** current adapters; **defer** broad renewal. |
| 24 | Connect / Connections and Sources | Connections | **Contract** organization/venue scope; build only for selected live provider. |
| 25 | Connect / Connections and Sources | Data Sources | **Contract** source identity required for retained import/provenance. |
| 26 | Connect / Connections and Sources | Transports | **Reuse** existing selected behavior; **defer** expansion. |
| 27 | Connect / Connections and Sources | Webhooks | **Reuse** where selected, with idempotency; never business truth. |
| 28 | Connect / Mapping and Control | Venue Mapping | **Contract** now; **build** if live provider enters V1. |
| 29 | Connect / Mapping and Control | Data Mappings | **Contract** against `menu.v1` paths; build for selected import. |
| 30 | Connect / Mapping and Control | Ownership Rules | **Contract** mandatory for imported fields and operational facts. |
| 31 | Connect / Mapping and Control | Validation Rules | **Contract/build** before any Change Set reaches Core. |
| 32 | Connect / Data Movement | Imports | **Reshape** accepted paste/import only if retained in V1. |
| 33 | Connect / Data Movement | Exports | **Defer**. |
| 34 | Connect / Data Movement | Sync Runs | **Build/reshape** minimum execution envelope for selected import. |
| 35 | Connect / Data Movement | Source Snapshots | **Build/reshape** immutable source preservation for selected path. |
| 36 | Connect / Data Movement | Change Sets | **Build** explicit controlled boundary into Core. |
| 37 | Connect / Reliability and Visibility | Connection Status | **Reuse** for selected live connection; otherwise defer renewed UI. |
| 38 | Connect / Reliability and Visibility | Sync History | **Build/reshape** minimum selected-path history. |
| 39 | Connect / Reliability and Visibility | Errors and Alerts | **Build/reshape** honest selected-path failure state. |
| 40 | Connect / Reliability and Visibility | Retries | **Reuse/reshape** bounded idempotent retry only where async work runs. |
| 41 | Connect / Reliability and Visibility | Reconciliation | **Contract** last-valid/replay; **defer** broad provider system. |
| 42 | Connect / Reliability and Visibility | Connect Audit | **Build/reshape** provenance and accepted/rejected evidence. |
| 43 | Runtime / Player Identity and Topology | Player Enrollment | **Reuse/reshape** registration and pre-registration nucleus. |
| 44 | Runtime / Player Identity and Topology | Player Authentication | **Build/reshape** restricted machine identity. |
| 45 | Runtime / Player Identity and Topology | Pairing Status | **Reuse/reshape** machine status separated from Core staff claim. |
| 46 | Runtime / Player Identity and Topology | Output Discovery | **Build** Player Output identity, even for one-output V1. |
| 47 | Runtime / Desired State and Delivery | Desired State | **Build** resolved per authenticated Output. |
| 48 | Runtime / Desired State and Delivery | Package Delivery | **Build** immutable Release-to-Package delivery. |
| 49 | Runtime / Desired State and Delivery | Asset Delivery | **Build/reshape** integrity/hash delivery for used assets. |
| 50 | Runtime / Desired State and Delivery | Override Delivery | **Build/reshape** immediate 86/restore path. |
| 51 | Runtime / Convergence and Evidence | Package Status | **Build/reshape** explicit delivery state machine. |
| 52 | Runtime / Convergence and Evidence | Showing State | **Build** authoritative actual-state proof. |
| 53 | Runtime / Convergence and Evidence | Synchronized Activation | **Defer** unless multi-output V1 is owner-required. |
| 54 | Runtime / Convergence and Evidence | Reconciliation Notifications | **Reuse/reshape** notification as prompt, authoritative reread. |
| 55 | Runtime / Health and Lifecycle | Health Reports | **Reuse/reshape** package/output-aware support evidence. |
| 56 | Runtime / Health and Lifecycle | Diagnostics | **Reshape** to authenticated or strictly local safe boundary. |
| 57 | Runtime / Health and Lifecycle | Runtime Updates | **Defer** full rollout; preserve compatibility. |
| 58 | Runtime / Health and Lifecycle | Recovery | **Build/reshape** last-known-valid Package and offline reconciliation. |
| 59 | Platform / Customer and Commercial Operations | Customer Support | **Reuse/reshape** small read-only Mosaic composed view. |
| 60 | Platform / Customer and Commercial Operations | Organization Administration | **Reuse** current behavior; **defer** broad recovery expansion. |
| 61 | Platform / Customer and Commercial Operations | Venue Administration | **Reuse** current minimum; no broad redesign. |
| 62 | Platform / Customer and Commercial Operations | Subscription Support | **Defer** beyond necessary existing support. |
| 63 | Platform / Customer and Commercial Operations | Plan and Tier Administration | **Reuse** existing entitlement input. |
| 64 | Platform / Customer and Commercial Operations | Revenue Reporting | **Defer**. |
| 65 | Platform / Entitlements and Configuration | Feature Management | **Reuse** existing capability system. |
| 66 | Platform / Entitlements and Configuration | Entitlement Policies | **Reuse/characterize** selected capability path. |
| 67 | Platform / Entitlements and Configuration | Customer Exceptions | **Reuse** only if Mosaic acceptance needs one. |
| 68 | Platform / Entitlements and Configuration | Platform Configuration | **Reuse/reshape** only Package/renderer values V1 requires. |
| 69 | Platform / Entitlements and Configuration | Environment Configuration | **Defer** renewed transfer workflow. |
| 70 | Platform / Entitlements and Configuration | Configuration History | **Reuse** existing safety; **defer** expansion. |
| 71 | Platform / Fleet and Delivery Operations | Fleet Monitoring | **Build/reshape** narrow health/showing view. |
| 72 | Platform / Fleet and Delivery Operations | Runtime Version Management | **Defer** full rings/canaries. |
| 73 | Platform / Fleet and Delivery Operations | Rendering Operations | **Build** enough queue/failure evidence for Mosaic. |
| 74 | Platform / Fleet and Delivery Operations | Package Delivery Monitoring | **Build** desired-to-showing aggregation. |
| 75 | Platform / Fleet and Delivery Operations | Connector Fleet Monitoring | **Defer** unless a live provider is required. |
| 76 | Platform / Fleet and Delivery Operations | Maintenance and Incidents | **Defer** broad workflow; preserve compatibility. |
| 77 | Platform / Governance and Safety | Workforce Authentication | **Reuse/reshape** before any Platform write; current shared-key pattern is insufficient as full governance. |
| 78 | Platform / Governance and Safety | Workforce Authorization | **Reuse/reshape** attributable scoped access as crossed. |
| 79 | Platform / Governance and Safety | Privileged Actions | **Defer** if Platform remains read-only; otherwise build explicit controls. |
| 80 | Platform / Governance and Safety | Platform Audit | **Build/reshape** for any V1 workforce mutation. |
| 81 | Platform / Governance and Safety | Compliance Evidence | **Defer** formal expansion. |
| 82 | Platform / Governance and Safety | Test Automation | **Reuse/repair** only where Mosaic verification requires it. |

## Final recommendation

Retain the content/presentation path as the Mosaic V1 **integration spine**, but do not schedule it as one long content-first queue.

The next authorized planning action should be the Mosaic reconciliation session, followed by the acceptance journey and contract gates. After those gates, run Core semantics, Theme compatibility, Runtime topology/evidence, Connect provenance, Platform read support, account/tenant sufficiency, and characterization as bounded parallel lanes. Integrate them in a controlled Release-to-Package sequence, with one migration and contract authority.

This report is the independently requested blueprint study. Earlier repository documents correctly noted that a separately mentioned future study had not yet been defined; this present request supplied that missing scope. The result is a recommendation for planning—not authorization to begin implementation.
