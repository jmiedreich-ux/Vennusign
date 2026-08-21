# Keystone Milestone 6 — POS Webhook Receiver

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.
>
> **Note on style:** behaviour and file names rather than code listings, at the owner's instruction. Every task still writes a failing test first and gives an exact verification command.

**Goal:** Build the POS Webhook Receiver — a thin front door for Square, Toast and Clover that verifies a provider's signature, resolves the merchant to a venue from its own registration, and forwards to the version that venue is assigned to.

**Architecture:** A new service, `src/Vennu.WebhookReceiver`, holding its own `(provider, external identifier) → venue` table and exposing a registration API the versioned API calls when a venue links or unlinks a provider. It never reads Vennu domain data — that coupling is exactly what the separate table exists to avoid. It asks VDS for the venue's version and forwards with an internal token whose provenance is **verified**, because unlike the Router it actually checked a signature.

**Tech Stack:** .NET 9, ASP.NET minimal API, `Vennu.Tenancy` from milestone 1, a durable queue for the VDS-unavailable path, xunit.

**Spec:** decisions 1, 2, 18, 31–34. **Register:** Q2, Q3.

## Milestone discipline

This is a numbered milestone under AGENTS.md's working model, not a loose batch of work.
Before starting: create the milestone issue, record the claim in `tracker/assignments.json`,
and branch as `feature/keystone-m6-<short-name>` from merged `master`. One PR. Verify locally
(CI is suspended by owner decision — local checks *are* the gate). Obtain independent review,
never by the author. Merge, then synchronize `PROJECT_STATUS.md`, the tracker,
`ai/handoffs/current.md` and this feature's records.

**Ends with a short owner acceptance workbook** (5–10 minutes) before the next milestone starts.
A milestone that ships no UI gets a demo script instead. Only one milestone runs at a time.

## Governance gate

Does not execute until the design authority is approved. Nothing is provisioned.

**Depends on milestones 1 and 2.** The token library and a VDS to ask. It does **not** depend on milestone 5 — the Webhook Receiver is deliberately a separate front door, so a Router change cannot break POS ingestion and vice versa.

## Global Constraints

- **Decision 2 — WR cannot roll itself out progressively**, and it sits in front of every version, so it must stay thin enough to change rarely.
- **WR owns its own registration mapping.** It never reads Vennu domain data and no other component writes its table. The contract is an API surface that can be versioned, not a table shape every live version must agree on.
- **Decision 33 — WR's provenance is `Verified`.** It checked a provider signature. That is a stronger assertion than the Router's and the receiving version must be able to tell.
- **Decision 18 — the registration API is additive-only forever.**
- **Prefer full names in speech** (decision 3): "WR" and "VDS" are easily confused when spoken.

## File structure

| File | Responsibility |
|---|---|
| `src/Vennu.WebhookReceiver/Program.cs` | Host, DI, endpoint mapping. |
| `src/Vennu.WebhookReceiver/Verification/` | Per-provider signature verification, one file each for Square, Toast, Clover. |
| `src/Vennu.WebhookReceiver/Registry/MerchantRegistry.cs` | The `(provider, external id) → venue` table. |
| `src/Vennu.WebhookReceiver/Registry/RegistrationEndpoints.cs` | The API the versioned API calls on link and unlink. |
| `src/Vennu.WebhookReceiver/Forwarding/Forwarder.cs` | VDS lookup, token minting, forward. |
| `src/Vennu.WebhookReceiver/Forwarding/PendingQueue.cs` | The VDS-unavailable path. |
| `tests/Vennu.WebhookReceiver.Tests/` | Unit tests. |

One verification file per provider because each has its own scheme, and a shared "verify" with three branches is how a weakening in one quietly applies to all three.

---

### Task 1: The merchant registry

**Files:** `src/Vennu.WebhookReceiver/Registry/MerchantRegistry.cs`, migration, `tests/Vennu.WebhookReceiver.Tests/MerchantRegistryTests.cs`

`(provider, external identifier) → venue`, owned entirely by WR.

**Tests must prove:** registration upserts on `(provider, external identifier)` rather than inserting, because reconnects and retries will re-register the same pair; the same external identifier can exist under two different providers without collision; and an unknown pair resolves to nothing rather than to a default, since guessing a venue for an unrecognised merchant sends someone else's sales data into a tenant.

- [ ] **Step 1: Write the failing tests** (LocalDB, per AGENTS.md)
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(webhook-receiver): merchant-to-venue registry owned by WR`

---

### Task 2: The registration API

**Files:** `src/Vennu.WebhookReceiver/Registry/RegistrationEndpoints.cs`, `tests/Vennu.WebhookReceiver.Tests/RegistrationEndpointTests.cs`

The three points the concept says this endpoint must resolve: authentication, idempotency, reconciliation.

**Tests must prove:** an unauthenticated call is refused — registration assigns venue ownership of an external identifier, so an openly callable endpoint would let anyone hijack a venue's POS events; registering the same pair twice is idempotent; a venue can re-assert its **complete** registration set so drift is recoverable, because a lost registration otherwise makes that venue's webhooks disappear silently with nothing to notice it; and re-asserting a set removes registrations no longer in it, or reconciliation only ever adds and drift accumulates.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(webhook-receiver): authenticated, idempotent, reconcilable registration`

