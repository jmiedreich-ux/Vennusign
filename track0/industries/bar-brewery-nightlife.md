# Bar, Brewery & Nightlife Industry Profile

## Identity

- **Industry:** Bar, Brewery & Nightlife
- **RWP range:** RWP-00.15 through RWP-00.26
- **Current status:** Industry definition, venue subtypes, and business terminology complete
- **Baseline:** Restaurant
- **Current completed RWP:** RWP-00.17
- **Next sequential RWP:** RWP-00.18 — Operating Characteristics

## Purpose

This profile covers beverage-led venues whose daily customer experience depends on accurate drink information, rapid operational changes, venue atmosphere, and time-sensitive event or entertainment communication.

It inherits the complete Restaurant baseline. This document records only meaningful differences needed to guide subtype, terminology, operations, capability, packaging, onboarding, dashboard, analytics, and future implementation planning.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- keep drink, tap, bottle, can, cocktail, wine, and limited-release information current;
- communicate fast-changing availability, pricing periods, service periods, and venue conditions without rebuilding content;
- promote entertainment, events, sports, private functions, and other time-bound reasons to visit;
- support drink lists, tap lists, specials, event schedules, entry information, venue guidance, and other distinct screen purposes;
- maintain legible presentation across bright daytime service, dim evening environments, crowded spaces, and long viewing distances;
- coordinate a consistent brand while allowing venue-level operational differences across mixed concepts.

## Inherited unchanged from Restaurant

Unless a later RWP records a meaningful exception, this industry and every subtype inherit:

- content, category, item, price, description, image, and label management;
- manual availability and Quick Update;
- screen pairing, management, explicit targeting, preview, and immediate publishing;
- delivery confirmation, online/offline and outdated status, and restoration of a prior published version;
- basic layouts and themes;
- business hours and venue information;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, campaign, multi-screen, multi-venue, approval, history, analytics, identity, AI, hardware, and integration capabilities.

Food menus, kitchen-led service, dietary information, and Restaurant-style dayparts remain inherited where used. They are not assumed to dominate every subtype.

## Meaningful differences from Restaurant

### Beverage-led catalog emphasis

The primary content model emphasizes drinks, rapidly rotating products, serving formats, producer or style information, and temporary or limited availability. This changes defaults, terminology, starter content, and recommendations; it does not create a separate entitlement model.

### Higher operational volatility

Products and offers can change repeatedly during a service period because of keg changes, limited releases, depleted stock, changing entertainment schedules, temporary service constraints, or last-call conditions. Quick Update remains core and requires especially clear current-state feedback and recovery.

### Event and atmosphere as first-class context

Live music, DJs, trivia, sports, tastings, release events, guest lists, private events, and related programming may be as important as the drink catalog.

### Late and irregular service periods

Operating days may cross midnight, vary by event, or include different access and service conditions during one calendar day. Product language and future scheduling must not assume that a business day ends at midnight.

### Distinct screen-purpose mix

A venue may combine beverage menus, rotating tap lists, promotional screens, event schedules, entry or cover information, wayfinding, and atmosphere-led displays. Not every screen is a menu board.

## Industry boundary

### Included

Beverage-led bars, pubs, sports bars, cocktail bars, wine bars, guest-facing breweries, brewpubs, taprooms, lounges, nightclubs with material beverage service, and related mixed concepts.

### Mixed-industry use

A venue may use this profile when its parent organization has another primary industry, such as a hotel bar, a dedicated bar within a Restaurant group, or a taproom operated by a broader manufacturing business.

### Outside the canonical boundary

- packaged beverage retail without meaningful on-premise service;
- beverage manufacturing or distribution with no guest-facing venue;
- food-led restaurants whose bar is secondary;
- entertainment or dance venues whose operating identity is not beverage-led;
- membership organizations whose membership administration defines the primary product need.

These boundaries determine defaults and profile selection only. They are not legal, licensing, tax, or regulatory classifications.

## Canonical venue subtypes

Subtype is venue-level product/domain configuration. It selects defaults, terminology candidates, starter-content suggestions, screen-purpose recommendations, and guidance. It is not a tier, entitlement, permission, limit, rollout flag, legal classification, or substitute for real content.

A venue may remain **Unspecified / General Bar** when no supported subtype clearly dominates.

