using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Vennu.Api.TestAutomation;

public sealed class TestAutomationAuthorization(IOptions<TestAutomationOptions> options)
{
    public bool Allows(HttpRequest request, string scope, Guid venueId)
    {
        var configured = options.Value;
        var supplied = request.Headers[TestAutomationOptions.HeaderName].ToString();
        var allowedVenues = scope switch
        {
            "availability.backdate" => configured.AvailabilityVenueIds,
            "venue.reset" => configured.ResetVenueIds,
            _ => []
        };
        return !string.IsNullOrWhiteSpace(configured.ApiKey)
            && !string.IsNullOrWhiteSpace(supplied)
            && CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(supplied)),
                SHA256.HashData(Encoding.UTF8.GetBytes(configured.ApiKey)))
            && configured.Scopes.Contains(scope)
            && allowedVenues.Contains(venueId);
    }
}
