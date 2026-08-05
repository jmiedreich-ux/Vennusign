# Entertainment & Attractions Terminology Model

## Authority

This document completes RWP-00.65 as a documentation-only companion to `track0/industries/entertainment-attractions.md`. Restaurant remains the canonical baseline. The terms below change labels, starter recommendations, help text, analytics presentation, and visitor-facing wording only; they do not grant capabilities, permissions, entitlements, limits, rollout access, or commercial access.

## Neutral cross-industry terms

Use these terms when a surface spans industries, organizations, or multiple Entertainment subtypes:

- organization;
- venue;
- area;
- content;
- schedule;
- program;
- experience;
- event;
- session;
- admission;
- availability;
- capacity;
- queue;
- wait time;
- notice;
- wayfinding;
- screen;
- publish;
- restore.

Preserve customer-authored venue, event, attraction, exhibit, production, film, team, performer, sponsor, area, and screen names. Never silently rewrite proper names after an industry or subtype change.

## Canonical distinctions

- **Venue** is the managed visitor-facing place. Campus, building, destination, park, arena district, and complex are contextual hierarchy terms.
- **Area** is the neutral physical subdivision. Zone, land, gallery, hall, concourse, floor, room, gate, section, auditorium, screen, stage, court, field, lane, ride, habitat, and exhibit area are contextual terms.
- **Program** is the neutral collection of scheduled or continuously available visitor experiences.
- **Experience** is the neutral visitor-facing unit when show, screening, exhibit, attraction, tour, activity, game, match, performance, or session is not more precise.
- **Event** is a bounded occurrence with a date or time. It does not replace a continuously available exhibit or attraction.
- **Show** is visitor-facing for a performance or presentation. Use **performance** for performing arts; **screening** for cinema; **presentation**, **talk**, or **demonstration** when those are more accurate.
- **Schedule** is the ordered set of operating times. **Showtimes**, **performance times**, **screening times**, **session times**, **tour departures**, **feeding times**, **game schedule**, and **event schedule** are subtype-specific presentations.
- **Admission** is the neutral access concept. Ticket, pass, membership, reservation, timed entry, wristband, credential, and guest-list entry are represented access methods, not universal synonyms.
- **Ticket** is evidence of admission where the venue uses ticketing. Do not imply every venue or experience requires a ticket.
- **Queue** is the waiting line or managed waiting context. **Wait time** is an estimate or measured duration and must display its source time or freshness when available.
- **Capacity** is the represented maximum or current occupancy constraint. **Full**, **limited**, **available**, and **closed** are distinct states; never infer exact remaining capacity.
- **Wayfinding** is the operator capability. Visitor copy uses direct destinations and actions such as “Screen 4,” “Gallery 2,” “Gate B,” “This way,” or “Use the north entrance.”
- **Notice** is the neutral operational message. Delay, cancellation, closure, relocation, weather, maintenance, safety, accessibility, last-entry, and reopening notices remain distinct.

## Visitor-facing state language

Use one clear state and one clear next action. Unknown state remains unknown.

| State | Meaning | Visitor-facing guidance |
| --- | --- | --- |
| Available | The represented experience or admission context is currently offered | Show the next relevant time, place, or action |
| Limited | Availability or capacity is constrained but an exact remainder is not authoritative | Say “Limited availability” and direct visitors to the authoritative source |
| Full | The represented queue, session, area, or capacity context cannot accept more visitors now | Offer another time or location only when known |
| Sold out | The represented admission inventory is exhausted | Do not substitute “full” unless the venue uses that distinction |
| Delayed | The planned start or movement is later than scheduled | Show revised timing only when authoritative |
| Paused | Operation has temporarily stopped and may resume | Avoid promising a reopening time unless known |
| Closed | The venue, area, attraction, exhibit, queue, or service is not operating | Identify scope and alternatives where known |
| Canceled | The scheduled event, performance, screening, session, or tour will not occur | Distinguish from delay and closure |
| Relocated | The experience or access point moved | Show the new destination and route |
| Weather affected | Weather changes operation, access, or timing | State the exact affected scope and authoritative next step |
| Reopening | A previously unavailable context is returning | Show time only when confirmed |

