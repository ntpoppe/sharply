using Microsoft.AspNetCore.SignalR;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;

namespace Sharply.Server.SignalR;

/// <summary>
/// Represents a SignalR Hub for managing real-time messaging and channel interactions.
/// </summary>
public class MessageHub : Hub
{
    private readonly ISharplyContextFactory<SharplyDbContext> _contextFactory;
    private readonly IUserService _userService;

    public MessageHub(ISharplyContextFactory<SharplyDbContext> contextFactory, IUserService userService)
    {
        _contextFactory = contextFactory;
        _userService = userService;
    }

    /// <summary>
    /// Adds the current connection to a SignalR group representing a specific channel.
    /// </summary>
    /// <param name="channelId">The name of the channel to join.</param>
    /// <remarks>
    /// Id must be converted to a string.
    /// </remarks>
    public async Task JoinChannel(int channelId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, channelId.ToString());

    /// <summary>
    /// Removes the current connection from a SignalR group representing a specific channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to leave.</param>
    public async Task LeaveChannel(int channelId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId.ToString());

    /// <summary>
    /// Sends a message to all users in a specific channel group and saves it to the database.
    /// </summary>
    /// <param name="channelId">The ID of the channel to send the message to.</param>
    /// <param name="userId">The username of the sender.</param>
    /// <param name="content">The content of the message.</param>
    public async Task SendMessageToChannel(int channelId, int userId, string content)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var channel = await context.Channels.FindAsync(channelId);
        if (channel == null) return;

        var message = new Message
        {
            Content = content,
            UserId = userId,
            ChannelId = channel.Id,
            Timestamp = DateTime.UtcNow
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        var username = await _userService.GetUsernameFromId(userId);
        await Clients.Group(channelId.ToString()).SendAsync("ReceiveMessage", username, content, message.Timestamp);
    }
}
