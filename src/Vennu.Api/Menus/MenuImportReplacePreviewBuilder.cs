using Vennu.Core.Models;

namespace Vennu.Api.Menus;

/// <summary>
/// What replacing a menu would actually do, worked out before it happens (M6.13).
///
/// The confirm screen used to show three facts, and the only one about change - "12 unpublished
/// changes already present" - describes the TARGET menu's own draft, which the replacement
/// discards. Nothing on the screen said what the replacement itself does. The operator was about
/// to overwrite a menu and was never shown one dish that arrives, goes, or moves price.
///
/// This is a pure function on purpose. Every part of it that could be wrong is arithmetic over
/// data already in hand, and arithmetic can be tested without a database.
/// </summary>
public static class MenuImportReplacePreviewBuilder
{
    /// <summary>How many names are listed before the rest become "and N more".</summary>
    private const int Shown = 6;

    /// <summary>
    /// <paramref name="placements"/> and <paramref name="library"/> describe the menu as it stands.
    /// The preview matches by ItemId and never by name: a line answered "add as a new item" carries
    /// a fresh id and therefore arrives, even where its name matches something already on the menu.
    /// That is honest - the operator said it was a different dish.
    /// </summary>
    public static MenuImportReplacePreview Build(
        MenuImportAggregate aggregate,
        IReadOnlyCollection<Placement> placements,
        IReadOnlyCollection<Item> library)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(library);

        var libraryPrice = library
            .GroupBy(item => item.Id)
            .ToDictionary(group => group.Key, group => group.First().Price);
        var libraryName = library
            .GroupBy(item => item.Id)
            .ToDictionary(group => group.Key, group => group.First().Name);

        var rows = Rows(aggregate).ToArray();
        var incoming = rows.Where(row => row.ItemId is not null).ToDictionary(row => row.ItemId!.Value, row => row);

        var arriving = new List<string>();
        var repriced = new List<MenuImportPriceMove>();

        foreach (var row in rows)
        {
            // No item id means the operator answered "new item", so nothing on the menu can match
            // it and it always arrives.
            if (row.ItemId is null || !placements.Any(placement => placement.ItemId == row.ItemId))
            {
                arriving.Add(row.Name);
                continue;
            }

            var placement = placements.First(placement => placement.ItemId == row.ItemId);
            var fallback = libraryPrice.GetValueOrDefault(row.ItemId!.Value);

            /*
             * The rule ConfirmReplaceSql actually follows, which is not the obvious one.
             *
             * It writes IIF(Existing=1,Price,NULL) as the override, and Price is the PASTED price -
             * null for a dish pasted without one. Effective price is COALESCE(override, library),
             * so a pasted line with no price does not blank the dish: it ERASES any override and
             * the price falls back to the library's.
             *
             * Comparing the pasted price directly against the current one gets this wrong twice:
             * it invents a move to blank that never happens, and where an override existed it
             * reports the right change with the wrong destination number.
             */
            var before = placement.ImportedPriceOverride ?? fallback;
            var after = row.Price ?? fallback;
            if (!Same(before, after)) repriced.Add(new MenuImportPriceMove(row.Name, before, after));
        }

        var leaving = placements
            .Where(placement => !incoming.ContainsKey(placement.ItemId))
            .Select(placement => libraryName.GetValueOrDefault(placement.ItemId) ?? "An item")
            .ToArray();

        return new MenuImportReplacePreview(
            arriving.Count,
            leaving.Length,
            repriced.Count,
            arriving.Take(Shown).ToArray(),
            leaving.Take(Shown).ToArray(),
            repriced.Take(Shown).ToArray());
    }

    /// <summary>
    /// The rows the replacement will place, resolved exactly as <c>ConfirmReplaceSql</c> resolves
    /// them: a line dispositioned <c>item</c>, or an <c>unresolved</c> line answered
    /// <c>fallback</c>. An answer carrying a SelectedItemId makes the row an existing library item.
    ///
    /// Answers reach sub-index 0 only, matching the SQL's
    /// <c>ql.LineNumber=l.LineNumber AND l.LineSubIndex=0</c>. A line holding several items carries
    /// one answer for the line, and the later items on it are new.
    /// </summary>
    private static IEnumerable<Row> Rows(MenuImportAggregate aggregate)
    {
        var answered = aggregate.Questions
            .Where(question => question.Answer is not null)
            .SelectMany(question => question.LineNumbers.Select(line => (Line: line, question.Answer)))
            .GroupBy(pair => pair.Line)
            .ToDictionary(group => group.Key, group => group.First().Answer!);

        foreach (var line in aggregate.Lines.OrderBy(line => line.LineNumber).ThenBy(line => line.LineSubIndex))
        {
            var answer = line.LineSubIndex == 0 && answered.TryGetValue(line.LineNumber, out var found) ? found : null;

            var placed = line.Disposition == "item"
                || (line.Disposition == "unresolved" && answer?.Choice == MenuImportChoices.Fallback);
            if (!placed) continue;

            var name = line.Disposition == "item" ? line.ParsedName : line.RawText.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            yield return new Row(
                answer?.Choice == MenuImportChoices.SameItem ? answer.SelectedItemId : null,
                name,
                line.ParsedPrice);
        }
    }

    /// <summary>Null and empty are the same absence, the way they are everywhere else on this route.</summary>
    private static bool Same(string? left, string? right) => (left ?? string.Empty) == (right ?? string.Empty);

    private readonly record struct Row(Guid? ItemId, string Name, string? Price);
}
