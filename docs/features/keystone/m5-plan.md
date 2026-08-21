# Keystone Milestone 5 — Product Router

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.
>
> **Note on style:** behaviour and file names rather than code listings, at the owner's instruction. Every task still writes a failing test first and gives an exact verification command.

**Goal:** Build the Product Router — the component that reads the tenant off every request, resolves it to a version, mints the internal token, and forwards.

**Architecture:** A new service, `src/Vennu.Router`, built on YARP (register Q12). It consumes the tenant prefix, forwards the bare path so versioned route templates never change, and carries the tenant onward in a signed token. It caches VDS answers so a VDS outage degrades rather than fails, and it is the one component positioned to emit per-customer telemetry with a version dimension.

**Tech Stack:** .NET 9, YARP (`Yarp.ReverseProxy`), `Vennu.Tenancy` from milestone 1, xunit.

**Spec:** decisions 11, 13, 14, 15, 16, 17, 25, 27, 31–35, 43. **Register:** Q1, Q3, Q12, Q13, Q14, Q17, Q19, Q27, Q28.

## Milestone discipline

This is a numbered milestone under AGENTS.md's working model, not a loose batch of work.
Before starting: create the milestone issue, record the claim in `tracker/assignments.json`,
and branch as `feature/keystone-m5-<short-name>` from merged `master`. One PR. Verify locally
(CI is suspended by owner decision — local checks *are* the gate). Obtain independent review,
never by the author. Merge, then synchronize `PROJECT_STATUS.md`, the tracker,
`ai/handoffs/current.md` and this feature's records.

**Ends with a short owner acceptance workbook** (5–10 minutes) before the next milestone starts.
A milestone that ships no UI gets a demo script instead. Only one milestone runs at a time.

## Governance gate

Does not execute until the design authority is approved. **Nothing is provisioned.** This milestone puts a new component on every request; decision 43 makes its own plan a later graduation, and the tier-and-cost conversation is still parked.

**Depends on milestones 1, 2 and 3.** The token library, a VDS to ask, and real instances to forward to.

## Global Constraints

- **Decision 2 — the Router cannot roll itself out progressively.** It is the single point every request passes through: backward-compatible only, slot swap, immediate rollback.
- **Decision 16 — never pattern-match a versioned API route.** The Router reads the tenant prefix and nothing else about the path.
- **Decision 15 — forward, never redirect to a per-version hostname.** The `__Host-` cookie prefix makes that fatal to sessions.
- **Decision 11 — the tenant is a hint.** The Router never authorizes. It selects a version and nothing else.
- **Decision 34 — the token authenticates the hop, not the claim.** Signing a caller-asserted tenant does not make it true.
- **Register Q13 — 15 ms added p95**, measured from the first deployment that puts it on the path.

## File structure

| File | Responsibility |
|---|---|
| `src/Vennu.Router/Program.cs` | Host, YARP pipeline, DI. |
| `src/Vennu.Router/Resolution/TenantResolver.cs` | Prefix off the path; nothing else. |
| `src/Vennu.Router/Resolution/VersionLookup.cs` | Ask VDS; apply the default-version rule. |
| `src/Vennu.Router/Resolution/AssignmentCache.cs` | Last-known assignments and the degraded-mode rules. |
| `src/Vennu.Router/Trust/TokenMinter.cs` | Mint the per-hop token from Key Vault key material. |
| `src/Vennu.Router/Telemetry/RequestTelemetry.cs` | Per-customer, per-version emission. |
| `tests/Vennu.Router.Tests/` | Unit tests. |

`AssignmentCache` is its own file because register Q1 and Q3 give it real rules — not-expiring, invalidating on two distinct events, alerting from the first degraded request — and burying them inside the lookup is how they get quietly softened later.

---

### Task 1: Resolve the tenant and strip the prefix

**Files:** `src/Vennu.Router/Resolution/TenantResolver.cs`, `tests/Vennu.Router.Tests/TenantResolverTests.cs`

Use `Vennu.Tenancy.TenantPath` from milestone 1 — the same parser the API uses, not a second one.

**Tests must prove:** a prefixed path yields a tenant and a bare remainder; an unprefixed path yields no tenant and is passed through untouched, since that is every pre-auth route and every request that exists today; and a malformed prefix is treated as *no* prefix rather than as an error, so a bad link reaches a normal 404 instead of a Router-specific failure page.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(router): resolve the tenant prefix and forward the bare path`

---

### Task 2: Version lookup and the default-version rule

**Files:** `src/Vennu.Router/Resolution/VersionLookup.cs`, `tests/Vennu.Router.Tests/VersionLookupTests.cs`

Ask VDS. Apply decisions 27 and 28 to whatever comes back.

**Tests must prove:** an assigned venue routes to its version; a request with **no** tenant routes to the default version, which is decision 27's unattributed path and covers every sign-in; a venue VDS reports as *not-assigned* also routes to the default, keeping that fallback in exactly one place (register Q8); and VDS reporting an **unset default** is a distinct, loud failure rather than a guess, because a system that cannot serve unattributed traffic is misconfigured and should say so.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(router): version lookup with the default-version fallback`

---

### Task 3: The assignment cache and degraded mode

**Files:** `src/Vennu.Router/Resolution/AssignmentCache.cs`, `tests/Vennu.Router.Tests/AssignmentCacheTests.cs`

Register Q1 and Q3. Serve last-known assignments when VDS is unreachable, with no time expiry, invalidating when VDS returns and when the default pointer changes.

