namespace Sharply.Server.Models;

/// <summary>
/// Represents a channel in the system. Channels are contained by a server.
/// </summary>

public class Channel
{
    /// <summary>
    /// Gets or sets the unique id for the channel.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the channel
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the server this channel belongs to.
    /// </summary>
    /// <remarks>
    /// This is the foreign key linked to the "Servers" table.
    /// </remarks>
    public required int ServerId { get; set; }

    /// <summary>
    /// Navigation property for the server that contains this channel.
    /// </summary>
    public Server? Server { get; set; }

    /// <summary>
    /// Gets or sets a collection of messages contained by the channel.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    /// <summary>
    /// A collection of users in this channel.
    /// </summary>
    public ICollection<UserChannel> UserChannels { get; set; } = new List<UserChannel>();

	/// <summary>
	/// Represents if the channel is the default channel in a server.
	/// </summary>
	public bool IsDefault { get; set; }

    /// <summary>
    /// Represents if a channel has been deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}
