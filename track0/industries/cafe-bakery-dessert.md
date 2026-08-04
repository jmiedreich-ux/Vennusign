# Café, Bakery & Dessert Industry Profile

## Identity

- **Industry:** Café, Bakery & Dessert
- **RWP range:** RWP-00.27 through RWP-00.38
- **Current status:** Industry definition complete; subtype definition is next
- **Baseline:** Restaurant
- **Current RWP:** RWP-00.27
- **Next sequential RWP:** RWP-00.28 — Venue Subtypes

## Purpose

This profile covers guest-facing concepts centered on prepared nonalcoholic beverages, bakery products, desserts, snacks, and closely related counter-service experiences. Their daily customer experience depends on accurate product information, fast sold-out and batch updates, clear size or option presentation, service-period awareness, and reliable pickup or preorder communication.

It inherits the complete Restaurant baseline. This document records only the differences needed to establish the industry boundary and guide later subtype, terminology, operations, capability, packaging, onboarding, dashboard, and analytics RWPs.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- keep frequently changing beverages, baked goods, desserts, sizes, options, and seasonal products current;
- mark products sold out, available again, limited, or expected in a later batch without rebuilding content;
- present products clearly across menu boards, display-case lists, pickup areas, queue-facing screens, and promotional surfaces;
- communicate service periods, preorder or pickup instructions, temporary closures, and other time-sensitive operating information;
- support high-throughput counter service while maintaining legibility for guests making quick decisions from a distance;
- coordinate brand consistency while allowing venue-level differences across mixed café, bakery, dessert, restaurant, hospitality, and retail concepts.

## Inherited unchanged from Restaurant

Unless a later Café, Bakery & Dessert RWP records a meaningful exception, this industry inherits:

- content, category, item, price, description, image, and dietary-label management;
- manual availability and Quick Update;
- screen pairing, management, explicit targeting, preview, and immediate publishing;
- delivery confirmation, online/offline and outdated status, and recovery to a prior published version;
- basic layouts and themes;
- business hours and venue information;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, campaign, multi-screen, multi-venue, approval, history, analytics, identity, AI, hardware, and integration capabilities.

Restaurant-style meal menus, table service, kitchen-led ordering, and full-service workflows remain inherited where a venue actually uses them; they are not assumed to define the primary operating model.

## Meaningful differences from Restaurant

### Product, size, and option emphasis

The primary content model often emphasizes beverages, bakery products, desserts, portions, temperatures, sizes, flavors, milk or base choices, add-ons, and other customer-facing options. This changes defaults, terminology, starter content, and presentation guidance; it does not establish a separate entitlement model.

### Batch, freshness, and sell-out rhythm

Products may become available in batches, sell out during a service period, return later, or have a short freshness window. Manual availability remains an inherited core capability. Batch and freshness values are product/domain state, not commercial gates. Detailed state vocabulary and operating rules belong to RWP-00.29 and RWP-00.30.

### Counter-service speed and queue context

Guests often decide and order while moving through a queue. Menu hierarchy, option clarity, current availability, and pricing must be understandable quickly and at distance. Later UI-facing planning must avoid dense restaurant-menu assumptions when a smaller, rapidly scanned catalog is more appropriate.

### Preorder and pickup communication

Custom cakes, catering trays, baked-good orders, beverage pickup, and other preorder scenarios may be important. This profile requires clear presentation of instructions, cutoffs, availability, and collection information, but it does not define ordering, payment, fulfillment, or external integration behavior in this RWP.

### Retail and prepared-service overlap

A venue may sell packaged beans, tea, bottled drinks, merchandise, or take-home bakery products alongside prepared items. Guest-facing signage may therefore mix immediate-consumption menus, display-case content, packaged retail information, and promotions without treating the venue as a general retail store.

### Early, seasonal, and demand-driven service periods

Daily operation may begin very early, vary by weekday, and change around holidays, school or commuter patterns, weather, seasonal products, or production capacity. Detailed scheduling and analytics treatment is deferred to later RWPs.

## Content and screen-purpose differences

