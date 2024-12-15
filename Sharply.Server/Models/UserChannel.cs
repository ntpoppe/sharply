﻿namespace Sharply.Server.Models;

/// <summary>
/// A join table for users and channels.
/// </summary>

public class UserChannel
{
    public int UserId { get; set; }
    public int ChannelId { get; set; }
    public User User { get; set; } = null!;
    public Channel Channel { get; set; } = null!;
}