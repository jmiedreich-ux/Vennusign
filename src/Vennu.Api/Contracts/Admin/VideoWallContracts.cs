namespace Vennu.Api.Contracts.Admin;

public sealed record VideoWallScreen(Guid Id, string Name, int Position);

public sealed record VideoWallGroup(string Name, string Layout, IReadOnlyCollection<VideoWallScreen> Screens);

public sealed record VideoWallSnapshot(bool Enabled, IReadOnlyCollection<VideoWallGroup> Groups);

public sealed record VideoWallSaveRequest(string Name, string Layout, IReadOnlyCollection<Guid> ScreenIds);
