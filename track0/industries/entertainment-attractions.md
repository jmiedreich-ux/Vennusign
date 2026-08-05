# Entertainment & Attractions Industry Profile

## Identity

- **Industry:** Entertainment & Attractions
- **RWP range:** RWP-00.63 through RWP-00.74
- **Baseline:** Restaurant
- **Completed through:** RWP-00.65
- **Current status:** Industry definition, venue subtypes, and business terminology complete in this proposed merge state
- **Next sequential RWP:** RWP-00.66 — Operating Characteristics

## Purpose and customer outcomes

This profile covers destination-, program-, exhibition-, performance-, recreation-, and attraction-led venues whose visitor experience depends on accurate schedules, admissions guidance, wayfinding, availability, queue, venue-state, safety, accessibility, and event information.

It inherits Restaurant content creation, editing, duplication, archive, restore, preview, publishing, screen pairing, targeting, delivery confirmation, offline/outdated detection, recovery, layouts, themes, venue information, permissions, state separation, limits, and packaging discipline.

Operators additionally need to:

- explain what is happening, where and when it happens, who it is for, whether it is available, and what action follows;
- keep programs, performances, screenings, exhibits, activities, attractions, sessions, tours, admissions, queues, closures, delays, relocations, accessibility, and safety guidance current;
- coordinate venue-wide information while preserving local area, attraction, exhibit, event, session, queue, and screen scope;
- communicate sold-out, full, delayed, paused, canceled, relocated, weather-affected, maintenance, closure, reopening, and recovery states;
- support accessible, multilingual, distance-readable presentation in crowded, outdoor, low-light, high-motion, and intermittent-connectivity environments;
- verify time-sensitive delivery and restore safely when displays are stale, offline, or failed.

Restaurant menu semantics remain inherited for embedded concessions, bars, cafés, restaurants, and food outlets. They do not define the primary Entertainment content model.

## Industry boundary

Native concepts include cinemas, performing-arts theaters, live-event venues, museums, non-retail galleries, science centers, planetariums, zoos, aquariums, botanical gardens, theme and amusement parks, family entertainment centers, arcades, bowling centers, spectator sports venues, attractions, tours, heritage sites, visitor centers, and related hybrids.

The profile excludes content production or distribution without an on-site visitor venue, retail art dealing, participant-only fitness or recreation without a managed spectator/attraction journey, gambling-only operations, incidental entertainment inside another primary industry, unmanaged public outdoor sites, and temporary private events without durable operator-managed visitor information.

These boundaries choose defaults only. They are not legal, licensing, accessibility, safety, capacity, film-rating, gaming, labor, tax, statistical, or admission classifications.

## Canonical venue subtypes

| Primary subtype | Best when | Meaningful emphasis |
| --- | --- | --- |
| **Cinema** | Motion-picture screenings and auditorium showtimes control the journey | Film, screening/showtime, auditorium/screen, format, rating/accessibility, start time, seating, sold-out, delay, next screening |
| **Performing Arts Theater** | A purpose-built house presents staged live performance | Production, performance, stage/auditorium, curtain/start, interval, late seating, access information |
| **Museum** | Collection stewardship, preservation, interpretation, and education dominate | Collection, exhibit, gallery, program, talk, tour, timed entry, interpretation, closures |
| **Gallery / Exhibition Venue** | Rotating exhibitions, installations, or non-retail display programs dominate | Exhibition, installation, hall/gallery, artist/exhibitor, opening, talk, temporary closure |
| **Zoo / Aquarium** | Living collections, habitats, conservation, and a managed visitor path dominate | Species/habitat, feeding/talk schedules, route, animal-care closure, weather, last entry |
| **Theme / Amusement Park** | Multiple rides, attractions, lands, shows, and queues control the day | Attraction state, queue/wait, access guidance, showtimes, lands/zones, weather, maintenance, reopening |
| **Family Entertainment Center** | A balanced mix of activities, groups, parties, and participation dominates | Activity/session, party, lane/area, age/access, check-in, queue, capacity, multi-activity wayfinding |
| **Arcade** | Game-floor, token/card, simulator, tournament, or redemption operation is primary | Game, game zone, card/token, availability, tournament, prize/redemption area |
| **Bowling Center** | Lanes, rounds, leagues, tournaments, and bowling scheduling dominate | Lane, game/round, league, tournament, check-in, shoe service, scoring notices |
| **Sports Venue** | Spectator competition, home-team calendar, match, race, or tournament dominates | Match/game/event, team/competitor, gate, section, field/court/track, fixture, parking, egress |
| **Live-Event Venue** | Promoter-led concerts, comedy, festivals, conferences, conventions, or changing productions dominate | Event, artist/speaker/production, doors, stage/room, seating/standing, delay, cancel, relocate, egress |
| **Attraction / Tour** | A managed route, guided destination, observation experience, heritage visit, or single attraction dominates | Experience/tour, departure or entry window, route, guide, language, capacity, last entry, closure |

