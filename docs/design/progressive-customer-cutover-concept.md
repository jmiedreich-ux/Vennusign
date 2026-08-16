# Scheduled Progressive Customer Cutover Concept

## Status and purpose

This document records an exploratory Vennusign deployment concept. It is not an approved work package, architecture decision, implementation authorization, roadmap commitment, or claim that a routing or scheduling mechanism currently exists. The milestone for this work is not yet known; this doc exists so the intent is captured rather than lost.

The concept is a deployment strategy in which multiple already-built release versions run concurrently, and each customer is assigned to one of them. A new release is published alongside the versions already serving customers, and customers are moved to it progressively, on a schedule, rather than in a single atomic cutover.

The component that performs this assignment is the **Version Router (VR)**. *(Decided.)*

Items marked *(decided)* are settled working decisions within this concept. They do not change the status above: the concept as a whole remains unapproved, and settled items still require work-package governance and architecture review before implementation.

## Framing: concurrent versions, not two environments

This is progressive delivery with per-customer version assignment. It is commonly described as blue/green, and that analogy is useful for a single rollout — one version is what a customer is on now, another is where they are headed — but it should not be taken as the model.

Blue and green are relative labels, not fixed environments or a pair of slots. They describe one rollout's before and after. The same running version is the target of one rollout and the origin of the next.

More than two versions may be live at once, and this is expected rather than exceptional: starting a rollout before the previous one completes produces three, and corrective releases overlapping feature releases compound it. Anything that assumes exactly two concurrent versions — infrastructure provisioning, naming schemes, assignment data structures, operational tooling — is assuming something this concept does not guarantee.

The number of versions supported concurrently is a decision, not an unlimited property. Database compatibility across live versions is already addressed by the expand-and-contract migration model in `DEPLOYMENT_VERSIONING.md`.

## Product intent

- A new release is published and begins serving alongside existing versions, which continue serving their assigned customers unchanged.
- Customers are moved to the new version over time, following a schedule, rather than all at once.
- Movement should be deterministic and auditable per customer (or per organization/venue — see Decisions Required), not a statistical per-request coin flip. Once a customer is moved, they stay on that version until deliberately moved again.
- A partially-complete rollout can be paused or reversed without affecting customers who have not yet moved, and ideally without disrupting customers already healthy on the new version.

## What the Version Router is, and is not

The Version Router points a customer's traffic at the correct already-running deployment version.

VR is **not** the deployment process. It does not build, promote, move, or configure artifacts. Deployment happens beforehand; by the time VR is involved, every version it can point to is already running. A cutover is therefore an assignment change, not a deployment event — there is no per-customer deployment work to schedule or spread.

Consequences of this boundary:

- Any concurrency or capacity limit belongs to the deployment process, not to VR, and must be supplied to VR rather than inferred by it.
- VR requires a result signal back from whatever performs deployment (succeeded, failed, reverted), or its assignment table will drift from what is actually running.
- Version retirement needs a defined owner: something must know when the last customer has left a version so it can be shut down. VR holds the assignment data that answers this, but retirement itself is outside VR.

## Why this is not simply DNS-level weighted routing

Azure Traffic Manager (or similar DNS-weighted routing) can split traffic by percentage, but the split is approximate: DNS answers are cached by resolvers and clients, so a given customer is not reliably pinned to one side, and there is no natural concept of "this organization" or "this customer" at the DNS layer. A schedule-driven, per-customer cutover needs a decision point that knows which customer is making the request, which DNS-level routing does not provide on its own.

Note also that Azure Traffic Manager is an existing product name. Naming this component "Traffic Manager" would invite confusion with a service that operates at a different layer and cannot satisfy this concept's requirements.

## Relationship to the existing deployment model

`docs/operations/DEPLOYMENT_VERSIONING.md` establishes that Vennusign already builds and promotes immutable, versioned artifacts, and that production never rebuilds a staging-approved component. That document owns version *identity*: the release manifest as canonical source, per-component semantic versions, carried-forward artifact identity, database migration ordering, stored-procedure contract versions, and the runtime version values exposed at `/health/version`. It explicitly places customer schedules, migration waves, rollback orchestration, and retirement outside its foundation.

This concept assumes that model rather than replacing it: the concurrently-running versions are already-built, already-promoted artifact versions. VR decides which version a given customer's traffic reaches; it does not rebuild, reconfigure, or re-version the artifacts themselves. VR consumes version identity and owns customer assignment.

