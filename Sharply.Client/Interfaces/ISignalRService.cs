using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sharply.Shared.Models;

namespace Sharply.Client.Interfaces;

public interface ISignalRService
{
    Task ConnectMessageHubAsync(string? token);
    Task ConnectUserHubAsync(string? token);
    void OnMessageReceived(Action<string, string, DateTime> callback);
    void OnNotificationReceived(Action<int, string> callback);
    void OnServerNotificationReceived(Action<int, int, string> callback);
    Task JoinChannelAsync(int channelId);
    Task LeaveChannelAsync(int channelId);
    Task SendMessageAsync(int channelId, int userId, string message);
    Task DisconnectMessageHubAsync();
    Task DisconnectUserHubAsync(int userId);
    void OnOnlineUsersUpdated(Action<List<UserDto>> callback);
    Task GoOnline(int userId);
    Task GoOffline();
}
