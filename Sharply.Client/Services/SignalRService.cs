using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Sharply.Client.Services;
public class SignalRService
{
    private HubConnection? _connection;

    /// <summary>
    /// Connects to the SignalR hub using the provided token for authentication.
    /// </summary>
    /// <param name="token">The JWT token for authentication.</param>
    public async Task ConnectAsync(string? token)
    {
        if (_connection != null && _connection.State == HubConnectionState.Connected)
            return;

        var serverUri = "https://localhost:8000";

        _connection = new HubConnectionBuilder()
            .WithUrl($"{serverUri}/hubs/messages", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += error =>
        {
            Console.WriteLine("Reconnecting...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            Console.WriteLine("Reconnected.");
            // Rejoin groups?
            return Task.CompletedTask;
        };

        await _connection.StartAsync();
    }

    /// <summary>
    /// Subscribes to the "ReceiveMessage" event to handle incoming messages.
    /// </summary>
    /// <param name="callback">The callback action to invoke when a message is received.</param>
    public void OnMessageReceived(Action<string, string, DateTime> callback)
        => _connection?.On("ReceiveMessage", callback);

    /// <summary>
    /// Subscribes to the "ReceiveNotification" event for other channels.
    /// </summary>
    /// <param name="callback">The callback action to invoke when a notification is received.</param>
    public void OnNotificationReceived(Action<int, string> callback)
        => _connection?.On("ReceiveNotification", callback);

    /// <summary>
    /// Subscribes to the "ReceiveServerNotification" event for other servers.
    /// </summary>
    /// <param name="callback">The callback action to invoke when a server notification is received.</param>
    public void OnServerNotificationReceived(Action<int, int, string> callback)
        => _connection?.On("ReceiveServerNotification", callback);

    /// <summary>
    /// Joins a specific SignalR group (channel).
    /// </summary>
    /// <param name="channelId">The ID of the channel to join.</param>
    public async Task JoinChannelAsync(int channelId)
    {
        if (_connection != null)
            await _connection.InvokeAsync("JoinChannel", channelId);
    }

    /// <summary>
    /// Leaves a specific SignalR group (channel).
    /// </summary>
    /// <param name="channelId">The ID of the channel to leave.</param>
    public async Task LeaveChannelAsync(int channelId)
    {
        if (_connection != null)
            await _connection.InvokeAsync("LeaveChannel", channelId);
    }

    /// <summary>
    /// Sends a message to the current SignalR group.
    /// </summary>
    /// <param name="channelId">The ID of the channel to send the message to.</param>
    /// <param name="userId">The ID of the user sending the message.</param>
    /// <param name="message">The message content.</param>
    public async Task SendMessageAsync(int channelId, int userId, string message)
    {
        if (_connection != null)
            await _connection.InvokeAsync("SendMessageToChannel", channelId, userId, message);
    }

    /// <summary>
    /// Disconnects the SignalR connection gracefully.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_connection != null && _connection.State != HubConnectionState.Disconnected)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}