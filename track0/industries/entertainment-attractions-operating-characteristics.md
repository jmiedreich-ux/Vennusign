# Entertainment & Attractions Operating Characteristics

## Authority

This documentation-only companion completes RWP-00.66 and extends the merged Entertainment & Attractions industry, subtype, and terminology models. It records operating rhythms, state boundaries, defaults, capability-presentation implications, and future UX constraints. It does not implement product behavior.

## Operating model

Entertainment venues combine scheduled occurrences, continuously available experiences, physical movement, admission conditions, queue and capacity states, safety and accessibility communication, and fast-changing venue conditions. The system must keep the state of the venue separate from the state of each area, attraction, exhibit, event, performance, screening, session, queue, admission window, and screen.

## Time and schedule characteristics

### Venue operating day

- A venue operating day may cross midnight and must not be inferred from calendar date alone.
- Venue hours, admission hours, attraction hours, exhibit hours, performance or screening schedules, last-entry times, and service hours remain separate values.
- A closed venue may still have future scheduled content; an open venue may contain closed or unavailable experiences.
- “Today,” “now,” and “next” must use the venue’s local time zone and operating-day rules.
- Schedule changes require source, freshness, effective time, target scope, and recovery visibility.

### Scheduled occurrences

Events, performances, screenings, sessions, tours, games, matches, talks, feedings, demonstrations, rounds, and timed-entry windows may overlap. Each occurrence needs, where applicable:

- authored identity and subtype-specific label;
- start time and optional end time or duration;
- venue, area, auditorium, screen, stage, field, court, lane, gate, room, or departure point;
- admission or access context;
- availability and operating state;
- accessibility, language, age, height, rating, seating, participation, or arrival guidance only when authoritative;
- delay, relocation, cancellation, closure, or recovery state;
- source and freshness where externally synchronized.

Continuous or self-guided exhibits and attractions must not be forced into fake showtimes. They use operating windows, availability, last entry, closure, and route context instead.

### Last-entry and end-of-day behavior

Last admission, last entry, final session, final screening, last ride, final tour, venue closing, area closing, and service closing are distinct. Visitor-facing content must identify the exact scope and avoid implying that every experience closes at the venue closing time.

## Queue and wait-time characteristics

- A queue may be open, forming, limited, full, paused, closed, redirected, virtual where represented, or unavailable.
- Wait time may be manually entered, estimated, measured, imported, stale, or unknown.
- The UI must show freshness or source context to operators and must never manufacture precision.
- Visitor-facing wait time may use an exact estimate, range, qualitative label, or “unavailable” according to source quality.
- A wait-time change does not automatically change attraction, admission, or venue state.
- Queue closure does not necessarily mean attraction closure; attraction closure does not automatically define the reason.
- Alternate queue, entrance, session, or experience guidance appears only when known.

Manual queue state and manual wait-time updates remain core. Dynamic measurement and synchronization remain later integration-packaging candidates.

## Capacity and admission characteristics

Capacity may apply independently to a venue, area, attraction, exhibit, event, performance, screening, session, queue, lane group, tour, or admission window.

- **Available** means the represented access context is open under its current conditions.
- **Limited** means constrained without asserting a precise remaining count.
- **Full** means current occupancy or participation capacity is reached.
- **Sold out** means saleable admission inventory is exhausted according to an authoritative source.
- **Entry paused** means admission is temporarily stopped without claiming the venue or experience is closed.
- **Closed** means the represented context is not operating or accessible.

Manual capacity-state communication remains core. Exact occupancy, ticket inventory, attendance, turnstile, access-control, and reservation synchronization remain later integrations. Public displays must not expose visitor-, ticket-, seat-, member-, participant-, performer-, security-, or operationally sensitive details.

## Attraction, exhibit, and experience characteristics

Continuously available and scheduled experiences share common operational needs but keep distinct objects and states:

- attraction or ride availability, queue, wait, maintenance, weather, reopening, and access guidance;
- exhibit or habitat availability, route, interpretation, temporary closure, conservation or care notices, and accessible alternatives;
- performance or screening schedule, auditorium or stage assignment, seating or boarding, delay, cancellation, relocation, and next occurrence;
- tour departure, route, guide or language, capacity, arrival point, last entry, weather, and cancellation;
- activity, lane, game, round, session, party, check-in, and availability in participation-led venues;
- sports event, team or competitor, gate, section, field/court/track, timing, event state, transport, and egress.

A content model must support one primary experience object plus linked occurrences and local operating state without requiring an external venue-management system.

## Closure, disruption, and recovery characteristics

Operational communication must distinguish scope and cause without inventing detail:

- delayed;
- paused;
- temporarily unavailable;
- closed;
- canceled;
- relocated;
- weather affected;
- maintenance;
- safety restriction;
- capacity restriction;
- access restriction;
- reopening or resumed.

A change flow must show affected venue/area/experience/session, effective time, source, target screens, visitor-facing message, alternate guidance where known, and restoration path. High-scope notices require confirmation and should support expiry or scheduled removal without silently suppressing emergency information.

## Safety, accessibility, and multilingual characteristics

- Safety and emergency content must be concise, authoritative, high-priority, distance-readable, non-ambiguous, and not dependent on color, animation, or audio alone.
- Vennusign must not invent legal, code, evacuation, medical, age, rating, height, accessibility, capacity, or security instructions.
- Accessibility guidance may cover accessible entrances, routes, seating, captioning, audio description, sensory considerations, mobility restrictions, service locations, and assistance points only when venue-authored or source-authoritative.
- Basic manual multilingual content and correct language labeling are required operational foundations.
- Automatic translation, translation workflow, glossary management, premium localization, and AI-assisted wording remain later packaging candidates.
- Language fallback must be explicit; stale or incomplete translations cannot silently replace current authoritative content.

