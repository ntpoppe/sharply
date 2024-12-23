using Sharply.Shared.Models;
using System;

namespace Sharply.Client.Models;

public class CurrentUser
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? Nickname { get; set; }

    public CurrentUser(UserDto dto)
    {
        Id = dto.Id;
        Username = dto.Username ?? throw new ArgumentNullException(nameof(dto.Username));
        Nickname = dto.Nickname;
    }

    public static CurrentUser FromDto(UserDto dto)
        => new CurrentUser(dto);
}
