# Keystone — Milestone Plan

Index of the six milestones that build Keystone end to end. Each is independently mergeable and
leaves `master` releasable, per AGENTS.md's working model.

**Spec:** `docs/design/proposed/keystone/decisions.md` (49 decisions).
**Register:** `docs/features/keystone/open-questions.md` (31 answered, 3 deferred).
**Conversational record:** `docs/features/keystone/decisions-so-far.md` — keeps the reasoning and
the rejected alternatives. It uses "slice" as a historical synonym for milestone.

## Governance gate

**No milestone executes yet.** The design authority is in `docs/design/proposed/`, not `approved/`.
AGENTS.md requires an approved authority before implementation, and the brainstorming skill's gate
is unmet until the owner approves the spec. Every milestone plan repeats this.

## Milestone discipline

Each milestone follows the GitHub-first sequence in AGENTS.md, in this order:

1. Create the milestone issue.
2. Record the claim in `tracker/assignments.json`.
3. Branch `feature/keystone-m<n>-<short-name>` from merged `master`.
4. One PR. Ship schema, API, UI and Playwright specs **together** where the milestone has them —
   tests are written with the implementation, never after.
5. Verify locally. CI is suspended by owner decision, so local checks *are* the gate: the affected
   Release builds, focused unit tests, applicable migration validation, and the Playwright UI gate.
6. Independent review, never by the author. New commits invalidate prior approval.
7. Merge, delete the branch, then synchronize `PROJECT_STATUS.md`, the tracker,
   `ai/handoffs/current.md` and this feature's records.
8. **Owner acceptance workbook**, 5–10 minutes, before the next milestone starts. A milestone that
   ships no UI gets a demo script instead — M1, M2, M3 and M6 are in that category.

**One milestone at a time.** A successor starts only after its predecessor is merged and its owner
workbook is accepted.

## The milestones

| # | Milestone | Plan | Builds | Acceptance |
|---|---|---|---|---|
| 1 | TenantContext contract and library | `m1-plan.md` | `Vennu.Tenancy`, API-side resolution | demo script |
| 2 | Version Discovery Service | `m2-plan.md` | `Vennu.Vds` — assignments, lookup, default pointer, PO writes | demo script |
| 3 | Application Discovery Service | `m3-plan.md` | `Vennu.Ads` — registry, health polling, replaces the VDS stub | demo script |
| 4 | The front ends adopt the tenant path | `m4-plan.md` | Pre-auth app split, tenant-prefixed back office, relative calls, Display route | workbook |
| 5 | Product Router | `m5-plan.md` | `Vennu.Router` — resolve, cache, mint, forward, 421/307 | workbook |
| 6 | POS Webhook Receiver | `m6-plan.md` | `Vennu.WebhookReceiver` — verify, resolve, queue, forward | demo script |

Ordering follows decisions 44 and 45 rather than convenience. M1 makes VDS's lookup signature a
fact rather than a guess. Nothing routes until VDS answers (M2), and VDS cannot answer usefully
until ADS knows where healthy instances are (M3). M4 puts the tenant in the URL, which M5 needs in
order to route on it. M6 depends on M2 and on M1's token but deliberately not on M5 — the Webhook
Receiver is a separate front door, so a Router change cannot break POS ingestion.

M4 is the only milestone touching customer-facing surfaces, which is why it is the only one
carrying a full acceptance workbook rather than a demo script.

## Where parallelism actually exists

Per `docs/MILESTONE_EXECUTION.md`, subagenting buys context capacity, review independence and a
plan audit on every milestone — but wall-clock only where tasks are genuinely disjoint. For
Keystone that is one milestone and a fraction of another.

| Milestone | Shape | Parallel? |
|---|---|---|
| M1 | Task 1 → (2, 3) → 4 → 5 | A two-way fan. `TenantPath` and `TenantToken` both depend on `TenantContext` but not on each other. |
| M2 | Serial chain | No. Each task builds on the previous store or endpoint. |
| M3 | Serial chain | No. The health rules feed the poller, which feeds the resolver. |
| M4 | Back office · onboarding · display | **Yes, genuinely.** Three surfaces that do not touch each other's files. |
| M5 | Serial chain | No. Resolve feeds lookup feeds cache feeds forward. |
| M6 | Serial chain | No. Registry feeds registration feeds forwarding. |

M4 is the milestone to plan concurrency into. Everywhere else, dispatch tasks one at a time and
take the context and independence benefits rather than manufacturing parallelism that produces
merge conflicts.

## Three answers running provisionally

Q31, Q32 and Q33 are deferred. Per the register's own rule a deferral runs on its recommended
default, and the affected milestone flags the consequence in its acceptance workbook so it stays
visible and cheap to overturn.

| Deferred | Provisional default in force | Where it bites |
|---|---|---|
| Q31 | The pre-auth app split lands before the URL restructure | M4 |
| Q32 | Provisioning automation is a prerequisite feature alongside Keystone, not inside it | before M5 |
| Q33 | Keystone decides the connection-membership mechanism but does not build it | not built |

## Not in scope

- **Connection membership.** Q33's provisional default is decide-only. Tracked as **#742**.
- **System Monitor.** Decision 1 names it as later work.
- **Provisioning automation.** Decision 47 and Q32. It must exist before M5 can stand up a second
  version, but it is not Keystone's to build.
- **Setting `VENNU_COMPONENT_VERSION`.** Decision 46, tracked as **#726**, answered in the register
  as a separate problem to fix. Decision 35's mis-forwarding check is inert until it lands, and no
  customer may be moved before it does.
- **Correcting the concept document.** Q34 accepted, tracked as **#743**.

## Cost gate

M2, M3, M5 and M6 each stand up an App Service. Decisions 41 and 42 settle the shape — one shared
Standard-tier plan for Keystone, separate from the versioned plan — but tier and plan cost are
deliberately deferred and **nothing is provisioned until the owner accepts that cost.** Those
milestones can be built, tested and accepted locally; they cannot be deployed.
