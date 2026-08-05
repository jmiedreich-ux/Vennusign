using Vennu.Core.Models;

namespace Vennu.DataAccess.Tests;

public sealed class CapabilityModelTests
{
    [Theory]
    [InlineData("screen.device.pair")]
    [InlineData("content.item.availability_update")]
    [InlineData("publishing.release.publish")]
    public void CapabilityId_AcceptsCanonicalFormat(string value)
    {
        Assert.Equal(value, CapabilityId.Parse(value).Value);
    }

    [Theory]
    [InlineData("screens")]
    [InlineData("Screen.Device.Pair")]
    [InlineData("starter.screen.pair")]
    [InlineData("screen.device.pair.extra")]
    [InlineData(" screen.device.pair")]
    public void CapabilityId_RejectsNonCanonicalFormat(string value)
    {
        Assert.Throws<FormatException>(() => CapabilityId.Parse(value));
    }

    [Fact]
    public void Registry_IsUniqueAndDomainMatchesIdentifier()
    {
        Assert.NotEmpty(Version1CapabilityRegistry.Definitions);
        Assert.Equal(
            Version1CapabilityRegistry.Definitions.Count,
            Version1CapabilityRegistry.Definitions.Select(item => item.Id).Distinct().Count());

        foreach (var definition in Version1CapabilityRegistry.Definitions)
        {
            var domain = definition.Id.Value.Split('.')[0];
            Assert.Equal(definition.Domain.ToString(), domain, ignoreCase: true);
            Assert.StartsWith($"capabilities.{definition.Id}.", definition.NameMessageKey, StringComparison.Ordinal);
            Assert.StartsWith($"capabilities.{definition.Id}.", definition.DescriptionMessageKey, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Registry_KeepsPublishingAsDistinctDomain()
    {
        var publishing = Version1CapabilityRegistry.Definitions
            .Where(item => item.Domain == CapabilityDomain.Publishing)
            .ToArray();

        Assert.NotEmpty(publishing);
        Assert.All(publishing, item => Assert.StartsWith("publishing.", item.Id.Value, StringComparison.Ordinal));
    }

    [Fact]
    public void Registry_DoesNotEmbedPackagingIndustryProviderRouteOrDisplayLabels()
    {
        string[] prohibitedTokens =
        [
            "free", "starter", "operate", "coordinate", "portfolio", "enterprise",
            "restaurant", "bar", "cafe", "hospitality", "entertainment", "food_truck",
            "stripe", "square", "toast", "clover", "route", "screen_page", "menu_page"
        ];

        Assert.All(
            Version1CapabilityRegistry.Definitions,
            definition => Assert.DoesNotContain(
                prohibitedTokens,
                token => definition.Id.Value.Contains(token, StringComparison.Ordinal)));
    }

    [Fact]
    public void EveryCurrentFeatureKeyHasExactlyOneTypedDisposition()
    {
        string[] expected =
        [
            "photo_grid", "classic_diner", "basic_scheduling", "allergen_badges", "analytics",
            "meal_periods", "bilingual_display", "ai_translation", "quick_update", "all_layouts",
            "happy_hour", "pos_integration", "staff_app", "ai_custom_builder", "multi_location",
            "white_label", "html_editor", "video_wall"
        ];

        Assert.Equal(expected.Order(), CurrentConceptReconciliation.FeatureKeys.Keys.Order());
        Assert.All(CurrentConceptReconciliation.FeatureKeys.Values, disposition =>
        {
            Assert.False(string.IsNullOrWhiteSpace(disposition.TypedTarget));
            Assert.All(disposition.Capabilities, id => Assert.True(Version1CapabilityRegistry.ById.ContainsKey(id)));
        });
    }

    [Fact]
    public void RouteKeysAreNavigationNotCapabilities()
    {
        Assert.All(
            CurrentConceptReconciliation.RouteKeys.Values,
            disposition => Assert.Equal(CurrentConceptDispositionKind.Navigation, disposition.Kind));
    }
}
