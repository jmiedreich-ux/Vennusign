# Bar, Brewery & Nightlife Industry Profile

## Identity

- **Industry:** Bar, Brewery & Nightlife
- **RWP range:** RWP-00.15 through RWP-00.26
- **Current status:** Industry definition and venue-subtype model complete
- **Baseline:** Restaurant
- **Current completed RWP:** RWP-00.16
- **Next sequential RWP:** RWP-00.17 — Business Terminology

## Purpose

This profile covers beverage-led venues whose daily customer experience depends on accurate drink information, rapid operational changes, venue atmosphere, and time-sensitive event or entertainment communication.

It inherits the complete Restaurant baseline. This document records only the differences needed to establish the industry boundary and guide subtype, terminology, operations, capability, packaging, onboarding, dashboard, and analytics work.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- keep drink, tap, bottle, can, cocktail, wine, and limited-release information current;
- communicate fast-changing availability, pricing periods, service periods, and venue conditions without rebuilding content;
- promote entertainment, events, sports, private functions, and other time-bound reasons to visit;
- support distinct guest-facing screen purposes such as drink lists, tap lists, specials, event schedules, cover or entry information, and venue guidance;
- maintain clear, legible presentation across bright daytime service, dim evening environments, crowded spaces, and long viewing distances;
- coordinate a consistent brand while allowing venue-level operational differences across mixed concepts.

## Inherited unchanged from Restaurant

Unless a later Bar, Brewery & Nightlife RWP records a meaningful exception, this industry and every subtype inherit:

- content, category, item, price, description, image, and label management;
- manual availability and Quick Update;
- screen pairing, management, explicit targeting, preview, and immediate publishing;
- delivery confirmation, online/offline and outdated status, and recovery to a prior published version;
- basic layouts and themes;
- business hours and venue information;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, campaign, multi-screen, multi-venue, approval, history, analytics, identity, AI, hardware, and integration capabilities.

Food menus, kitchen-led service, dietary information, and restaurant-style dayparts remain inherited where a venue uses them. They are not assumed to be the dominant operating model for every subtype.

## Meaningful differences from Restaurant

### Beverage-led catalog emphasis

The primary content model emphasizes drinks, rapidly rotating products, pours or serving formats, producer or style information, and temporary or limited availability. This changes defaults, terminology, starter content, and recommendations; it does not create a separate entitlement model.

### Higher operational volatility

Products and offers can change repeatedly during a service period because of keg changes, limited releases, depleted stock, changing entertainment schedules, temporary service constraints, or last-call conditions. Quick Update remains an inherited core capability, but this industry depends on it more heavily and requires especially clear current-state feedback and recovery.

### Event and atmosphere as first-class context

Live music, DJs, trivia, sports, tastings, release events, guest lists, private events, and similar programming may be as important as the drink catalog. Detailed operating and capability treatment is deferred to later approved RWPs.

### Late and irregular service periods

Operating days may cross midnight, vary by event, or include different access and service conditions during the same calendar day. Later work must avoid assuming that a business day ends at midnight.

### Distinct screen-purpose mix

A venue may combine beverage menus, rotating tap lists, promotional screens, event schedules, entry or cover information, wayfinding, and atmosphere-led displays. The profile does not presume that every screen is a menu board.

## Industry boundary

### Included as native concepts

The profile supports beverage-led bars, pubs, sports bars, cocktail bars, wine bars, guest-facing breweries, brewpubs, taprooms, lounges, nightclubs with beverage service, and related mixed concepts.

### Included through venue-level mixed-industry behavior

A venue may use this business type even when its parent organization has another primary industry. Examples include a hotel bar within a Hospitality organization, a dedicated bar venue within a Restaurant group, or a taproom operated by a broader manufacturing business.

### Outside the canonical boundary

The following are not native Bar, Brewery & Nightlife concepts unless an included on-premise service venue is also present:

