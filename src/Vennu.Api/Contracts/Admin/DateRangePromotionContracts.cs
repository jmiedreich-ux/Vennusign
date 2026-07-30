namespace Vennu.Api.Contracts.Admin;

public sealed record DateRangePromotionWriteRequest(
    string Name,
    DateTime StartLocalDate,
    DateTime EndLocalDate,
    string? TargetLayout,
    string? Title,
    string? Body,
    int Priority,
    bool IsEnabled);