A venue may remain **Unspecified / General Entertainment & Attraction Venue** when no subtype clearly controls its daily visitor journey and information rhythm. This neutral state is not a reduced plan.

### Hybrid and ambiguous concepts

Use one primary subtype plus optional traits such as outdoor, vehicle-based, water-based, seasonal, weather-sensitive, immersive, heritage, living-collection, science/education, general-admission, reserved-seat, timed-entry, membership-led, multi-use, campus, touring, resident-company, team-home, tournament, festival, low-light, high-motion, continuous, self-guided, guided, or temporary.

- Drive-in: Cinema with outdoor and vehicle-based traits.
- Water park: Theme / Amusement Park with a water-based trait.
- Botanical garden: Zoo / Aquarium when living collection and visitor route dominate.
- Planetarium or observatory: Museum when education and interpretation dominate; showtime-led venues may use Cinema or Performing Arts Theater.
- Historical site: Museum when preservation dominates; Attraction / Tour when the managed route dominates.
- Escape room, haunted attraction, observation deck, cave, scenic route, or single immersive walkthrough: Attraction / Tour unless a broader activity mix makes Family Entertainment Center more accurate.
- Trampoline park or balanced miniature-golf, laser-tag, climbing, and party destination: Family Entertainment Center.
- Bowling with incidental arcade/food: Bowling Center; a balanced activity mix may be Family Entertainment Center.
- Arena or stadium: Sports Venue when competition dominates; Live-Event Venue when promoter-led productions dominate.
- Purpose-built resident performance house: Performing Arts Theater; flexible concert/comedy/event facility: Live-Event Venue.
- Exhibition-led non-retail art space: Gallery / Exhibition Venue; collection stewardship: Museum.
- Resort or casino with lodging: Hospitality at property level, with local Entertainment venues typed independently.

Traits affect recommendations only. They do not grant capabilities, stack entitlements, transfer authority, or increase limits.

## Canonical business terminology