## Source of schedule and selection data

The customer maintenance window is stored in the customer's Vennu profile in Platform Operations, alongside tier and KPI data. VR reads this data; it does not author it. A single source therefore supplies both *who* is eligible to move and *when* they may move.

Cost-allocation KPIs are intended to live in the same profile, at venue and organization level. Exact per-customer Azure cost is not directly observable — Azure bills shared resources such as a SQL database, storage account, or App Service plan, and no customer appears in that bill. The workable approach is to measure per-customer activity (API calls, payload sizes, compute time, SignalR connection-minutes, storage bytes) and allocate Azure cost against those activity drivers. The resulting figure is an allocation rather than a measurement, and its accuracy depends on choosing an appropriate driver per resource; that per-resource driver selection is not decided here.

## Illustrative example

A new API and Back Office release is approved and begins running. Rollout starts with a small selected cohort of organizations. Each moves at its own maintenance window. With no new errors reported for those organizations, rollout advances to the next wave, and so on until complete, after which the superseded version can be retired once no customer remains assigned to it. If an issue is detected at any point, remaining organizations stay where they are, and organizations already moved can be reverted deterministically.

## Selecting the rollout order

The first wave is a deliberately small, selected set of customers. The selection methodology is undecided, but cost and usage KPIs alone are insufficient: those track volume, whereas rollout risk tracks consequence. A single-screen venue may be negligible by every cost driver while having no fallback if its only menu fails during service; a larger multi-venue organization may generate real cost while having printed fallbacks and a responsive operations contact. The industry operating-characteristic material in `track0/industries` already distinguishes these profiles.

Two axes worth scoring separately:

- **Blast radius** — what breaks if the new version is bad. Screens per venue, venues per organization, whether the window sits in genuinely dead time, industry operating characteristics, fallback availability.
- **Signal value** — what is learned if the new version is fine. Feature surface exercised, data volume, integration count. A customer exercising three features validates little.

Several of these inputs (screens per venue, fallback availability) are not cost KPIs and are not currently held in the Platform Operations profile. This should be known before that schema is settled.

## Bug fixes and affected customers

A maintenance window protects a working customer from disruption. A customer who is already broken is not being protected by waiting, so a fix reaches them immediately rather than at their next window. The distinction is about the change's effect on that specific customer, not about severity in the abstract.

This makes "is this organization affected by the bug being fixed" a per-organization input to VR. Linking the fix to the originating support ticket supplies that input from a source already maintained. Two gaps follow:

- **Silent breakage.** Affected organizations that never filed a ticket appear healthy and would wait for their window. A shared error signature may allow telemetry-based detection to supplement ticket linkage.
- **Fix-to-ticket traceability.** The release manifest names components and commits, not tickets. Something must declare which tickets a build resolves, or VR cannot match a release to the customers it unblocks.

Because reporting organizations receive the fix first, they are also the only parties able to confirm it worked, which makes them a natural validation cohort.

Maintenance windows are expected to recur — commonly daily, at the same local time — rather than being rare one-off slots. Windows will therefore cluster around similar local hours; differing customer time zones spread this somewhat.

Everything routes through VR regardless of urgency, and there is no bypass path for hotfixes *(decided)*: a bypass would duplicate VR's function with less scrutiny and would lose per-customer version visibility precisely when it matters most. A hotfix is a wave shape within VR, not an exception to it, and remains recorded, revertible, and reportable.

## Automation

Scoring, wave assignment, triggering a cutover at a customer's window, monitoring error rate and latency per version, halting on threshold breach, and reverting a customer are all candidates for automation.

Advancing to the next wave is a judgment call — a clean signal may mean the new version is healthy, or may mean nobody exercised the broken path — and manual approval gates are a reasonable default there. Automatic *rollback* carries different risk from automatic *advancement* and can be more permissive.

For releases that are strictly corrective, an objective machine-checkable gate is available from the existing versioning model: patch-level component bump, no database migration, no API contract major change. Releases failing that check fall back to gated waves. Monitoring and revert behavior still apply.

Because windows are per-customer, a wave does not complete at a single moment. A rule is needed for when a wave counts as observed — for example, all customers in the wave having passed their window plus a defined period of live traffic. No customer should advance on the strength of an observation period that did not include their window.

