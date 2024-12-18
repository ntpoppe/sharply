using Sharply.Shared.Models;

namespace Sharply.Server.Interfaces;
public interface IUserTrackerService
{
    Task<int?> AddUser(string connectionId, int userId);
    int? RemoveUser(string connectionId);

    List<int> GetTrackedUserChannels(int userId);
    Task<List<UserDto>> GetAllTrackedUsers();
}

