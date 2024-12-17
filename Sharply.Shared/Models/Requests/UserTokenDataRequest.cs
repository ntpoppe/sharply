namespace Sharply.Shared.Requests;

public class UserTokenDataRequest
{
    public required int UserId { get; set; }
    public required string Username { get; set; }
}

