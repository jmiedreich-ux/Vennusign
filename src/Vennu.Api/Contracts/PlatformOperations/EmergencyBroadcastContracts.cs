namespace Vennu.Api.Contracts.PlatformOperations;

public sealed record EmergencyBroadcastWriteRequest(
    Guid? ScreenId, string Title, string Message, string? MediaUrl, int DurationMinutes);
