namespace Sharply.Server.Models;

/// <summary>
/// Represents a server in the system.
/// </summary>
public class Server
{
    /// <summary>
    /// Gets or sets the unique id of the server.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the server.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the owner user id of the server.
    /// </summary>
    public required int OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the invite code for the server.
    /// </summary>
    public required string InviteCode { get; set; }

    /// <summary>
    /// Gets or sets a collection of channels contained by the server.
    /// </summary>
    public ICollection<Channel> Channels { get; set; } = new List<Channel>();

    /// <summary>
    /// A collection of users that are in this server.
    /// </summary>
    public ICollection<UserServer> UserServers { get; set; } = new List<UserServer>();

    /// <summary>
    /// Represents if a server is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

}