- packaged beverage retail without meaningful on-premise service;
- beverage manufacturing or distribution with no guest-facing service venue;
- food-led restaurants whose bar is secondary to the Restaurant operating model;
- entertainment or dance venues whose operating identity is not beverage-led;
- membership organizations whose membership administration, rather than venue service, defines the primary product need.

These boundaries determine product defaults and profile selection only. They are not legal, licensing, tax, or regulatory classifications.

## Canonical venue subtypes

Subtype is venue-level product/domain configuration. It selects defaults, terminology candidates, starter-content suggestions, screen-purpose recommendations, and operational guidance. It is not a tier, entitlement, permission, usage allowance, rollout flag, legal classification, or substitute for the venue's real content.

A venue may remain **Unspecified / General Bar** when no supported subtype is clearly dominant. This is a neutral fallback state rather than a tenth commercial package.

| Primary subtype | Bounded definition and inclusion rule | Exclusion or neighboring-profile rule | Meaningful defaults and presentation differences |
| --- | --- | --- | --- |
| **Pub** | A casual, community-oriented beverage-led venue where beer, cider, spirits, conversation, recurring social activity, and optional food are central. Includes taverns and beverage-led gastropubs. | Use Restaurant when meal service and food discovery are the defining customer journey. Do not use Pub merely because a restaurant contains a bar. | Favor drink lists, house or rotating specials, recurring events, familiar categories, optional food menus, and clear available/unavailable updates. Tone may be approachable and local rather than highly technical. |
| **Sports Bar** | A beverage-led venue where watching scheduled sports across one or more viewing areas is a defining reason to visit. Food may be substantial but game-day programming and drink service remain central. | Use Pub when sports are incidental. Use Restaurant when food service is the dominant operating identity and sports viewing is secondary. | Favor fixture or event schedules, viewing-area or zone guidance, game-day offers, drink and food menus, and rapid event-state changes. Multiple physical displays do not create a separate capability or entitlement. |
| **Cocktail Bar** | A beverage-led venue centered on made-to-order cocktails, spirits, signature recipes, classics, seasonal lists, and bartender-led craft. Alcohol-free cocktail concepts may use this subtype when the operating model otherwise matches. | Use Lounge when seated atmosphere and reservation-led social service are more defining than the cocktail program. Use Restaurant when drinks support a food-led experience. | Favor signature/classic/seasonal groupings, concise ingredient or flavor guidance, spirit highlights, premium presentation, and unavailable-substitution clarity. Readability and price remain more important than decorative atmosphere on operational lists. |
| **Wine Bar** | A beverage-led venue centered on wine by the glass, bottle, tasting pour, or flight, often using region, producer, varietal, vintage, style, or pairing context. Food may be present but secondary. | Use Restaurant when the meal and pairing journey is primarily food-led. Packaged wine retail without meaningful on-premise service is outside the profile. | Favor by-glass/by-bottle organization, tasting flights, concise producer or origin context, current vintage where useful, and rapid sold-out or replacement handling. |
| **Brewery** | A guest-facing venue whose brewery production identity, house portfolio, releases, packaged product, tours, or producer story materially shapes the on-premise experience. A guest-facing service operation must exist. | Manufacturing-only, wholesale-only, or distribution-only operations are outside the profile. Use Taproom when service from the taps is dominant and production context is secondary. | Favor house portfolio, tap and packaged formats, release status, tours or events, take-home availability, and producer-led storytelling. Production data is not assumed to be automatically integrated. |
| **Brewpub** | A brewery-identified venue combining on-site or closely associated brewing with substantial prepared-food and meal service. Both the beverage program and restaurant operation are material. | Use Restaurant when food and meal discovery clearly dominate and brewery identity is secondary. Use Brewery or Taproom when food is limited or incidental. | Favor coordinated tap and food menus, pairing or service-period recommendations, releases, events, and both beverage and kitchen availability. It inherits Restaurant food capabilities without creating a special bundle. |
| **Taproom** | A beverage service venue centered on a rotating tap list, pours, flights, releases, and direct consumption from a producer or curated tap program, commonly with limited or partner-provided food. | Use Brewery when production identity, tours, packaged portfolio, and releases are broader than the service room. Use Pub when community tavern behavior is more defining than a tap-led catalog. | Favor current taps, pour sizes, flights, styles, strength or other approved descriptors, release state, keg-change availability, and optional food-source guidance. |
| **Nightclub** | A late-night venue where dancing, DJs, live entertainment, admission conditions, and beverage service together define the operating experience. Beverage service must be a material part of the venue model. | Use Entertainment & Attractions when beverage service is incidental or absent. Membership administration or ticketing complexity beyond venue communication remains outside this subtype decision. | Favor event lineup, start and door times, entry or cover information, venue zones, bar menus, safety or access guidance, and fast changes for delays, sell-outs, room moves, or cancellations. |
| **Lounge** | A beverage-led social venue characterized by seated service, atmosphere, conversation, reservations or table context, and a curated cocktail, wine, or premium beverage program. | Use Cocktail Bar when the cocktail catalog and bartender craft are dominant. Use Nightclub when dancing, high-volume entertainment, and admission operations dominate. | Favor concise curated lists, reservations or seating guidance where supported, events, table or area context, premium presentation, and low-light readability. |

