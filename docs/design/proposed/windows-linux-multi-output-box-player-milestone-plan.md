# Box Player — Milestone Plan

- **Status:** Planned. The architecture is agreed; this is **not implementation approval**.
- **Authority:** `docs/design/proposed/windows-linux-multi-output-box-player.md`. Before M1 begins, the owner promotes the settled design into `docs/design/approved/box-player/`; its `decisions.md` becomes the conflict-winning authority under `AGENTS.md`.
- **Visual companion:** `docs/design/proposed/windows-linux-multi-output-box-player-flow.svg`. It is deliberately an inspectable HTML/SVG diagram in the Keystone style: labeled arrows, exact responsibilities, and a plain-language caption.
- **Delivery model:** one milestone at a time, one GitHub issue and one independently reviewed PR per milestone. A milestone leaves `master` releasable. CI is currently suspended, so recorded local verification is the gate. Every normal milestone includes schema → API → Back Office UI → Playwright coverage; M1 is the approved schema-only compatibility exception and ends in a repeatable demo script.

## Scope boundary

This plan builds **Windows first**. It introduces real box Players without changing the customer-facing TV/direct-URL setup: those continue to pair as Screens, with an implicit Player and implicit primary PlayerOutput created behind the current experience.

The plan covers a box’s cloud control plane, its declared port inventory, its isolated local Display Runtimes, and the Back Office surfaces needed to operate them. It does not decide fleet rollout policy, a customer pilot, pricing, hardware purchasing, Linux shipment, Screen-move orchestration, automatic screenshots, or a permanent native surface host.

The first Windows hardware acceptance bar is **six concurrent outputs**. There is no artificial schema or product limit of six; supported media workloads are certified separately.

## System sequence

~~~text
M1  gives every existing Screen an implicit Player + Output without re-pairing.
 │
M2  makes real Box Players claimable, assignable, and visible in Back Office.
 │
M3  proves one Windows output can boot/display from local verified content.
 │
M4  repeats that safely for declared physical ports; acceptance reaches six outputs.
 │
M5  makes failure states, health, recovery, and offline continuity operable.
 │
M6  makes Host → Supervisor → Runtime updates signed, serial, and recoverable.
~~~

## Non-negotiable invariants

1. A Screen is logical content; a PlayerOutput is the physical assignment. The authoritative path is `Player → PlayerOutput → optional Screen`.
2. A PlayerOutput has a stable, never-reused `PortKey` unique within its Player. It is not Windows display ordering, `DISPLAY1`, Chromium display id, EDID alone, or `WallPosition`.
3. `PlayerOutput.ScreenId` is nullable and unique. An output or a Screen may be intentionally unassigned; no Screen can be on two outputs.
4. Existing Screen ids, pairing URLs/codes, menus, wall configuration, deliveries, and history survive M1. No existing device re-pairs.
5. A box claim binds the whole Player to exactly one venue. It never silently assigns content or validates a panel.
6. The Supervisor is the only component with cloud credentials. A Display Runtime has only local, output-scoped authority.
7. SignalR wakes the Supervisor; it is never the source of truth. The Supervisor pulls a versioned desired-state snapshot and treats it idempotently.
8. The current complete, verified revision remains visible through cloud and Supervisor failure. No generic offline board replaces it.
9. A late heartbeat alone never restarts a visible runtime. Replacement is warmed from verified local content and only takes the output after the prior surface is drained or gone.
10. Runtime updates are one output at a time. Supervisor replacement is performed by Player Host; a Supervisor never replaces itself.
11. The database records credential identifiers/revocation, never a device secret. Production executable artifacts and manifests are signature- and hash-verified.

## Local-agent execution contract

Every assigned task must carry this exact information in its GitHub issue or PR description:

| Required field | Rule |
|---|---|
| Context | Link the approved design decision(s), this milestone, and any task-specific contract. Read `AGENTS.md`, current handoff, tracker, status, and this feature record before editing. |
| Scope / non-goals | State the user-facing behavior and the behavior deliberately not changed. Do not borrow work from the successor milestone. |
| File ownership | Name the exact files/directories owned by the task. Migrations, DI, contracts, project files, shared test fixtures, tracker, and handoff remain orchestrator-owned unless the issue explicitly transfers them. |
| Invariants | List the data states that cannot exist, the write paths that could violate them, and the automatic assertions added. |
| Paths | Cover happy, empty, conflict, duplicate/retry, permission-denied, stale actor, restart/recovery, and existing-data paths that apply. Name any path that remains untested. |
| Evidence | Write a focused test first where possible; run it red, implement, run green. Include exact commands and output in the PR. Search the repository for every existing behavior location and report unchanged locations with a reason. |
| Handoff | State exact commit, changed behavior, validation, skipped physical/infrastructure checks, residual risks, and the single next action. |

