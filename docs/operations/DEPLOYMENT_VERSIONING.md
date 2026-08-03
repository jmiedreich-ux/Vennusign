# Deployment Versioning

`docs/operations/release/release-manifest.template.json` is the canonical product/component version source. A release pipeline materializes it with the tested source commit and build identity, validates it, and promotes those exact immutable artifacts; production never rebuilds a staging-approved component.

Every independently deployable component has its own semantic version. The product version names one approved combination. A component marked `carried-forward` must preserve both its version and artifact identity, so a product-only version change cannot rebuild or redeploy it. TV shells also use increasing platform build numbers and remain separate from the hosted player. Shells and the player declare the native-bridge range they support.

The database uses ordered `YYYYMMDD.NN` migration versions and expand-and-contract changes while older releases remain supported. Existing tables, columns, and callable behavior cannot be removed or changed incompatibly during that window. A stored procedure receives a new callable contract version when its parameters, result shape, meaning, or behavior becomes incompatible; compatible internal fixes retain the contract version.

Builds provide the manifest values to runtime through `VENNU_PRODUCT_VERSION`, `VENNU_COMPONENT_VERSION`, `VENNU_API_CONTRACT_MAJOR`, `VENNU_SOURCE_COMMIT`, `VENNU_BUILD_ID`, `VENNU_DATABASE_SCHEMA_VERSION`, and `VENNU_CONFIGURATION_SCHEMA_VERSION`. The API exposes these non-secret values at `/health/version`.

Future Platform Operations deployment workflows consume the validated manifest to compare an environment's current combination with a target release, select only `changed` artifacts, enforce contract compatibility, and record carried-forward identities. Customer schedules, migration waves, provisioning, rollback orchestration, and retirement remain outside this foundation.
