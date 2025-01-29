using Sharply.Shared.Models;

namespace Sharply.Shared.Requests;

public class JoinServerResponse
{
	public required ServerDto? Server { get; set; }
	public required string Message { get; set; }
}
