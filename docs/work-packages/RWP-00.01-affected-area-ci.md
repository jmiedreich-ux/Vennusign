# RWP-00.01 — Affected-Area CI and WP/RWP Validation Optimization

## Status

In Progress — claimed under GitHub issue #335.

## Purpose

Reduce duplicate and unrelated GitHub Actions work while preserving an authoritative required check for every WP and RWP pull request.

## Scope

- Classify changes into documentation, .NET API/data, Admin, Venue Admin, Display, and individual TV package areas.
- Run only the builds and unit-test projects affected by a normal WP/RWP.
- Use lightweight repository-record validation for documentation and completion-evidence-only changes.
- Run the complete non-integration suite only for phase closure, an explicit `full-validation` label/manual dispatch, workflow changes, or nightly.
- Preserve the standing exclusion for integration and external-system tests.
- Apply the same validation policy in sequential and collaborative modes.
- Keep one stable `build-and-test` gate so branch protection remains consistent.

## Out of Scope

- Application behavior, Phase 13 implementation, integration testing, live-provider testing, or branch-protection administration.

## Acceptance Criteria

1. Pull-request CI detects documentation-only, affected implementation areas, and full-validation work.
2. Normal WP/RWP changes do not build unrelated frontends, TV packages, or .NET unit-test projects.
3. The full unit-test suite is not run for a normal WP/RWP.
4. Phase-closing packages, nightly/manual runs, workflow changes, and explicitly labeled runs execute the full non-integration suite.
5. Completion evidence is included in the implementation PR where practical; a documentation-only follow-up does not build applications.
6. Superseded runs are cancelled and dependency caches are reused.
7. Classification scenarios cover docs-only, API, frontend, TV, closure, and workflow changes.
8. Agent and PR guidance applies the policy equally to sequential and collaborative WP/RWP work.

## Validation Evidence

Pending GitHub Actions validation and ChatGPT review on the exact PR head.
