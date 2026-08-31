- <a href="#vennusign-connector-platform--technical-architecture" id="toc-vennusign-connector-platform--technical-architecture">VennuSign Connector Platform — Technical Architecture</a>
  - <a href="#1-executive-summary" id="toc-1-executive-summary">1. Executive summary</a>
  - <a href="#2-confirmed-requirements" id="toc-2-confirmed-requirements">2. Confirmed requirements</a>
  - <a href="#3-scope" id="toc-3-scope">3. Scope</a>
    - <a href="#31-in-scope" id="toc-31-in-scope">3.1 In scope</a>
    - <a href="#32-out-of-scope" id="toc-32-out-of-scope">3.2 Out of scope</a>
  - <a href="#4-terminology-and-responsibilities" id="toc-4-terminology-and-responsibilities">4. Terminology and responsibilities</a>
  - <a href="#5-non-functional-requirements" id="toc-5-non-functional-requirements">5. Non-functional requirements</a>
    - <a href="#51-service-objectives" id="toc-51-service-objectives">5.1 Service objectives</a>
    - <a href="#52-the-seconds-level-pull-constraint" id="toc-52-the-seconds-level-pull-constraint">5.2 The seconds-level pull constraint</a>
  - <a href="#6-architecture-principles" id="toc-6-architecture-principles">6. Architecture principles</a>
  - <a href="#7-c4-system-context" id="toc-7-c4-system-context">7. C4 system context</a>
  - <a href="#8-c4-container-architecture" id="toc-8-c4-container-architecture">8. C4 container architecture</a>
  - <a href="#9-recommended-azure-technology-stack" id="toc-9-recommended-azure-technology-stack">9. Recommended Azure technology stack</a>
  - <a href="#10-logical-components" id="toc-10-logical-components">10. Logical components</a>
    - <a href="#101-connector-registry-and-control-plane" id="toc-101-connector-registry-and-control-plane">10.1 Connector registry and control plane</a>
    - <a href="#102-pull-scheduler-and-workers" id="toc-102-pull-scheduler-and-workers">10.2 Pull scheduler and workers</a>
    - <a href="#103-push-rest-ingestion" id="toc-103-push-rest-ingestion">10.3 Push REST ingestion</a>
    - <a href="#104-sftp-landing-zone" id="toc-104-sftp-landing-zone">10.4 SFTP landing zone</a>
    - <a href="#105-parser-registry" id="toc-105-parser-registry">10.5 Parser registry</a>
    - <a href="#106-mapping-engine" id="toc-106-mapping-engine">10.6 Mapping engine</a>
    - <a href="#107-canonical-schema-registry-and-governance" id="toc-107-canonical-schema-registry-and-governance">10.7 Canonical schema registry and governance</a>
    - <a href="#108-validation-diff-and-apply" id="toc-108-validation-diff-and-apply">10.8 Validation, diff, and apply</a>
  - <a href="#11-data-semantics" id="toc-11-data-semantics">11. Data semantics</a>
    - <a href="#111-snapshots" id="toc-111-snapshots">11.1 Snapshots</a>
    - <a href="#112-deltas" id="toc-112-deltas">11.2 Deltas</a>
    - <a href="#113-ownership-and-manual-edits" id="toc-113-ownership-and-manual-edits">11.3 Ownership and manual edits</a>
  - <a href="#12-key-data-entities" id="toc-12-key-data-entities">12. Key data entities</a>
  - <a href="#13-state-machine" id="toc-13-state-machine">13. State machine</a>
  - <a href="#14-sequence-diagrams" id="toc-14-sequence-diagrams">14. Sequence diagrams</a>
    - <a href="#141-rest-api-pull" id="toc-141-rest-api-pull">14.1 REST API pull</a>
    - <a href="#142-inbound-push-api" id="toc-142-inbound-push-api">14.2 Inbound push API</a>
    - <a href="#143-sftp-push" id="toc-143-sftp-push">14.3 SFTP push</a>
    - <a href="#144-failure-and-last-valid-state-behavior" id="toc-144-failure-and-last-valid-state-behavior">14.4 Failure and last-valid-state behavior</a>
  - <a href="#15-consistency-concurrency-and-idempotency" id="toc-15-consistency-concurrency-and-idempotency">15. Consistency, concurrency, and idempotency</a>
  - <a href="#16-security-architecture" id="toc-16-security-architecture">16. Security architecture</a>
    - <a href="#161-identity-and-access" id="toc-161-identity-and-access">16.1 Identity and access</a>
    - <a href="#162-network-controls" id="toc-162-network-controls">16.2 Network controls</a>
    - <a href="#163-payload-security" id="toc-163-payload-security">16.3 Payload security</a>
    - <a href="#164-card-data-prohibition" id="toc-164-card-data-prohibition">16.4 Card-data prohibition</a>
    - <a href="#165-threats-and-mitigations" id="toc-165-threats-and-mitigations">16.5 Threats and mitigations</a>
  - <a href="#17-reliability-and-failure-handling" id="toc-17-reliability-and-failure-handling">17. Reliability and failure handling</a>
    - <a href="#171-retry-policy" id="toc-171-retry-policy">17.1 Retry policy</a>
    - <a href="#172-circuit-breakers-and-bulkheads" id="toc-172-circuit-breakers-and-bulkheads">17.2 Circuit breakers and bulkheads</a>
    - <a href="#173-last-valid-data" id="toc-173-last-valid-data">17.3 Last valid data</a>
    - <a href="#174-dead-letter-and-quarantine" id="toc-174-dead-letter-and-quarantine">17.4 Dead-letter and quarantine</a>
  - <a href="#18-availability-backup-and-disaster-recovery" id="toc-18-availability-backup-and-disaster-recovery">18. Availability, backup, and disaster recovery</a>
    - <a href="#181-single-region-production-baseline" id="toc-181-single-region-production-baseline">18.1 Single-region production baseline</a>
    - <a href="#182-recovery-objectives" id="toc-182-recovery-objectives">18.2 Recovery objectives</a>
    - <a href="#183-regional-disaster-evolution" id="toc-183-regional-disaster-evolution">18.3 Regional disaster evolution</a>
  - <a href="#19-observability-and-operations" id="toc-19-observability-and-operations">19. Observability and operations</a>
    - <a href="#191-metrics" id="toc-191-metrics">19.1 Metrics</a>
    - <a href="#192-alerts" id="toc-192-alerts">19.2 Alerts</a>
    - <a href="#193-operational-controls" id="toc-193-operational-controls">19.3 Operational controls</a>
  - <a href="#20-deployment-topology-and-scaling" id="toc-20-deployment-topology-and-scaling">20. Deployment topology and scaling</a>
  - <a href="#21-api-and-event-contracts" id="toc-21-api-and-event-contracts">21. API and event contracts</a>
    - <a href="#211-push-api-envelope-example" id="toc-211-push-api-envelope-example">21.1 Push API envelope example</a>
    - <a href="#212-internal-ingestion-event" id="toc-212-internal-ingestion-event">21.2 Internal ingestion event</a>
  - <a href="#22-mapping-lifecycle-and-testing" id="toc-22-mapping-lifecycle-and-testing">22. Mapping lifecycle and testing</a>
  - <a href="#23-architectural-trade-offs" id="toc-23-architectural-trade-offs">23. Architectural trade-offs</a>
    - <a href="#231-modular-platform-vs-fine-grained-microservices" id="toc-231-modular-platform-vs-fine-grained-microservices">23.1 Modular platform vs. fine-grained microservices</a>
    - <a href="#232-asynchronous-pipeline-vs-synchronous-processing" id="toc-232-asynchronous-pipeline-vs-synchronous-processing">23.2 Asynchronous pipeline vs. synchronous processing</a>
    - <a href="#233-azure-sql-vs-nosql-for-metadata" id="toc-233-azure-sql-vs-nosql-for-metadata">23.3 Azure SQL vs. NoSQL for metadata</a>
    - <a href="#234-service-bus-vs-event-grid-alone" id="toc-234-service-bus-vs-event-grid-alone">23.4 Service Bus vs. Event Grid alone</a>
    - <a href="#235-container-apps-vs-functions-vs-aks" id="toc-235-container-apps-vs-functions-vs-aks">23.5 Container Apps vs. Functions vs. AKS</a>
    - <a href="#236-declarative-mapper-vs-arbitrary-scripts" id="toc-236-declarative-mapper-vs-arbitrary-scripts">23.6 Declarative mapper vs. arbitrary scripts</a>
    - <a href="#237-shared-vs-dedicated-tenant-infrastructure" id="toc-237-shared-vs-dedicated-tenant-infrastructure">23.7 Shared vs. dedicated tenant infrastructure</a>
  - <a href="#24-architecture-decision-records" id="toc-24-architecture-decision-records">24. Architecture Decision Records</a>
    - <a href="#adr-001-one-connector-platform-for-pull-and-push" id="toc-adr-001-one-connector-platform-for-pull-and-push">ADR-001: One Connector Platform for pull and push</a>
    - <a href="#adr-002-canonical-data-types-are-reused-by-default" id="toc-adr-002-canonical-data-types-are-reused-by-default">ADR-002: Canonical data types are reused by default</a>
    - <a href="#adr-003-event-driven-asynchronous-processing" id="toc-adr-003-event-driven-asynchronous-processing">ADR-003: Event-driven asynchronous processing</a>
    - <a href="#adr-004-azure-managed-services-stack" id="toc-adr-004-azure-managed-services-stack">ADR-004: Azure managed-services stack</a>
    - <a href="#adr-005-at-least-once-delivery-with-effectively-once-effects" id="toc-adr-005-at-least-once-delivery-with-effectively-once-effects">ADR-005: At-least-once delivery with effectively-once effects</a>
    - <a href="#adr-006-integration-controlled-data-is-read-only" id="toc-adr-006-integration-controlled-data-is-read-only">ADR-006: Integration-controlled data is read-only</a>
    - <a href="#adr-007-preserve-last-valid-data-on-failure" id="toc-adr-007-preserve-last-valid-data-on-failure">ADR-007: Preserve last valid data on failure</a>
    - <a href="#adr-008-sftp-is-managed-blob-ingress-ftp-is-unsupported" id="toc-adr-008-sftp-is-managed-blob-ingress-ftp-is-unsupported">ADR-008: SFTP is managed Blob ingress; FTP is unsupported</a>
    - <a href="#adr-009-mapping-uses-a-safe-dsl" id="toc-adr-009-mapping-uses-a-safe-dsl">ADR-009: Mapping uses a safe DSL</a>
    - <a href="#adr-010-no-payment-card-data" id="toc-adr-010-no-payment-card-data">ADR-010: No payment-card data</a>
  - <a href="#25-phased-implementation-plan" id="toc-25-phased-implementation-plan">25. Phased implementation plan</a>
    - <a href="#phase-0--contract-and-threat-model-foundation" id="toc-phase-0--contract-and-threat-model-foundation">Phase 0 — Contract and threat-model foundation</a>
    - <a href="#phase-1--vertical-platform-slice" id="toc-phase-1--vertical-platform-slice">Phase 1 — Vertical platform slice</a>
    - <a href="#phase-2--pull-platform" id="toc-phase-2--pull-platform">Phase 2 — Pull platform</a>
    - <a href="#phase-3--file-push-platform" id="toc-phase-3--file-push-platform">Phase 3 — File push platform</a>
    - <a href="#phase-4--operations-and-controlled-rollout" id="toc-phase-4--operations-and-controlled-rollout">Phase 4 — Operations and controlled rollout</a>
  - <a href="#26-acceptance-criteria" id="toc-26-acceptance-criteria">26. Acceptance criteria</a>
  - <a href="#27-risks-and-follow-up-decisions" id="toc-27-risks-and-follow-up-decisions">27. Risks and follow-up decisions</a>
  - <a href="#28-final-recommendation" id="toc-28-final-recommendation">28. Final recommendation</a>
  - <a href="#29-primary-implementation-references" id="toc-29-primary-implementation-references">29. Primary implementation references</a>

