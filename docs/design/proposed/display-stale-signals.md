# Stale-screen signals on the guest board

**Status: Proposed — not yet approved.** Repository presence does not constitute design approval.

- Wireframes: `display-stale-signals.html` (interactive; the dimmed mark animates)
- Full sheet: `display-stale-signals.png`
- Per-treatment crops: `display-stale-signals-00.png` … `-06.png` (`-00` is the current build)
- Affected code: `src/display/src/displayPresentation.mjs`, `getConnectionPresentation`

## The problem

The player currently prints a black box in the top-left of the wall reading **“Live updates unavailable — current content remains on screen.”** It is returned `visible: true` for three of the four connection states — `connecting`, `reconnecting` and `degraded` — so a guest sees it during an ordinary reconnect as well as during a genuine outage.

The message is accurate and aimed at the wrong person. A guest reading a menu cannot act on it and cannot fix it; they learn only that the venue's systems are unwell. The thing they do have a stake in — whether these prices and sold-out marks are real — is not addressed.

It also takes the strongest position on the board: the top-left corner, and the only element carrying a border and a drop shadow, so it reads as more urgent than the menu beneath it.

## What is already decided

Two existing decisions bound this, and neither is being questioned here.

- **Decision 5** — “Blocked is not the same as absent. Permission, disconnection, limits and offline targets are real states and must say exactly what they are.” Disconnection stays named honestly. The proposal moves *where*, not *whether*.
- **Owner amendment, 2026-08-13** — “Guest copy is **Sold out**; staff copy is **86**.” This is the governing precedent: one fact, two vocabularies, chosen by who is looking. The current banner is staff vocabulary on a guest surface.

Also relevant: **decision 14** — the venue chooses what shows when nothing is published, rather than the product generating it. Treatment 06 follows the same principle for guest-facing outage copy.

## The argument

Today's banner escalates on **connection state**. It fires the instant a socket drops and says the same sentence at four seconds as at four hours.

What actually changes over time is the probability the board is **wrong** — an 86 lifted, a happy-hour price expired — and that is the only part a guest has a stake in. Disclosure should follow that risk.

## Treatments

| # | Treatment | Recommendation |
|---|---|---|
| 00 | As it ships — banner, three of four states | Replace |
| 01 | Say nothing at all | Adopt, for short interruptions |
| 02 | Venue mark dims and breathes | Adopt, as the everyday signal |
| 03 | “Menu as of 7:42 pm” — a time, not a status | Adopt late |
| 04 | Diagnostics on demand (long-press / `?staff=1`) | Adopt, replacing the banner for staff |
| 05 | Withhold claims that depend on being current | Design further |
| 06 | Guest-language line, venue-authored | Adopt last |

**01 — Say nothing.** A brief reconnect changes nothing a guest can perceive; the prices on the wall are still the prices. The truth goes to the Screens page, which already reports Online/Offline per screen. This alone covers `connecting` and `reconnecting`, which are the network working rather than a fault.

**02 — The mark beside the venue name.** The dot before the venue name is board furniture in every state. When the screen stops listening it desaturates and breathes slowly. Staff who have been told what it means read it across a room; a guest reads a design detail. No words to misread, and no layout cost.

**03 — A time, not a status.** “Menu as of 7:42 pm” states a fact about the content rather than a fault in the plumbing. It earns its place only once the gap is long enough to matter — on a board thirty seconds behind it invites a question nobody needed to ask.

**04 — Diagnostics on demand.** Elapsed time, last content, revision and screen id, behind a deliberate gesture: a long press in a corner, a key on the paired remote, or `?staff=1`, shown for fifteen seconds. Strictly better than a permanent banner even for the person the banner was written for — a banner affords six words, this affords everything.

**05 — Withhold the claims you can't stand behind.** The real danger of a stale board is not that it looks stale; it is that *Sold out* may have been lifted an hour ago, or a happy-hour price expired. Rather than announce a fault, stop making the claims that depend on being current and let the durable ones stand. This is the strongest idea here and the one with real product consequences — it needs its own decision, because it changes what the board asserts, not merely how it looks.

**06 — Guest words, and only at the end.** Past the point where the board may genuinely mislead, silence stops being kind. The honest sentence is about what the guest should do — “Please ask us about today's specials” — not about a socket that will not open. It is hospitality copy, so the venue writes it.

## Proposed ladder

| Elapsed | What it means | Guest sees | Staff sees |
|---|---|---|---|
| 0–60s | A reconnect. Content still correct. | Nothing. | Nothing on the wall; Screens page still Online. |
| 1–15 min | A real drop. Content almost certainly still correct. | Nothing. | Venue mark dims and breathes; Screens page flips to Offline. |
| 15–60 min | An 86 or price change could have been missed. | Volatile claims withdrawn — no sold-out marks, no happy-hour pricing. | Dimmed mark, bottom hairline, full detail on demand. |
| 60 min + | The board may be actively misleading. | The venue's own line, e.g. “Please ask us about today's specials”. | All of the above, plus the timestamp printed. |

## What outage testing showed (2026-08-22)

Three deliberate API outages against a live dev screen, 158s, 180s and one shorter, changed two things in this proposal.

**There are two guest-visible offline messages, not one.** This document was written against the connection banner alone. The second lives at the bottom of the screen — `describeCachedContent`, rendered by `DisplayPage.tsx:218` whenever `state.source === 'cache'`:

> Offline — showing saved content from 5 minutes ago. New updates will appear when the connection returns.

It only appears when the content fetch fails as well as the socket, which is why it is easy to miss: killing the socket alone shows the top banner by itself. Any decision taken here has to cover both, or the wall simply swaps one system message for another.

Its copy is also the better of the two, and closest to treatment 03. It describes the *content* and its age rather than naming a socket, and it tells the reader what will happen next. If one of these two is the model for the rest, it is this one.

**The player is more resilient than the banner implies.** Three independent layers, all exercised:

1. the realtime socket, which now retries indefinitely (#768)
2. the 60s recovery poll, which carries content when the socket is down
3. a `localStorage` cache (`displayCache.mjs`), which carries content when nothing is reachable at all — verified by reloading the page with the API fully stopped, so it survives a device reboot mid-outage

That third layer is the strongest argument in this document. The product is deliberately built to keep drawing a correct-looking board through an outage, and it succeeds: a full page reload with no API produced the menu, not a blank screen. Announcing a fault to a room of guests contradicts the thing the system is actually doing on their behalf.

It also sharpens the question. With three layers holding the content up, "is the connection down" is almost never the interesting question — "is what is on the wall still true" is, and only staleness answers that. Which is what the ladder below already proposes.

## Open questions for review

1. **Treatment 05 needs an owner decision of its own.** Withdrawing sold-out marks when stale is arguably *more* misleading than showing a possibly-lifted one — a guest orders something genuinely unavailable. The opposite reading is that a stale *Sold out* costs a sale that was available. Which error the venue would rather make is not ours to choose.
2. **The 15 and 60 minute boundaries are placeholders.** They should come from how often a venue actually 86s something during service, which we do not currently measure.
3. **Does the dimmed mark survive the venue's own theme?** Themes set `SectionColors` and accents per venue; a signal built from the accent has to stay legible in all of them.
4. **Treatment 04's gesture must not be discoverable by accident** on a screen mounted within reach of guests.

## Not covered

This proposal is about what the wall shows. It does not address why the wall goes stale — see the realtime-delivery work around publish notification (#763), which found that displays join `screen:{id}` while every content notification was broadcast to `venue:{id}`.
