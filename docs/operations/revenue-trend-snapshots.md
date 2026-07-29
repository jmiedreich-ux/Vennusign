# Revenue Trend Snapshots

## Capture

- A successful protected live-revenue request captures the established Stripe-derived USD totals.
- `SnapshotDateUtc` is normalized to midnight UTC and is the table primary key.
- Additional captures on the same UTC day update that row, so retries do not create duplicates.
- Capture stores aggregate revenue values only; Stripe payloads, customer data, and credentials are not persisted.

## Trend

- `GET /api/admin/dashboard/revenue/trend?months=12` returns 1 to 24 months.
- The latest daily snapshot in each UTC calendar month represents that month.
- Results are ordered oldest to newest.
- MRR percentage change is returned only when an immediately preceding calendar month exists and its MRR is nonzero.
- A missing or zero prior month returns `null`; the dashboard displays “No prior month” rather than fabricating a percentage.

## Validation

- Migration `011_create_revenue_daily_snapshots.sql` is covered by non-integration migration-resource validation.
- Integration-type tests are intentionally skipped under the standing repository-owner instruction.
