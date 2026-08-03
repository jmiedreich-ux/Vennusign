# Issue-392 — Fix Configuration Rotation Migration

## Status

In Review

## Execution Mode

Collaborative

## Evidence

Actual API startup against LocalDB failed in migration 050 with SQL error 207 because SQL Server compiled direct references to the newly added `RotationReminderDays` column before executing the preceding `ALTER TABLE`.

The saved local bootstrap connection string also contained an accidental opening quote and doubled LocalDB separator; those user-environment values were corrected locally and are not committed.

## Scope

- Defer the new-column constraint and data update through `sys.sp_executesql` after the column-add statement.
- Add focused embedded-migration regression coverage.
- Verify actual API startup, LocalDB migration execution, and authenticated configuration endpoint behavior.

## Validation

- `DatabaseMigratorTests`: 3/3 passed.
- Debug API build passed.
- Actual LocalDB migration 050 completed successfully.
- API listened on port 5192.
- Authenticated configuration endpoint returned HTTP 200 with 57 registered definitions.
- The supplemental startup process was stopped after validation.
- GitHub Actions pending.
