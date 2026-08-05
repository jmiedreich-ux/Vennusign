# RWP-00.65 — Entertainment & Attractions Business Terminology

## Status

Complete in this proposed merge state.

## Issue

- #540

## Objective

Define canonical Entertainment & Attractions terminology, subtype preferences, neutral organization-wide fallbacks, operator-facing and visitor-facing language, preservation rules, and classification boundaries without implementing product behavior.

## Dependency verified

- RWP-00.64 is complete and merged.
- Restaurant remains the canonical baseline.
- The Entertainment industry definition and venue-subtype model are authoritative.
- RWP-00.66 — Operating Characteristics (#541) is the approved next item.

## Delivered

- Defined canonical terms for venue, attraction, experience, program, event, show, performance, screening, session, exhibit, collection, zone, queue, wait time, capacity, admission, ticket, schedule, wayfinding, notice, delay, relocation, closure, cancellation, and reopening.
- Distinguished object names, operating states, source precision, and visitor-facing presentation.
- Identified Restaurant terms inherited unchanged for embedded food-and-beverage operations.
- Defined neutral organization-wide fallbacks for mixed portfolios and ambiguous scopes.
- Added subtype terminology preferences without creating subtype-specific entitlement models.
- Preserved customer-authored names, imported source labels, authority, privacy, commercial access, and local language choices.
- Applied Impeccable `clarify` principles to future terminology selection, editing, and visitor-facing presentation.
- Updated the capability matrix, project status, handoff, and tracker.

## Canonical terminology

| Concept | Canonical operator term | Visitor-facing guidance |
| --- | --- | --- |
| Managed public destination | Venue | Use the customer-authored venue name whenever available |
| Specific destination or activity | Attraction | Use ride, habitat, activity, landmark, or local name when that is clearer |
| Visitor proposition spanning one or more objects | Experience | Use only when it communicates a complete visitor activity rather than hiding a precise object |
| Curated body of scheduled content | Program | Use today, this week, festival, season, or named program where clearer |
| Time-bound occurrence | Event | Use the authored event, match, concert, talk, class, or tournament name |
| General presented occurrence | Show | Prefer the subtype-specific term when known |
| Live staged occurrence | Performance | Use performance for theater, dance, opera, orchestral, or similar live presentation |
| Motion-picture occurrence | Screening | Use screening or showtime; preserve local cinema language |
| One schedulable occurrence or participation window | Session | Use session, round, game, tour, departure, entry time, or time slot according to subtype |
| Presented interpretive or display unit | Exhibit | Use exhibit, exhibition, installation, habitat, gallery, display, or local name accurately |
| Stewarded group of objects or living resources | Collection | Use collection only when stewardship or curation is represented |
| Navigable local area | Zone | Prefer land, gallery, hall, concourse, floor, gate, section, area, or local name when clearer |
| Waiting population or line | Queue | Use line where that is the established visitor term; do not imply measurement if manually maintained |
| Estimated delay before access | Wait time | State estimate/source freshness; use “wait estimate unavailable” rather than false precision |
| Maximum or current occupancy constraint | Capacity | Use full, limited availability, spaces available, or entry paused only when supported by known state |
| Permission to enter or participate | Admission | Distinguish general, timed, member, ticketed, participant, and restricted admission without exposing personal data |
| Admission credential | Ticket | Preserve pass, wristband, membership, reservation, booking, credential, or local term where accurate |
| Ordered timing information | Schedule | Use showtimes, program, timetable, fixtures, departures, sessions, or calendar by subtype |
| Direction and orientation information | Wayfinding | Visitor-facing content should give destination, direction, distance or level where known, and accessible route information |
| Operational communication | Notice | Prefer specific labels such as delay, closure, relocation, weather, access, safety, or service update |

## State wording

- **Available / Open:** currently usable or accessible under the represented admission conditions.
- **Limited:** availability remains but is constrained; do not imply a quantity unless known.
- **Sold out:** no saleable admission remains for the represented event/session according to the authoritative source.
- **Full:** current occupancy or participation capacity is reached; it is not automatically the same as sold out.
- **Boarding / Seating / Check-in open:** the represented arrival step is active.
- **Delayed:** expected start or access is later than planned; include the revised time only when known.
- **Paused:** temporarily stopped with an expectation of further review or resumption.
- **Relocated:** moved to another known location; show the new destination and route where known.
- **Canceled:** the represented occurrence will not proceed; avoid implying refund or rebooking policy unless provided.
- **Closed:** not operating or accessible for the represented period.
- **Maintenance:** unavailable because maintenance is occurring; do not disclose sensitive operational detail.
- **Weather affected:** operating state is influenced by weather; use a precise state such as delayed, closed, or limited when known.
- **Reopening / Resuming:** expected to return; show a time only when authorized and known.

## Subtype terminology preferences

- **Cinema:** film, screening, showtime, auditorium, screen, format, rating, captions, audio description, seating.
- **Performing Arts Theater:** production, performance, curtain, auditorium, stage, interval/intermission, late seating.
- **Museum:** collection, exhibit, gallery, program, talk, tour, timed entry, interpretation.
- **Gallery / Exhibition Venue:** exhibition, installation, hall/gallery, artist/exhibitor, opening, talk.
- **Zoo / Aquarium:** habitat, species, feeding/talk, trail/route, animal-care closure, last entry.
- **Theme / Amusement Park:** attraction, ride, land/zone, queue, wait time, height/access guidance, reopening.
- **Family Entertainment Center:** activity, session, party, area, lane, game, check-in.
- **Arcade:** game, game zone, card/token, tournament, prize/redemption area where represented.
- **Bowling Center:** lane, game, round, league, tournament, check-in, shoe service.
- **Sports Venue:** match/game/event, team/competitor, gate, section, field/court/track, fixture.
- **Live-Event Venue:** event, artist/speaker/production, doors, stage/room, seating/standing, set or session.
- **Attraction / Tour:** experience, tour, departure, entry window, route, guide, stop, last entry.

Subtype preferences are default language candidates only. Customer-authored names and local vocabulary remain authoritative unless invalid, unsafe, or privacy-sensitive.

## Neutral mixed-organization fallbacks

Use **organization**, **venue**, **area**, **destination**, **event**, **scheduled occurrence**, **content**, **screen**, **location**, **availability**, **operating state**, **publish**, and **restore** when a mixed portfolio cannot safely use subtype-specific language.

Do not use attraction, show, screening, exhibit, match, ride, room, property, outlet, or similar subtype-specific terms across an organization when the scope contains incompatible concepts.

## Operator and visitor language boundary

- Operator language may expose source, freshness, scope, target, approval, permission, delivery, and recovery detail.
- Visitor language must prioritize identity, current state, time, location, admission/access condition, direction, and next action.
- Public signage must not expose ticket holder, seat holder, member, participant, performer, sponsor, security, or operationally sensitive detail by default.
- Imported source terminology must not be presented as current or authoritative after its source becomes stale, disconnected, or overridden.

## Classification decisions

1. Canonical and subtype-preferred terminology is **product/domain state**.
2. Customer-authored labels and names are **product/domain state** governed by permissions and validation.
3. Changing terminology is a **core capability** for an authorized operator because usable local language is required for daily operation.
4. Terminology does not grant capability access, transfer authority, alter privacy scope, increase limits, or determine tier/add-on access.
5. External source labels remain product state with source-authority and freshness relationships; the integration itself remains a later add-on or tier candidate.
6. Localization, translation workflow, premium copy assistance, and AI generation remain later capability/packaging questions; basic clear manual wording remains core.

## Impeccable clarification result

Future terminology surfaces are **Operate** experiences. They must:

- show the object and scope being renamed;
- compare canonical, subtype-preferred, customer-authored, imported, and neutral fallback language without implying plan differences;
- preview visitor-facing impact before high-scope changes;
- preserve existing authored content and imported source authority;
- explain validation failures in plain language;
- support localization expansion, keyboard and assistive technology, 200% zoom, and non-color distinctions;
- provide permission, stale-source, conflict, interrupted-save, success, undo, and restoration states;
- preserve the approved Sky Blue administrative direction.

## Validation

- Restaurant inheritance and embedded food-and-beverage language remain intact.
- Every issue-listed concept has canonical guidance.
- Operator, visitor, subtype, neutral, and customer-authored language are separated.
- Terminology remains distinct from permissions, entitlements, add-ons, limits, rollout, privacy, and source authority.
- No product implementation was performed.
- The next sequential item is RWP-00.66.

## Skipped under standing owner instruction

Azure SQL and all external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and other integration-type tests.

## Exact next action

After merge, issue closure, default-branch verification, and claim release, execute **RWP-00.66 — Entertainment & Attractions Operating Characteristics** (#541).
