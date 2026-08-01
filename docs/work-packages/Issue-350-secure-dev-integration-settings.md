# Issue-350 — Secure Local Development Integration Settings

## Status

In Review

## Execution Mode

Collaborative

## Issue and Branch

- Issue: #350
- Branch: `issue/350-secure-dev-integration-settings`

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
- Draft PR #352 head `0a3a97f40fa642df91e8b159a4450ed7b8a2c87e` contains no usable credential.
- The narrow `DatabaseMigratorTests` smoke run confirmed database migration execution; its stale inventory assertion is tracked in issue #351.