## Release lifecycle and responsibilities

*This section is in progress. Release candidacy, withdrawal, PO's scope, and orchestration are settled; board columns and the split of infrastructure releases are not.*

Four responsibilities are kept separate. Collapsing any two of them costs something specific.

1. **Build and promote.** Already established in `DEPLOYMENT_VERSIONING.md`: immutable versioned artifacts, staging-approved components never rebuilt in production. Unchanged by this concept.
2. **Run a version.** Deployment stands up an instance and confirms it healthy. Owns infrastructure and any concurrency limit. Not VR.
3. **Register it.** After promotion, the version is registered with VR as a routable target carrying zero assigned customers. *(Decided: registration lives with Platform Operations. It is a step in the PO release workflow rather than a separate component.)*
4. **Assign.** VR moves customers according to the schedule. The only step that affects customers.

The seam between 3 and 4 is the one that matters. Registration is a fact about what exists; assignment is a decision about customers. If deploying a version implicitly begins serving traffic, progressive delivery is lost.

The corresponding boundary: PO tells VR that a version exists and when it may be retired; PO does not move customers. Otherwise two components decide assignment.

Retirement runs the sequence backwards — VR reports that no customer remains assigned to a version, PO tears it down and deregisters it. This also answers version-retirement ownership, since VR holds the assignment data and PO performs the teardown.

### Environments

Dev, staging, and app each run a single version. Concurrent versions exist only in app, so VR, the Webhook Receiver, and assignment-aware background filtering are exercised only there. Staging cannot validate the rollout mechanism itself; that is a known gap.

```
LOCAL          dev codes
                 |
PR             build + test on pull request
                 |
MERGE to master
                 |
DEV            auto-deploy on merge, single version
                 |
               operator: "cut release"
                 |
STAGING        artifacts built once, deployed, approved here
                 |                          <- GATE 1
APP            same artifacts promoted, never rebuilt
                 |
               previous and new version both running
               VR assigns customers between them
                                            <- GATE 2, between waves
```

Merging is not releasing. Master accumulates merges; cutting a release is a separate deliberate act over some batch of them.

### PO's scope

*(Decided.)* PO is not a build or development tracker. Builds, pull request checks, dev deployments, and staging runs are visible in GitHub, which is where engineering already works. PO's concern begins at approved-for-production: which version each customer is on, which wave is running, and whether to advance.

The operator question at Gate 1 is therefore not "did the build pass" but "should this reach customers, and in what order."

### Release board

Release candidates move through phases as cards on a board. *(Decided in shape; columns not final.)*

```
BUILT    STAGING    APPROVED    ROLLING OUT    LIVE    RETIRED
                       ^            ^           ^
                    GATE 1        waves      GATE 2 between waves
                       |________ PO acts here ________|
```

Cards are release candidates, not pull requests. PO renders the full board for context but permits moves only from APPROVED rightward.

Two things a plain board does not express, and which the design must:

- **ROLLING OUT is not a single state.** A version part-way through its waves needs to open into detail: which customers have moved, their health, and the next gate.
- **Two versions occupy LIVE simultaneously.** During a rollout the previous and new versions both serve customers. This is normal operation, not a conflict state.

The board is the summary; the version detail view is where an operator actually works.

### Release candidacy

*(Decided.)* A release becomes a candidate in PO through a deliberate manual action in GitHub, not automatically on a successful staging deployment. A green pipeline means the workflow completed, not that anyone assessed what changed; keeping the declaration manual leaves that judgment with the people who know the change, consistent with PO's scope beginning at approved-for-production.

The concrete marker is expected to be a GitHub Release on the tag: a first-class object with an author and timestamp, giving an audit record of who declared the candidate, and carrying the version number already computed at build.

PO learns of it by webhook, with a low-frequency poll as fallback. Webhook delivery can fail, and a missed event would mean a candidate silently never appears; releases are infrequent enough that polling costs little. Note that a webhook requires an inbound endpoint on the PO backend, which runs against the otherwise one-directional trust model described under Orchestration; polling alone remains acceptable, as this path is not latency-sensitive.

### Withdrawing a release

Withdrawal splits at the point customers are involved.

**Before any customer is assigned**, withdrawal is clean. The development team deleting or unpublishing the release is a judgment call they are positioned to make, nothing is serving customers, and PO drops the card.

