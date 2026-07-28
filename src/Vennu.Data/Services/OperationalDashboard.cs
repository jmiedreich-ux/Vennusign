namespace Vennu.Data.Services;

public sealed record OperationalDashboard(
    int TotalVenues,
    int ActiveVenues,
    int TrialingVenues,
    int CanceledLast30Days,
    int OnlineScreens,
    int OfflineScreens,
    IReadOnlyCollection<OperationalScreenHealth> Screens);

public sealed record OperationalScreenHealth(
    Guid ScreenId,
    Guid? VenueId,
    string VenueName,
    string ScreenName,
    string? Location,
    string Status,
    DateTime? LastSeen);
