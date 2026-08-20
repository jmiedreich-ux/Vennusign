# Keystone — decisions so far

Status: **not an approved feature.** This file records what was settled in conversation on
2026-08-20 so it is not lost. It confers no implementation authorization. The source
concept, `docs/design/progressive-customer-cutover-concept.md`, remains explicitly
unapproved: "No implementation should begin from this concept alone."

## Name and scope

**Keystone** is the codename for the feature that builds the progressive-cutover **thin
layer and discovery services**. Named for the stone placed first that everything else
bears on, and that cannot be removed while the arch stands — which is the defining
property of this layer: it cannot roll itself out progressively, so it deploys
all-customers-at-once, backward-compatible only, with immediate rollback as the sole
recovery path.

This is a *feature* codename and is separate from *release* codenames, where v1.0 is
Mosaic.

In scope: **Product Router**, **POS Webhook Receiver**, **connection membership**,
**Version Discovery Service (VDS)**, **Application Discovery Service (ADS)**.
**System Monitor** is named in the concept but is later work.

## Hosting topology — decided in shape, not in cost

Question asked: one App Service for the three thin-layer services, one plan holding thin
layer plus discovery, or a plan each?

**Decided shape:**

- **Separate App Services, not one bundled app.** The reason these components are kept
  thin is that a Webhook Receiver change must not be able to break routing. One process
  re-couples exactly what the design separates, and since immediate rollback is the only
  recovery path, a single bundle means rolling back all three to fix one.
- **One shared App Service Plan for Keystone, not a plan each.** A plan bills whether or
  not it is full, and four of these are small. A plan each buys isolation whose need
  cannot yet be measured.
- **Standard tier, not Basic.** Deployment slots are Standard-and-up, and slot swap with
  instant swap-back is the stated recovery model for these components.
- **Keystone's plan must be separate from the versioned `Vennu.Api` plan.** This is the
  boundary that matters most, and it follows from the concept doc's own constraint that
  concurrently running versions divide their plan's CPU and memory. If Keystone shared
  that pool, starting v1.6 alongside v1.5 would starve the Product Router — which sits on
  every request and whose latency is paid on every call.
- **Product Router graduates to its own plan later,** when plan-aggregate CPU or its
  added p99 becomes measurable. Not before.
- **Connection membership is probably not an App Service at all.** If Azure SignalR
  Service or a Redis backplane wins, it is managed infrastructure with no plan. The
  mechanism is still open, so no capacity is reserved for it.

Indicative shape:

```
asp-keystone-<env>   (Standard, slots)
  ├─ vds       lookup API                 request-serving
  ├─ ads       registry + health poller    continuous, low load
  ├─ wr        POS webhook receiver        bursty; holds POS secrets
  ├─ router    Product Router              on every request  <- first to graduate
  └─ sysmon    System Monitor              later; holds Azure management credentials

asp-versions-<env>   (separate)   api v1.4, v1.5, ...
connection membership -> managed service, no plan, mechanism undecided
```

**Open thread, deliberately deferred:** tier and plan cost. Standard is a tier change from
the current `rg-basic-website` arrangement and the owner has not yet accepted that cost.
This needs its own conversation before any plan is provisioned.

## First slice — recommended, not confirmed

**VDS + ADS together.** Bootstrap order forces it: nothing routes until VDS answers, and
VDS cannot answer usefully until ADS knows where healthy instances are. It also ships with
no traffic depending on it yet, so it is independently mergeable and harmless if wrong.

Alternatives considered: Product Router first (proves the request path early, but needs a
VDS stub and puts a new component on every request before the lookup behind it is real);
Webhook Receiver first (smallest clear boundary, but exercises none of the assignment
machinery).

Not chosen by the owner yet — the brainstorming stage below is expected to settle it.

## Governance gaps to close before implementation

- Keystone is a **new feature area**. It needs its own design authority under
  `docs/design/approved/keystone/` and its own question register under
  `docs/features/keystone/`. Nothing is inherited from Menus.
- The concept document is unapproved. Its `Decisions required before planning` list is
  long and includes items Keystone cannot avoid: unit of assignment, behaviour when VDS is
  unavailable on every path, the shared connection mechanism, and the trust boundary for
  forwarded webhook requests.
- Naming is settled and should be used consistently: VDS, Product Router, ADS, System
  Monitor. "Version Router" and "enforcement point" are retired.

## Method

The build is intended to run through the **superpowers** plugin (installed at user scope
on 2026-08-20): brainstorming → git worktree → written plan in 2–5 minute tasks →
subagent-driven development with two-stage review → red/green TDD → code review against
the plan → branch finishing. No feature record or design authority was pre-written here,
deliberately: brainstorming is that workflow's first stage and pre-empting it would defeat
the point of trying it.
