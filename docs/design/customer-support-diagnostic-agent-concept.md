# Customer Support Diagnostic Agent Concept

## Status and purpose

This document records an exploratory Vennusign product concept. It is not an approved work package, architecture decision, implementation authorization, roadmap commitment, or claim that the described telemetry and agent capabilities exist.

The concept is a customer-facing support agent embedded in Vennusign that can investigate a problem affecting the customer's current session, explain what happened, help the customer recover safely, and preserve a deeper root-cause report for the Vennusign owner and engineering team.

## Product intent

When Vennusign crashes, behaves unexpectedly, fails to publish content, loses a display connection, or otherwise prevents a customer from completing an operation, the customer should be able to ask for help without manually collecting logs or translating technical symptoms.

The support agent should:

1. identify the affected organization, venue, user session, screen, operation, and time window;
2. assemble permission-scoped and privacy-safe evidence from the browser, API, background processing, SignalR, display player, and relevant integrations;
3. distinguish the observed failure from its immediate cause, trigger, software root cause, and systemic prevention gap;
4. explain the customer impact and current data safety in plain language;
5. recommend the smallest safe recovery action;
6. require explicit approval before any corrective mutation;
7. retain an owner-facing diagnostic summary even when a workaround restores service;
8. state uncertainty honestly when the available evidence cannot prove a root cause.

The agent must not treat a successful restart, retry, or re-pairing operation as proof that the underlying defect has been understood or eliminated.

## Example customer experience

A customer reports:

> I changed the dinner menu, but the lobby screen still shows lunch.

A useful customer response would be:

> I found the update. It was saved at 6:04 PM and published to three screens. The lobby screen has not acknowledged the new content version and has been offline since 5:51 PM. Your menu is safe. I recommend restarting that player or generating a new pairing code.

This response restores customer confidence and offers recovery, but it describes the failure state rather than necessarily proving the root cause.

## Owner and engineering result

The same investigation should produce a private, evidence-backed report such as:

- **Customer impact:** The lobby screen displayed content version 183 for 17 minutes after version 184 was published.
- **Immediate cause:** The player stopped acknowledging content-delivery messages at 5:51 PM.
- **Trigger:** The device changed Wi-Fi access points and its SignalR transport disconnected.
- **Software root cause:** Authentication renewal and transport reconnection succeeded, but the player did not restore its screen-channel subscription.
- **Why the workaround worked:** Restarting created a new authenticated connection and subscribed the player to the correct channel.
- **Systemic prevention gap:** No watchdog detected that heartbeats continued while delivery acknowledgements had stopped.
- **Recommended correction:** Make screen-channel subscription idempotent, restore it after every successful reconnect, and add a regression test and runtime watchdog for connected-without-subscription state.
- **Confidence:** High, with links to the supporting correlated events.

Owner-facing reports should also include contradictory evidence, affected versions, duration, customer scope, recurrence across other sessions, related deployments, and any proposed engineering or operational follow-up.

## Causal depth model

The agent should label the deepest conclusion supported by evidence:

1. **Observed failure** — what did not work;
2. **Immediate cause** — the state directly preventing success;
3. **Trigger** — the event that initiated that state;
4. **Software or operational root cause** — the defect, configuration, dependency, or action that allowed the trigger to become a failure;
5. **Systemic cause** — the missing guardrail, test, alert, recovery path, or design constraint that allowed the issue to reach a customer.

If evidence supports only the observed failure or immediate cause, the agent must say so. It must not convert correlation, likelihood, or a successful workaround into a claimed root cause.

## Diagnostic foundation

The product must become diagnosable before an agent can reason reliably about it. General console logging alone is insufficient. Vennusign needs correlated, structured, privacy-safe telemetry and explicit lifecycle events.

### Correlation

Compatible identifiers should connect relevant activity across browser, HTTP, hosted services, SignalR, display players, and integrations. Candidate dimensions include:

- trace and span identifiers;
- application session and temporary support-session identifiers;
- organization and venue identifiers;
- user and effective-role identifiers, represented safely;
- screen, player, content, publication, delivery, and acknowledgement identifiers;
- background-job and integration-operation identifiers;
- application, player, API, schema, and deployment versions.

Identifiers must not broaden authorization. Diagnostic retrieval must always reapply organization, venue, role, permission, support-access, and retention boundaries.

### Structured client evidence

The browser may contribute:

- unhandled JavaScript errors and rejected promises;
- React error-boundary events;
- failed or slow API operations;
- navigation and relevant domain actions;
- connectivity and SignalR state transitions;
- the active route and sanitized semantic UI state;
- browser, operating-system, device, application, and deployment versions.

Passwords, access tokens, payment data, secret configuration, arbitrary DOM contents, and unrestricted customer-authored content must not be collected.

### Server and player evidence

Application Insights or an equivalent observability platform may store correlated traces, exceptions, dependencies, metrics, and structured events. Stable event names and typed properties are preferable to free-form log sentences.

Important workflows should emit successful state transitions as well as failures. For content publication, an expected trail might be:

