using Sharply.Client.ViewModels;
using Sharply.Shared.Models;
using Sharply.Shared.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharply.Client.Interfaces;

public interface IApiService
{
    Task<UserViewModel> RegisterAsync(string username, string password);
    Task<UserViewModel> LoginAsync(string username, string password);
	Task<ServerViewModel> CreateServerAsync(string tokenString, CreateServerRequest request);
	Task SoftDeleteServerAsync(string tokenString, int serverId);
    Task<List<ServerViewModel>> GetServersAsync(string tokenString);
    Task<UserDto?> GetCurrentUserData(string tokenString);
    Task<List<MessageViewModel>> GetMessagesForChannel(string tokenString, int channelId);
    Task<bool> CheckUserChannelAccess(string tokenString, int userId, int channelId);
}
