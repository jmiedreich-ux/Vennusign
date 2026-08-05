# Café, Bakery & Dessert Industry Profile

## Identity

- **Industry:** Café, Bakery & Dessert
- **RWP range:** RWP-00.27 through RWP-00.38
- **Current status:** Industry definition, venue-subtype model, and business terminology complete
- **Baseline:** Restaurant
- **Current completed RWP:** RWP-00.29
- **Next sequential RWP:** RWP-00.30 — Operating Characteristics

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

Products may become available in batches, sell out during a service period, return later, or have a short freshness window. Manual availability remains an inherited core capability. Batch and freshness values are product/domain state, not commercial gates. Canonical vocabulary is defined below; detailed operating rules belong to RWP-00.30.

### Counter-service speed and queue context

Guests often decide and order while moving through a queue. Menu hierarchy, option clarity, current availability, and pricing must be understandable quickly and at distance. Later UI-facing planning must avoid dense restaurant-menu assumptions when a smaller, rapidly scanned catalog is more appropriate.

### Preorder and pickup communication

Custom cakes, catering trays, baked-good orders, beverage pickup, and other preorder scenarios may be important. This profile requires clear presentation of instructions, cutoffs, availability, and collection information, but it does not define ordering, payment, fulfillment, or external integration behavior in this RWP.

### Retail and prepared-service overlap

A venue may sell packaged beans, tea, bottled drinks, merchandise, or take-home bakery products alongside prepared items. Guest-facing signage may therefore mix immediate-consumption menus, display-case content, packaged retail information, and promotions without treating the venue as a general retail store.

### Early, seasonal, and demand-driven service periods

Daily operation may begin very early, vary by weekday, and change around holidays, school or commuter patterns, weather, seasonal products, or production capacity. Detailed scheduling and analytics treatment is deferred to later RWPs.

## Canonical business terminology

Terminology is UI-facing product/domain configuration. It selects default labels, starter recommendations, help text, analytics presentation, and guest wording. It does not grant capabilities, alter permissions, increase limits, control rollout, change commercial access, or authorize ordering and fulfillment implementation.

### Language layers and fallback rules

- **Operator language** names stable product objects and actions used across setup, editing, Quick Update, publishing, support, and analytics.
- **Guest language** uses the clearest known product or service noun for the venue and immediate context.
- **Organization-wide language** remains neutral when venues use different industries or subtypes.
- **Customer-authored names** remain authoritative. A subtype or profile change may suggest alternatives but must not silently rename existing content.
- **Unknown operational facts** remain unknown. Copy must not invent quantity, freshness, production time, expected return, pickup readiness, or preorder acceptance.

### Restaurant terms inherited unchanged

Content, category, item, price, description, image, dietary label, screen, preview, publish, restore, venue, availability, business hours, and service period remain valid neutral operator terms. Menu remains appropriate when the content is a menu. Restaurant ordering, table-service, and kitchen nouns are used only where the venue actually operates those models.

### Canonical glossary