A Café, Bakery & Dessert venue may use a combination of:

- beverage or food menu boards;
- pastry, bakery, dessert, or display-case lists;
- seasonal and limited-product promotions;
- preorder, pickup, queue, or collection instructions;
- service-period, hours, temporary closure, or sell-out information;
- packaged retail or merchandise promotions;
- wayfinding and venue-information screens;
- atmosphere-led brand content where operational information remains clear.

The profile does not presume that every screen is a complete menu board or that every product is continuously available.

## Industry boundary

### Included as native concepts

The profile is intended to support concepts including:

- cafés and coffee shops;
- tea shops and nonalcoholic specialty-beverage bars;
- bakeries, patisseries, and bakery-cafés;
- doughnut, bagel, pretzel, cookie, and similar baked-specialty shops;
- dessert shops and dessert cafés;
- ice cream, gelato, frozen-yogurt, and related frozen-dessert shops;
- juice and smoothie bars;
- closely related hybrids where beverages, baked goods, desserts, or specialty snacks are a primary operating identity.

The exact supported subtype catalog, subtype definitions, and hybrid rules belong to RWP-00.28.

### Included through venue-level mixed-industry behavior

A venue may use this business type even when its parent organization has another primary industry. Examples include a hotel café within a Hospitality organization, a bakery counter within a Restaurant group, or a dessert venue operated alongside Entertainment & Attractions locations.

### Outside the canonical boundary

The following are not treated as native Café, Bakery & Dessert concepts unless an included guest-facing prepared-service venue is also present:

- meal-led restaurants whose defining experience is better represented by the Restaurant profile;
- mobile food or beverage operations better represented by Food Truck & Concession;
- packaged-food retail with no meaningful preparation or immediate-consumption service;
- grocery-store departments where broader grocery operations define the product need;
- commercial or industrial bakery manufacturing with no guest-facing retail venue;
- alcohol-led bars, taprooms, lounges, or nightlife concepts;
- general confectionery or specialty retail where prepared service and on-premise consumption are not meaningful.

These boundaries determine Vennusign defaults and profile selection only. They are not legal, licensing, tax, food-safety, or statistical classifications.

## Organization and venue behavior

### Organization primary industry

- An organization may select Café, Bakery & Dessert as its primary industry.
- Primary industry seeds organization-level terminology, recommendations, starter content, and first-venue setup.
- Primary industry is product/domain configuration, not a subscription entitlement.
- Changing primary industry must not silently add or remove commercial access.

### Venue business type

- Every venue may select its own business type and, later, a supported subtype.
- Venue business type controls local defaults, labels, screen-purpose recommendations, starter content, and operational guidance.
- Venue business type does not override organization-level entitlement authority.
- Changing a venue type must preserve existing customer content and require explicit review before defaults are replaced.

### Mixed organizations

- Café, Bakery & Dessert, Restaurant, Hospitality, Entertainment & Attractions, and other venue types may coexist within one organization.
- Shared libraries, brand controls, users, analytics, and commercial access remain organization concerns unless a later approved policy explicitly defines venue scope.
- Venue-specific terminology and defaults must remain local so one venue cannot make another venue's interface misleading.
- Organization-wide views must use neutral language when subtype-specific terms would be ambiguous.

## Initial capability-classification rules

RWP-00.27 establishes these rules for later detailed work:

1. Organization primary industry is **product/domain state** that selects defaults and recommendations.
2. Venue business type or subtype is **product/domain state** that selects venue-local defaults and terminology.
3. Rapid manual sell-out and availability changes remain an inherited **core capability** acting on product state.
4. Batch timing, freshness, limited quantity, and expected-return values are **product/domain state** when represented; they are not tier entitlements or rollout flags.
5. Automatic availability, order, inventory, or POS synchronization remains a future integration-packaging question and must not replace the core manual operation.

Detailed required, optional, packaging, onboarding, dashboard, and analytics classifications are intentionally deferred to their approved RWPs.

## Impeccable planning guardrails

