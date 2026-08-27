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
    private static readonly Regex PriceToken = new(@"(?<![\w.])\$?\d+\.\d{2}(?![\w.])|\$\d+", RegexOptions.Compiled);
    private static readonly Regex LabelledNote = new(@"^[^:]{1,40}:\s*\S", RegexOptions.Compiled);
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
        var priceSetPrice = string.Empty;
        var priceSetNote = string.Empty;

        for (var index = 0; index < physicalLines.Length; index++)
        {
            var number = index + 1;
            var subIndex = 0;
            var raw = physicalLines[index];
            var trimmed = raw.Trim();
            var shape = shapes[index];

            if (shape == Shape.Blank)
            {
                lines.Add(Line("blank"));
                openItem = -1;
                // A blank ends a price set. Without this, "Mana-Thai Cuisine" and "All Natural
                // Authentic Thai Cuisine" - the restaurant's name and tagline, sitting under a
                // blank at the foot of the page - were imported as dishes.
                inPriceSet = false;
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
                    openItem = -1;
                    if (isPriceSet)
                    {
                        /*
                         * The set prices everything below it. Leaving those dishes blank - which is
                         * what the first cut did - left a third of a real menu with no price at
                         * all, which is not a menu anybody can put on a screen.
                         *
                         * The dish takes the FIRST price and carries the whole set in its
                         * description, because Vennusign stores one DECIMAL(19,4) per item and
                         * cannot hold three. "$11.95" with "Chicken $11.95, Beef $12.95, Shrimp
                         * $13.95" printed underneath is what the paper menu says. Three separate
                         * items would be truer still and needs one source line to yield several,
                         * which the (SessionId, LineNumber) key does not allow.
                         *
                         * No question: the set is stated on the dish, so there is nothing to ask.
                         */
                        priceSetPrice = PriceAtEnd.Match(SplitOutsideParentheses(trimmed)[0]).Groups["price"].Value;
                        priceSetNote = trimmed;
                        inPriceSet = true;
                        lines.Add(Line("description", parsedDescription: trimmed));
                        continue;
                    }

                    inPriceSet = false;
                    var fragments = SplitOutsideParentheses(trimmed);
                    if (LabelledNote.IsMatch(fragments[0]))
                    {
                        var colon = fragments[0].IndexOf(':', StringComparison.Ordinal);
                        lines.Add(Line("section", fragments[0][..colon].Trim()));
                        fragments[0] = fragments[0][(colon + 1)..].Trim();
                        subIndex++;
                    }
                    foreach (var fragment in fragments)
                    {
                        var bare = StripTrailingParenthetical(fragment);
                        var fragmentMatch = PriceAtEnd.Match(bare);
                        if (!fragmentMatch.Success) continue;
                        EmitItem(fragmentMatch.Groups["name"].Value.Trim(), fragmentMatch.Groups["price"].Value);
                        if (!string.Equals(bare, fragment, StringComparison.Ordinal))
                            lines[openItem] = lines[openItem] with { ParsedDescription = fragment[bare.Length..].Trim() };
                        subIndex++;
                    }
                    openItem = -1;
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

                    /*
                     * Two Title Case lines in a row are not a heading and its first dish, and not
                     * two headings either. On this menu they were the restaurant's name and its
                     * tagline, straddling a page break - "Mana-Thai Cuisine" was imported as a dish
                     * priced $11.95, and "All Natural Authentic Thai Cuisine" became a section.
                     *
                     * A heading is followed by something priced. A dish under a price set is
                     * followed by its description. Neither is followed by another bare Title Case
                     * line, so this stays a question rather than being guessed at.
                     */
                    if (next == Shape.TitleCase)
                    {
                        questions.Add(Question("unreadable", trimmed, []));
                        lines.Add(Line("unresolved", reason: "item_format_not_recognized"));
                        openItem = -1;
                        inPriceSet = false;
                        continue;
                    }

                    if (next is Shape.Priced or Shape.CommaPriced)
                    {
                        lines.Add(Line("section", trimmed));
                        openItem = -1;
                        inPriceSet = false;
                        continue;
                    }

                    if (inPriceSet)
                    {
                        EmitItem(trimmed, priceSetPrice);
                        lines[openItem] = lines[openItem] with { ParsedDescription = priceSetNote };
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

                case Shape.Note:
                    /*
                     * "(Served w. Steamed Jasmine Rice)" appears under five different headings on
                     * one menu and produced a question every time - ten identical questions asking
                     * what a note is. Decision 33's rule for near-misses is the same rule here:
                     * one fact is one question, never thirty. This one is not even a question. It
                     * is a note about the section it sits in, kept and never imported.
                     */
                    if (openItem >= 0)
                    {
                        var noted = lines[openItem];
                        lines[openItem] = noted with { ParsedDescription = string.IsNullOrEmpty(noted.ParsedDescription) ? trimmed : $"{noted.ParsedDescription} {trimmed}" };
                    }
                    lines.Add(Line("description", parsedDescription: trimmed));
                    continue;

                case Shape.Prose:
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
                        /*
                         * A dish under a price set is emitted carrying the set, and its own
                         * description arrives on the next line. The dish's words come first and the
                         * price set follows, because that is the order they are read in - the set
                         * is a footnote about the dish, not its opening sentence.
                         */
                        var joined = string.IsNullOrEmpty(owner.ParsedDescription) ? trimmed
                            : owner.ParsedDescription == priceSetNote ? $"{trimmed} \u00b7 {priceSetNote}"
                            : $"{owner.ParsedDescription} {trimmed}";
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
            var stripped = StripTrailingParenthetical(trimmed);
            var match = PriceAtEnd.Match(stripped);
            var pricedName = match.Groups["name"].Value.Trim().TrimEnd('.', '-', '·', '•').Trim();
            inPriceSet = false;
            EmitItem(pricedName, match.Groups["price"].Value);
            if (!string.Equals(stripped, trimmed, StringComparison.Ordinal))
                lines[openItem] = lines[openItem] with { ParsedDescription = trimmed[stripped.Length..].Trim() };
            continue;

            void EmitItem(string name, string? price)
            {
                itemCount++;
                lookup.TryGetValue(NormalizeIdentity(name), out var candidates);
                if (candidates is { Length: 1 })
                {
                    /*
                     * A dish you already have, at the price you already charge, is not a question.
                     *
                     * Re-importing a menu used to ask about every line on it - forty-four "safe
                     * matches" and forty-eight answers to import a menu the venue had imported an
                     * hour earlier. Accepting them in bulk was one click, but it was still a wall
                     * in front of an operation with nothing to decide in it.
                     *
                     * A18 forbids pre-answering unless a rule can name why. This one can: the name
                     * matched after case, spacing and punctuation, AND the price is the same, and
                     * the answer carries `exact_normalized` as its match rule. The question is
                     * still recorded and still listed under "Review all N pasted lines", so it can
                     * be found and changed - it simply arrives answered.
                     *
                     * A price that differs is the one thing worth stopping for, and still does.
                     */
                    var candidate = candidates[0];
                    var settled = SamePrice(candidate.Price, price);
                    var question = Question("identity", name,
                        [new MenuImportCandidate(candidate.Id, candidate.Name, candidate.Price, "exact_normalized", true)],
                        settled ? new MenuImportAnswer(string.Empty, MenuImportChoices.SameItem, candidate.Id, revision, default, null) : null,
                        required: !settled);
                    questions.Add(question);
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

            MenuImportReviewQuestion Question(string kind, string value, IReadOnlyCollection<MenuImportCandidate> candidatesForQuestion,
                MenuImportAnswer? answer = null, bool required = true)
            {
                var candidateShape = string.Join('\n', candidatesForQuestion.Select(candidate =>
                    $"{candidate.ItemId}|{candidate.DisplayName}|{candidate.DisplayPrice}|{candidate.MatchRule}|{candidate.IsSafe}"));
                var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                    $"{number}\n{kind}\n{value}\n{candidateShape}\n{dependencyStamp}"))).ToLowerInvariant();
                return new MenuImportReviewQuestion(sessionId, venueId, $"line-{number}-{kind}", fingerprint, kind, questions.Count, required, revision, [number], candidatesForQuestion,
                    answer is null ? null : answer with { Fingerprint = fingerprint });
            }

            MenuImportSourceLine Line(string disposition, string? parsedName = null, string? parsedPrice = null, string? reason = null, string? parsedDescription = null) =>
                new(sessionId, venueId, number, subIndex, raw, disposition, parsedName, parsedDescription, parsedPrice, reason, revision);
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

        // "Tea $2.00 *(Green, Jasmine, Black & Red)" is a priced item whose price is not the last
        // thing on the line. The parenthetical is what it comes in, and becomes its description.
        if (PriceAtEnd.IsMatch(StripTrailingParenthetical(trimmed))) return Shape.Priced;
        // "If you have any Food Allergies, Please speak to our staff & let us know!" - a sentence
        // addressed to the reader, at the foot of the page. It carries no price and asks nothing
        // of the operator, so it is kept as a note rather than raised as a question.
        if (trimmed[^1] is '!' or '?' && !PriceToken.IsMatch(trimmed)) return Shape.Note;
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

        /*
         * Three things wear Title Case and are not headings. Found by pasting a whole real menu at
         * the deployed parser rather than by reasoning about it - each one had produced a section.
         *
         *   "Tea $2.00 *(Green, Jasmine, Black & Red)"    a priced item whose price is not last
         *   "Choice of Sauce: Garlic Sauce, Ginger Sauce" a labelled note about the dish above
         *   "& Red Curry Pineapple"                       the wrapped tail of the line before it
         *
         * A heading names a group. It never carries a price, never labels itself with a colon, and
         * never begins mid-sentence.
         */
        if (PriceToken.IsMatch(value)) return false;
        if (LabelledNote.IsMatch(value)) return false;
        if (value[0] is '&' or '+' or '/') return false;
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

    /// <summary>
    /// Two prices a person would call the same.
    ///
    /// Prices are stored exactly as typed (Q115/Q190), so "7", "7.00" and "$7.00" are one price
    /// written three ways and none of them is worth stopping an operator for. A currency symbol,
    /// surrounding space and trailing zeros are the only differences forgiven; anything else - MP
    /// against a number, 7.00 against 7.50 - is a real difference and raises its question.
    /// </summary>
    private static bool SamePrice(string? library, string? pasted)
    {
        var left = Money(library);
        var right = Money(pasted);
        return left is not null && right is not null && left == right;
    }

    private static string? Money(string? value)
    {
        var trimmed = value?.Trim().TrimStart('$', '\u00a3', '\u20ac').Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        return decimal.TryParse(trimmed, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amount)
            ? amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
            : trimmed.ToUpperInvariant();
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
