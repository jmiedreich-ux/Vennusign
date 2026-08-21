# Keystone — Milestone Plan

Index of the slice plans that build Keystone end to end. Each slice is independently
mergeable and leaves `master` releasable, per AGENTS.md.

**Spec:** `docs/design/proposed/keystone/decisions.md` (49 decisions).
**Register:** `docs/features/keystone/open-questions.md` (31 answered, 3 deferred).

## Governance gate

**No slice executes yet.** The design authority is in `docs/design/proposed/`, not `approved/`.
AGENTS.md requires an approved authority before implementation, and the brainstorming skill's
gate is unmet until the owner approves the spec. Every slice plan repeats this.

**Three answers run provisionally.** Q31, Q32 and Q33 are deferred, so per the register's own
rule they run on their recommended defaults and each affected slice flags the consequence in its
acceptance workbook:

| Deferred | Provisional default in force |
|---|---|
| Q31 | The pre-auth app split lands before the URL restructure (slice 2). |
| Q32 | Provisioning automation is a prerequisite feature alongside Keystone, not inside it. |
| Q33 | Keystone decides the connection-membership mechanism but does not build it. |

## The slices

| # | Slice | Plan | Builds |
|---|---|---|---|
| 1 | TenantContext contract and library | `slice-1-plan.md` | `Vennu.Tenancy`, API-side resolution |
| 2 | The front ends adopt the tenant path | `slice-2-plan.md` | Pre-auth app split, tenant-prefixed back office, relative API calls, Display route |
| 3 | Version Discovery Service | `slice-3-plan.md` | `Vennu.Vds` — assignment table, lookup, PO writes, default pointer |
| 4 | Application Discovery Service | `slice-4-plan.md` | `Vennu.Ads` — registration, health polling, healthy-set delegation |
| 5 | Product Router | `slice-5-plan.md` | `Vennu.Router` — resolve, mint token, forward, degrade |
| 6 | POS Webhook Receiver | `slice-6-plan.md` | `Vennu.WebhookReceiver` — verify, resolve, queue, forward |

Bootstrap order is not arbitrary. Slice 1 makes VDS's lookup signature a fact rather than a
guess (decision 44). Slice 3 must answer before anything routes, and cannot answer usefully
until slice 4 knows where healthy instances are (decision 45). Slice 5 depends on both. Slice 6
depends on slice 3 and on slice 1's token, but not on slice 5 — the Webhook Receiver is
deliberately a separate front door.

Slice 2 sits where it does because it is the only slice that touches customer-facing surfaces,
and putting it early means the URL shape is proven by real traffic long before anything routes
on it. It is also reversible on its own.

## Not in scope

- **Connection membership.** Q33's provisional default is decide-only. The decision and the
  version-scoped group rule are recorded in decisions 1 and 41; the build is its own feature,
  gated on the deferred tier-and-cost conversation. Tracked as **#742**.
- **System Monitor.** Decision 1 names it as later work.
- **Provisioning automation.** Decision 47 and Q32: a prerequisite feature alongside Keystone.
  It must exist before slice 5 can stand up a second version, but it is not Keystone's to build.
- **Setting `VENNU_COMPONENT_VERSION`.** Decision 46, tracked as **#726**, answered in the
  register as a separate problem to fix. Decision 35's mis-forwarding check is inert until it
  lands, and no customer may be moved before it does.
- **Correcting the concept document.** Q34 accepted, tracked as **#743**.

## Cost gate

Slices 3 to 6 each stand up an App Service. Decisions 41 and 42 settle the shape — one shared
Standard-tier plan for Keystone, separate from the versioned plan — but tier and plan cost are
deliberately deferred and **nothing is provisioned until the owner accepts that cost.** Slices 3
to 6 can be built and tested locally without it; they cannot be deployed.
