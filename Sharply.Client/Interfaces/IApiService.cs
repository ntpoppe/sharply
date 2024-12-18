using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharply.Client.Interfaces;

public interface IApiService
{
    Task<UserViewModel> RegisterAsync(string username, string password);
    Task<UserViewModel> LoginAsync(string username, string password);
    Task<List<ServerViewModel>> GetServersAsync(string tokenString);
    Task<UserTokenDataRequest?> GetCurrentUserTokenData(string tokenString);
    Task<List<MessageViewModel>> GetMessagesForChannel(string tokenString, int channelId);
    Task<bool> DoesUserHaveAccessToChannel(string tokenString, int userId, int channelId);
}