| Primary subtype | Bounded definition | Meaningful defaults and presentation differences |
| --- | --- | --- |
| **Pub** | Casual, community-oriented beverage-led venue with beer, cider, spirits, recurring social activity, and optional food. | Drinks, house or rotating specials, recurring events, optional food, approachable local tone. |
| **Sports Bar** | Beverage-led venue where watching scheduled sports across one or more viewing areas is a defining reason to visit. | Games or matches, viewing zones, game-day offers, drinks, food, rapid event-state changes. |
| **Cocktail Bar** | Venue centered on made-to-order cocktails, spirits, signature recipes, classics, seasonal lists, and bartender-led craft. | Signature, classic, and seasonal groupings; concise ingredients or flavor cues; premium but readable presentation. |
| **Wine Bar** | Venue centered on wine by the glass, bottle, tasting pour, or flight. | By-glass and by-bottle organization, flights, producer or origin context, vintage where useful. |
| **Brewery** | Guest-facing venue whose production identity, house portfolio, releases, packaged product, tours, or producer story shapes the experience. | House portfolio, tap and packaged formats, releases, tours, take-home availability. |
| **Brewpub** | Brewery-identified venue combining brewing with substantial prepared-food and meal service. | Coordinated tap and food menus, pairings, releases, kitchen and beverage availability. |
| **Taproom** | Beverage service venue centered on a rotating tap list, pours, flights, and releases. | Current taps, pour sizes, flights, styles, keg changes, optional food-source guidance. |
| **Nightclub** | Late-night venue where dancing, DJs or live entertainment, admission conditions, and beverage service together define the experience. | Event lineup, doors, entry information, cover, guest list, zones, bar menu, safety and access guidance. |
| **Lounge** | Beverage-led social venue characterized by seated service, atmosphere, reservations or table context, and a curated beverage program. | Curated lists, reservations, tables or seating areas, premium presentation, low-light readability. |

## Hybrid and ambiguous concepts

Hybrid concepts use one primary subtype plus optional descriptive traits. Traits tune recommendations and terminology; they do not stack entitlements, increase limits, or create multiple commercial identities.

Selection follows the venue's dominant guest journey and daily operating rhythm. When two models are materially equal, choose the one that should control default terminology and record the other as a trait. When neither dominates, keep the neutral subtype.

Canonical cases:

- **Gastropub:** Pub when beverage-led community service dominates; Restaurant when meals and food discovery dominate.
- **Sports pub:** Sports Bar when fixtures, viewing zones, and game-day operations dominate; otherwise Pub with a sports trait.
- **Brewery taproom:** Brewery when producer identity, tours, releases, and packaged portfolio dominate; Taproom when current taps and room service dominate.
- **Winery, cidery, meadery, or distillery tasting room:** select Wine Bar, Taproom, Cocktail Bar, or Brewery according to the actual guest-service model, with a producer or tasting-room trait.
- **Alcohol-free bar:** included when the beverage-led operating model matches a supported subtype.
- **Hotel, casino, resort, food hall, or entertainment-complex bar:** model the local outlet with the appropriate Bar subtype while the parent keeps its own industry.
- **Bottle shop with tasting area:** included only when on-premise service is meaningful enough to require venue content and screens.

## Canonical terminology model

### Language hierarchy

1. Use one stable neutral product noun for cross-industry or mixed-venue administration.
2. Use a subtype-preferred noun when the venue context is known and the term improves comprehension.
3. Preserve customer-authored names and labels.
4. Do not use terminology to imply capability access, price, permission, ownership, or legal status.
5. Do not alternate synonyms for literary variety inside one flow.

### Neutral organization-wide terms

Use these when one surface spans different industries, venue subtypes, or hybrid concepts:

- **organization** — the customer account or parent operating group;
- **venue** — the local business unit;
- **content** — the umbrella for menus, lists, events, notices, promotions, and guidance;
- **item** — the neutral operator-facing catalog object;
- **category** — a named grouping of items;
- **screen** — a paired display endpoint;
- **area** — a neutral physical or operational subdivision;
- **event** — a neutral scheduled program item;
- **service period** — a bounded operating interval;
- **special** — a time-bound promoted offer;
- **availability** — the current usable or sellable state;
- **publish** — make selected content current on selected screens;
- **restore** — return to a prior saved or published state.

### Canonical glossary

