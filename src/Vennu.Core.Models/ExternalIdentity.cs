namespace Vennu.Core.Models;

public enum ExternalIdentityProvider
{
    Google = 1,
    Apple = 2,

    /// <summary>
    /// Entra External ID's own local account (email + password/OTP, "Sign in with Vennusign"),
    /// not a third-party federated identity. Distinct from Google/Apple even though all three
    /// are brokered through the same Entra tenant - see
    /// docs/design/approved/authentication/decisions.md #2.
    /// </summary>
    Vennusign = 3
}

public sealed class ExternalIdentity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ExternalIdentityProvider Provider { get; set; }
    public string ProviderSubject { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
