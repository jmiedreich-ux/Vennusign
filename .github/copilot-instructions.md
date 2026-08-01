# GitHub Copilot Instructions

Follow the repository-root `AGENTS.md` as the authoritative rulebook before proposing or changing code.

## Required startup

1. Read `AGENTS.md`, the current handoff, project status, assignment tracker, and the approved WP/RWP.
2. Check the linked issue, active branches, open pull requests, and current queue.
3. Work only on one claimed, approved issue and stay inside its documented scope.

## Coordination

- Testing and review findings are GitHub issues first. Do not create or reprioritize WPs/RWPs.
- Every change, including documentation-only work and local-only development configuration work, still requires its own approved issue, branch, and pull request.
- Only the designated planning agent may promote a finding into an RWP or change queue order.
- Do not edit files owned by another active agent.
- Use one branch and PR per WP/RWP.
- Treat `Sequential` and `Collaborative` as execution modes only; they do not replace the underlying WP/RWP workflow.
- Do not merge or declare completion until required non-integration GitHub Actions checks pass and ChatGPT approval is recorded.
- Integration-type tests remain skipped under the repository-owner exception in `AGENTS.md`.
- Never commit secrets, credentials, generated runtime output, or unrelated changes.
