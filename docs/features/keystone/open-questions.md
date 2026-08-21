# Keystone — Open Questions Register

- **Status:** 34 questions — **31 answered, 3 deferred, none outstanding** (owner sitting, 2026-08-20).
  Raised from the six brainstorming sittings recorded in `decisions-so-far.md` and from the
  concept's own `Decisions required before planning` list.
- **Owner deviations of note:** Q8 rejected the recommendation as not answering the question and
  is re-asked with a revised one. Q19 **overrides** its recommendation on information-disclosure
  grounds and becomes decision 49. Q3, Q7, Q13 and Q17 accept with refinements worth reading.
  Q30 is re-asked because the issue number was unfamiliar. Q31–Q33 are deferred and run
  provisionally on their recommended defaults.
- **Authority context:** `docs/design/proposed/keystone/decisions.md` (49 decisions). Nothing here
  re-asks those. Where a question touches a decision, the decision number is cited.
- **Note on the four originally-named items.** Unit of assignment is settled (decisions 22–25).
  The trust boundary for forwarded webhook requests is settled (decisions 31–34). The shared
  connection mechanism is parked as **#742**. **Behaviour when VDS is unavailable was never worked
  in the sittings** and is Q1–Q4 below.

## How to answer

Every question has three valid answers:

1. **Accept the recommended default** — say `rec`.
2. **Your own answer** — a sentence is enough.
3. **Decide later** — say `defer`. Deferrals stay tracked here; if a slice cannot wait, the
   recommended default is used *provisionally* and flagged in that slice's acceptance workbook so
   the consequence is visible live and cheap to overturn.

Shorthand works: `Q1 rec, Q5 defer, Q7: no more than three`. Unanswered questions are treated as
**defer**, never as silent acceptance.

**BLOCKING** = slice 1 or 2 needs the answer (or uses the provisional default) before that work
starts. *important* = needed before the slice that builds it. *minor* = cheap to change late.

---

## VDS availability — the unworked item

### Q1 · BLOCKING

**What does the Product Router do when VDS is unavailable?**

The Router sits on every request and cannot answer without VDS. Failing closed takes the whole
product down for an outage in a component that exists to make outages survivable — but serving
from stale data can send a customer to a version they were deliberately moved off, which is the
one thing assignment exists to prevent.

*Recommended:* Serve from a cached last-known assignment, with no expiry during an outage, and
emit a loud degraded-mode signal. Rationale: assignments change rarely and deliberately (decision
27's pointer advances are deliberate acts; waves move customers at windows), so a cached
assignment is almost always still correct, whereas an unavailable Router is certainly an outage.
A rollback in progress during a VDS outage is the bad case, and it is rare and detectable.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 11, 12, 17; concept "Decisions required before planning"</sub>

### Q2 · BLOCKING

**What does the POS Webhook Receiver do when VDS is unavailable?**

WR's failure mode differs from the Router's: POS providers retry, and a dropped webhook is lost
sales data rather than a visible outage. The concept names queue-or-cache without choosing.

*Recommended:* Queue, and drain when VDS returns. WR is not latency-sensitive from the provider's
point of view (a fast 200 then asynchronous processing is the normal shape), so queueing loses
nothing and avoids forwarding to a stale version.

*Answer:* Accepted recommended (2026-08-20).

<sub>concept "POS Webhook Receiver"; decision 33</sub>

### Q3 · BLOCKING

**How long may a cached assignment be served, and does it differ between the Router and WR?**

Q1's recommendation says "no expiry during an outage." That is deliberate but needs a bound
stated, or the cache silently becomes the source of truth.

*Recommended:* No time expiry, but a hard invalidation on VDS returning, plus a degraded-mode
alert from the first cache-served request. Time-based expiry converts a VDS outage into a
staggered product outage, which is worse than serving slightly stale routing.

*Answer:* Accepted with a refinement (2026-08-20): no expiration, with invalidation when a new version becomes the default as well as when VDS returns.

The refinement is safer than it looks, for a reason worth recording: **PO writes assignments through VDS**, so while VDS is unreachable no assignment and no default-pointer change can occur. A cache served during an outage is therefore exactly as correct as it was when the outage began, rather than merely probably-correct. The residual risk is a *partial* failure — VDS reachable by PO but not by the Router — where changes can land that the Router does not see. That case is why the degraded-mode alert fires from the first cache-served request.

<sub>Q1; decision 12</sub>

### Q4 · important

**What do assignment-aware background services do when VDS is unavailable?**

Not Keystone's code — these live in the versioned API — but Keystone's contract determines the
answer. The concept states a preference without deciding: "Processing no venues is safer than
processing all of them."

*Recommended:* Process no venues, and alert. Unlike request routing, background work deferred by
minutes is recoverable, whereas two versions both processing every venue is not.

*Answer:* Accepted recommended (2026-08-20).

<sub>concept "Background services"</sub>

---

## VDS contract

### Q5 · BLOCKING

**Does VDS return a version only, or a version and a resolved instance?**

The concept says VDS delegates instance discovery to ADS *internally*, so callers see one lookup.
That still leaves whether the Router receives an instance to forward to, or receives a version and
resolves the instance itself.

*Recommended:* Version and a resolved target. One round trip on a hop that is paid on every
request, and it keeps ADS entirely invisible to callers as the concept intends.

*Answer:* Accepted recommended (2026-08-20).

<sub>concept "Application Discovery Service (ADS)"; decision 45</sub>

### Q6 · BLOCKING

**Does ADS feed a load-balancer backend pool, or does the Router pick the instance?**

The concept decided ADS "feeds a standard per-version load-balancing layer (an Azure Front
Door/Application Gateway backend pool)". No Front Door, Application Gateway or APIM exists
anywhere today, and the Product Router would be the first gateway hop Vennusign has. Adding a
managed load balancer is a cost decision adjacent to the deferred tier conversation.

