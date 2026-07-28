# Stripe Webhook Operations

## Endpoint

Configure Stripe to deliver these snapshot events to `POST /api/webhooks/stripe`:

- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `invoice.paid`

Verified event types outside this list are acknowledged without processing.

## Configuration

Provide the endpoint signing secret through `Stripe__Webhook__SigningSecret`. Never commit the `whsec_` value. The optional `Stripe__Webhook__ToleranceSeconds` setting defaults to 300 seconds and must remain between 1 and 3600.

Stripe.net 52.1.0 expects the `2026-06-24.dahlia` API release train. Configure the Stripe webhook endpoint to use a compatible API version.

## Subscription Contract

- Set subscription metadata `venue_id` to the Vennu venue GUID.
- Use exactly one active Stripe subscription item for the selected Vennu tier.
- Configure that item's Price ID on the matching Vennu subscription tier.

Invalid signatures and malformed supported payloads return HTTP 400. Valid supported events are processed through the persistent Stripe event idempotency service, so duplicate deliveries are safe.
