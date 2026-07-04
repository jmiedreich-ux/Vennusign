using Microsoft.AspNetCore.SignalR;

namespace Vennu.Api.Hubs;

public class VennuHub : Hub
{
    public Task JoinScreen(Guid screenId) => Groups.AddToGroupAsync(Context.ConnectionId, $"screen:{screenId}");

    public Task LeaveScreen(Guid screenId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"screen:{screenId}");

    public Task JoinVenue(Guid venueId) => Groups.AddToGroupAsync(Context.ConnectionId, $"venue:{venueId}");

    public Task LeaveVenue(Guid venueId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"venue:{venueId}");

    public Task JoinVideoWall(Guid wallId, int position)
    {
        _ = position;
        return Groups.AddToGroupAsync(Context.ConnectionId, $"wall:{wallId}");
    }

    public Task LeaveVideoWall(Guid wallId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"wall:{wallId}");
}
