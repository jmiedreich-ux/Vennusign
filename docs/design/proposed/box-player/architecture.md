# Windows/Linux multi-output box Player

**Status: Proposed — agreed architecture to date; not implementation approval.**

This document records the decisions made for a Windows/Linux box that drives multiple physical display outputs. It is a durable design reference for future feature planning, backend work, and player implementation. It does not authorize code changes.

- **Architecture overview:** [control, content, and physical outputs](overview.svg)
- **Request/response flow set:** [claim/setup, replacement, reconciliation, health/recovery, and updates](interaction-flows.md)
- **Proposed implementation plan:** [task-level Windows-first milestones](milestone-plan.md)

## Purpose

Existing Vennusign TV shells and direct-URL displays are effectively one process, one output, and one Screen pairing. A Windows or Linux box can drive several physical outputs and therefore needs a box-level control plane without losing Vennusign's primary reliability goal:

> Keep the last known good content visible for as long as the operating system and display hardware permit.

A Screen remains the logical destination for customer content. A real box is a Player; each durable physical connector is a Player Output.

## Vocabulary

- **Screen** — logical content destination, such as “Lobby — Left Menu.” Menus, playlists, broadcasts, and WallPosition belong here.
- **Player** — device/runtime identity. A Box Player is a real Windows/Linux machine. TV and direct-URL players use an implicit Player behind the existing Screen-first experience.
- **Player Output** — a declared physical connector/slot on a Player. It may be assigned to one Screen or intentionally unassigned.
- **Player Host** — small, stable local lifecycle owner. It starts/watches the Supervisor, hosts the read-only Local Content Gateway, and performs process handovers.
- **Player Supervisor** — one per box. It owns the cloud connection, cache/content reconciliation, output inventory, local health, update planning, and cloud reporting.
- **Display Runtime** — isolated process per assigned output. It owns fullscreen display, composition, media decode/playback, and local render health.

~~~text
Vennusign cloud
       │  one authenticated connection per box
       ▼
Player Supervisor ─────────── Supervisor Content Store
       │                                   │
       │ local authenticated control        │ read-only local content
       ▼                                   ▼
Display Runtime (HDMI-1)       Display Runtime (HDMI-2) ...
       │                                   │
   physical output                       physical output
~~~

## Core decisions

### One box is claimed once

A box is claimed when an authorized venue administrator enters the one-time code shown by that physical box in Back Office.

Claiming:

- associates the Player with exactly one venue;
- grants one long-lived, revocable box credential;
- does **not** assign Screens or prove a monitor is connected;
- replaces one pairing code per output with one player-level claim.

The Supervisor is the only local component that holds and uses the box credential. Player Host has no ordinary cloud authority. Display Runtimes never receive a cloud credential.

### Screen, Player, and output are distinct

The authoritative physical assignment is:

~~~text
Player → Player Output → optional Screen
~~~

PlayerOutput.ScreenId is optional and unique. A Screen may exist before it is assigned to a Player Output; an output may exist before it is assigned to a Screen. A Screen cannot accidentally be assigned to two outputs.

WallPosition remains a content/layout concept on Screen. It is never a physical port number.

TV and direct-URL pairing remain Screen-first in the product UI. On pairing, Vennusign quietly creates an **implicit Player** with a single implicit primary output assigned to that Screen. Real Box Players and their outputs are first-class operational objects in Back Office.

For current Screen replacement, the **logical Screen is preserved**. The replacement device's implicit primary output is assigned to that existing Screen; the removed device's former implicit output is retained as unassigned/retired history. Content, configuration, and Screen history stay with the logical Screen—replacement never creates a second logical destination.

### A physical port is the identity

A Player Output is keyed by its connector/port identity, not monitor ordering and not EDID alone.

- Never use Windows DISPLAY1/DISPLAY2, Electron/Chromium display IDs, or enumeration position as identity.
- Windows uses connector-scoped display topology information, including DisplayTarget.AdapterRelativeId, discovered through QueryDisplayConfig.
- Linux uses DRM connector identity under /sys/class/drm, tied to the PCI device path rather than a volatile card0 index.
- EDID is corroborating evidence only. Duplicate or blank serials are common on identical panels.

