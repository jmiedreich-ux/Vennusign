# RWP-00.60 — Hospitality Default Dashboard

## Status

Complete in this proposed merge state.

## Issue

- #535

## Objective

Define the default Hospitality dashboard as an attention-first, task-first operating surface. Cover property status, guest notices, amenities and outlets, meetings and events, wayfinding, screen and publication health, source freshness, emergency and recovery visibility, role-aware and shift-aware presentation, property-group views, and mobile/desktop priorities. Documentation only.

## Dependency verified

- RWP-00.59 is merged, verified, closed, and released.
- The approved Hospitality industry, subtype, terminology, operating, capability, classification, tier, and onboarding records are authoritative inputs.
- RWP-00.61 — Hospitality KPIs & Analytics (#536) is next.

## Delivered

- Added `track0/industries/hospitality-default-dashboard.md`.
- Defined an Operate-mode dashboard led by current exceptions and guest impact.
- Defined current guest communication, quick actions, property operating snapshot, notices, amenities/outlets, meetings/events, wayfinding, screen/publish health, source/override health, and recovery regions.
- Kept save, schedule, publish request, delivery confirmation, correction, supersession, expiry, retry, and restoration distinct.
- Defined property-operator, shift-lead, content/brand, technical, property-group, and restricted-user presentation.
- Defined start-of-shift, arrival peak, ordinary operation, event peak, departure peak, and overnight emphasis without introducing private guest-state automation.
- Defined exception-first property-group views, inherited/local state, safe bulk action, local control, and rollback expectations.
- Defined mobile and desktop priorities, required empty/loading/failure/success states, and contextual upgrade/add-on presentation.
- Applied project-local Impeccable `shape`, `clarify`, `harden`, `adapt`, and bounded `polish` guidance.

## Classification result

- Current property, object, notice, schedule, language, source, target, screen, delivery, and recovery values are product/domain state.
- Edit, publish, restore, screen, property, restricted-content, and group authority are permissions.
- Advanced workflow, coordination, portfolio, governance, and enterprise administration remain tier candidates.
- External source connections and managed services remain independent add-on candidates.
- Counts, retention, volume, frequency, storage, and consumption remain limits.
- Temporary release controls remain rollout flags.

The dashboard must never collapse these into one generic unavailable condition.

## Core protection

Manual property information, notices, hours and states, meetings/events, wayfinding, language variants, explicit targeting, preview, publishing, delivery confidence, offline/outdated awareness, correction, expiry, supersession, retry, and restoration remain visible core actions. Integrations and premium workflows may add convenience or scale but cannot replace the manual fallback.

## Role and shift result

Presentation changes according to current responsibility and operating context, but role, shift, industry, subtype, property-group membership, or dashboard visibility does not grant commercial entitlement or object authority.

No private guest data is required for shift-aware emphasis.

## Property-group result

Cross-property views begin with exceptions and mixed states rather than totals. They preserve local time, source, inherited/local status, excluded properties, local overrides, privacy boundaries, urgent notices, current public truth, and last-known-good content. Bulk actions require explicit scope, preview, impact, confirmation, and property-specific recovery.

## Impeccable result

The dominant mode is Operate. Future UI should show immediate state, scope, impact, action, feedback, and recovery. It must support keyboard and assistive technology, visible focus, non-color-only state, 200% zoom, long names, localization expansion, right-to-left layouts, reduced motion, responsive layouts, local date/time, and the approved Sky Blue administrative direction.

## Validation

Documentation-only review confirmed:

- issue #535 scope is covered;
- the dashboard is operational rather than promotional or analytics-first;
- urgent exceptions and current guest impact are prioritized;
- property, object, screen, publication, source, language, role, shift, and group states are represented;
- manual core operation remains available;
- permission, tier, add-on, limit, state, source, privacy, and rollout remain separate;
- recovery and partial-delivery states are explicit;
- RWP-00.61 is the exact next Hospitality item.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

All integration and external-system testing and all product implementation, including UI, API, schema, migrations, analytics pipelines, alerts, permissions, billing, entitlements, feature gates, limits, localization systems, privacy systems, PMS/event/occupancy connections, player behavior, AI, hardware, connectivity, monitoring, and managed services.

## Shared-record checkpoint

Semantic updates for `tracker/assignments.json`, `PROJECT_STATUS.md`, `ai/handoffs/current.md`, and `track0/CAPABILITY_MATRIX.md` remain queued. They will be reconciled once at the final Hospitality completion checkpoint under RWP-00.62 using a short transactional write window against current `master`.

## Exact next action

After this RWP is merged, verified on `master`, issue #535 is closed, and the claim is released, execute **RWP-00.61 — Hospitality KPIs & Analytics** (#536).
