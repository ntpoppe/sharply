using Sharply.Server.Data;
using Sharply.Server.Models;

namespace Sharply.Server.Services;
public class ChannelService
{
    private readonly SharplyDbContext _context;

    public ChannelService(SharplyDbContext context)
        => _context = context;

    public async Task AddUserToChannel(int userId, int channelId)
    {
        var userChannel = new UserChannel
        {
            UserId = userId,
            ChannelId = channelId
        };

        _context.UserChannels.Add(userChannel);
        await _context.SaveChangesAsync();
    }
}

