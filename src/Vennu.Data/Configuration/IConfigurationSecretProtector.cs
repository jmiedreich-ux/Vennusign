namespace Vennu.Data.Configuration;

public interface IConfigurationSecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}
