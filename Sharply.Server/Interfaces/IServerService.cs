using Sharply.Shared.Models;

namespace Sharply.Server.Interfaces;

public interface IServerService
{
    Task<List<ServerDto>> GetServersWithChannelsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<ChannelDto>> GetChannelsForServerAsync(int serverId, CancellationToken cancellationToken = default);
}

