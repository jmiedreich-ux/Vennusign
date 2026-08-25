# qwen3.5:9b-q4_K_M — review result

- Fixture commit: `e178f44`
- Start/finish: 2026-08-25T00:21:13.432870530-04:00 / 2026-08-25T00:23:23.539838233-04:00
- Wall-clock duration: 130,106,011,552 ns; exit 0; GPU sampling recorded the required context.
- Score: 48/100.

Awarded seeded findings (28/70): raw access-token logging (7), broad exception with misleading success (7), check-then-write race (7), and non-atomic account/binding writes (7). The review did not identify unverified-email trust, third-party provider rebinding, unique-key collision handling, premature subject mutation, normalization, or the required focused regression coverage.

Severity/prioritization: 8/10; remediation: 8/10; regression-test plan: 4/10. The complete unedited model review is `review-output.md`.
