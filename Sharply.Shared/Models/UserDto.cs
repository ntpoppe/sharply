namespace Sharply.Shared.Models;

public class UserDto
{
    public required int Id { get; set; }
    public required string Username { get; set; }
    public string? Nickname { get; set; }

}
