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

    public ParsedMenuPaste Parse(Guid sessionId, Guid venueId, string rawPaste, long revision, IReadOnlyCollection<Item> library,
        IReadOnlySet<int>? sectionOverrides = null)
    {
        ArgumentNullException.ThrowIfNull(rawPaste);
        ArgumentNullException.ThrowIfNull(library);

        var normalizedLibrary = library.Where(item => item.IsActive).Select(item => new { Item = item, Identity = NormalizeIdentity(item.Name) }).ToArray();
        var lookup = normalizedLibrary.GroupBy(entry => entry.Identity, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Item).OrderBy(item => item.Id).ToArray(), StringComparer.Ordinal);
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

            var match = PriceAtEnd.Match(trimmed);
            if (!match.Success && (IsHeading(trimmed) || sectionOverrides?.Contains(number) == true))
            {
                lines.Add(Line("section", trimmed));
                continue;
            }

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
            else
            {
                var pastedIdentity = NormalizeIdentity(name);
                var nearMatches = normalizedLibrary
                    .Select(entry => new { entry.Item, Distance = EditDistance(pastedIdentity, entry.Identity) })
                    .Where(match => match.Distance <= NearMatchLimit(pastedIdentity.Length))
                    .OrderBy(match => match.Distance).ThenBy(match => match.Item.Name).ThenBy(match => match.Item.Id)
                    .Take(3)
                    .Select(match => new MenuImportCandidate(match.Item.Id, match.Item.Name, match.Item.Price, "semantic", false))
                    .ToArray();
                if (nearMatches.Length > 0) questions.Add(Question("identity", name, nearMatches));
            }

            lines.Add(Line("item", name, price));

            MenuImportReviewQuestion Question(string kind, string value, IReadOnlyCollection<MenuImportCandidate> candidatesForQuestion)
            {
                var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{number}\n{kind}\n{value}\n{string.Join(',', candidatesForQuestion.Select(c => c.ItemId))}"))).ToLowerInvariant();
                return new MenuImportReviewQuestion(sessionId, venueId, $"line-{number}-{kind}", fingerprint, kind, questions.Count, true, revision, [number], candidatesForQuestion, null);
            }

            MenuImportSourceLine Line(string disposition, string? parsedName = null, string? parsedPrice = null, string? reason = null) =>
                new(sessionId, venueId, number, raw, disposition, parsedName, null, parsedPrice, reason, revision);
        }

        return new ParsedMenuPaste(lines, questions, itemCount);
    }

    public static string NormalizeIdentity(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Normalize(NormalizationForm.FormC))
        {
            // Only ornamental punctuation is safe to discard. Symbols and
            // meaningful connectors remain identity-bearing.
            builder.Append(char.IsWhiteSpace(character) || character is '.' or ',' or '!' or '?' or ':' or ';' or '-' or '–' or '—' or '·' or '•'
                ? ' '
                : char.ToUpperInvariant(character));
        }
        return Spaces.Replace(builder.ToString().Normalize(NormalizationForm.FormC).Trim(), " ");
    }

    private static bool IsHeading(string value) =>
        value.Any(char.IsLetter) && value.Where(char.IsLetter).All(char.IsUpper) && value.Length <= Item.NameMaxLength;

    private static int NearMatchLimit(int length) => Math.Clamp((int)Math.Ceiling(length * .2), 1, 3);

    private static int EditDistance(string left, string right)
    {
        if (left.Length == 0) return right.Length;
        if (right.Length == 0) return left.Length;
        var previous = Enumerable.Range(0, right.Length + 1).ToArray();
        var current = new int[right.Length + 1];
        for (var i = 1; i <= left.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= right.Length; j++)
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1));
            (previous, current) = (current, previous);
        }
        return previous[right.Length];
    }
}
