# Café, Bakery & Dessert Optional Capabilities

## Purpose

This document defines optional Café, Bakery & Dessert capabilities beyond the required manual core established in RWP-00.31. Optional capabilities may improve coordination, automation, presentation, insight, governance, or managed operation, but they must never be required to maintain accurate guest-facing information.

This is documentation and product planning only. It does not authorize UI, API, schema, billing, entitlement, ordering, payment, production, inventory, fulfillment, analytics, AI, hardware, or integration implementation.

## Packaging principles

1. Required manual operation remains core at every subscription level.
2. Native advanced Vennusign workflow is a tier candidate when it creates broader coordination or governance value.
3. External systems, consumption-backed services, and managed services are independent add-on candidates when they create separable cost or value.
4. Counts, volume, frequency, retention, export, storage, transactions, support, and AI consumption are limits, not capabilities.
5. Industry and subtype remain non-commercial configuration.
6. Optional capability loss, disconnect, failure, or downgrade must preserve customer-authored content, essential state, manual fallback, source/freshness context, delivery confidence, and recovery.

## Native workflow tier candidates

### Advanced scheduling and rotations

Candidates include:

- recurring service periods, product rotations, batch messages, specials, and seasonal collections;
- reusable schedules and daypart templates;
- planned availability and expected-return transitions;
- schedule conflict detection and review;
- holiday, event, and exception overlays; and
- safe preview, cancellation, supersession, and rollback.

Manual immediate changes and current-period operation remain core.

### Campaigns and promotion orchestration

Candidates include:

- reusable campaigns and promotional collections;
- multi-screen and multi-venue campaign coordination;
- start, stop, pause, replace, and expiration workflow;
- audience, venue, service-period, and screen-purpose targeting;
- approval, version, and campaign history; and
- campaign performance views based on available Vennusign evidence.

A campaign must not override urgent operational truth such as sold-out, closure, pickup, or changed-hours information.

### Advanced presentation

Candidates include:

- richer layout systems and brand libraries;
- content variants by screen purpose, service context, language, or daypart;
- advanced menu, case, flavor, promotion, and pickup presentation;
- responsive content adaptation and governance; and
- managed reusable components and templates.

Basic clear layouts, themes, accessibility, and effective publication remain core.

### Multi-screen and multi-venue coordination

Candidates include:

- synchronized screen groups;
- venue and organization templates with local inheritance and overrides;
- safe bulk edits and publication;
- mixed-state review and partial-success recovery;
- cross-venue calendars and operating coordination;
- organization-wide governance with local control; and
- portfolio-level delivery and freshness oversight.

Single-venue operation, explicit targeting, and per-target confirmation remain core.

### Approvals, governance, and history

Candidates include:

- configurable approvals and separation of duties;
- scheduled approvals and campaign review;
- advanced audit trails and extended version history;
- brand, legal, policy, or corporate review workflow;
- controlled template inheritance; and
- enterprise administration and governance.

Basic permission checks, current correction, undo, and restoration remain core.

### Localization workflow

Candidates include:

- translation workflow and review;
- terminology libraries and locale variants;
- content completeness and stale-translation reporting;
- multi-language campaigns and coordinated publication; and
- reusable language templates.

Accessible customer-authored content and basic language variants remain core where supported. External premium translation is an add-on candidate.

### Advanced analytics and optimization

Candidates include:

- product, category, promotion, service-period, screen, venue, and subtype analysis;
- content freshness and operational response analysis;
- campaign comparison and organization benchmarking;
- advanced exports and scheduled reporting;
- optimization recommendations; and
- reviewed AI-assisted analysis.

Core operational delivery and freshness evidence remains available without advanced analytics. Analytics may not claim sales, demand, inventory, conversion, or guest behavior without an authoritative source.

### Loyalty and engagement workflow

Candidates include:

- Vennusign-native loyalty content orchestration;
- member or audience content workflow where privacy and authorization allow;
- reusable offer and campaign management;
- coordinated messaging and screen presentation; and
- outcome analysis based on available evidence.

External loyalty systems and messaging delivery remain add-on candidates.

## Independent integration add-on candidates

### POS and catalog synchronization

Potential value includes product, category, price, option, availability, and sales-context synchronization. Requirements include source identity, freshness, mapping, conflict handling, partial coverage, manual fallback, disconnect, cancellation, and data-retention policy.

### Inventory and production systems

