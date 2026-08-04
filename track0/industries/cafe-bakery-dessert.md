# Café, Bakery & Dessert Industry Profile

## Identity

- **Industry:** Café, Bakery & Dessert
- **RWP range:** RWP-00.27 through RWP-00.38
- **Current status:** Industry definition and venue-subtype model complete
- **Baseline:** Restaurant
- **Current completed RWP:** RWP-00.28
- **Next sequential RWP:** RWP-00.29 — Business Terminology

## Purpose

This profile covers guest-facing concepts centered on prepared nonalcoholic beverages, bakery products, desserts, snacks, and closely related counter-service experiences. Their daily customer experience depends on accurate product information, fast sold-out and batch updates, clear size or option presentation, service-period awareness, and reliable pickup or preorder communication.

It inherits the complete Restaurant baseline. This document records only the differences needed to establish the industry boundary and guide subtype, terminology, operations, capability, packaging, onboarding, dashboard, and analytics work.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- keep frequently changing beverages, baked goods, desserts, sizes, options, and seasonal products current;
- mark products sold out, available again, limited, or expected in a later batch without rebuilding content;
- present products clearly across menu boards, display-case lists, pickup areas, queue-facing screens, and promotional surfaces;
- communicate service periods, preorder or pickup instructions, temporary closures, and other time-sensitive operating information;
- support high-throughput counter service while maintaining legibility for guests making quick decisions from a distance;
- coordinate brand consistency while allowing venue-level differences across mixed café, bakery, dessert, restaurant, hospitality, and retail concepts.

## Inherited unchanged from Restaurant

Unless a later Café, Bakery & Dessert RWP records a meaningful exception, this industry and every subtype inherit:

- content, category, item, price, description, image, and dietary-label management;
- manual availability and Quick Update;
- screen pairing, management, explicit targeting, preview, and immediate publishing;
- delivery confirmation, online/offline and outdated status, and recovery to a prior published version;
- basic layouts and themes;
- business hours and venue information;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, campaign, multi-screen, multi-venue, approval, history, analytics, identity, AI, hardware, and integration capabilities.

Restaurant-style meal menus, table service, kitchen-led ordering, and full-service workflows remain inherited where a venue uses them. They are not assumed to define the primary operating model for every subtype.

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

The profile supports cafés, coffee shops, tea shops, nonalcoholic specialty-beverage bars, bakeries, patisseries, bakery-cafés, baked-specialty shops, dessert shops, frozen-dessert shops, juice and smoothie bars, and related mixed concepts.

### Included through venue-level mixed-industry behavior

A venue may use this business type even when its parent organization has another primary industry. Examples include a hotel café within a Hospitality organization, a bakery counter within a Restaurant group, or a dessert venue operated alongside Entertainment & Attractions locations.

### Outside the canonical boundary

The following are not native Café, Bakery & Dessert concepts unless an included guest-facing prepared-service venue is also present:

- meal-led restaurants whose defining experience is better represented by the Restaurant profile;
- mobile food or beverage operations better represented by Food Truck & Concession;
- packaged-food retail with no meaningful preparation or immediate-consumption service;
- grocery-store departments where broader grocery operations define the product need;
- commercial or industrial manufacturing with no guest-facing retail or service venue;
- alcohol-led bars, taprooms, lounges, or nightlife concepts;
- general confectionery or specialty retail where prepared service and on-premise consumption are not meaningful.

These boundaries determine Vennusign defaults and profile selection only. They are not legal, licensing, tax, food-safety, manufacturing, or statistical classifications.

## Canonical venue subtypes

Subtype is venue-level product/domain configuration. It selects defaults, terminology candidates, starter-content suggestions, screen-purpose recommendations, and operational guidance. It is not a tier, entitlement, permission, usage allowance, rollout flag, legal classification, or substitute for the venue's real content.

A venue may remain **Unspecified / General Café** when no supported subtype is clearly dominant. This is a neutral fallback state rather than a tenth commercial package.