*Recommended:* The Router picks, from the healthy set ADS reports, until a measured reason to add
a load-balancing layer appears. It avoids introducing a paid component into a design whose cost
conversation is deliberately parked, and the Router is already on every request.

*Answer:* Accepted recommended (2026-08-20).

<sub>concept "Multiple instances per (app, version)"; decisions 40–43</sub>

### Q7 · important

**How many versions may run concurrently, and what bounds that number?**

Named in the concept's required list. It bounds plan sizing, `release/X.Y` branch policy, and how
long a wave may straddle.

*Recommended:* Three, as a configured maximum rather than an architectural limit, with the release
cut refusing to register a fourth until one retires. Matches the informal "2–3" already noted for
release branches.

*Answer:* Owner answer (2026-08-20): configured in Platform Operations rather than fixed in the design. No number is set here. PO already owns registration, so PO holds the limit and enforces it at the point a version is registered.

<sub>concept "Framing"; "Decisions required before planning"</sub>

### Q8 · important

**What does VDS return for a venue with no assignment?**

A brand-new venue created during onboarding has never been assigned. Decision 28 says new
customers start on the default version, but that could be an explicit assignment written at
creation or a fallback computed at lookup.

*Recommended (revised 2026-08-20, after the owner rejected the first attempt as not answering the
question):* VDS returns an explicit **not-assigned** result — a distinct outcome, neither an error
nor a silently substituted version. The Router then treats a not-assigned venue as unattributed
traffic and applies decision 27, so the default-version fallback lives in exactly one place rather
than two.

Separately, and *not* as the answer: venues should still receive an explicit assignment at creation,
so that not-assigned stays rare and indicates a defect rather than a normal path.

*Answer:* Closed 2026-08-20 as already answered by the authority. The owner's first response
rejected the original recommendation as not answering the question, which was correct; the second
observed that decisions 27 and 28 already settle it. They do: a venue with no assignment resolves
to the default version, and new venues are assigned there at creation. No separate rule is needed.

<sub>decisions 27, 28</sub>

---

## ADS

### Q9 · important

**Health poll interval, and how many consecutive failures mark an instance unhealthy?**

*Recommended:* Poll every 10 seconds; unhealthy after 3 consecutive failures; healthy again after
2 consecutive successes. Fast enough that a crashed instance leaves the pool in well under a
minute, slow enough not to be a load source itself.

*Answer:* Accepted recommended (2026-08-20).

<sub>concept "Why this needs to be a separate, continuously-running concern"</sub>