# VennuSign Connector Platform — Technical Architecture

**Status:** Proposed  
**Date:** 2026-08-13  
**Audience:** Product engineering, platform engineering, security, operations, and integration developers  
**Scope:** Reusable platform architecture; no first connector is selected by this design

## 1. Executive summary

VennuSign should build one **Connector Platform** that supports two ingestion directions:

- **Pull connectors** retrieve data from approved third-party REST APIs.
- **Push connectors** receive data through a VennuSign inbound REST API or customer-isolated SFTP folders.

After transport, both paths converge on the same versioned ingestion pipeline: retain the source payload, identify the tenant and connector, parse the source format, map it to an existing VennuSign canonical data type, validate it, apply it atomically, and notify the downstream VennuSign content/publishing pipeline.

The mapper is the central capability. A new source system normally requires a new connector definition and mapping profile—not a new VennuSign data type. A new canonical data type is justified only when existing types cannot represent the business meaning without loss or distortion.

The recommended implementation is a **modular, event-driven .NET platform on Azure**, initially deployed as a small number of independently scalable Azure Container Apps rather than many fine-grained microservices. Azure API Management protects inbound APIs; Azure Blob Storage provides raw payload retention and native SFTP ingress; Azure Service Bus decouples pipeline stages; Azure SQL Database stores control-plane state, mappings, lineage, and synchronization state; Azure Key Vault holds external credentials; and Azure Monitor/Application Insights provides end-to-end observability.

This design targets 1,000 customer locations, 99.9% monthly platform availability, recovery within two hours, durable at-least-once processing with idempotent effects, and seconds-level processing for urgent updates after VennuSign receives a push or completes a pull response.

## 2. Confirmed requirements

| Area                   | Decision                                                                                   |
|------------------------|--------------------------------------------------------------------------------------------|
| Capacity               | First architecture supports up to 1,000 customer locations                                 |
| Urgent freshness       | Within seconds for both push and API pull                                                  |
| Pull transport         | Third-party REST APIs only                                                                 |
| Push transports        | VennuSign inbound REST API and SFTP file delivery                                          |
| Payload styles         | Complete snapshots and incremental/delta updates                                           |
| Formats                | JSON, XML, CSV, and connector-defined flat files                                           |
| Mapping                | Both pull and push use the shared mapping platform                                         |
| Canonical types        | Reuse existing VennuSign data types unless a genuinely new semantic model is required      |
| Ownership              | No manual editing of integration-controlled data in VennuSign                              |
| Failure behavior       | Retain the last valid data and alert; never replace it with invalid data                   |
| Permitted data         | Operational display data; customer/order data only when necessary for the display use case |
| Prohibited data        | Payment-card data must never be accepted, stored, logged, or processed                     |
| Availability           | 99.9% monthly                                                                              |
| Recovery               | RTO 2 hours                                                                                |
| Initial implementation | Platform-first; no specific cinema or restaurant connector chosen                          |

## 3. Scope

### 3.1 In scope

- Connector onboarding, configuration, enablement, suspension, and health.
- Scheduled and on-demand REST API pull execution.
- Inbound REST API push ingestion.
- Inbound SFTP ingestion backed by managed object storage.
- JSON, XML, CSV, and bounded flat-file parsing.
- Mapping source records into versioned VennuSign canonical types.
- Snapshot and delta semantics.
- Validation, preview/dry run, atomic application, lineage, replay, quarantine, and alerting.
- Tenant/location isolation and connector credential management.
- Integration events that trigger the existing VennuSign content/render/publish workflow.
- Operator controls suitable for later exposure through the VennuSign Operations Platform.

### 3.2 Out of scope

- Pulling files from customer FTP/SFTP servers.
- Unencrypted FTP.
- Payment processing or storage of payment-card data.
- A customer-facing general-purpose ETL/data warehouse product.
- Building a specific Toast, cinema, POS, or ticketing connector in this document.
- Replacing VennuSign's canonical domain services or rendering engine.
- Arbitrary user-authored code running inside the production mapper.

## 4. Terminology and responsibilities

| Term                | Meaning                                                                                                                             |
|---------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| Connector type      | Versioned implementation describing a source product/protocol, authentication, extraction, parsing, and supported canonical targets |
| Connector instance  | One tenant/location's configured use of a connector type                                                                            |
| Pull run            | One scheduled or manually triggered attempt to retrieve source data                                                                 |
| Ingestion           | One accepted API request, uploaded file, or completed pull response                                                                 |
| Mapping profile     | Versioned rules translating a source schema into a canonical VennuSign type                                                         |
| Canonical data type | Stable VennuSign business model such as menu data or cinema showtime data                                                           |
| Snapshot            | Source-authoritative complete set for a declared scope                                                                              |
| Delta               | A set of explicit upserts/deletes relative to known source identifiers                                                              |
| Last valid state    | Most recently validated and atomically applied canonical version for a scope                                                        |
| Scope               | The tenant, location, data type, and source-defined partition affected by an ingestion                                              |

