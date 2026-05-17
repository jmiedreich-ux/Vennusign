using System.Security.Cryptography;

namespace Vennu.Api.Infrastructure;

internal static class IdentifierGenerator
{
    private const string ScreenKeyCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    public static string CreateScreenKey() => $"sc-{CreateToken(6, ScreenKeyCharacters)}";

    public static string CreatePairingCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string CreateToken(int length, string characters)
    {
        Span<char> token = stackalloc char[length];

        for (var index = 0; index < token.Length; index++)
        {
            token[index] = characters[RandomNumberGenerator.GetInt32(0, characters.Length)];
        }

        return new string(token);
    }
}
