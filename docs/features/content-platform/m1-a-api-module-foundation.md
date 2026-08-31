# M1-A — API Module Foundation and Fixture Skeleton

**Status:** Proposed implementation packet; owner approval required before implementation  
**Tracking issue:** #970  
**Base:** `master` at packet creation  
**Authority:** `docs/architecture/content-platform-architecture-renewal.md` §11.10; `docs/architecture/mosaic-v1-independent-blueprint-study.md`; `AGENTS.md`

## 1. Outcome

M1-A makes the renewed boundaries real inside the existing modular monolith without changing a customer-facing behavior.

It creates a small, explicitly disposable fixture path that proves these facts can remain separate:

```
Core: Published Presentation
  → Runtime: Runtime Package for one specific Player Output
  → Runtime: Showing evidence
  → Platform: read-only composed support view
```

The fixture represents one organization, one venue, one logical Screen, and one Player Output. It is test material only—not a customer record, a public API response, or a new system of record.

## 2. Why this is first

The current Menu and import work is useful and mature, but it is Menu-shaped. Building `menu.v1` first in production code would leave the highest-risk seams unexercised: frozen presentation versus output-specific package, desired versus actual state, Screen versus Player Output, and support evidence.

M1-A exposes those seams early while the real model and Theme contracts are prepared. It does **not** approve a production Runtime contract. The fixture must be deleted when the minimum `menu.v1` compiler and Default Theme binding path replace it.

## 3. In scope

1. Add four internal module homes in `src/Vennu.Api`: `Core`, `Connect`, `Runtime`, and `Platform`.
2. Put a short `README.md` in every home. Each guide names:
   - what the module owns;
   - its allowed inputs and outputs;
   - terms it may use;
   - data and tests it owns;
   - the condition for retiring any old path it later replaces.
3. Add internal, versioned fixture contracts for:
   - venue scope;
   - one frozen Published Presentation;
   - one Runtime Package that names exactly one Player Output;
   - Showing evidence with received, verified, applied, and currently-showing facts.
4. Add a small in-process composition test that proves:
   - a package cannot be produced for a different Player Output, organization, or venue;
   - a Live overlay and a composed Platform view refuse a different organization or venue scope;
   - Platform reads composed facts and owns no copy;
   - Showing records actual Runtime facts, never desired Core assignment;
   - a Live state overlay is distinct from the frozen Published Presentation.
5. State the fixture deletion condition in code and tests: replace it when the minimum `menu.v1` compiler plus Default Theme binding path is accepted, before Mosaic acceptance.

## 4. Explicit exclusions

M1-A must not:

- add or alter public HTTP, SignalR, or client contracts;
- add a database table, migration, seed, or customer data;
- create a real Package delivery path, pairing flow, player network protocol, cache, or device behavior;
- create a Theme, a Theme editor, a renderer change, or a Theme binding contract beyond an opaque fixture compatibility identity;
- change current Menu, paste import, assignment, Screen, Player, authentication, authorization, or Platform Operations behavior;
- generalize the current Menu persistence schema;
- move legacy code just to make the new folders look complete;
- use the retired content-object label in new architecture material.

Connect exists in this milestone only as an ownership seam and guide. It does not parse, import, map, or write data.

## 5. Boundary contract

| Owner | M1-A responsibility | May not own |
|---|---|---|
| Core | fixture scope, frozen Published Presentation, desired assignment reference | player delivery, actual showing, support copy |
| Connect | future entry boundary and data-source/change-authority guide | direct writes to Core fixture |
| Runtime | Player Output identity, output-specific Package, Showing evidence, Live overlay fixture | editing or mutating Published Presentation |
| Platform | read-only composition of owned facts for support | independent persistence or command authority |

The Runtime Package must carry its target Player Output identity. A logical Screen is not a Player Output. A Published Presentation is immutable and does not include the Live overlay. The overlay is resolved by Runtime against a stable Item ID plus venue scope.

No module may import another module's storage representation. M1-A uses only internal contracts; no transport type becomes a public promise.

## 6. Required implementation shape

The work graph fixes every implementation filename before dispatch. All new production source remains beneath:

- `src/Vennu.Api/Core/`
- `src/Vennu.Api/Connect/`
- `src/Vennu.Api/Runtime/`
- `src/Vennu.Api/Platform/`

