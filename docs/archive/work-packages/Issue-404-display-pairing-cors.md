# Issue-404 — Display Pairing CORS

## Status

Complete through PR #405.

## Execution Mode

Collaborative

## Evidence

The browser pairing page at `http://localhost:5175/pair` showed “Pairing unavailable.” Direct `POST https://localhost:7138/api/screens` returned 201, while the browser preflight from `Origin: http://localhost:5175` returned 204 without `Access-Control-Allow-Origin`.

## Scope

- Add exact HTTP and HTTPS localhost Display origins to the Development-only CORS allowlist.
- Keep the allowlist explicit; do not add a wildcard or alter production configuration.
- Add focused preflight regression coverage.

## Validation

- API unit tests passed 331/331, including 2 focused CORS tests.
- Debug API build passed.
- Actual preflight returned 204 with `Access-Control-Allow-Origin: http://localhost:5175`.
- Actual browser-equivalent screen registration returned 201 with the same allowed origin and a screen ID.
- API and Display are currently listening on ports 7138 and 5175 for immediate browser reload.
- GitHub Actions pending.
- Initial CI exposed a parallel theory/TestServer disposal race; origin checks now execute sequentially in one test client and the complete API unit project passes.
