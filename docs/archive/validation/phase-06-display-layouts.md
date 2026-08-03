# Phase 06 Display Layouts Validation

## Capability Map

| Player area | Delivered capability | Repeatable non-integration evidence |
| --- | --- | --- |
| Layout foundation | Typed additive registry with deterministic fallback | layout registry tests; critical journeys |
| Photo Grid | Responsive food cards, merchandising states, four densities, and wall overflow | Photo Grid tests; display/API mapping tests |
| Classic Diner | Two/three-column text menu, aligned prices, happy hour, and daily special | Classic Diner tests; display/API mapping tests |
| Basic themes | Venue-scoped colors/fonts, admin builder, exact player preview, and venue-wide refresh | theme service/admin/display tests |
| Offline content | Versioned per-screen cache, bounded age, safe 404 behavior, and online recovery | display cache tests; critical journeys |
| Offline media | Versioned image cache, old-version cleanup, network refresh, and cached fallback | display cache tests; service-worker contract |

## Critical Journeys

`phase06-critical-journeys.test.mjs` validates the composed player path across layout selection, merchandising, pricing, theme preview, real-time updates, offline content, online recovery, and media fallback. The focused layout, theme, cache, API, service, repository, and migration tests remain the detailed evidence for each bounded behavior.

## Standing Validation Exception

Azure SQL execution, hosted infrastructure, credentials, containers, external services, browser/network integration, and other integration-type tests are intentionally skipped under the standing repository-owner instruction. Their omission is not evidence that those external paths were exercised.

## Residual Risks

- Service-worker behavior is validated through deterministic source contracts rather than a browser-controlled network-disconnection suite.
- Cross-origin CDN cache behavior depends on production CDN response policy; opaque image responses are intentionally supported.
- Azure SQL migration execution remains environment-dependent despite migration inventory and pure unit validation.

## Phase Result

Phase 06 delivers production-ready restaurant/cafe layouts, all-tier basic theming, multi-screen overflow, and a player that keeps the last valid menu and images visible through temporary outages.

## Validation Evidence

- Local display contracts passed 45/45.
- GitHub Actions run #282 passed restore, Release build, both frontend production builds/tests, non-integration unit tests, and the explicit integration-test skip at reviewed head `92b9f3e`.
- PR #116 merged as `2095731`.
- Integration-type tests were intentionally skipped.
