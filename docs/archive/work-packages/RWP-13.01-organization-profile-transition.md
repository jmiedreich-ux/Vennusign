# RWP-13.01 — Organization Profile and Onboarding-to-Back-Office Transition

## UI and function gap analysis

| Concern | Completed behavior |
| --- | --- |
| Goal and hierarchy | Account setup now gathers the minimum business profile before Plan; Go Live presents Open Back Office as the primary transition. |
| Read/create behavior | New profiles return in the authorized onboarding snapshot; existing organizations retain nullable compatibility values without fabricated data. |
| Required/optional fields | Display name, primary contact name, contact email, and mailing address are required. Legal name and phone are labeled optional. |
| Validation and privacy | Browser types/lengths provide immediate help; server trimming, limits, email validation, tenant-derived ownership, and authorized-only copy remain authoritative. |
| Essential states | Existing loading, error, retry, saved, checkout, pairing, paired-offline, Online, missing-session, and stale-access recovery remain intact. |
| Accessibility/responsiveness | Every input has a label and appropriate autocomplete; status/error semantics remain; the existing single-column form and responsive completion actions need no parallel page. |
| API/data/authorization | Migration 054 adds nullable compatibility columns; creation is membership/audit transactional; snapshots resolve only the journey-owned organization. |
| Transition safety | Paired-offline and Online customers may open `/`; Back Office revalidates membership and saved venue and fails closed on missing or removed access. |

## Validation

- Back Office tests/build pass locally.
- Focused data-access tests cover profile normalization and snapshot persistence; exact-head Actions is authoritative for .NET validation.
- No live database, provider, browser, or device validation was run.

Completion evidence remains in the implementation PR. Issue: #416. Phase 14+ remains paused.