| Primary subtype | Bounded definition and inclusion rule | Exclusion or neighboring-profile rule | Meaningful defaults and presentation differences |
| --- | --- | --- | --- |
| **Café** | A general counter-service or casual venue where prepared nonalcoholic beverages, light food, snacks, and social or work-friendly use are collectively important, with no narrower subtype clearly dominant. | Use Coffee Shop, Tea Shop, Bakery-Café, or another specific subtype when one product program defines the guest journey. Use Restaurant when meal service and food discovery dominate. | Favor balanced beverage and light-food menus, service periods, daily or seasonal specials, venue information, pickup guidance, and flexible menu-board layouts. |
| **Coffee Shop** | A beverage-led venue centered on espresso, brewed coffee, cold coffee, coffee-based drinks, and closely related additions, with optional pastries or light food. | Use Café when the beverage program is broad and coffee is not dominant. Use Bakery-Café when baked goods and light meals are equally material. | Favor hot/iced structure, sizes, milk or base choices, add-ons, seasonal drinks, brew methods where useful, rapid sold-out handling, and queue-readable pricing. |
| **Tea Shop** | A beverage-led venue centered on brewed tea, milk tea, fruit tea, bubble tea, matcha, tea-based specialties, or tea service. Bubble-tea concepts are included when tea or tea-style specialty beverages define the operation. | Use Café when tea is one part of a broad offering. Use Juice & Smoothie Bar when fresh fruit, blended drinks, or bowls dominate. | Favor hot/iced choices, sweetness and ice options where relevant, sizes, toppings, tea base or style, seasonal drinks, and clear option hierarchy without overwhelming the primary product list. |
| **Bakery** | A guest-facing venue centered on bread, rolls, bagels, doughnuts, cookies, pastries, or other baked goods produced on site or for the venue, commonly using daily batches and display-case service. | Manufacturing-only and wholesale-only operations are outside the profile. Use Bakery-Café when beverages and light meals are equally central; use Restaurant when meals dominate. | Favor today's selection, batch or next-batch state, display-case grouping, sold-out and available-again changes, preorder or pickup instructions, and take-home quantities. |
| **Patisserie** | A guest-facing bakery or dessert venue centered on crafted pastries, cakes, tarts, entremets, chocolates, or premium small-format desserts, often with custom or preorder work. | Use Bakery when everyday bread and broad baked-goods production dominate. Use Dessert Shop when prepared-to-order dessert service is more central than pastry craft or production. | Favor collection-led presentation, concise flavor and size information, custom-order or preorder guidance, seasonal collections, limited availability, and premium imagery without weakening price or pickup clarity. |
| **Bakery-Café** | A hybrid venue where baked goods, prepared beverages, and light meals are all material to the guest journey and daily operating rhythm. | Use Restaurant when meal service, kitchen-led ordering, and food discovery clearly dominate. Use Bakery when beverage and meal service are secondary. | Favor coordinated bakery, beverage, breakfast, lunch, and seasonal sections; daypart-aware guidance; display-case and menu-board alignment; and both production and counter-service availability. It inherits Restaurant capabilities without creating a special bundle. |
| **Dessert Shop** | A venue centered on prepared desserts or sweets such as cakes, crepes, waffles, cookies, brownies, puddings, churros, confections, or plated and takeaway dessert combinations, excluding concepts primarily defined by frozen dessert. | Use Patisserie when pastry craft and produced collections dominate. Use Frozen Dessert Shop when ice cream, gelato, frozen yogurt, shaved ice, or another frozen base defines the visit. Packaged confectionery retail without meaningful prepared service is outside the profile. | Favor dessert categories, portions, flavors, toppings, combinations, seasonal or limited items, made-to-order timing where useful, and strong visual presentation with operational clarity. |
| **Frozen Dessert Shop** | A venue centered on ice cream, gelato, frozen yogurt, custard, sorbet, shaved ice, or related frozen-dessert service, including scoop, cup, cone, size, flavor, topping, and take-home formats. | Use Dessert Shop when frozen products are secondary. Packaged frozen retail without meaningful service is outside the profile. | Favor current flavors, format and size choices, cones or vessels, toppings, limited or rotating availability, take-home options, allergy or dietary guidance, and fast queue scanning. |
| **Juice & Smoothie Bar** | A beverage-led venue centered on fresh juice, smoothies, blended drinks, wellness-style beverages, bowls, and related add-ins or bases. | Use Café when coffee, tea, and light food are equally central. Use Restaurant when meal bowls and food service dominate. Mobile-only concepts use Food Truck & Concession. | Favor base, size, ingredients, add-ins, dietary or allergen cues, made-to-order availability, seasonal produce, bowls where used, and pickup or queue guidance. |

