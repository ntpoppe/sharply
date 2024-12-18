using Microsoft.AspNetCore.SignalR;
using Sharply.Server.Interfaces;

/// <summary>
/// Manages user presence in real-time, tracking which users are online and broadcasting updates to all connected clients.
/// </summary>
public class UserHub : Hub
{
    private readonly IUserTrackerService _userTrackerService;

    public UserHub(IUserService userService, IUserTrackerService userTrackerService)
    {
        _userTrackerService = userTrackerService;
    }

    /// <summary>
    /// Marks a user as online, adds their user ID to the online users list, and notifies all connected clients.
    /// </summary>
    /// <param name="userId">The ID of the user who is now online.</param>
    public async Task GoOnline(int userId)
    {
        await _userTrackerService.AddUser(Context.ConnectionId, userId);
        var userChannelIds = _userTrackerService.GetTrackedUserChannels(userId);

        Console.WriteLine($"User {userId} is online");
        foreach (var channelId in userChannelIds)
        {
            await BroadcastOnlineUsers(channelId);
        }
    }

    /// <summary>
    /// Marks a user as offline, removes their user ID from the online users list, and notifies all connected clients.
    /// </summary>
    /// <param name="userId">The ID of the user who is now offline.</param>
    public async Task GoOffline()
    {
        var userId = _userTrackerService.GetUserIdFromConnectionId(Context.ConnectionId);
        if (userId.HasValue)
        {
            var channels = _userTrackerService.GetTrackedUserChannels(userId.Value);
            _userTrackerService.RemoveUser(Context.ConnectionId);

            Console.WriteLine($"User {userId} went offline");

            foreach (var channelId in channels)
            {
                await BroadcastOnlineUsers(channelId);
            }
        }
    }

    /// <summary>
    /// Handles a client disconnection by performing cleanup logic.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnection, if any.</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await GoOffline();
        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastOnlineUsers(int channelId)
    {
        var userDtos = await _userTrackerService.GetAllTrackedUsers();
        await Clients.All.SendAsync("UpdateOnlineUsers", userDtos);
    }
}