## 5. Non-functional requirements

### 5.1 Service objectives

| NFR                      |                                                                                          Target | Measurement boundary                                                                               |
|--------------------------|------------------------------------------------------------------------------------------------:|----------------------------------------------------------------------------------------------------|
| Availability             |                                                                                   99.9% monthly | Connector control plane and ingestion acceptance endpoints                                         |
| Push API acknowledgement |                                                                       p95 \< 500 ms; p99 \< 1 s | Valid request accepted and durably queued, excluding payload upload time                           |
| Urgent push processing   |                                                                         p95 \< 5 s; p99 \< 15 s | Durable acceptance to canonical commit, for payloads within the standard size envelope             |
| Pull response processing |                                                                         p95 \< 5 s; p99 \< 15 s | Complete source HTTP response stored to canonical commit                                           |
| Pull dispatch jitter     |                                                                                      p95 \< 2 s | Due time to worker dispatch, when provider quota and platform health permit                        |
| Downstream notification  |                                                                                      p95 \< 2 s | Canonical commit to durable `CanonicalDataChanged` event                                           |
| Sustained throughput     |                                                            100 ingestion events/s platform-wide | Normal operations                                                                                  |
| Burst throughput         |                                                            500 ingestion events/s for 5 minutes | Buffered without data loss; backlog drains within 10 minutes                                       |
| Payload size             |                                                        10 MiB API default; 250 MiB SFTP default | Larger files require explicit connector approval and streaming tests                               |
| Data durability          |                                                                    No acknowledged payload loss | Accepted payload must be durably stored before success is returned                                 |
| Delivery semantics       |                                                               At least once, idempotent effects | Every stage may retry without duplicate domain effects                                             |
| Tenant isolation         |                                                                              100% scoped access | Every operation requires tenant and connector identity; cross-tenant access is a security incident |
| RTO                      |                                                                                         2 hours | Restore critical ingestion and application capability                                              |
| RPO                      | 15 minutes for control/canonical metadata; effectively zero for accepted raw payloads in-region | Subject to Azure service durability and configured replication                                     |
| Audit retention          |                                                                                13 months online | Configuration, mapping publication, execution, rejection, replay, and operator actions             |
| Raw payload retention    |                                                                                 30 days default | Configurable downward for privacy; longer only by explicit policy                                  |

### 5.2 The seconds-level pull constraint

The platform can process a completed pull within seconds, but **continuous seconds-level freshness cannot be universally guaranteed for pull connectors**. It depends on the provider's API rate limits, data-change visibility, response latency, availability, and number of customer credentials.

Each pull connector must therefore publish a **freshness contract**:

- minimum safe polling interval;
- provider rate-limit budget and backoff rules;
- whether conditional requests, cursors, or `updated_since` filters exist;
- expected provider-to-VennuSign detection time;
- VennuSign processing target after receipt;
- whether a manual/urgent refresh is permitted.

For sources that cannot safely support frequent polling, the UI must show the actual expected freshness rather than claiming seconds. Push is the preferred mechanism for truly event-like updates.

## 6. Architecture principles

1.  **Transport and semantics are separate.** REST pull, REST push, and SFTP are adapters into one pipeline.
2.  **Canonical models remain source-independent.** Source-specific fields do not leak into display contracts unless formally added through schema governance.
3.  **Validate before replace.** Invalid or incomplete input cannot displace last valid data.
4.  **Raw input is immutable.** Every accepted ingestion is traceable and replayable.
5.  **Effects are idempotent.** The broker and workers provide at-least-once delivery; business state behaves as if applied once.
6.  **Snapshots are scoped.** A snapshot replaces only its declared scope, never an entire tenant implicitly.
7.  **Integration-owned means read-only.** VennuSign UI and ordinary APIs reject direct edits to controlled fields/records.
8.  **No arbitrary production mapping code.** Use a constrained declarative mapping DSL and reviewed extension modules.
9.  **Fail closed on identity and schema; fail safe on display.** Reject questionable input while screens retain last valid content.
10. **Start modular, split only where scaling or ownership proves it necessary.**

## 7. C4 system context

``` mermaid
C4Context
    title VennuSign Connector Platform - System Context
    Person(customerAdmin, "Customer Administrator", "Configures connections and reviews health")
    Person(opsUser, "VennuSign Operator", "Supports connectors, mappings, replay, and incidents")
    System_Ext(source, "External Business System", "Cinema, POS, menu, ticketing, or other provider")
    System(connectorPlatform, "VennuSign Connector Platform", "Securely pulls or receives, maps, validates, and applies external data")
    System_Ext(vennusign, "VennuSign Core Platform", "Owns canonical content, rendering, publishing, and players")

    Rel(customerAdmin, connectorPlatform, "Configures and monitors", "HTTPS")
    Rel(opsUser, connectorPlatform, "Operates and governs", "HTTPS")
    Rel(source, connectorPlatform, "Pushes API/files or answers pulls", "HTTPS/SFTP")
    Rel(connectorPlatform, vennusign, "Applies canonical updates and publishes change events", "Private API/events")
```

## 8. C4 container architecture

``` mermaid
C4Container
    title VennuSign Connector Platform - Containers
    Person(admin, "Administrators and Operators", "Configure and monitor integrations")
    System_Ext(source, "External Systems", "REST APIs and push clients")
    System_Ext(core, "VennuSign Core", "Canonical domain and publishing services")

    System_Boundary(platform, "Connector Platform") {
        Container(gateway, "API Gateway", "Azure API Management", "Authenticates, throttles, validates request envelope, and routes")
        Container(control, "Connector Control Plane", ".NET / Azure Container Apps", "Configuration, mappings, schedules, status, replay, and audit APIs")
        Container(ingress, "Ingestion API", ".NET / Azure Container Apps", "Accepts push payloads and durably records ingestion")
        Container(sftp, "SFTP Landing Zone", "Azure Blob Storage SFTP", "Isolated customer upload folders")
        Container(scheduler, "Pull Scheduler and Workers", ".NET / Azure Container Apps", "Schedules, rate-limits, calls source APIs, and stores responses")
        Container(pipeline, "Mapping Pipeline Workers", ".NET / Azure Container Apps", "Parse, map, validate, deduplicate, diff, and apply")
        ContainerDb(bus, "Durable Message Broker", "Azure Service Bus Premium", "Queues, sessions, retries, and dead-lettering")
        ContainerDb(blob, "Payload and Artifact Store", "Azure Blob Storage", "Immutable raw payloads, quarantine, mapping artifacts, and reports")
        ContainerDb(sql, "Connector Metadata Store", "Azure SQL Database", "Configuration, mappings, lineage, locks, watermarks, and audit")
        ContainerDb(vault, "Secrets Store", "Azure Key Vault", "Provider credentials, keys, and certificates")
        Container(obs, "Observability", "Azure Monitor / Application Insights", "Metrics, traces, logs, alerts, and dashboards")
    }

    Rel(admin, gateway, "Uses", "HTTPS/OIDC")
    Rel(source, gateway, "Pushes", "HTTPS")
    Rel(source, sftp, "Uploads", "SFTP")
    Rel(gateway, control, "Routes control requests", "HTTPS")
    Rel(gateway, ingress, "Routes ingestion requests", "HTTPS")
    Rel(control, sql, "Reads/writes configuration", "TDS/TLS")
    Rel(control, vault, "References credentials", "Managed identity")
    Rel(scheduler, source, "Pulls data", "HTTPS")
    Rel(scheduler, vault, "Gets credentials", "Managed identity")
    Rel(scheduler, blob, "Stores response", "Managed identity")
    Rel(ingress, blob, "Stores payload", "Managed identity")
    Rel(ingress, bus, "Queues ingestion reference", "AMQP/TLS")
    Rel(sftp, bus, "Emits finalized-file event", "Event routing")
    Rel(pipeline, bus, "Consumes and publishes stages", "AMQP/TLS")
    Rel(pipeline, blob, "Reads payload/writes reports", "Managed identity")
    Rel(pipeline, sql, "Reads mappings/writes lineage", "TDS/TLS")
    Rel(pipeline, core, "Applies canonical transaction", "Private HTTPS")
    Rel(pipeline, obs, "Emits telemetry", "OpenTelemetry")
```