### Q10 · important

**How is ADS's registration endpoint authenticated?**

The deploy pipeline calls it to register a newly-healthy instance. An openly callable endpoint
would let anyone insert a routing target, which is a traffic-hijacking primitive.

*Recommended:* The same asymmetric signed-token scheme as decisions 31–33, with the pipeline
holding its own key identity, plus network restriction. One mechanism rather than two.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 31–33; concept "Registration is automated, not manual"</sub>

### Q11 · important

**What happens when every instance of a version is unhealthy?**

Customers are assigned to that version and there is nowhere healthy to send them.

*Recommended:* Serve 503 with a retry hint, alert, and do **not** silently reroute to another
version. Silently serving a customer a different version than they are assigned to is the failure
this whole feature exists to prevent, and it would do so at the exact moment nobody is watching
the right thing. Rerouting is an operator decision through PO, not an automatic one.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 11, 25, 35</sub>

---

## Product Router

### Q12 · BLOCKING

**What is the Product Router built with?**

It is the first gateway hop Vennusign has had and sits on every request.

*Recommended:* YARP. It is Microsoft's .NET reverse proxy library, purpose-built for programmable
per-request routing, and the team and repository are .NET 9. Note decision 18 holds regardless:
the wire contract, not a shared assembly, is what binds Keystone to concurrently-running versions.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 14, 18; AGENTS.md "Target .NET 9"</sub>

### Q13 · important

**What latency budget does the Router hop get, and what happens when it is exceeded?**

Decision 43 says the Router graduates to its own plan when its added p99 becomes measurable, which
presumes a number to measure against.

*Recommended:* 15 ms added p95 as the budget, measured from the first slice that puts it on the
request path, with breach as a capacity signal rather than a failure.

*Answer:* Accepted with a requirement (2026-08-20): the budget stands, and the Router latency KPI must be surfaced front and centre rather than merely collected. It belongs on the version detail view an operator actually works from, alongside the cohort-health signals of Q27, not buried in a dashboard nobody opens.

<sub>decisions 41, 43</sub>

### Q14 · important

**Where does DNS point, and what terminates TLS?**

Today five App Services each serve their own hostname. With a Router in front, something must own
the customer-facing name.

*Recommended:* DNS points at the Router, which terminates TLS and forwards over the internal
network. Any other arrangement reintroduces a per-version hostname, which decision 15 forbids.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 15, 40</sub>

---

## Internal tokens

### Q15 · important

**Signing algorithm, and key rotation cadence?**

*Recommended:* ES256, rotating quarterly, with the previous public key accepted through one
rotation period so a rotation never requires simultaneous redeployment of every live version —
which decision 2 makes impossible anyway.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 31, 32</sub>

### Q16 · important

**Token time-to-live?**

*Recommended:* 60 seconds. Long enough to absorb clock skew between App Services, short enough
that a captured token is useless before it can be replayed meaningfully.

*Answer:* Accepted recommended (2026-08-20).

<sub>decision 32</sub>

### Q17 · important

**Where does the Router's private key live, and how does it reach the Router?**

*Recommended:* Key Vault, reached by managed identity, never an app setting. `kv-vennusign-dev`
already exists and already holds the project's credentials.

*Answer:* Accepted with a note (2026-08-20): the Key Vault name is environment-specific, so it is configuration and never a constant. `kv-vennusign-dev` is the dev vault only; stage and production resolve their own. The managed-identity mechanism is unchanged.

<sub>decisions 31, 37</sub>

---

## The URL restructure

### Q18 · BLOCKING

**Is the `/o/{orgId}` segment kept, given the Router only ever keys on venue?**

Decision 25 makes the org segment non-routing. It earns its place for the application — org-scoped
surfaces, multi-venue functions, reporting — but it is redundant to Keystone and makes every URL
longer.

*Recommended:* Keep it. Org-scoped surfaces need a coherent home that is not "under an arbitrary
venue", and it expresses the hierarchy honestly. The cost is URL length; the benefit is that
`/o/{orgId}` is a real place.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 19, 25</sub>

### Q19 · important

**What happens when a URL carries a valid `venueId` under the wrong `orgId`?**

Bookmarks go stale, venues can in principle move between organizations, and the segment is
forgeable.

