using Microsoft.AspNetCore.SignalR;
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
    /// <param name="channelId">The ID of the channel to join.</param>
	public async Task JoinChannel(string channelId)
		=> await Groups.AddToGroupAsync(Context.ConnectionId, channelId);

	/// <summary>
    /// Removes the current connection from a SignalR group representing a specific channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to leave.</param>
	public async Task LeaveChannel(string channelId)
		=> await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);

	/// <summary>
    /// Sends a message to all users in a specific channel group and saves it to the database.
    /// </summary>
    /// <param name="channelId">The ID of the channel to send the message to.</param>
    /// <param name="username">The username of the sender.</param>
    /// <param name="content">The content of the message.</param>
	public async Task SendMessageToChannel(string channelId, string username, string content)
	{
		var channel = await _context.Channels.FindAsync(int.Parse(channelId));
		if (channel == null) return;

		var message = new Message 
		{
			Content = content,
			Username = username,
			ChannelId = channel.Id,
			Timestamp = DateTime.UtcNow
		};

		_context.Messages.Add(message);
		await _context.SaveChangesAsync();

		await Clients.Group(channelId).SendAsync("ReceiveMessage", username, content, message.Timestamp);
	}
}
