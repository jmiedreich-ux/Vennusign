namespace Vennu.Core.Models;

public enum CustomerAuthenticationMethod
{
    Google = 1,
    Apple = 2,
    EmailLink = 3,
    Passkey = 4,
    Totp = 5,
    RecoveryCode = 6,

    /// <summary>Entra External ID's own local account ("Sign in with Vennusign").</summary>
    Vennusign = 7
}

public enum CustomerAuthenticationAssurance { Primary = 1, Strong = 2, Recovery = 3 }
public enum CustomerAuthenticationChallengeType { PasskeyRegistration = 1, PasskeyAssertion = 2 }

public sealed class CustomerAuthSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public CustomerAuthenticationMethod AuthenticationMethod { get; set; }
    public CustomerAuthenticationAssurance Assurance { get; set; } = CustomerAuthenticationAssurance.Primary;
    public DateTime AuthenticatedUtc { get; set; }
    public DateTime? StepUpUtc { get; set; }
    public DateTime LastSeenUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public sealed class CustomerPasskeyCredential
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public byte[] CredentialId { get; set; } = [];
    public byte[] PublicKey { get; set; } = [];
    public byte[] UserHandle { get; set; } = [];
    public uint SignatureCounter { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime? LastUsedUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
}

public sealed class CustomerTotpAuthenticator
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ProtectedSecret { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime? VerifiedUtc { get; set; }
    public long? LastAcceptedCounter { get; set; }
    public DateTime? RevokedUtc { get; set; }
}

public sealed class CustomerRecoveryCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime? UsedUtc { get; set; }
}

public sealed class CustomerAuthenticationChallenge
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public CustomerAuthenticationChallengeType Type { get; set; }
    public string ProtectedOptions { get; set; } = string.Empty;
    public DateTime ExpiresUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
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
