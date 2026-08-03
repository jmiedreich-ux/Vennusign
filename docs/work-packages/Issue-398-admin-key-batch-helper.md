# Issue-398 — Super Admin Access Key Batch Helper

## Status

In Review

## Execution Mode

Collaborative

## Scope

- Add a root-level Windows batch helper.
- Copy the existing current-user `SuperAdmin__ApiKey` to the clipboard without displaying it.
- Generate and persist a random 256-bit key only when no current-user key exists.
- Preserve the existing PowerShell helper's explicit rotation behavior when run without `-ReuseExisting`.

## Validation

- PowerShell parser validation passed.
- Root batch execution passed.
- Existing user key remained unchanged.
- Clipboard matched the stored key without printing it.
- Development Control Windows tests passed 9/9, including root batch wiring and no direct environment-variable echo.
- GitHub Actions pending.
