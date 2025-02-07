using Sharply.Shared.Models;
using Sharply.Shared.Requests;

namespace Sharply.Server.Interfaces;

public interface IServerService
{
	Task<ServerDto> CreateServerAsync(CreateServerRequest request, CancellationToken cancellationToken = default);
	Task SoftDeleteServerAsync(int serverId, CancellationToken cancellationToken = default);
    Task<List<ServerDto>> GetServersWithChannelsForUserAsync(int userId, CancellationToken cancellationToken = default);
	Task<bool> AddUserToServerAsync(int userId, int serverId, CancellationToken cancellationToken = default);
	Task<bool> RemoveUserFromServerAsync(int userId, int serverId, CancellationToken cancellationToken = default);
	Task<ServerDto?> GetServerByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);
    Task<List<ChannelDto>> GetChannelsForServerAsync(int serverId, CancellationToken cancellationToken = default);
}

