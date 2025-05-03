using System.Collections.Generic;
using System.Threading.Tasks;
using Sharply.Client.ViewModels;
using Sharply.Shared.Models;
using Sharply.Shared.Requests;

namespace Sharply.Client.Interfaces;

public interface IApiService
{
    Task<UserViewModel> RegisterAsync(string username, string password);
    Task<UserViewModel> LoginAsync(string username, string password);
    Task<ServerDto> CreateServerAsync(string tokenString, CreateServerRequest request);
    Task SoftDeleteServerAsync(string tokenString, int serverId);
    Task<ServerDto> JoinServerAsync(string tokenString, JoinServerRequest inviteCode);
    Task<bool> LeaveServerAsync(string tokenString, LeaveServerRequest request);
    Task<List<ServerDto>> GetServersAsync(string tokenString);
    Task<UserDto?> GetCurrentUserData(string tokenString);
    Task<List<MessageDto>> GetMessagesForChannel(string tokenString, int channelId);
    Task<bool> CheckUserChannelAccess(string tokenString, int userId, int channelId);
}
