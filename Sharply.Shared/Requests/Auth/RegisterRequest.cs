namespace Sharply.Shared.Requests;

/// <summary>
/// Represents the data required for user registration.
/// </summary>
public class RegisterRequest
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
