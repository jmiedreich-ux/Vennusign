# Vennusign Session Handoff

## Current State

- Item: RWP-13.04 — Signup and Marketing Page with Live Demo / issue #460
- Mode: Sequential
- Branch: `rwp/13.04-signup-marketing-live-demo`
- Status: Implemented and locally validated; pending exact-head Actions, review, and merge

## Approved Queue

RWP-13.05 / issue #461 is next only after RWP-13.04 fully merges, closes, verifies, and releases its claim. The remaining approved queue continues through RWP-11.04 / issue #465. RWP-13.06 / issue #466 is held and excluded. Phase 14+ remains paused.

## RWP-13.04 Proposed Outcome

- Public signup explains Vennusign through an interactive, self-contained service-period preview before account creation.
- Public plan pricing, limits, and trial availability come from the existing anonymous contract and cannot grant entitlement.
- Grounded product proof and the pairing-to-Online story set accurate expectations without inventing customer data or live state.
- Existing account, provider, passkey, email-link, onboarding, authorization, and webhook behavior remains unchanged.

## Boundaries

- No API, server persistence, authorization, entitlement, schema, or data-contract changes.
- RWP-13.05 retains ownership of the go-live and first-run experience.
- Do not claim or implement held RWP-13.06 / issue #466 or resume Phase 14+.
- Azure SQL, live Stripe, hosted browser, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

Publish the RWP-13.04 implementation PR, require affected Back Office GitHub Actions on the exact reviewed head, review and merge it, close issue #460, verify `master`, and release the claim. RWP-13.05 / issue #461 is next only after that sequence completes.
