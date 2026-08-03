# GitHub Copilot Instructions

Follow the repository-root `AGENTS.md` as the authoritative rulebook before proposing or changing code.

## Required startup

1. Read `AGENTS.md`, the current handoff, project status, assignment tracker, and the approved WP/RWP.
2. Select the execution mode explicitly. Visual Studio and VS Code interactive work defaults to `Desktop Collaborative`; chat/mobile collaboration remains `Mobile Collaborative` and is unchanged.
3. In Desktop Collaborative mode, confirm sequential schedules are paused and a visible desktop-session lock exists before editing. Read GitHub state once at session start and again only at publish checkpoints or when ownership/drift may have changed.
4. Work only inside approved session scope. Desktop logical branches may combine coherently related issues and merge locally into the session integration branch.
5. Use `docs/README.md` for task-scoped routing; never load `docs/archive/` or `ai/handoffs/archive/` routinely.

## Coordination

- Testing and review findings are GitHub issues first. Do not create or reprioritize WPs/RWPs.
- Sequential and Mobile Collaborative changes require their own approved issue, branch, and PR. Desktop Collaborative work uses one session lock, one local session integration branch, local logical branches, and checkpoint PRs.
- Every active claim or lock must identify its execution mode. Never edit Sequential work from a collaborative session.
- Only the designated planning agent may promote a finding into an RWP or change queue order.
- Do not edit files owned by another active agent.
- Mobile Collaborative keeps one branch and PR per WP/RWP. Desktop Collaborative may close multiple coherently related issues in one checkpoint PR while preserving issue traceability.
- Do not create Markdown for local branches, experiments, routine test results, or intermediate handoffs. Update existing living documents once per publish checkpoint; create a new Markdown file only for an approved package or a durable architecture/operations need that has no suitable existing home.
- Do not merge or declare completion until required affected-area non-integration GitHub Actions checks pass and ChatGPT approval is recorded; after those gates pass, the active agent may perform the merge.
- For every normal WP/RWP, build only affected areas and run only explicitly affected unit-test projects. Do not run the full unit-test suite or unrelated TV/frontend checks.
- Include completion evidence in the implementation PR when practical. Documentation-only completion follow-ups use lightweight validation.
- Phase-closure work must run the broader full non-integration suite before approval.
- Consult the available UX best-practices MCP and complete a documented UI/function gap analysis before implementing a new or changed UI page or screen; resolve or explicitly defer required action, state, navigation, accessibility, API, and authorization gaps.
- Integration-type tests remain skipped under the repository-owner exception in `AGENTS.md`.
- Never commit secrets, credentials, generated runtime output, or unrelated changes.
