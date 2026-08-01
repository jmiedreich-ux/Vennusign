using Microsoft.AspNetCore.DataProtection;
using Vennu.Data.Services;

namespace Vennu.Api.CustomerAuthentication;

public sealed class DataProtectionCustomerSecretProtector(IDataProtectionProvider provider) : ICustomerSecretProtector
{
    private readonly IDataProtector protector = provider.CreateProtector("Vennu.CustomerAuthentication.StrongFactors.v1");
    public string Protect(byte[] secret) => protector.Protect(Convert.ToBase64String(secret));
    public byte[] Unprotect(string protectedSecret) => Convert.FromBase64String(protector.Unprotect(protectedSecret));
}
