# GitHub Copilot Instructions

Follow the repository-root `AGENTS.md` as the authoritative rulebook before proposing or changing code.

## Required startup

1. Read `AGENTS.md`, the current handoff, project status, assignment tracker, and the approved WP/RWP.
2. Check the linked issue, active branches, open pull requests, and current queue.
3. Work only on one claimed, approved issue and stay inside its documented scope.

## Coordination

- Testing and review findings are GitHub issues first. Do not create or reprioritize WPs/RWPs.
- Every change, including documentation-only work and local-only development configuration work, still requires its own approved issue, branch, and pull request.
- Every active work item must be claimed in `tracker/assignments.json` before changes begin.
- Every tracker claim must identify whether the work is `Sequential` or `Collaborative`; do not edit work claimed as `Sequential` from a collaborative session.
- Only the designated planning agent may promote a finding into an RWP or change queue order.
- Do not edit files owned by another active agent.
- Use one branch and PR per WP/RWP.
- Treat `Sequential` and `Collaborative` as execution modes only; they do not replace the underlying WP/RWP workflow.
- Do not merge or declare completion until required affected-area non-integration GitHub Actions checks pass and ChatGPT approval is recorded; after those gates pass, the active agent may perform the merge.
- For every normal WP/RWP, build only affected areas and run only explicitly affected unit-test projects. Do not run the full unit-test suite or unrelated TV/frontend checks.
- Include completion evidence in the implementation PR when practical. Documentation-only completion follow-ups use lightweight validation.
- Phase-closure work must run the broader full non-integration suite before approval.
- Consult the available UX best-practices MCP and complete a documented UI/function gap analysis before implementing a new or changed UI page or screen; resolve or explicitly defer required action, state, navigation, accessibility, API, and authorization gaps.
- Integration-type tests remain skipped under the repository-owner exception in `AGENTS.md`.
- Never commit secrets, credentials, generated runtime output, or unrelated changes.
