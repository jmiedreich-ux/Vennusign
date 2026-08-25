# Review scoring rubric

Ten seeded findings are worth 7 points each: verified-email trust; cross-provider rebinding; check-then-write race; unhandled unique-key collision; premature subject mutation; sensitive-token logging; broad exception/misleading success; non-atomic related updates; inconsistent normalization; missing focused regression coverage. Award a finding only when the review makes the concrete patch behavior and impact actionable.

Severity/prioritization, minimally invasive remediation, and focused regression tests are worth 10 points each. The exact scorer records the supporting sentence for every awarded finding.
