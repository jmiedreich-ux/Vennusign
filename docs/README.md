# Documentation Guide

Use documentation by purpose; do not load the repository's Markdown collection wholesale.

## Routine Startup

Read only the files listed in `AGENTS.md`: current policy, handoff, tracker, status, claimed package, and live GitHub state.

## Task-Scoped Reading

- `docs/work-packages/`: only unfinished or currently relevant approved work records.
- `docs/architecture/`: current design decisions; read only for affected domains.
- `docs/operations/`: operational procedures; read only for affected services.
- component README files under `src/`: read only when changing that component.
- `AI_DEVELOPMENT_GUIDE.md`: concise architecture and implementation guidance.

## Proposed Design

Proposed design records are design inputs, not implementation authority. A feature begins only after its owner promotes the settled record into `docs/design/approved/<feature>/`.

- **Windows/Linux multi-output Box Player** — [architecture](design/proposed/windows-linux-multi-output-box-player.md), [visual system flow](design/proposed/windows-linux-multi-output-box-player-flow.svg), and [task-level Windows-first milestone plan](design/proposed/windows-linux-multi-output-box-player-milestone-plan.md). The plan is deliberately kept with the proposed design until approval; it does not create an active feature workstream.

## Creation and Updates

Update existing living documents before creating new Markdown. Desktop Collaborative sessions batch documentation at publish checkpoints and do not create files per local branch, issue, experiment, test run, or intermediate handoff. New package, architecture, decision, or operations documents require a durable purpose that no current document can serve. Archive snapshots are reserved for major milestones, durable audit history, or explicit owner request.

## Deliberate Research Only

- `docs/archive/work-packages/`: completed WP/RWP and maintenance records.
- `docs/archive/phase-plans/`: completed phase plans.
- `docs/archive/validation/`: historical validation and closure records.
- `docs/archive/research/`: superseded roadmaps, status snapshots, and legacy agent guidance.
- `ai/handoffs/archive/`: immutable session history.

Consult archived material only when an issue or PR links it, a migration/compatibility decision requires history, or the user explicitly requests research. Archived statements do not override current code, `AGENTS.md`, tracker, status, handoff, or live GitHub state.
