# Documentation Guide

Use documentation by purpose. Start with the [VennuSign Engineering Architecture Bible](architecture/VENNUSIGN_ENGINEERING_ARCHITECTURE_BIBLE.md) for the cross-system map, authority rules, terminology, and links to the owning records. Do not load the repository's Markdown collection wholesale.

## Routine Startup

Read only the files listed in `AGENTS.md`: current policy, handoff, tracker, status, claimed package, and live GitHub state.

## Task-Scoped Reading

- `docs/work-packages/`: only unfinished or currently relevant approved work records.
- `docs/architecture/`: current cross-system design decisions; start with the [architecture bible](architecture/VENNUSIGN_ENGINEERING_ARCHITECTURE_BIBLE.md), then read only affected domains.
- `docs/operations/`: operational procedures; read only for affected services.
- component README files under `src/`: read only when changing that component.
- `AI_DEVELOPMENT_GUIDE.md`: concise implementation guidance.
- `docs/features/<feature>/`: feature authority, decisions, question registers, milestones, and acceptance records.

## Proposed Design

Proposed design records are design inputs, not implementation authority. A feature becomes implementation-authoritative only after its settled design and decisions are landed in the relevant `docs/features/<feature>/` directory.

- **Windows/Linux multi-output Box Player** — [architecture](design/proposed/box-player/architecture.md), [visual system flow](design/proposed/box-player/overview.svg), [request/response interaction flow set](design/proposed/box-player/interaction-flows.md), and [task-level Windows-first milestone plan](design/proposed/box-player/milestone-plan.md). The entire proposal remains under `docs/design/proposed/box-player/` until approval.
- **Maestro dev-lead agent framework** — [proposal](design/proposed/maestro-dev-lead-agent-framework.md). It is separate from the product runtime and remains proposed.

## Creation and Updates

Update an existing living document before creating new Markdown. Batch documentation updates at publish checkpoints; do not create files per local branch, issue, experiment, test run, or intermediate handoff. New package, architecture, decision, or operations documents require a durable purpose that no current document can serve. Archive snapshots are reserved for major milestones, durable audit history, or explicit owner request.

When a durable cross-system decision changes, update the architecture bible and its owning source record in the same change set. The bible is not a work log and should not duplicate detailed schemas, migration text, UI specifications, or acceptance steps.

## Deliberate Research Only

- `docs/archive/work-packages/`: completed WP/RWP and maintenance records.
- `docs/archive/phase-plans/`: completed phase plans.
- `docs/archive/validation/`: historical validation and closure records.
- `docs/archive/research/`: superseded roadmaps, status snapshots, and legacy agent guidance.
- `ai/handoffs/archive/`: immutable session history.

Consult archived material only when an issue or PR links it, a migration/compatibility decision requires history, or the user explicitly requests research. Archived statements do not override current code, `AGENTS.md`, tracker, status, handoff, or live GitHub state.
