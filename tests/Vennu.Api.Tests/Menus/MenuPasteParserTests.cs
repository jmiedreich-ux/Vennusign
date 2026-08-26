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
}