An output keeps an accepted **expected panel fingerprint** and a changing **observed panel fingerprint**. A mismatch raises “panel/cable may have changed”; it never silently remaps content. A user must explicitly accept the new fingerprint, preserving history.

Slots are declared inventory:

- a sleeping/unplugged TV marks its known output disconnected;
- an output is never deleted because it disappears temporarily;
- a newly observed physical port is shown as “New output detected,” unassigned, and requires confirmation before declaration;
- retiring a port is explicit and historical;
- an HDMI splitter remains one source connector/output even if it drives several panels.

## Reliability model

### Display Runtime isolation

Each assigned output runs in its own Display Runtime. The Supervisor can restart or update one without intentionally affecting other outputs.

Supervisor-to-runtime control uses authenticated local IPC:

- Windows: authenticated named pipe;
- Linux: authenticated Unix-domain socket.

The runtime receives only output-scoped commands and short-lived local content permissions. It cannot administer the box, change assignments, access cloud, or access another output's content.

The Supervisor and runtime exchange messages such as startup/identity, assigned Screen and local revision available, heartbeat and rendering progress, revision applied, fault, drain, and surface released.

A runtime is not restarted merely because a heartbeat is late. Health uses multiple signals: process liveness, render/frame progress, currently applied revision, media quality, and observed output state.

### Content remains visible through control-plane failures

The Supervisor Content Store is box-local:

- SQLite holds metadata, journal, assignments, current pointers, validity, and queued receipts;
- immutable files and a SHA-256 content-addressed blob store hold packages/assets;
- the Supervisor is the only writer;
- runtimes are read-only consumers;
- retain three complete verified revisions per Screen: current plus two fallbacks, subject to a box disk budget.

A revision is transactional: all data, assets, fonts, images, and media must download and validate before an atomic promotion. On failure, the prior complete revision remains active.

The Player Host provides a read-only **Local Content Gateway** for committed files. An applied runtime revision is fully pinned and self-contained; a visible Runtime must not need a live Supervisor to fetch its next current-revision asset. This lets content continue across a Supervisor restart.

On a cloud outage, the last successfully applied content remains visible indefinitely. The product reports freshness and status later; it does not replace the board with a generic “offline” screen.

On box boot, Player Host and Supervisor use the local store first. Assigned runtimes launch on their saved ports and show cached content before cloud networking is available.

### Recovery behavior

A runtime progresses through operational states:

~~~text
Healthy → Suspect → Degraded → Recovering → Healthy
                                  └───────→ Lost
~~~

- **Suspect**: one late signal; no visible intervention.
- **Degraded**: repeated evidence; report attention and prepare recovery.
- **Recovering**: warm a replacement from verified local content, then drain/swap only when it is ready.
- **Lost**: no viable runtime or known visible content remains after bounded recovery; emergency.

Automatic recovery has a bounded retry count and backoff. It does not endlessly blank/restart a possibly still-visible output. Repeated failure becomes **Needs Attention** unless there is no reason to believe content remains visible, which is **Emergency**.

The ability to remove a stale display surface cleanly depends on the operating system compositor. A hard process crash can create a small unavoidable blank interval before a replacement can own the OS surface. A permanent native surface host that eliminates this is a future, more complex option.

## Media and GPU policy

Display Runtime owns rendering, hardware video decode, presentation, and playback. The Supervisor never transcodes while content is playing.

The cloud media pipeline produces suitable renditions. The Supervisor:

- downloads and verifies assets before use;
- selects a compatible rendition using the box's actual media capability profile;
- keeps playback local to disk;
- manages a shared GPU/media budget across outputs;
- limits simultaneous heavy decode, pre-roll, and updates;
- reports dropped frames, decode fallback, and scheduling pressure.

Boxes are certified against explicit workloads (for example, static 4K outputs or a defined number of 1080p H.264 motion outputs), not marketing claims.

## Update architecture

### Component roles

A box contains three independently versioned parts:

1. Player Host;
2. Player Supervisor;
3. Display Runtime.

