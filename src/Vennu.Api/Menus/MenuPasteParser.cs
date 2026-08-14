using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Vennu.Core.Models;

namespace Vennu.Api.Menus;

public sealed record ParsedMenuPaste(
    IReadOnlyCollection<MenuImportSourceLine> Lines,
    IReadOnlyCollection<MenuImportReviewQuestion> Questions,
    int ItemCount);

public sealed class MenuPasteParser
{
    private static readonly Regex PriceAtEnd = new(@"^(?<name>.+?)(?:\s{2,}|\s+[.·•-]{2,}\s*)(?<price>\$?\d+(?:\.\d{1,2})?|MP)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

    public ParsedMenuPaste Parse(Guid sessionId, Guid venueId, string rawPaste, long revision, IReadOnlyCollection<Item> library)
    {
        ArgumentNullException.ThrowIfNull(rawPaste);
        ArgumentNullException.ThrowIfNull(library);

        var lookup = library.Where(item => item.IsActive).GroupBy(item => NormalizeIdentity(item.Name), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Id).ToArray(), StringComparer.Ordinal);
        var lines = new List<MenuImportSourceLine>();
        var questions = new List<MenuImportReviewQuestion>();
        var itemCount = 0;
        var physicalLines = rawPaste.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');

        for (var index = 0; index < physicalLines.Length; index++)
        {
            var number = index + 1;
            var raw = physicalLines[index];
            var trimmed = raw.Trim();
            if (trimmed.Length == 0)
            {
                lines.Add(Line("blank"));
                continue;
            }

            if (IsHeading(trimmed))
            {
                lines.Add(Line("section", trimmed));
                continue;
            }

            var match = PriceAtEnd.Match(trimmed);
            if (!match.Success)
            {
                var question = Question("unreadable", trimmed, []);
                questions.Add(question);
                lines.Add(Line("unresolved", reason: "item_format_not_recognized"));
                continue;
            }

            var name = match.Groups["name"].Value.Trim().TrimEnd('.', '-', '·', '•').Trim();
            var price = match.Groups["price"].Value;
            itemCount++;
            lookup.TryGetValue(NormalizeIdentity(name), out var candidates);
            if (candidates is { Length: 1 })
            {
                var candidate = candidates[0];
                questions.Add(Question("identity", name,
                [new MenuImportCandidate(candidate.Id, candidate.Name, candidate.Price, "exact_normalized", true)]));
            }
            else if (candidates is { Length: > 1 })
            {
                questions.Add(Question("identity", name, candidates.Select(candidate =>
                    new MenuImportCandidate(candidate.Id, candidate.Name, candidate.Price, "exact_normalized", false)).ToArray()));
            }

            lines.Add(Line("item", name, price));

            MenuImportReviewQuestion Question(string kind, string value, IReadOnlyCollection<MenuImportCandidate> candidatesForQuestion)
            {
                var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{kind}\n{value}\n{string.Join(',', candidatesForQuestion.Select(c => c.ItemId))}"))).ToLowerInvariant();
                return new MenuImportReviewQuestion(sessionId, venueId, $"line-{number}-{kind}", fingerprint, kind, questions.Count, true, revision, [number], candidatesForQuestion, null);
            }

            MenuImportSourceLine Line(string disposition, string? parsedName = null, string? parsedPrice = null, string? reason = null) =>
                new(sessionId, venueId, number, raw, disposition, parsedName, null, parsedPrice, reason, revision);
        }

        return new ParsedMenuPaste(lines, questions, itemCount);
    }

    public static string NormalizeIdentity(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            builder.Append(char.IsWhiteSpace(character) || char.IsPunctuation(character) || char.IsSymbol(character)
                ? ' '
                : char.ToUpperInvariant(character));
        }
        return Spaces.Replace(builder.ToString().Normalize(NormalizationForm.FormC).Trim(), " ");
    }

    private static bool IsHeading(string value) =>
        value.Any(char.IsLetter) && value.Where(char.IsLetter).All(char.IsUpper) && value.Length <= Item.NameMaxLength;
}
