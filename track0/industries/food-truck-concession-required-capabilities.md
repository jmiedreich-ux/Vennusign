# Food Truck & Concession Required Capabilities

## Purpose

This document defines the smallest viable capability set required for ordinary Food Truck & Concession operation. It inherits the Restaurant baseline and applies the approved Food Truck & Concession industry, subtype, terminology, and operating-characteristics decisions.

Required capabilities are not subtype unlocks, premium integrations, usage allowances, or rollout flags. Permissions determine who may act; product state describes what is true; commercial entitlements determine optional access; limits constrain quantity; rollout flags remain internal.

## Required capability principles

1. A single-unit operator must be able to complete ordinary service without purchasing an integration, premium analytics package, managed hardware service, or advanced scheduling feature.
2. Required capabilities exist because they are necessary for credible daily operation and remain available across all Food Truck & Concession subtypes.
3. Manual operation must remain available when an external system is absent, disconnected, delayed, or stale.
4. Subtype changes recommendations and language only; it does not unlock required behavior.
5. Quantity limits may constrain included screens, units, service points, schedules, or retained versions, but they do not convert required behavior into an optional capability.

## Menu and offer management

Required capabilities include:

- create, edit, organize, preview, and publish menu categories, items, combos, prices, descriptions, images, options, dietary labels, and service instructions;
- support compact and broader menu presentations without duplicating the underlying menu model;
- preserve customer-authored content when subtype, location, event, host, or operating state changes;
- identify validation errors before publication;
- retain the last successfully published version when a draft is incomplete or invalid.

Catalog synchronization, inventory automation, dynamic pricing, recipe planning, and production-system control are optional candidates.

## Rapid availability and sell-out control

Required capabilities include:

- Quick Update for item, combo, category, service point, menu, and whole-operation availability where those scopes exist;
- distinct available, unavailable, sold out, limited, paused, reopened, and closed states;
- clear effective scope, changed-by identity, changed time, and publish result;
- safe reversal and recovery;
- preservation of item state when a service period or whole operation pauses or closes.

Manual rapid updates remain required even when POS, inventory, kitchen, or order integrations are connected.

## Location, event, and service context

Required capabilities include:

- represent the current authoritative stop, pitch, host location, event, market, stand, gate, section, service point, or temporary location as applicable;
- represent ordinary hours, bounded service periods, event windows, last-order information, delays, relocation, cancellation, temporary closure, and reopening;
- distinguish planned context from current operating state;
- require review of affected screens and guest guidance when location or event context changes;
- prevent location changes from silently changing ownership, permissions, subtype, entitlement, or screen assignment;
- preserve unknown time, location, and reopening information as unknown.

Live vehicle tracking, route optimization, host-calendar synchronization, and arrival prediction are not required core behavior.

## Screen pairing, targeting, preview, and publish

Required capabilities include:

- pair and identify screens;
- explicitly select the operation, menu or content set, service point, location or event context, and intended screens;
- preview the proposed guest-facing result;
- publish immediately to selected targets;
- prevent accidental cross-location or cross-event publication;
- preserve screen identity and pairing across setup, teardown, relocation, and temporary offline periods.

## Publish confirmation and delivery confidence

Required capabilities include:

- confirm whether publication was accepted and identify every intended target;
- show online, offline, outdated, unknown, failed, and successfully delivered states without relying on color alone;
- show the last successfully delivered version and time when known;
- identify targets that did not receive the proposed version;
- provide retry and recovery actions that do not duplicate content or silently retarget screens;
- preserve the prior stable guest-facing version during temporary connectivity loss where supported by the player.

Managed connectivity, advanced monitoring, remote support, and hardware service remain optional candidates.

## Recovery and restoration

Required capabilities include:

- retain enough publication history to identify and restore a prior successful version within the included retention allowance;
- preview the restore target and affected screens;
- distinguish restoring Vennusign content from restoring an external source state;
- preserve newer approved work when reconnecting after an outage or edit conflict;
- expose actionable conflict, stale-source, partial-delivery, and failed-publication recovery states.

Extended history, compliance archives, and enterprise approval records may be tier or add-on candidates.

## Queue, pickup, and service-window guidance

Required manual guest communication includes:

- order, express, pickup, collection, accessible, and locally named lanes;
- open and closed service windows or counters;
- temporary rerouting, queue instructions, last orders, and collection guidance;
- paused, limited-capacity, relocated, delayed, or closed service notices.

The required capability is accurate manual communication. Live queue measurement, wait-time prediction, order-ready feeds, ordering, and payment integrations are optional.

## Operating-state and disruption communication

Required capabilities include operator-confirmed communication for:

- planned, setup, ready, open, limited, paused, relocating, closed, canceled, and serving-again states where applicable;
- delayed opening, early closure, weather-affected service, changed service side, reduced menu, changed location, and host-directed notices;
- high-scope confirmation before changing a whole operation, event, menu, or multiple screens;
- clear restoration after an outage, pause, relocation, or temporary closure.

Vennusign records and publishes operator-confirmed state. It does not make legal, safety, permit, weather, or venue-operation decisions.

## Permissions and authority

The required permission model must support at least:

- view operation, content, screen, publication, and delivery state;
- edit menu and content;
- change availability and operating state;
- manage location, event, service-period, queue, and pickup guidance;
- pair and manage screens;
- publish to authorized targets;
- restore a prior version;
- administer local users and authority where permitted.

Host, sponsor, operator, caterer, property, and organization authority must be explicit by object and scope. Permission never implies entitlement, and entitlement never grants permission.

## Required states and feedback

Every required workflow must account for:

- first use and no-content state;
- no paired screen or no selected target;
- loading and saving;
- validation errors;
- permission denied;
- offline, outdated, unknown, and partially delivered targets;
- stale external source and source conflicts;
- publish success, partial success, and failure;
- destructive or high-scope confirmation;
- retry, restore, and safe exit;
- narrow mobile layouts, outdoor readability, 200% zoom, keyboard access, assistive technology, long names, and non-color-only status.

The project-local Impeccable `shape` and `harden` guidance informs these planning requirements. No UI implementation is authorized.

## Classification summary

| Concern | Primary classification | Treatment |
| --- | --- | --- |
| Menu/content editing and preview | Core capability | Required |
| Manual availability and sell-out updates | Core capability | Required |
| Current location/event/service communication | Core capability plus product state | Required |
| Screen pairing and explicit targeting | Core capability | Required |
| Immediate publish and per-target confirmation | Core capability | Required |
| Offline/outdated awareness and basic recovery | Core capability | Required |
| Manual queue, pickup, and service-window guidance | Core capability plus product state | Required |
| Role and object authority | Permission | Required permission model |
| Advanced route/event scheduling | Tier candidate | Optional |
| Multi-unit orchestration and approvals | Tier candidate | Optional |
| POS, order, inventory, weather, host, queue, and event feeds | Add-on candidate | Optional |
| Screen, unit, event, integration, and history counts | Limit | Separate from capability |
| Internal staged delivery | Rollout flag | Never customer packaging |

## Validation

The required set covers menu and availability management, location and event context, explicit targeting, publish confirmation, offline and outdated awareness, recovery, queue and service guidance, rapid updates, permissions, and all required experience states. It keeps ordinary manual operation core and does not authorize product, UI, API, schema, billing, integration, analytics, routing, ordering, payment, inventory, event-management, or hardware implementation.
