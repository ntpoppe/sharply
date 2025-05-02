using System.Collections.Generic;
using System.Threading.Tasks;
using Sharply.Client.ViewModels;
using Sharply.Shared;
using Sharply.Shared.Models;
using Sharply.Shared.Requests;

namespace Sharply.Client.Interfaces;

public interface IApiService
{
    Task<UserViewModel> RegisterAsync(string username, string password);
    Task<UserViewModel> LoginAsync(string username, string password);
    Task<ServerViewModel> CreateServerAsync(string tokenString, CreateServerRequest request);
    Task SoftDeleteServerAsync(string tokenString, int serverId);
    Task<ApiResponse<ServerDto>> JoinServerAsync(string tokenString, JoinServerRequest inviteCode);
    Task<ApiResponse<bool>> LeaveServerAsync(string tokenString, LeaveServerRequest request);
    Task<List<ServerViewModel>> GetServersAsync(string tokenString);
    Task<UserDto?> GetCurrentUserData(string tokenString);
    Task<List<MessageViewModel>> GetMessagesForChannel(string tokenString, int channelId);
    Task<bool> CheckUserChannelAccess(string tokenString, int userId, int channelId);
}