*Recommended:* The Router ignores the org segment entirely (decision 25) and routes on the venue.
The application then corrects the URL to the venue's real organization, so a stale link
self-heals rather than erroring.

*Answer:* Owner answer (2026-08-20), **overriding the recommendation.** The recommendation was wrong: silently correcting the URL to the venue's real organization discloses that organization to anyone who guesses a venue id.

Correct behaviour: the Router still ignores the org segment and routes on venue (decision 25 is unchanged), but what happens next runs through the ordinary security-role and authentication path. **The response must never hint that the venue might belong to another organization.** URL correction is permitted only after authorization succeeds — a caller entitled to the venue may have the link tidied. An unauthorized caller receives the identical refusal they would get for a venue that does not exist, revealing neither its existence nor its owner. Recorded as decision 49.

<sub>decisions 11, 19, 25</sub>

### Q20 · important

**Do the front ends move to relative API URLs, and is anything blocked by that?**

Decision 13 depends on it: the tenant is inherited from the bundle's own path only if calls are
relative. Today `loadBackOfficeConfiguration` supplies an absolute `apiBaseUrl`.

*Recommended:* Yes, relative, with the absolute base retained only for local development against a
separately-hosted API. If anything genuinely cannot be relative, it must carry the tenant
explicitly rather than silently routing to the default version.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 13, 14</sub>

---

## The default-version pointer

### Q21 · important

**Who may advance the default-version pointer, and is it gated?**

Decision 27 makes it a deliberate act. It is also the single pointer that, set wrongly, affects
every sign-in.

*Recommended:* PO operators with release authority, gated behind the same approval as advancing a
wave, and never automatic on registration.

*Answer:* Accepted recommended (2026-08-20).

<sub>decision 27</sub>

### Q22 · important

**What happens if the default pointer names a version that is being retired?**

Retirement runs when no customer remains assigned — but unattributed traffic is not a customer
assignment, so a version could be "empty" and still serving every sign-in.

*Recommended:* Retirement is refused while a version is the default. The pointer must be advanced
first, which makes the ordering explicit rather than implicit.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 27, 28; concept "Release lifecycle"</sub>

---

## Waves and scheduling

### Q23 · important

**What is a maintenance window, concretely?**

The concept says windows live in the Vennu profile and are commonly daily at the same local time,
but never fixes the shape.

*Recommended:* A recurring daily local-time range per venue, with an optional per-venue override
for a specific date. Anything richer is schedule modelling that no evidence yet demands.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 23, 24; concept "Source of schedule and selection data"</sub>

### Q24 · important

**When does a wave count as observed?**

Named in the concept's required list. Windows are per-venue, so a wave does not complete at a
single moment.

*Recommended:* Every venue in the wave has passed its window **and** accumulated a defined period
of live traffic since moving. No venue advances on an observation period that did not include its
own window.

*Answer:* Accepted recommended (2026-08-20).

<sub>decision 24; concept "Automation"</sub>

### Q25 · important

**What happens when a venue is eligible under two in-flight rollouts?**

Named in the concept's required list.

*Recommended:* Newest rollout wins, and the older one records the venue as superseded rather than
skipped, so the audit trail explains why it never moved.

*Answer:* Accepted recommended (2026-08-20).

<sub>concept "Decisions required before planning"</sub>

### Q26 · important

**What are rollback and abort semantics?**

Named in the concept's required list. The constraint is stated — moving back must be at least as
safe and immediate as moving forward — but the mechanism is not.

*Recommended:* Rollback is an ordinary assignment change through the same path as a rollout, with
two differences: it ignores windows when the customer is already broken (per the concept's
corrective-release rule), and it halts the wave that produced it.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 23, 24; concept "Withdrawing a release"</sub>

---

## Observability

### Q27 · BLOCKING

**Where does per-customer telemetry with a version dimension come from?**

Nothing emits it today. Without it, cohort health cannot be assessed, which removes the point of
waves — so this is not an add-on, it is what makes progressive delivery mean anything.

*Recommended:* The Router emits it, because it is the one component that sees every request and
already knows both the tenant and the resolved version. That also makes it available before any
versioned application changes.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 11, 44; concept "Open questions carried forward"</sub>