| Concept | Canonical operator term | Visitor-facing guidance |
| --- | --- | --- |
| Managed destination | **Venue** | Prefer the customer-authored venue name |
| Specific destination/activity | **Attraction** | Use ride, habitat, activity, landmark, or local name when clearer |
| Complete visitor proposition | **Experience** | Use only when it does not hide a more precise object |
| Curated scheduled body | **Program** | Today, this week, festival, season, or named program may be clearer |
| Time-bound occurrence | **Event** | Preserve the event, match, concert, talk, class, or tournament name |
| General presented occurrence | **Show** | Prefer subtype-specific terms when known |
| Live staged occurrence | **Performance** | Use for theater, dance, opera, orchestral, or similar live presentation |
| Motion-picture occurrence | **Screening** | Screening or showtime; preserve established local cinema language |
| Schedulable occurrence/window | **Session** | Session, round, game, tour, departure, entry time, or time slot by subtype |
| Interpretive/display unit | **Exhibit** | Exhibit, exhibition, installation, habitat, gallery, display, or local name accurately |
| Stewarded group | **Collection** | Use only when stewardship or curation is represented |
| Navigable local area | **Zone** | Prefer land, gallery, hall, concourse, floor, gate, section, or local name where clearer |
| Waiting line/population | **Queue** | “Line” may be used locally; do not imply automated measurement |
| Estimated access delay | **Wait time** | Show estimate/source freshness; avoid false precision |
| Occupancy constraint | **Capacity** | Use full, limited availability, spaces available, or entry paused only when supported |
| Permission to enter | **Admission** | Distinguish general, timed, member, ticketed, participant, and restricted scope |
| Admission credential | **Ticket** | Preserve pass, wristband, membership, reservation, booking, credential, or local term |
| Ordered timing information | **Schedule** | Showtimes, program, timetable, fixtures, departures, sessions, or calendar by subtype |
| Direction/orientation | **Wayfinding** | Give destination, direction, level/distance where known, and accessible route information |
| Operational communication | **Notice** | Prefer delay, closure, relocation, weather, access, safety, or service update |

### State wording

- **Available / Open:** currently usable or accessible under represented conditions.
- **Limited:** available but constrained; do not imply a quantity unless known.
- **Sold out:** no saleable admission remains for the represented event/session according to the authoritative source.
- **Full:** current occupancy or participation capacity is reached; not automatically the same as sold out.
- **Boarding / Seating / Check-in open:** the represented arrival step is active.
- **Delayed:** later than planned; include a revised time only when known.
- **Paused:** temporarily stopped pending review or resumption.
- **Relocated:** moved to a known location; include destination and route where known.
- **Canceled:** will not proceed; do not imply refund or rebooking policy unless provided.
- **Closed:** not operating or accessible for the represented period.
- **Maintenance:** unavailable for maintenance without exposing sensitive operational detail.
- **Weather affected:** use a more precise delayed, closed, limited, or reopened state when known.
- **Reopening / Resuming:** expected to return; show a time only when authorized and known.

### Subtype terminology preferences

- Cinema: film, screening, showtime, auditorium, screen, format, rating, captions, audio description, seating.
- Performing Arts Theater: production, performance, curtain, stage, auditorium, interval/intermission, late seating.
- Museum: collection, exhibit, gallery, program, talk, tour, timed entry, interpretation.
- Gallery / Exhibition Venue: exhibition, installation, hall/gallery, artist/exhibitor, opening, talk.
- Zoo / Aquarium: habitat, species, feeding/talk, trail/route, animal-care closure, last entry.
- Theme / Amusement Park: attraction, ride, land/zone, queue, wait time, height/access guidance, reopening.
- Family Entertainment Center: activity, session, party, area, lane, game, check-in.
- Arcade: game, game zone, card/token, tournament, prize/redemption area.
- Bowling Center: lane, game, round, league, tournament, check-in, shoe service.
- Sports Venue: match/game/event, team/competitor, gate, section, field/court/track, fixture.
- Live-Event Venue: event, artist/speaker/production, doors, stage/room, seating/standing.
- Attraction / Tour: experience, tour, departure, entry window, route, guide, stop, last entry.

These are default candidates only. Customer-authored names and local vocabulary remain authoritative unless invalid, unsafe, or privacy-sensitive.

### Neutral mixed-organization fallbacks

Use **organization**, **venue**, **area**, **destination**, **event**, **scheduled occurrence**, **content**, **screen**, **location**, **availability**, **operating state**, **publish**, and **restore** when a mixed portfolio cannot safely use subtype-specific language.

Do not use attraction, show, screening, exhibit, match, ride, property, outlet, or similar subtype terms organization-wide when the scope contains incompatible concepts.

## Operator, visitor, authority, and preservation boundaries

