namespace Sharply.Shared.Requests;

public class CreateServerRequest()
{
    public required int OwnerId { get; set; }
    public required string Name { get; set; }
}
