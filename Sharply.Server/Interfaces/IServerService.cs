using Sharply.Shared.Models;
using Sharply.Shared.Requests;

namespace Sharply.Server.Interfaces;

public interface IServerService
{
	Task<ServerDto> CreateServer(CreateServerRequest request, CancellationToken cancellationToken = default);
    Task<List<ServerDto>> GetServersWithChannelsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<ChannelDto>> GetChannelsForServerAsync(int serverId, CancellationToken cancellationToken = default);
}