Focused tests live only under `tests/Vennu.Api.Tests/ContentPlatform/M1A/`. M1-A must not create a new deployable service or project merely to express these boundaries.

Existing `Program.cs`, `Menus`, `PlatformOperations`, `Release`, `Controllers`, `Hubs`, and `Services` remain untouched. If the proof requires any composition-root or existing-path edit, M1-A is blocked and returns to architecture.

## 7. Acceptance proof

The implementation PR must contain a Done Record and show:

1. the four module guides exist and agree with this packet;
2. the fixture data is deterministic, clearly non-production, and contains one organization/venue/Screen/Player Output;
3. the Core → Runtime → Showing path passes;
4. changing the Player Output, organization, or venue causes the Package, Live overlay, and composed Platform-view contract tests to refuse the request;
5. the Live overlay can change while the fixture's Published Presentation identity remains unchanged;
6. the Platform view is built from Core and Runtime facts and has no independently stored or writable copy of truth;
7. existing affected API tests and local build pass, or each unavailable check is honestly marked UNTESTED with reason;
8. a source search in the changed paths finds no new public endpoint mapping, migration, or persisted fixture.

## 8. Quality contract

| Field | Constraint |
|---|---|
| Protected outcome | One clear ownership path; no second writable truth and no accidental customer behavior change. |
| Operating model | Existing Vennu.Api modular monolith; internal contracts and deterministic in-process tests only. |
| Exclusions | All exclusions in section 4 are hard stop conditions. |
| Assurance model | Build plus focused automated contract tests. No live device, provider, database, or hosted proof claim. |
| Proof source | Test names/results, changed-path search, and Done Record at the PR head. |
| Permitted implementation | Module guides, fixture contracts, narrow composition, focused tests. |
| Change ceiling | No endpoint, migration, client change, new deployable project, or legacy code move. |
| Stop and escalate | Any need for a public contract, persistence, legacy behavior modification, theme semantics, auth change, or uncertain scope ownership stops work and returns to the architect. |

## 9. Dependencies and follow-on graph

**Depends on:** the owner-approved renewal direction and exact-head source-base check.

**Unblocks:** minimum `menu.v1` model/compiler lane, Default Theme binding/renderer lane, Runtime production package lane, and later Connect paste migration.

**Does not unblock by itself:** a Mosaic feature claim. The fixture is intentionally disposable.

Before a Runtime production Package is accepted, the following must be frozen: `menu.v1` stable identities, placement semantics, state names, Theme binding paths, compatibility identities, and the fixture deletion replacement.

## 10. Routing and review

- **Packet author / boundary owner:** VennueSign Architect Lead.
- **Execution shape:** M1-A is a coordinated packet set, defined in `m1-a/work-graph.md`; it is not one worker’s assignment.
- **Local Qwen:** M1-A.0 exact-head source map, M1-A.4 bounded read-only Platform composition, and M1-A.5 isolated module guides. It does not define the shared Core or Runtime contracts.
- **ChatGPT CLI using Sol:** M1-A.1 Core fixture contract and M1-A.3 Live-overlay/Showing contract—the high-judgment shared-boundary work.
- **ChatGPT CLI using Terra:** M1-A.2 output-specific Runtime Package and M1-A.6 complete journey proof.
- **Independent reviewers:** Claude Sonnet reviews the bounded Runtime Package, Platform, and guide packets; Claude Opus reviews the Core, Live/Showing, integration, and Decision Fidelity gates. A reviewer does not author the packet it reviews.
- **Packet sizing:** M1-A is eight packets, M1-A.0 through M1-A.7. Each implementation packet owns one outcome, one to three code/test files, an estimated size ceiling, observable assertions, exact command gates, a required commit, and a fixed completion-report format.
- **Targeted correction review:** the same independent reviewer checks only any identified corrections.
- **No implementation starts** until the owner accepts this packet and the Decision Fidelity Review approves it—or every finding is corrected and approved by the targeted correction review.

## 11. Owner decision requested

Approve this packet as the implementation boundary for M1-A, or name the part that should change. Approval authorizes the assigned specialist to prepare the implementation PR; it does not pre-approve any expansion beyond this packet.
