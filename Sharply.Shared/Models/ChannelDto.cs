using Sharply.Shared.Models;

public class ChannelDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int ServerId { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
}

