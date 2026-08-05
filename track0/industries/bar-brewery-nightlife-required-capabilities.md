# Bar, Brewery & Nightlife Required Capabilities

## Authority and scope

This document defines the smallest viable Bar, Brewery & Nightlife capability set for RWP-00.19. It inherits the approved Restaurant baseline and adds only requirements necessary for practical beverage-led, late-night, event-aware daily operation. It does not approve packaging or implement product behavior.

## Required-core rule

A venue must be able to complete essential daily work without purchasing a premium tier or external integration. Manual operation, explicit targeting, publishing confidence, correction, and recovery cannot be commercially gated.

## Inherited Restaurant core

The Bar profile inherits menu/content organization; item, category, price, description, image, and label management; manual availability and Quick Update; venue information and hours; screen pairing and management; explicit targeting and preview; immediate publishing; delivery confirmation; online/offline and outdated state; prior-version restoration; basic layouts and themes; and permission, state, entitlement, add-on, limit, and rollout separation.

## Required Bar capability groups

### 1. Drink, list, and serving-format management

Operators require manual drink menus, tap lists, cocktail lists, wine lists, specials, releases, flights, pour sizes, packaged formats, and optional food content. Item names, categories, producer/style context, prices, serving formats, and customer-authored terminology remain editable core content.

### 2. Rapid availability and Quick Update

Operators require immediate available, unavailable, and sold-out changes at the correct item or serving context; clear venue/list/area scope; current-state feedback; preview; publish; delivery result; correction; supersession; retry; and restoration. Automated inventory is not required.

### 3. Hours and service-period communication

Core operation includes venue, kitchen, bar, happy-hour, doors, event, last-entry, and locally authored last-call timing, including cross-midnight periods, temporary changes, early closure, delayed opening, private-event periods, and next confirmed service information.

### 4. Specials, releases, and time-bound content

Operators require manual happy-hour, featured item, tasting, flight, release, game-day, and event-linked content with effective local time, venue/area scope, targets, expiration, correction, and restore behavior. Advanced recurrence and automated scheduling are optional.

### 5. Events and entertainment information

Manual live-music, DJ, trivia, karaoke, sports, tasting, release-event, and watch-party information remains core, including local date/time, doors, start, area/viewing zone, delay, cancellation, relocation, replacement, pause, resumption, entry guidance, and affected screens.

### 6. Venue, area, service-model, and entry guidance

Operators require clear manual guidance for bar, table, counter, patio, lounge, standing, viewing-zone, and hybrid service contexts. General reservation, guest-list, cover, ticket, private-event, age, and access information may be communicated without claiming a person's eligibility or transaction state.

### 7. Responsible and controlled public wording

Authorized operators require manual locally approved age, access, service, warning, and responsible-content wording. Vennusign does not invent jurisdictional policy. Controlled wording requires permission boundaries, explicit scope, preview, delivery confirmation, correction, expiration, supersession, and restoration.

### 8. Screen-purpose targeting and preview

Core screen purposes include drink menu, tap list, cocktail/wine list, food menu where used, specials, release, event lineup, sports/viewing schedule, entry guidance, private-event notice, wayfinding, and promotional/atmosphere content. Operators require explicit target selection and preview before high-scope publication.

### 9. Publishing and delivery confidence

Every publish must identify selected content, venue/area, target screens, intended version, result, failed/partial targets, offline/outdated screens, and actionable retry or recovery. A saved draft is not a published version.

### 10. Correction, supersession, and restoration

Operators require safe correction of wrong prices, availability, times, event state, entry information, and targets; clear replacement or expiry; last-known-good content; undo or prior-version restoration; and no silent overwrite from stale sources.

### 11. Permissions and required state separation

Permissions control view, edit, approve, publish, restore, controlled wording, venue scope, and area scope. Availability, hours, events, entry, delivery, source, and freshness are product/domain state. Neither permission nor state becomes a commercial feature flag.

## Required states

Future implementations must distinguish first use, no content, no matching result, draft, scheduled, active, expired, superseded, available, unavailable, sold out, canceled, delayed, relocated, unknown, saved, publishing, published, partially delivered, failed, offline, outdated, stale source, source conflict, restored, validation failure, permission restriction, and success/undo.

## Subtype emphasis

Pub emphasizes recurring events and mixed drink/food service. Sports Bar emphasizes fixtures, viewing zones, and game-day changes. Cocktail Bar emphasizes curated lists and ingredient availability. Wine Bar emphasizes serving formats and tastings. Brewery emphasizes house portfolio, releases, tours, and package formats. Brewpub coordinates beverage and kitchen operation. Taproom emphasizes taps, pours, flights, and rapid keg changes. Nightclub emphasizes doors, entry, lineup, zones, and late-night operation. Lounge emphasizes reservations, tables/areas, curated lists, and low-light readability.

These emphases alter defaults and presentation, not capability access.

## Not required core

The required set excludes automated POS/inventory/tap synchronization, ordering/payment, reservation or guest-list transaction handling, ticketing, identity or access control, sports feeds, advanced recurrence, campaigns, approvals, cross-venue libraries, deep analytics, AI, managed hardware, and custom integrations. These may be tier or add-on candidates but cannot replace manual core operation.

## Impeccable planning implications

Future Operate surfaces must expose the smallest task-relevant set, keep Quick Update and publish/recovery actions immediately available, use specific verb-object labels, show permission and commercial-access states separately, and cover phone/desktop, keyboard, assistive technology, 200% zoom, long names, local-time clarity, non-color state, low-light and distance readability, restrained motion, and the approved Sky Blue direction.

## Classification summary

- The eleven capability groups are **core capabilities**.
- Content, hours, service periods, availability, events, areas, entry information, source, freshness, publication, and delivery are **product/domain state** where represented.
- Authority is **permission**.
- Advanced workflow and scale are **tier-entitlement candidates**.
- External automation and managed services are **independent add-on candidates**.
- Counts and consumption are **usage or quantity limits**.
- Staged delivery controls are **internal rollout flags**.

## Boundaries and handoff

Documentation only. RWP-13.06 and Phase 14+ remain paused. RWP-00.20 owns optional-capability candidates and must preserve every required manual capability defined here.