The existing Vennusign update/cutover procedures decide that a Player has a newer approved target. Cloud sends a lightweight availability/reconcile notification. The Supervisor determines locally which components differ, creates a durable update plan, downloads/verifies the required artifacts from an approved source, and applies them in the safe order.

Cloud does not need to command the internal sequence.

### Safe order and continuity

The normal order is:

~~~text
Player Host, if required → Player Supervisor → Display Runtimes, one output at a time
~~~

A component is skipped when its target version is already installed. New Supervisor/Runtime releases must support the current and immediately previous local protocol generation so a serial Runtime rollout is safe.

Supervisor updates use side-by-side staged versions. Player Host stops the old Supervisor, starts the candidate in probation, and rolls back if it cannot re-open the store, reattach runtimes, and restore its cloud/control health. Display Runtimes remain displaying their pinned content through this handover.

Display Runtime updates are also side-by-side and serial per output: warm hidden, verify composition/media/local content, drain the old runtime, acquire the surface, retain rollback, then advance to the next output.

Before asking Player Host to replace it, the Supervisor becomes **quiescent**:

- finish or safely abort downloads, promotion, cache pruning, and topology work;
- wait for an in-progress Display Runtime handover;
- stop scheduling new pre-roll/GPU work;
- persist a safe handover snapshot.

Updates follow the existing venue maintenance/cutover rules. The initial policy is no speculative fleet-wide pre-download: download, verify, and activate in the allowed maintenance window. An unfinished update stays on its proven version, reports **Update Pending**, and retries later. Controlled staging can be introduced later if package size or maintenance windows justify the added operational state.

If power is lost during an update, the box starts the last proven healthy version on boot. It never automatically resumes an unknown half-finished activation. The interrupted attempt is reported and retried later.

Player Host itself is updated through a small signed Player Updater/bootstrap path. It follows the same deployment policy, but remains rare and uses stricter handover checks.

All production software artifacts and manifests are signed by Vennusign. A box verifies signatures and expected hashes before executing or activating a package. There is no hidden production bypass.

## Cloud contract

SignalR is a doorbell, not the source of truth. A missed or duplicated event is harmless.

The Supervisor pulls a versioned, whole-box **desired-state snapshot**. It contains:

- approved software target;
- declared outputs;
- Screen assignments;
- accepted panel expectations;
- pending durable remote actions;
- for each assigned Screen, the desired content revision/pointer.

It does not include raw content files. The Supervisor uses the existing content/delivery path to fetch the specified revision.

Deliberate remote operations—identify an output, request inventory, retry recovery, or begin an approved update—are durable, numbered actions with expiry, idempotency, and recorded outcome. They are not raw SignalR payloads.

## Backend model

Stable configuration records:

| Record | Responsibility |
|---|---|
| Player | ownership, claim state, platform, type (Box or Implicit), friendly name, credential identifier, timestamps |
| PlayerOutput | Player, permanent port key, optional installer label, optional Screen assignment, accepted panel fingerprint, declared/retired timestamps |
| PlayerOutputPanelHistory | accepted panel changes and audit history |
| PlayerAction | durable requested operation and its outcome |

Latest operational state is separate from configuration:

| Record | Responsibility |
|---|---|
| PlayerStatus | last cloud contact, installed Host/Supervisor/Runtime versions, cache/disk and media capacity, overall state |
| PlayerOutputStatus | connected state, observed panel fingerprint/resolution, runtime state, applied content revision, last rendered confirmation, alert reason |
| PlayerEvent | meaningful state transitions, recovery/update failures, connector/fingerprint changes; not every heartbeat |

The database stores a credential identifier and revocation state, never the device secret itself.

## Safe migration from the current Screen-only model

The first database change is additive and data-preserving:

