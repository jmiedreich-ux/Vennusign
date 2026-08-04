# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Active implementation WP/RWP: none.
- Active planning track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488). Product implementation remains paused.
- The owner approved independent native-industry Track 0 schedules. Each industry remains sequential inside its own approved RWP range and must avoid shared-file ownership conflicts.
- Restaurant is the canonical approved baseline inherited by later native-industry profiles.
- RWP-00.15 — Bar, Brewery & Nightlife Industry Definition (#490) is complete and merged. Its next item is RWP-00.16 — Venue Subtypes (#491).
- RWP-00.27 — Café, Bakery & Dessert Industry Definition (#502) is complete and merged. Its next item is RWP-00.28 — Venue Subtypes (#503).
- RWP-00.39 — Food Truck & Concession Industry Definition (#514) is complete and merged. Its next item is RWP-00.40 — Venue Subtypes (#515).
- RWP-00.51 — Hospitality Industry Definition (#526) is complete in this proposed merge state. Its canonical profile defines Restaurant inheritance, lodging-led property boundaries, property and local-venue behavior, initial capability classifications, privacy constraints, and Impeccable planning guardrails.
- The next Hospitality item is RWP-00.52 — Venue Subtypes (#527).
- The 13-package retrospective remediation round is complete.
- RWP-00.04 — Deployment Component Versioning and Release Manifest (#437) is complete.
- RWP-05.07 — Atomic Screen Replacement and Pairing Recovery (#439) is complete.
- RWP-08.02 — Daylight-Saving-Safe Scheduling Resolution (#440) is complete.
- RWP-10.02 — Durable Player Content Receipts and Delivery Reconciliation (#441) is complete.
- RWP-00.05 — Affected-Screen Action Completeness and Recovery (#442) is complete.
- A new 18-item Sequential remediation queue is approved through issues #448–#465.
- RWP-02.01 — Display Player State-Screen Presentation (#448) is complete and merged.
- RWP-00.06 — Shared Design Tokens and Palette Consolidation (#449) is complete and merged.
- RWP-00.07 — Small-Text Contrast Remediation (#450) is complete and merged.
- RWP-00.08 — Destructive-Action Confirmation Standardization (#451) is complete and merged.
- RWP-05.08 — Screens Page Information Architecture (#452) is complete and merged.
- RWP-04.03 — Platform Operations Mobile and Console Polish (#453) is complete and merged.
- RWP-00.09 — Transient Feedback System (#454) is complete and merged.
- RWP-00.10 — Iconography, Empty States, and Loading Skeletons (#455) is complete and merged.
- RWP-07.01 — Display Theme Font Bundling (#456) is complete and merged.
- RWP-00.11 — Midnight Admin Theme (#457) is complete and merged.
- RWP-00.12 — Sky UI Visual Standard (#458) is complete and merged.
- RWP-05.09 — Daypart Home and Navigation Shell (#459) is complete and merged.
- RWP-13.04 — Signup and Marketing Page with Live Demo (#460) is complete and merged.
- RWP-13.05 — Go-Live and First-Run Experience (#461) is complete and merged.
- RWP-00.13 — Action Hierarchy and Button Placement Standard (#462) is complete and merged.
- RWP-05.10 — Visual-First Screens Fleet (#463) is complete and merged.
- RWP-11.03 — Unified Entitlement Experience (#464) is complete and merged.
- RWP-11.04 — Personalized Locked Previews (#465) presents the active venue's authorized menu content inside read-only locked theme/layout previews.
- The 18-item approved Sequential remediation queue is complete.
- RWP-00.14 — Project-Local Impeccable Codex Design Skill (#486) is complete and merged. It installs the official v4.0.4 skill, its advisory edit/stop hook, and the repository rule requiring it for UI work without changing product runtime or UI.
- RWP-13.06 — Trial-First Onboarding (#466) implementation is paused while Track 0 establishes the supported-industry, capability, packaging, and entitlement model.
- Research program `INT-TESTING-001` remains documented but is not part of Track 0 planning.

## Completed Delivery

| Phase | Result |
| --- | --- |
| 02 | Core backend, display boot, realtime updates, and heartbeat lifecycle |
| 03 | Tier, feature, subscription, Stripe, and usage foundations |
| 04 | Protected Platform Operations CRM, support, commercial, and revenue workflows |
| 05 | Back Office CMS, menus, quick update, screens, and video walls |
| 06 | Restaurant/cafe layouts, themes, overflow, and offline media |
| 07 | Bar layouts, advanced themes, motion, and multilingual font delivery |
| 08 | Scheduling, playlists, promotions, and emergency broadcast |
| 09 | Tap administration, brewery layouts, and pairing registration |
| 10 | Android/Fire TV, Tizen, webOS, HaaS provisioning, and fleet health |
| 11 | Upgrade UX, Checkout, Billing Portal, and HaaS billing guardrails |
| 12 | Square, Toast, and Clover integrations through a shared POS model |
| 13 | Customer identity, organization entitlements, signup, onboarding, and legacy-token migration |

Cross-cutting remediation completed after Phase 13 includes RWP-00.02, RWP-00.03, RWP-04.02, RWP-05.04, RWP-05.05, RWP-05.06, RWP-08.01, RWP-09.01, RWP-10.01, RWP-11.02, RWP-13.03, RWP-13.01, and RWP-13.02. Completed WP/RWP details, phase plans, validation evidence, and earlier status snapshots are retained under `docs/archive/` for deliberate research.

## Track 0 Classification Policy

Every capability must have one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

The Café, Bakery & Dessert definition confirms that manual sold-out and available-again changes remain a core capability acting on product state. Batch, freshness, limited-quantity, and expected-return values are product/domain state when represented. Automatic POS, order, inventory, production, or pickup synchronization remains a later integration-packaging question and cannot replace the manual core operation.

The Food Truck & Concession definition confirms that current operating location, event, service window, relocation, closure, and related operational values are product/domain state. Manual location and closure communication, rapid availability changes, explicit screen targeting, publishing, delivery confirmation, offline awareness, and recovery remain core. Counts of venues, units, stands, screens, users, connections, storage, history, or AI consumption remain limits rather than capabilities. Automatic route, event, host-venue, POS, order, inventory, or location synchronization remains a later packaging question.

The Hospitality definition confirms that property, building or area, outlet, room or event, amenity, service window, closure, relocation, and similar operational values are product/domain state. Manual guest-information, wayfinding, event, amenity, service, changed-hours, targeting, publishing, delivery confirmation, offline awareness, and recovery operations remain core. Authorization and privacy scope remain separate from commercial access. Automatic property-management, event, room-booking, transport, guest-service, or other external synchronization remains a later packaging question.

## Validation Policy

Normal work uses affected-area non-integration validation. Full non-integration validation is reserved for phase closure and the exceptions defined in `AGENTS.md`. Documentation-only Track 0 changes use lightweight repository validation. Integration and external-system tests remain skipped under the standing owner instruction unless separately approved.

## Next Action

After this RWP is merged, verified on `master`, and issue #526 is closed, continue the Hospitality queue with **RWP-00.52 — Venue Subtypes** (#527). Define hotel, resort, motel, hostel, extended-stay, serviced-apartment, conference-property, casino-resort, boutique-lodging, and hybrid subtypes; establish boundary and ambiguous-case rules; map subtype-specific deltas; and keep subtype separate from tiers, permissions, and limits.

Other owner-approved native-industry schedules may continue independently inside their own sequential queues. They must use Restaurant as the canonical baseline, treat only merged work as authoritative, and avoid concurrent edits to shared controlled files.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, or migrations until the owner approves the completed capability matrix and implementation packages. RWP-13.06 and Phase 14+ remain paused.
