namespace Sharply.Server.Models;

/// <summary>
/// Represents a message sent by a user.
/// </summary>
public class Message
{
	/// <summary>
	/// Gets or sets the ID for the message.
	/// </summary>
	public required int Id { get; set; }

	/// <summary>
	/// Gets or sets the content of the message.
	/// </summary>
	public required string Content { get; set; }

	/// <summary>
	/// Gets or sets the timestamp when the message was sent.
	/// </summary>
	public required DateTime Timestamp { get; set; }

	/// <summary>
	/// Gets or sets the channel id the message is contained in.
	/// </summary>
	/// <remarks>
	/// This is a foreign key linking to the "Channels" table.
	/// </remarks>
	public required int ChannelId { get; set; }

	/// <summary>
	/// Navigation property for the channel that contains this message.
	/// </summary>
	public required Channel Channel { get; set; }

	/// <summary>
	/// Gets or sets the username of the sender.
	/// </summary>
	/// <remarks>
	/// This is a foreign key linking to the "Users" table.
	/// </remarks>
	public required string Username { get; set; }

	/// <summary>
	/// Navigation property for the user who sent the message
	/// </summary>
	public required User User { get; set; }
}
