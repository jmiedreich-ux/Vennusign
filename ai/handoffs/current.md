# Vennusign Session Handoff

## Current State

- Item: RWP-00.14 — Project-Local Impeccable Codex Design Skill / issue #486
- Mode: Sequential
- Branch: `rwp/00.14-impeccable-codex-skill`
- Status: Official v4.0.4 project-local skill and advisory hook installed; pending exact-head Actions, review, and merge

## Approved Queue

The approved 18-item product remediation queue is complete. RWP-00.14 / issue #486 is a bounded maintenance package and does not authorize product work. RWP-13.06 / issue #466 is held and excluded. Phase 14+ remains paused.

## RWP-00.14 Proposed Outcome

- Codex discovers the complete official Impeccable v4.0.4 skill from `.agents/skills/impeccable`.
- `.codex/hooks.json` runs the project-local advisory detector after UI edits and at stop on Node 22 or newer, with POSIX and Windows paths.
- `AGENTS.md` requires the skill's routed, bounded UX workflow for every changed UI surface.

## Boundaries

- No product UI, API, server persistence, authorization, entitlement, schema, or data-contract changes.
- `PRODUCT.md` and `DESIGN.md` are not invented during installation; a future explicit Impeccable `init` or `document` request owns those artifacts.
- No later product package is approved.
- Do not claim or implement held RWP-13.06 / issue #466 or resume Phase 14+.
- Azure SQL, live Stripe, hosted browser, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

Publish the RWP-00.14 implementation PR, require affected tooling/documentation GitHub Actions on the exact reviewed head, review and merge it, close issue #486, verify `master`, and release the claim. Then stop with the approved product queue complete.
