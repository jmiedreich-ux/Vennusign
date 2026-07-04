using Vennu.Api.Contracts.Display;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Contracts.Venues;

namespace Vennu.Api.Tests.Contracts;

public class DtoValidationTests
{
    [Fact]
    public void CreateVenueRequest_RequiresFields()
    {
        var model = new CreateVenueRequest
        {
            Name = string.Empty,
            Timezone = string.Empty,
            Type = string.Empty,
            PrimaryLanguage = string.Empty
        };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.Name), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.Timezone), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.Type), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.PrimaryLanguage), StringComparer.Ordinal));
    }

    [Fact]
    public void CreateVenueRequest_EnforcesMaxLengths()
    {
        var model = new CreateVenueRequest
        {
            Name = new string('n', 201),
            Timezone = new string('t', 101),
            Type = new string('y', 51),
            PrimaryLanguage = new string('l', 11),
            SecondaryLanguage = new string('s', 11)
        };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.Name), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.Timezone), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.Type), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.PrimaryLanguage), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateVenueRequest.SecondaryLanguage), StringComparer.Ordinal));
    }

    [Fact]
    public void RegisterScreenRequest_RequiresName()
    {
        var model = new RegisterScreenRequest { Name = string.Empty };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterScreenRequest.Name), StringComparer.Ordinal));
    }

    [Fact]
    public void RegisterScreenRequest_EnforcesMaxLengths()
    {
        var model = new RegisterScreenRequest
        {
            Name = new string('n', 201),
            Location = new string('l', 201),
            Platform = new string('p', 51),
            AppVersion = new string('v', 51)
        };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterScreenRequest.Name), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterScreenRequest.Location), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterScreenRequest.Platform), StringComparer.Ordinal));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterScreenRequest.AppVersion), StringComparer.Ordinal));
    }

    [Fact]
    public void ScreenHeartbeatRequest_RequiresStatus()
    {
        var model = new ScreenHeartbeatRequest { Status = string.Empty };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ScreenHeartbeatRequest.Status), StringComparer.Ordinal));
    }

    [Fact]
    public void ScreenHeartbeatRequest_EnforcesMaxLength()
    {
        var model = new ScreenHeartbeatRequest { Status = new string('o', 31) };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ScreenHeartbeatRequest.Status), StringComparer.Ordinal));
    }

    [Fact]
    public void GuidContracts_AllowGuidValues_ButControllerHandlesEmptyGuid()
    {
        var pairingRequestResults = Validate(new CreateScreenPairingCodeRequest { ScreenId = Guid.NewGuid() });
        var claimRequestResults = Validate(new ClaimScreenPairingCodeRequest { VenueId = Guid.NewGuid() });

        Assert.Empty(pairingRequestResults);
        Assert.Empty(claimRequestResults);
    }

    private static IReadOnlyCollection<ValidationResult> Validate<T>(T model) where T : class
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        _ = Validator.TryValidateObject(model, context, results, true);
        return results;
    }
}
