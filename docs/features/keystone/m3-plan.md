# Keystone Milestone 3 — Application Discovery Service

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.
>
> **Note on style:** behaviour and file names rather than code listings, at the owner's instruction. Every task still writes a failing test first and gives an exact verification command.

**Goal:** Build ADS — the registry that knows where every deployed app is and which instances are healthy — and replace milestone 2's stub so VDS resolves real targets.

**Architecture:** A new service, `src/Vennu.Ads`, holding `(app, version) → set of instances` and health-polling each one continuously. It is a registry and a health reporter; it never picks an instance for a caller and never sits on the data path. VDS calls it internally, so callers still see exactly one lookup.

**Tech Stack:** .NET 9, ASP.NET minimal API, a hosted service for polling, Dapper, xunit.

**Spec:** decisions 1, 2, 45. **Register:** Q6, Q9, Q10, Q11.

## Milestone discipline

This is a numbered milestone under AGENTS.md's working model, not a loose batch of work.
Before starting: create the milestone issue, record the claim in `tracker/assignments.json`,
and branch as `feature/keystone-m3-<short-name>` from merged `master`. One PR. Verify locally
(CI is suspended by owner decision — local checks *are* the gate). Obtain independent review,
never by the author. Merge, then synchronize `PROJECT_STATUS.md`, the tracker,
`ai/handoffs/current.md` and this feature's records.

**Ends with a short owner acceptance workbook** (5–10 minutes) before the next milestone starts.
A milestone that ships no UI gets a demo script instead. Only one milestone runs at a time.

## Governance gate

Does not execute until the design authority is approved. Nothing is provisioned.

## Global Constraints

- **Registration is automated, never hand-entered.** The deploy pipeline writes to ADS as part of health-gate-then-register. This is what solves the labour problem for frequent releases — ADS existing is not what solves it.
- **`(app, version)` maps to a *set* of instances**, not one. A version may scale out, and that is deployment's decision, not ADS's.
- **ADS is continuous, not deploy-time.** A version can go unhealthy weeks after shipping for reasons unrelated to deployment. A one-time gate cannot see that.
- **The Router picks the instance** (register Q6), from the healthy set ADS reports. ADS does not select, and no managed load balancer is introduced — that would put a paid component into a design whose cost conversation is parked.

## File structure

| File | Responsibility |
|---|---|
| `src/Vennu.Ads/Program.cs` | Host, DI, endpoint mapping. |
| `src/Vennu.Ads/Registry/InstanceRegistry.cs` | The `(app, version) → instances` store. |
| `src/Vennu.Ads/Registry/RegistrationEndpoints.cs` | Pipeline-facing register and deregister. |
| `src/Vennu.Ads/Health/HealthPoller.cs` | The hosted service. Polls every registered instance on a cadence. |
| `src/Vennu.Ads/Health/InstanceHealth.cs` | Consecutive-failure state and the healthy/unhealthy transition rules. |
| `src/Vennu.Ads/Query/HealthySetEndpoint.cs` | What VDS calls. |
| `src/Vennu.Vds/Lookup/AdsInstanceResolver.cs` | Replaces milestone 2's stub. |
| `tests/Vennu.Ads.Tests/` | Unit tests. |

`InstanceHealth` is separate from `HealthPoller` on purpose: the transition rules are pure and must be testable without a timer or a network.

---

### Task 1: The instance registry

**Files:** `src/Vennu.Ads/Registry/InstanceRegistry.cs`, migration, `tests/Vennu.Ads.Tests/InstanceRegistryTests.cs`

Store `(app, version) → set of instances`, each with a hostname and a registered-at timestamp.

**Tests must prove:** registering the same instance twice upserts rather than duplicating, because reconnects and pipeline retries will do exactly that; a version can hold more than one instance; and deregistering the last instance of a version leaves the version known-but-empty rather than deleting it, so the difference between "never deployed" and "deployed and now gone" survives.