## Hybrid and ambiguous concepts

Hybrid concepts are supported through one primary subtype plus optional descriptive operating traits. Traits tune recommendations and future terminology; they do not stack entitlements, increase limits, or create multiple commercial identities.

### Selection rules

1. Choose the subtype that best describes the venue's **dominant guest journey and daily operating rhythm**, not its legal entity, production license, ownership structure, building form, marketing phrase, or one occasional product line.
2. When two models are materially equal, select the one that should control default terminology and first-run recommendations, then record the other as a descriptive trait.
3. When neither model clearly dominates, use Café or remain neutral rather than forcing a misleading narrow subtype.
4. Organization primary industry may seed the first suggestion but never overrides the venue's own subtype.
5. Subtype does not determine whether a capability is commercially available.

### Canonical ambiguous cases

- **Bubble-tea shop:** Tea Shop when tea, milk tea, fruit tea, and topping-based beverage service dominate; Juice & Smoothie Bar when fresh blended fruit and wellness-style drinks dominate.
- **Doughnut, bagel, pretzel, cookie, or similar specialty shop:** Bakery when baked production, batches, and display-case retail dominate; Café when beverages and mixed counter service are equally important; Dessert Shop when made-to-order dessert combinations define the visit.
- **Commercial bakery with a guest-facing retail counter:** Bakery when the retail or service counter is a meaningful venue with its own content and screens. Manufacturing, wholesale, or distribution operations remain outside the profile.
- **Meal-heavy bakery-café:** Bakery-Café when bakery, beverages, and light meals are co-equal; Restaurant when meal discovery, kitchen operations, or table/full-service behavior dominate.
- **Custom-order cake studio or pickup-only patisserie:** Patisserie or Bakery when there is a routine guest-facing consultation, display, collection, or pickup venue. A production-only facility with no guest-facing operation remains outside the profile.
- **Chocolatier or confectionery shop:** Patisserie or Dessert Shop when prepared service, made-on-site collections, tasting, or immediate consumption is meaningful; otherwise remain specialty retail outside this profile.
- **Hotel, resort, casino, food hall, campus, airport, or entertainment-property outlet:** model the outlet as a venue with the appropriate Café subtype inside the mixed-industry organization. The parent property keeps its own primary industry.
- **Mobile coffee, dessert, or smoothie operation:** use Food Truck & Concession when mobility, event stops, or temporary location is the defining operating model, with Café-related descriptive traits if later supported.
- **Preorder-led business with limited walk-in trade:** use Bakery or Patisserie when a guest-facing pickup or consultation venue exists; preorder volume alone does not create a separate entitlement or subtype.

## Restaurant capability inheritance by subtype

Every subtype inherits the Restaurant baseline. The table records only where the inherited capability is emphasized or where starter recommendations differ.

