# VennuSign Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

VennuSign serves operators responsible for what the public sees on physical screens. The primary user works in interrupted, time-sensitive bursts—often on a phone, standing up, and mid-shift—to change information, check the screens, and leave. Longer desk sessions support bulk editing, scheduling, and review, but the burst workflow wins when they conflict.

Authority is real rather than cosmetic:

- **Organization Owner** has full authority.
- **Content Editor** edits content but cannot publish or control screens.
- **Publisher** publishes and recovers deliveries but cannot edit content.

## Product Purpose

VennuSign is digital signage that tells the truth. People author public-facing content, preview it as the audience will see it, target physical screens, publish it, verify that it landed, and recover delivery when a player drops.

The core loop is:

> Edit content → preview exactly what the screen will show → target → publish → verify delivery → recover when a player drops.

Success means the public sees correct information and the operator can prove what each screen applied.

## Positioning

VennuSign runs screens and proves it. Delivery is proven rather than assumed: each screen tracks an authoritative revision against an applied revision through requested, received, and applied states, with recovered, superseded, offline, and failed as distinct outcomes. The product says “Applied” only when the applied revision matches the authoritative one.

Offline operation is normal. A publish to an offline screen waits and recovers automatically on reconnect without requiring an operator to push again.

Industry and subtype are configuration, not commercial packaging. They may change terminology, defaults, starter content, and dashboard emphasis, but never the content model, editor, or entitlement. A tap list, class schedule, departure board, and clinic notice set share the same underlying object model.

## Operating Context

VennuSign manages:

- **Content:** collections, sections, and entries with descriptions, pricing, variants, availability, dietary or attribute data, images, and translations. Save-race protection prevents a slow save from overwriting a newer edit.
- **Screens:** six-digit pairing, pre-registration, online/offline/stale heartbeat state, replacement that preserves logical identity and history, archive, restore, unpair, reset, and video walls.
- **Scheduling:** service periods, happy hour, playlists, date-range promotions, and emergency broadcasts. Scheduling is resolved by the server in the venue’s timezone; the browser is never the scheduling authority.
- **Publishing:** preview, explicit targeting, publish, per-target delivery confidence, correction, supersession, expiry, unpublish, retry, undo, and restore.
- **Sources:** optional POS and external-system synchronization with visible source identity, freshness, staleness, conflict, and local override.
- **Access:** roles, scopes, allowances, entitlements, and audit as separate concerns.
- **Localization:** language variants with fallback. Language changes words, never decisions.

## Capabilities and Constraints

- Every action resolves server-side as allowed, denied, unavailable, or temporarily blocked. Each result carries a reason code, plain-language message, structured detail where relevant, and a correlation ID.
- A limit may block growth but never repair. An allowance can stop a new screen from being paired; it cannot stop correction of content already live.
- Manual operation always survives when an integration, automation, external source, or paid add-on is unavailable. Essential operation is core; automation is the add-on.
- The system consists of a .NET API and SQL data layer, browser-based Back Office, display player, native Android/Tizen/webOS TV shells, a shared render engine, and an internal operations console.
- Preview and display use the shared render engine so they agree.

## Brand Commitments

The product name is **VennuSign**. Its identity is “sky”: light surfaces, sky blue on slate ink, and a fixed semantic status palette—live green, off red, warning amber, emergency red, and promotion purple.

The voice is direct, evidence-based, operational, and honest. It distinguishes permission denial, plan unavailability, and temporary blockage because each has a different remedy.

## Evidence on Hand

- Approved product and feature design authority, controlled feature plans, and owner decisions
  all live under `docs/features/<feature>/` — one directory per feature (amended 2026-08-24;
  approved design used to live in a separate `docs/design/approved/` tree).
- Delivery states, access decisions, allowances, availability, and shared rendering have executable implementations and tests in the repository.
- Do not fabricate customer names, testimonials, performance benchmarks, prices, deployment claims, or device compatibility evidence.

## Product Principles

1. **State before controls.** Show what is true on the screens before offering an action.
2. **Never claim what the server cannot prove.** Delivery and refusal language carries evidence, counts, and reasons.
3. **A limit blocks growth, never repair.**
4. **Every “no” says which kind it is** and what would change it in the customer’s language.
5. **Learnable in the thick of it.** A screen’s primary job must be possible without training or documentation.

## Accessibility & Inclusion

Primary tasks must remain understandable and operable during interrupted, high-pressure work. Status cannot depend on color alone. Roles, entitlement state, delivery state, and failure recovery must be communicated in plain language and with semantic controls. Mobile use is important to the product context, but a surface does not claim mobile support until it has been explicitly designed and verified.
