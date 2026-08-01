# Vennu Session Handoff

## Work Package

- ID: RWP-00.01
- Status: In Progress
- Execution mode: Sequential

## Git State

- Branch: `rwp/00.01-affected-area-ci`
- Issue: #335
- Pull request: pending
- CI state: pending affected-area workflow validation

## Completed This Session

- Created approved remediation issue #335 and claimed RWP-00.01 ahead of WP-13.01.
- Added deterministic path classification for documentation, .NET, frontend, display, and TV areas.
- Split CI into affected-area jobs with one stable required gate, dependency caching, and superseded-run cancellation.
- Reserved full non-integration validation for phase closure, nightly/manual runs, workflow changes, and explicit labels.
- Updated sequential and collaborative WP/RWP governance to prohibit full unit and unrelated TV/frontend validation by default.

## Files Changed

- GitHub Actions workflow and CI classification scripts.
- Agent, Copilot, pull-request, project-status, assignment, work-package, and handoff records.

## Decisions

- Preserve `build-and-test` as the stable branch-protection gate.
- Use explicit test-project selection instead of solution-wide unit testing.
- Treat workflow changes as full validation so CI policy changes prove the complete non-integration path.
- Keep documentation-only follow-ups lightweight and prefer completion evidence in the implementation PR.

## Validation

- Commands: classifier scenario tests, assignment JSON parse, workflow YAML parse, and `git diff --check`.
- Results: classifier scenarios, assignment JSON, workflow YAML structure, shell syntax, and diff whitespace checks passed locally; authoritative GitHub Actions validation remains pending.
- Skipped checks and reason: integration and external-system tests remain excluded by standing owner instruction.

## Remaining Work

- Validate, review, and merge RWP-00.01, then release its claim.
- WP-13.01 — Identity, Organization, and Membership Foundation remains next.

## Known Risks or Blockers

- Branch protection must continue to require the stable `build-and-test` check name.
- Path mappings must be updated when new applications or test projects are added.

## Exact Next Action

- Run local classifier and syntax checks, publish the RWP PR, and use its exact-head Actions result for review.

## Do Not Redo or Reverse

- Do not restore solution-wide unit tests or unrelated TV/frontend builds for normal WP/RWP merges.
- Do not start WP-13.01 until this sequential RWP claim is released.
