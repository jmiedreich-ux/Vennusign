# Issue-371 — Local Development Control Panel

## Status

Complete through PR #372.

## Execution Mode

Collaborative

## Scope

- Replace the nonfunctional PowerShell control panel with a local .NET 9 WPF application.
- Start, stop, restart, and open the API, Admin, Venue Admin, and Display services independently.
- Stop only processes launched by the panel, including their child process trees.

## UX and Gap Analysis

- Dashboard layout: each service has a visible port, status, and Start, Stop, Restart, and Open actions.
- States: stopped, starting, and running are visible through port polling.
- Safety: Stop Owned does not target unrelated processes; no credentials appear in the UI.
- Accessibility: buttons have explicit action labels and service state is readable as text.

## Validation

- `dotnet build tools/Vennu.DevControl/Vennu.DevControl.csproj` passed.
