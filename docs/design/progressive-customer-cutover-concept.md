# Scheduled Progressive Customer Cutover (Blue/Green) Concept

## Status and purpose

This document records an exploratory Vennusign deployment concept. It is not an approved work package, architecture decision, implementation authorization, roadmap commitment, or claim that a routing or scheduling mechanism currently exists. The milestone for this work is not yet known; this doc exists so the intent is captured rather than lost.

The concept is a deployment strategy in which a new release ("green") runs alongside the currently-live release ("blue"), and customers are moved from blue to green progressively, on a schedule, rather than in a single atomic cutover.

## Product intent

- A new release is published as green while blue continues serving customers unchanged.
- Customers are moved from blue to green over time, following a schedule, rather than all at once.
- Movement should be deterministic and auditable per customer (or per organization/venue — see Decisions Required), not a statistical per-request coin flip. Once a customer is moved to green, they stay on green until deliberately moved again.
- A partially-complete rollout can be paused or reversed without affecting customers who have not yet moved, and ideally without disrupting customers already healthy on green.

## Why this is not simply DNS-level weighted routing

Azure Traffic Manager (or similar DNS-weighted routing) can split traffic by percentage, but the split is approximate: DNS answers are cached by resolvers and clients, so a given customer is not reliably pinned to one side, and there is no natural concept of "this organization" or "this customer" at the DNS layer. A schedule-driven, per-customer cutover needs a decision point that knows which customer is making the request, which DNS-level routing does not provide on its own.

## Relationship to the existing deployment model

`docs/operations/DEPLOYMENT_VERSIONING.md` establishes that Vennusign already builds and promotes immutable, versioned artifacts, and that production never rebuilds a staging-approved component. This concept assumes that model rather than replacing it: blue and green are two already-built, already-promoted artifact versions running concurrently. The cutover mechanism decides which version a given customer's traffic reaches; it does not rebuild, reconfigure, or re-version the artifacts themselves.

## Illustrative example

A new API and Back Office release is approved and deployed as green. Rollout begins at 10% of organizations. Six hours later, with no new errors reported for those organizations, rollout advances to 50%. After 24 hours with a clean signal, rollout completes to 100% and blue is retired for that release. If an issue is detected at any point, remaining organizations stay on blue, and organizations already moved to green can be reverted deterministically.

## Design considerations

- **Unit of assignment.** Whether the schedule moves individual users, venues, or whole organizations needs a decision. Organization-level assignment is likely the more coherent unit given Vennusign's existing organization/venue model, but this is not decided here.
- **Real-time connections.** Display maintains a long-lived SignalR connection (`/hubs/vennusign`). A cutover must not abruptly disconnect an active screen mid-session. Any implementation needs a defined behavior for in-flight real-time sessions — for example, pinning an already-connected session to its current version until a natural reconnect, rather than forcibly severing it.
- **Schedule representation.** What actually advances the rollout — a time-based ramp (e.g. 10% at hour 0, 50% at hour 6, 100% at hour 24), explicit per-organization allow-listing, or manual approval gates between steps — is undecided.
- **Rollback.** Moving a customer back to blue must be at least as safe and immediate as moving them forward.
- **Observability.** The diagnostic concept in `docs/design/customer-support-diagnostic-agent-concept.md` already treats "deployment version" as a correlatable field. A cutover mechanism should report, per customer, which version they are currently assigned to, so that concept's causal analysis can account for version skew during a rollout.
- **Where the decision is enforced.** Candidate options include an edge/gateway layer in front of Vennu.Api (and possibly in front of the frontend apps), or a per-customer flag read inside the API itself. These have different operational and cost implications and are not decided here.

## Candidate approaches (not decisions)

1. **Application-level routing.** A gateway or the API itself inspects an organization identifier on each request and consults a rollout-assignment table to decide which backend version instance handles it.
2. **Feature-flag-driven single deployment.** Instead of two fully separate running copies, one deployment reads a per-customer flag and switches internal behavior. This may be cheaper to run but requires the application to support two behavior paths simultaneously, which may not fit the binary artifact-promotion model in `DEPLOYMENT_VERSIONING.md`.
3. **Hybrid.** Coarse infrastructure-level routing (e.g. Traffic Manager or Front Door) for failover, combined with an application-level assignment table for the actual customer-facing rollout percentage.

## Decisions required before planning

- Unit of assignment: organization, venue, or individual user.
- Schedule shape, and who authors and approves it.
- Behavior for active real-time (SignalR) sessions during a customer's cutover.
- Rollback and abort semantics.
- Where the routing/assignment decision is enforced (gateway vs. in-application).
- How this interacts with the immutable release-manifest/versioning model already in place.
- Observability requirements, including per-customer version visibility tied to the diagnostic-agent concept.
- Cost and operational ownership of any new routing or gateway component.
- Interaction with the subdomain/hosting structure currently being established (`app.<service>.vennusign.com` per-environment, per-service pattern) — whether blue/green requires two app instances per production service, and how that maps to that naming scheme.

No implementation should begin from this concept alone. Approved scope, issue and work-package governance, architecture review, and acceptance criteria remain necessary.
