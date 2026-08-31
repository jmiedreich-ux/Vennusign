# M1-A.2 — Fixture Contracts and Seam Tests

**Status:** Proposed execution packet  
**Worker:** ChatGPT CLI — Sol  
**Planned worktree:** `m1a-2-fixture-contracts`  
**Reviewer:** Claude Opus  
**Depends on:** reviewed PASS from M1-A.0  
**May run in parallel with:** M1-A.1  
**File lock:** new, narrowly named contract/fixture/test files only; no guide files

## Job

Implement a deterministic in-process fixture path:

```
Published Presentation → Runtime Package for one Player Output → Showing evidence → read-only Platform composition
```

The contracts are internal and test-only. The fixture models exactly one organization, venue, logical Screen, and Player Output.

## Required behavior

- A Runtime Package names and refuses any Player Output other than its exact target.
- Package creation, Live-overlay resolution, and Platform composition refuse a mismatched organization or venue.
- Published Presentation is frozen; the Live overlay changes separately and targets stable Item ID plus venue.
- Showing captures actual Runtime facts: received, verified, applied, and currently showing. It does not restate Core desired assignment as actual state.
- Platform composes Core and Runtime facts without a writable copy.
- Every fixture type carries an explicit disposable/test-only marker and a deletion condition tied to the accepted minimum `menu.v1` compiler and Default Theme binding path.

## Allowed change shape

New source below `src/Vennu.Api/Core/`, `src/Vennu.Api/Runtime/`, and `src/Vennu.Api/Platform/`, plus focused tests in the location identified by M1-A.0. A minimal internal composition registration is allowed only if the source map proves it is required; it must be reported before handoff.

## Hard stops

No HTTP/SignalR map, controller, hub, migration, persistence, seed, client change, real delivery/pairing/cache/device behavior, or legacy-code move. Do not touch M1-A.1 guide files.

## Validation and review

Run focused contract tests and affected local build. Demonstrate green cases and each refusal case. Claude Opus reviews decision fidelity, scope, and actual test evidence.
