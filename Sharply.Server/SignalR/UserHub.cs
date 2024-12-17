using Microsoft.AspNetCore.SignalR;
using Sharply.Server.Services;
using Sharply.Shared.Models;

/// <summary>
/// Manages user presence in real-time, tracking which users are online and broadcasting updates to all connected clients.
/// </summary>
public class UserHub : Hub
{
    private readonly UserService _userService;

    public UserHub(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Tracks the mapping between SignalR connection IDs and user IDs.
    /// </summary>
    private static readonly Dictionary<string, int> ConnectionUserMap = new();

    /// <summary>
    /// Marks a user as online, adds their user ID to the online users list, and notifies all connected clients.
    /// </summary>
    /// <param name="userId">The ID of the user who is now online.</param>
    public async Task GoOnline(int userId)
    {
        lock (ConnectionUserMap)
        {
            ConnectionUserMap[Context.ConnectionId] = userId;
        }

        Console.WriteLine($"User {userId} is online");
        var userDtos = await GetUserDtos(ConnectionUserMap.Values.Distinct().ToList());
        await Clients.All.SendAsync("UpdateOnlineUsers", userDtos);
    }

    /// <summary>
    /// Marks a user as offline, removes their user ID from the online users list, and notifies all connected clients.
    /// </summary>
    /// <param name="userId">The ID of the user who is now offline.</param>
    public async Task GoOffline()
    {
        int? userId = null;

        lock (ConnectionUserMap)
        {
            if (ConnectionUserMap.TryGetValue(Context.ConnectionId, out var id))
            {
                userId = id;
                ConnectionUserMap.Remove(Context.ConnectionId);
            }
        }

        if (userId.HasValue)
        {
            Console.WriteLine($"User {userId} went offline");
            var userDtos = await GetUserDtos(ConnectionUserMap.Values.Distinct().ToList());
            await Clients.All.SendAsync("UpdateOnlineUsers", userDtos);
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

    private async Task<List<UserDto>> GetUserDtos(List<int> userIds)
    {
        var userDtoTasks = userIds.Select(async id =>
        {
            var username = await _userService.GetUsernameFromId(id);
            return new UserDto
            {
                Id = id,
                Username = username
            };
        });

        var userDtos = await Task.WhenAll(userDtoTasks);

        return userDtos.Where(dto => dto.Username != null)
                       .OrderBy(dto => dto.Username)
                       .ToList();
    }
}