## Hybrid and ambiguous concepts

Hybrid concepts are supported through one primary subtype plus optional descriptive operating traits. The traits tune recommendations and future terminology; they do not stack entitlements, increase limits, or create multiple commercial identities.

### Selection rules

1. Choose the subtype that best describes the venue's **dominant guest journey and daily operating rhythm**, not its building form, license, ownership structure, marketing phrase, or one occasional event.
2. When two models are materially equal, select the one that should control default terminology and first-run recommendations, then record the other as a descriptive trait.
3. When neither model clearly dominates, leave the subtype neutral rather than forcing a misleading choice.
4. Organization primary industry may seed the first suggestion but never overrides the venue's own subtype.
5. Subtype does not determine whether a capability is commercially available.

### Canonical ambiguous cases

- **Gastropub:** Pub when beverage-led community service and drink discovery dominate; Restaurant when meals and food discovery dominate.
- **Sports pub:** Sports Bar when fixtures, viewing zones, and game-day operations dominate; otherwise Pub with a sports trait.
- **Brewery taproom:** Brewery when producer identity, tours, releases, and packaged portfolio are central; Taproom when current taps, pours, and room service dominate.
- **Winery, cidery, meadery, or distillery tasting room:** supported as Wine Bar, Taproom, Cocktail Bar, or Brewery according to the actual guest-service model, with a producer/tasting-room trait. RWP-00.16 does not create separate entitlements or first-class subtype values for each producer category.
- **Alcohol-free bar or nightlife venue:** included when beverage-led service and the operating model match Cocktail Bar, Lounge, Pub, Sports Bar, or Nightclub. Alcohol sale is not required for product classification.
- **Private or membership club:** included only when the guest-facing beverage-service workflow fits this profile and membership administration does not define the primary product need.
- **Hotel, casino, resort, food hall, or entertainment-complex bar:** model the bar as a venue or outlet with the appropriate Bar subtype inside the mixed-industry organization. The parent property keeps its own primary industry.
- **Bottle shop with tasting area:** included only when the on-premise tasting/service operation is meaningful enough to require its own venue content and screens; otherwise remain outside this profile.

## Restaurant capability inheritance by subtype

Every subtype inherits the Restaurant baseline. The table records only where the inherited capability is emphasized or where starter recommendations differ.

