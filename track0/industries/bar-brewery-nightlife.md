# Bar, Brewery & Nightlife Industry Profile

## Identity

- **Industry:** Bar, Brewery & Nightlife
- **RWP range:** RWP-00.15 through RWP-00.26
- **Current status:** Industry definition complete; subtype definition is next
- **Baseline:** Restaurant
- **Current RWP:** RWP-00.15
- **Next sequential RWP:** RWP-00.16 — Venue Subtypes

## Purpose

This profile covers beverage-led venues whose daily customer experience depends on accurate drink information, rapid operational changes, venue atmosphere, and time-sensitive event or entertainment communication.

It inherits the complete Restaurant baseline. This document records only the differences needed to establish the industry boundary and guide later subtype, terminology, operations, capability, packaging, onboarding, dashboard, and analytics RWPs.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- keep drink, tap, bottle, can, cocktail, wine, and limited-release information current;
- communicate fast-changing availability, pricing periods, service periods, and venue conditions without rebuilding content;
- promote entertainment, events, sports, private functions, and other time-bound reasons to visit;
- support distinct guest-facing screen purposes such as drink lists, tap lists, specials, event schedules, cover or entry information, and venue guidance;
- maintain clear, legible presentation across bright daytime service, dim evening environments, crowded spaces, and long viewing distances;
- coordinate a consistent brand while allowing venue-level operational differences across mixed concepts.

## Inherited unchanged from Restaurant

Unless a later Bar, Brewery & Nightlife RWP records a meaningful exception, this industry inherits:

- content, category, item, price, description, image, and label management;
- manual availability and Quick Update;
- screen pairing, management, explicit targeting, preview, and immediate publishing;
- delivery confirmation, online/offline and outdated status, and recovery to a prior published version;
- basic layouts and themes;
- business hours and venue information;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, campaign, multi-screen, multi-venue, approval, history, analytics, identity, AI, hardware, and integration capabilities.

Food menus, kitchen-led service, dietary information, and restaurant-style dayparts remain inherited where a venue actually uses them; they are not assumed to be the dominant operating model.

## Meaningful differences from Restaurant

### Beverage-led catalog emphasis

The primary content model emphasizes drinks, rapidly rotating products, pours or serving formats, producer or style information, and temporary or limited availability. This changes defaults, terminology, starter content, and recommendations; it does not create a separate entitlement model.

### Higher operational volatility

Products and offers can change repeatedly during a service period because of keg changes, limited releases, depleted stock, changing entertainment schedules, temporary service constraints, or last-call conditions. Quick Update remains an inherited core capability, but this industry depends on it more heavily and requires especially clear current-state feedback and recovery.

### Event and atmosphere as first-class context

Live music, DJs, trivia, sports, tastings, release events, guest lists, private events, and similar programming may be as important as the drink catalog. Detailed operating and capability treatment is deferred to RWP-00.18 onward.

### Late and irregular service periods

Operating days may cross midnight, vary by event, or include different access and service conditions during the same calendar day. Exact scheduling behavior is not defined in this RWP, but later work must avoid assuming that a business day ends at midnight.

### Distinct screen-purpose mix

A venue may combine beverage menus, rotating tap lists, promotional screens, event schedules, entry or cover information, wayfinding, and atmosphere-led displays. The profile does not presume that every screen is a menu board.

## Industry boundary

### Included as native concepts

The profile is intended to support beverage-led concepts including:

- bars and taverns;
- pubs and gastropubs where beverage service is the defining experience;
- sports bars;
- cocktail bars;
- wine bars;
- brewery taprooms;
- brewpubs;
- lounges;
- nightclubs with on-premise beverage service;
- related hybrid concepts where beverage-led service is a primary operating identity.

The exact supported subtype list, subtype definitions, and hybrid rules belong to RWP-00.16.

### Included through venue-level mixed-industry behavior

A venue may use this business type even when its parent organization has another primary industry. Examples include a hotel bar within a Hospitality organization, a dedicated bar venue within a Restaurant group, or a taproom operated by a broader manufacturing business.

### Outside the canonical boundary

The following are not treated as native Bar, Brewery & Nightlife concepts unless an included on-premise venue operation is also present:

- packaged alcohol retail without meaningful on-premise service;
- beverage manufacturing or distribution operations with no guest-facing taproom or service venue;
- food-led restaurants whose bar is secondary to the Restaurant operating model;
- entertainment or dance venues without beverage-led service;
- private clubs whose membership model, rather than venue service, is the defining product need.

These boundaries determine defaults and profile selection only. They are not legal, licensing, tax, or regulatory classifications.

## Organization and venue behavior

### Organization primary industry

- An organization may select Bar, Brewery & Nightlife as its primary industry.
- Primary industry seeds organization-level terminology, recommendations, starter content, and the first-venue setup experience.
- Primary industry is product/domain configuration, not a subscription entitlement.
- Changing primary industry must not silently add or remove commercial access.

