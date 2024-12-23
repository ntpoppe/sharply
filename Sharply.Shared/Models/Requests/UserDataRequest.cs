namespace Sharply.Shared.Requests;

public class UserDataRequest
{
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public string? Nickname { get; set; }
}

