# Documentation Guide

Use documentation by purpose; do not load the repository's Markdown collection wholesale.

## Routine Startup

Read only the files listed in `AGENTS.md`: current policy, handoff, tracker, status, claimed package, and live GitHub state.

## Task-Scoped Reading

- `docs/MILESTONE_EXECUTION.md`: the ordered steps for working a milestone. Read at the start
  of any milestone, alongside `AGENTS.md`.
- `docs/work-packages/`: only unfinished or currently relevant approved work records.
- `docs/architecture/`: current design decisions; read only for affected domains.
- `docs/operations/`: operational procedures; read only for affected services.
- component README files under `src/`: read only when changing that component.
- `AI_DEVELOPMENT_GUIDE.md`: concise architecture and implementation guidance.

## Creation and Updates

Update existing living documents before creating new Markdown. Desktop Collaborative sessions batch documentation at publish checkpoints and do not create files per local branch, issue, experiment, test run, or intermediate handoff. New package, architecture, decision, or operations documents require a durable purpose that no current document can serve. Archive snapshots are reserved for major milestones, durable audit history, or explicit owner request.

## Deliberate Research Only

- `docs/archive/work-packages/`: completed WP/RWP and maintenance records.
- `docs/archive/phase-plans/`: completed phase plans.
- `docs/archive/validation/`: historical validation and closure records.
- `docs/archive/research/`: superseded roadmaps, status snapshots, and legacy agent guidance.
- `ai/handoffs/archive/`: immutable session history.

Consult archived material only when an issue or PR links it, a migration/compatibility decision requires history, or the user explicitly requests research. Archived statements do not override current code, `AGENTS.md`, tracker, status, handoff, or live GitHub state.
