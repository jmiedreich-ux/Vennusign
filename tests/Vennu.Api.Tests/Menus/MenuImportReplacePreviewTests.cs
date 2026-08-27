using Vennu.Api.Menus;
using Vennu.Core.Models;

namespace Vennu.Api.Tests.Menus;

/// <summary>
/// M6.13 - the replacement, described before it happens.
///
/// The two NULL-price cases below are the reason this is a pure function with its own tests. An
/// adversarial review of the plan found both, and both would have put a wrong number in front of
/// an operator about to overwrite a menu.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenuImportReplacePreviewTests
{
    private static readonly Guid Venue = Guid.NewGuid();
    private static readonly Guid Session = Guid.NewGuid();

    [Fact]
    public void ADishPastedWithoutAPriceDoesNotInventAMoveToBlank()
    {
        /*
         * Library 12.95, no override, pasted with no price.
         *
         * ConfirmReplaceSql writes IIF(Existing=1,Price,NULL) and Price is the PASTED price, so the
         * override goes NULL -> NULL and the effective price stays 12.95. Comparing the pasted
         * price straight against the current one would announce "12.95 -> (blank)" for a change
         * that never happens.
         */
        var item = Guid.NewGuid();
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "item", "Pad Thai", price: null), Answered(1, MenuImportChoices.SameItem, item)),
            [Placement(item, priceOverride: null)],
            [LibraryItem(item, "Pad Thai", "12.95")]);

        Assert.Empty(preview.Repriced);
        Assert.Equal(0, preview.RepricedCount);
    }

    [Fact]
    public void ADishPastedWithoutAPriceFallsBackToTheLibraryRatherThanToNothing()
    {
        /*
         * Library 12.95, override 13.95 from an earlier import, pasted with no price.
         *
         * The override is ERASED, so the price drops to the library's 12.95. This is a real change,
         * and the naive comparison reports it with the wrong destination - "13.95 -> (blank)".
         */
        var item = Guid.NewGuid();
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "item", "Pad Thai", price: null), Answered(1, MenuImportChoices.SameItem, item)),
            [Placement(item, priceOverride: "13.95")],
            [LibraryItem(item, "Pad Thai", "12.95")]);

        var move = Assert.Single(preview.Repriced);
        Assert.Equal("13.95", move.From);
        Assert.Equal("12.95", move.To);
    }

    [Fact]
    public void APriceThatMovesIsNamedWithBothNumbers()
    {
        var item = Guid.NewGuid();
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "item", "Pad Thai", "13.95"), Answered(1, MenuImportChoices.SameItem, item)),
            [Placement(item, priceOverride: "12.95")],
            [LibraryItem(item, "Pad Thai", "11.00")]);

        var move = Assert.Single(preview.Repriced);
        Assert.Equal("Pad Thai", move.Name);
        Assert.Equal("12.95", move.From);
        Assert.Equal("13.95", move.To);
        // Q115/Q190 - exactly as stored. "13.95" never becomes "$13.95" and "MP" stays "MP".
        Assert.DoesNotContain("$", move.To);
    }

    [Fact]
    public void APriceThatDoesNotMoveIsNotMentioned()
    {
        // Re-importing a menu that has not changed must not read as if every dish moved.
        var item = Guid.NewGuid();
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "item", "Pad Thai", "12.95"), Answered(1, MenuImportChoices.SameItem, item)),
            [Placement(item, priceOverride: "12.95")],
            [LibraryItem(item, "Pad Thai", "12.95")]);

        Assert.Empty(preview.Repriced);
        Assert.Empty(preview.Arriving);
        Assert.Empty(preview.Leaving);
    }

    [Fact]
    public void ADishOnTheMenuAndNotInThePasteIsLeaving()
    {
        var staying = Guid.NewGuid();
        var going = Guid.NewGuid();
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "item", "Pad Thai", "12.95"), Answered(1, MenuImportChoices.SameItem, staying)),
            [Placement(staying, "12.95"), Placement(going, "9.00")],
            [LibraryItem(staying, "Pad Thai", "12.95"), LibraryItem(going, "Tom Kha", "9.00")]);

        Assert.Equal(["Tom Kha"], preview.Leaving);
        Assert.Equal(1, preview.LeavingCount);
    }

    [Fact]
    public void ANewItemArrivesEvenWhenItsNameIsAlreadyOnTheMenu()
    {
        /*
         * Matched by ItemId, never by name. Answering "add as a new item" means the operator said
         * this is a different dish, and reading it as a rename would overrule them - and would also
         * hide that the one already there is on its way out.
         */
        var existing = Guid.NewGuid();
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "item", "Pad Thai", "12.95"), Answered(1, MenuImportChoices.NewItem, null)),
            [Placement(existing, "12.95")],
            [LibraryItem(existing, "Pad Thai", "12.95")]);

        Assert.Equal(["Pad Thai"], preview.Arriving);
        Assert.Equal(["Pad Thai"], preview.Leaving);
    }

    [Fact]
    public void AnUnresolvedLineCountsOnlyWhenItWasAnsweredKeepIt()
    {
        // 'fallback' is "keep it in the import". Any other answer - leave_out especially - places
        // nothing, and a preview that counted it would promise a dish the confirm will not create.
        var kept = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "unresolved", raw: "Chef's pick of the day"), Answered(1, MenuImportChoices.Fallback, null)), [], []);
        Assert.Equal(["Chef's pick of the day"], kept.Arriving);

        var dropped = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "unresolved", raw: "Chef's pick of the day"), Answered(1, MenuImportChoices.LeaveOut, null)), [], []);
        Assert.Empty(dropped.Arriving);

        var unanswered = MenuImportReplacePreviewBuilder.Build(
            Aggregate(Line(1, "unresolved", raw: "Chef's pick of the day")), [], []);
        Assert.Empty(unanswered.Arriving);
    }

    [Fact]
    public void SectionsAndBlanksAreNotDishes()
    {
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate([Line(1, "section", "MAINS"), Line(2, "blank", raw: ""), Line(3, "description", raw: "Rice noodles")], []),
            [], []);

        Assert.Empty(preview.Arriving);
        Assert.Equal(0, preview.ArrivingCount);
    }

    [Fact]
    public void TheAnswerOnALineHoldingSeveralItemsReachesOnlyTheFirst()
    {
        /*
         * M6.9 let one pasted line hold several items, and the SQL joins answers at
         * LineSubIndex = 0. So a "same item" answer identifies the FIRST item on that line; the
         * ones after it are new. Reading the answer as covering the whole line would report a dish
         * as staying when the replacement is about to create a second copy of it.
         */
        var item = Guid.NewGuid();
        var preview = MenuImportReplacePreviewBuilder.Build(
            Aggregate(
                [Line(1, "item", "Jasmine Rice", "2.00", subIndex: 0), Line(1, "item", "Brown Rice", "3.00", subIndex: 1)],
                [Answer(1, MenuImportChoices.SameItem, item)]),
            [Placement(item, "2.00")],
            [LibraryItem(item, "Jasmine Rice", "2.00")]);

        Assert.Equal(["Brown Rice"], preview.Arriving);
        Assert.Empty(preview.Leaving);
    }

    [Fact]
    public void LongListsAreCappedAndTheTotalIsStillTrue()
    {
        // Decision 12 - counts summarize, names are the exception. A 60-dish list is neither.
        var lines = Enumerable.Range(1, 20).Select(number => Line(number, "item", $"Dish {number}", "1.00")).ToArray();
        var preview = MenuImportReplacePreviewBuilder.Build(Aggregate(lines, []), [], []);

        Assert.Equal(20, preview.ArrivingCount);
        Assert.Equal(6, preview.Arriving.Count);
    }

    // ---- fixtures ------------------------------------------------------------

    private static MenuImportSourceLine Line(int number, string disposition, string? name = null, string? price = null,
        string? raw = null, int subIndex = 0) =>
        new(Session, Venue, number, subIndex, raw ?? name ?? string.Empty, disposition, name, null, price, null, 1);

    private static MenuImportReviewQuestion Answer(int line, string choice, Guid? selected) =>
        new(Session, Venue, $"line-{line}-0-identity", new string('a', 64), "identity", 0, true, 1, [line], [],
            new MenuImportAnswer(new string('a', 64), choice, selected, 1, default, null));

    private static MenuImportReviewQuestion[] Answered(int line, string choice, Guid? selected) => [Answer(line, choice, selected)];

    private static MenuImportAggregate Aggregate(MenuImportSourceLine line, MenuImportReviewQuestion[] questions) =>
        Aggregate([line], questions);

    private static MenuImportAggregate Aggregate(MenuImportSourceLine line) => Aggregate([line], []);

    private static MenuImportAggregate Aggregate(MenuImportSourceLine[] lines, MenuImportReviewQuestion[] questions) =>
        new(new MenuImportSession(Session, Venue, "", 1, MenuImportStatuses.Resolved, lines.Length, 0,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow, DateTime.UtcNow, null, []), lines, questions);

    private static Placement Placement(Guid itemId, string? priceOverride) =>
        new() { Id = Guid.NewGuid(), VenueId = Venue, ItemId = itemId, ImportedPriceOverride = priceOverride };

    private static Item LibraryItem(Guid id, string name, string? price) =>
        new() { Id = id, VenueId = Venue, Name = name, Price = price };
}
