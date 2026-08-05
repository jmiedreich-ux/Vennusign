# RWP-00.21 — Bar, Brewery & Nightlife Capability Classification

## Status

Complete in this proposed merge state.

## Issue

#496

## Scope completed

- Consolidated required and optional Bar capability inventories.
- Assigned one primary Track 0 classification to every inventoried capability and concern.
- Resolved ambiguity among availability/Quick Update, happy hour, taps, events, reservations, approvals, analytics, monitoring, subtype, screen count, and external synchronization.
- Preserved Restaurant inheritance instead of duplicating unchanged baseline capabilities.
- Identified owner-review questions that are packaging decisions rather than classification gaps.
- Applied Impeccable state-separation guidance for future capability and upgrade surfaces.

## Validation

Reviewed against issue #496, RWP-00.18 through RWP-00.20, Restaurant inheritance, and the Track 0 classification policy. Manual availability and other operating values remain product state, permissions remain authority, quantities remain limits, and no live gates changed. Documentation only; integration and external-system tests remain skipped.

## Handoff

After merge, verification, issue closure, and claim release, execute **RWP-00.22 — Bar, Brewery & Nightlife Subscription Tier Mapping** (#497). Shared-record changes remain queued for the consolidated Bar completion checkpoint.