**After customers have been assigned**, the same GitHub action cannot mean the same thing. Deleting a release does not unassign anyone, and the engineer performing it may not know how many customers are currently on that version. PO acts on the event by initiating rollback through VR, with timing determined by severity:

- Default is **window-timed** — customers revert at their next maintenance window, mirroring the treatment of non-corrective changes.
- **Immediate** rollback applies when the fault is severe enough that waiting causes more harm than the disruption of reverting mid-service.

Severity is not carried by GitHub's release event. The convention is a label — for example `rollback:immediate` versus `rollback:windowed` — applied to the release or to the pull request that withdrew it. Two constraints on that convention:

- **Absence must default to the safe option.** An untagged withdrawal is treated as window-timed and flagged for operator review. The mechanism cannot depend on someone remembering the convention at two in the morning.
- **The label is a signal, not a command.** It records what engineering believes; the PO operator decides what happens to customers and may override in either direction.

The boundary holds throughout: PO decides, VR executes, and GitHub never writes assignment directly.

The webhook payload carries the action, the release object (tag, body, author, timestamps, draft and prerelease flags), the repository and the sender. PO holds a GitHub credential already, for workflow dispatch, so it can read further detail from the API as needed — the tagged commit, the diff, linked pull requests and their labels.

A related case: if a tag is force-pushed so that a version number comes to point at different code, what is running is unaffected, since artifacts are immutable and already promoted — but the audit trail becomes misleading. PO should capture the commit SHA at registration so the mismatch is detectable.

### Orchestration

*(Decided.)* PO is a frontend over disconnected services rather than an orchestrator holding infrastructure credentials. A PO backend sits between the frontend and those services, holding operator permissions and release state, so approval authority lives in one component rather than being reimplemented in each service. VR therefore needs no operator authentication of its own: it accepts writes from the PO backend as a service identity, and serves lookups to the enforcement point and the Webhook Receiver.

CI/CD is expected to live in GitHub Actions, since the repository is already there and the approval sophistication that would otherwise favor Azure DevOps is being built into PO regardless. Trust runs one way: the PO backend dispatches workflows and polls the GitHub API for run status, so Actions never calls back into the network and no inbound endpoint is required. Azure deployment credentials remain in GitHub via federated identity rather than stored keys.

```
                    +-----------------+
                    |   PO frontend   |   operator logs in here
                    +--------+--------+
                             |  one auth surface
                    +--------v--------+
                    |   PO backend    |   permissions + release state
                    +-+------+------+-+
          +-----------+      |      +-----------+
          |                  v                  v
          |           +--------------+   +--------------+
          |           |      VR      |   |  connection  |
          |           +------+-------+   |   manager    |
          |                  v           +--------------+
          |          assignment table
          |
          |  dispatch workflow
          |                              +----------------------+
          +-- poll run status ---------->|   GitHub Actions     |
              (GitHub API)               +----------+-----------+
                                                    | federated identity
                                                    v
                                              Azure infra
```

Steps 2, 3 and retirement can be fully automatic. Assignment is automatic within a wave and deliberately gated between waves.

Note that no release pipeline currently exists in the repository: `.github/workflows` contains only test workflows, and Platform Operations today is a frontend plus authentication handlers, with no backend service, pipeline, or infrastructure credentials. The model described here is a target, not current state.

## Version discovery

Two directions are needed.

**Upward — VR learning what is running.** VR cannot route to versions it does not know about, and its set of routable targets should not be hand-maintained. This is the registration step above, driven by PO after promotion rather than by instances self-registering on startup, so that a version becomes known to VR only once it is intended to be routable.

**Downward — a client learning it is stale.** `/health/version` already exposes runtime version values. A loaded single-page application does not re-ask: Back Office fetches its bundle once and may run for hours across a cutover, leaving old client code calling a newer API. A version identifier returned on API responses, compared against what the client booted with, lets the client detect the mismatch and reload. Display is less affected, since it already recovers authoritative content periodically.

Frontend surfaces are expected to follow the same assignment as the API, so VR routes the initial bundle load as well. Which version a running client is *using* still requires the staleness check above.

## Version number determination

Version numbers should be derived automatically, with a declared major release remaining a human decision and any automatic proposal always overridable.

