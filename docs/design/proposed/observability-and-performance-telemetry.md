# Observability, correlation and performance telemetry

**Status: Proposed — not yet approved.** Repository presence does not constitute design approval. Nothing here is scheduled or started.

Companion to `../customer-support-diagnostic-agent-concept.md`, which describes what a support agent should be able to *conclude*. This document is about the evidence that would have to exist first, and adds the performance dimension that concept does not cover.

## Why now

On 2026-08-22 the owner asked a simple question — how long does it take to drag a menu section into a new order — and it could not be answered from anything the product records. There is no Application Insights component in the subscription, HTTP logging to storage is disabled, and the container logs contain lifecycle lines only (`Application started`, `Hosting environment`). The only way to answer was to build an instrumented Playwright probe and reproduce the action.

That is the cost being carried today: **every question about behaviour becomes a reproduction exercise**, and reproduction only works for problems that reproduce. The ones that matter usually do not.

The same session produced two defects that had been true for a long time and were invisible for the same reason — #769 (no display has ever received a venue-scoped notification) and #770 (a deployed display never picks up a new build). Both were masked by a 60-second recovery poll quietly doing all the work. Delivery telemetry would have shown a chain that stops at step two.

## The central design decision: measure the action, not the request

The reorder measured at **3,981 ms**. The `PUT /sections/order` behind it returned **204 in well under a second**.

Request-level logging would have shown a healthy API and missed the entire problem, because the action was `PUT` → `refresh()` (four parallel GETs) → render. A team looking at server metrics alone would have concluded the backend was fine and gone looking for a bigger machine.

So the unit of measurement is the **user-perceived span**, and every span is decomposed into server time, network, and client time:

```
publish → wall redrawn = 12,757 ms
  ├─ server                3,598 ms   ← code, query, or contention
  └─ client               ~9,000 ms   ← player refetch and render
```

Without that split it is impossible to tell "our code is heavy" from "our box is small", and the likely outcome is buying hardware for a client-side problem.

### Spans worth naming first

| Span | From | To |
|---|---|---|
| `menu.section.reorder` | drop | rail repainted |
| `menu.item.add` | create clicked | board repainted |
| `menu.publish` | publish clicked | publish accepted |
| `content.delivery` | publish accepted | player acknowledged |
| `backoffice.entry` | navigation | first usable paint |
| `display.recover` | connection lost | connection restored |

## Percentiles, not averages

Support asks *what happened to this venue at 18:04*. Performance asks *what the distribution looks like across every venue*. Same instrumentation, different aggregation — and performance needs **p50 / p95 / p99**.

An average of 61 ms hides the venue on hotel wifi at four seconds, and the p99 is usually where the product actually feels broken.

## The delivery metric already exists and is discarded

Every player posts content receipts carrying `AuthoritativeRevision`, `AppliedRevision`, `State` and `AppliedUtc`. Every publish records `PublishedUtc`.

**`AppliedUtc − PublishedUtc` is publish-to-wall latency, per screen, continuously, across every venue.**

The 12,757 ms figure obtained by hand with a Playwright probe could be a live metric derived from data the product already sends and throws away. This is persistence and aggregation of an existing signal, not a new pipeline, and it is the cheapest first win available.

## Correlation: organization → venue → user

The identity spine exists — CIAM identity, the `X-Vennusign-Venue-Id` header, venue-scoped repositories throughout. What is missing is that nothing stamps a correlation id through a request and nothing writes a structured record when it completes.

Minimum viable, in order:

1. a correlation id generated at the edge and returned to the client, so a customer can quote it;
2. structured request records carrying `orgId`, `venueId`, `userId`, `screenId`, `route`, `outcome`, `durationMs`, and version stamps;
3. a queryable sink.

"Replay" then means filtering by venue and time window.

### What must not be logged

Identifiers and outcomes, never payloads. `orgId`, `venueId`, `userId`, `screenId`, `menuId`, revision numbers, durations, status codes — never menu item names, prices, guest-facing copy, or user emails.

This is not an authorization question, it is a data-protection one: logs full of customer content mean reading logs is reading customer data through a side door. The support concept already draws this line.

### What this is *not* blocked on

Three distinct tiers, and only the third is a new permission surface:

| Tier | What | Gated by |
|---|---|---|
| 1 | Application logs and telemetry | Azure RBAC. No product permission involved |
| 2 | Platform Operations read views | Internal key and explicit permission claims — **already exists** |
| 3 | Acting inside a venue as a venue admin | Does not exist; a genuinely new surface |

`docs/architecture/administrative-identity.md` already settles tiers 1 and 2: "The internal Vennusign support and platform console is **Platform Operations**… Back Office membership never grants Platform Operations access." Platform Operations already reads and writes venue-scoped data.

**Observability does not wait on any impersonation decision.**

## OpenTelemetry

Recommended as the instrumentation API and wire format, with the depth varying by tier.