### Q28 · important

**Where does the hint/authority mismatch signal go, and what is done with it?**

Decision 11 makes the mismatch the built-in detector for assignment drift, which only helps if
something watches it.

*Recommended:* The same telemetry pipeline as Q27, with an alert threshold rather than per-event
alerting, since a low background rate is expected from stale bookmarks.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 11, 35</sub>

---

## Secrets migration

### Q29 · BLOCKING

**Who owns moving durable secrets off Data Protection, and does existing data need migrating?**

Decision 37 settles the destination but not the path. Existing POS credentials and strong-auth
factors are currently encrypted with a per-app key ring.

*Recommended:* Owned by the `Vennu.Api` area rather than Keystone, and with no data migration —
nothing is live, so existing dev values are disposable and can be re-entered. If that turns out to
be false for any environment, the answer changes to a dual-read migration.

*Answer:* Accepted recommended (2026-08-20).

<sub>decisions 36, 37</sub>

---

## Sequencing and scope

### Q30 · BLOCKING

**Does #726 land before slice 1, or with it?**

**What #726 is:** an issue filed on 2026-08-19/20 recording that deploys never set version
environment variables, so `/health/version` always reports the local placeholder and no deploy can
be verified as actually live. `ReleaseVersionMetadata.cs:14` reads `VENNU_COMPONENT_VERSION` with a
fallback of `"0.0.0-local"`; nothing sets it. Decision 46 makes it a Keystone prerequisite because
decision 35's mis-forwarding check compares the Router's stamped version against exactly that
value, and the concept's assignment-aware background services need it to know which venues are
theirs.

*Recommended:* Before slice 1, as its own small change. It is independently valuable — it is also
what makes any deploy verifiable — and it is cheap.

*Answer:* Owner answer (2026-08-20): a separate problem to fix. #726 is not part of a Keystone
slice and does not sequence against one — it is fixed on its own. Decision 46 still holds: it must
be in place before any customer is moved, because decision 35's check compares against the value
it sets.

<sub>decisions 35, 46; issue #726</sub>

### Q31 · BLOCKING

**Does the pre-auth app split land before the URL restructure, or after?**

Decision 48 recommends splitting onboarding into its own app. The URL restructure moves back
office under a tenant prefix. Doing them in the wrong order means doing the entry routes twice.

*Recommended:* The split first. `main.tsx`'s two-way switch is already the seam, and splitting
first means the URL restructure only ever touches the post-auth app.

*Answer:* Deferred (2026-08-20). Runs on the recommended default provisionally if a slice cannot wait, flagged in that slice's acceptance workbook.

<sub>decisions 19, 21, 48</sub>

### Q32 · important

**Is provisioning automation inside Keystone's scope, or a prerequisite feature alongside it?**

Decision 47 states the gap without assigning it.

*Recommended:* A prerequisite feature alongside. The concept explicitly keeps the deployment
process outside VDS, and Keystone routing between versions is a separable concern from standing
those versions up. It cannot be nobody's, though.

*Answer:* Deferred (2026-08-20). Runs on the recommended default provisionally if a slice cannot wait, flagged in that slice's acceptance workbook.

<sub>decisions 45, 47; concept "What the Version Discovery Service is, and is not"</sub>

### Q33 · important

**Does Keystone decide the shared connection-membership mechanism, or also build it?**

Parked as **#742**. Restated here so the register is complete.

*Recommended:* Decide only. Azure SignalR Service is the one Keystone item that certainly costs
money, and tier and plan cost are deliberately deferred.

*Answer:* Deferred (2026-08-20). Runs on the recommended default provisionally if a slice cannot wait, flagged in that slice's acceptance workbook.

<sub>issue #742; decisions 1, 41</sub>

### Q34 · minor

**Does Keystone own correcting the concept document?**

**#743** records that the concept still places the version chooser at `dev.vennusign.com` and
misidentifies PO. The correction is known; the owner of the edit is not.

*Recommended:* Yes, as part of landing the design authority — the concept is Keystone's source
document and leaving a known-false statement in it sends the next reader wrong.

*Answer:* Accepted recommended (2026-08-20).

<sub>issue #743; decisions 38, 39</sub>
