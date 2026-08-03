# Issue-386 — Development Control Bootstrap Editor

## Status

In Review

## Execution Mode

Collaborative

## UX Guidance and Gap Analysis

Pattern search identified Form Validation and Password guidance.

- Goal: configure the bootstrap values used by API processes launched from Vennu Development Control.
- Hierarchy: configuration-provider inputs precede service controls; provider selection reveals the required local-key or Key Vault field.
- Actions: apply for this control session, generate a local key, explicitly save to Windows user environment, and explicitly clear saved values.
- States: existing values loaded, validation errors, applied, saved, cleared, API restart required, and API startup blocked.
- Safety: connection strings and local keys use masked inputs; values are never written to repository files, logs, status messages, or command arguments.
- Accessibility: labeled native controls, keyboard-accessible buttons, text status with live-region semantics, and no color-only status.
- Scope: local Windows development only. Hosted configuration and application behavior are unchanged.

## Acceptance

- Supports `VENU_CONFIGURATION_ENVIRONMENT`, `VENU_CONFIGURATION_CONNECTION_STRING`, `VENU_CONFIGURATION_KEY_PROVIDER`, `VENU_CONFIGURATION_LOCAL_KEY`, and `VENU_CONFIGURATION_KEY_ID`.
- Validated values are injected into API processes started or restarted by the control panel.
- A cryptographically random 256-bit local key can be generated.
- User-environment persistence and clearing are explicit.
- Secrets remain outside Git and are never printed.
- GitHub Actions classifies the development-control area explicitly and validates it on Windows.

## Validation

- `dotnet test tools/Vennu.DevControl.Tests/Vennu.DevControl.Tests.csproj -c Release` passed 6/6.
- Release WPF process launched and remained running.
- Change-classifier scenarios passed with explicit `dev_control` mapping.
- WCAG AA settings-form review reported no issues across labels, keyboard access, focus, names, and live feedback.
- GitHub Actions validation pending.