A local agent may make a narrow implementation recommendation, but does not invent data fields, cloud commands, new daemon responsibilities, or a transport change. It stops and asks the orchestrator if the approved authority does not answer the question.

## Milestone execution discipline

Before each milestone: create its issue; claim it in the tracker; branch `feature/box-player-m<n>-<short-name>` from current merged `master`; declare file ownership; and prove the previous milestone’s owner acceptance/demo is complete.

After each milestone: independent review, merge, owner demo/workbook, then synchronize the issue, `PROJECT_STATUS.md`, tracker, `ai/handoffs/current.md`, this workstream, and affected durable design/operations records. Pushes use `[skip ci]` while CI is suspended.

---

## M1 — Compatibility foundation: Player and durable Player Output

**Goal:** introduce the relationship that future box work depends on while every current Screen workflow and player endpoint continues to work exactly as before.

**Dependency:** approved design authority.  
**Acceptance:** schema/demo script, not a new Back Office UI.  
**Excluded:** box claim, box credential, new Player routes, output discovery, OS code, statuses/events/actions, and Screen-field cleanup.

### Task 1 — Add the additive schema and backfill

**Orchestrator-owned files:** `src/Vennu.Data/Scripts/074_player_foundation.sql`, model/repository registration, migration fixture ownership.

- [ ] Inspect the actual current migration head before final naming; do not edit `001_baseline.sql`.
- [ ] Create `Players` and `PlayerOutputs` with explicit FKs, check constraints, and indexes. Include Player type (`Implicit`/future `Box`), nullable venue ownership suitable for unclaimed boxes, immutable `PortKey`, nullable unique `ScreenId`, declared/retired timestamps, and no `WallPosition` column.
- [ ] Backfill exactly one implicit Player and one implicit primary output per existing Screen. Use `legacy-screen:{ScreenId}` as the port key.
- [ ] Make the migration rerunnable only through DbUp’s journal semantics; no destructive delete/recreate technique.
- [ ] Add LocalDB migration/invariant tests proving every historical Screen now has exactly one assigned implicit output; every Screen id and existing Screen relationship remains unchanged; and a Screen cannot appear on two outputs.
- [ ] Run the migration tests red/green and record the exact migration version.

### Task 2 — Add persistence seams and model invariants

**Expected owned area:** `src/Vennu.Core.Models/`, `src/Vennu.Data/Models/`, `src/Vennu.Data/Repositories/`, and focused `tests/Vennu.Data.IntegrationTests/` files.  
**Non-goal:** do not move existing Screen device fields or change existing request contracts.

- [ ] Define the minimum Player/PlayerOutput reads required to resolve a Screen’s implicit output.
- [ ] Add repository methods that retrieve by Player, PortKey, and Screen without relying on output enumeration order.
- [ ] Add area invariants after every Player integration test: unique output port within a Player; at most one output per Screen; no output references a missing Player/Screen; implicit records stay assigned as a pair.
- [ ] Search all Screen create/pair/replacement paths and list every one in the PR.

### Task 3 — Preserve present-day Screen behavior and prove it

**Expected owned area:** `src/Vennu.Api/Controllers/ScreensController.cs`, `src/Vennu.Api/Controllers/BackOffice/BackOfficePairingController.cs`, `src/Vennu.Api/Services/ScreenManagementService.cs`, `src/Vennu.Data/Services/ScreenReplacementService.cs`, their focused tests, and `scripts/run-box-player-m1-demo.ps1`.

- [ ] Make every new Screen-first pairing/create path create its implicit Player and output in the same transaction. If a pre-registration path cannot be transactional, stop and escalate rather than leaving an orphan rule implicit.
- [ ] Keep existing Screen endpoints and response shapes compatible; client apps must not send Player ids.
- [ ] Prove retries do not create duplicate implicit Players/outputs.
- [ ] Write the demo script: migrate a fixture containing existing paired/pre-registered/replaced Screens, run current Screen operations, and assert the Screen remains usable while the implicit relationship exists once.
- [ ] Document all physical-device checks as **UNTESTED / not applicable** for M1.

**M1 owner demo:** run the script against a fresh LocalDB database; show its explicit assertions and successful exit.

