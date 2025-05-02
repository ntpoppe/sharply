using Sharply.Shared.Models;

namespace Sharply.Server.Interfaces;

public interface IMessageService
{
    Task<MessageDto> CreateMessage(int channelId, int userId, string context);
}
