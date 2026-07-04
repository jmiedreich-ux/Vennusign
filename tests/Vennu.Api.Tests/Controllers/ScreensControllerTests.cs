using System.Text.RegularExpressions;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Controllers;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Data.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Controllers;

public class ScreensControllerTests
{
    [Fact]
    public async Task RegisterScreen_ReturnsCreated_WithScreenKeyFormat()
    {
        var screenRepository = new FakeScreenRepository();
        var sut = CreateController(screenRepository, new FakeScreenPairingCodeRepository(), new FakeVenueRepository());
        var request = new RegisterScreenRequest
        {
            Name = "  Main TV  ",
            Location = "  Bar  ",
            Platform = "  web  ",
            AppVersion = " 1.2.3 "
        };

        var result = await sut.RegisterScreen(request, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var response = Assert.IsType<RegisterScreenResponse>(created.Value);
        Assert.Matches(new Regex("^sc-[a-z0-9]{6}$"), response.ScreenKey);
        Assert.Equal($"/api/screens/{response.ScreenId}", created.Location);
        Assert.NotNull(screenRepository.LastCreatedScreen);
        Assert.Equal("Main TV", screenRepository.LastCreatedScreen!.Name);
        Assert.Equal("Bar", screenRepository.LastCreatedScreen.Location);
        Assert.Equal("web", screenRepository.LastCreatedScreen.Platform);
        Assert.Equal("1.2.3", screenRepository.LastCreatedScreen.AppVersion);
        Assert.Equal("Offline", screenRepository.LastCreatedScreen.Status);
    }

    [Fact]
    public async Task RegisterScreen_Throws_WhenScreenKeyCannotBeGeneratedAfterRetries()
    {
        var screenRepository = new FakeScreenRepository
        {
            GetByScreenKeyAsyncHandler = (_, _) => Task.FromResult<Screen?>(new Screen { Id = Guid.NewGuid() })
        };

        var sut = CreateController(screenRepository, new FakeScreenPairingCodeRepository(), new FakeVenueRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.RegisterScreen(new RegisterScreenRequest { Name = "Screen" }, CancellationToken.None));

        Assert.Equal("Unable to generate a unique screen key.", ex.Message);
    }

    [Fact]
    public async Task CreatePairingCode_ReturnsBadRequest_WhenScreenIdIsEmpty()
    {
        var sut = CreateController(new FakeScreenRepository(), new FakeScreenPairingCodeRepository(), new FakeVenueRepository());

        var result = await sut.CreatePairingCode(new CreateScreenPairingCodeRequest { ScreenId = Guid.Empty }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var validation = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Contains(nameof(CreateScreenPairingCodeRequest.ScreenId), validation.Errors.Keys);
    }

    [Fact]
    public async Task CreatePairingCode_ReturnsNotFound_WhenScreenMissing()
    {
        var sut = CreateController(new FakeScreenRepository(), new FakeScreenPairingCodeRepository(), new FakeVenueRepository());

        var result = await sut.CreatePairingCode(new CreateScreenPairingCodeRequest { ScreenId = Guid.NewGuid() }, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task CreatePairingCode_ReturnsCreated_WhenScreenExists()
    {
        var screenId = Guid.NewGuid();
        var pairingRepository = new FakeScreenPairingCodeRepository();
        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(new Screen { Id = screenId })
        };

        var sut = CreateController(screenRepository, pairingRepository, new FakeVenueRepository());

        var result = await sut.CreatePairingCode(new CreateScreenPairingCodeRequest { ScreenId = screenId }, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var response = Assert.IsType<CreateScreenPairingCodeResponse>(created.Value);
        Assert.Matches(new Regex("^\\d{6}$"), response.Code);
        Assert.Equal(screenId, response.ScreenId);
        Assert.StartsWith("/api/screens/pairing/", created.Location, StringComparison.Ordinal);
        Assert.EndsWith("/status", created.Location, StringComparison.Ordinal);
        Assert.NotNull(pairingRepository.LastCreatedPairingCode);
    }

    [Fact]
    public async Task CreatePairingCode_Throws_WhenUniqueCodeCannotBeGeneratedAfterRetries()
    {
        var screenId = Guid.NewGuid();
        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(new Screen { Id = screenId })
        };

        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(new ScreenPairingCode { Code = "111111" })
        };

        var sut = CreateController(screenRepository, pairingRepository, new FakeVenueRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreatePairingCode(new CreateScreenPairingCodeRequest { ScreenId = screenId }, CancellationToken.None));

        Assert.Equal("Unable to generate a unique pairing code.", ex.Message);
    }

    [Fact]
    public async Task GetPairingStatus_ReturnsNotFound_WhenCodeMissing()
    {
        var sut = CreateController(new FakeScreenRepository(), new FakeScreenPairingCodeRepository(), new FakeVenueRepository());

        var result = await sut.GetPairingStatus("123456", CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task GetPairingStatus_ReturnsGone_WhenCodeExpiredAndUnclaimed()
    {
        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(new ScreenPairingCode
            {
                Code = "123456",
                ScreenId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                IsClaimed = false
            })
        };

        var sut = CreateController(new FakeScreenRepository(), pairingRepository, new FakeVenueRepository());

        var result = await sut.GetPairingStatus("123456", CancellationToken.None);

        var gone = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status410Gone, gone.StatusCode);
    }

    [Fact]
    public async Task GetPairingStatus_ReturnsLinkedFalse_WhenNotClaimedAndActive()
    {
        var screenId = Guid.NewGuid();
        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(new ScreenPairingCode
            {
                Code = "123456",
                ScreenId = screenId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsClaimed = false
            })
        };

        var sut = CreateController(new FakeScreenRepository(), pairingRepository, new FakeVenueRepository());

        var result = await sut.GetPairingStatus("123456", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ScreenPairingStatusResponse>(ok.Value);
        Assert.False(response.Linked);
        Assert.Null(response.ScreenId);
    }

    [Fact]
    public async Task GetPairingStatus_ReturnsLinkedTrue_WhenClaimed()
    {
        var screenId = Guid.NewGuid();
        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(new ScreenPairingCode
            {
                Code = "123456",
                ScreenId = screenId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                IsClaimed = true
            })
        };

        var sut = CreateController(new FakeScreenRepository(), pairingRepository, new FakeVenueRepository());

        var result = await sut.GetPairingStatus("123456", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ScreenPairingStatusResponse>(ok.Value);
        Assert.True(response.Linked);
        Assert.Equal(screenId, response.ScreenId);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsBadRequest_WhenVenueIdIsEmpty()
    {
        var sut = CreateController(new FakeScreenRepository(), new FakeScreenPairingCodeRepository(), new FakeVenueRepository());

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = Guid.Empty }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var validation = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Contains(nameof(ClaimScreenPairingCodeRequest.VenueId), validation.Errors.Keys);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsNotFound_WhenCodeMissing()
    {
        var sut = CreateController(new FakeScreenRepository(), new FakeScreenPairingCodeRepository(), new FakeVenueRepository());

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = Guid.NewGuid() }, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsConflict_WhenCodeAlreadyClaimed()
    {
        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(new ScreenPairingCode
            {
                Code = "123456",
                IsClaimed = true,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                ScreenId = Guid.NewGuid()
            })
        };

        var sut = CreateController(new FakeScreenRepository(), pairingRepository, new FakeVenueRepository());

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = Guid.NewGuid() }, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsGone_WhenCodeExpired()
    {
        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(new ScreenPairingCode
            {
                Code = "123456",
                IsClaimed = false,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                ScreenId = Guid.NewGuid()
            })
        };

        var sut = CreateController(new FakeScreenRepository(), pairingRepository, new FakeVenueRepository());

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = Guid.NewGuid() }, CancellationToken.None);

