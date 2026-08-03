# Stripe Revenue Dashboard Operations

The protected Platform Operations revenue panel reads current active subscriptions directly from Stripe.

Configure the restricted Stripe secret key through:

```text
Stripe__Revenue__ApiKey
```

Do not commit a Stripe secret key. Grant the key read access to subscriptions and prices only. The dashboard supports USD `per_unit` recurring prices with monthly or annual intervals; unsupported or mixed-currency data fails closed rather than displaying an incomplete total.

The request uses Stripe.net automatic pagination, includes subscription quantities, and maps prices to Vennu tiers through `StripeMonthlyPriceId` and `StripeAnnualPriceId`. Unmapped prices remain visible in the dashboard warning.

Revenue is calculated live and is not persisted. Month-over-month history requires a later bounded work package with approved snapshot semantics.
