# Vennusign Session Handoff

## Current State

- Item: RWP-13.03 — Onboarding Ownership and Navigation Unification / issue #421
- Mode: Sequential
- Branch: `rwp/13.03-onboarding-ownership-navigation`
- Status: Complete in the proposed merge state

## Result

- Back Office owns the only customer onboarding journey at `/onboarding`; signup and sign-in remain authentication entries only.
- Google, Apple, and email-link returns pass through the canonical resolver. Incomplete customers always resume the persisted server-selected task; completed customers continue only to a validated local Back Office path.
- Paired customers can enter Back Office without waiting for Online status; Back Office rechecks membership and the saved venue.
- Missing, stale, removed-access, provider-return, and pairing states preserve authority and create no duplicate journey.
- Platform Operations remains protected, read-only support visibility and explicitly does not enter or impersonate a customer workspace.
- The durable contract and UI/function gap analysis are recorded in `docs/architecture/onboarding-ownership-navigation.md` and `docs/archive/work-packages/RWP-13.03-onboarding-ownership-navigation.md`.

## Validation

- Back Office Node tests pass (60/60) and its production build passes locally.
- Platform Operations Node tests pass (86/86); its local production build is delegated because the local TypeScript compiler is unavailable.
- Exact-head affected-area GitHub Actions is authoritative for both frontend builds/tests and repository records.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

After this RWP merges and its claim is released, reassess and claim only RWP-13.01 / issue #416 in Sequential mode if it has no active owner.

## Do Not Redo

Do not create another onboarding route or state machine, trust browser step/return parameters, duplicate customer forms in Platform Operations, bypass membership checks, skip the recorded queue, or resume Phase 14+.
