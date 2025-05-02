using System;
using System.Linq;
using Avalonia.Media;
using Sharply.Shared.Models;

namespace Sharply.Client.Models;

public class CurrentUser
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? Nickname { get; set; }
    public SolidColorBrush ProfileBrush { get; set; }
    public string Status { get; set; } = "Online";
    public string Initials => string.Join("", Username.Split(' ').Select(s => s[0])).ToUpper();


    public CurrentUser(UserDto dto)
    {
        Id = dto.Id;
        Username = dto.Username ?? throw new ArgumentNullException(nameof(dto.Username));
        Nickname = dto.Nickname;
        ProfileBrush = GenerateBrushFromUsername(dto.Username);
    }

    public static CurrentUser FromDto(UserDto dto)
        => new CurrentUser(dto);

    private SolidColorBrush GenerateBrushFromUsername(string username)
    {
        // Simple hash-based color generator
        int hash = username.GetHashCode();
        byte r = (byte)((hash >> 16) & 0xFF);
        byte g = (byte)((hash >> 8) & 0xFF);
        byte b = (byte)(hash & 0xFF);

        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }
}