## Subtype terminology preferences

- **Cinema:** film, screening, showtime, auditorium or screen, format, rating/accessibility information, doors or seating where used, sold out, delayed, canceled, next screening.
- **Performing Arts Theater:** production, performance, curtain or start time, stage, auditorium, interval or intermission according to house usage, late seating, cast, creative team.
- **Museum:** collection, exhibit, gallery, program, tour, talk, timed entry, interpretation, member admission, temporary closure.
- **Gallery / Exhibition Venue:** exhibition, installation, hall or gallery, artist, exhibitor, opening, talk, entry window, temporary closure.
- **Zoo / Aquarium:** animal, species, habitat, exhibit, keeper talk, feeding, route, last entry, animal-care closure, conservation notice.
- **Theme / Amusement Park:** attraction, ride, land or zone, showtime, queue, wait time, height/access guidance, temporarily closed, reopening, last ride.
- **Family Entertainment Center:** activity, session, party, check-in, area, lane, capacity, wait, available, paused.
- **Arcade:** game, game zone, card/token guidance, tournament, prize area, available, unavailable, maintenance.
- **Bowling Center:** lane, game or round, session, league, tournament, check-in, lane assignment, available lane, delayed start.
- **Sports Venue:** game, match, race, event, team, competitor, gate, section, field/court/track, start time, event status, egress.
- **Live-Event Venue:** event, artist, speaker, production, doors, start time, stage or room, gate, seating or standing area, delayed, canceled, relocated.
- **Attraction / Tour:** attraction, experience, tour, departure, entry window, route, guide, language, last entry, weather notice, closed, reopening.
- **Neutral subtype:** venue, experience, program, schedule, area, admission, queue, availability, notice, and wayfinding.

## Operator actions

Use explicit verb-object labels:

- Add experience
- Edit schedule
- Update availability
- Set queue status
- Update wait time
- Set capacity status
- Add wayfinding notice
- Publish notice
- Preview screens
- Restore previous version

Avoid vague actions such as “Manage,” “Update,” “Apply,” or “Continue” when the outcome can be named.

## Analytics language

Stable neutral dimensions are organization, venue, subtype, area, experience type, experience, event, session, schedule date, admission context, capacity state, queue state, notice type, screen, publish result, and delivery state. Subtype-specific labels may be presented in the interface while exports and cross-industry reporting retain stable neutral dimensions.

## Classification

Industry, subtype, terminology preference, venue hierarchy, program, experience, event, session, schedule, admission method, ticket reference, queue, wait-time value, capacity state, operational notice, wayfinding destination, and availability state are product/domain state where represented. Customer-authored labels remain customer content. Who may edit or publish is permission. Commercial availability remains tier entitlement or add-on. Counts remain limits. Temporary release control remains rollout flag.

Manual editing of schedules, availability, queue/capacity states, wayfinding, notices, targeting, publishing, delivery confirmation, offline awareness, and restoration remains core. Ticketing, admissions, access-control, dynamic wait-time, venue-management, show-control, collection-management, attraction, event, sports, mapping, and related synchronization remain later integration-packaging decisions.

## Impeccable `clarify` guidance

Future UI copy must use one noun and verb consistently across a flow; preserve visible labels; distinguish first-use, empty, filtered, unavailable, permission, validation, stale-data, publish-failure, success, and recovery states; align visible and accessible names; support keyboard and assistive technology; remain understandable at 200% zoom; tolerate long venue and event names plus localization expansion; avoid color-only meaning; and preserve the approved Sky Blue administrative direction.

## Deferred questions

RWP-00.66 and later must resolve operating characteristics, required and optional capabilities, final classification, packaging, onboarding, dashboard, analytics, and validation. Jurisdiction-specific admission, safety, accessibility, age, rating, capacity, and emergency wording remains outside this terminology model.