| Subtype | Restaurant capabilities most visibly inherited | Additional emphasis, not a separate capability |
| --- | --- | --- |
| Café | menus, categories, prices, images, availability, hours, screens, publishing | balanced beverage/light-food offering, flexible service periods, venue information |
| Coffee Shop | menus, modifiers, prices, Quick Update, pickup guidance, themes | drink sizes, temperature, milk or base choices, seasonal drinks, brew methods |
| Tea Shop | menus, options, prices, availability, dietary labels where used | tea base/style, sweetness, ice, toppings, seasonal or specialty beverages |
| Bakery | categories, descriptions, prices, availability, preorders candidate, publishing | batches, display-case selection, next-batch or sold-out state, take-home quantity |
| Patisserie | items, images, descriptions, prices, availability, venue information | collections, custom-order guidance, premium presentation, limited seasonal inventory |
| Bakery-Café | full food and beverage menu inheritance, dayparts where used, availability | coordinated bakery/beverage/light-meal experience and production/service state |
| Dessert Shop | menus, modifiers, images, prices, availability, promotions | flavors, portions, toppings, combinations, made-to-order timing |
| Frozen Dessert Shop | menus, options, prices, dietary labels, Quick Update | current flavors, sizes, vessels, toppings, limited rotation, take-home formats |
| Juice & Smoothie Bar | menus, options, prices, dietary labels, pickup guidance | bases, ingredients, add-ins, bowls, seasonal produce, made-to-order state |

No subtype automatically receives scheduling, ordering, payments, pickup automation, production management, inventory, identity, analytics, AI, integrations, advanced themes, hardware, or any other candidate capability. Those capabilities keep their independent Track 0 classification and later packaging decision.

## Organization, venue, selection, and change behavior

### Organization primary industry

- An organization may select Café, Bakery & Dessert as its primary industry.
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

- preserve menus, items, images, prices, options, dietary labels, screens, targeting, schedules, themes, publication history, and custom terminology;
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
- Aggregate views use neutral terms such as venue, content, product, item, category, service period, and screen when subtype-specific language would be misleading.
- Copying content between unlike subtypes must preserve the source content and allow the destination venue to review terminology and presentation rather than silently transforming it.

## Screen-purpose guidance by subtype

The following are recommendation inputs, not entitlements or required screens:

- **Café:** beverages, light food, seasonal items, service periods, pickup, venue information.
- **Coffee Shop:** hot and iced drinks, sizes and options, seasonal drinks, pastries or light food, pickup.
- **Tea Shop:** tea and specialty beverages, sizes and options, toppings, seasonal drinks, pickup.
- **Bakery:** today's selection, display-case list, bread or pastry categories, batches, preorders, pickup.
- **Patisserie:** pastry or cake collections, custom orders, seasonal ranges, pickup and collection guidance.
- **Bakery-Café:** bakery case, beverage menu, breakfast or lunch, specials, service periods, pickup.
- **Dessert Shop:** dessert menu, flavors and toppings, combinations, seasonal promotions, queue guidance.
- **Frozen Dessert Shop:** current flavors, sizes, cones or vessels, toppings, take-home formats, queue guidance.
- **Juice & Smoothie Bar:** drinks, bases and add-ins, bowls, dietary cues, seasonal produce, pickup.

## Impeccable shape brief for subtype selection and change

The project-local Impeccable skill and `shape` playbook were consulted because subtype selection affects future onboarding and administration.

Because this is a non-interactive planning run, assumptions are explicit: the user is an owner or authorized manager, selection is venue-local, overlap between concepts is common, and preservation of existing content is mandatory.

- **Job and audience:** an owner or authorized manager in Operate mode chooses the closest operating model during onboarding or venue settings, often while uncertain about overlapping café, bakery, beverage, and dessert concepts.
- **Outcome and proof:** the user can compare bounded “best when” definitions, select one primary subtype or remain neutral, understand the venue-local defaults that will change, and see an explicit statement that plan access and existing content do not change.
- **Selected direction:** use the established Vennusign administrative visual world and approved Sky Blue direction. Present primary subtype choices before optional hybrid traits; use dominant guest journey, example products, screen purposes, and changed defaults as the comparison evidence.
- **Scope and boundaries:** planning covers subtype selection, review, change preview, confirmation, cancellation, success, permission restriction, validation failure, and restoration. It does not design pricing, entitlement, billing, schema, or implementation behavior.
- **States and ranges:** support first-run with no selection, one current subtype, neutral fallback, ambiguous/hybrid classification, a multi-venue organization with different subtypes, permission-restricted viewing, validation failure, saved success, and safe cancellation or recovery. The bounded catalog is nine primary subtypes plus neutral state and optional traits.
- **Interaction and layout:** keep comparisons scannable on phone and desktop; reveal detail progressively; preview changed defaults before confirmation; preserve content; provide visible feedback; avoid color-only distinctions; expose selection and confirmation to keyboard and assistive technology.
- **Constraints and open decisions:** canonical operator and guest terminology belongs to RWP-00.29. A builder must not invent new subtype values, commercial consequences, automatic content transformation, or hidden trait-based feature gates.