| Concept | Canonical operator term | Preferred guest-facing treatment | Boundary and usage rule |
| --- | --- | --- | --- |
| Cross-industry body of managed material | **Content** | Use the known menu, list, case, collection, promotion, or information name | Content is the neutral umbrella; it is not a guest label when a clearer noun exists. |
| Presented product offering | **Menu** or the known content name | Drink menu, coffee menu, tea menu, bakery case, today's selection, dessert menu, current flavors, juice and smoothie menu | Do not force menu onto every display-case, flavor, pickup, retail, or information surface. |
| Sellable or displayable product object | **Item** | Use the product name or a known noun such as drink, coffee, tea, pastry, bread, cake, dessert, flavor, smoothie, bowl, or take-home item | Product may be used in explanatory copy; item remains the stable neutral operator object. |
| Group of related items | **Category** | Section, collection, case group, menu group, or flavor group when clearer | Collection may describe a curated product range; it must not be confused with order pickup in the same flow. |
| Quantity or format choice | **Size** | Use the venue's actual size name or measured amount | Portion, serving, scoop, slice, cup, cone, pack, and take-home format are used only when they describe the actual product format. |
| Selectable item choice | **Option** | Name the actual choice, such as size, temperature, milk choice, sweetness, ice, flavor, topping, or add-in | Modifier is acceptable operator or integration language but should not be the default guest label. |
| Hot, iced, frozen, or similar preparation choice | **Temperature** | Hot, iced, frozen, or venue-authored labels | Do not combine temperature with size or flavor when guests must understand them separately. |
| Liquid, milk, tea, juice, or bowl foundation | **Milk choice** or **base** | Use the actual choice label | Use milk choice only for milk choices; base is the neutral term for broader beverage or bowl foundations. |
| Primary taste or named variety | **Flavor** | Use the authored flavor name | Flavor is not a universal substitute for ingredient, tea style, coffee roast, syrup, filling, or topping. |
| Added ingredient mixed into or served with an item | **Add-in** | Use the actual add-in name | Add-on may imply commercial packaging; use add-in for product configuration. |
| Added finishing ingredient | **Topping** | Use the actual topping name | Topping and add-in remain distinct where preparation or display depends on the difference. |
| Produced group made available together | **Batch** | Next batch, fresh batch, or available again when known | Batch is product/domain state; do not infer production time or freshness. |
| Product-age or production guidance | **Freshness guidance** | Use venue-authored claims such as baked today only when authoritative | Vennusign must not invent freshness, shelf life, safety, or production claims. |
| Current ability to offer an item | **Availability** | Available, unavailable, sold out, limited, next batch, or available again as applicable | State words are not feature flags or commercial-access labels. |
| Finite or time-bounded supply | **Limited** | Limited quantity, limited today, seasonal, or while available when authored | Limited does not imply a known remaining count and must not be used to create false urgency. |
| Known future return | **Expected return** | Next batch at, available again at, or returns on when known | Never display a time or date that is not authoritative. |
| Request made before pickup or fulfillment | **Preorder** | Preorder available, preorder by, preorder closed, or authored instructions | This terminology does not authorize order capture, payment, production, or fulfillment implementation. |
| Guest-specific produced request | **Custom order** | Custom cake, custom pastry, catering tray, or authored service name | Use only when guest-specific configuration or production is actually supported. |
| Guest collection of prepared goods | **Pickup** | Pickup, pickup window, pickup instructions, pickup area | Collection may replace pickup only where established venue language requires it; do not alternate both in one flow. |
| Bounded operating interval | **Service period** | Morning, breakfast, lunch, afternoon, evening, late night, pickup hours, or authored names | Service period is neutral operator language; guest labels should be recognizable. |
| Promoted or time-bound item or offer | **Special** | Seasonal drink, daily special, featured pastry, limited flavor, collection, or release when context supports it | Special is presentation and content state, not a separate capability or entitlement. |
| Visible products in a physical merchandising area | **Display case** or **today's selection** | Use the actual case or selection name | A case list is not assumed to be a complete inventory count or continuously available menu. |

### Availability and timing distinctions

- **Available:** the item can currently be offered through the represented service context.
- **Unavailable:** the item cannot currently be offered and the more specific reason is unknown, not communicated, or not depletion.
- **Sold out:** the currently sellable quantity or current batch is exhausted.
- **Limited:** quantity or offer duration is intentionally constrained; it does not imply a known remaining count.
- **Next batch:** another produced group is expected. Include a time only when it is known and authoritative.
- **Available again:** an expected return is known. Do not invent timing.
- **Preorder available / preorder closed:** describes the preorder window only, not general item availability.
- **Pickup available / pickup paused:** describes the represented pickup channel or instructions only.

Sold out, unavailable, and preorder closed are not interchangeable. Unknown state must remain unknown.

### Operator actions and state labels

Future operator-facing language should prefer explicit verb-object actions such as:

- `Add item`;
- `Add size`;
- `Add option`;
- `Mark sold out`;
- `Mark available`;
- `Mark unavailable`;
- `Add next-batch time`;
- `Set expected return`;
- `Add preorder instructions`;
- `Add pickup instructions`;
- `Publish menu` or the known subtype-specific content name;
- `Restore previous version`.

Use menu-specific verbs only when the object is truly a menu. Do not label an action `Update`, `Manage`, `Submit`, or `Save changes` when a more specific outcome is known.

### Subtype terminology preferences