| Subtype | Restaurant capabilities most visibly inherited | Additional emphasis, not a separate capability |
| --- | --- | --- |
| Pub | menu and category editing, food labels where used, Quick Update, hours, screens, publishing | rotating drinks, recurring events, house specials, optional food menu |
| Sports Bar | food and drink menus, promotions, scheduling candidates, multi-screen targeting | game schedule, viewing-zone guidance, game-day state and offers |
| Cocktail Bar | item descriptions, prices, images where useful, availability, themes | signature lists, ingredients/flavor cues, seasonal rotation, substitutions |
| Wine Bar | categories, descriptions, prices, labels, availability | by-glass/by-bottle views, flights, producer/origin context, vintage where useful |
| Brewery | menus, availability, events, venue information, publishing | house portfolio, releases, packaged formats, tours, take-home status |
| Brewpub | full food-menu and beverage-menu inheritance, dayparts where used, availability | coordinated tap/food experience, releases, kitchen and beverage state |
| Taproom | menus, Quick Update, events, hours, explicit screen targeting | live tap list, pours, flights, keg changes, limited food-source guidance |
| Nightclub | events, promotions, hours, venue information, targeting, emergency communication | door/entry information, lineup, zones, late-night state changes |
| Lounge | curated menus, reservations candidate where later approved, events, themes | table/area context, premium lists, atmosphere with low-light clarity |

No subtype automatically receives scheduling, reservations, identity, analytics, AI, integration, advanced themes, hardware, or any other candidate capability. Those capabilities keep their independent Track 0 classification and later packaging decision.

## Organization, venue, selection, and change behavior

### Organization primary industry

- An organization may select Bar, Brewery & Nightlife as its primary industry.
- Primary industry seeds organization-level neutral terminology, recommendations, starter content, and first-venue suggestions.
- It does not force every venue to use the same subtype.
- Changing primary industry must not silently add, remove, or reprice commercial access.

### Venue subtype selection

- Each venue selects its own primary subtype or remains neutral.
- A venue may record a small set of descriptive traits for hybrid handling; traits must not be used as hidden feature flags.
- Selection changes venue-local defaults, future terminology suggestions, starter-content recommendations, screen-purpose suggestions, and guidance only.
- Organization-wide surfaces use neutral language when venues differ.
- Selection must explain that all existing customer content and commercial access remain intact.

### Venue subtype change

A later implementation must treat subtype change as a deliberate product-state update:

- preserve menus, items, images, prices, screens, targeting, schedules, themes, publication history, and custom terminology;
- preview which defaults, suggestions, and future starter content would change;
- never overwrite existing customer-authored content automatically;
- require explicit confirmation from an authorized user;
- record enough change history for support and future analytics interpretation;
- provide safe cancellation before apply and a clear way to restore the prior subtype configuration;
- avoid implying that the change upgrades, downgrades, unlocks, or removes paid functionality.

### Mixed organizations and multi-venue operators

- Different venues in one organization may use different industries and subtypes.
- Shared users, brand assets, content libraries, analytics, and commercial access remain organization concerns unless later policy explicitly defines venue scope.
- Venue-specific labels, recommendations, and defaults remain local.
- Aggregate views use neutral terms such as venue, content, item, event, and screen when subtype-specific language would be misleading.
- Copying content between unlike subtypes must preserve the source content and allow the destination venue to review terminology and presentation rather than silently transforming it.

## Screen-purpose guidance by subtype

The following are recommendation inputs, not entitlements or required screens:

- **Pub:** drinks, food where used, specials, recurring events, venue information.
- **Sports Bar:** game schedule, viewing zones, drinks, food, game-day offers, venue information.
- **Cocktail Bar:** signature cocktails, classics, seasonal lists, spirit highlights, events.
- **Wine Bar:** by-glass, by-bottle, flights, producer or region highlights, events.
- **Brewery:** tap and packaged portfolio, releases, tours, events, take-home information.
- **Brewpub:** tap list, food menu, pairings or specials, releases, events.
- **Taproom:** current taps, pour/flight options, releases, events, food-source guidance.
- **Nightclub:** event lineup, door/cover information, venue zones, bar menu, access and safety guidance.
- **Lounge:** curated beverage list, reservations or seating guidance where supported, events, venue information.

## Impeccable shape brief for subtype selection and change

The project-local Impeccable skill and `shape` playbook were consulted because subtype selection affects future onboarding and administration.