The derivation separates two things: **classification** of a change is a judgment, while **incrementing** from a classification is arithmetic. Keeping them apart means the resulting number is deterministic given a category.

Much of the classification is structural rather than interpretive, because `DEPLOYMENT_VERSIONING.md` already defines the relevant boundaries: whether a migration is present, whether a stored-procedure contract version changed, whether an API contract major changed. These are readable from the diff and the manifest, and are the same checks that gate the corrective-release fast path described under Automation.

Where judgment is genuinely required — no schema or contract change, but the diff adds capability rather than correcting behavior — classification may be produced by other means, including an AI classifier. Two constraints on that:

- Categories should map to the structural vocabulary already in use (schema-affecting, contract-affecting, capability-adding, corrective, documentation-only) rather than generic semantic-versioning language, so that a proposed classification can be checked against the diff. A classification of "corrective" for a change containing a migration is a contradiction the pipeline should catch. The commit history already carries a strong conventional-commit signal (`feat`, `fix`, `docs` with module scopes) that can be used before or alongside any model.
- The **category itself must be recorded**, not only the resulting version number. VR's corrective-release handling depends on knowing why a release is patch-level; a version number alone does not carry that.

## Observed constraints in the current codebase

These are noted from reading `src/Vennu.Api` as it stands. They are observations, not decisions or assigned work, and several are not VR's to resolve.

- **Background services assume a single running instance.** `HeartbeatMonitor`, `ScheduledContentActivationService`, `HappyHourEvaluatorService`, `PromotionActivationService`, `PosWebhookWorker`, and `ToastPollingService` are registered via `AddHostedService` inside the API host. A second concurrently-running API version runs a second copy of each. These services iterate all venues rather than an assigned subset, and hold last-known transition state in per-process `ConcurrentDictionary` fields, so two instances would neither see each other's work nor agree on what has already fired. See the Background services section below.
- **SignalR has no backplane.** `AddSignalR()` is registered without Azure SignalR Service or a Redis backplane. `VennuHub` groups (`screen:{id}`, `venue:{id}`, `wall:{id}`) are therefore per-process, and `SignalRScreenUpdateNotifier` on one instance cannot reach connections held by another. This affects any multi-instance topology, not only version rollout. See the Real-time connections section below.
- **POS webhooks arrive at a single endpoint.** Square, Toast, and Clover post to one URL, so the receiving instance may not be the version the affected venue is assigned to. See the POS Webhook Receiver section below.
- **Versioning granularity.** Billing, Pos, Menus, Notifications and the rest are modules within one deployable API. A change confined to one module still produces a whole new concurrently-running API instance, which affects the cost of running many versions at once.

## Real-time connections

Hub groups are currently held per-process, so no instance can reach connections held by another. Two things are needed, and they are separate concerns.

**Shared group membership across processes** is required regardless of version rollout, since it also blocks horizontal scaling within a single version. Whether this is Azure SignalR Service, a Redis backplane on self-hosted SignalR, or a purpose-built connection manager is an open question outside this concept. A custom manager is under discussion; note that replacing SignalR means reimplementing transport fallback (relevant for the Tizen and webOS shells), reconnect and backoff, group membership, and cross-instance routing — not merely the message send, since SignalR already uses WebSockets. The `IScreenUpdateNotifier` abstraction means calling code is unaffected by that choice.

**Version-scoped groups are decided.** Group names carry the version identity — for example `v1.5:venue:{id}` — so an instance reaches only the screens assigned to its own version. This keeps real-time behavior consistent with request routing and with the assignment-aware decision for background services. A shared backplane without version scoping would let any instance notify any screen, so a screen assigned to an older version would receive events generated by newer logic. Version scoping works on any transport and does not remove the shared-membership requirement; it sits on top of it.

A screen therefore moves version at reconnect rather than mid-session, which is consistent with not severing an active display session during cutover.

