namespace Sharply.Shared.Models;

/// <summary>
/// Represents the response returned after a successful registration.
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// The username of the registered user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The JSON Web Token for the registered user.
    /// </summary>
    public required string Token { get; set; }
}