```text
Published
  -> DeliveryQueued
  -> DeliverySent
  -> PlayerReceived
  -> ContentApplied
  -> PlayerAcknowledged
```

This allows diagnostics to locate the missing transition instead of merely finding the final timeout. Similar lifecycle models may be required for pairing, authentication renewal, SignalR reconnection, scheduled activation, POS import and synchronization, billing or capability changes, and recovery operations.

### Support evidence service

The agent should not receive unrestricted access to production databases, raw telemetry stores, or administrative credentials. A Vennusign-owned support evidence service should return a sanitized, time-bounded, permission-checked diagnostic bundle for one authorized support session.

The bundle may include:

- current session and application context;
- relevant recent failures and state transitions;
- screen health and content-delivery state;
- capability and permission decisions;
- configuration and deployment versions;
- likely causal paths with supporting and contradictory evidence;
- the diagnostic tools and corrective actions permitted for the current user.

## Agent tools and action safety

The agent needs narrow tools rather than only a prompt over raw logs. Candidate capabilities include:

- retrieve current session context;
- retrieve recent correlated errors;
- inspect failed requests and dependencies;
- inspect screen and player health;
- trace one content publication from save through acknowledgement;
- explain a capability or permission decision;
- test an integration connection;
- retry a bounded delivery operation;
- generate a diagnostic report;
- create or enrich a support case.

Every tool must enforce authorization on the server. The model is never the security boundary. Read-only diagnosis should be the default. Mutations must be bounded, audited, idempotent where practical, and explicitly confirmed by an authorized user. Destructive, billing, identity, permission, or broad multi-venue actions require stronger controls or exclusion from the customer-facing agent.

## Session visibility and privacy

Support visibility should progress in layers:

1. **Sanitized semantic state:** route, selected entity identifiers, current operation, validation state, and relevant component state without raw DOM capture;
2. **User-authorized screenshot:** an explicit "Share current screen" action with sensitive-region masking and a temporary retention policy;
3. **Session replay:** considered only after explicit consent, redaction, access, retention, deletion, audit, and jurisdictional requirements are designed and approved.

Continuous screen recording or unrestricted session replay should not be the initial design. Customers must be told what is being shared, why it is needed, who can access it, and how long it is retained.

## Customer and owner presentation

The customer-facing result should prioritize:

- a plain-language explanation;
- current impact and data safety;
- the smallest safe recovery step;
- whether the problem is resolved;
- whether Vennusign support or engineering has been notified.

The owner and engineering result should prioritize:

- the correlated timeline;
- causal-depth classification and confidence;
- supporting and contradictory evidence;
- affected customer scope and duration;
- application, player, browser, device, and deployment versions;
- recurrence and clustering across other sessions;
- recommended product correction and regression coverage;
- links to authorized traces and any resulting issue or incident.

A customer incident may be resolved after a workaround while its engineering investigation remains open.

## MCP position

A Vennusign MCP server may eventually expose mature diagnostic capabilities to approved agents and internal tools. MCP should be an adapter over stable Vennusign support contracts, not the first diagnostic foundation.

Before an MCP surface is introduced, the underlying support APIs and tools should have typed contracts, authorization, redaction, audit records, bounded time windows, rate limits, read-only defaults, mutation confirmations, and tested tenant isolation.

## Possible delivery sequence

1. Establish correlation, structured telemetry, lifecycle events, version reporting, privacy rules, and retention controls.
2. Build a read-only internal support evidence service and Vennusign staff diagnostic copilot.
3. Validate diagnostic accuracy, causal-confidence reporting, redaction, and tenant isolation against real support cases.
4. Expose a customer-facing read-only assistant with semantic session state and optional authorized screenshots.
5. Add a small set of customer-approved, auditable corrective actions.
6. Consider an MCP adapter after the diagnostic contracts and operating controls are mature.

An internal support copilot is the recommended first agent experience because it allows Vennusign to validate evidence quality, privacy boundaries, root-cause accuracy, and recovery safety before presenting autonomous conclusions or actions directly to customers.

## Product principle

The agent may inspect only the evidence allowed by the customer's authorization and explicit sharing choices, explain only conclusions supported by that evidence, and propose the smallest safe recovery action. Successful recovery must not conceal an unresolved root cause.

## Decisions required before planning

Future product planning would need explicit decisions about:

- whether the first user is Vennusign support staff, the customer, or both;
- telemetry provider and operational ownership;
- event taxonomy and state-machine coverage;
- data classification, redaction, regional storage, retention, deletion, and legal review;
- support-access authorization and customer consent;
- screenshot and replay boundaries;
- diagnostic confidence representation;
- which recovery actions, if any, are safe for customer execution;
- incident creation, clustering, escalation, and owner notification;
- availability and behavior during API, telemetry, identity, or agent-provider outages;
- cost, latency, rate limits, and model-provider data-handling requirements;
- the gate for introducing a Vennusign MCP server.

No implementation should begin from this concept alone. Approved scope, issue and work-package governance, architecture and privacy review, acceptance criteria, and validation requirements remain necessary.