On urgency, `docs/architecture/player-delivery-reliability.md` records that the player periodically recovers authoritative content independently of push, and that Back Office refreshes the screen list on a ten-second cadence. A gap in real-time delivery degrades to slow rather than broken. The events that genuinely depend on push are item availability changes (86'ing, where staff expect the board to change while they are standing at it) and video wall sync ticks; scheduled content transitions, promotion transitions, and theme updates are tolerant of the periodic recovery path.

That same document also records that persistent content-change events cause the player to reload its authoritative screen content. The spurious transition publish described below would therefore trigger a real reload rather than being absorbed by client-side reconciliation.

## Background services

Three shapes were considered for running background services alongside multiple API versions: moving them out of the versioned API host into a single separate worker; leader election, where all instances run the services but one holds a lease; and assignment-aware processing, where each instance handles only the venues assigned to its own version.

**Assignment-aware is decided.** Leader election was set aside because the elected leader runs an arbitrary version, so a venue assigned to an older version would have its scheduled content and promotions evaluated by newer logic — quietly breaking the guarantee VR exists to provide. Moving the services out entirely has the same effect. Assignment-aware processing keeps background behavior consistent with request routing, with VR as the single source of truth for both.

Implications:

- `GetAllAsync()` becomes "venues assigned to my version." Each instance needs its own version identity, which is already available at runtime as `VENNU_COMPONENT_VERSION`.
- With no venue owned by two instances, the per-process `ConcurrentDictionary` state stops being a correctness problem between instances. It remains a problem across handover and restart — see below.
- Behavior when VR is unavailable needs defining. Processing no venues is safer than processing all of them.
- `ToastPollingCoordinator` already keeps its due-time in the database (`NextSyncAttemptUtc`) rather than in memory, and survives handover cleanly. It is the model the others should follow. Note that it is not a lease: two instances reading "due" would both poll, which assignment-aware filtering resolves.

### Transition state must be persisted

`ScheduledContentActivationService` and `PromotionActivationService` detect *change* rather than state: each holds `venue → last active period/promotion` in memory and notifies only when the value differs from the previous tick.

When a venue moves between versions, the receiving instance has never seen that venue and starts with an empty dictionary. Its first tick computes the current meal period, finds no previous value, and treats it as a transition — publishing `ContentUpdated` to every screen in the venue when nothing has actually changed. The same spurious publish already occurs today on any process restart, independent of version rollout.

The fix, *decided*, is to persist last-published transition state per venue — venue, service, last published identifier, timestamp — and compare against the stored value rather than process memory. Any instance picking up a venue then sees what was actually last sent and stays quiet when nothing has changed. This covers handover, restart, and scale-out with one mechanism.

On cost: reading state per venue per tick would add a query inside loops that already query per venue. Reading the whole state table once per tick into memory, comparing there, and writing only venues that actually transitioned is cheaper — one additional query per tick, and near-zero writes in steady state, since transitions are rare.

Severity if deferred: for scheduled feature rollouts the spurious publish lands during a maintenance window and is largely harmless. It is more visible for venues whose window is not genuinely dead time — late-closing bars and nightlife venues — and for corrective releases, which deliberately skip windows and therefore land mid-service on customers who are already affected. The player does not reconcile the payload against what it is showing: `docs/architecture/player-delivery-reliability.md` states that persistent content-change events cause the player to reload its authoritative screen content. A spurious publish therefore causes a real reload across every screen in the venue at once.

## POS Webhook Receiver

POS providers (Square, Toast, Clover) post to a single endpoint, so the receiving instance may not be running the version the affected venue is assigned to. A thin **POS Webhook Receiver (WR)** is placed in front of the API versions. *(Decided: a separate receiver, not handled inside VR; WR owns its own registration mapping and exposes a registration API.)*

WR performs four steps: verify the provider signature, resolve the provider's external merchant/location identifier to a Vennu venue, consult VR for that venue's assigned version, and forward the payload to that version.

```
  Square / Toast / Clover
            │
            ▼
  ┌──────────────────────┐
  │ POS Webhook Receiver │
  │                      │
  │ 1. verify signature  │
  │ 2. merchant → venue  │
  │ 3. ask VR ───────────┼──────►  Version Router
  │ 4. forward           │◄───────  (venue → version)
  └──────────┬───────────┘
             │
      ┌──────┴──────┐
      ▼             ▼
  Vennu.Api     Vennu.Api
    v1.4          v1.5
```

VR remains a lookup and stays off the data path. Merging webhook handling into VR itself was considered and set aside: it would turn a control-plane decision service into a data-plane request handler holding POS secrets and provider signature logic, changing VR's security surface, release cadence, and failure mode — a VR outage would drop POS events rather than merely block new assignments.

WR owns its own registration mapping (provider, external identifier, venue) rather than reading Vennu domain data, and exposes a registration API rather than having the mapping written into shared storage by other components. The API calls that endpoint when a venue links or unlinks a POS provider; WR only ever reads its own table. This keeps the contract an API surface that can be versioned, rather than a table shape every live API version must agree on.

Points to resolve for that registration API:

- **Authentication.** Registration assigns venue ownership of an external identifier, so an openly callable endpoint would allow hijacking a venue's POS events. Internal-only network reachability plus a service credential.
- **Idempotency.** Reconnects and retries will re-register the same pair; registration should upsert on (provider, external identifier) rather than insert.
- **Reconciliation.** A lost registration causes that venue's webhooks to disappear silently. The API should be able to re-assert its complete registration set so drift is recoverable.

Also unresolved: forwarded requests must not be re-verified by the receiving version, but must not be forgeable either, so the internal forwarding path needs its own trust boundary (network isolation or a signed internal token). And because WR depends on VR to route, WR needs defined behavior when VR is unavailable — queue, or fall back to a cached last-known assignment.

Note that WR is infrastructure sitting in front of every version and is therefore difficult to version itself; it should stay thin enough to change rarely. Note also that "WR" and "VR" are easily confused when spoken; prefer the full names in discussion.

## Deploying the supporting components

VR, the Webhook Receiver, and the connection manager cannot use the mechanism they enable. VR cannot route traffic to itself, and the Webhook Receiver sits in front of every version. They therefore deploy conventionally — one version at a time, all customers at once — which means each needs the properties progressive delivery was meant to make unnecessary:

- **Backward-compatible changes only.** There is no per-customer safety net, so a bad deployment reaches everyone.
- **Slot swap or rolling instances** for zero-downtime replacement.
- **Immediate rollback** as the sole recovery path. There is no wave to halt.

This is the substance of the instruction to keep these components thin: thin is not an aesthetic preference but a consequence of not being able to roll them out progressively, so their change frequency must stay low.

There is also a bootstrap order, not merely a dependency graph. VR must be deployed and healthy before any versioned instance is registered; the Webhook Receiver must be in place before POS traffic is routed through it.

Whether these components deploy together as a single infrastructure release or separately is undecided. Separate deployment is more work but prevents a Webhook Receiver change from being able to break routing.

### Hosting constraints

Concurrent versions do not require a particular App Service tier: multiple apps can share one App Service plan on any dedicated tier, and the practical limit is resource utilization rather than a fixed count. Compute is dedicated at the plan level rather than per app, so concurrently running versions divide the plan's CPU and memory between them.

Deployment slots are a separate matter and are **not** available on Basic; Standard is the lowest tier that offers them. Slots are also the wrong tool for the versioned API, since a slot swap is atomic and moves every customer at once — precisely what VR exists to avoid. Slots suit the supporting components above, where all-at-once with instant swap-back is the desired behavior.

One figure worth carrying into the connection-layer decision: a Windows app on the Basic tier scaled to two instances allows 350 concurrent connections per instance. Since each display holds a persistent connection, that ceiling is reached well before SignalR's own scaling characteristics become the binding constraint. Current tier and screen-count projections should be confirmed against the Azure pricing calculator for the relevant region and operating system rather than taken from general figures.

## Design considerations

- **Unit of assignment.** Whether the schedule moves individual users, venues, or whole organizations needs a decision. Organization-level assignment is likely the more coherent unit given Vennusign's existing organization/venue model, but this is not decided here.
- **Real-time connections.** Version-scoped groups mean a screen changes version at reconnect rather than mid-session; see the Real-time connections section. The shared-membership mechanism underneath remains open.
- **What the window actually protects.** Since assignment is a pointer change rather than a deployment, the window is not protecting the customer from downtime. It protects them from behavior changing mid-service. This remains a reason to honor windows, but it is a different reason than deployment downtime, and it may imply different rules.
- **Schedule representation.** What advances the rollout — a time-based ramp, explicit per-organization allow-listing, or manual approval gates between steps — is undecided. Window-waiving for corrective releases should be a first-class property of the schedule model rather than something operators work around.
- **Concurrent rollouts.** If more than one rollout can be in flight at once, a customer may be eligible to move under two schedules simultaneously. Precedence and conflict behavior need defining.
- **Rollback.** Moving a customer back to their previous version must be at least as safe and immediate as moving them forward.
- **Observability.** The diagnostic concept in `docs/design/customer-support-diagnostic-agent-concept.md` already treats "deployment version" as a correlatable field. VR should report, per customer, which version they are currently assigned to, so that concept's causal analysis can account for version skew during a rollout. Per-customer telemetry carrying a version dimension is also what indicates whether a new version is healthy for an early cohort.
- **Where the decision is enforced.** Candidate options include an edge/gateway layer in front of Vennu.Api (and possibly in front of the frontend apps), or a per-customer flag read inside the API itself. These have different operational and cost implications and are not decided here.

## Candidate approaches (not decisions)

1. **Application-level routing.** A gateway or the API itself inspects an organization identifier on each request and consults a rollout-assignment table to decide which backend version instance handles it.
2. **Feature-flag-driven single deployment.** Instead of separate running copies, one deployment reads a per-customer flag and switches internal behavior. This may be cheaper to run but requires the application to support multiple behavior paths simultaneously, which may not fit the binary artifact-promotion model in `DEPLOYMENT_VERSIONING.md`.
3. **Hybrid.** Coarse infrastructure-level routing (e.g. Traffic Manager or Front Door) for failover, combined with an application-level assignment table for the actual customer-facing rollout.

## Open questions carried forward

Raised during discussion and not yet resolved. Recorded so they are not lost.

- **Session and data-protection key sharing.** `DataProtectionCustomerSecretProtector` and `ProtectedPosOAuthStateService` both rely on ASP.NET data protection. If the key ring is per-instance rather than shared, moving a customer between concurrently running versions could invalidate their session or render protected values unreadable. This works correctly with a single instance and fails as soon as there are two, so it should be confirmed early.
- **Ticket system.** Bug-to-customer linkage depends on one, and none is identified anywhere in the repository.
- **Screen count and the connection-layer objection.** Whether the concern with SignalR is cost or scale determines whether a purpose-built connection manager is warranted, and the projected screen count determines whether the scaling concern binds at all.
- **Per-customer telemetry with a version dimension.** Without it, cohort health cannot be assessed, which removes the point of waves. Nothing currently emits it.
- **Frontend staleness detection.** A loaded Back Office client continues calling a newer API with older code across a cutover. Deferred, not resolved.
- **Module granularity.** Noted under observed constraints and not revisited. It has no fix, only a cost.

## Decisions required before planning

- Unit of assignment: organization, venue, or individual user.
- How many versions may run concurrently, and what bounds that number.
- Precedence when a customer is eligible under more than one in-flight rollout.
- Selection methodology for the first cohort, and which risk inputs beyond cost KPIs the Platform Operations profile must carry.
- Schedule shape, and who authors and approves it.
- Definition of when a wave counts as observed.
- The shared connection-membership mechanism (Azure SignalR Service, Redis backplane, or a purpose-built connection manager), including the screen-count and cost assumptions behind that choice.
- Rollback and abort semantics.
- Where the routing/assignment decision is enforced (gateway vs. in-application).
- The handoff contract between VR and the deployment process, including result reporting.
- Session and data-protection key sharing across concurrently running versions, so that moving a customer does not invalidate an active session.
- Staleness detection and reload behavior for a loaded Back Office client whose assigned version changes mid-session.
- Which classification method produces the non-structural part of a version bump, and how a proposed classification is validated against the diff.
- Which ticket system supplies bug-to-customer linkage, and how a release declares the tickets it resolves.
- Per-resource cost-allocation drivers.
- Background service behavior when VR is unavailable, and whether transition-state persistence lands before or with the first rollout.
- Authentication, idempotency, and reconciliation semantics for the Webhook Receiver registration API.
- Trust boundary for forwarded webhook requests, and Webhook Receiver behavior when VR is unavailable.
- How this interacts with the immutable release-manifest/versioning model already in place.
- Observability requirements, including per-customer version visibility tied to the diagnostic-agent concept.
- Cost and operational ownership of any new routing or gateway component.
- Interaction with the subdomain/hosting structure currently being established (`app.<service>.vennusign.com` per-environment, per-service pattern) — how many concurrent app instances per production service this implies, and how that maps to that naming scheme.

No implementation should begin from this concept alone. Approved scope, issue and work-package governance, architecture review, and acceptance criteria remain necessary.