---

### Task 3: Provider signature verification

**Files:** `src/Vennu.WebhookReceiver/Verification/SquareVerifier.cs`, `ToastVerifier.cs`, `CloverVerifier.cs`, `tests/Vennu.WebhookReceiver.Tests/VerificationTests.cs`

Move the existing schemes rather than reinventing them — `src/Vennu.Api/Pos/` already holds `ToastPosWebhookVerifier`, `SquareWebhookOptions` and the verification contracts. Read those first.

**Tests must prove:** a correctly signed payload from each provider verifies; a tampered payload does not; a payload signed with the wrong provider's secret does not, so the three cannot be crossed; and verification runs on the **raw** body rather than a re-serialised one, since re-serialising is the classic way a signature check silently starts passing everything.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(webhook-receiver): per-provider signature verification`

---

### Task 4: Forward to the assigned version

**Files:** `src/Vennu.WebhookReceiver/Forwarding/Forwarder.cs`, `tests/Vennu.WebhookReceiver.Tests/ForwarderTests.cs`

Verify, resolve merchant to venue, ask VDS, forward with a token.

**Tests must prove:** the forwarded request carries a token whose provenance is **`Verified`**, not `Asserted` — this is the whole reason decision 33 exists, and it is what lets the receiving version distinguish a checked fact from the Router's caller-supplied hint; the payload is forwarded unmodified; an unverified payload is never forwarded at all; and the receiving version is not asked to re-verify the provider signature, since it has neither the secret nor the raw body by then.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(webhook-receiver): forward verified events to the assigned version`

---

### Task 5: Queue when VDS is unavailable

**Files:** `src/Vennu.WebhookReceiver/Forwarding/PendingQueue.cs`, `tests/Vennu.WebhookReceiver.Tests/PendingQueueTests.cs`

Register Q2: queue, and drain when VDS returns. WR's failure mode differs from the Router's — providers retry, and a dropped webhook is lost sales data rather than a visible outage — so queueing loses nothing and avoids forwarding to a stale version.

**Tests must prove:** an event arriving while VDS is unreachable is queued and the provider still gets a fast success, since a slow response invites provider-side retries and duplicates; the queue drains in order once VDS returns; a queued event is verified **before** queueing, not after, so the queue never holds unverified payloads; and the queue is durable across a restart, because an in-memory queue turns a VDS blip plus a deploy into silent data loss.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(webhook-receiver): queue events through a VDS outage`

---

### Task 6: Point the versioned API at the registration API

**Files:** `src/Vennu.Api/Pos/` (the link and unlink paths), `tests/Vennu.Api.Tests/Pos/WebhookRegistrationTests.cs`

When a venue links or unlinks a POS provider, the API calls WR's registration endpoint. Find the call sites first:

```bash
grep -rn "ExternalMerchantId" src/Vennu.Api --include=*.cs
```

**Tests must prove:** linking a provider registers the pair with WR; unlinking deregisters it; and a WR call failing does **not** silently succeed the link — a venue that believes it is connected but whose webhooks go nowhere is worse than a link that visibly failed.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Full milestone verification**

```bash
dotnet build src/Vennu.WebhookReceiver/Vennu.WebhookReceiver.csproj -c Release
dotnet test tests/Vennu.WebhookReceiver.Tests/Vennu.WebhookReceiver.Tests.csproj
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj
```

Azure and external-service tests remain skipped by standing owner exception; record them as skipped rather than passing.

- [ ] **Step 6: Commit** — `feat(api): register POS links with the Webhook Receiver`

---

## Excluded

- **Retiring the API's existing webhook endpoint.** `PosWebhooksController` keeps working until WR is deployed and providers are repointed. Removing it is a later, deliberate act — and per AGENTS.md a milestone that replaces a surface retires the legacy specs it obsoletes in the same PR, so that retirement carries its own test cleanup.
- **Repointing the providers themselves.** Provider-side configuration, gated on deployment and therefore on the cost conversation.
- **Stripe.** `StripeWebhooksController` is billing, not POS, and is not version-routed by venue.

## Self-review

**Spec coverage.** Decisions 31–34 have tasks; decision 33's `Verified` provenance is the explicit subject of Task 4. Register Q2 is Task 5. Decision 18's additive-only constraint on the registration API is a review rule rather than a test.

**Type consistency.** `TenantContext` and `TenantTokenIssuer` keep milestone 1's shapes. WR mints with provenance `Verified` where the Router mints `Asserted` — the one deliberate difference, and the reason the field exists.

**Known risk.** Task 6 creates a runtime dependency from the versioned API to WR at link time. If WR is unavailable, linking fails — which is correct, but it means a WR outage blocks a customer action that has nothing to do with webhooks arriving. A reviewer should confirm the failure is surfaced as a retryable "try again" rather than a permanent error, and that a later reconciliation sweep can re-assert links made while WR was down.
