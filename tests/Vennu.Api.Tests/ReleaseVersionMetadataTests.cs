using Vennu.Api.Release;

namespace Vennu.Api.Tests;

public sealed class ReleaseVersionMetadataTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void FromEnvironment_UsesSafeLocalDefaults()
    {
        var metadata = ReleaseVersionMetadata.FromEnvironment();
        Assert.NotEmpty(metadata.ProductVersion);
        Assert.NotEmpty(metadata.ComponentVersion);
        Assert.True(metadata.ApiContractMajor > 0);
    }
}
