using Microsoft.AspNetCore.DataProtection;
using Vennu.Data.Services;

namespace Vennu.Api.Infrastructure;

public sealed class DataProtectionPosCredentialProtector(IDataProtectionProvider dataProtectionProvider)
    : IPosCredentialProtector
{
    private const string Purpose = "Vennu.PosCredentials.v1";
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector(Purpose);

    public string Protect(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        return protector.Protect(plaintext);
    }

    public string Unprotect(string protectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedValue);
        return protector.Unprotect(protectedValue);
    }
}