- **`Vennu.Api` (.NET) — full OTel.** The Azure Monitor OpenTelemetry Distro is production-ready and App Insights ingests OTel natively, so the neutral format costs nothing.
- **W3C trace context (`traceparent`) everywhere — the part that matters most.** It is what lets one trace id survive browser → API → SignalR → player. Without it there are three dashboards instead of one span, which defeats the purpose.
- **Browser and player — pragmatic.** The OTel JS SDK is heavier and less mature than the .NET one, and the player runs on Tizen, webOS and Android TV, where the current bundle is 238 KB. Adding a large SDK to a TV app in order to measure it would be self-defeating. Measure the bundle cost first; a thin emitter speaking OTLP without the full SDK is a legitimate outcome.

## Cost and fidelity

Full-fidelity tracing of everything is expensive, and the instrumentation itself adds latency.

- **Tail-based sampling.** Keep 100% of slow and failed spans, sample fast ones hard. The p99 is what is needed and it is the cheapest part to keep.
- **Version-stamp every span** — API commit, player version, shell version, schema version. Without this a regression cannot be distinguished from a busy Tuesday, and comparing across deploys is the entire point.
- **Capture the baseline before optimising.** The reorder improving from 3,981 ms to 61 ms is only demonstrable because it was measured first.

## What it buys

| Question | What answers it |
|---|---|
| Should this be coded better? | Server-time share of the span, and which call dominates it |
| Is this feature heavier than we assumed? | p95 for the action against its peers, and whether it scales with menu size |
| Do we need more or different resources? | Span latency correlated against plan CPU and memory |

The third is worth dwelling on. On 2026-08-22 the plan was scaled from B1 to B3 partly on a 97% CPU reading, but it was never established that the failing test runs were CPU-bound. That decision was taken without the data that would have justified it — which is the normal situation today, not an unusual one.

## A cheaper interim step: kept in the browser, pulled on demand, no database

Owner idea, 2026-08-23, prompted by #800's temp `[perf:deliver]` console instrumentation actually catching the add-item slowness live. The question behind it: since the display already holds a per-device SignalR connection, could the same console data just be pulled over that connection on demand, instead of building the full pipeline above first?

**Not quite as posed, but close.** The back office — where #800's logging actually lives — has no socket connection at all today (`VennuHub.JoinVenue` exists, unused, "kept for a possible future back-office-only consumer" per its own comment). The display does have one, and already keeps a rolling per-device record in `localStorage` (`displayDiagnostics.mjs`) — but that record is deliberately read back only by that same device's own `/diag` page, never sent anywhere. Nothing today lets a developer ask a *live, currently-connected* client "send me what you're holding."

**The idea that survives:** a client-kept event ring buffer (what `displayDiagnostics.mjs` already does) plus an on-demand pull over whatever socket that client already holds, with **no SQL storage and no ingestion pipeline** — the opposite end of the spectrum from full OTel above. Cheap, no new infrastructure, but narrower: point-in-time inspection of one live session, not percentiles across venues, not retained history, not usable once the client disconnects or reloads.

Where this could fit relative to the order below: a proof-of-concept for step 2 (correlation ids, structured records, a sink) rather than a replacement for it — it answers "what is this one screen/session doing right now" cheaply, but doesn't accumulate the history percentiles or regression-across-deploys comparisons need. Whether it's worth building as its own small step, or just as validation before committing to the fuller pipeline, is an open call for whoever picks this up.

## Suggested order

1. **Surface what already exists** — publish and delivery trail as a Platform Operations view, plus `AppliedUtc − PublishedUtc` as a metric. No new infrastructure.
2. **Correlation ids, structured request records, and a sink.** Unblocks everything else.
3. **Lifecycle events across the delivery chain**, filling the blind middle between `Published` and `PlayerAcknowledged`. Would have surfaced #769 and #770.
4. **Client spans** for the named user actions, with the server/network/client decomposition.
5. **Usage telemetry**, deliberately last and deliberately separate.

## Usage telemetry is a different product

Support answers *what happened to this venue at 18:04*. Usage answers *what venues actually do*. Different consumers, different retention, different privacy exposure. Sharing a collection pipe is reasonable; sharing a dataset is how personal data ends up in an analytics warehouse.

## Open questions

1. **Retention and residency.** How long is diagnostic data kept, and does any of it leave the tenant?
2. **Sampling floor.** What percentage of healthy spans is worth keeping for baselines?
3. **Player cost.** What is the real bundle and CPU cost of client spans on the oldest supported TV?
4. **Who sees performance data?** Tier 2 is settled for support views; whether venue operators see their own screen latency is a product decision, not an infrastructure one.
5. **Does the existing content-receipt trail have enough fidelity**, or does the delivery chain need its own events?

## Related

- `../customer-support-diagnostic-agent-concept.md` — what a support agent should conclude from this evidence
- #769 — no display has ever received a venue-scoped notification
- #770 — a deployed display never picks up a new build
- #755 — the builder waits on four API calls and cannot say which one it is waiting for
