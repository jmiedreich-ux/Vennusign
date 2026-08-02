# Issue-370 — Canonical Customer Root Routes

## Status

In Review

## Execution Mode

Collaborative

## Scope

- Serve customer signup, sign-in, and onboarding at `/signup`, `/signin`, and `/onboarding` in every environment.
- Keep authenticated Venue Admin at `/` on the same application host.
- Remove the local `/venue-admin/` Vite base-path mismatch.
- Update local launchers and route coverage.

## Validation

- `npm.cmd run build --prefix src/venue-admin`
- Venue Admin route coverage includes signup, sign-in, and onboarding.
- WPF and PowerShell launchers use the canonical Venue Admin root URL.