## 9. Recommended Azure technology stack

| Concern         | Recommendation                                                               | Reason                                                                                                                |
|-----------------|------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------|
| Runtime         | .NET 10 LTS (or the VennuSign-supported current LTS at implementation time)  | Strong typing, mature HTTP/data libraries, existing VennuSign alignment, good Azure integration                       |
| Compute         | Azure Container Apps, minimum one warm replica for latency-sensitive workers | Managed containers, independent scaling, KEDA-based queue/HTTP autoscaling, revision support                          |
| Inbound gateway | Azure API Management Standard v2 initially                                   | Authentication policies, per-connector throttling, request limits, versioning, analytics, and private backend routing |
| Broker          | Azure Service Bus Premium                                                    | Durable queues/topics, sessions for per-scope ordering, duplicate detection, DLQs, predictable isolation              |
| SFTP            | Azure Blob Storage with hierarchical namespace and SFTP                      | Managed SFTP endpoint without operating servers; SSH-key authentication and folder isolation                          |
| Raw payloads    | Separate Azure Blob Storage account/containers                               | Cheap durable payload retention, quarantine, replay, lifecycle policies, soft delete/versioning                       |
| Metadata/state  | Azure SQL Database, zone redundant where supported                           | Transactions, relational configuration, uniqueness constraints, lineage queries, existing VennuSign fit               |
| Credentials     | Azure Key Vault                                                              | Rotation, auditing, RBAC, certificate/secret support; applications use managed identity                               |
| Observability   | Application Insights, Log Analytics, Azure Monitor alerts/workbooks          | Distributed tracing, SLOs, dashboards, actionable alerts                                                              |
| Deployment      | Bicep plus GitHub Actions using workload identity federation                 | Repeatable infrastructure and no long-lived Azure deployment secrets                                                  |

Container Apps supports event-driven scaling through KEDA, including Service Bus metrics and managed-identity authentication. Service Bus provides duplicate detection, ordered processing through sessions, and dead-letter queues. Azure Blob Storage provides managed SFTP support. API Management supplies authentication, certificate validation, rate limiting, and backend protection. These capabilities make the selected stack a practical match without requiring VennuSign to operate Kubernetes or SFTP servers.

## 10. Logical components

### 10.1 Connector registry and control plane

Stores and governs:

- connector type, version, capabilities, supported transports, and canonical targets;
- connector instance, tenant, location, status, schedules, source scopes, and rate limits;
- credential **references** (never secret values in SQL);
- mapping profiles and versions;
- ownership rules and destination bindings;
- execution history, health, watermarks, cursors, and circuit-breaker state;
- operator approvals, replays, and mapping promotions.

Connector instances move through explicit states: `Draft -> Validating -> Active -> Degraded -> Suspended -> Retired`. Activation requires a connectivity test, mapping validation, sample/dry-run success, and ownership conflict check.

### 10.2 Pull scheduler and workers

- Scheduler calculates due work using connector-specific cadence, provider quota, time zone, priority, and jitter.
- A distributed lease prevents two schedulers from issuing the same logical run.
- Workers obtain credentials from Key Vault, execute HTTPS requests, enforce timeouts, honor `Retry-After`, and use conditional GET/cursors when available.
- Retries use exponential backoff with jitter; persistent failures open a connector-specific circuit breaker.
- A successful response is stored as an immutable raw payload before downstream processing.
- Urgent refresh bypasses ordinary cadence but never bypasses provider quotas or safety limits.

### 10.3 Push REST ingestion

- Every connector receives a stable versioned endpoint or connector identifier.
- Preferred authentication order: OAuth2 client credentials or mTLS; per-connector HMAC/API keys are allowed only when the sender cannot support stronger methods.
- Require a request ID/idempotency key, source timestamp, payload mode (`snapshot` or `delta`), declared schema/version, and scope.
- Stream payload to Blob Storage while calculating a SHA-256 digest and enforcing compressed/decompressed size limits.
- Return `202 Accepted` only after durable storage and queue publication succeed.
- Return the ingestion ID and status URL. Do not hold the request open for mapping or publication.

### 10.4 SFTP landing zone

- Use SFTP, never legacy FTP.
- Prefer one storage account per environment and isolated home directories/containers per tenant or connector; high-risk enterprise tenants may receive dedicated accounts.
- Authenticate with SSH public keys; passwords are disabled by default.
- Upload protocol: source writes a temporary filename, closes it, then renames it to a final filename or uploads a small completion marker. Processing must never start on a partially written object.
- Storage events enqueue only a payload reference. Workers re-check size stability, extension, MIME/content signature, connector ownership, and allowed path.
- File names are metadata only and never trusted for tenant identity or command execution.
- Rejected files move logically to quarantine; they are not silently deleted.

### 10.5 Parser registry

Parsers convert raw bytes into a source-neutral record stream while preserving source locations for errors:

- JSON: streaming parser, maximum nesting depth, duplicate-property policy.
- XML: streaming reader; DTDs and external entities disabled to prevent XXE; maximum depth and expansion limits.
- CSV: explicit encoding, delimiter, quote, header, newline, and culture rules per connector.
- Flat file: versioned record layouts with exact length/column constraints; no heuristic production parsing.

Large payloads must be streamed. A parser cannot load an unbounded file into memory.

### 10.6 Mapping engine

The mapping engine accepts a record stream plus an immutable mapping-profile version and produces a canonical candidate set. The declarative mapping model should support:

- field rename and selection;
- type conversion with explicit locale/time-zone/currency rules;
- constants and defaults;
- concatenation and bounded expressions;
- lookups and reference tables;
- record filtering;
- nested collection construction;
- source ID to canonical external-key mapping;
- explicit delete/tombstone mapping;
- required-field, range, enumeration, and cross-record validation;
- warning versus fatal-error severity.

The expression language must be sandboxed: no filesystem, network, reflection, process access, arbitrary SQL, or unbounded loops. Complex transformations use signed, reviewed extension modules deployed with the platform—not scripts uploaded by customers.

Each mapping profile contains:

- `mappingProfileId` and semantic version;
- source schema identifier/version;
- target canonical type/version;
- compatible connector type versions;
- transformation rules and lookup versions;
- validation policy;
- test fixtures and expected canonical output;
- author, approver, publication time, and rollback predecessor.

### 10.7 Canonical schema registry and governance

Canonical types are owned by VennuSign domain teams, not by individual connectors. Types use semantic versions:

- additive optional fields: backward-compatible minor version;
- changed meaning, deletion, or required field: major version;
- mappings declare compatible source and target versions;
- old versions remain readable during a defined migration window.

**New canonical type gate:** approve a new type only if all are true:

1.  No existing type represents the business concept without semantic loss.
2.  The need is expected from more than one connector or is a strategic VennuSign domain.
3.  Domain ownership, validation, rendering behavior, migration, and lifecycle are defined.
4.  An ADR approves the addition.

Source-specific extra fields may be retained in raw payloads and lineage metadata, but are not automatically added to canonical models.

### 10.8 Validation, diff, and apply

Validation occurs at four levels:

1.  **Envelope:** identity, tenant, connector state, schema version, size, checksum, and replay protection.
2.  **Syntax:** valid JSON/XML/CSV/flat-file structure.
3.  **Mapping:** conversions and required source data.
4.  **Domain:** canonical invariants, references, duplicates, time ranges, currency, and scope completeness.

Only fully valid candidates proceed. Warnings may proceed if the mapping policy explicitly permits them.

The apply path should call a dedicated VennuSign canonical ingestion contract, not write directly into domain tables. The domain service performs a transaction that:

- verifies connector ownership and expected prior version;
- upserts the candidate state or delta;
- records source lineage and canonical version;
- records the ingestion ID in an idempotency ledger;
- writes a transactional outbox event;
- commits atomically.

The outbox publisher emits `CanonicalDataChanged`; existing VennuSign processes then rebuild the affected display artifact and distribute it to players. The connector platform does not directly control player screens.

