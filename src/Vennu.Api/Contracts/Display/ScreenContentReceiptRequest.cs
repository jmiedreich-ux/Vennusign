using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Display;

public sealed class ScreenContentReceiptRequest
{
    [Range(1, long.MaxValue)] public long Revision { get; set; }
    [Required, StringLength(20)] public string State { get; set; } = string.Empty;
    [Required, StringLength(9)] public string ScreenKey { get; set; } = string.Empty;
    [StringLength(50)] public string? PlayerVersion { get; set; }
    [StringLength(50)] public string? ShellVersion { get; set; }
    [StringLength(50)] public string? Platform { get; set; }
    [StringLength(50)] public string? FailureCode { get; set; }
    [StringLength(240)] public string? FailureDetail { get; set; }
    public bool Recovered { get; set; }
}

public sealed record ScreenContentReceiptResponse(long AuthoritativeRevision, long? AppliedRevision, string State, DateTime? AppliedUtc);
