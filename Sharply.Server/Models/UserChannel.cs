namespace Sharply.Server.Models;

/// <summary>
/// A join table for users and channels.
/// </summary>

public class UserChannel
{
    public required int UserId { get; set; }
    public required int ChannelId { get; set; }
    public User User { get; set; } = null!;
    public Channel Channel { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
