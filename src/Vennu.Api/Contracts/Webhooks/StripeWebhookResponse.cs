namespace Vennu.Api.Contracts.Webhooks;

public sealed record StripeWebhookResponse(bool Received, bool Processed);
