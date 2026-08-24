using Vennu.Api.Services;
using Vennu.Core.Models;

namespace Vennu.Api.Tests.Services;

/// <summary>
/// The derived save model, tested where its logic lives. The draft is a comparison
/// between the menu and the snapshot its screens are showing, so these run against
/// snapshots directly — no database, and no fake that could quietly re-implement
/// the behaviour it is meant to be checking.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenuSnapshotDiffTests
{
    private static readonly Guid MenuId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SectionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ItemId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid ScreenId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static string Snapshot(
        string price = "12",
        string sectionName = "Drinks",
        string menuName = "Summer",
        string? theme = null,
        bool placed = true,
        bool onScreen = true,
        int itemOrder = 0,
        bool isListed = true)
    {
        var snapshot = new MenuSnapshot
        {
            MenuId = MenuId,
            Name = menuName,
            Theme = theme,
            DwellSeconds = 8,
            LoopWarningSeconds = 60,
            Screens = onScreen ? [new SnapshotScreen { ScreenId = ScreenId }] : [],
            Sections =
            [
                new SnapshotSection
                {
                    SectionId = SectionId,
                    Name = sectionName,
                    SortOrder = 0,
                    Items = placed
                        ? [new SnapshotItem { ItemId = ItemId, Name = "Berry Fizz", Price = price, SortOrder = itemOrder, IsListed = isListed }]
                        : []
                }
            ]
        };

        return MenuSnapshot.Serialize(snapshot);
    }

    private static readonly Guid SecondSectionId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    /// <summary>The same library item placed on two sections of one menu.</summary>
    private static string TwoSectionSnapshot(string price = "12", bool secondSectionHoldsItem = true)
    {
        SnapshotItem Item() => new() { ItemId = ItemId, Name = "Berry Fizz", Price = price, SortOrder = 0 };

        return MenuSnapshot.Serialize(new MenuSnapshot
        {
            MenuId = MenuId,
            Name = "Summer",
            Theme = null,
            DwellSeconds = 8,
            LoopWarningSeconds = 60,
            Screens = [new SnapshotScreen { ScreenId = ScreenId }],
            Sections =
            [
                new SnapshotSection { SectionId = SectionId, Name = "Drinks", SortOrder = 0, Items = [Item()] },
                new SnapshotSection
                {
                    SectionId = SecondSectionId,
                    Name = "Happy Hour",
                    SortOrder = 1,
                    Items = secondSectionHoldsItem ? [Item()] : []
                }
            ]
        });
    }

    [Fact]
    public void AMenuThatMatchesItsScreens_HasNothingToPublish()
    {
        Assert.Empty(MenuSnapshot.Diff(Snapshot(), Snapshot()));
    }

    // Q182: the count is what is currently different. A price edited and then put
    // back is not a change, and no client assertion is involved in deciding that.
    [Fact]
    public void AnEditTakenBackToThePublishedValue_IsNotAChange()
    {
        var published = Snapshot(price: "12");

        Assert.Single(MenuSnapshot.Diff(published, Snapshot(price: "13")));
        Assert.Empty(MenuSnapshot.Diff(published, Snapshot(price: "12")));
    }

    // The previous value always comes from the snapshot, so a caller cannot claim a
    // change is a revert when it is not.
    [Fact]
    public void TheBeforeValueComesFromTheSnapshotNotTheCaller()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(price: "12"), Snapshot(price: "13")));

        Assert.Equal("12", change.BeforeValue);
        Assert.Equal("13", change.AfterValue);
    }

    // The defect that made the previous review's narrow-restore justification false:
    // section edits are reachable and must be visible to the draft.
    [Fact]
    public void RenamingASection_IsAChange()
    {
        var change = Assert.Single(
            MenuSnapshot.Diff(Snapshot(sectionName: "Drinks"), Snapshot(sectionName: "Cocktails")));

        Assert.Equal(DraftTargetKinds.Section, change.TargetKind);
        Assert.Equal(SectionId, change.TargetId);
        Assert.Equal("Cocktails", change.AfterValue);
    }

    [Fact]
    public void ReorderingAnItem_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(itemOrder: 0), Snapshot(itemOrder: 3)));

        Assert.Equal(DraftTargetKinds.Placement, change.TargetKind);
        Assert.Equal("sortOrder", change.Field);
    }

    // A menu with no theme attached is a valid state (Q86), so null is a value the
    // diff compares like any other: attaching one is a change, taking it off is a
    // change, and having never had one is not.
    [Fact]
    public void AttachingAThemeToAnUnthemedMenu_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(theme: null), Snapshot(theme: "harbour-dark")));

        Assert.Equal(DraftTargetKinds.Theme, change.TargetKind);
        Assert.Null(change.BeforeValue);
        Assert.Equal("harbour-dark", change.AfterValue);
    }

    [Fact]
    public void TakingTheThemeOffAMenu_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(theme: "harbour-dark"), Snapshot(theme: null)));

        Assert.Equal(DraftTargetKinds.Theme, change.TargetKind);
        Assert.Equal("harbour-dark", change.BeforeValue);
        Assert.Null(change.AfterValue);
    }

    [Fact]
    public void AMenuThatNeverHadAThemeHasNothingToPublishAboutOne()
    {
        Assert.DoesNotContain(
            MenuSnapshot.Diff(Snapshot(theme: null), Snapshot(theme: null)),
            change => change.TargetKind == DraftTargetKinds.Theme);
    }

    [Fact]
    public void RenamingTheMenu_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(menuName: "Summer"), Snapshot(menuName: "Winter")));

        Assert.Equal("name", change.Field);
        Assert.Equal("Winter", change.AfterValue);
    }

    [Fact]
    public void RemovingAnItemFromTheBoard_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(placed: true), Snapshot(placed: false)));

        Assert.Equal(DraftTargetKinds.Placement, change.TargetKind);
        Assert.Equal("false", change.AfterValue);
    }

    [Fact]
    public void PuttingAnItemBackOnTheBoard_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(placed: false), Snapshot(placed: true)));

        Assert.Equal("placed", change.Field);
        Assert.Equal("true", change.AfterValue);
    }

    // Q182 counts the latest state per field per item. One library item can sit on
    // several boards, so editing its price once must read as one change however
    // many places it appears - a person who changed one price is never told they
    // changed three things.
    [Fact]
    public void EditingAnItemPlacedTwice_IsStillOneChange()
    {
        var change = Assert.Single(
            MenuSnapshot.Diff(TwoSectionSnapshot(price: "12"), TwoSectionSnapshot(price: "13")));

        Assert.Equal(DraftTargetKinds.Item, change.TargetKind);
        Assert.Equal(ItemId, change.TargetId);
        Assert.Equal("price", change.Field);
        Assert.Equal("12", change.BeforeValue);
        Assert.Equal("13", change.AfterValue);
    }

    // Available is a drafted change like name/description/price: turning it off
    // is one change that waits for Publish, unlike 86 (ItemAvailability), which
    // never appears in a snapshot because it is never drafted.
    [Fact]
    public void TurningAnItemUnavailable_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(isListed: true), Snapshot(isListed: false)));

        Assert.Equal(DraftTargetKinds.Item, change.TargetKind);
        Assert.Equal(ItemId, change.TargetId);
        Assert.Equal("isListed", change.Field);
        Assert.Equal("true", change.BeforeValue);
        Assert.Equal("false", change.AfterValue);
    }

    // ...and the placements themselves are still counted separately, because
    // taking the item off one board is a different act from editing it.
    [Fact]
    public void RemovingOneOfTwoPlacements_IsOnePlacementChange()
    {
        var change = Assert.Single(
            MenuSnapshot.Diff(TwoSectionSnapshot(), TwoSectionSnapshot(secondSectionHoldsItem: false)));

        Assert.Equal(DraftTargetKinds.Placement, change.TargetKind);
        Assert.Equal("placed", change.Field);
        Assert.Equal("false", change.AfterValue);
    }

    // Q68: take-off waits as a difference in which screens the menu is on, and
    // reaches them on the next publish.
    [Fact]
    public void TakingTheMenuOffItsScreens_IsAChange()
    {
        var change = Assert.Single(MenuSnapshot.Diff(Snapshot(onScreen: true), Snapshot(onScreen: false)));

        Assert.Equal(DraftTargetKinds.Screens, change.TargetKind);
        Assert.Equal($"{ScreenId}:{Guid.Empty}", change.BeforeValue);
        Assert.Equal(string.Empty, change.AfterValue);
    }

    // Q115/Q190: prices are compared as typed, so these are genuinely different.
    [Fact]
    public void PricesAreComparedAsTypedNotAsNumbers()
    {
        Assert.Single(MenuSnapshot.Diff(Snapshot(price: "9.5"), Snapshot(price: "9.50")));
        Assert.Empty(MenuSnapshot.Diff(Snapshot(price: "MP"), Snapshot(price: "MP")));
    }

    // A menu that has never been published is entirely new, so everything about it
    // is waiting to go out rather than nothing.
    [Fact]
    public void AMenuNeverPublished_ReportsItsContentAsWaiting()
    {
        var changes = MenuSnapshot.Diff(null, Snapshot());

        Assert.NotEmpty(changes);
        Assert.Contains(changes, change => change.Field == "name" && change.BeforeValue is null);
    }

    // Regression for the snapshot JSON defect: a snapshot must survive the round
    // trip the restore path depends on. Nested arrays escaped as strings would fail
    // here rather than at restore time.
    [Fact]
    public void ASerializedSnapshotRoundTripsWithItsNestedContent()
    {
        var parsed = MenuSnapshot.Parse(Snapshot());

        Assert.NotNull(parsed);
        var section = Assert.Single(parsed!.Sections!);
        var item = Assert.Single(section.Items!);
        Assert.Equal(ItemId, item.ItemId);
        Assert.Equal(ScreenId, Assert.Single(parsed.Screens!).ScreenId);
    }
}
