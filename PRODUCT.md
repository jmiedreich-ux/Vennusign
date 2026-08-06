# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Hospitality venue operators (bars, restaurants). The primary user is the owner "in the thick of it": on a laptop or phone at the venue, working in short interrupted bursts — change a price, check the TVs, get out. The same product must also serve occasional longer desk sessions (bulk menu edits, scheduling, reviewing), owner-first when the two conflict. Staff roles exist with real authority boundaries: Organization Owner, Content Editor (edits content, cannot publish or touch screens), Publisher (publishes and recovers deliveries, cannot edit content or target screens).

## Product Purpose

VennuSign runs the venue's TVs: menus, tap lists, schedules, promotions and emergency broadcasts, authored in a Back Office and delivered to physical display players. Success is the owner seeing what is on their screens right now, changing it quickly, and knowing with evidence that the change landed.

## Positioning

Truth-first signage: the UI never claims more than the server can prove. Delivery is reported as requested / received / applied and "applied" is only shown when the applied revision matches the authoritative one. Every refusal is a server capability decision with a reason — permission, plan entitlement, allowance limit, or temporary rollout block — not a client guess.

## Operating Context

- A venue has menus (sections, items, prices, translations), screens (paired physical TVs with heartbeat status Online/Offline/Stale), schedules (meal periods, happy hour, playlists, date-range promotions, emergency broadcasts), themes/layouts, and optional POS sync (Clover, Square, Toast).
- The operating loop: edit content → preview exactly what the TV will show → push → verify delivery state → recover (reset/reconnect) when a player drops.
- Screens can be Offline for long stretches; queuing a push to an offline screen is normal and must read as "will recover on reconnect", never as displayed.
- Plan allowances limit quantity (e.g. 1 active screen); reaching a limit must block only adding more, never correcting what exists.

## Capabilities and Constraints

- Server-authoritative capability decisions per session: allowed / denied (permission) / unavailable (not in plan) / temporarily-blocked (rollout), each with reason code, message key, structured parameters (e.g. used/limit), resolved locale, correlation ID.
- Three distinct kinds of "no" that must never look alike: permission ("ask an administrator"), entitlement ("your plan does not include"), temporary block ("try again later").
- Localization with fallback chain (e.g. fr-CA → fr → en-US); messages change language, decisions never change.
- Capability vocabulary (publish/confirm/replace/target) is internal; the customer-facing vocabulary is the product's own (e.g. "Push").
- V1 scope: no hosted deployment, no live POS in acceptance, single venue per session in practice.

## Brand Commitments

The "sky" identity is binding: light surfaces, sky-blue primary `#87ceeb` on slate ink (`#0f172a`), the existing semantic status colors (live green `#178a52`, off red `#b03a33`, warning amber `#c9871a`, emergency red, promotion purple). Redesign may restructure layout, composition, hierarchy and components, but the product must still read as this light, sky-blue family. Name: VennuSign.

## Evidence on Hand

- Deterministic acceptance fixture: Harbor Acceptance Venue, Acceptance Menu (Featured → Harbor Lemonade $4.50), Acceptance Screen (Offline), tokens track1-owner-review / track1-content-editor / track1-publisher.
- Real capability decision payloads observable at `/api/back-office/session`.
- Track 1 owner acceptance (2026-08-06) closed "Needs adjustment": stable, but UX usability on many screens is the named gap. Five recorded themes: role identity not first-class; generic permission refusals; three kinds of "no" indistinguishable on controls; product-vs-model vocabulary split; scattered disclosure on Screens.

## Product Principles

1. State before controls: the owner sees what is true on the TVs before being offered anything to do about it.
2. Never claim what the server can't prove; delivery and refusal wording carries evidence (revisions, counts, reasons).
3. A limit blocks only growth, never repair.
4. Every "no" says which kind it is and what changes it, in venue language.
5. Learnable in the thick of it: each screen's primary function must be performable without training or documentation.