| Subtype | Preferred context terms | Terms that must remain conditional |
| --- | --- | --- |
| **Café** | menu, drinks, food, daily or seasonal specials, service periods, pickup, venue information | breakfast, lunch, table service, or bakery-case language only when used |
| **Coffee Shop** | coffee menu, espresso drinks, brewed coffee, hot or iced, size, milk choice, extra shot, flavor or syrup, seasonal drinks, pastries | roast, origin, brew method, or pickup state only when supported |
| **Tea Shop** | tea menu, tea base or style, hot or iced, size, sweetness, ice, toppings, add-ins, seasonal drinks | bubble tea, matcha, milk tea, fruit tea, or tea-service terms only when applicable |
| **Bakery** | today's selection, bakery case or display case, bread, pastry, baked goods, batch, next batch, sold out, preorder, pickup | fresh, baked today, remaining quantity, or next-batch time only when authoritative |
| **Patisserie** | pastry or cake collection, flavor, size or servings where known, custom order, preorder, pickup, seasonal collection, limited availability | collection must not mean both product range and pickup in one flow |
| **Bakery-Café** | bakery case, beverage menu, breakfast or lunch where used, service period, daily specials, pickup, available and sold-out state | Restaurant meal and table-service language only when the venue operates those models |
| **Dessert Shop** | dessert menu, portion or format, flavor, toppings, add-ins, combinations, made to order, seasonal or limited item | production time, pickup readiness, or quantity only when known |
| **Frozen Dessert Shop** | current flavors, scoop or serving format, size, cup or cone, toppings, take-home, limited or rotating flavor | scoop, cone, cup, self-serve, or take-home terms only when offered |
| **Juice & Smoothie Bar** | juice and smoothie menu, size, base, ingredients, add-ins, boosts where locally used, bowls, seasonal produce, pickup | wellness, nutrition, fresh, or functional claims only when authored and supported |
| **Unspecified / General Café** | menu, item, category, size, options, availability, special, preorder where supported, pickup, service period | subtype-specific nouns remain suggestions, not defaults |

### Mixed organizations and hybrid fallback

- Organization-wide and cross-industry surfaces use organization, venue, content, item, category, option, availability, service period, screen, publish, and restore.
- Venue-scoped surfaces may use subtype terminology when the subtype and content type are known.
- Hybrid traits may influence suggestions but must not silently rename customer-authored content.
- When a venue combines equal concepts, use the term that describes the immediate task rather than forcing one subtype noun across the entire venue.
- Copying content between unlike subtypes preserves source names and presents destination terminology as reviewable suggestions only.
- Local custom labels remain authoritative until an authorized user changes them.

### Analytics terminology

Core operational views use neutral dimensions such as venue, content type, item, category, availability state, service period, screen, and publish state. Subtype-specific drill-downs may use drink type, product type, size, option type, batch state, preorder state, pickup context, flavor, or format when the data actually exists. Analytics labels must not imply inventory precision, freshness guarantees, production timing, fulfillment status, or paid access that the source data does not support.

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
- **Constraints and open decisions:** canonical operator and guest terminology is now defined by RWP-00.29. A builder must not invent new subtype values, commercial consequences, automatic content transformation, or hidden trait-based feature gates.

This brief is planning only. It authorizes no UI, API, schema, migration, or product implementation.

## Impeccable planning for terminology

The project-local Impeccable skill and `clarify` playbook were consulted because terminology affects future onboarding, navigation, forms, editor labels, state messages, help text, analytics, and guest-facing screens.

The future UI must:

- keep one noun and one verb for the same concept throughout a flow;
- use specific verb-object actions and name the affected item or content;
- use persistent labels and examples rather than placeholders as labels;
- distinguish first use, no results, filters, permissions, failure, empty content, sold-out state, unavailable state, and unknown timing;
- explain what failed and how to recover without exposing internal codes as the primary message;
- avoid promising freshness, quantity, production time, return time, pickup readiness, or preorder acceptance without authoritative data;
- use complete translatable messages rather than concatenated fragments;
- keep visible labels and accessible names aligned;
- support long product, category, option, venue, and collection names, localization expansion, pluralization, dynamic values, keyboard access, assistive technology, and 200% zoom;
- avoid color-only meaning and unnecessary abbreviations;
- preserve customer-authored terminology through profile and subtype changes;
- preserve the established Vennusign administrative visual world and approved Sky Blue direction.

