using Vennu.Api.Menus;

namespace Vennu.Api.Tests.Menus;

/// <summary>
/// The whole of one real four-page restaurant menu, asserted against what a person reading it
/// gets - not against what the previous parser got.
///
/// This exists because the M6.7 round was reported as a success on the strength of "91 questions
/// became 15". That is a ratio between two wrong answers. The menu it produced had seventeen
/// dishes with no price at all, the restaurant's name imported as a $11.95 dish, and its tagline
/// as a section. Measuring against the previous parser hid every one of those.
///
/// The fixture is the owner's own paste, out of the restaurant's own PDF. The numbers below are
/// countable by eye off the printed menu, which is the point: a person can check this test.
/// </summary>
[Trait("Category", "Unit")]
public sealed class RealPrintedMenuTests
{
    private static readonly string Menu = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Menus", "real-printed-menu.txt"));
    private readonly MenuPasteParser parser = new();

    private ParsedMenuPaste Parsed() => parser.Parse(Guid.NewGuid(), Guid.NewGuid(), Menu, 1, []);

    [Fact]
    public void EverySectionOnThePage_AndNothingElse()
    {
        Assert.Equal(
            ["Appetizers", "Salads", "Soups", "Noodle Soups", "Noodles", "Rice",
             "Traditional Thai Curry", "Basic Exotic Dishes", "Mana\u2019s Special Thai Dishes".Replace("\u2019", "'"),
             "Fish", "Vegetarian", "Sides", "Beverages", "Desserts"],
            Parsed().Lines.Where(line => line.Disposition == "section").Select(line => line.ParsedName));
    }

    [Fact]
    public void EveryDishHasAPrice()
    {
        // Seventeen did not, after the first cut. A dish with no price cannot go on a screen, so
        // "we left it blank rather than guess" is not a safer answer - it is an unusable one.
        var items = Parsed().Lines.Where(line => line.Disposition == "item").ToArray();

        Assert.Equal(60, items.Length);
        Assert.DoesNotContain(items, item => string.IsNullOrEmpty(item.ParsedPrice));
    }

    [Fact]
    public void ADishUnderAPriceSetTakesThatPrice_AndCarriesTheWholeSet()
    {
        // Vennusign stores one DECIMAL(19,4) per item, so the three prices cannot all be the
        // price. The dish takes the first and prints the set underneath, which is what the paper
        // menu says.
        var padThai = Parsed().Lines.First(line => line.ParsedName == "Pad Thai");

        Assert.Equal("$11.95", padThai.ParsedPrice);
        Assert.Equal("Rice Noodles sauteed w. egg, peanuts, bean sprouts & scallions \u00b7 Chicken $11.95, Beef $12.95, Shrimp $13.95", padThai.ParsedDescription);
    }

    [Fact]
    public void TheRestaurantsOwnNameIsNotADish()
    {
        // "Mana-Thai Cuisine" was imported as a dish priced $11.95, and "All Natural Authentic
        // Thai Cuisine" as a section. Both sit at a page break inside a price set.
        var result = Parsed();

        Assert.DoesNotContain(result.Lines, line => line.Disposition is "item" or "section"
            && (line.ParsedName == "Mana-Thai Cuisine" || line.ParsedName == "All Natural Authentic Thai Cuisine"));
    }

    [Fact]
    public void APricedItemIsReadWhereverItsParentheticalSits()
    {
        // "Tea $2.00 *(Green, Jasmine, Black & Red)" - the price is not the last thing on the line.
        var tea = Parsed().Lines.First(line => line.ParsedName == "Tea");

        Assert.Equal("$2.00", tea.ParsedPrice);
        Assert.Equal("*(Green, Jasmine, Black & Red)", tea.ParsedDescription);
    }

    [Fact]
    public void ARepeatedNoteIsNeverARepeatedQuestion()
    {
        // "(Served w. Steamed Jasmine Rice)" sits under five headings and asked five times; the
        // owner counted ten identical questions on the full menu. Decision 33's rule holds here:
        // one fact is one question, and this one is not a question at all.
        var result = Parsed();

        Assert.DoesNotContain(result.Lines, line => line.Disposition == "unresolved"
            && line.RawText.Contains("Served w. Steamed Jasmine Rice", StringComparison.Ordinal));
    }

    [Fact]
    public void WhatIsLeftToAsk_AndWhy()
    {
        // Five questions on a 132-line menu, and each one is named. Three are the same unsolved
        // thing: a source line is one row keyed (SessionId, LineNumber), so a line holding five
        // items can only be one of them. That is a schema decision, not a parser rule.
        // Two, on a 133-line menu, and both are right to ask: the restaurant's own name and its
        // tagline, off the top of a page. They are not menu content, and guessing at them is what
        // put them in the menu the first time.
        var unresolved = Parsed().Lines.Where(line => line.Disposition == "unresolved").ToArray();

        Assert.Equal(2, unresolved.Length);
        Assert.DoesNotContain(unresolved, line => line.ParserReason == "multiple_items_on_one_line");
        Assert.Equal(["Mana-Thai Cuisine", "All Natural Authentic Thai Cuisine"], unresolved.Select(line => line.RawText.Trim()));
    }

    [Fact]
    public void NoLineIsEverDropped()
    {
        // Q81's invariant, stated for a world where one line can hold several items: every line of
        // the paste is still accounted for, and its number still points at it.
        var result = Parsed();
        var physical = Menu.Replace("\r\n", "\n").Split('\n').Length;

        Assert.Equal(physical, result.Lines.Select(line => line.LineNumber).Distinct().Count());
        Assert.Equal(Enumerable.Range(1, physical), result.Lines.Select(line => line.LineNumber).Distinct().Order());
    }

    [Fact]
    public void OnePastedLineCanHoldSeveralItems()
    {
        // "Sides: Steamed Jasmine Rice $2.00, Brown Rice $3.00, ..." is five items and a heading on
        // one physical line. They share that line's number and are ordered within it, so "line 128"
        // still means line 128 of what was pasted.
        var sides = Parsed().Lines.Where(line => line.ParsedName is "Steamed Jasmine Rice" or "Brown Rice" or "Peanut Sauce").ToArray();

        Assert.Equal(3, sides.Length);
        Assert.Single(sides.Select(line => line.LineNumber).Distinct());
        Assert.Equal(sides.Select(line => line.LineSubIndex).Order(), sides.Select(line => line.LineSubIndex));
    }
}
