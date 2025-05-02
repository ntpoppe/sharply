using System.Threading.Tasks;
using Sharply.Client.Models;
using Sharply.Shared.Models;

namespace Sharply.Client.Interfaces;

public interface IUserService
{
    CurrentUser? CurrentUser { get; }
    Task<CurrentUser> InitializeCurrentUserAsync();
    Task<UserDto?> GetCurrentUserDataAsync();
    Task<bool> CheckUserChannelAccessAsync(int userId, int channelId);
    void ClearCurrentUser();
}
