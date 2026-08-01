namespace Vennu.Core.Models;

public enum ExternalIdentityProvider
{
    Google = 1,
    Apple = 2
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