RWP-00.27 is definition work rather than a detailed UI specification. The project-local Impeccable skill and its `shape` guidance establish these constraints for later UI-facing RWPs:

- **Operator surfaces use Operate mode:** prioritize rapid scanning, confident product-state changes, publishing feedback, and recovery over decorative expression.
- **Guest-facing operational screens use Read mode:** product name, current availability, price, size or option structure, service period, dietary information, and pickup instructions outrank promotional detail.
- **Experience mode is selective:** brand or seasonal promotions may be expressive, but must not obscure ordering facts, accessibility, or current-state information.
- **Hierarchy:** availability and sold-out state, product identity, price, meaningful size or option differences, and time-sensitive instructions must remain immediately understandable.
- **States:** later specifications must cover first-run, empty, active, limited, sold-out, available-again, next-batch, seasonal, preorder-open or closed, outdated, offline, permission-restricted, publish-failed, and recovery conditions where applicable.
- **Realistic ranges:** planning must account for short and long product names, zero to many options, small and large catalogs, price ranges, multilingual content, image and no-image cases, and both continuously available and batch-based products.
- **Responsive and environmental behavior:** later work must consider mobile use behind a counter, desktop administration, portrait and landscape displays, bright windows, queue viewing distances, rapid guest scanning, and crowded service environments.
- **Accessibility:** color alone must not communicate availability, freshness, dietary information, or status; text and hierarchy must remain legible under glare and at distance; motion must never delay access to essential facts.
- **Feedback and recovery:** high-frequency sold-out or available-again changes require clear confirmation, intended-screen targeting, delivery state, undo or restoration, and guidance when publishing does not reach a screen.
- **Visual direction:** preserve the approved Sky Blue direction for Vennusign administrative surfaces. Guest-facing themes may express the venue brand while maintaining the operational hierarchy above.

These guardrails shape planning only and authorize no UI implementation.

## Owner decisions and deferred questions

The following are intentionally carried into RWP-00.28 or later RWPs rather than decided here:

- the exact distinction among café, coffee shop, tea shop, bakery, patisserie, dessert shop, frozen-dessert shop, juice or smoothie bar, and bakery-café subtypes;
- whether bubble-tea concepts are a tea-shop subtype or a broader specialty-beverage subtype;
- when a meal-heavy bakery-café should use Restaurant rather than Café, Bakery & Dessert;
- how commercial bakeries with a guest-facing retail counter should be represented;
- whether chocolatiers, confectionery shops, and packaged-dessert retail belong here when prepared service is limited;
- whether custom-order businesses without routine walk-in service require a dedicated subtype or compatible-industry treatment;
- the default business type for a mixed organization when no single venue type is dominant;
- the neutral organization-wide term for products when venue subtypes use different vocabulary.

## Reference anchors

These references inform the boundary but do not replace Vennusign's product model:

- [U.S. Census Bureau 2022 NAICS 722515 — Snack and Nonalcoholic Beverage Bars](https://www.census.gov/naics/?details=722515&input=722515&year=2022) includes coffee shops, nonalcoholic beverage bars, ice cream parlors, juice bars, and several baked-specialty shops serving immediate consumption.
- [U.S. Census Bureau 2022 NAICS 311811 — Retail Bakeries](https://www.census.gov/naics/?details=311811&input=311811&year=2022) distinguishes retail bakeries producing on premises from commercial manufacturing and resale-only operations.

These references are used only as industry-boundary evidence. They do not define Vennusign entitlements, subtype eligibility, or legal obligations.

## Validation checklist

- [x] Restaurant inheritance is explicit.
- [x] Only meaningful deltas are documented.
- [x] Initial concerns have one primary classification.
- [x] Essential manual availability and publishing operations remain core.
- [x] Permissions, states, entitlements, add-ons, limits, and rollout flags remain separate.
- [x] Impeccable `shape` guidance was consulted for UI-facing planning.
- [x] Hierarchy, states, realistic ranges, accessibility, responsive behavior, feedback, and recovery are documented.
- [x] No product implementation was performed.
- [x] The next sequential RWP is identified as RWP-00.28.