## 11. Data semantics

### 11.1 Snapshots

A snapshot is complete only for its declared scope. Required envelope fields:

- tenant and connector instance;
- canonical target and scope ID;
- source snapshot ID and generation timestamp;
- source schema version;
- sequence/cursor when supported;
- explicit `snapshot` mode;
- record count and optional digest.

Application algorithm:

1.  Map and validate the entire snapshot into staging.
2.  Compare with last valid canonical state for the same scope.
3.  Reject implausible changes using connector policies (for example, a sudden 100% deletion) unless explicitly confirmed.
4.  Atomically apply upserts and remove records absent from the **same declared scope**.
5.  Advance the source watermark only after commit.

An empty snapshot is not automatically equivalent to "delete everything." Each connector must explicitly declare whether empty is valid and may require a two-step safety rule.

### 11.2 Deltas

Deltas require stable source record IDs and explicit operations: `upsert` or `delete`. When supported, require monotonically increasing sequence numbers or opaque provider cursors. Per scope:

- duplicate event ID: acknowledge and ignore;
- lower/previous sequence: ignore and audit;
- sequence gap: pause the scope, alert, and request/retrieve a recovery snapshot;
- out-of-order concurrent events: serialize with a Service Bus session keyed by tenant + connector + target scope.

### 11.3 Ownership and manual edits

When a connector binds to a canonical scope, its controlled fields/records become read-only in VennuSign. Enforcement belongs in the domain API, not only the UI.

- UI displays source, last synchronization, and a lock indicator.
- Direct writes return a conflict explaining connector ownership.
- Switching connectors or returning to manual control is an explicit, audited operation.
- A new connector may not silently take over a scope already owned by another connector.
- Optional VennuSign presentation metadata may remain editable only if it is outside the connector-owned canonical field set and cannot corrupt source meaning.

## 12. Key data entities

| Entity               | Key fields and purpose                                                                  |
|----------------------|-----------------------------------------------------------------------------------------|
| `ConnectorType`      | Type/version, transport capabilities, parser, canonical targets, lifecycle status       |
| `ConnectorInstance`  | Tenant/location, connector type version, state, credential reference, schedules, policy |
| `SourceScopeBinding` | Connector/source scope to canonical scope; ownership boundary                           |
| `MappingProfile`     | Immutable published version, schemas, rules, tests, approvals                           |
| `PullSchedule`       | Cadence, time zone, priority, quota class, next due time                                |
| `Execution`          | Pull/push/SFTP run, timestamps, state, correlation IDs, diagnostics                     |
| `PayloadArtifact`    | Blob reference, hash, bytes, media type, retention class, scan state                    |
| `Ingestion`          | Mode, scope, source ID/version/sequence, mapping version, status                        |
| `SourceWatermark`    | Cursor/sequence/ETag/last-modified state per connector scope                            |
| `IdempotencyRecord`  | Connector + source event/request ID and terminal result                                 |
| `CanonicalLineage`   | Source record and payload to canonical entity/version association                       |
| `ValidationReport`   | Counts, warnings, fatal errors, field/source locations                                  |
| `AuditEvent`         | Actor/service, action, before/after reference, reason, correlation ID                   |

Use globally unique IDs internally. Every row includes `TenantId`; primary/unique indexes must include tenant boundaries where appropriate. Repository/data-access APIs require a tenant context and defense-in-depth row-level security may be added for operator-query paths.

## 13. State machine

``` mermaid
stateDiagram-v2
    [*] --> Received
    Received --> Parsing
    Parsing --> Mapping
    Mapping --> Validating
    Validating --> Applying
    Applying --> Applied
    Parsing --> Quarantined: Syntax failure
    Mapping --> Quarantined: Mapping failure
    Validating --> Quarantined: Domain failure
    Applying --> RetryPending: Transient failure
    RetryPending --> Applying
    RetryPending --> DeadLettered: Retry exhausted
    Quarantined --> Received: Approved replay
    DeadLettered --> Received: Approved replay
    Applied --> [*]
```

Every transition is persisted with a correlation ID and reason. Terminal failure never alters last valid canonical state.

## 14. Sequence diagrams

### 14.1 REST API pull

``` mermaid
sequenceDiagram
    autonumber
    participant S as Pull Scheduler
    participant W as Pull Worker
    participant E as External REST API
    participant B as Payload Store
    participant Q as Service Bus
    participant M as Mapping Pipeline
    participant V as VennuSign Domain API

    S->>Q: Enqueue due pull run
    Q->>W: Deliver run
    W->>E: GET changes/snapshot with cursor or ETag
    E-->>W: Response + quota/cursor metadata
    W->>B: Store immutable response
    W->>Q: Enqueue ingestion reference
    W-->>Q: Complete pull message
    Q->>M: Deliver ingestion
    M->>B: Stream raw payload
    M->>M: Parse, map, validate, and diff
    M->>V: Apply idempotent canonical change
    V-->>M: Canonical version committed
    M->>Q: Publish CanonicalDataChanged
```

### 14.2 Inbound push API

``` mermaid
sequenceDiagram
    autonumber
    participant E as External System
    participant G as API Gateway
    participant I as Ingestion API
    participant B as Payload Store
    participant Q as Service Bus
    participant M as Mapping Pipeline
    participant V as VennuSign Domain API

    E->>G: POST snapshot/delta + idempotency key
    G->>G: Authenticate, authorize, throttle
    G->>I: Forward validated envelope
    I->>B: Stream and store payload + hash
    I->>Q: Enqueue ingestion reference
    I-->>E: 202 Accepted + ingestion ID
    Q->>M: Deliver ingestion
    M->>M: Parse, map, validate, and diff
    alt Valid
        M->>V: Apply idempotent canonical change
        V-->>M: Commit version
        M->>Q: Publish CanonicalDataChanged
    else Invalid
        M->>B: Store validation report/quarantine
        M->>Q: Publish ConnectorIngestionFailed
    end
```

### 14.3 SFTP push

``` mermaid
sequenceDiagram
    autonumber
    participant E as External SFTP Client
    participant S as SFTP Landing Zone
    participant N as Storage Event Router
    participant Q as Service Bus
    participant M as Mapping Pipeline
    participant V as VennuSign Domain API

    E->>S: Upload temporary file
    E->>S: Close and rename/mark complete
    S->>N: Finalized-object event
    N->>Q: Enqueue connector + object reference
    Q->>M: Deliver ingestion
    M->>S: Verify identity, stability, size, and signature
    M->>M: Parse, map, validate, and diff
    alt Valid
        M->>V: Apply idempotent canonical change
        V-->>M: Commit version
        M->>Q: Publish CanonicalDataChanged
    else Invalid
        M->>M: Quarantine and alert
    end
```

### 14.4 Failure and last-valid-state behavior

``` mermaid
sequenceDiagram
    autonumber
    participant M as Mapping Pipeline
    participant C as Canonical Store
    participant A as Alerting
    participant O as Operator

    M->>M: Validation fails
    M-xC: Do not apply candidate
    Note over C: Last valid state remains active
    M->>A: Emit failure with tenant/connector/scope
    A-->>O: Alert with runbook and ingestion ID
    O->>M: Correct mapping/configuration and replay
    M->>C: Apply only after full validation succeeds
```

## 15. Consistency, concurrency, and idempotency

The platform uses eventual consistency between ingestion and displayed content, with strong transactional consistency at each canonical scope commit.

- **At-least-once transport:** messages may be redelivered.
- **Inbox/idempotency ledger:** VennuSign domain API records ingestion IDs inside the same transaction as changes.
- **Outbox:** canonical change event is recorded in the same transaction and published asynchronously.
- **Per-scope ordering:** Service Bus sessions use a stable session key such as `TenantId:ConnectorInstanceId:CanonicalScopeId`.
- **Optimistic concurrency:** the apply command includes the expected canonical/source version.
- **Duplicate detection:** broker duplicate detection is enabled, but consumer idempotency remains mandatory.
- **Snapshot supersession:** newer completed source versions may supersede queued older snapshots before expensive mapping, when ordering is provable.

Exactly-once delivery across external systems is neither realistic nor required. The correct guarantee is **at-least-once delivery with effectively-once domain effects**.

## 16. Security architecture

### 16.1 Identity and access

