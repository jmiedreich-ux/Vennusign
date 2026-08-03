# Vennu Session Handoff

## Current State

- Item: DOC-MAINT-001 / issue #410
- Mode: Collaborative
- Branch: `issue/410-agent-doc-routing`
- Status: Complete in the proposed merge state

## Goal

Reduce routine agent Markdown context while preserving historical material for deliberate research.

## Result

- Routine startup guidance is current and reduced.
- Completed records and superseded guidance are preserved under `docs/archive/`.
- `docs/work-packages/` contains only the unfinished `INT-TESTING-001` record and its routing README.
- No application behavior or future-phase plan changed.

## Validation

- Markdown inventory, active-path routing, stale-phase scan, JSON parsing, and repository diff checks pass locally.
- Documentation-only GitHub Actions validation is required on the exact PR head.
- Integration and application tests are not applicable.

## Exact Next Action

Validate the exact PR head, complete ChatGPT review, merge issue #410, and retain Phase 14+ as paused.

## Do Not Redo

Do not move `INT-TESTING-001` into the archive while it remains unfinished, and do not treat archived statements as current authority.
