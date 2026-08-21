# VennuSign Deployment Growth Playbook

## Status and authority

This is the master operating playbook for growing VennuSign's Azure deployment without buying
production-scale infrastructure before the product needs it. It connects application inventory,
environment policy, App Service tiers, cost, capacity signals, rollout safety, database growth,
and support maturity.

This document is a **decision framework**, not authorization to provision or resize Azure
resources. Each move between stages requires an owner-approved change supported by the evidence
defined here. Customer, venue, and screen counts are planning signals; observed service health is
the primary scaling authority.

Pricing is an indicative USD pay-as-you-go snapshot taken **2026-08-21**. Azure prices vary by
region, agreement, currency, and time. Recheck the Azure calculator before approving a change.

Related authorities:

- `docs/operations/DEPLOYMENT_VERSIONING.md` owns release and artifact identity.
- `docs/design/proposed/keystone/decisions.md` owns the proposed Keystone design.
- `docs/features/keystone/open-questions.md` records the answered Keystone questions.
- `docs/design/progressive-customer-cutover-concept.md` is exploratory source material and is not
  implementation authority.

## 1. Authoritative application and component inventory

Capacity and cost reviews must name every application below. Do not infer an application from an
old Azure resource name.

### 1.1 Applications deployed today

| Application | Repository source | Current role | Version-routed later? |
|---|---|---|---|
| Public website | `src/www` | Public marketing and entry site | No |
| VennuSign API | `src/Vennu.Api` | Product API and current server-side composition | Yes |
| Back Office | `src/back-office` | Customer administration frontend | Yes |
| Display | `src/display` | Browser-based display/player frontend | Yes |
| Platform Operations | `src/platform-operations` | VennuSign operator frontend | No |

### 1.2 Nonproduction and shared code

| Component | Repository source | Deployment treatment |
|---|---|---|
| VennuSign Test API | `src/Vennu.TestApi` | Local/test harness only; never a production application |
| Board Engine | `src/board-engine` | Shared source library used by applications; no independent App Service |
| Theme Studio | Planned application | Development-only while designed; receives an independent deployment decision before production |

### 1.3 Planned applications and services

| Application/service | Role | Version-routed? | Initial deployment policy |
|---|---|---|---|
| Onboarding | Pre-authentication customer onboarding | No | Separate application when built |
| Platform Operations API | Server side of Platform Operations | No | Separate application when built |
| Product Router | Routes a venue to its assigned product version | No; it performs routing | Conventional deployment with instant rollback |
| Version Discovery Service (VDS) | Venue-to-version assignment authority | No | Conventional deployment with instant rollback |
| Application Discovery Service (ADS) | Version-to-healthy-target registry | No | Conventional deployment with instant rollback |
| POS Webhook Receiver | Verifies, resolves, queues, and forwards provider webhooks | No | Conventional deployment with instant rollback |
| System Monitor | Observes fleet health and requests capacity actions | No | Later; do not reserve capacity now |

There is **no independently deployed TV application and no Workbook application**. Old Azure
resources carrying those names are legacy cleanup candidates, not members of the architecture or
inputs to sizing. Board Engine is also not an independently deployed application.

## 2. Current Azure baseline

The observed baseline on 2026-08-21 is one Linux B1 App Service plan in Central US carrying a
large collection of dev, stage, production, and legacy App Service resources. The live dev
pipeline actually targets five applications: Public website, VennuSign API, Back Office, Display,
and Platform Operations. The plan is inexpensive, but the unused/stale resources make the Azure
inventory look larger than the real system and can compete for the same 1.75 GB worker when left
running.

The development SQL server is in Australia East while the App Services are in Central US. That is
tolerable for development experimentation, but it is not an acceptable production or performance-
test topology. A database used for external pilot traffic must be in the same region as its
application tier before latency evidence is accepted.

### Immediate low-cost posture

1. Keep the existing B1 plan while the five deployed applications remain healthy.
2. Stop unused stage and production applications when they are not under active test.
3. Verify and retire legacy Azure resources only after traffic, configuration, DNS, and retained
   data checks; deletion is a separate approved operation.
