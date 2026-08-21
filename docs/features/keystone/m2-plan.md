# Keystone Milestone 2 — Version Discovery Service

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.
>
> **Note on style:** this plan describes behaviour and names files rather than listing code, at the owner's instruction. Each task still ends with a test that must fail first, and every verification step gives an exact command.

**Goal:** Build VDS — the one lookup the Product Router and the Webhook Receiver call — so that a venue resolves to the version serving it.

**Architecture:** A new ASP.NET minimal-API service, `src/Vennu.Vds`, owning an assignment table and the default-version pointer. It answers a single read for callers and accepts writes only from Platform Operations as a service identity. Instance selection is delegated internally to ADS (milestone 3); until that exists, the delegation is behind an interface with a stub that returns the registered target directly.

**Tech Stack:** .NET 9, ASP.NET minimal API, Dapper against Azure SQL via `Vennu.DataAccess` (matching how `Vennu.Data` reaches the database), xunit.

**Spec:** decisions 1, 2, 25, 27, 28, 45. **Register:** Q1, Q3, Q5, Q7, Q8, Q21, Q22.

## Milestone discipline

This is a numbered milestone under AGENTS.md's working model, not a loose batch of work.
Before starting: create the milestone issue, record the claim in `tracker/assignments.json`,
and branch as `feature/keystone-m2-<short-name>` from merged `master`. One PR. Verify locally
(CI is suspended by owner decision — local checks *are* the gate). Obtain independent review,
never by the author. Merge, then synchronize `PROJECT_STATUS.md`, the tracker,
`ai/handoffs/current.md` and this feature's records.

**Ends with a short owner acceptance workbook** (5–10 minutes) before the next milestone starts.
A milestone that ships no UI gets a demo script instead. Only one milestone runs at a time.

## Governance gate

Does not execute until the design authority is approved. **Nothing is provisioned** — decisions 41 and 42 settle the hosting shape but tier and plan cost are deferred, so this milestone is built and tested locally and not deployed.

## Global Constraints

- **Decision 2 — VDS cannot roll itself out progressively.** Backward-compatible changes only, and its API is additive-only forever (decision 18).
- **Decision 25 — VDS is keyed on venue.** No organization is ever assigned a version.
- **VDS stays off the data path.** It answers lookups and accepts assignment writes. It never handles a customer request, holds a POS secret, or forwards a payload.
- **Decision 27 — the default pointer is set by PO, never inferred.** Registration must not advance it.
- **No operator authentication in VDS.** Decision 38 and the concept both place approval authority in the PO backend; VDS accepts writes from PO as a service identity.

## File structure

| File | Responsibility |
|---|---|
| `src/Vennu.Vds/Program.cs` | Host, DI, endpoint mapping. |
| `src/Vennu.Vds/Assignments/AssignmentStore.cs` | Read and write venue→version. The only place SQL for assignments lives. |
| `src/Vennu.Vds/Assignments/DefaultVersionPointer.cs` | Read and advance the default. Separate from assignments because it is a different fact with a different writer. |
| `src/Vennu.Vds/Lookup/LookupEndpoint.cs` | The caller-facing read. |
| `src/Vennu.Vds/Lookup/IInstanceResolver.cs` | The ADS seam. Stubbed in this milestone. |
| `src/Vennu.Vds/Admin/AssignmentEndpoints.cs` | PO-only writes. |
| `src/Vennu.Vds/Scripts/` | DbUp migrations for the two tables, numbered from the current head. |
| `tests/Vennu.Vds.Tests/` | Unit tests for lookup logic and endpoint behaviour. |

Assignments and the default pointer are separate files because they are separate decisions with separate writers — folding them together is how "registering a version" quietly starts advancing the default, which decision 27 forbids.

---

### Task 1: The assignment table and its migration

**Files:** `src/Vennu.Vds/Scripts/` (new migration), `src/Vennu.Vds/Assignments/AssignmentStore.cs`, `tests/Vennu.Vds.Tests/AssignmentStoreTests.cs`

Two tables: one row per venue carrying its assigned version, a `ChangedUtc`, and who changed it; and a single-row table for the default pointer. Per AGENTS.md, schema changes go through ordered DbUp migrations and new migrations start after the current head — check it with `ls src/Vennu.Data/Scripts | tail -3` before numbering.

**Tests must prove:** a venue with no row reads back as not-assigned rather than as a default or an error (register Q8); writing an assignment is idempotent on the same version; and every write records who made it, because per-customer auditability is the point of assignment existing.

- [ ] **Step 1: Write the failing tests** in `tests/Vennu.Vds.Tests/AssignmentStoreTests.cs`, against LocalDB per AGENTS.md — a rule enforced in SQL is asserted against a database, never an in-memory double.
- [ ] **Step 2: Run and confirm they fail** — `dotnet test tests/Vennu.Vds.Tests/Vennu.Vds.Tests.csproj`
- [ ] **Step 3: Write the migration and `AssignmentStore`**
- [ ] **Step 4: Run and confirm they pass** — same command
- [ ] **Step 5: Commit** — `feat(vds): assignment table, keyed on venue`

---

