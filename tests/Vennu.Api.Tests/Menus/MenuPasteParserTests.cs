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
}
