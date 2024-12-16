using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Models;

namespace Sharply.Server.SignalR;

/// <summary>
/// Represents a SignalR Hub for managing real-time messaging and channel interactions.
/// </summary>
public class MessageHub : Hub
{
    private readonly SharplyDbContext _context;

    /// <summary>
    /// Initializes the MessageHub with a database context for accessing channels and messages.
    /// </summary>
    /// <param name="context">The database context to interact with channels and messages.</param>
	public MessageHub(SharplyDbContext context)
        => _context = context;

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
    /// Joins the default channel for the default global server.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task JoinDefaultChannel(int userId)
    {
        var generalChannel = await _context.Channels.FirstOrDefaultAsync(c => c.Name == "General");
        if (generalChannel == null) return;

        var userChannel = await _context.UserChannels.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChannelId == generalChannel.Id);
        if (userChannel == null)
        {
            _context.UserChannels.Add(new UserChannel
            {
                UserId = userId,
                ChannelId = generalChannel.Id
            });
            await _context.SaveChangesAsync();
        }

        await JoinChannel(generalChannel.Id);
    }

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
        var channel = await _context.Channels.FindAsync(channelId);
        if (channel == null) return;

        var message = new Message
        {
            Content = content,
            UserId = userId,
            ChannelId = channel.Id,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        await Clients.Group(channelId.ToString()).SendAsync("ReceiveMessage", userId, content, message.Timestamp);
    }
}
