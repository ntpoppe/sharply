namespace Sharply.Shared.Requests;

/// <summary>
/// Represents the response returned after a successful login.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The Id of the authenticated user.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// The username of the authenticated user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The JSON Web Token for the authenticated user.
    /// </summary>
    public required string Token { get; set; }
}
