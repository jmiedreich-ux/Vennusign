namespace Vennu.Core.Models;

public enum CustomerUserStatus
{
    Active = 1,
    Suspended = 2,
    Deleted = 3
}

public sealed class CustomerUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public CustomerUserStatus Status { get; set; } = CustomerUserStatus.Active;
    public DateTime? EmailVerifiedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