- Administrators authenticate through VennuSign's identity provider using OIDC; RBAC separates customer admin, support, mapping author, mapping approver, replay operator, and security auditor.
- External push callers use tenant/connector-scoped OAuth clients or mTLS. HMAC/API keys are connector-specific, hashed or Key-Vault-managed, rotateable, and never shared across tenants.
- Azure workloads use managed identities and least-privilege RBAC; no Azure secrets are embedded in code or deployment configuration.
- Pull credentials are stored only in Key Vault. SQL stores a Key Vault reference and non-secret metadata.
- Production access is just-in-time and audited. Mapping publication and high-impact replay require separate privileges.

### 16.2 Network controls

- Public ingress terminates at API Management; backend services use private networking where feasible.
- Storage, SQL, Service Bus, and Key Vault use private endpoints and deny public network access except the explicitly public SFTP/API entry points.
- Outbound pull traffic uses controlled egress with stable IPs when providers require allowlisting.
- Connector destinations are allowlisted to prevent server-side request forgery. Redirects are disabled or strictly revalidated; private/link-local/metadata IP ranges are blocked.

### 16.3 Payload security

- TLS 1.2+ for HTTPS; SFTP with current ciphers and SSH keys.
- Enforce compressed and expanded size, record count, nesting depth, parse time, and mapping execution limits.
- Disable XML DTD and external entity processing.
- Scan file payloads where supported; content-type sniffing must agree with allowed connector format.
- Encrypt all storage at rest; customer-managed keys are optional for customers with contractual requirements.
- Raw payload access is more restricted than ordinary application logs because it may contain customer/order data.
- Logs contain identifiers and counts, not raw records, credentials, tokens, customer names, order details, or full payloads.

### 16.4 Card-data prohibition

The platform must remain outside the payment-card data path:

- contracts and connector documentation state that PAN, CVV, track data, PIN data, and equivalent payment credentials are forbidden;
- schema allowlists reject known payment fields before mapping;
- pattern detection flags likely PANs in raw payloads and immediately quarantines access;
- prohibited-data incidents trigger a security workflow, restricted deletion/retention handling, and credential review;
- no feature may log or echo rejected sensitive values;
- customer/order data is minimized to the fields required for the display use case and governed by retention policy.

Pattern detection is a backstop, not proof of compliance; provider agreements and schema-level prevention remain primary.

### 16.5 Threats and mitigations

| Threat                        | Primary mitigations                                                                                     |
|-------------------------------|---------------------------------------------------------------------------------------------------------|
| Cross-tenant mapping or apply | Tenant-bound identity, scoped bindings, composite keys, domain authorization, automated isolation tests |
| Replay/duplicate updates      | Idempotency keys, timestamp window, payload digest, inbox ledger, source sequence                       |
| Malicious XML/zip bomb        | Streaming parse, DTD/XXE disabled, expansion and resource limits                                        |
| Credential theft              | Key Vault, managed identity, rotation, least privilege, secret-free logs                                |
| SSRF from pull configuration  | Host allowlists, DNS/IP revalidation, controlled egress, no arbitrary URL templates                     |
| Mapping denial of service     | Sandboxed DSL, instruction/time/memory limits, bounded lookups, offline validation                      |
| Partial SFTP upload           | Temporary name + atomic finalize protocol, stability/marker verification                                |
| Poison message                | Bounded retries, DLQ, quarantine, runbook-driven replay                                                 |
| Mass unintended deletion      | Scoped snapshots, deletion thresholds, empty-snapshot policy, dry run, last-valid retention             |
| Operator abuse/error          | RBAC, separation of duties, immutable audit, approval for publish/replay                                |

## 17. Reliability and failure handling

### 17.1 Retry policy

- Retry only transient failures: timeouts, connection resets, HTTP 408/429/5xx, broker throttling, and temporary database errors.
- Use exponential backoff with jitter and respect provider `Retry-After`.
- Do not automatically retry deterministic parse, schema, mapping, authentication, or domain-validation errors.
- Separate retry budgets by connector so one bad provider cannot exhaust platform workers.

### 17.2 Circuit breakers and bulkheads

- Circuit breaker per connector instance/provider endpoint.
- Separate concurrency pools/queues for pull, API push, file ingestion, and canonical apply.
- Per-tenant quotas prevent noisy-neighbor behavior.
- High-priority urgent updates may use a reserved queue/worker allocation but still follow fairness and quota rules.

### 17.3 Last valid data

The currently active canonical version and displayed artifact remain unchanged when:

- the provider is unavailable;
- authentication fails;
- a payload is malformed;
- mapping or domain validation fails;
- sequence gaps are detected;
- a snapshot breaches safety thresholds;
- downstream apply fails.

Staleness is visible and alerted; it is never silently represented as fresh. Customer-facing screens continue operating from the player/platform's last valid artifact according to VennuSign's existing offline behavior.

### 17.4 Dead-letter and quarantine

- **Quarantine:** payload is understood as permanently invalid until mapping/config/data changes.
- **DLQ:** processing could not complete after transient retry budget was exhausted.
- Operator tools show a sanitized error summary, record/field locations, mapping version, and correlation ID.
- Replay creates a new execution linked to the original payload; it never mutates history.
- Bulk replay is rate limited and requires an impact preview.

## 18. Availability, backup, and disaster recovery

### 18.1 Single-region production baseline

For the confirmed 99.9% / two-hour target:

- deploy two or more replicas for API/control/critical worker apps across availability zones where supported;
- keep at least one warm replica for ingestion and urgent-processing workers;
- use Service Bus Premium with zone redundancy where available;
- use zone-redundant Azure SQL where supported;
- enable Blob soft delete, versioning, lifecycle policies, and zone-redundant storage appropriate to the region;
- deploy infrastructure from Bicep and application images from a replicated registry;
- maintain tested restore scripts and configuration exports.

### 18.2 Recovery objectives

- **RTO:** restore acceptance, processing, and canonical apply within 2 hours.
- **RPO:** 15 minutes for mutable control/metadata; accepted raw payloads are retained durably and can be replayed, making effective ingestion RPO close to zero within the chosen regional storage durability.
- Azure SQL point-in-time recovery and geo-redundant backups support database restoration; restoration drills must verify the actual two-hour objective rather than assume it.

### 18.3 Regional disaster evolution

Do not pay for active-active multi-region processing initially unless business commitments require it. Prepare for warm standby by:

- keeping deployments and configuration fully reproducible;
- using geo-redundant backup/storage options;
- separating globally unique ingestion IDs from regional execution IDs;
- documenting DNS/API failover and SFTP endpoint implications;
- preventing both regions from actively polling/applying the same scope without a global lease.

Run a recovery exercise at least twice annually.

## 19. Observability and operations

Every execution propagates W3C trace context and these identifiers: tenant, connector instance, execution, ingestion, payload, mapping version, canonical scope, and source event/sequence. Sensitive record data must not appear in telemetry.

### 19.1 Metrics

- ingestion acceptance rate and bytes by transport;
- end-to-end and stage latency percentiles;
- queue depth, oldest-message age, retries, DLQ count;
- pull schedule lag, response codes, rate-limit remaining, circuit state;
- parse/mapping/validation failure rates by connector and version;
- canonical apply conflicts and idempotent duplicates;
- last successful sync and data age by connector/scope;
- downstream change-notification and publishing lag;
- SLO availability and error-budget burn.

### 19.2 Alerts

- urgent pipeline p95/p99 target breach;
- no successful sync beyond connector freshness contract;
- queue oldest age above 30 seconds urgent / 5 minutes standard;
- consecutive failures or circuit open;
- DLQ/quarantine growth;
- sequence gap or mass-deletion guard;
- authentication/credential expiry;
- suspected prohibited card data;
- cross-tenant authorization denial spikes;
- database/storage capacity and regional service health.

Alerts should route by severity to the Operations Platform and incident tooling. A single bad record should not page an engineer; repeated connector-wide failure, freshness breach, security signal, or customer-impacting backlog should.

### 19.3 Operational controls

- test connection without ingesting;
- upload/fetch sample and run mapping preview;
- compare candidate with current canonical state;
- activate/suspend connector;
- trigger refresh within quota;
- view health, freshness, execution timeline, and sanitized errors;
- replay one ingestion or an approved range;
- roll back mapping version for future executions and replay affected payloads;
- export audit trail.

