## Work package

- ID: <!-- WP-xx.xx or RWP-xx.xx -->
- Linked issue: <!-- Closes #123 -->
- Type: <!-- WP / RWP / documentation / emergency correction -->
- Queue position approved by: <!-- planning issue or decision -->

## What changed

<!-- Plain-language summary of the bounded change. -->

## Why

<!-- Requirement, gap evidence, or root cause. For RWP work, explain the originating phase and reconciliation impact. -->

## Scope

- In scope:
- Out of scope:
- Other active branches or agents checked:

## Acceptance criteria

- [ ] Linked issue criteria are satisfied.
- [ ] Architecture and application boundaries remain correct.
- [ ] No unrelated refactoring or generated runtime output is included.
- [ ] Tracking, active package, project status, and handoff records are synchronized.

## Validation

- [ ] Required non-integration GitHub Actions checks passed on this exact head.
- [ ] Only affected unit/static/build/migration checks ran for a normal WP/RWP.
- [ ] Unrelated frontend, TV, and .NET unit-test areas were skipped.
- [ ] Phase closure or explicitly approved full validation ran the complete non-integration suite when applicable.
- [ ] Integration-type tests were skipped under the standing owner exception.
- Validation scope: <!-- docs-only / affected areas / full -->
- Affected areas:
- Head commit:
- Workflow run(s):
- Intentionally non-applicable checks:

## Review and completion

- [ ] PR is ready for ChatGPT review.
- [ ] All blocking review comments are resolved.
- [ ] `CHATGPT APPROVED` is recorded against the current head.
- [ ] Completion evidence and exact next action are recorded.

## Risks and follow-up

<!-- Residual risks, migrations, dependent reconciliation work, or "None". -->