### Venue business type

- Every venue may select its own business type and, later, a supported subtype.
- Venue business type controls local defaults, labels, screen-purpose recommendations, starter content, and operational guidance.
- Venue business type does not override organization-level entitlement authority.
- Changing a venue type must preserve existing customer content and require an explicit review before replacing defaults.

### Mixed organizations

- Restaurant, Bar, Brewery & Nightlife, Hospitality, and other venue types may coexist within one organization.
- Shared libraries, brand controls, users, analytics, and commercial access remain organization concerns unless a later policy explicitly defines venue scope.
- Venue-specific terminology and defaults must remain local so one venue cannot make another venue's interface misleading.
- Organization-wide views must use neutral language when subtype-specific terms would be ambiguous.

## Impeccable planning guardrails

RWP-00.15 is definition work rather than a detailed UI specification, but it establishes UI-facing constraints for later RWPs using the project-local Impeccable `shape` guidance:

- **Operator surfaces use Operate mode:** prioritize rapid scanning, state confidence, frequent updates, and recovery over decorative expression.
- **Guest-facing screens choose mode by purpose:** informational lists favor Read; atmosphere-led promotional screens may favor Experience while preserving legibility and essential facts.
- **Hierarchy:** current availability, price, active service period, event timing, and delivery status must outrank optional promotional detail when operationally relevant.
- **States:** later specifications must cover first-run, empty, scheduled, active, sold-out or unavailable, outdated, offline, permission-restricted, and recovery conditions.
- **Responsive and environmental behavior:** later work must consider mobile operator use, desktop administration, television displays, low-light venues, long viewing distances, and crowded visual environments.
- **Accessibility:** color alone must not communicate availability or status; hierarchy and text must remain understandable under glare, low light, motion, and reduced visual acuity.
- **Recovery:** high-frequency changes need clear confirmation, undo or restoration, and confidence that intended screens received the change.
- **Visual direction:** preserve the approved Sky Blue direction for Vennusign administrative surfaces. Venue content themes may express the venue brand without weakening operational clarity.

These guardrails shape planning only and authorize no UI implementation.

## Initial capability-matrix deltas

RWP-00.15 establishes three classification rules for later detailed work:

1. Organization primary industry is **product/domain state** that selects defaults and recommendations.
2. Venue business type or subtype is **product/domain state** that selects venue-local defaults and terminology.
3. Rapid manual beverage availability changes remain an inherited **core capability** acting on item availability state; the higher frequency of use does not turn availability into a tier gate.

Detailed required, optional, packaging, onboarding, dashboard, and analytics classifications are intentionally deferred to their approved RWPs.

## Owner decisions and deferred questions

The following are intentionally carried into RWP-00.16 rather than decided here:

- whether winery and distillery tasting rooms are first-class subtypes or related hybrid concepts;
- whether alcohol-free nightlife venues belong here or under Entertainment & Attractions;
- how private membership clubs should be classified when beverage service is operationally dominant;
- the exact distinction among pub, gastropub, brewpub, taproom, lounge, and nightclub subtypes;
- the default business type for a mixed organization when no single venue type is dominant.

## Reference anchors

These references inform the boundary but do not replace Vennusign's product model:

- The U.S. Census Bureau's 2022 NAICS definition for Drinking Places includes bars, taverns, nightclubs, cocktail lounges, tap rooms, and beverage-led brewpubs, while distinguishing food-led restaurants, packaged retail, manufacturing-only operations, and dance clubs without alcoholic beverage service: https://www.census.gov/naics/?details=722410&input=722410&year=2022
- The Brewers Association distinguishes brewpubs with significant food service from taproom breweries without significant food service, supporting subtype separation in RWP-00.16: https://www.brewersassociation.org/statistics-and-data/craft-beer-industry-market-segments/

## RWP-00.15 completion and handoff

### Completed

- Defined the profile purpose and additional customer outcomes.
- Recorded Restaurant inheritance and meaningful deltas only.
- Established included, mixed-industry, and excluded boundaries.
- Defined organization primary-industry and venue business-type behavior.
- Identified mixed-organization rules.
- Applied Impeccable planning guardrails where the definition affects future UI-facing work.
- Added the initial classification deltas to the Track 0 capability matrix.

### Not performed

- No product, UI, API, schema, migration, billing, entitlement, feature-gate, or rollout implementation.
- No integration or external-system testing.
- No subtype, terminology, detailed operations, packaging, onboarding, dashboard, or analytics decisions beyond the boundary required by this RWP.

### Next sequential RWP

**RWP-00.16 — Bar, Brewery & Nightlife Venue Subtypes** must define the supported subtype catalog, inclusion and exclusion rules, subtype-specific deltas, selection and change behavior, and hybrid scenarios before RWP-00.17 begins.