---

## M2 — Box claim, declared-output setup, and Back Office flow

**Goal:** a simulated Box Player can register, show one claim code, be claimed by one venue admin, declare slots, and have each slot attached to an existing Screen, a new Screen, or no Screen.

**Dependency:** M1 merged and accepted.  
**Acceptance:** 5–10 minute Back Office workbook using a simulated box.  
**Excluded:** real Windows enumeration, Display Runtime, local cache, live hardware health, and remote arbitrary command execution.

### Task 1 — Claim and credential persistence

**Orchestrator-owned files:** next ordered DbUp migration (expected `075_player_claim_and_setup.sql`), Player claim/credential model/repository interfaces, DI.

- [ ] Add `PlayerClaimCodes`: Player id, code hash only, issued/expires/claimed/cancelled times, claimant user/venue, and a uniqueness rule allowing one active code only for an unclaimed Player.
- [ ] Add `PlayerCredentials`: credential id/public-key thumbprint or equivalent public metadata, issued/revoked/replaced timestamps. Never persist the credential secret.
- [ ] Make a claim an atomic compare-and-claim operation: expiry, cancellation, already-claimed, wrong venue, and concurrent double-submit all end honestly with no partial ownership.
- [ ] Add unclaim/transfer behavior: revoke active credentials immediately, clear current venue, retain history, require a new physical claim. Do not move Screens automatically.
- [ ] Assert persistence invariants and LocalDB tests for all write paths.

### Task 2 — Registration, desired state, and setup API contracts

**Expected owned area:** `src/Vennu.Api/Contracts/Players/` (new), Player registration/controller/service files (new), `src/Vennu.Api/Hubs/VennuHub.cs` only if a narrowly defined Player doorbell is required, and focused API tests.

- [ ] Define a registration request that creates or resumes an unclaimed Box Player and returns only a short-lived claim presentation state; it must not create Screens.
- [ ] Define a claimed Supervisor authentication/desired-state read that returns a whole-box versioned snapshot, not raw content, and is safe to poll/retry.
- [ ] Define declared-output inventory submission: new observed port becomes pending/new; known missing port remains declared/disconnected later rather than deleted.
- [ ] Define assign/unassign operations with server-derived venue authorization and a unique Screen assignment guard.
- [ ] Keep `ScreenPairingCode` behavior for TV/direct URL. Do not repurpose or break it.
- [ ] Write focused tests for unauthenticated, unauthorized venue, expired code, repeated registration, duplicate output, Screen already assigned, and stale desired-state version.

### Task 3 — Back Office setup and operational starting point

**Expected owned area:** `src/back-office/` Player feature files (new, placed according to its current feature conventions), focused API client files, `tests/ui/specs/` Player setup spec, and any narrow test seed extension.

- [ ] Add a clear “Box Players” entry point that separates a real box from current Screen pairing.
- [ ] Implement the claim step: enter the six-digit code; show expired/used/wrong-venue retry states without losing the page context.
- [ ] Implement declared-output setup: show each port as a slot, allow installer label, attach existing Screen, create a Screen, or leave unassigned.
- [ ] Render declared-but-disconnected and intentionally unassigned as different states.
- [ ] Do not show `WallPosition` as a port mapping control.
- [ ] Add Playwright coverage for claim, attach, create, leave unassigned, duplicate assignment refusal, refresh/resume, and venue access refusal.

### Task 4 — Simulated-box acceptance fixture

**Expected owned area:** `src/Vennu.TestApi/` or the established test seam, focused tests, and `docs/features/box-player/m2-acceptance-workbook.md`.

- [ ] Provide a deterministic simulated Box Player that can register, expose three named test ports, report a disconnect, and submit an EDID-change observation. It must be test-only and never ship as a production bypass.
- [ ] Seed two existing Screens plus an unassigned slot.
- [ ] Build the owner workbook around a physical claim-like flow: code shown by simulated box → admin claims → outputs appear → one attach, one create, one left empty → refreshed page remains correct.
- [ ] Record that real Windows connector discovery is intentionally deferred to M4.

---

## M3 — Windows lab foundation: one output stays alive from local content

**Goal:** prove the local topology on one Windows output: Player Host starts/watches a Supervisor and one Display Runtime; committed local content renders before cloud recovery; a Supervisor restart does not intentionally remove visible content.

**Dependency:** M2 accepted and a named Windows lab machine with interactive kiosk session.  
**Acceptance:** lab demo script plus observed manual run.  
**Excluded:** multi-output, customer deployment, Linux, broad hardware certification, automatic update, arbitrary remote shell, and claim UX changes.

