namespace Vennu.Core.Models;

public enum PosProvider
{
    Square = 1,
    Toast = 2,
    Clover = 3
}

public enum PosConnectionStatus
{
    Disconnected = 0,
    Connected = 1,
    Error = 2,
    ReauthorizationRequired = 3
}

public sealed class PosConnection
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public PosProvider Provider { get; set; }
    public PosConnectionStatus Status { get; set; }
    public string ExternalMerchantId { get; set; } = string.Empty;
    public string ProtectedAccessToken { get; set; } = string.Empty;
    public string? ProtectedRefreshToken { get; set; }
    public DateTime? AccessTokenExpiresUtc { get; set; }
    public DateTime? LastSyncedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