- Operator language may expose source, freshness, scope, target, approval, permission, delivery, and recovery detail.
- Visitor language prioritizes identity, current state, time, location, admission/access condition, direction, and next action.
- Public signage must not expose ticket holder, seat holder, member, participant, performer, sponsor, security, or operationally sensitive detail by default.
- Imported labels must not appear authoritative after a source becomes stale, disconnected, or overridden.
- Changing industry, subtype, or terminology must preserve customer-authored content, screens, schedules, history, operational state, permissions, privacy, source authority, integrations, limits, and commercial access.
- Organization industry may suggest local language but cannot override a venue-local choice.
- Ownership, operator, promoter, presenter, tenant, sponsor, team, performer, distributor, rights-holder, or host relationships do not silently transfer authority.

## Classification decisions through RWP-00.65

1. Industry, subtype, descriptive traits, canonical terminology, subtype-preferred terminology, customer-authored labels, and neutral fallbacks are **product/domain state**.
2. Authorized manual terminology change is a **core capability** because usable local language is required for daily operation.
3. Venue hierarchy and operating values remain product/domain state independent of terminology.
4. Manual program, showtime, admissions, wayfinding, queue, capacity, delay, closure, relocation, accessibility, safety, targeting, publishing, delivery confirmation, offline awareness, and recovery remain **core capabilities**.
5. Authorization, audience, admission, privacy, content authority, and source authority remain **permission/scope relationships**, not commercial access.
6. Counts, storage, retained history, connections, and consumption remain **usage or quantity limits**.
7. External ticketing, admissions, queue, venue, cinema, show-control, collection, attraction, event, or sports synchronization remains a future **add-on candidate** or approved tier bundle; manual core operation remains available.
8. Localization workflow, premium translation, copy assistance, and AI generation remain later packaging questions; basic clear manual wording remains core.

## Impeccable planning guardrails

Future subtype and terminology surfaces are **Operate** experiences.

- Compare canonical, subtype-preferred, customer-authored, imported, and neutral language without implying plan differences.
- Show object and scope; preview visitor-facing impact before high-scope changes.
- Explain validation and source conflicts in plain language.
- Preserve authored content, authority, privacy, and commercial access.
- Cover first-run, recommendation, no-match, permission, stale-source, conflict, interrupted-save, success, undo, and restoration states.
- Support phone and desktop, keyboard and assistive technology, localization expansion, 200% zoom, and non-color distinctions.
- Preserve the approved Sky Blue administrative direction.

No UI or product implementation is authorized by this planning work.

## Owner decisions and deferred questions

- exact hierarchy and quantity-limit counting across organization, venue, campus, building, area, attraction, exhibit, event, session, queue, and screen;
- authority representation among owners, operators, promoters, presenters, tenants, sponsors, teams, performers, distributors, rights-holders, and hosts;
- public, ticketed, member-only, participant-only, staff-only, performer-only, sponsor-only, and sensitive information scopes;
- personalized or ticket-specific display support and privacy rules;
- final tier, add-on, limit, downgrade, retention, localization, translation, AI, and integration packaging;
- cross-industry mixed-resort, casino, arena-district, cultural-campus, and destination-portfolio behavior.

## Validation checklist

- [x] Restaurant inheritance is explicit.
- [x] Only meaningful deltas are documented.
- [x] All issue-listed subtypes and terminology concepts have bounded guidance.
- [x] Customer-authored, subtype, operator, visitor, imported, and neutral language are separated.
- [x] Every concern has one primary classification.
- [x] Essential manual operation remains core.
- [x] Permissions, states, entitlements, add-ons, limits, and rollout flags remain separate.
- [x] Impeccable `shape` and `clarify` guidance were applied.
- [x] Accessibility, responsive, hierarchy, state, and recovery considerations are documented.
- [x] No product implementation was performed.
- [x] The next sequential RWP is RWP-00.66.
