using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;

/// <summary>
/// Manages operations specific to channels, such as retrieving messages, adding users to channels, or modifying channel properties.
/// </summary>
public class ChannelService : IChannelService
{
    private readonly ISharplyContextFactory<SharplyDbContext> _contextFactory;
    private readonly IMapper _mapper;

    public ChannelService(ISharplyContextFactory<SharplyDbContext> contextFactory, IMapper mapper)
    {
        _contextFactory = contextFactory;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves all messages for a specific channel.
    /// </summary>
    public async Task<List<MessageDto>> GetMessagesForChannelAsync(int channelId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var messages = await context.Messages
            .Include(m => m.User)
            .Where(m => m.ChannelId == channelId)
            .Where(m => m.IsDeleted == false)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<MessageDto>>(messages);
    }

    /// <summary>
    /// Adds a user to a specific channel.
    /// </summary>
    public async Task AddUserToChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var userChannel = new UserChannel
        {
            UserId = userId,
            ChannelId = channelId,
			IsActive = true
        };

        context.UserChannels.Add(userChannel);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a user has access to a channel.
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> CheckUserChannelAccessAsync(int channelId, int userId)
    {
        using var context = _contextFactory.CreateSharplyContext();
        return await context.UserChannels
            .AnyAsync(uc => uc.ChannelId == channelId && uc.UserId == userId && uc.IsActive == true);
    }
}