4. Do not operate a permanent staging plan yet. Create or activate staging for a release exercise,
   then remove or downsize its paid plan afterward.
5. Do not create production infrastructure until an external pilot has a date and acceptance gate.
6. Keep normal App Service package deployment. Do not add Docker, Azure Container Apps, AKS,
   Kubernetes, Helm, or a service mesh at this stage.

## 3. Cost and tier reference

The following prices are the reference values used in this playbook.

| Linux App Service tier | vCPU | RAM | Indicative monthly price per instance | Working density ceiling |
|---|---:|---:|---:|---:|
| B1 | 1 | 1.75 GB | $13.14 | 8 active apps |
| B2 | 2 | 3.5 GB | $25.55 | 16 active apps |
| P0v4 | 1 | 4 GB | $65.55 | 8 active apps |
| P1v4 | 2 | 8 GB | $131.40 | 16 active apps |
| P2v4 | 4 | 16 GB | $262.07 | 32 active apps |

The density values are Microsoft guidance, not hard quotas. An active deployment slot counts as
an active app because it consumes the same plan resources. This playbook therefore uses a
planning ceiling below Microsoft's maximum whenever customer traffic is present.

Sources:

- [Azure App Service for Linux pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/linux/)
- [Azure App Service plan behavior and application-density guidance](https://learn.microsoft.com/en-us/azure/app-service/overview-hosting-plans)
- [Azure App Service deployment slots](https://learn.microsoft.com/en-us/azure/app-service/deploy-staging-slots)
- [Azure Monitor autoscale guidance](https://learn.microsoft.com/en-us/azure/azure-monitor/autoscale/autoscale-best-practices)

Slots do not have a separate line-item price, but they consume CPU and memory. Stopping an
application does not eliminate the plan charge. Cost falls only when a paid plan is scaled down
or removed.

## 4. Growth stages

Counts in this table indicate when to start reviewing the next stage. They do not independently
authorize it. The metric gates in section 6 decide the change.

| Stage | Business posture | Environment shape | App Service baseline | Indicative App Service/month |
|---|---|---|---|---:|
| G0 — Build | Owner/developer use; no customers | Local plus shared dev; staging on demand; no production | Existing B1 × 1 | **$13.14** |
| G1 — Integration | Keystone or concurrent-version integration begins | Dev always on; staging activated only for release exercises | Dev B2 × 1 | **$25.55** |
| G2 — Private pilot | 1–5 organizations; controlled pilot; named contacts | Dev B1/B2; staging on demand; production has product and Keystone separation | Product B2 × 1; Keystone P1v4 × 1 | **$156.95 production** |
| G3 — Paid launch | Paying customers; 99.9% service objective; maintenance windows | Low-cost dev; permanent or release-window staging; redundant production request path | Router P0v4 × 2; Control P1v4 × 1; Product P1v4 × 2 | **$525.31 production** |
| G4 — Growth | Repeatable sales; regular releases; measurable peaks | Permanent stage; production autoscale and independent control capacity | Router P1v4 × 2; Control P1v4 × 2; Product P1v4 × 2–4 | **$788.40–$1,051.20 production** |
| G5 — Fleet scale | Hundreds of organizations or thousands of screens | Load-tested plan boundaries; version or regional isolation as justified | P1v4/P2v4 plans sized from evidence | **Workload-specific** |
| G6 — Large/multiregion | Material regional cohorts, contractual availability, or data-residency needs | Multiple regions and explicit failover architecture | Separate regional Router, product, data, and connection capacity | **Architecture review required** |

Database, observability, Key Vault, storage, network egress, identity, connection infrastructure,
and support tooling are additional. Early stages should use a range rather than false precision:

| Stage | Expected complete Azure range | Notes |
|---|---:|---|
| G0 | **$25–$50/month** | Existing B1, small development databases, restrained logs |
| G1 | **$40–$90/month** | B2 dev plus databases and temporary staging hours |
| G2 | **$220–$350/month** | Pilot production, same-region SQL, modest logs; nonproduction kept lean |
| G3 | **$600–$800/month** | Redundant production request path plus lean dev/stage and supporting services |
| G4 | **$1,000–$1,600/month** | Autoscale range, permanent stage, stronger SQL/monitoring |
| G5–G6 | Budget from measured unit economics | Cost per organization, venue, screen, request, connection-minute, and stored GB |

### G0 — Build: the current recommended stage

Applications active on the shared B1 plan:

- Public website
- VennuSign API
- Back Office
- Display
- Platform Operations

Board Engine is delivered inside its consumers and consumes no App Service of its own. Theme
Studio is not added to the always-on inventory until there is deployable code. VennuSign Test API
remains local/test only. Onboarding, Platform Operations API, Product Router, VDS, ADS, POS
Webhook Receiver, and System Monitor do not consume Azure capacity before they exist.

The existing B1 remains acceptable while all of the following are true:

- no external customer depends on it;
- no slots are required;
- no more than six of the active-app allowance is routinely used, leaving development headroom;
- plan CPU remains below 70% and memory below 75% during normal working tests;
- restarts and cold starts do not obstruct acceptance work;
- environment instability is not causing repeated developer/support time.

### G1 — Integration: first purposeful upgrade

Move dev from B1 to B2 when Keystone services or concurrent product targets need to run together.
B2 is the cost-first development tier: two vCPUs, 3.5 GB RAM, and guidance for 16 active apps at
about $25.55 per month. Development does not need permanent deployment slots, autoscale, or the
Premium feature set.

Do not permanently mirror production in staging yet. Use B2 for ordinary integrated staging and
temporarily create the required Premium plan when the test specifically covers Keystone slot
swap, multi-instance behavior, or the production topology. Run the complete test, capture
evidence, then scale down or remove the temporary paid plan. App Service is billed over the plan's
provisioned lifetime, so deleting a short-lived test plan avoids a permanent monthly charge.

### G2 — Private pilot: controlled risk for a low customer count

Production uses two plans:

| Plan | Tier/instances | Applications |
|---|---|---|
| `asp-product-prod` | B2 × 1 | Concurrent version targets for VennuSign API, Back Office, and Display |
| `asp-keystone-prod` | P1v4 × 1 | Product Router, VDS, ADS, POS Webhook Receiver, Platform Operations API; deployment slots as capacity permits |

Public website, Platform Operations frontend, and planned Onboarding remain outside customer
version routing. Board Engine ships within Back Office and Display. Theme Studio remains outside
the pilot production path. System Monitor is deferred.

This stage knowingly has single-instance failure exposure. It is permitted only for a small,
named, reversible pilot with owner-approved expectations and a tested manual rollback. It is not
the paid-customer target.

### G3 — Paid launch: production reliability boundary

Production becomes three plans:

| Plan | Tier/instances | Applications |
|---|---|---|
| `asp-keystone-router-prod` | P0v4 × 2 | Product Router only; one staging slot |
| `asp-keystone-control-prod` | P1v4 × 1 | VDS, ADS, POS Webhook Receiver, Platform Operations API; System Monitor when justified |
| `asp-product-prod` | P1v4 × 2 | Concurrent version targets for VennuSign API, Back Office, and Display |

The public website, Platform Operations frontend, and Onboarding may share inexpensive
non-versioned capacity if telemetry proves they cannot impair the Router or product API. Theme
Studio receives its own placement decision before production. Board Engine remains a library.

Stage can remain cost-controlled: keep one shared P0v4 staging plan continuously only when release
frequency makes repeated provisioning more expensive than the plan. A full three-plan stage is
created temporarily for topology, failover, or scale rehearsals.

### G4 — Growth: automated capacity

- Router: P1v4 × 2, autoscale to a tested maximum if Router latency or CPU requires it.
- Keystone control: P1v4 × 2 so control services survive a worker loss.
- Product: P1v4 × 2 minimum, autoscale to 4; move to P2v4 only if scale-out does not relieve
  per-instance memory/CPU or the active-app density is the limiting factor.
- Stage: one continuous P1v4 plan unless full-topology rehearsal is scheduled.
- SQL: move from entry-level DTU sizing to a tier selected from CPU, data IO, log IO, storage,
  connection, and latency history.

### G5/G6 — Fleet and regional scale

Do not jump directly to AKS. First split proven pressure points:

1. isolate the Product Router;
2. isolate product API capacity from static frontends;
3. split active product versions into separate plans when one version creates a noisy neighbor;
4. add managed connection capacity only when tested concurrent-connection limits require it;
5. introduce a second region only for measured latency, contractual availability, disaster
   recovery, or data-residency requirements.

Containerization and Azure Container Apps may be reconsidered for short-lived version targets when
they offer demonstrable cost or operational benefit. Kubernetes/AKS requires its own architecture,
staffing, security, cost, and failure-mode decision; customer count alone never triggers it.

## 5. Environment policy

### Development

- Optimized for low cost and fast feedback, not availability.
- Direct package deployments are acceptable.
- No deployment slots are required at G0.
- Developer data must be synthetic or explicitly nonproduction.
- Logging defaults to seven days unless an investigation requires longer retention.
- Scale only when resource pressure obstructs development or makes test evidence unreliable.

### Staging

- Exists to validate an actual release candidate and the real register/assign/cutover path.
- Does not need to be always on while releases are infrequent.
- Uses the exact staging-approved artifacts later promoted to production; production never rebuilds.
- Uses its own database, identity configuration, Key Vault, DNS, and assignments.
- Must be in the production region before performance results count.
- Becomes continuously available when releases occur at least twice per month, external acceptance
  depends on it, or provisioning/reconstruction consumes more than four engineering hours per
  month.

### Production

- Created only for an approved external pilot.
- Uses separate credentials, data, DNS, monitoring, budgets, and access control.
- SQL and request-serving App Services are colocated before external traffic.
- Product versions are separate App Services/targets. Deployment slots are used for Keystone's
  conventional all-at-once deployment, not for customer percentage routing.
- No resource is resized solely because a customer-count threshold was crossed; measured headroom
  and support obligations must support the decision.

## 6. Metric gates

These are starting thresholds. Recalibrate after at least 30 days of representative traffic and
after each material architecture change. A single spike creates an investigation, not an automatic
permanent upgrade.

### 6.1 App Service plan gates

| Signal | Scale-out/investigate gate | Scale-up or split gate |
|---|---|---|
| CPU | Average >70% for 10 minutes in three separate periods within 24 hours | >70% remains after scale-out, or one application dominates the plan |
| Memory | Average >75% for 10 minutes or repeated memory recycling | >75% after scale-out, or per-process memory requires a larger worker |
| HTTP queue | Sustained growth or >100 queued requests per active instance for 5 minutes | Queue remains after scale-out and dependency latency is ruled out |
| Active apps/slots | Reach 75% of the tier's guidance | Split by workload before reaching the published guidance ceiling |
| Restarts | More than two unplanned worker/app restarts in 24 hours | Move the offending app or increase memory after root-cause analysis |
| Manual scaling | More than two manual capacity interventions in 30 days | Enable bounded autoscale and create an operational runbook |

Scale out at 70%; scale in only after CPU is below 30% **and** memory below 50% for at least 30
minutes. Use cooldowns and minimum/maximum bounds so scale-in and scale-out rules cannot flap.

### 6.2 Product Router gates

| Signal | Action |
|---|---|
| Added Router latency exceeds 15 ms p95 for 15 minutes | Investigate VDS/ADS/cache latency; scale Router if compute-bound |
| Router CPU >60% for 10 minutes | Scale before the general 70% threshold because every request depends on it |
| Router 5xx rate >0.1% for 5 minutes | Halt rollout advancement and investigate |
| Any Router serves from degraded VDS cache | Alert immediately; prevent Platform Operations assignment changes until all Routers recover |
| Router restarts or deployment interrupts live requests | Move to two instances or correct slot/swap behavior before adding customers |

### 6.3 Versioned product gates

| Signal | Action |
|---|---|
| Product API p95 latency exceeds its agreed SLO for 15 minutes | Separate dependency latency from compute; scale only when compute-bound |
| Product 5xx rate >1% for 5 minutes, or doubles from baseline | Halt the affected rollout wave; assess venue rollback |
| A version consumes >50% of shared-plan CPU or memory | Give that version independent capacity or split the API from static frontends |
| Configured concurrent-version limit is reached | Retire a version before registering another; do not hide lifecycle failure with a larger SKU |
| Three versions remain active for more than 30 days | Operations review of stalled venues, compatibility window, and retirement blockers |

### 6.4 Database gates

Before external pilot traffic, place the production database in the same region as production App
Service. Then use:

| Signal | Action |
|---|---|
| CPU/DTU, data IO, or log IO >70% for 15 minutes in repeated periods | Tune the query/index first; increase tier if demand is legitimate |
| Any resource repeatedly reaches 90% | Immediate capacity review and rollout pause |
| Storage >70% | Forecast and increase before 80%; review retention and unexpected growth |
| Connection utilization >70% of the tested safe limit | Inspect leaks/pooling, then increase database or application capacity |
| Deadlocks or migration-lock waits affect requests | Correct concurrency/migration behavior; a larger tier is not the default fix |
| Database p95 is the dominant part of API latency | Tune/query-plan review before App Service scaling |

### 6.5 Display and connection gates

- Track connected displays, connection-minutes, disconnects, reconnect attempts, and recovery time
  by version and venue.
- Scale or add managed connection infrastructure at 70% of a load-tested safe connection limit,
  not from a vendor's theoretical maximum.
- A reconnect surge must restore 95% of expected displays within the tested recovery objective.
- More than 2% unexpected disconnects in five minutes halts rollout advancement.
- Azure SignalR Service, Redis, or a purpose-built connection manager remains a separate decision;
  do not reserve an App Service for an undecided mechanism.

### 6.6 Observability and cost gates

- Budget alerts at 50%, 75%, 90%, and 100% of the approved monthly Azure budget.
- Alert on a 25% week-over-week cost increase not explained by an approved scale event.
- Tag every paid resource with environment, application/service, owner, and cost category.
- Track cost per organization, venue, screen, 1,000 API requests, connection-minute, stored GB,
  and active product version.
- Do not buy a one- or three-year commitment until at least 60 days of steady utilization shows the
  baseline will remain allocated.
- Development logs default to 7 days, staging to 14 days, and production to 30 days initially;
  increase retention only for a stated support, security, or compliance requirement.

## 7. Support maturity gates

Infrastructure and support capability grow together. A larger SKU does not repair an operating
model that cannot detect or respond to failure.

| Stage | Service commitment | Initial response target | Required operating capability |
|---|---|---|---|
| G0 | Best effort; no external SLA | Next working session | Owner/developer diagnostics |
| G1 | Internal alpha | Same business day | Health/version evidence and repeatable dev deploy |
| G2 | Named private pilot | Sev-1 within 4 hours | Named contacts, manual rollback, incident log, tested restore |
| G3 | Paid production; 99.9% objective | Sev-1 within 1 hour | Alerting, on-call ownership, runbooks, backup restore test, rollback authority |
| G4 | Growing production | Sev-1 within 30 minutes | Rotation/backup responder, autoscale review, error budget, monthly capacity review |
| G5/G6 | Contractual or large-fleet service | Contract-specific, potentially 15 minutes/24×7 | Incident command, regional failover exercises, security response, vendor escalation |

Support signals that force a stage review:

- more than one out-of-hours Sev-1 in a month requires an on-call coverage decision;
- two response-target misses in 90 days require support-process correction before customer growth;
- two capacity-driven incidents in 30 days require a plan split, autoscale, or tier review;
- more than two manual scaling actions in 30 days require automation;
- consuming half the monthly error budget before mid-month freezes rollout advancement;
- any rollback caused by undetected cohort harm requires telemetry/acceptance improvement before
  the next wave;
- repeated support reports without correlatable venue/version telemetry block further progressive
  rollout, regardless of available compute.

## 8. Scaling trajectories

Growth does not happen along one axis. Use the trajectory matching the observed pressure.

### More customers, stable traffic per customer

Scale out product workers first. Keep VDS/ADS small because assignments and registrations grow far
more slowly than requests. Add Router workers when Router CPU or latency requires them, not merely
because the customer count rose.

### More displays and persistent connections

Measure connected displays and reconnect storms separately from API request volume. Split the
connection layer from ordinary API capacity only after load testing identifies the binding limit.
Do not infer this need from the existence of SignalR alone.

### More concurrent product versions

Every active version adds application processes and database compatibility obligations. When a
version becomes a noisy neighbor, move its product targets to a separate plan. Do not use slots as
versions: slots swap every customer at once and are reserved for Keystone rollback.

### More data or expensive queries

Tune indexes, query shapes, background schedules, and caching before increasing SQL tier. If the
working set or IO remains the limit after tuning, scale SQL independently from App Service.

### More release frequency

Make staging continuously available when reconstruction cost exceeds its monthly plan cost or when
an external release calendar depends on it. More releases may justify deployment automation and
better rollback evidence; it does not automatically justify larger production workers.

### More regions

Add a region only for a stated latency, availability, disaster-recovery, regulatory, or data-
residency objective. A second region introduces data replication, failover consistency, routing,
secret/key distribution, operational testing, and doubled minimum capacity; it is not a generic
"scale" step.

## 9. Deployment technology policy

Current policy:

- VennuSign uses normal App Service package deployments.
- Product versions run as separate App Service targets and are registered through ADS/VDS/Platform
  Operations; Docker is not required for that model.
- Public website, Back Office, Display, Platform Operations, and Theme Studio are frontend build
  artifacts unless a separately hosted server component is explicitly created.
- Board Engine is a shared library and travels with its consumers.
- Product Router, VDS, ADS, POS Webhook Receiver, Platform Operations API, and System Monitor may
  become container-ready when built, but containerization is not a prerequisite.
- Docker is reconsidered only for runtime-dependency consistency, portability, short-lived version
  environments, or a measured hosting-cost benefit.
- Azure Container Apps is evaluated before AKS for scale-to-zero or ephemeral workloads.
- AKS/Kubernetes requires a separate approved architecture and an operations team capable of owning
  cluster security, networking, upgrades, observability, and incident response.

## 10. Change procedure

Every environment or tier change records:

1. the complete application/service inventory affected;
2. the present tier, instance count, active apps, and active slots;
3. 30 days of relevant CPU, memory, request, latency, error, restart, database, connection, support,
   and cost evidence—or the reason less history is acceptable;
4. the exact metric gate crossed;
5. alternatives considered: code/query correction, scale out, scale up, plan split, retirement,
   scheduled capacity, or no change;
6. the expected monthly minimum and maximum cost;
7. the rollback/downscale plan;
8. the date for reviewing whether the added capacity is still needed;
9. owner approval before provisioning.

Emergency scale-out may occur within a pre-approved maximum to protect service. It must be reviewed
the next business day and either retained with evidence or removed.

## 11. Immediate actions from this playbook

1. Keep G0 as the approved cost posture until external pilot planning begins.
2. Inventory the existing Azure App Services against section 1 and classify every unmatched
   resource as active, retained-but-stopped, or legacy cleanup candidate.
3. Keep only Public website, VennuSign API, Back Office, Display, and Platform Operations active in
   shared dev unless an explicit test needs another application.
4. Add Azure budgets and baseline CPU, memory, request, restart, database, and log-ingestion charts.
5. Correct the database region as a prerequisite to external pilot and performance evidence, not as
   a current-development expense.
6. Build Keystone locally before provisioning its App Services where milestone acceptance permits.
7. Reprice G1/G2 immediately before the first plan change.

## 12. Review cadence

- Review costs monthly while at G0/G1.
- Review capacity and support signals before every external pilot expansion and rollout wave.
- Review the playbook after 30 days of production traffic, after any Sev-1, after a new active
  product version, and after any region or hosting-technology change.
- Update prices and observed baselines without changing the underlying decision principles.
