# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Active implementation WP/RWP: none.
- Active planning track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488). Product implementation remains paused.
- Restaurant is the canonical approved baseline inherited by later native-industry profiles.
- RWP-13.06 — Trial-First Onboarding (#466) remains paused until Track 0 produces an owner-approved capability and packaging model.

## Native-Industry Track 0 Progress

| Industry | Completed through | Result | Next approved item |
| --- | --- | --- | --- |
| Bar, Brewery & Nightlife | **RWP-00.17** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.18 — Operating Characteristics (#493)** |
| Café, Bakery & Dessert | **RWP-00.29** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.30 — Operating Characteristics (#505)** |
| Food Truck & Concession | **RWP-00.41** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.42 — Operating Characteristics (#517)** |
| Hospitality | **RWP-00.52** | Industry definition and nine bounded property subtypes plus neutral fallback are documented. | **RWP-00.53 — Business Terminology (#528)** |
| Entertainment & Attractions | **RWP-00.65** | Industry definition, twelve venue subtypes plus neutral fallback, hybrid rules, canonical terminology, state wording, subtype preferences, neutral mixed-portfolio fallbacks, operator/visitor language boundaries, classifications, and Impeccable clarification guidance are complete in this proposed merge state. | **RWP-00.66 — Operating Characteristics (#541)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Entertainment & Attractions Terminology Result

Canonical operator concepts now include venue, attraction, experience, program, event, show, performance, screening, session, exhibit, collection, zone, queue, wait time, capacity, admission, ticket, schedule, wayfinding, and notice.

### Language rules

- Use subtype-specific language when it is precise and neutral organization-wide language when a mixed portfolio contains incompatible concepts.
- Preserve customer-authored names and local vocabulary unless invalid, unsafe, privacy-sensitive, or superseded by an authoritative source.
- Distinguish sold out from full, delayed from paused, canceled from closed, and expected reopening from a confirmed operating time.
- Do not imply quantity, wait precision, refund/rebooking policy, source authority, reopening time, or automated synchronization when unknown.
- Operator language may expose source, freshness, scope, target, approval, permission, delivery, and recovery detail.
- Visitor language prioritizes identity, current state, time, location, admission/access condition, direction, and next action.
- Public signage must not expose visitor-specific, ticket-specific, seat-specific, member-specific, participant-specific, performer-specific, sponsor-specific, security-sensitive, or operationally sensitive information by default.

### Classification

- Canonical, subtype-preferred, customer-authored, imported, and neutral-fallback terminology is **product/domain state**.
- Authorized manual terminology configuration is a **core capability**.
- Terminology does not grant commercial access, transfer authority, alter privacy scope, increase limits, or control rollout.
- Source-provided labels retain source-authority and freshness relationships; the integration remains a later add-on or tier candidate.
- Basic clear manual wording remains core. Localization workflow, premium translation, copy assistance, and AI generation remain later packaging questions.

### Impeccable clarification

Future terminology surfaces are Operate experiences. They must show object and scope, compare canonical/subtype/customer/imported/neutral language without implying plan differences, preview high-scope visitor impact, preserve authored content and authority, explain validation and stale-source conflicts plainly, cover permission/failure/success/undo/restoration states, support responsive and accessible use, and preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, billing, entitlement, feature-gate, limit-counting, privacy-system, localization, translation, AI, ticketing, admissions, queue, venue, show-control, collection, attraction, event, sports, or other product implementation was performed.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Action

After RWP-00.65 is merged, verified on `master`, issue #540 is closed, and the claim is released, execute **RWP-00.66 — Entertainment & Attractions Operating Characteristics** (#541).

RWP-00.66 must document timed schedules, screenings/shows, queues and wait times, capacity, admissions, exhibits/attractions, closures, safety notices, wayfinding, event surges, multilingual needs, subtype differences, source/freshness boundaries, defaults, and capability presentation. It must remain documentation-only and hand off to RWP-00.67.

Other owner-approved native-industry schedules may continue independently only when shared controlled-file ownership does not conflict.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, ticketing, admissions, access control, queue management, venue management, show control, collection management, attractions, events, sports, analytics, localization, integrations, or later-phase work until the owner approves the completed capability matrix and implementation packages.