        var gone = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status410Gone, gone.StatusCode);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsNotFound_WhenVenueDoesNotExist()
    {
        var pairing = new ScreenPairingCode
        {
            Code = "123456",
            IsClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ScreenId = Guid.NewGuid()
        };

        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(pairing)
        };

        var sut = CreateController(new FakeScreenRepository(), pairingRepository, new FakeVenueRepository());

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = Guid.NewGuid() }, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsNotFound_WhenScreenDoesNotExist()
    {
        var venueId = Guid.NewGuid();
        var pairing = new ScreenPairingCode
        {
            Code = "123456",
            IsClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ScreenId = Guid.NewGuid()
        };

        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(pairing)
        };

        var venueRepository = new FakeVenueRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Venue?>(new Venue { Id = venueId, Name = "Venue" })
        };

        var sut = CreateController(new FakeScreenRepository(), pairingRepository, venueRepository);

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = venueId }, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsConflict_WhenClaimRepositoryReturnsFalse()
    {
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var pairing = new ScreenPairingCode
        {
            Code = "123456",
            IsClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ScreenId = screenId
        };

        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(pairing),
            ClaimAsyncHandler = (_, _, _) => Task.FromResult(false)
        };

        var venueRepository = new FakeVenueRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Venue?>(new Venue { Id = venueId, Name = "Venue" })
        };

        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(new Screen { Id = screenId, Name = "Screen", ScreenKey = "sc-abc123", Status = "Offline" })
        };

        var sut = CreateController(screenRepository, pairingRepository, venueRepository);

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = venueId }, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsProblem_WhenScreenLinkFails()
    {
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var pairing = new ScreenPairingCode
        {
            Code = "123456",
            IsClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ScreenId = screenId
        };

        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(pairing),
            ClaimAsyncHandler = (_, _, _) => Task.FromResult(true)
        };

        var venueRepository = new FakeVenueRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Venue?>(new Venue { Id = venueId, Name = "Venue" })
        };

        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(new Screen { Id = screenId, Name = "Screen", ScreenKey = "sc-abc123", Status = "Offline" }),
            AssignVenueAsyncHandler = (_, _, _) => Task.FromResult(false)
        };

        var sut = CreateController(screenRepository, pairingRepository, venueRepository);

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = venueId }, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [Fact]
    public async Task ClaimPairingCode_ReturnsOk_WhenClaimAndLinkSucceed()
    {
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var pairing = new ScreenPairingCode
        {
            Code = "123456",
            IsClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ScreenId = screenId
        };

        var pairingRepository = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) => Task.FromResult<ScreenPairingCode?>(pairing),
            ClaimAsyncHandler = (_, _, _) => Task.FromResult(true)
        };

        var venueRepository = new FakeVenueRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Venue?>(new Venue { Id = venueId, Name = "Venue" })
        };

        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(new Screen { Id = screenId, Name = "Screen", ScreenKey = "sc-abc123", Status = "Offline" }),
            AssignVenueAsyncHandler = (_, _, _) => Task.FromResult(true)
        };

        var sut = CreateController(screenRepository, pairingRepository, venueRepository);

        var result = await sut.ClaimPairingCode("123456", new ClaimScreenPairingCodeRequest { VenueId = venueId }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ClaimScreenPairingCodeResponse>(ok.Value);
        Assert.True(response.Linked);
        Assert.Equal(screenId, response.ScreenId);
        Assert.Equal(venueId, response.VenueId);
    }

    private static ScreensController CreateController(IScreenRepository screenRepository, IScreenPairingCodeRepository pairingCodeRepository, IVenueRepository venueRepository)
    {
        return new ScreensController(screenRepository, pairingCodeRepository, venueRepository);
    }
}