## Event surges and environmental characteristics

Peak arrivals, intermissions, halftime, post-event egress, weather changes, school groups, festivals, tournaments, sold-out events, and simultaneous sessions may rapidly change priorities.

Future surfaces must support:

- now/next prioritization;
- fast state updates with clear scope;
- entrance, gate, concourse, queue, auditorium, area, parking, transportation, and exit targeting;
- mobile operation by authorized staff;
- readable large-format and outdoor presentation;
- low-light modes without losing status clarity;
- intermittent connectivity, queued publication, delivery confirmation, stale-state visibility, and recovery;
- restrained motion and high information hierarchy in crowded, high-motion environments.

## Subtype operating differences

| Subtype | Dominant rhythm | Highest-priority state and content |
| --- | --- | --- |
| Cinema | Repeating screening schedule across auditoriums | Film/showtime, auditorium, format/accessibility, seating/boarding, sold out, delay, cancellation, next screening |
| Performing Arts Theater | Production and performance calendar | Performance, curtain/start, auditorium/stage, interval, late seating, delay/cancellation, access guidance |
| Museum | Operating-day access plus exhibits and programs | Exhibit/gallery availability, timed entry, tours/talks, route, temporary closure, interpretation |
| Gallery / Exhibition Venue | Rotating exhibitions and temporary programs | Exhibition/hall, opening, talk, entry window, capacity, temporary closure |
| Zoo / Aquarium | Operating day, habitats, talks, and route | Habitat/exhibit availability, feeding/talk schedule, care closure, route, weather, last entry |
| Theme / Amusement Park | Multi-attraction operating day | Attraction state, queue/wait, access guidance, showtimes, weather, closure/reopening, last ride |
| Family Entertainment Center | Activity sessions and group/party surges | Activity/session availability, check-in, party timing, area/lane assignment, capacity, queue |
| Arcade | Continuous game-floor operation with events | Game-zone availability, card/token guidance, tournament, maintenance, prize-area information |
| Bowling Center | Lane/session/league/tournament rhythm | Lane availability/assignment, check-in, session delay, league/tournament schedule, service guidance |
| Sports Venue | Event-day arrival through egress | Event, gate/section, start, transport, concessions, safety, event state, egress |
| Live-Event Venue | Promoter-led event and doors/start rhythm | Event/artist, doors/start, room/stage, gate, delay/cancel/relocate, merchandise, egress |
| Attraction / Tour | Entry windows, departures, route, and capacity | Experience/tour, departure/entry, route, language, weather, capacity, last entry, closure/reopening |

## Defaults and capability presentation

Industry and subtype may seed:

- terminology and starter-content recommendations;
- default screen-purpose suggestions;
- today/now/next presentation emphasis;
- queue, wait, capacity, schedule, exhibit, attraction, event, safety, accessibility, and wayfinding modules;
- recommended operational notices and recovery checklists;
- responsive and environmental presentation guidance.

These are product-state defaults and recommendations, not entitlements or rollout flags. Essential manual operation remains visible in every tier. Optional integrations, advanced coordination, analytics, identity, AI, and managed hardware must not obscure or replace the manual core.

## Source, freshness, and authority rules

- Every imported schedule, wait-time, capacity, admission, score, attraction, exhibit, event, or operational value needs source and freshness context.
- Manual override authority must be explicit and reversible.
- Stale, disconnected, conflicting, or partially synchronized data must remain visible to operators.
- Visitor-facing content must not present stale imported data as current by default.
- Privacy, rights, approval, and content authority remain independent from commercial access.

## Impeccable `shape` planning result

Future operating surfaces are primarily **Operate** experiences for authorized venue staff working under time pressure, often on mobile devices and in crowded or intermittent-connectivity environments. The primary outcome is a correct scoped update that reaches the intended screens and can be verified and recovered.

The future flow must:

- make venue, area, experience/session, state, effective time, source, audience, and target scope visible;
- prioritize one primary action and show impact before publication;
- cover first-use, empty, scheduled, live, stale, conflicting-source, permission, offline, publish-failure, partial-delivery, success, undo, and restoration states;
- scale from one-screen venues to multi-building campuses and event districts;
- support phone, desktop, keyboard, assistive technology, localization expansion, and 200% zoom;
- avoid color-only meaning and uncontrolled motion;
- preserve the approved Sky Blue administrative direction.

## Classification decisions through RWP-00.66

1. Venue, area, schedule, occurrence, operating window, admission context, queue state, wait-time value, capacity state, attraction/exhibit state, closure, delay, relocation, safety/accessibility notice, language, source, and freshness are product/domain state where represented.
2. Authorized manual schedule, state, queue, wait, capacity, wayfinding, notice, targeting, publishing, confirmation, offline-awareness, and restoration operations are core capabilities.
3. Who may change or publish is permission.
4. External synchronization is an independent add-on or tier candidate; connection and consumption counts are limits.
5. Advanced coordination, analytics, approvals, identity, AI, premium localization, and managed hardware remain later tier/add-on candidates.
6. Temporary release control remains a rollout flag and cannot stand in for product state.

## Deferred questions

RWP-00.67 and later must finalize required capabilities, optional capabilities, classification, tier mapping, onboarding, dashboard, analytics, validation, owner decisions, hierarchy, limit counting, privacy/retention, source precedence, and downgrade behavior.
