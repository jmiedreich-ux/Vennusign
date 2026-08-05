using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public sealed class RepositoryCapabilityMessageCatalogTests
{
    private readonly RepositoryCapabilityMessageCatalog catalog = new();

    [Fact]
    public void LocaleFallback_IsSpecificLanguageThenDefault()
    {
        Assert.Equal(["fr-CA", "fr", "en-US"], catalog.GetFallbackChain("fr-CA"));
        Assert.Equal("Cette action est disponible.", catalog.Resolve("fr-CA", "decisions.allowed"));
        Assert.Equal(
            "Demandez à un administrateur l’autorisation d’effectuer cette action.",
            catalog.Resolve("fr-CA", "decisions.permission.required"));
        Assert.Equal(
            "The current allowance has been reached.",
            catalog.Resolve("fr-CA", "decisions.allowance.reached"));
    }

    [Fact]
    public void UnknownLocaleAndUnknownKeyFailSafely()
    {
        Assert.Equal(["en-US"], catalog.GetFallbackChain("not-a-locale"));
        Assert.Equal("decisions.unknown", catalog.Resolve("not-a-locale", "decisions.unknown"));
    }
}
