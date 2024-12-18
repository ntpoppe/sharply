using Sharply.Server.Interfaces;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;

/// <summary>
/// Handles tracking online users, moved into it's own service for other potential SignalR hubs to use.
/// </summary>

public class UserTrackerService : IUserTrackerService
{
    private readonly IUserService _userService;

    public UserTrackerService(IUserService userService)
    {
        _userService = userService;
    }

    private readonly Dictionary<string, int> ConnectionUserMap = new(); // Tracks the mapping between SignalR connection IDs and user IDs.
    private readonly Dictionary<int, List<int>> UserServerAccess = new(); // Tracks the servers each user has access to.
    private readonly Dictionary<int, List<int>> UserChannelAccess = new(); // Tracks the channels each user has access to.

    /// <summary>
    /// Adds a user to the list of active connections.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="userId"></param>
    /// <returns>The id of the user that was added.</returns>
    public async Task<int?> AddUser(string connectionId, int userId)
    {
        var userChannels = await _userService.GetChannelsForUserAsync(userId);
        var userChannelIds = userChannels.Select(c => c.Id).ToList();
        lock (ConnectionUserMap)
        {
            ConnectionUserMap[connectionId] = userId;
            UserChannelAccess[userId] = userChannelIds;
        }

        return userId;
    }

    /// <summary>
    /// Removes a user from the list of active connections.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns>The id of the user that was removed.</returns>
    public int? RemoveUser(string connectionId)
    {
        int? userId = null;
        lock (ConnectionUserMap)
        {
            if (ConnectionUserMap.TryGetValue(connectionId, out var id))
            {
                userId = id;
                ConnectionUserMap.Remove(connectionId);
                UserChannelAccess.Remove(id);
            }
        }

        return userId;
    }

    /// <summary>
    /// Returns a list of tracked channels the online user has access to.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public List<int> GetTrackedUserChannels(int userId)
    {
        if (UserChannelAccess.ContainsKey(userId))
            return UserChannelAccess[userId];

        return new List<int>();
    }

    /// <summary>
    /// Returns a list of all users with an active connection.
    /// </summary>
    /// <returns>A list of user data transfer objects.</returns>
    public async Task<List<UserDto>> GetAllTrackedUsers()
    {
        var onlineUsersIds = ConnectionUserMap.Values.ToList();

        var userDtoTasks = onlineUsersIds.Select(async id =>
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