| Concept | Operator-facing canonical term | Guest-facing preferred language | Rules and ambiguity handling |
| --- | --- | --- | --- |
| Beverage catalog | **Drink menu** or neutral **menu** | Drink menu, drinks, cocktails, wine list, beer list, or tap list | Use menu as the general fallback. Use list when the content is primarily a current selection rather than a meal-style menu. |
| Catalog object | **Item** | Product name or drink, cocktail, wine, beer, cider, spirit, bottle, can, pour, or flight | Item is neutral administration language, not preferred guest copy. |
| Tap-based selection | **Tap list** | On tap, current taps, beers on tap | A tap is a serving source or list position, not the universal noun for the beverage item. |
| Serving amount | **Pour size** or **serve** | Pour, glass, half pour, tasting pour, measure, or named size | Preserve local units and responsible-display requirements; do not invent legal measures. |
| Grouped tasting | **Flight** | Flight or tasting flight | A flight is a grouped selection, not a separate capability or package. |
| Packaged format | **Bottle** or **can** | Bottle, can, take-home bottle, take-home can | Distinguish on-premise and take-home only when meaningful. |
| Cocktail grouping | **Cocktail list** | Signature cocktails, classics, seasonal cocktails, alcohol-free cocktails | Use actual customer language; do not assume every cocktail is alcoholic. |
| Wine format | **By the glass**, **by the bottle**, **tasting pour**, **flight** | Same | Producer, region, varietal, vintage, and style are descriptive fields, not separate capabilities. |
| Beer or producer release | **Release** | New release, limited release, seasonal release | Use product/domain state for release timing and availability. |
| Time-bound offer | **Special** | Happy hour, house special, featured drink, game-day offer, tasting, or release offer | Happy hour is a subtype/context label, not the universal object name. |
| Program item | **Event** | Game, match, live music, DJ set, trivia, tasting, release event, private event | Event is the neutral analytics and administration noun. |
| Admission charge | **Cover** | Cover, entry charge, admission | Cover means a charge only. Use entry information when charge, ticket, guest list, age, or access model is mixed or unknown. |
| Access timing | **Doors** or **door time** | Doors open, entry from, last entry | Door time is distinct from event start time and service hours. |
| Held booking | **Reservation** | Reservation, table reservation, area reservation | Use booking only where local convention or an integrated source requires it; do not alternate within a flow. |
| Guest-list access | **Guest list** | Guest list, list entry | Guest list is an access method, not a reservation or ticket by default. |
| Seating object | **Table** | Table | Keep table identity separate from reservation state and commercial access. |
| Physical subdivision | **Area** or **section** | Viewing area, room, patio, bar, table area, venue zone | Section is neutral only when the business already uses it. Prefer the known physical noun. |
| Sports location | **Viewing area** or **viewing zone** | Watch in, showing in, viewing area | A physical screen count does not create a separate feature. |
| Operating interval | **Service period** | Lunch, evening service, happy hour, late night, doors, event hours | Service period is operator language. Use recognizable guest labels. |
| End-of-service context | **Last call** | Last call or last orders | Do not infer legal timing. It is an operational message whose exact rules are defined locally. |
| Product state | **Available**, **unavailable**, **sold out** | Available, unavailable, sold out, back soon where known | Sold out means no sellable quantity now. Unavailable is broader. Do not promise return timing unless known. |
| Replacement | **Substitution** or **replacement item** | Ask about a substitute, replaced with, now serving | Never silently rewrite a guest-facing item after depletion. |
| Promotion grouping | **Featured content** or **promotion** | Featured drinks, tonight, this week, game-day offers | Promotion is a content purpose, not a capability classification by itself. |
| Venue schedule | **Hours** and **service periods** | Open, closes, kitchen hours, bar hours, doors, event hours | Keep venue hours, kitchen hours, door time, event start, and last entry distinct. |

### Subtype terminology preferences

| Subtype | Preferred catalog and operational terms | Neutral fallback when context is mixed |
| --- | --- | --- |
| Pub | drinks, food, house specials, recurring events, bar, dining area, patio | menu, items, specials, events, areas |
| Sports Bar | games or matches, viewing areas or zones, game-day offers, drinks, food, event schedule | events, areas, specials, menu |
| Cocktail Bar | cocktails, signature cocktails, classics, seasonal list, spirits, serves or measures, bar seating | drink menu, items, categories, areas |
| Wine Bar | wine list, by the glass, by the bottle, tasting pour, flight, producer, region, varietal, vintage | drink menu, items, serving options |
| Brewery | house beers, releases, tap list, packaged beer, cans, bottles, take-home, tours | drinks, items, events, formats |
| Brewpub | tap list, food menu, pairings, releases, kitchen availability, bar and dining areas | menu, items, specials, areas |
| Taproom | current taps, pour sizes, flights, releases, guest food or food partner, taproom | drink menu, items, serving options, venue |
| Nightclub | event lineup, doors, entry information, cover, guest list, rooms or venue zones, bar menu | events, access information, areas, menu |
| Lounge | curated drinks, cocktails, wine, reservations, tables, seating areas, events | menu, items, reservations, areas |
| Unspecified / General Bar | drinks, menu, specials, events, reservations where supported, areas, service periods | content, items, events, areas |

### Operator actions and state language

Future UI actions should use specific verb-object labels:

