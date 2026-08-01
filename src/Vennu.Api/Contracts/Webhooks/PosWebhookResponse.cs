namespace Vennu.Api.Contracts.Webhooks;

public sealed record PosWebhookResponse(bool Received, bool Queued);