### Task 1 — Create bounded local projects and lifecycle ownership

**Orchestrator-owned files:** new project definitions/solution wiring/packaging configuration.  
**Expected project boundary:** `src/Vennu.Player.Host/`, `src/Vennu.Player.Supervisor/`, `src/Vennu.DisplayRuntime/` (names are confirmed by the orchestrator when M3 opens).

- [ ] Add a Player Host that is the lifecycle root, not a Windows Service rendering session. It must run in the interactive kiosk user’s desktop session.
- [ ] Add a Supervisor that can be independently stopped/restarted by Host.
- [ ] Add one output-scoped Display Runtime launched by Supervisor but deliberately not killed merely because Supervisor exits.
- [ ] Define process identity, logs, bounded restart policy, and explicit ownership; do not add cloud authority to Host or Runtime.
- [ ] Add process-lifecycle unit tests with fakes for crash, normal stop, and Host restart.

### Task 2 — Local content store and read-only gateway

**Expected owned area:** Supervisor local-store/gateway files and their unit tests.

- [ ] Implement SQLite metadata/journal plus immutable revision/blob directories. Supervisor is the sole writer.
- [ ] Require complete hash validation before atomic pointer promotion; retain current plus two verified fallbacks for the one assigned Screen subject to a configured disk budget.
- [ ] Implement a Host-owned read-only Local Content Gateway serving committed revision files only.
- [ ] Give Runtime a pinned local revision reference; it must not fetch an uncommitted asset or require a running Supervisor once applied.
- [ ] Test interrupted download, bad hash, interrupted promotion, cold boot from a complete cache, and Gateway read rejection outside committed roots.

### Task 3 — Authenticated local control and one Display Runtime

**Expected owned area:** named-pipe protocol files, Runtime host/renderer integration, and focused tests.

- [ ] Use a Windows authenticated named pipe (or the selected equivalent whose security properties are documented) between Supervisor and Runtime.
- [ ] Implement only: hello/identity, assignment, revision-ready, applied, render-progress, fault, drain, and surface-released.
- [ ] Start the Runtime hidden/warm from a verified local revision; make it acquire the chosen lab surface only when ready.
- [ ] Refuse a message for another output, an unknown revision, or an unauthenticated caller.
- [ ] Reuse the current display renderer through the local gateway where practical; do not give it a cloud SignalR connection.

### Task 4 — One-output lab proof

**Expected owned area:** lab demo script/runbook and focused automated tests.

- [ ] Demonstrate boot with network unavailable: cached content is visible before cloud reconnect.
- [ ] Demonstrate Supervisor restart: Runtime remains visible, then Supervisor reattaches and reconciles.
- [ ] Demonstrate Runtime crash: Host/Supervisor applies bounded recovery and records any unavoidable blank interval honestly.
- [ ] Verify no Runtime cloud credentials and no direct cloud SignalR connection.
- [ ] Mark physical display/codec results with exact machine, OS, GPU, cable topology, and content fixture; they are evidence, not generic test passes.

---

## M4 — Windows multi-output: physical-port inventory and six-output acceptance

**Goal:** a Windows Box Player declares and retains durable physical connector slots, maps each assigned slot to an isolated Runtime, lets an installer identify the real display, and passes a six-concurrent-output acceptance workload.

**Dependency:** M3 accepted; a six-output lab topology is available.  
**Acceptance:** Back Office workbook plus hardware acceptance record.  
**Excluded:** Linux, screen moves, panel auto-acceptance, arbitrary desktop placement on unsupported compositor, and media claims beyond recorded certified workloads.

### Task 1 — Windows topology provider and port identity

**Expected owned area:** Windows-specific topology provider inside the Player Supervisor, narrow fake topology provider, unit/integration tests.

- [ ] Enumerate with `QueryDisplayConfig(QDC_ALL_PATHS)` for inventory reconciliation, not in a tight health loop.
- [ ] Construct `PortKey` from connector-scoped identity including `DisplayTarget.AdapterRelativeId`; never use adapter LUID, `DISPLAY<n>`, Chromium/Electron id, array position, or EDID as the key.
- [ ] Debounce topology changes and preserve declared ports when a TV is asleep/unplugged.
- [ ] Capture EDID only as observed corroboration; report mismatch against expected panel instead of remapping or changing assignment.
- [ ] Test reordered enumeration, duplicate/blank EDID, disappeared known port, and newly seen unknown port.

