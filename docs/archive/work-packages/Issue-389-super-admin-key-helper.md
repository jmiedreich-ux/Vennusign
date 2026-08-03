# Issue-389 — Super Admin Key Helper

## Status

Complete through PR #390.

## Execution Mode

Collaborative

## Scope

- Generate a cryptographically random 256-bit temporary `SuperAdmin__ApiKey`.
- Store it only in the current Windows user environment.
- Copy it directly to the Windows clipboard.
- Print restart guidance without printing the key.
- Clear in-process byte and string references after use.

## Validation

- PowerShell parser validation passed.
- The script executed successfully, stored the user environment value, and copied it to the clipboard without console disclosure.
- The Windows development-control CI gate validates helper syntax without executing or generating a key.
- GitHub Actions validation pending.
