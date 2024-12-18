namespace Sharply.Server.Models;

/// <summary>
/// A join table for users and servers.
/// </summary>

public class UserServer
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int ServerId { get; set; }
    public Server Server { get; set; }

    public bool IsActive { get; set; }
}