Because this is planning only, no UI strings, components, routes, schemas, APIs, analytics implementation, ordering, pickup automation, or localization resources were changed.

## Capability-matrix classification through RWP-00.29

1. Industry, venue subtype, neutral subtype state, hybrid descriptive traits, and terminology preference are **product/domain state**.
2. Terminology changes defaults, labels, help text, starter recommendations, analytics presentation, and guest wording only.
3. Terminology never grants capability access, raises limits, changes permissions, controls rollout, or changes commercial access.
4. Batch, freshness, limited-quantity, expected-return, availability, preorder-window, pickup-context, service-period, size, and option values retain product/domain-state treatment where represented.
5. Manual item editing, manual availability changes, publishing, delivery confirmation, offline awareness, and recovery remain inherited core capabilities.
6. Ordering, payment, production management, fulfillment, inventory, POS, pickup-source, or other synchronization remains a later capability and integration-packaging decision.
7. Counts of venues, screens, users, integrations, content, storage, history, or AI consumption remain limits independent of subtype and terminology.
8. Customer-authored names and custom labels must be preserved through future profile or subtype changes.

## Deferred to RWP-00.30 and later

- detailed early-hours, business-day, service-period, batch-production, freshness-window, rotating-product, sell-out, preorder, pickup, seasonal-demand, table-service, and counter-service operating rules;
- required and optional capability decisions beyond inherited core manual operations;
- packaging, onboarding, dashboard, analytics, and implementation design;
- ordering, payments, production, fulfillment, inventory, POS, and external-source behavior.

## Reference anchors

These references informed the original profile boundary but do not replace Vennusign's product model:

- U.S. Census Bureau 2022 NAICS 722515 boundary for snack and nonalcoholic beverage bars;
- U.S. Census Bureau 2022 NAICS 311811 distinction for retail bakeries.

They are boundary evidence only, not Vennusign entitlement, subtype, legal, licensing, tax, food-safety, or regulatory classifications.

## RWP-00.27 completion summary

RWP-00.27 established the industry purpose, Restaurant inheritance, meaningful deltas, native boundary, organization and venue behavior, initial capability classifications, and Impeccable planning guardrails.

## RWP-00.28 completion summary

RWP-00.28 defined nine bounded primary subtypes plus a neutral fallback; established inclusion, exclusion, neighboring-profile, hybrid, subtype-selection, subtype-change, mixed-organization, multi-venue, screen-purpose, and Restaurant-inheritance rules; and applied Impeccable `shape` guidance to future subtype selection and change.

## RWP-00.29 completion and handoff

### Completed

- Defined the canonical operator-facing and guest-facing terminology glossary.
- Defined inherited Restaurant terms, subtype preferences, mixed-organization neutral terms, and hybrid fallback behavior.
- Distinguished product, category, size, option, temperature, milk or base, flavor, add-in, topping, batch, freshness, availability, preorder, custom-order, pickup, special, and service-period terms.
- Distinguished available, unavailable, sold out, limited, next batch, available again, preorder-window, and pickup-context language.
- Defined explicit operator actions, state labels, analytics labels, and source-authority boundaries.
- Applied the Impeccable `clarify` guidance to future UI copy, accessibility, localization, errors, empty states, success, and recovery.
- Preserved the previously approved Impeccable `shape` brief for subtype selection and change.
- Updated the Track 0 capability classification for terminology preference and Café-specific operational values.

### Not performed

- No product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, pricing, ordering, payment, production, fulfillment, inventory, pickup automation, analytics, localization, or integration implementation.
- No integration or external-system testing.
- No detailed operating-characteristic or capability-packaging design beyond what was required to define terminology boundaries.

### Next sequential RWP

**RWP-00.30 — Café, Bakery & Dessert Operating Characteristics** (#505) must document early hours, business-day and service-period behavior, batch production, freshness windows, rotating daily products, sell-outs, preorders, pickup, seasonal demand, table and counter service, and subtype-specific operating differences. It must tie each difference to defaults, terminology, content, screen purposes, or capability classification; remain documentation-only; and avoid jurisdiction-specific invention.