## 20. Deployment topology and scaling

Begin with five deployable workloads:

1.  Connector Control Plane API.
2.  Push Ingestion API.
3.  Pull Scheduler/Workers.
4.  Mapping Pipeline Workers.
5.  Outbox/Event Publisher (or hosted with the domain service if already present).

They may share a repository and common libraries but deploy independently. Scale workers on Service Bus queue depth/age; scale APIs on HTTP concurrency. Preserve warm replicas for seconds-level paths.

Suggested initial concurrency controls:

- global worker ceiling protects SQL and VennuSign domain APIs;
- per-provider pull concurrency and requests/second;
- per-connector serial apply through sessions;
- per-tenant concurrent-ingestion quota;
- payload size and records/run limit;
- separate urgent and standard priorities.

Load tests must validate 1,000 locations under synchronized opening-time bursts, provider recovery storms, large snapshots, many tiny deltas, and one deliberately noisy tenant.

## 21. API and event contracts

### 21.1 Push API envelope example

<div id="cb8" class="sourceCode">

``` sourceCode
{
  "schema": "source.vendor-menu.v2",
  "mode": "delta",
  "scope": "location:123/menu:dinner",
  "eventId": "vendor-event-84721",
  "sequence": 1042,
  "generatedAt": "2026-08-13T07:15:22Z",
  "data": {}
}
```

</div>

Required HTTP metadata includes connector identity, `Idempotency-Key`, content type, and optional payload signature. The connector identity—not a tenant ID supplied inside the payload—determines the authorized tenant.

### 21.2 Internal ingestion event

<div id="cb9" class="sourceCode">

``` sourceCode
{
  "eventType": "ConnectorPayloadAccepted.v1",
  "ingestionId": "01J...",
  "tenantId": "...",
  "connectorInstanceId": "...",
  "transport": "PushApi",
  "payloadUri": "internal-reference-only",
  "payloadSha256": "...",
  "mappingProfileVersion": "3.2.0",
  "receivedAt": "2026-08-13T07:15:23Z"
}
```

</div>

Events contain references and non-sensitive metadata, not entire business payloads. Schemas are versioned and registered; consumers tolerate additive fields.

## 22. Mapping lifecycle and testing

A mapping moves through `Draft -> Tested -> Approved -> Published -> Deprecated -> Retired`.

Publication gates:

- schema validation;
- unit fixtures for normal records;
- snapshot and delta fixtures;
- create, update, delete, empty, duplicate, and sequence-gap cases;
- missing/unknown fields and new source schema versions;
- locale, currency, daylight-saving, and time-zone boundaries;
- maximum sizes and pathological nesting/line lengths;
- prohibited card-data fixtures;
- deterministic replay produces identical canonical output;
- comparison against the previous published mapping using representative payloads;
- security review for any compiled extension module.

Production mapping changes use canary rollout by connector instances, with automated error-rate comparison and rapid rollback. Because mappings are immutable after publication, historical executions remain reproducible.

## 23. Architectural trade-offs

### 23.1 Modular platform vs. fine-grained microservices

| Option                         | Pros                                                                                                            | Cons                                                                                                     |
|--------------------------------|-----------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------|
| Modular services (recommended) | Lower operational cost; simpler deployment/debugging; clear code boundaries; independent scaling of major paths | Less isolation between parser/mapper modules; requires discipline to preserve boundaries                 |
| Many microservices             | Maximum team and failure isolation; fine-grained scaling                                                        | Distributed-transaction complexity, higher latency/cost, many deployments, premature for 1,000 locations |

**Decision:** Use a modular architecture with five coarse deployables. Split a component only when measured scale, security isolation, or independent ownership requires it.

### 23.2 Asynchronous pipeline vs. synchronous processing

| Option                                      | Pros                                                                                               | Cons                                                                                      |
|---------------------------------------------|----------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| Asynchronous durable pipeline (recommended) | Absorbs bursts, retries safely, isolates source latency, supports replay, fast API acknowledgement | Eventual consistency; more operational components; requires idempotency                   |
| Synchronous end-to-end                      | Simple request mental model; immediate final answer                                                | Fragile under downstream failure, long timeouts, poor file support, hard to absorb bursts |

**Decision:** Acknowledge only after durable acceptance, then process asynchronously.

### 23.3 Azure SQL vs. NoSQL for metadata

| Option                  | Pros                                                                                  | Cons                                                                                                        |
|-------------------------|---------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------|
| Azure SQL (recommended) | Strong transactions, constraints, relational mappings/lineage, familiar tooling, PITR | Vertical limits eventually; careful indexing/partitioning needed                                            |
| Cosmos DB               | Elastic distribution, flexible documents, global replication                          | Higher conceptual/cost burden; multi-entity transaction limitations; weaker fit for relational control data |

**Decision:** Azure SQL for control and lineage; Blob Storage for large/raw artifacts. Revisit only after measured partition or global-distribution needs.

### 23.4 Service Bus vs. Event Grid alone

| Option                            | Pros                                                                  | Cons                                                                              |
|-----------------------------------|-----------------------------------------------------------------------|-----------------------------------------------------------------------------------|
| Service Bus Premium (recommended) | Durable work queues, sessions, DLQ, duplicate detection, backpressure | Additional cost and broker administration                                         |
| Event Grid alone                  | Excellent event fanout and storage-event integration; serverless      | Not ideal as the sole ordered work queue; weaker workflow retry/control semantics |

**Decision:** Event Grid may capture Blob finalized events, but Service Bus owns durable processing workflows.

### 23.5 Container Apps vs. Functions vs. AKS

| Option                       | Pros                                                                                 | Cons                                                                                    |
|------------------------------|--------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------|
| Container Apps (recommended) | Managed containers, KEDA scaling, revisions, VNet, lower operational burden than AKS | Some platform constraints; warm replicas required for predictable seconds-level latency |
| Azure Functions              | Very fast event development; consumption economics                                   | Runtime/timeout/cold-start constraints and less uniform hosting for diverse parsers     |
| AKS                          | Maximum control and ecosystem                                                        | Highest operational/security burden; unjustified at current scale                       |

**Decision:** Container Apps. Functions-style triggers may be used inside containerized workloads where helpful.

### 23.6 Declarative mapper vs. arbitrary scripts

| Option                                        | Pros                                                             | Cons                                                                  |
|-----------------------------------------------|------------------------------------------------------------------|-----------------------------------------------------------------------|
| Constrained declarative mapping (recommended) | Auditable, testable, deterministic, safer multi-tenant execution | Complex edge cases need approved extensions; DSL must be designed     |
| Arbitrary scripts/plugins                     | Maximum flexibility and rapid one-offs                           | Severe security, determinism, resource, versioning, and support risks |

**Decision:** Declarative mappings plus signed reviewed extension modules.

### 23.7 Shared vs. dedicated tenant infrastructure

| Option                                     | Pros                                         | Cons                                                      |
|--------------------------------------------|----------------------------------------------|-----------------------------------------------------------|
| Shared multi-tenant baseline (recommended) | Efficient and manageable for 1,000 locations | Requires rigorous logical isolation and quota enforcement |
| Dedicated stack per tenant                 | Strong isolation and custom scaling          | High cost and operational sprawl                          |

**Decision:** Shared platform with tenant-scoped identities, paths, rows, encryption, quotas, and tests. Allow dedicated landing zones/stacks only for contractual enterprise needs.

## 24. Architecture Decision Records

### ADR-001: One Connector Platform for pull and push

- **Status:** Proposed
- **Context:** Pull and push differ in transport but require identical mapping, validation, lineage, failure handling, and canonical application.
- **Decision:** Implement common ingestion and mapping services behind REST pull, REST push, and SFTP adapters.
- **Consequences:** Reduced duplication and consistent semantics; shared services become critical infrastructure and require strong isolation/observability.

### ADR-002: Canonical data types are reused by default

- **Status:** Proposed
- **Context:** Creating a schema per source would couple VennuSign to vendors and fragment rendering/business logic.
- **Decision:** Map sources into existing versioned canonical types; require an ADR/domain review for a new type.
- **Consequences:** Connectors remain replaceable and themes consume stable data; schema governance becomes a formal responsibility.

### ADR-003: Event-driven asynchronous processing