- **Job and audience:** an owner or authorized manager in Operate mode chooses the closest operating model during onboarding or venue settings, often while uncertain about overlapping terms.
- **Outcome and proof:** the user can compare bounded “best when” definitions, select one primary subtype or remain neutral, understand the venue-local defaults that will change, and see an explicit statement that plan access and existing content do not change.
- **Hierarchy and interaction:** show primary subtype choices before optional hybrid traits; prioritize dominant guest journey, example screen purposes, and changed defaults; keep legal, licensing, and marketing language out of the decision. A change flow previews effects, preserves content, requires confirmation, and supports restoration.
- **States and ranges:** support first-run with no selection, one current subtype, neutral fallback, ambiguous/hybrid classification, a multi-venue organization with different subtypes, permission-restricted viewing, validation failure, saved success, and safe cancellation or recovery. The bounded catalog is nine primary subtypes plus neutral state and optional traits.
- **Responsive and accessibility constraints:** choices must remain scannable on phone and desktop, work without color-only distinctions, use plain-language comparisons, expose selection and confirmation to keyboard and assistive technology, and preserve the approved Sky Blue administrative direction.

This brief is planning only. It authorizes no UI, API, schema, migration, or product implementation.

## Capability-matrix classification resulting from RWP-00.16

1. Venue subtype, neutral subtype state, and hybrid descriptive traits are **product/domain state**.
2. Subtype changes defaults, terminology candidates, starter recommendations, and capability presentation only.
3. Subtype never grants capability access, raises limits, changes permissions, or acts as a rollout flag.
4. All subtype-specific screen purposes are recommendations using inherited or later-classified capabilities, not new entitlements.
5. Counts of venues, screens, users, integrations, or content remain limits independent of subtype.

## Deferred to RWP-00.17 and later

- the canonical operator-facing and guest-facing glossary;
- exact subtype-specific labels and fallback wording;
- detailed event, entry, cover, reservation, tap, pour, flight, and service-period semantics;
- required and optional capability decisions;
- packaging, onboarding, dashboard, analytics, and implementation design.

## Reference anchors

These references informed the original profile boundary but do not replace Vennusign's product model:

- U.S. Census Bureau 2022 NAICS 722410 boundary for drinking places;
- Brewers Association distinction among brewpub and taproom brewery market segments.

They are boundary evidence only, not Vennusign entitlement, legal, licensing, tax, or regulatory classifications.

## RWP-00.15 completion summary

RWP-00.15 established the industry purpose, Restaurant inheritance, meaningful deltas, native boundary, organization and venue behavior, initial capability classifications, and Impeccable planning guardrails.

## RWP-00.16 completion and handoff

### Completed

- Defined nine bounded primary subtypes plus a neutral fallback.
- Established inclusion, exclusion, neighboring-profile, and ambiguous-case rules.
- Mapped every subtype to inherited Restaurant capabilities and meaningful deltas only.
- Defined subtype-specific operational, content, screen-purpose, and presentation recommendations.
- Resolved hybrid concepts through one primary subtype plus optional descriptive traits.
- Defined organization, venue, subtype selection, subtype change, mixed-organization, and multi-venue behavior.
- Resolved producer tasting rooms, alcohol-free concepts, private clubs, gastropubs, brewery taprooms, and mixed-property bars without creating separate entitlement models.
- Applied the Impeccable `shape` guidance to future subtype selection and change flows.
- Updated the Track 0 capability classification for venue subtype and hybrid traits.

### Not performed

- No product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, or pricing implementation.
- No integration or external-system testing.
- No canonical terminology glossary or detailed operational-capability design beyond what was required to distinguish subtypes.

### Next sequential RWP

**RWP-00.17 — Bar, Brewery & Nightlife Business Terminology** (#492) must define the canonical glossary, inherited and subtype-specific wording, operator versus guest language, neutral fallbacks, and hybrid terminology behavior before RWP-00.18 begins.
