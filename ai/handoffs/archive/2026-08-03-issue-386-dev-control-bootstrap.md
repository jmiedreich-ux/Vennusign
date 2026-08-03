# Vennu Session Handoff

## Work Package
- ID: Issue-386
- Status: In Review
- Execution mode: Collaborative

## Git State
- Branch: `issue/386-dev-control-bootstrap`
- Issue: #386
- Pull request: #387
- CI state: pending

## Completed
- Added environment, connection-string, and key-provider bootstrap entry to Vennu Development Control.
- Added masked local-key/connection inputs, provider-dependent Key Vault input, and cryptographic local-key generation.
- Added session apply, explicit Windows user-environment save, confirmed clear, validation, and restart feedback.
- API Start/Restart receives only current validated bootstrap values; invalid combinations block API startup.
- Added a focused Windows test project with 6 passing tests.
- Verified the Release WPF process launches and completed a WCAG AA settings-form review with no issues.

## Validation
- `dotnet test tools/Vennu.DevControl.Tests/Vennu.DevControl.Tests.csproj -c Release`: 6/6 passed.
- Release WPF launch check passed.
- GitHub Actions pending.

## Remaining Work
- Run PR #387 checks, review, merge, release the claim, and synchronize completion records.

## Known Risks
- Windows user environment variables are user-readable storage, not a secrets vault; persistence is explicit and local-development-only.
- Hosted Key Vault behavior is unchanged and live testing remains excluded.

## Exact Next Action
- Run PR #387 affected WPF/tooling validation and review the exact head.

## Do Not Redo or Reverse
- Do not persist bootstrap values to repository files.
- Do not display connection strings or local keys in logs or status text.
- Do not inject these values into non-API child processes.