- [ ] **Step 1: Write the failing tests** (LocalDB, per AGENTS.md)
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(ads): instance registry keyed on app and version`

---

### Task 2: Health transition rules

**Files:** `src/Vennu.Ads/Health/InstanceHealth.cs`, `tests/Vennu.Ads.Tests/InstanceHealthTests.cs`

Pure state machine. Register Q9 sets the thresholds: unhealthy after 3 consecutive failures, healthy again after 2 consecutive successes.

**Tests must prove:** one failure does not eject a healthy instance, so a single blip does not shrink the pool; three consecutive do; a success part-way through resets the count rather than merely pausing it; and a newly registered instance starts *unhealthy* until it passes, because assuming health on arrival is how traffic reaches an instance that never came up.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(ads): health transition rules as a pure state machine`

---

### Task 3: The health poller

**Files:** `src/Vennu.Ads/Health/HealthPoller.cs`, `tests/Vennu.Ads.Tests/HealthPollerTests.cs`

A hosted service polling every registered instance every 10 seconds (register Q9), driving the Task 2 state machine. Use `TimeProvider` and an injected HTTP abstraction so the tests need neither a clock nor a network.

**Tests must prove:** every registered instance is polled each cycle; a slow or hanging instance does not delay the others, since one bad instance must not stall the whole registry; and a poll that throws is a failure rather than an unhandled crash of the hosted service.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(ads): continuous health polling independent of deploy events`

---

### Task 4: Registration endpoint authentication

**Files:** `src/Vennu.Ads/Registry/RegistrationEndpoints.cs`, `tests/Vennu.Ads.Tests/RegistrationEndpointTests.cs`

Register Q10: the same asymmetric signed-token scheme as decision 31, with the deploy pipeline holding its own key identity, plus network restriction.

**Tests must prove:** an unsigned registration is refused; a registration signed by an unknown key is refused; and the refusal does not reveal whether the `(app, version)` already exists, because an openly probeable registry is a map of the fleet.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(ads): authenticate pipeline registration`

---

### Task 5: Replace the VDS stub, and the all-unhealthy case

**Files:** `src/Vennu.Ads/Query/HealthySetEndpoint.cs`, `src/Vennu.Vds/Lookup/AdsInstanceResolver.cs`, `tests/Vennu.Vds.Tests/AdsInstanceResolverTests.cs`

VDS asks ADS for the healthy set of an `(app, version)` and returns it as the resolved target. Register Q11 settles what happens when the set is empty.

**Tests must prove:** a version with healthy instances resolves to them; a version whose instances are *all* unhealthy produces a distinct outcome that the caller renders as 503, **not** a silent reroute to another version — serving a customer a version they are not assigned to is the failure this whole feature exists to prevent, and it would happen at the moment nobody is watching for it; and ADS being unreachable is distinguishable from a version having no healthy instances, since one is an infrastructure fault and the other is a real answer.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement, deleting milestone 2's stub**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Full milestone verification**

```bash
dotnet build src/Vennu.Ads/Vennu.Ads.csproj -c Release
dotnet test tests/Vennu.Ads.Tests/Vennu.Ads.Tests.csproj
dotnet test tests/Vennu.Vds.Tests/Vennu.Vds.Tests.csproj
```

- [ ] **Step 6: Commit** — `feat(ads): resolve healthy instances for VDS, refusing to reroute`

---

## Excluded

- **System Monitor.** Decision 1 makes it later work. ADS reports health; acting on it — scaling, redeploying, telling VDS a version is out of headroom — is System Monitor's job and is not built here.
- **A managed load-balancing layer.** Register Q6 chose the Router picking from the healthy set instead.
- **Deployment and the pipeline's registration call.** Q32 puts provisioning automation outside Keystone; ADS exposes the endpoint, the pipeline calling it is that other work.

## Self-review

**Spec coverage.** Decision 45's bootstrap requirement is satisfied by Task 5 replacing the stub. Register Q6, Q9, Q10 and Q11 each have a task and an asserting test.

**Type consistency.** `IInstanceResolver` keeps the shape milestone 2 defined; only the implementation changes.

**Known risk.** Task 3's poller and Task 1's registry can disagree during a deploy — an instance registered a moment ago has not been polled, and Task 2 starts it unhealthy. That is deliberate, but it means a freshly registered version is briefly unroutable, and the deploy pipeline's health gate must therefore wait for ADS to mark it healthy rather than assuming registration is enough. A reviewer should confirm that ordering is stated wherever the pipeline work lands.