- Add drink
- Add category
- Add event
- Mark unavailable
- Mark sold out
- Make available
- Add pour size
- Create flight
- Publish drink menu
- Publish tap list
- Publish event lineup
- Preview selected screens
- Restore previous version
- Change venue subtype

State and feedback text must distinguish:

- first use from no results;
- unavailable from sold out;
- validation failure from publishing failure;
- permission restriction from commercial access;
- screen offline from content outdated;
- saved draft from published content;
- event canceled from event sold out;
- reservation unavailable from reservation capability unavailable.

### Analytics terminology

Use neutral stable dimensions for cross-industry reporting:

- organization;
- venue;
- subtype;
- content type;
- item;
- category;
- event;
- area;
- screen;
- service period;
- availability state;
- publish and delivery state.

Subtype-specific display labels may be applied in a venue-scoped view, but exported fields and cross-industry aggregate labels should remain neutral unless an approved analytics contract says otherwise.

## Terminology inheritance and customization

- Restaurant terms remain unchanged when their meaning is unchanged: organization, venue, menu, category, item, price, description, image, label, availability, special, screen, preview, publish, draft, published, online, offline, outdated, restore, user, role, and permission.
- Venue subtype may seed terminology defaults but must not overwrite customer-authored category names, item names, event names, area names, or custom labels.
- A future subtype change must preview terminology defaults that would change and preserve existing content.
- Hybrid venues choose terminology from the primary subtype and use neutral fallbacks for mixed concepts.
- Mixed-organization surfaces use neutral terms rather than switching labels row by row.
- External integrations may supply source terminology, but Vennusign must map it to a stable internal neutral concept and preserve the source label where useful.

## Impeccable clarify brief for future UI copy

The project-local Impeccable skill and `clarify` guidance were consulted because this terminology will appear in future onboarding, navigation, forms, editor labels, help text, analytics, and guest-facing screens.

- Keep one noun and one verb for the same concept throughout a flow.
- Use persistent labels; placeholders are examples, not labels.
- Make actions describe outcomes, not gestures.
- Errors explain what failed and how to recover without leading with internal codes.
- Empty states distinguish first use, no results, filters, permissions, and failure.
- Use complete translatable messages rather than concatenated fragments.
- Keep visible labels and accessible names aligned.
- Support long names, localization expansion, pluralization, dynamic values, keyboard access, assistive technology, and 200% zoom.
- Do not rely on color, punctuation, or icons alone to communicate state.
- Preserve the approved Sky Blue administrative direction.

This brief is planning only and authorizes no UI or implementation work.

## Capability-matrix classification resulting from RWP-00.17

1. Industry, subtype, hybrid traits, and terminology preference are **product/domain state**.
2. Terminology affects defaults, labels, starter recommendations, help text, and presentation only.
3. Terminology never grants capability access, raises limits, changes permissions, or acts as a rollout flag.
4. Availability, event, reservation, entry, service-period, and area values retain their own product-state classifications.
5. Manual editing, availability changes, publishing, delivery confirmation, and recovery remain core.
6. Automatic POS, inventory, tap-management, reservation, ticketing, or event synchronization remains a later integration-packaging question.

## Deferred to RWP-00.18 and later

- late-night business-day and service-period rules;
- happy-hour, rotating-tap, limited-release, and last-call operating behavior;
- table, bar, counter, and hybrid service models;
- age restriction and responsible-display considerations;
- entertainment, guest-list, cover, ticketing, reservation, and private-event operations;
- detailed required and optional capability decisions;
- packaging, onboarding, dashboard, analytics, and implementation design.

## RWP completion history

### RWP-00.15

Established the industry purpose, Restaurant inheritance, meaningful deltas, native boundary, organization and venue behavior, initial capability classifications, and Impeccable planning guardrails.

### RWP-00.16

Defined nine bounded primary subtypes plus a neutral fallback, hybrid and mixed-organization behavior, subtype selection and change rules, Restaurant inheritance, screen-purpose recommendations, and subtype classification as product/domain state.

### RWP-00.17

Defined the canonical glossary, inherited terminology, subtype-specific preferred terms, operator versus guest language, ambiguous-term rules, neutral mixed-organization fallbacks, hybrid behavior, analytics labels, action and state wording, and Impeccable clarification guidance.

No product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, analytics, localization, or integration implementation was performed.

## Next sequential RWP

**RWP-00.18 — Bar, Brewery & Nightlife Operating Characteristics** (#493) must define late-night hours and business-day behavior, service periods, happy hour, rotating taps, limited releases, last call, service models, responsible-display considerations, entertainment and event operations, reservations, guest lists, cover and ticketing considerations, inventory volatility, and subtype-specific operating differences.
