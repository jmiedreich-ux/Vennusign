namespace Vennu.Api.Contracts.Admin;

public sealed record EmergencyBroadcastWriteRequest(
    Guid? ScreenId, string Title, string Message, string? MediaUrl, int DurationMinutes);
