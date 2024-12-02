namespace Sharply.Client.Models;

/// <summary>
/// Represents the response returned after a successful user registration.
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// The username of the registered user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The password of the registered user.
    /// </summary>
    public required string Token { get; set; }
}
