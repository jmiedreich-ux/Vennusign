# Track 1 — Capability, Entitlement and Authority Foundation

## Status

Track 1 is approved for bounded sequential execution. This roadmap defines the package order and dependencies. It does not itself implement product behavior.

Only Track 1.01 is initially available to claim. Later packages remain blocked until their direct predecessor merges, the default branch is verified, and the sequential claim is released.

## Purpose

Track 1 creates the shared technical foundation required by later tier, allowance, add-on, access-state, industry, onboarding, billing, and migration work.

It establishes a consistent answer to three questions:

1. What canonical capability is being requested?
2. Is that capability available in the current organization and operating context, and why?
3. Is the current actor authorized to perform the requested action at that scope?

## Sequential package order

### Track 1.01 — Canonical Capability Registry and Legacy Aliases

GitHub issue: #640

Define stable capability identifiers, metadata, primary classification, compatibility aliases, deprecation rules, collision handling, current consumers, migration records, and focused compatibility tests.

### Track 1.02 — Server Capability Decision and Reason Contract

GitHub issue: #641

Consume canonical capability IDs and return server-authoritative availability decisions with distinct entitlement, permission, product-state, add-on, limit, restriction, exception, and rollout reasons plus safe actions.

Blocked by Track 1.01.

### Track 1.03 — Scoped Permission and Authority Model

GitHub issue: #642

Normalize organization, venue/context, object, and action authority independently from commercial entitlement and product state. Preserve bounded compatibility for existing roles, claims, sessions, support authority, billing authority, and platform administration.

Blocked by Track 1.02.

### Track 1.04 — Essential Core and Legacy Gate Migration

GitHub issue: #643

Migrate essential manual operation away from overloaded commercial gates while retaining advanced outcomes as separately classified capabilities or add-ons and preserving public-output correction and recovery.

Blocked by Track 1.03.

### Track 1.05 — Validation and Handoff

GitHub issue: #644

Validate the Track 1 foundation across representative organization, context, object, actor, legacy-key, denied, unavailable, and recovery journeys; reconcile compatibility evidence; close Track 1; and hand off bounded downstream dependencies without starting another track.

Blocked by Track 1.04.

## Dependency chain

Track 1.01 → Track 1.02 → Track 1.03 → Track 1.04 → Track 1.05

No package may skip ahead. One Sequential claim and one implementation PR are allowed at a time.

## Track completion result

Track 1 is complete only when:

- stable canonical capability IDs exist;
- current keys and slugs have explicit compatibility aliases;
- server capability decisions return stable reasons and safe actions;
- permissions are independently scoped by organization, local context, object, and action;
- essential manual operation no longer depends incorrectly on overloaded commercial flags;
- existing supported consumers have a tested migration path;
- public-output correction, unpublish, retry, restore, and manual fallback remain protected;
- Track 1 validation passes on the exact PR head;
- default-branch verification and final handoff are complete.

## Boundaries

Track 1 does not set or implement:

- final tier names or pricing;
- numeric allowances or screen pricing;
- Tier Manager or Billing Manager;
- Free-tier onboarding or paid trial conversion;
- promotions or migration campaigns;
- complete add-on or integration models;
- dashboard redesign or analytics implementation;
- native-industry vertical features;
- RWP-13.06 as currently written;
- Phase 14 or later work.

Azure SQL, live Stripe, devices, hosted/browser, credentialed infrastructure, and integration/external-system tests remain skipped unless separately authorized. Focused affected-area non-integration tests and exact-head GitHub Actions are authoritative.

## Execution rules

- Recheck default branch, tracker, issues, PRs, recent merges, reviews, and Actions before claiming.
- Claim exactly one package in Sequential mode.
- Use a dedicated branch and PR per package.
- Do not combine multiple Track 1 packages into one PR.
- Preserve compatibility unless the accepted package explicitly migrates a consumer.
- Record migrations, deployment compatibility, focused validation, skipped tests, and rollback/recovery in every package.
- Merge, close, verify the default branch, release the claim, and unblock the next issue only after exact-head review passes.

## Current release state

- Track 1.01 / issue #640: approved and ready after this roadmap merges.
- Track 1.02 / issue #641: blocked by #640.
- Track 1.03 / issue #642: blocked by #641.
- Track 1.04 / issue #643: blocked by #642.
- Track 1.05 / issue #644: blocked by #643.
