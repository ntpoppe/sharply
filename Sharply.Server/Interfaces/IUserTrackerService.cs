using Sharply.Shared.Models;

namespace Sharply.Server.Interfaces;
public interface IUserTrackerService
{
    Task AddUser(string connectionId, int userId);
    void RemoveUser(string connectionId);
    int? GetUserIdFromConnectionId(string connectionId);
    List<int> GetTrackedUserChannels(int userId);
    Task<List<UserDto>> GetAllTrackedUsers();
}

