# Issue-350 — Secure Local Development Integration Settings

## Status

Complete

## Execution Mode

Collaborative

## Issue and Branch

- Issue: #350
- Branch: `issue/350-secure-dev-integration-settings`

## Completion Evidence

- PR #352 merged at `ee9e35768b0d6f82ec9acd3cb0156585a5624d3a` after GitHub Actions passed on reviewed head `c6ee15c051cb28cf2a2f442c10b2b311d280fe9f`.
- ChatGPT approval is recorded on PR #352 for the reviewed head.
- The local settings file is ignored and absent from the Git index; the user-level environment variable was verified without printing its value.
- The narrow migration integration smoke run confirmed database migration execution. The stale migration inventory assertion is issue #351.

## Scope

- Move the existing local integration-test connection configuration to the user environment variable without logging its value.
- Stop tracking and ignore local `app.settings.json` files.
- Provide a credential-free example configuration.
- Record the credential rotation and Git-history remediation follow-up.

## Out of Scope

- Changing application behavior, test behavior, or database schema.
- Running integration tests.
- Rotating Azure SQL credentials or rewriting Git history from this work item.

## Acceptance Criteria

- Git operations cannot overwrite a developer's local integration-test settings file.
- No usable database credential is tracked in the repository head.
- The local configuration format is available as a safe example.
- The environment-variable-first configuration path is preserved.

## Validation

- Verify the user-level `VENU_TEST_AZURE_SQL_CONNECTION_STRING` is present without printing it.
- Verify `tests/Vennu.Data.IntegrationTests/app.settings.json` is ignored and absent from the Git index.
- Run `git diff --check`.
- Draft PR #352 contains no usable credential; GitHub Actions must validate its latest head before approval.
- The narrow `DatabaseMigratorTests` smoke run confirmed database migration execution; its stale inventory assertion is tracked in issue #351.
