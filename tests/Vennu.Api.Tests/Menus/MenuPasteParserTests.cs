using Vennu.Api.Menus;
using Vennu.Core.Models;

namespace Vennu.Api.Tests.Menus;

[Trait("Category", "Unit")]
public sealed class MenuPasteParserTests
{
    private readonly MenuPasteParser parser = new();

    [Fact]
    public void Parse_RetainsEveryPhysicalLineAndRecognizesHeadingAndItem()
    {
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), "STARTERS\nBurger  12\n\nUnreadable", 1, []);

        Assert.Equal(4, result.Lines.Count);
        Assert.Equal(["section", "item", "blank", "unresolved"], result.Lines.Select(line => line.Disposition));
        Assert.Equal(1, result.ItemCount);
        Assert.Single(result.Questions, question => question.Kind == "unreadable");
    }

    [Theory]
    [InlineData("MAC-CHEESE!", "mac cheese")]
    [InlineData("  Fish   Chips ", "fish chips")]
    public void NormalizeIdentity_OnlyNormalizesSafeCasePunctuationAndSpacing(string left, string right) =>
        Assert.Equal(MenuPasteParser.NormalizeIdentity(left), MenuPasteParser.NormalizeIdentity(right));

    [Theory]
    [InlineData("Crème", "Creme")]
    [InlineData("Fish & Chips", "Fish Chips")]
    [InlineData("Burger + fries", "Burger fries")]
    public void NormalizeIdentity_PreservesSemanticMarksAndSymbols(string left, string right) =>
        Assert.NotEqual(MenuPasteParser.NormalizeIdentity(left), MenuPasteParser.NormalizeIdentity(right));

    [Fact]
    public void Parse_PricedUppercaseLineIsAnItemNotAHeading()
    {
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), "SANDWICHES\nBLT  12", 1, []);

        Assert.Equal(["section", "item"], result.Lines.Select(line => line.Disposition));
        Assert.Equal("BLT", result.Lines.Last().ParsedName);
        Assert.Equal(1, result.ItemCount);
    }

    [Fact]
    public void Parse_UniqueExactNormalizedCandidateIsSafe()
    {
        var venueId = Guid.NewGuid();
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = "Fish-Chips", Price = "14", IsActive = true };

        var result = parser.Parse(Guid.NewGuid(), venueId, "fish chips  16", 1, [item]);

        var candidate = Assert.Single(Assert.Single(result.Questions).Candidates);
        Assert.True(candidate.IsSafe);
        Assert.Equal("exact_normalized", candidate.MatchRule);
    }

    [Fact]
    public void Parse_DuplicateExactNormalizedCandidatesAreNeverSafe()
    {
        var venueId = Guid.NewGuid();
        var items = new[]
        {
            new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = "Burger", IsActive = true },
            new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = "BURGER!", IsActive = true }
        };

        var result = parser.Parse(Guid.NewGuid(), venueId, "Burger  12", 1, items);

        Assert.Equal(2, Assert.Single(result.Questions).Candidates.Count);
        Assert.All(Assert.Single(result.Questions).Candidates, candidate => Assert.False(candidate.IsSafe));
    }

    [Fact]
    public void Parse_SemanticNearMissIsSuggestedButNeverSafeOrPreanswered()
    {
        var venueId = Guid.NewGuid();
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = "Cheeseburger", Price = "14", IsActive = true };

        var result = parser.Parse(Guid.NewGuid(), venueId, "Cheeseburgr  16", 1, [item]);

        var question = Assert.Single(result.Questions);
        var candidate = Assert.Single(question.Candidates);
        Assert.Equal("semantic", candidate.MatchRule);
        Assert.False(candidate.IsSafe);
        Assert.Null(question.Answer);
    }

    [Fact]
    public void Parse_DistantLibraryItemDoesNotCreateAFalseIdentityQuestion()
    {
        var venueId = Guid.NewGuid();
        var item = new Item { Id = Guid.NewGuid(), VenueId = venueId, Name = "Cheeseburger", IsActive = true };

        var result = parser.Parse(Guid.NewGuid(), venueId, "House salad  11", 1, [item]);

        Assert.Empty(result.Questions);
    }

    [Fact]
    public void Parse_Candidate_display_change_invalidates_the_question_fingerprint()
    {
        var venueId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var original = new Item { Id = itemId, VenueId = venueId, Name = "Burger", Price = "12", IsActive = true };
        var changed = new Item { Id = itemId, VenueId = venueId, Name = "Burger", Price = "13", IsActive = true };

        var first = parser.Parse(Guid.NewGuid(), venueId, "Burger  14", 1, [original]);
        var second = parser.Parse(Guid.NewGuid(), venueId, "Burger  14", 2, [changed]);

        Assert.NotEqual(Assert.Single(first.Questions).Fingerprint, Assert.Single(second.Questions).Fingerprint);
    }

    /// <summary>
    /// The separator between a name and its price is one-or-more whitespace, and a tab counts.
    ///
    /// It used to demand two spaces or a dot leader, which rejected the most ordinary line a menu
    /// has and every tab-separated line — so pasting a real menu, or anything out of a
    /// spreadsheet, produced zero items and a question on every line. Every existing test in this
    /// file wrote "Burger  12" with two spaces, so the suite passed while encoding the defect.
    /// </summary>
    [Theory]
    [InlineData("Garlic Bread 6.50", "Garlic Bread", "6.50")]
    [InlineData("Garlic Bread\t6.50", "Garlic Bread", "6.50")]
    [InlineData("Soup of the Day 7.00", "Soup of the Day", "7.00")]
    [InlineData("Ribeye 32", "Ribeye", "32")]
    [InlineData("Ribeye $32.00", "Ribeye", "$32.00")]
    [InlineData("Soup MP", "Soup", "MP")]
    [InlineData("Garlic Bread  6.50", "Garlic Bread", "6.50")]
    [InlineData("Garlic Bread .... 6.50", "Garlic Bread", "6.50")]
    public void Parse_ReadsAnItemWhateverSeparatesTheNameFromThePrice(string line, string name, string price)
    {
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), line, 1, []);

        var parsed = Assert.Single(result.Lines);
        Assert.Equal("item", parsed.Disposition);
        Assert.Equal(name, parsed.ParsedName);
        Assert.Equal(price, parsed.ParsedPrice);
        Assert.Equal(1, result.ItemCount);
    }

    [Fact]
    public void Parse_ReadsAnOrdinaryPastedMenu()
    {
        const string menu = "STARTERS\nGarlic Bread 6.50\nSoup of the Day 7.00\n\nMAINS\nRibeye 32.00\nSalmon 26.50";

        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), menu, 1, []);

        Assert.Equal(4, result.ItemCount);
        Assert.Equal(2, result.Lines.Count(line => line.Disposition == "section"));
        Assert.DoesNotContain(result.Lines, line => line.Disposition == "unresolved");
        Assert.DoesNotContain(result.Questions, question => question.Kind == "unreadable");
    }

    /// <summary>
    /// The cost of accepting a single space, recorded rather than hidden: a capitals-only heading
    /// ending in a bare number now reads as an item. "BLT 12" and "SPECIALS 2" are the same shape,
    /// and this parser already answers "item" for that shape — see
    /// <see cref="Parse_PricedUppercaseLineIsAnItemNotAHeading"/>. Review can promote any line to
    /// a section, so it is recoverable rather than lost.
    /// </summary>
    [Theory]
    [InlineData("GARLIC BREAD 6.50")]
    [InlineData("SPECIALS 2")]
    public void Parse_ReadsACapitalsLineEndingInAPriceAsAnItem(string line)
    {
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), line, 1, []);

        Assert.Equal("item", Assert.Single(result.Lines).Disposition);
    }

    /*
     * M6.7 - the parser reads a real printed menu.
     *
     * The fixture below is the first two pages of a real four-page restaurant menu, pasted by the
     * owner out of the restaurant's own PDF. Against the M6.4 parser it produced 19 items, three of
     * them nonsense, ZERO sections, and 48 unresolved lines - the full menu produced 91 questions
     * on the review screen. That is not a menu anybody would sit and answer.
     *
     * It is used deliberately instead of another hand-written fixture. Every parser test before
     * this one was written from the same assumptions as the parser, which is exactly why the suite
     * stayed green while the two-space defect shipped (M6.4) and why it stayed green again while
     * a printed menu could not be read at all.
     */
    private const string RealPrintedMenu = """
        Appetizers
        Chicken Satay $7.00
        Chicken marinated in a curry sauce barbecued & served on bamboo
        skewers to be dipped in a flavorful peanut & cucumber sauce
        Edamame $4.00
        Steamed healthy soybeans
        Steamed Vegetable Dumpling $7.00
        Salads
        Thai Salad $6.50
        Garden fresh greens, cucumbers, tomatoes, bean sprouts,
        dried bean curd w. a light peanut dressing

        Soups
        Tofu Soup $4.95
        Assorted vegetables in a clear soup w. tofu
        Noodles
        Chicken $11.95, Beef $12.95, Shrimp $13.95
        Pad Thai
        Rice Noodles sauteed w. egg, peanuts, bean sprouts & scallions
        Pad Se-Ew
        Flat noodles sauteed w. egg & broccoli
        """;

    [Fact]
    public void Parse_ReadsTheSectionsOfARealPrintedMenu()
    {
        // Headings on a printed menu are Title Case, not capitals. Requiring capitals meant a
        // normally-typed menu produced no structure at all.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), RealPrintedMenu, 1, []);

        Assert.Equal(["Appetizers", "Salads", "Soups", "Noodles"],
            result.Lines.Where(line => line.Disposition == "section").Select(line => line.ParsedName));
    }

    [Fact]
    public void Parse_ReadsAnUnpricedLineUnderAnItemAsThatItemsDescription()
    {
        // Q81, settled 2026-08-07 and never implemented. Descriptions wrap across physical lines
        // on a real menu, so both halves have to land on the same item.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), RealPrintedMenu, 1, []);

        var satay = Assert.Single(result.Lines, line => line.ParsedName == "Chicken Satay");
        Assert.Equal("Chicken marinated in a curry sauce barbecued & served on bamboo skewers to be dipped in a flavorful peanut & cucumber sauce", satay.ParsedDescription);
        Assert.Equal("Steamed healthy soybeans", Assert.Single(result.Lines, line => line.ParsedName == "Edamame").ParsedDescription);
    }

    [Fact]
    public void Parse_ReadsADishUnderAPriceSetAsAnItemCarryingThatPrice()
    {
        // "Chicken $11.95, Beef $12.95, Shrimp $13.95" prices everything below it, per protein.
        // Vennusign has no variant model, so the dishes come through unpriced - which A11 allows -
        // and the price set raises exactly one question rather than becoming a fake item.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), RealPrintedMenu, 1, []);

        var padThai = Assert.Single(result.Lines, line => line.ParsedName == "Pad Thai");
        Assert.Equal("item", padThai.Disposition);
        Assert.Equal("$11.95", padThai.ParsedPrice);
        Assert.Contains("Rice Noodles sauteed", padThai.ParsedDescription!, StringComparison.Ordinal);
        Assert.Contains("Chicken $11.95, Beef $12.95, Shrimp $13.95", padThai.ParsedDescription!, StringComparison.Ordinal);

        var priceSet = Assert.Single(result.Lines, line => line.RawText.Trim().StartsWith("Chicken $11.95", StringComparison.Ordinal));
        Assert.Equal("description", priceSet.Disposition);
    }

    [Fact]
    public void Parse_APriceSetNeverBecomesAnItemNamedAfterItsOwnPrices()
    {
        // The exact defect: this line used to parse as an item named
        // "Chicken $11.95, Beef $12.95, Shrimp" priced $13.95. A plausible-looking row of nonsense
        // is worse than a question, because nothing about it asks to be checked.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), RealPrintedMenu, 1, []);

        Assert.DoesNotContain(result.Lines, line => line.ParsedName?.Contains('$', StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Parse_SeveralItemsOnOneLineBecomeSeveralItems()
    {
        // Same shape as a price set, different meaning - told apart by what follows it. Splitting
        // one line into several items is what Q216's LineSubIndex made possible.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(),
            "Sides: Jasmine Rice $2.00, Brown Rice $3.00, Peanut Sauce $2.00", 1, []);

        // The label becomes the section and each fragment becomes an item, all sharing the one
        // line number they were pasted on (Q216).
        Assert.Equal(3, result.ItemCount);
        Assert.Equal("Sides", Assert.Single(result.Lines, line => line.Disposition == "section").ParsedName);
        Assert.Equal(["Jasmine Rice", "Brown Rice", "Peanut Sauce"],
            result.Lines.Where(line => line.Disposition == "item").Select(line => line.ParsedName));
        Assert.Single(result.Lines.Select(line => line.LineNumber).Distinct());
    }

    [Fact]
    public void Parse_ARealPrintedMenuAsksAlmostNothing()
    {
        // The number that matters. This fixture produced 21 unresolved lines against the M6.4
        // parser; a whole four-page menu produced 91 questions. Decision 18 says confirm only what
        // we were unsure of - a review screen that asks about every line is the parser being
        // wrong, not the menu being messy.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), RealPrintedMenu, 1, []);

        // The price set no longer asks anything: it prices the dishes under it and travels in
        // their descriptions, so this excerpt now parses with no questions at all.
        Assert.Empty(result.Lines.Where(line => line.Disposition == "unresolved"));
        Assert.Equal(7, result.ItemCount);
        Assert.Equal("$11.95", result.Lines.First(line => line.ParsedName == "Pad Thai").ParsedPrice);
    }

    [Fact]
    public void Parse_StillRetainsEveryPhysicalLine()
    {
        // The invariant Q81 called out: a pasted line is never silently dropped. Descriptions gain
        // a disposition of their own rather than disappearing into the item above them.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), RealPrintedMenu, 1, []);

        Assert.Equal(RealPrintedMenu.Split('\n').Length, result.Lines.Count);
        Assert.All(result.Lines, line => Assert.Contains(line.Disposition,
            new[] { "blank", "section", "item", "unresolved", "fallback", "description" }));
    }

    [Fact]
    public void Parse_ACapitalsHeadingStillWins()
    {
        // M6.4's behaviour is unchanged for menus that were already readable.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), "STARTERS\nGarlic Bread 6.50\nWings 12", 1, []);

        Assert.Equal(["section", "item", "item"], result.Lines.Select(line => line.Disposition));
        Assert.Equal(2, result.ItemCount);
    }

    [Fact]
    public void Parse_AParenthesisedNoteIsNeverADish()
    {
        // "(Served w. Steamed Jasmine Rice)" is Title Case by shape and would otherwise be read as
        // a dish sitting under a price set.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(),
            "Curry\nChicken $12.95, Beef $13.95\n(Served w. Steamed Jasmine Rice)\nRed Curry\nSauteed w. coconut milk", 1, []);

        Assert.DoesNotContain(result.Lines, line => line.ParsedName?.StartsWith('(') == true);
        Assert.Contains(result.Lines, line => line.ParsedName == "Red Curry" && line.Disposition == "item");
    }

    [Theory]
    [InlineData("Tea $2.00 *(Green, Jasmine, Black & Red)")]
    [InlineData("Choice of Sauce: Garlic Sauce, Ginger Sauce, Green Curry")]
    [InlineData("& Red Curry Pineapple")]
    public void Parse_ATitleCaseLineThatIsNotAHeadingDoesNotBecomeASection(string line)
    {
        // All three produced a section on the first real-menu run against dev. A heading names a
        // group: it never carries a price, never labels itself with a colon, and never begins
        // mid-sentence. Found by pasting a whole real menu, not by reasoning about one.
        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(), $"Fish\nGrilled Fillet Fish $16.95\n{line}", 1, []);

        Assert.DoesNotContain(result.Lines, candidate => candidate.Disposition == "section" && candidate.ParsedName == line);
    }

    [Fact]
    public void Parse_EveryQuestionOnOneLineHasItsOwnKey()
    {
        /*
         * The 500 the owner hit on resume, reduced.
         *
         * M6.9 let one pasted line hold several items, and the question key was still
         * "line-{number}-{kind}" - so a Sides line's five items produced five questions all called
         * line-128-identity. Latent while only an operator wrote answers, one key at a time. The
         * moment the parser started answering questions itself, five answers arrived for one key,
         * PK_MenuImportAnswers threw inside the re-parse, and a re-parse is a resume.
         *
         * Reduced to the parser because that is where the key is minted, and asserting uniqueness
         * here is cheaper and clearer than reproducing a primary-key violation through SQL.
         */
        var library = new[]
        {
            new Item { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Name = "Jasmine Rice", Price = "2.00", IsActive = true },
            new Item { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Name = "Brown Rice", Price = "3.00", IsActive = true },
            new Item { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Name = "Peanut Sauce", Price = "2.00", IsActive = true }
        };

        var result = parser.Parse(Guid.NewGuid(), Guid.NewGuid(),
            "Appetizers\nGarlic Bread 6.50\nSides: Jasmine Rice 2.00, Brown Rice 3.00, Peanut Sauce 2.00", 1, library);

        Assert.True(result.Lines.GroupBy(line => line.LineNumber).Any(group => group.Count() > 1),
            "the fixture must contain a line holding several items, or it does not test the defect");
        Assert.Equal(result.Questions.Count, result.Questions.Select(question => question.QuestionKey).Distinct().Count());
    }
}