### Task 2 — Inventory reconciliation, panel history, and identify action

**Orchestrator-owned files:** next migration (expected `076_player_output_observation.sql`), stable observation/history models, DI.

- [ ] Add expected-vs-observed panel history and latest observation data without putting transient state on PlayerOutput configuration.
- [ ] Reconcile discovered ports: declared connection changes; new pending ports; no automatic delete; explicit retire only.
- [ ] Add only the approved durable `IdentifyOutput` action. It carries Player/output scope, requester, expiry, idempotency key, state, and safe result text; it is never an arbitrary command runner.
- [ ] Implement a large temporary label on the selected physical output.
- [ ] Require explicit Back Office acceptance before replacing the expected panel fingerprint.
- [ ] Test idempotent action retry, expired action, wrong-output result, and acceptance audit.

### Task 3 — Per-output Runtime orchestration

**Expected owned area:** Host/Supervisor/Runtime output orchestration and local-process tests.

- [ ] Run one isolated Runtime per assigned declared output, with separate local permissions and fault domain.
- [ ] Reconcile assignment add/remove without stopping unrelated outputs.
- [ ] Warm/revision/swap one Runtime at a time. A stale process is drained before a warm replacement presents; on hard crash, record the actual visual interruption instead of promising none.
- [ ] Add global admission control for process start/pre-roll so the box cannot initiate work beyond its configured GPU/media budget.
- [ ] Test one Runtime crash/restart does not intentionally terminate others.

### Task 4 — Back Office inventory and six-output acceptance

**Expected owned area:** Player details/output UI, Playwright using the simulated topology seam, hardware workbook/record.

- [ ] Show connector label/PortKey, installer label, assigned Screen, declared vs observed state, expected vs observed panel, and Runtime state without exposing volatile Windows display ordering.
- [ ] Give the installer Identify, accept panel change, leave unassigned, and retire actions with appropriate safety.
- [ ] Add Playwright coverage using six simulated ports: assignment stays tied to port through a reordered enumeration; mismatched panel is visible but content is not silently moved.
- [ ] Execute the hardware test with six concurrent assigned outputs. Record topology, resolutions, media fixture, GPU metrics, frame/drop results, and every limitation. A failed workload becomes a bounded capability rule, not a hidden retry.

---

## M5 — Resilience and operations: health, recovery, offline continuity

**Goal:** operations can distinguish attention from emergency, the box can recover local processes safely, and stale but visible content remains content—not an offline board.

**Dependency:** M4 accepted.  
**Acceptance:** owner workbook driven by deterministic fault injection and a bounded hardware check.  
**Excluded:** generic remote terminal, automatic screenshots/video, coordinated Screen moves, and mass update deployment.

### Task 1 — Durable cloud status, events, actions, and desired state

**Orchestrator-owned files:** next ordered DbUp migration (expected `077_player_operations.sql`), persistence/DI/contract ownership.

- [ ] Add PlayerStatus, PlayerOutputStatus, and PlayerEvent as compact latest state plus meaningful append-only transitions; never write a heartbeat event every interval.
- [ ] Generalize durable PlayerAction only for the allowlist: identify output, refresh inventory, retry recovery, approved update.
- [ ] Add versioned whole-box desired-state snapshots. SignalR only says “reconcile”; duplicate/missed doorbells must be harmless.
- [ ] Test status upsert idempotency, action expiry, action exactly-once effect under retry, snapshot version staleness, and event noise suppression.

### Task 2 — Multi-signal health and bounded recovery

**Expected owned area:** Supervisor health/recovery state machine and tests.

- [ ] Evaluate process liveness, render/frame progress, applied revision, media quality, and output observation together.
- [ ] Implement `Healthy → Suspect → Degraded → Recovering → Healthy|Lost`, with configurable thresholds/backoff and a bounded recovery count.
- [ ] A late heartbeat alone enters Suspect only; it does not replace a potentially visible Runtime.
- [ ] Recover with verified cached content; stop after the bounded policy and report Needs Attention unless no viable content is known.
- [ ] Test each transition and the no-restart-on-one-late-heartbeat rule.

### Task 3 — Truthful Back Office operations view

**Expected owned area:** Player operational UI/API read model/Playwright.

- [ ] Show normal, Attention (cloud offline but last content confirmed), Action Required (newer desired content waiting), and Emergency (lost output/no known viable content) as distinct states.
- [ ] Show current vs desired revision, last render confirmation, observed panel state, retry state, disk/cache headroom, and component versions.
- [ ] Allow only approved action requests, with requester/result history; no free-text remote command.
- [ ] Add empty/loading/refusal/retry/long-output-label coverage and Playwright paths for each severity.

