using Sharply.Shared.Models;

namespace Sharply.Server.Interfaces;

public interface IChannelService
{
    Task<List<MessageDto>> GetMessagesForChannelAsync(int channelId, CancellationToken cancellationToken = default);
    Task AddUserToChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default);
    Task<bool> CheckUserChannelAccessAsync(int channelId, int userId);
}


