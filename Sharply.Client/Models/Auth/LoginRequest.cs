namespace Sharply.Client.Models;

/// <summary>
/// Represents the data required for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The password of the user.
    /// </summary>
    public required string Password { get; set; }
}
