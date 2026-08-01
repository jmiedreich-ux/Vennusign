namespace Vennu.Core.Models;

public enum CustomerAuthenticationMethod
{
    Google = 1,
    Apple = 2,
    EmailLink = 3
}

public sealed class CustomerAuthSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public CustomerAuthenticationMethod AuthenticationMethod { get; set; }
    public DateTime AuthenticatedUtc { get; set; }
    public DateTime LastSeenUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public sealed class EmailLoginToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string ReturnPath { get; set; } = string.Empty;
    public DateTime ExpiresUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
}
