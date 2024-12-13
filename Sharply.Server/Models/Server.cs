namespace Sharply.Server.Models;

/// <summary>
/// Represents a server in the system.
/// </summary>
public class Server
{
    /// <summary>
    /// Gets or sets the unique id of the server.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the server.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets a collection of channels contained by the server.
    /// </summary>
    public ICollection<Channel>? Channels { get; set; }
}