This brief is planning only. It authorizes no UI, API, schema, migration, or product implementation.

## Capability-matrix classification resulting from RWP-00.28

1. Venue subtype, neutral subtype state, and hybrid descriptive traits are **product/domain state**.
2. Subtype changes defaults, terminology candidates, starter recommendations, and capability presentation only.
3. Subtype never grants capability access, raises limits, changes permissions, or acts as a rollout flag.
4. All subtype-specific screen purposes are recommendations using inherited or later-classified capabilities, not new entitlements.
5. Counts of venues, screens, users, integrations, content, storage, history, or AI consumption remain limits independent of subtype.
6. Batch, freshness, limited-quantity, expected-return, preorder, pickup, and production values keep their own product-state or later integration classifications independent of subtype.

## Deferred to RWP-00.29 and later

- the canonical operator-facing and guest-facing glossary;
- exact subtype-specific labels and neutral fallback wording;
- detailed batch, freshness, availability, preorder, pickup, size, modifier, and service-period semantics;
- required and optional capability decisions;
- packaging, onboarding, dashboard, analytics, and implementation design.

## Reference anchors

These references informed the original profile boundary but do not replace Vennusign's product model:

- U.S. Census Bureau 2022 NAICS 722515 boundary for snack and nonalcoholic beverage bars;
- U.S. Census Bureau 2022 NAICS 311811 distinction for retail bakeries.

They are boundary evidence only, not Vennusign entitlement, subtype, legal, licensing, tax, food-safety, or regulatory classifications.

## RWP-00.27 completion summary

RWP-00.27 established the industry purpose, Restaurant inheritance, meaningful deltas, native boundary, organization and venue behavior, initial capability classifications, and Impeccable planning guardrails.

## RWP-00.28 completion and handoff

### Completed

- Defined nine bounded primary subtypes plus a neutral fallback.
- Established inclusion, exclusion, neighboring-profile, and ambiguous-case rules.
- Mapped every subtype to inherited Restaurant capabilities and meaningful deltas only.
- Defined subtype-specific operational, content, screen-purpose, and presentation recommendations.
- Resolved hybrid concepts through one primary subtype plus optional descriptive traits.
- Defined organization, venue, subtype selection, subtype change, mixed-organization, and multi-venue behavior.
- Resolved bubble-tea shops, specialty baked-good shops, retail-counter bakeries, meal-heavy bakery-cafés, custom-order studios, confectionery concepts, mobile concepts, and mixed-property outlets without creating separate entitlement models.
- Applied the Impeccable `shape` guidance to future subtype selection and change flows.
- Updated the Track 0 capability classification for venue subtype and hybrid traits.

### Not performed

- No product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, pricing, ordering, payment, production, inventory, or integration implementation.
- No integration or external-system testing.
- No canonical terminology glossary or detailed operational-capability design beyond what was required to distinguish subtypes.

### Next sequential RWP

**RWP-00.29 — Café, Bakery & Dessert Business Terminology** (#504) must define canonical operator and guest terminology for products, sizes, modifiers, batches, freshness, availability, preorders, pickup, and service periods; identify Restaurant inheritance, subtype overrides, and hybrid fallbacks; keep terminology separate from permissions and entitlements; and hand off to RWP-00.30.
