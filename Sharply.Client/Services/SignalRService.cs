using Microsoft.AspNetCore.SignalR.Client;
using Sharply.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sharply.Client.Services;

public class SignalRService
{
    private HubConnection? _messageHubConnection;
    private HubConnection? _userHubConnection;
    private const string URI = "https://localhost:8000";

    /// <summary>
    /// Connects to the "messages" SignalR hub using the provided token for authentication.
    /// </summary>
    /// <param name="token">The JWT token for authentication.</param>
    public async Task ConnectMessageHubAsync(string? token)
    {
        if (_messageHubConnection != null && _messageHubConnection.State == HubConnectionState.Connected)
            return;

        _messageHubConnection = new HubConnectionBuilder()
            .WithUrl($"{URI}/hubs/messages", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        await _messageHubConnection.StartAsync();
    }

    /// <summary>
    /// Connects to the "users" SignalR hub using the provided token for authentication.
    /// </summary>
    /// <param name="token">The JWT token for authentication.</param>
    public async Task ConnectUserHubAsync(string? token)
    {
        _userHubConnection = new HubConnectionBuilder()
            .WithUrl($"{URI}/hubs/users", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        await _userHubConnection.StartAsync();
    }

    /// <summary>
    /// Subscribes to the "ReceiveMessage" event to handle incoming messages.
    /// </summary>
    /// <param name="callback">The callback action to invoke when a message is received.</param>
    public void OnMessageReceived(Action<string, string, DateTime> callback)
        => _messageHubConnection?.On("ReceiveMessage", callback);

    /// <summary>
    /// Subscribes to the "ReceiveNotification" event for other channels.
    /// </summary>
    /// <param name="callback">The callback action to invoke when a notification is received.</param>
    public void OnNotificationReceived(Action<int, string> callback)
        => _messageHubConnection?.On("ReceiveNotification", callback);

    /// <summary>
    /// Subscribes to the "ReceiveServerNotification" event for other servers.
    /// </summary>
    /// <param name="callback">The callback action to invoke when a server notification is received.</param>
    public void OnServerNotificationReceived(Action<int, int, string> callback)
        => _messageHubConnection?.On("ReceiveServerNotification", callback);

    /// <summary>
    /// Joins a specific SignalR group (channel).
    /// </summary>
    /// <param name="channelId">The ID of the channel to join.</param>
    public async Task JoinChannelAsync(int channelId)
    {
        if (_messageHubConnection != null)
            await _messageHubConnection.InvokeAsync("JoinChannel", channelId);
    }

    /// <summary>
    /// Leaves a specific SignalR group (channel).
    /// </summary>
    /// <param name="channelId">The ID of the channel to leave.</param>
    public async Task LeaveChannelAsync(int channelId)
    {
        if (_messageHubConnection != null)
            await _messageHubConnection.InvokeAsync("LeaveChannel", channelId);
    }

    /// <summary>
    /// Sends a message to the current SignalR group.
    /// </summary>
    /// <param name="channelId">The ID of the channel to send the message to.</param>
    /// <param name="userId">The ID of the user sending the message.</param>
    /// <param name="message">The message content.</param>
    public async Task SendMessageAsync(int channelId, int userId, string message)
    {
        if (_messageHubConnection != null)
            await _messageHubConnection.InvokeAsync("SendMessageToChannel", channelId, userId, message);
    }

    /// <summary>
    /// Disconnects the SignalR connection gracefully.
    /// </summary>
    public async Task DisconnectMessageHubAsync()
    {
        if (_messageHubConnection != null && _messageHubConnection.State != HubConnectionState.Disconnected)
        {
            await _messageHubConnection.StopAsync();
            await _messageHubConnection.DisposeAsync();
            _messageHubConnection = null;
        }
    }

    /// <summary>
    /// Registers a callback to be executed when the list of online users is updated.
    /// </summary>
    public void OnOnlineUsersUpdated(Action<List<UserDto>> callback)
        => _userHubConnection?.On("UpdateOnlineUsers", callback);

    /// <summary>
    /// Notifies the server that a user has come online.
    /// </summary>
    /// <param name="userId">The ID of the user who is now online.</param>
    public async Task GoOnline(int userId)
        => await _userHubConnection?.InvokeAsync("GoOnline", userId);

    /// <summary>
    /// Notifies the server that a user has gone offline.
    /// </summary>
    /// <param name="userId">The ID of the user who is now offline.</param>
    public async Task GoOffline(int userId)
        => await _userHubConnection?.InvokeAsync("GoOffline", userId);
}