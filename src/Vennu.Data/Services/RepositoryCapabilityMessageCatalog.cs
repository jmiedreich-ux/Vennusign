using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Vennu.Data.Services;

public interface ICapabilityMessageCatalog
{
    string Resolve(string locale, string messageKey, IReadOnlyDictionary<string, string>? parameters = null);
    IReadOnlyList<string> GetFallbackChain(string locale);
}

public sealed class RepositoryCapabilityMessageCatalog : ICapabilityMessageCatalog
{
    private const string DefaultLocale = "en-US";
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> catalogs;

    public RepositoryCapabilityMessageCatalog()
        : this(LoadEmbeddedCatalogs())
    {
    }

    internal RepositoryCapabilityMessageCatalog(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> catalogs) =>
        this.catalogs = catalogs;

    public string Resolve(string locale, string messageKey, IReadOnlyDictionary<string, string>? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageKey);
        foreach (var candidate in GetFallbackChain(locale))
        {
            if (catalogs.TryGetValue(candidate, out var catalog) && catalog.TryGetValue(messageKey, out var message))
            {
                return ApplyParameters(message, parameters);
            }
        }

        return messageKey;
    }

    public IReadOnlyList<string> GetFallbackChain(string locale)
    {
        var normalized = NormalizeLocale(locale);
        var chain = new List<string> { normalized };
        var separator = normalized.IndexOf('-');
        if (separator > 0)
        {
            chain.Add(normalized[..separator]);
        }

        if (!chain.Contains(DefaultLocale, StringComparer.OrdinalIgnoreCase))
        {
            chain.Add(DefaultLocale);
        }

        return chain.AsReadOnly();
    }

    private static string NormalizeLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale)) return DefaultLocale;
        try
        {
            return CultureInfo.GetCultureInfo(locale.Trim()).Name;
        }
        catch (CultureNotFoundException)
        {
            return DefaultLocale;
        }
    }

    private static string ApplyParameters(string message, IReadOnlyDictionary<string, string>? parameters)
    {
        if (parameters is null) return message;
        return parameters.Aggregate(
            message,
            (current, pair) => current.Replace($"{{{pair.Key}}}", pair.Value, StringComparison.Ordinal));
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadEmbeddedCatalogs()
    {
        var assembly = typeof(RepositoryCapabilityMessageCatalog).Assembly;
        var prefix = $"{assembly.GetName().Name}.Resources.CapabilityMessages.";
        var loaded = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var resource in assembly.GetManifestResourceNames().Where(name => name.StartsWith(prefix, StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Message catalog resource '{resource}' is unavailable.");
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
                ?? throw new InvalidOperationException($"Message catalog resource '{resource}' is invalid.");
            var locale = resource[prefix.Length..^".json".Length];
            loaded[locale] = new ReadOnlyDictionary<string, string>(values);
        }

        if (!loaded.ContainsKey(DefaultLocale))
        {
            throw new InvalidOperationException($"The required '{DefaultLocale}' capability message catalog is missing.");
        }

        return new ReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>(loaded);
    }
}
