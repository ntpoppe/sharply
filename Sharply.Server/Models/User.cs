namespace Sharply.Server.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    /// <remarks>
    /// This property is typically used as the primary key in the database.
    /// </remarks>
    public int Id { get; set; }


    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    /// <remarks>
    /// The username is required and must be unique for each user.
    /// </remarks>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the hashed version of the user's password.
    /// </summary>
    /// <remarks>
    /// This value is generated using a secure hashing algorithm.
    /// It is nullable to allow for scenarios where a password might not initially be set.
    /// </remarks>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// A collection of messages sent by the user. Navigation property.
    /// </summary>
    public ICollection<Message>? Messages { get; set; }

    /// <summary>
    /// A collection of servers the user is in.
    /// </summary>
    public ICollection<UserServer> UserServers { get; set; } = new List<UserServer>();

    /// <summary>
    /// A collection of channels the user is in.
    /// </summary>
    public ICollection<UserChannel> UserChannels { get; set; } = new List<UserChannel>();
}