**Tests must prove:** a cached assignment is served when VDS is unreachable, rather than the request failing; the cache does **not** expire on a timer, because time-based expiry converts a VDS outage into a staggered product outage — which is worse than slightly stale routing; the cache is invalidated when VDS returns and when the default advances; and a degraded-mode signal is emitted from the *first* cache-served request, not after a threshold, since the residual risk Q3 names is a partial failure where VDS is reachable by PO but not by the Router, and the alert is the only thing that surfaces it.

Note for the implementer, from the register: this is safer than it first looks because PO writes assignments *through* VDS, so while VDS is unreachable no assignment can change. A cache served during an outage is exactly as correct as it was when the outage began.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(router): serve cached assignments through a VDS outage`

---

### Task 4: Mint the internal token

**Files:** `src/Vennu.Router/Trust/TokenMinter.cs`, `tests/Vennu.Router.Tests/TokenMinterTests.cs`

Use `Vennu.Tenancy.TenantTokenIssuer` from milestone 1. Key material from Key Vault by managed identity (register Q17), with the vault name resolved from environment configuration rather than a constant — the owner's note on Q17 is that it changes per environment.

**Tests must prove:** the minted token names the resolved version as its audience, so it cannot be replayed at another (decision 32); it carries provenance as *asserted*, because the Router resolved a caller-supplied hint and signing it does not make it true (decision 34); its lifetime is 60 seconds (register Q16); and a missing or unreadable key is a startup failure rather than a per-request one, so the Router never runs in a state where it silently forwards unsigned.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(router): mint the per-hop tenant token`

---

### Task 5: Forward, and handle misdirection

**Files:** `src/Vennu.Router/Program.cs` (YARP pipeline), `tests/Vennu.Router.Tests/ForwardingTests.cs`

Forward to an instance the Router picks from the healthy set ADS reports (register Q6), stamping the resolved version so the receiving instance can check it (decision 35).

**Tests must prove:** the forwarded request carries the bare path and the token; the Router picks from healthy instances only and spreads across them rather than pinning to the first; a version with **no** healthy instances produces 503 with a retry hint and never reroutes to a different version (register Q11); and a 421 returned by a versioned instance is passed back to the client unchanged rather than being retried, because a retry against the same wrong premise loops.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(router): forward with a stamped version, refusing to reroute`

---

### Task 6: Per-customer telemetry with a version dimension

**Files:** `src/Vennu.Router/Telemetry/RequestTelemetry.cs`, `tests/Vennu.Router.Tests/RequestTelemetryTests.cs`

Register Q27: the Router emits it, because it is the one component that sees every request and already knows both the tenant and the resolved version. Without it, cohort health cannot be assessed — which removes the point of waves.

**Tests must prove:** every request emits tenant, resolved version, status and latency; unattributed requests emit with no tenant rather than being dropped, since sign-in volume is real signal; the Router's own added latency is emitted separately from the upstream's, because register Q13's budget is about the hop and not the request; and no customer identifier beyond the venue id reaches telemetry.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(router): emit per-customer telemetry carrying the version`

---

### Task 7: The receiving side — 421 and 307

**Files:** `src/Vennu.Api/Infrastructure/MisdirectedRequestMiddleware.cs`, `tests/Vennu.Api.Tests/Infrastructure/MisdirectedRequestTests.cs`

Decision 35, and the first milestone with an authority to compare a hint against. Also where decision 49 lands, since this is the first surface that authorizes a venue arriving in a URL.

**Tests must prove:** a request whose stamped version differs from this instance's `VENNU_COMPONENT_VERSION` gets **421** — noting this is inert until #726 lands, which the test should assert explicitly so the dependency is visible rather than silently passing; a request whose hint names one venue while the authority names another gets 421 and is **not** served on the false premise; a document navigation gets **307** to the correct URL instead, because no client code is running yet to interpret a 421; and — decision 49 — an unauthorized caller receives the identical refusal they would receive for a venue that does not exist, revealing neither its existence nor its owner, with URL correction happening only *after* authorization succeeds.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Full milestone verification**

```bash
dotnet build src/Vennu.Router/Vennu.Router.csproj -c Release
dotnet test tests/Vennu.Router.Tests/Vennu.Router.Tests.csproj
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj
```

Then the Playwright gate from `tests/ui`, since this milestone changes what answers every request.

- [ ] **Step 6: Commit** — `feat(api): refuse misdirected requests without disclosing the tenant`

---

## Excluded

- **DNS and TLS cutover** (register Q14 answers the shape: DNS points at the Router, which terminates TLS). Doing it is deployment work, gated on the cost conversation.
- **The Router's own App Service Plan.** Decision 43 makes that a later graduation, when its added p99 becomes measurable.
- **System Monitor** acting on the telemetry this milestone emits. Decision 1, later work.

## Self-review

**Spec coverage.** Decisions 11, 13, 14, 15, 16, 25, 27, 31–35 and 49 each have a task. Decision 17's self-correction is a client behaviour, delivered in milestone 4 and exercised here.

**Type consistency.** `TenantContext` and `TenantTokenIssuer` keep milestone 1's shapes; `IInstanceResolver` keeps milestone 2's.

**Known risk, stated rather than hidden.** Task 7's stamped-version check compares against `VENNU_COMPONENT_VERSION`, which #726 has not yet made real. Until it lands, every instance reports `0.0.0-local` and the check compares two placeholders — it will pass while proving nothing. The test must assert the dependency explicitly so this is visible in a failing state rather than a green one, and decision 46 means no customer may be moved until #726 is done regardless.
