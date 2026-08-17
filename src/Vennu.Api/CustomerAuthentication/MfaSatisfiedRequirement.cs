using Microsoft.AspNetCore.Authorization;

namespace Vennu.Api.CustomerAuthentication;

/// <summary>Marker requirement for <see cref="MfaSatisfiedAuthorizationHandler"/>.</summary>
public sealed class MfaSatisfiedRequirement : IAuthorizationRequirement;
