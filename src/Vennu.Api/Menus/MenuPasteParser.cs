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
    /// <summary>
    /// An item line: a name, whitespace, then a price at the end.
    ///
    /// The separator is <c>\s+</c> — one space is enough, and a tab counts. It previously demanded
    /// two or more spaces or a dot leader, which silently rejected the most ordinary line a menu
    /// has ("Garlic Bread 6.50") and every tab-separated line, so a paste out of a spreadsheet —
    /// a route the product advertises — imported as zero items. That contradicted the design
    /// authority's own promise for this screen: "no syntax to learn".
    ///
    /// The number format is unchanged. Whole numbers, a currency symbol and MP already parsed;
    /// they only ever failed for want of a second space.
    ///
    /// One consequence, accepted deliberately: a capitals-only heading ending in a bare number
    /// ("SPECIALS 2") now reads as an item priced at 2. That is the same trade this parser already
    /// made — a priced uppercase line is an item, asserted by
    /// <c>Parse_PricedUppercaseLineIsAnItemNotAHeading</c> — and "BLT 12" cannot be told from
    /// "SPECIALS 2" by shape alone. Review can promote any line to a section, so it is recoverable.
    /// </summary>
    private static readonly Regex PriceAtEnd = new(@"^(?<name>.+?)(?:\s+[.·•-]{2,}\s*|\s+)(?<price>\$?\d+(?:\.\d{1,2})?|MP)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TrailingParenthetical = new(@"\s*\*?\([^()]*\)\s*$", RegexOptions.Compiled);
    private const int DescriptionMaxLength = 1000;
    private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// What a line looks like on its own, before context is applied.
    ///
    /// Shape is deliberately separate from disposition. A line's meaning depends on its
    /// neighbours - "Pad Thai" is a dish under a price set and a heading anywhere else - so the
    /// parser reads every line's shape first and only then walks the document deciding what each
    /// one *is*. The single pass this replaced could not tell those apart, because it never looked
    /// at line n+1.
    /// </summary>
    private enum Shape { Blank, Priced, CommaPriced, CapsHeading, TitleCase, Prose, Note }

    public ParsedMenuPaste Parse(Guid sessionId, Guid venueId, string rawPaste, long revision, IReadOnlyCollection<Item> library,
        IReadOnlySet<int>? sectionOverrides = null, string dependencyStamp = "")
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
        var shapes = physicalLines.Select(line => ShapeOf(line.Trim())).ToArray();

        // The index in `lines` of the item a description would belong to, or -1 when the last
        // thing read cannot own one. Descriptions wrap across physical lines on a real menu, so
        // this survives until something that is not prose ends the run.
        var openItem = -1;
        var inPriceSet = false;

        for (var index = 0; index < physicalLines.Length; index++)
        {
            var number = index + 1;
            var raw = physicalLines[index];
            var trimmed = raw.Trim();
            var shape = shapes[index];

            if (shape == Shape.Blank)
            {
                lines.Add(Line("blank"));
                openItem = -1;
                continue;
            }

            // A line the operator promoted at review is a section whatever it looks like.
            if (sectionOverrides?.Contains(number) == true && shape != Shape.Priced)
            {
                lines.Add(Line("section", trimmed));
                openItem = -1;
                inPriceSet = false;
                continue;
            }

            switch (shape)
            {
                case Shape.CapsHeading:
                    lines.Add(Line("section", trimmed));
                    openItem = -1;
                    inPriceSet = false;
                    continue;

                case Shape.CommaPriced:
                {
                    /*
                     * Two different things wear the same clothes.
                     *
                     * "Chicken $11.95, Beef $12.95, Shrimp $13.95" sitting above a run of unpriced
                     * dish names is a *price set*: it prices everything below it, per protein. The
                     * same shape not followed by an unpriced dish name is several items crammed on
                     * one line ("Sides: Jasmine Rice $2.00, Brown Rice $3.00").
                     *
                     * Neither becomes an item. Before this, the price set parsed as an item named
                     * "Chicken $11.95, Beef $12.95, Shrimp" priced $13.95 - a plausible-looking row
                     * of nonsense, which is worse than a question. Both now raise exactly one
                     * question, with a reason that says which of the two it is.
                     *
                     * Splitting a multi-item line into several items is not possible here: a source
                     * line is one row, keyed (SessionId, LineNumber). That is a schema change and
                     * its own milestone.
                     */
                    var isPriceSet = NextMeaningful(shapes, index) == Shape.TitleCase;
                    inPriceSet = isPriceSet;
                    openItem = -1;
                    questions.Add(Question("unreadable", trimmed, []));
                    lines.Add(Line("unresolved", reason: isPriceSet ? "price_set_needs_choosing" : "multiple_items_on_one_line"));
                    continue;
                }

                case Shape.TitleCase:
                {
                    /*
                     * Title Case is what separates a heading from a description, and it is the rule
                     * that was missing. "Appetizers", "Salads", "Noodle Soups" capitalise every
                     * word; "Steamed healthy soybeans" capitalises only the first. The parser used
                     * to require a heading to be ALL CAPS, so an ordinary printed menu produced
                     * zero sections and every heading became a question.
                     *
                     * Inside a price set the same shape is a dish whose price lives on the header
                     * above it, so it is an item with no price - which A11 already allows.
                     */
                    var next = NextMeaningful(shapes, index);
                    if (next is Shape.Priced or Shape.CommaPriced)
                    {
                        lines.Add(Line("section", trimmed));
                        openItem = -1;
                        inPriceSet = false;
                        continue;
                    }

                    if (inPriceSet)
                    {
                        EmitItem(trimmed, null);
                        continue;
                    }

                    /*
                     * A heading needs something to hold. A Title Case line with nothing after it is
                     * as likely to be a stray line off the bottom of a PDF - a restaurant name, a
                     * tagline - as a section, so it stays a question rather than becoming an empty
                     * section nobody asked for. Q81's invariant: never silently drop a line.
                     */
                    if (next == Shape.Blank)
                    {
                        questions.Add(Question("unreadable", trimmed, []));
                        lines.Add(Line("unresolved", reason: "item_format_not_recognized"));
                        openItem = -1;
                        continue;
                    }

                    lines.Add(Line("section", trimmed));
                    openItem = -1;
                    continue;
                }

                case Shape.Prose:
                case Shape.Note:
                {
                    /*
                     * Q81: an unpriced, non-heading line under an item is that item's description.
                     * Specified in 2026-08-07 and never implemented - so on a real menu every
                     * description line came back `item_format_not_recognized`, which is where most
                     * of the ninety-one questions came from.
                     */
                    if (openItem >= 0)
                    {
                        var owner = lines[openItem];
                        var joined = string.IsNullOrEmpty(owner.ParsedDescription) ? trimmed : $"{owner.ParsedDescription} {trimmed}";
                        lines[openItem] = owner with { ParsedDescription = Truncate(joined, DescriptionMaxLength) };
                        lines.Add(Line("description", parsedDescription: trimmed));
                        continue;
                    }

                    questions.Add(Question("unreadable", trimmed, []));
                    lines.Add(Line("unresolved", reason: "item_format_not_recognized"));
                    continue;
                }
            }

            // Shape.Priced - a name and a price on one line, the shape that always worked.
            var match = PriceAtEnd.Match(trimmed);
            var pricedName = match.Groups["name"].Value.Trim().TrimEnd('.', '-', '·', '•').Trim();
            inPriceSet = false;
            EmitItem(pricedName, match.Groups["price"].Value);
            continue;

            void EmitItem(string name, string? price)
            {
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
                        .Where(candidate => candidate.Distance <= NearMatchLimit(pastedIdentity.Length))
                        .OrderBy(candidate => candidate.Distance).ThenBy(candidate => candidate.Item.Name).ThenBy(candidate => candidate.Item.Id)
                        .Take(3)
                        .Select(candidate => new MenuImportCandidate(candidate.Item.Id, candidate.Item.Name, candidate.Item.Price, "semantic", false))
                        .ToArray();
                    if (nearMatches.Length > 0) questions.Add(Question("identity", name, nearMatches));
                }

                openItem = lines.Count;
                lines.Add(Line("item", name, price));
            }

            MenuImportReviewQuestion Question(string kind, string value, IReadOnlyCollection<MenuImportCandidate> candidatesForQuestion)
            {
                var candidateShape = string.Join('\n', candidatesForQuestion.Select(candidate =>
                    $"{candidate.ItemId}|{candidate.DisplayName}|{candidate.DisplayPrice}|{candidate.MatchRule}|{candidate.IsSafe}"));
                var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                    $"{number}\n{kind}\n{value}\n{candidateShape}\n{dependencyStamp}"))).ToLowerInvariant();
                return new MenuImportReviewQuestion(sessionId, venueId, $"line-{number}-{kind}", fingerprint, kind, questions.Count, true, revision, [number], candidatesForQuestion, null);
            }

            MenuImportSourceLine Line(string disposition, string? parsedName = null, string? parsedPrice = null, string? reason = null, string? parsedDescription = null) =>
                new(sessionId, venueId, number, raw, disposition, parsedName, parsedDescription, parsedPrice, reason, revision);
        }

        return new ParsedMenuPaste(lines, questions, itemCount);
    }

    /// <summary>
    /// The next line that decides anything, skipping blanks and parenthesised notes.
    ///
    /// Notes are skipped because they sit between a price set and the dishes it prices -
    /// "(Served w. Steamed Jasmine Rice)" - and a look-ahead that stopped at one would decide the
    /// price set was a list of separate items. Returns <see cref="Shape.Blank"/> at the end of the
    /// paste, which is how "nothing follows this" is told from "something does".
    /// </summary>
    private static Shape NextMeaningful(IReadOnlyList<Shape> shapes, int index)
    {
        for (var next = index + 1; next < shapes.Count; next++)
            if (shapes[next] is not (Shape.Blank or Shape.Note)) return shapes[next];
        return Shape.Blank;
    }

    private static Shape ShapeOf(string trimmed)
    {
        if (trimmed.Length == 0) return Shape.Blank;

        // A parenthesised line is a note about the section it sits in - "(Served w. Steamed
        // Jasmine Rice)" - never a dish, however it is capitalised.
        if (trimmed[0] == '(') return Shape.Note;

        // Tested before Priced: a price set matches PriceAtEnd too, taking everything up to the
        // last price as one very long name.
        if (IsCommaSeparatedPrices(trimmed)) return Shape.CommaPriced;
        if (PriceAtEnd.IsMatch(trimmed)) return Shape.Priced;
        if (IsCapsHeading(trimmed)) return Shape.CapsHeading;
        if (IsTitleCase(trimmed)) return Shape.TitleCase;
        return Shape.Prose;
    }

    /// <summary>Two or more comma-separated fragments, every one of them a name and a price.</summary>
    private static bool IsCommaSeparatedPrices(string value)
    {
        var fragments = SplitOutsideParentheses(value);
        return fragments.Count >= 2 && fragments.All(fragment => PriceAtEnd.IsMatch(StripTrailingParenthetical(fragment)));
    }

    private static List<string> SplitOutsideParentheses(string value)
    {
        var fragments = new List<string>();
        var depth = 0;
        var start = 0;
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] == '(') depth++;
            else if (value[index] == ')') depth = Math.Max(0, depth - 1);
            else if (value[index] == ',' && depth == 0)
            {
                fragments.Add(value[start..index].Trim());
                start = index + 1;
            }
        }
        fragments.Add(value[start..].Trim());
        return fragments.Where(fragment => fragment.Length > 0).ToList();
    }

    private static string StripTrailingParenthetical(string value) => TrailingParenthetical.Replace(value, string.Empty).Trim();

    /// <summary>
    /// Every word worth counting starts with a capital.
    ///
    /// This is the whole difference between a heading and a description on a printed menu, and it
    /// holds without measuring length or counting commas: "Noodle Soups" against "Steamed healthy
    /// soybeans". Words of one or two letters are skipped - "&amp;", "w.", "of", "in" - because
    /// title case does not capitalise them and a rule that demanded it would fail on the first
    /// heading with a joining word in it.
    /// </summary>
    private static bool IsTitleCase(string value)
    {
        if (value.Length > Item.NameMaxLength) return false;
        var significant = 0;
        foreach (var word in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var letters = word.Where(char.IsLetter).ToArray();
            if (letters.Length < 3) continue;
            significant++;
            if (!char.IsUpper(letters[0])) return false;
        }
        return significant > 0;
    }

    private static string Truncate(string value, int limit) => value.Length <= limit ? value : value[..limit];

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

    /// <summary>
    /// A heading the parser recognises on its own, with no operator override.
    ///
    /// Public because <c>MenuImportService</c> used to carry a byte-identical private copy to work
    /// out which sections were the operator's doing. Two copies of a rule is how a rule drifts -
    /// and this one was about to, because Title Case headings are natural now and the copy did not
    /// know it. One definition, called from both places.
    /// </summary>
    public static bool IsNaturalHeading(string value)
    {
        var trimmed = value.Trim();
        return IsCapsHeading(trimmed) || IsTitleCase(trimmed);
    }

    private static bool IsCapsHeading(string value) =>
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