Potential value includes stock, batch, production, sell-out, expected-return, and production-status signals. The integration must not invent public freshness, readiness, safety, quantity, or return claims. Manual authority and fallback remain required.

### Ordering, payment, and fulfillment

Potential value includes public ordering links, preorder windows, pickup information, fulfillment status, and transaction-aware messaging. Private guest, order, payment, and fulfillment data requires explicit privacy, authorization, minimization, retention, and display policy. Public screens must not expose private data by default.

### Loyalty, CRM, and messaging

Potential value includes campaign audiences, member content, messaging, offers, and attribution. Consent, privacy, identity, audience correctness, opt-out, source authority, and safe failure are mandatory planning concerns.

### Supplier, calendar, weather, event, and traffic sources

Potential value includes product planning context, exceptions, venue events, seasonal changes, weather-sensitive messages, and travel or demand context. External data must show source, freshness, coverage, confidence, and failure state and must not silently control essential public truth.

### Translation and language services

Potential value includes machine or managed translation. Customer review, terminology preservation, privacy, language completeness, stale-translation visibility, safe rollback, and manual fallback are required.

### Identity and enterprise systems

Potential value includes identity-provider connection, provisioning, role administration, and enterprise governance. Authentication and commercial access remain separate from operational permissions and capability classification.

### AI services

Potential value includes draft content, classification, translation assistance, recommendation, anomaly detection, summarization, and optimization. AI output requires review, source disclosure, privacy controls, usage limits, safe rejection, and manual operation. AI may not invent product facts, scarcity, freshness, health, safety, readiness, or performance claims.

## Managed-service add-on candidates

Candidates include:

- managed player or screen hardware;
- installation and event deployment;
- connectivity and managed network service;
- monitoring and operational response;
- premium support and service commitments;
- managed content or campaign services; and
- managed localization, analytics, or optimization services.

Managed service may not replace customer ownership, export, correction, restore, or safe exit expectations.

## Candidate limits

Limits may apply independently to:

- venues, screens, users, roles, products, categories, menus, layouts, templates, languages, schedules, campaigns, approvals, and versions;
- history and retention periods;
- file, image, storage, export, and report volume;
- publication frequency and transaction volume;
- integrations, connected accounts, sync frequency, and API consumption;
- messaging, translation, monitoring, support, and managed-service volume; and
- AI requests, tokens, models, or advanced-analysis consumption.

A reached limit must be distinguishable from permission denial, unavailable product state, disconnected integration, unsupported configuration, and rollout status. Essential public correction and recovery must not be trapped behind an exhausted optional limit.

## Required lifecycle behavior

Every optional capability must define:

- eligibility and commercial access;
- permission requirements;
- configuration and first-use state;
- source, freshness, coverage, and conflict behavior;
- manual fallback and core preservation;
- loading, empty, partial, stale, disconnected, failed, and recovery states;
- cancellation, disconnect, upgrade, downgrade, and reactivation;
- data ownership, export, retention, deletion, and read-only behavior; and
- support responsibility and customer-visible status.

## Subtype presentation

Optional capabilities may be recommended differently by subtype but may not create subtype entitlements. Examples:

- Coffee Shops may emphasize POS, loyalty, queue, and multi-location coordination.
- Bakeries and Patisseries may emphasize production, batch, preorder, pickup, seasonal templates, and custom-order systems.
- Tea, Dessert, Frozen Dessert, and Juice concepts may emphasize rotating products, flavor presentation, seasonal campaigns, options, and loyalty.
- Bakery-Cafés may emphasize mixed service periods, multi-menu coordination, table/counter contexts, and Restaurant inheritance.

## Impeccable planning brief

Modes are **Operate** for configuration and daily work and **Persuade** only for optional-capability discovery.

- Optional capabilities must be introduced by operational outcome, not abstract feature names.
- Discovery must not interrupt an urgent core task or first-screen activation.
- Locked states must distinguish entitlement, permission, missing setup, disconnected source, reached limit, unsupported context, and staged rollout.
- Upgrade prompts must preserve the user's current work and explain what becomes possible.
- Every optional flow requires complete loading, empty, permission, integration, partial, failure, downgrade, and recovery states on phone and desktop.

## Validation

This catalog addresses scheduling, recurring promotions, advanced presentation, multi-screen, multi-venue, loyalty, preorder, POS, inventory, AI, analytics, managed hardware, and integration candidates. It separates tier, add-on, permission, state, limit, privacy, and rollout concerns and leaves the required manual core unchanged.