- **Status:** Proposed
- **Context:** External systems are bursty and unreliable, and file/snapshot processing may outlive an HTTP request.
- **Decision:** Durably store input and enqueue references; process via Service Bus with at-least-once delivery and idempotent effects.
- **Consequences:** High resilience and replayability; eventual consistency and operational queue management are accepted.

### ADR-004: Azure managed-services stack

- **Status:** Proposed
- **Context:** VennuSign needs seconds-level paths and 1,000-location capacity without a large platform-operations team.
- **Decision:** Use Azure Container Apps, API Management, Service Bus Premium, Blob/SFTP, Azure SQL, Key Vault, and Azure Monitor.
- **Consequences:** Lower operational burden and strong Azure integration; introduces Azure coupling and managed-service cost.

### ADR-005: At-least-once delivery with effectively-once effects

- **Status:** Proposed
- **Context:** Network and broker retries make exactly-once distributed delivery impractical.
- **Decision:** Use source event IDs, hashes, per-scope ordering, an inbox/idempotency ledger, optimistic concurrency, and a transactional outbox.
- **Consequences:** Safe retries and replay; every connector must provide or derive stable identifiers.

### ADR-006: Integration-controlled data is read-only

- **Status:** Accepted from product requirement
- **Context:** Manual edits would create ambiguous ownership and would be overwritten unpredictably.
- **Decision:** Domain APIs reject manual writes to connector-owned fields/scopes.
- **Consequences:** Clear source of truth; customers must change data upstream or disconnect ownership explicitly.

### ADR-007: Preserve last valid data on failure

- **Status:** Accepted from product requirement
- **Context:** Replacing working signage with invalid/empty content is more harmful than temporary staleness.
- **Decision:** Candidates are staged and validated fully before atomic application; failures alert and retain prior state.
- **Consequences:** Displays remain stable; the system must prominently track and alert on staleness.

### ADR-008: SFTP is managed Blob ingress; FTP is unsupported

- **Status:** Proposed
- **Context:** Cinema and legacy systems often push files, but operating SFTP servers adds security and maintenance burden; FTP is unencrypted.
- **Decision:** Use Azure Blob Storage SFTP with SSH keys, isolated folders, and finalized-file protocol. Do not support FTP.
- **Consequences:** Secure managed file delivery; Azure SFTP feature constraints and costs must be validated during implementation.

### ADR-009: Mapping uses a safe DSL

- **Status:** Proposed
- **Context:** Vendor schemas vary, but arbitrary user code creates unacceptable multi-tenant risk.
- **Decision:** Build a declarative versioned mapping DSL with bounded execution and reviewed extension modules.
- **Consequences:** Strong auditability and security; initial investment is required and rare transforms may need code releases.

### ADR-010: No payment-card data

- **Status:** Accepted from product requirement
- **Context:** Display integrations do not require cardholder data and accepting it would greatly expand risk and compliance scope.
- **Decision:** Prohibit card data contractually and technically; detect, quarantine, and escalate suspected violations.
- **Consequences:** Lower risk and narrower compliance scope; incompatible source payloads must be filtered upstream or rejected.

## 25. Phased implementation plan

### Phase 0 — Contract and threat-model foundation

- Define canonical schema/version governance and new-type approval.
- Define ingestion envelope, snapshot/delta semantics, ownership contract, and internal events.
- Threat model API push, SFTP, pull SSRF, mapping execution, tenant isolation, and prohibited data.
- Create NFR/SLO dashboards and a performance test model before implementation.

### Phase 1 — Vertical platform slice

- Control-plane data model and connector lifecycle.
- Push REST ingestion with durable payload storage and `202` acknowledgement.
- JSON parser, mapping DSL minimum viable subset, validation, dry run, and one existing canonical target used only as a platform test fixture.
- Service Bus pipeline, idempotent apply contract, lineage, outbox, quarantine, alerting, and replay.
- No public production connector yet.

### Phase 2 — Pull platform

- Scheduler, distributed leases, conditional requests/cursors, quota management, retry/circuit breaker, and urgent refresh.
- OAuth2/API-key/mTLS credential strategies through Key Vault.
- Provider simulator and chaos tests for throttling, timeouts, duplicate data, and sequence gaps.

### Phase 3 — File push platform

- Managed SFTP landing zone, tenant isolation, finalized-file protocol, file events, quarantine, and lifecycle cleanup.
- XML/CSV/flat-file streaming parsers and security limits.
- File validation reports accessible to operators/customers without exposing sensitive records.

### Phase 4 — Operations and controlled rollout

- Operations Platform views: freshness, runs, errors, mappings, replay, credential expiry, and audit.
- Canary mapping publication, rollback, mass-change safety previews, and capacity dashboards.
- Load, penetration, tenant-isolation, disaster-recovery, and prohibited-data response exercises.
- Select the first production connector only after the platform acceptance criteria pass.

## 26. Acceptance criteria

The platform is ready for its first production connector when:

1.  REST pull, REST push, and SFTP can all produce the same canonical result from equivalent fixtures.
2.  Snapshot/delta, duplicate, replay, out-of-order, gap, and empty-snapshot cases behave as specified.
3.  Invalid input never changes last valid canonical state.
4.  Manual domain edits are rejected for connector-controlled scopes.
5.  Cross-tenant penetration and automated isolation tests show no data or control leakage.
6.  Suspected card data is quarantined without leaking values into logs or responses.
7.  A 1,000-location load model meets the processing SLOs, including synchronized bursts.
8.  Queue loss, worker termination, database transient faults, and provider outages recover without duplicate effects.
9.  Mapping rollback and payload replay are demonstrated.
10. A recovery drill restores the critical service inside two hours and meets the RPO target.
11. Operators can identify a failed connector, understand the sanitized cause, retain last valid content, and safely replay it.
12. The true freshness contract is visible for every pull connector and reflects provider limitations.

## 27. Risks and follow-up decisions

| Risk/open decision                                                  | Required action before production                                                                                                       |
|---------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------|
| Seconds-level pull may violate provider quotas                      | Establish freshness contract for each connector; prefer delta endpoints/conditional requests/push                                       |
| Canonical domain API may not yet support ownership/idempotent apply | Design and implement the domain ingestion contract and transactional outbox                                                             |
| SFTP feature limits/cost vary by Azure configuration                | Run a focused proof of concept with finalize semantics, isolation, events, and lifecycle                                                |
| Customer/order data creates privacy obligations                     | Define purpose, minimization, retention, access, deletion, and incident policies per canonical type                                     |
| Mapping DSL scope can grow into a product                           | Keep initial operations minimal; require evidence before adding language features                                                       |
| Downstream screen refresh latency is outside this subsystem         | Define a separate end-to-end VennuSign display freshness SLO across canonical apply, render, publish, and player receipt                |
| No first connector is selected                                      | Use provider simulators and representative cinema/menu fixtures, but delay vendor-specific decisions until platform contracts stabilize |

## 28. Final recommendation

Build the Connector Platform as a shared, Azure-managed, event-driven ingestion capability with coarse-grained .NET services. Treat REST pull, REST push, and SFTP as interchangeable transport adapters feeding one immutable, versioned mapping and validation pipeline. Keep VennuSign canonical types stable and source-independent; enforce integration ownership in the domain API; preserve last valid data on every failure; and make replay, lineage, freshness, and tenant isolation first-class rather than operational afterthoughts.

The most important product promise should be precise: **VennuSign processes urgent data within seconds after it is received or retrieved.** For API pull, detection time remains connector-specific because the external provider controls rate limits and change visibility.

## 29. Primary implementation references

- [Azure Container Apps scaling](https://learn.microsoft.com/en-us/azure/container-apps/scale-app)
- [Azure Service Bus duplicate handling and ordered sessions](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-message-loss-and-duplicates)
- [Azure Service Bus dead-letter queues](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues)
- [SFTP support for Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/secure-file-transfer-protocol-support)
- [Azure API Management policy reference](https://learn.microsoft.com/en-us/azure/api-management/api-management-policies)
- [Azure Key Vault application authentication](https://learn.microsoft.com/en-us/azure/key-vault/general/authentication)
- [Azure SQL automated backups](https://learn.microsoft.com/en-us/azure/azure-sql/database/automated-backups-overview)
- [Azure Architecture Center reliability patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)
