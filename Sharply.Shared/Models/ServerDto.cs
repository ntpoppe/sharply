namespace Sharply.Shared.Models;

public class ServerDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int OwnerId { get; set; }
    public required string InviteCode { get; set; }
    public List<ChannelDto> Channels { get; set; } = new List<ChannelDto>();
}


