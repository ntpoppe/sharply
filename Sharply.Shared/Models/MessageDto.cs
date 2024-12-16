namespace Sharply.Shared.Models;

public class MessageDto
{
    public required int Id { get; set; }
    public required string Content { get; set; }
    public required DateTime Timestamp { get; set; }
    public required int ChannelId { get; set; }
    public required int UserId { get; set; }

}
