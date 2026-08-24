# Box Player — Request/Response Interaction Flows

**Status: Proposed — these diagrams explain the agreed design; they do not authorize implementation.**

The architecture overview answers “what runs on a box.” These diagrams answer the equally important “who asks whom, what comes back, and what is allowed to fail without taking content off screen.”

Every future Box Player design change needs both:

1. an overview showing component ownership and trust boundaries; and
2. a request/response sequence for every meaningful cross-process or cloud interaction.

A diagram is incomplete if it shows a request without its response, or a happy path without its refusal/recovery boundary.

## Interaction set

| Flow | What it proves |
|---|---|
| [Claim a Box Player once](windows-linux-multi-output-box-player-flow-claim.svg) | One code claims one box for one venue; outputs are set up after claim; claim failure changes nothing. |
| [Reconcile a desired content change](windows-linux-multi-output-box-player-flow-reconcile.svg) | Cloud wakes one Supervisor; it pulls the whole snapshot, verifies local files, and tells the isolated Runtime about a ready revision. |
| [Report health and recover safely](windows-linux-multi-output-box-player-flow-health.svg) | Render evidence—not a heartbeat alone—drives bounded recovery and truthful cloud status. |
| [Coordinate a signed component update](windows-linux-multi-output-box-player-flow-update.svg) | Cloud names a target; Host replaces Supervisor; Supervisor updates Runtimes serially with rollback. |

## Reading convention

- **Dark solid arrow:** request.
- **Gray dashed arrow:** response.
- **Orange arrow:** local control or lifecycle action.
- **Green arrow:** verified local content.
- The amber guardrail at the bottom of every diagram names the refusal, failure, or continuity rule that its happy path could otherwise hide.

## Required future diagrams

Before a milestone that changes one of these contracts closes, add or revise the matching request/response flow and show the owner its rendered SVG alongside the acceptance workbook/demo. A local agent does not replace a diagram with prose, and it does not invent a new cross-process/cloud message without an owner-approved diagram and contract.

Future flows currently anticipated but not designed:

- coordinated Screen move (prepare destination before stopping source);
- panel-fingerprint acceptance;
- optional audited support capture;
- Linux compositor and local IPC behavior.
