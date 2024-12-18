using Sharply.Shared.Models;

namespace Sharply.Server.Interfaces;
public interface IUserService
{
    Task AddUserToServerAsync(int userId, int serverId, CancellationToken cancellationToken = default);
    Task<List<ServerDto>> GetServersForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<ChannelDto>> GetChannelsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<string?> GetUsernameFromId(int userId, CancellationToken cancellationToken = default);
}