### Task 4 — Fault-injection proof

- [ ] Network loss: current verified content stays visible and the state is Attention, not Emergency.
- [ ] Supervisor exit/restart: pinned Runtime content continues and Supervisor reattaches.
- [ ] Repeated Runtime failure: bounded retries, then Needs Attention; no infinite blank loop.
- [ ] Disconnected known port vs absent Runtime with no viable content: their severity differs correctly.
- [ ] Record physical constraints/untested cases honestly.

---

## M6 — Signed, coordinated component updates

**Goal:** a box receives an approved release target and safely coordinates its own Host/Supervisor/Runtime updates without giving cloud direct process control or taking every display down at once.

**Dependency:** M5 accepted; existing Vennusign release/maintenance procedures identify an approved target; signed artifact-manifest source selected before implementation.  
**Acceptance:** owner workbook plus crash/power-loss simulation.  
**Excluded:** speculative fleet-wide pre-download, new rollout-group product, unsigned developer bypass in production, and Linux update implementation.

### Task 1 — Release target and signed artifact contract

**Orchestrator-owned files:** contract/schema/DI/artifact validation ownership.

- [ ] Define a box release target with independently versioned Host, Supervisor, and Runtime artifacts plus protocol generation.
- [ ] Cloud supplies only the approved desired target. It does not send shell commands or dictate process ordering.
- [ ] Verify manifest signature, artifact signature, hash, compatibility range, and expiry before staging or execution.
- [ ] Reject unknown signer, bad hash, stale target, downgrade outside explicit policy, and incompatible Runtime protocol.
- [ ] Test every refusal before any process is stopped.

### Task 2 — Supervisor update plan and quiescence

**Expected owned area:** Supervisor update planner/store/recovery tests.

- [ ] Compare installed versions to target and persist an idempotent local update plan.
- [ ] At the allowed maintenance window, download and verify required artifacts; do not implement blanket speculative fleet pre-download.
- [ ] Before Supervisor replacement: finish/abort writes safely, wait for Runtime handovers, stop new pre-roll/GPU work, and persist a handover snapshot.
- [ ] If power loss/interruption occurs, boot the last proven healthy version; do not resume an unknown activation automatically.
- [ ] Test retry, cancellation, crash before/after persistence, and old-working-version fallback.

### Task 3 — Host-led Supervisor and serial Runtime replacement

**Expected owned area:** Host updater/bootstrap, Supervisor candidate activation, Runtime side-by-side handover tests.

- [ ] Host, not Supervisor, starts the staged Supervisor candidate in probation; candidate must reopen store, reattach Runtime sessions, and restore health before old process retirement.
- [ ] Update Runtimes serially: warm hidden → verify composition/media/local revision → drain old → acquire surface → retain rollback → next output.
- [ ] Keep output count one-at-a-time even when all Runtimes target the same version.
- [ ] Update Host through the small signed updater/bootstrap path only when its target differs and after its stricter checks.
- [ ] Test each failure point, including one Runtime candidate failing while other outputs remain untouched.

### Task 4 — Operations experience and final acceptance

**Expected owned area:** Back Office update status/action UI, Player action API read/write, Playwright, acceptance workbook.

- [ ] Show installed/current target, staged/verified/activating/rolled-back/update-pending states, action history, and concrete failure reason.
- [ ] “Approve update” creates the durable allowed action; it never invokes an arbitrary command.
- [ ] Demonstrate a mixed-component target and prove safe order: Host if required → Supervisor → Runtime outputs serially.
- [ ] Demonstrate a failed candidate and rollback to a proven healthy version with content retained where the OS permits.
- [ ] Record actual physical behavior, any blank interval, and skipped hardware/security infrastructure checks.

## Follow-on work, deliberately not numbered here

- **Linux Box Player feature:** reuse the approved Player/Output/desired-state model; use DRM connector identity tied to PCI path; choose and certify a pinned kiosk compositor before implementation. It does not start before Windows M6 establishes the reusable contracts.
- **Screen move workflow:** future prepare-destination-before-stop-source behavior; no automatic move logic in this plan.
- **Support capture:** optional, explicit, audited screenshots only; no background camera/video capability.
- **Pre-staging and broader media certification:** future operational/product decisions after measured package sizes, windows, and six-output workload evidence.