### Task 2: The default-version pointer

**Files:** `src/Vennu.Vds/Assignments/DefaultVersionPointer.cs`, `tests/Vennu.Vds.Tests/DefaultVersionPointerTests.cs`

Read the current default; advance it as an explicit act carrying an actor.

**Tests must prove:** the pointer never moves as a side effect of anything else — in particular, registering a version leaves it untouched (decision 27); advancing it records who did so; and reading it when unset is an error rather than a guess, because a system with no default cannot serve unattributed traffic and should say so loudly.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(vds): explicit default-version pointer, advanced only by PO`

---

### Task 3: The lookup endpoint

**Files:** `src/Vennu.Vds/Lookup/LookupEndpoint.cs`, `src/Vennu.Vds/Lookup/IInstanceResolver.cs`, `tests/Vennu.Vds.Tests/LookupEndpointTests.cs`

One read: given a venue, return the version and a resolved target (register Q5 — one round trip on a hop paid on every request). `IInstanceResolver` is the ADS seam; this milestone ships a stub returning the version's registered target.

**Tests must prove:** an assigned venue returns its version and a target; an unassigned venue returns an explicit not-assigned result that is neither an error nor a silently substituted version (register Q8), leaving the default-version fallback to the caller so it lives in one place; and an unknown venue is indistinguishable from an unassigned one, so the endpoint cannot be used to enumerate venues.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(vds): venue lookup returning version and target`

---

### Task 4: PO-only assignment writes

**Files:** `src/Vennu.Vds/Admin/AssignmentEndpoints.cs`, `tests/Vennu.Vds.Tests/AssignmentEndpointTests.cs`

Write endpoints for assigning a venue, advancing the default, and registering a version as routable with zero assigned customers. Authenticated as a service identity using the same asymmetric signed-token scheme as decision 31, verified with PO's public key.

**Tests must prove:** an unsigned or wrongly-signed write is refused; registering a version does not assign anybody and does not move the default (decision 27, and the seam the concept says matters most); and the concurrent-version limit is enforced at registration from PO configuration rather than a constant in VDS (register Q7).

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(vds): PO-authenticated assignment and registration writes`

---

### Task 5: Refuse to retire a version that is the default

**Files:** `src/Vennu.Vds/Admin/AssignmentEndpoints.cs`, `tests/Vennu.Vds.Tests/RetirementTests.cs`

Retirement asks VDS whether any customer remains assigned. Register Q22 adds a second condition.

**Tests must prove:** retirement is refused while a version is the default even when no customer is assigned to it — because unattributed traffic is not a customer assignment, so an "empty" version can still be serving every sign-in; and the refusal names the reason, so an operator knows to advance the pointer first.

- [ ] **Step 1: Write the failing tests**
- [ ] **Step 2: Run and confirm they fail**
- [ ] **Step 3: Implement**
- [ ] **Step 4: Run and confirm they pass**
- [ ] **Step 5: Commit** — `feat(vds): refuse retirement while a version is the default`

---

### Task 6: Host, health and milestone verification

**Files:** `src/Vennu.Vds/Program.cs`, `src/Vennu.Vds/Vennu.Vds.csproj`, `Vennusign.sln`

Wire the endpoints, expose `/health/version` using the same `ReleaseVersionMetadata` shape the API uses so ADS can poll VDS the way it polls everything else, and register both projects in the solution.

- [ ] **Step 1: Write a failing host test** asserting `/health/version` answers and that the lookup endpoint is reachable through the real pipeline
- [ ] **Step 2: Run and confirm it fails**
- [ ] **Step 3: Implement and register in `Vennusign.sln`**
- [ ] **Step 4: Run and confirm it passes**
- [ ] **Step 5: Full milestone verification**

```bash
dotnet build src/Vennu.Vds/Vennu.Vds.csproj -c Release
dotnet test tests/Vennu.Vds.Tests/Vennu.Vds.Tests.csproj
```

LocalDB is the default per AGENTS.md; Azure is reached only by setting `VENU_TEST_AZURE_SQL_CONNECTION_STRING` for a single run. A suite that cannot reach its database must fail rather than report a pass having asserted nothing.

- [ ] **Step 6: Commit** — `feat(vds): host, health endpoint, solution registration`

---

## Excluded

- **Behaviour when VDS itself is down** — that is the caller's problem, answered in register Q1–Q3 and built in milestones 5 and 6.
- **Real instance resolution** — the `IInstanceResolver` stub is replaced in milestone 3.
- **Deployment** — gated on the deferred cost conversation.

## Self-review

**Spec coverage.** Decisions 25, 27, 28 and 45 each have a task. Decision 2's additive-only constraint is a review rule rather than a test. Register Q5, Q7, Q8, Q21 and Q22 are each implemented and asserted.

**Type consistency.** `IInstanceResolver` is the only seam ADS replaces in milestone 3; nothing else in this milestone knows ADS exists.

**Known risk.** Task 3's not-assigned result and Task 2's unset-default error can both occur on the same request, and the caller must handle them differently — not-assigned means fall back to the default, unset default means the system is misconfigured. A reviewer should check the caller in milestone 5 distinguishes them.