1. Add Player, Player Output, status/event/action, and panel-history tables through ordered DbUp migrations.
2. Backfill every existing Screen with one implicit Player and one implicit primary Player Output assigned to that Screen.
3. Use a stable legacy port key such as legacy-screen:{ScreenId}. It represents the historic one-Screen player relationship; it does not assert nonexistent physical hardware.
4. Preserve Screen IDs, existing pairing URLs/codes, menus, video-wall configuration, delivery receipts, statuses, pre-registration, and archive/history records.
5. Keep existing Screen-based player endpoints working during the compatibility release; new code resolves the implicit Player/Output behind them.
6. Make new TV/direct-URL pairing create Screen, implicit Player, and implicit Output transactionally.
7. Make the existing Screen replacement flow a single physical-device reassignment transaction: preserve the target logical Screen, assign the incoming device's implicit primary output to it, and leave the outgoing output unassigned/retired historically. Do not copy configuration into a new logical Screen or make the customer re-pair.
8. Remove or relocate duplicated device fields from Screen only in a later, separately planned cleanup migration.

No existing display should have to re-pair.

## Platform constraints

- Windows rendering must run in an interactive kiosk session. Session 0 services have no display desktop. Use autologon/Assigned Access or equivalent supported kiosk configuration; Windows LTSC/IoT Enterprise is preferred where update control matters.
- Windows topology enumeration uses QueryDisplayConfig; the deployment must debounce topology changes rather than trust boot-time ordering.
- Linux DRM connector paths and EDID observations are the inventory source. Avoid force-probing displays in routine health loops.
- X11 can place clients through XRandR. General GNOME-on-Wayland cannot guarantee arbitrary output placement; supported Linux deployments need a pinned compositor/kiosk approach such as Weston kiosk-shell, GNOME Kiosk where its monitor placement is suitable, or cage-per-output.
- The installer/Back Office needs an **Identify Outputs** action that shows a large temporary label on a physical output so a person can confirm the real-world location. This directly addresses unstable Windows display ordering and remote video-extender chains.

## Current operational severity

These are intentionally separate from raw connectivity:

| Condition | Operational meaning |
|---|---|
| Online and current | Normal |
| Cloud offline, last applied content still confirmed | Attention |
| Cloud offline with newer desired content waiting | Action required / potentially stale |
| Output/runtime lost with no known viable content | Emergency |

## Deferred or future work

- Coordinated Screen moves between outputs. The current data model supports this. A future move workflow should prepare the destination before stopping the source, allowing brief intentional overlap rather than an unexpected blank.
- Optional, explicit, audited support screenshots. No automatic screenshot or video capture.
- Controlled pre-staging of large update packages.
- Permanent native surface host for a stronger zero-blank handover guarantee.
- Exact artifact storage/CDN choice.
- Exact Windows/Linux packaging, kiosk/compositor, and low-level display implementation choices.
- Back Office visual design and detailed role/permission matrix.
- Detailed IPC schemas, fault thresholds, and test plan.

## Design completion map

The decisions required to plan implementation are settled and already mapped to the task register:

1. **DbUp model and safe migration** — additive Player/PlayerOutput foundation, legacy backfill, atomic pairing claim, and physical-device replacement are recorded in [M1](milestone-plan.md#M1).
2. **Cloud and local contracts** — one box-level claim, desired-state snapshots, durable actions, credentials, inventory, status, and updates are defined in the relevant [M2](milestone-plan.md#M2), [M5](milestone-plan.md#M5), and [M6](milestone-plan.md#M6) tasks.
3. **Back Office operation** — box claim/setup, declared outputs, inventory, severity, and update operations are covered by [M2](milestone-plan.md#M2), [M4](milestone-plan.md#M4), [M5](milestone-plan.md#M5), and [M6](milestone-plan.md#M6).
4. **Windows-first runtime and platform boundary** — interactive kiosk-session launch, local store/gateway, one Display Runtime per output, Windows port identity, and six-output acceptance are covered by [M3](milestone-plan.md#M3) and [M4](milestone-plan.md#M4). Linux implementation remains deliberately later.
5. **IPC, failure, and continuity rules** — authenticated local IPC, warm/drain swap, multi-signal health, bounded recovery, and exact evidence/acceptance are covered by [M3](milestone-plan.md#M3), [M5](milestone-plan.md#M5), and the request/response [interaction flows](interaction-flows.md).

The exact code artifacts for those settled decisions—SQL migration text, request DTOs, process APIs, and test fixtures—are implementation deliverables inside their numbered tasks, not a reason to reopen the architecture.
