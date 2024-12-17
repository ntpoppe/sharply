using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Models;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;

/// <summary>
/// Manages operations specific to channels, such as retrieving messages, adding users to channels, or modifying channel properties.
/// </summary>
public class ChannelService
{
    private readonly SharplyDbContext _context;
    private readonly IMapper _mapper;

    public ChannelService(SharplyDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves all messages for a specific channel.
    /// </summary>
    public async Task<List<MessageDto>> GetMessagesForChannelAsync(int channelId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Include(m => m.User)
            .Where(m => m.ChannelId == channelId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<MessageDto>>(messages);
    }

    /// <summary>
    /// Adds a user to a specific channel.
    /// </summary>
    public async Task AddUserToChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        var userChannel = new UserChannel
        {
            UserId = userId,
            ChannelId = channelId
        };

        _context.UserChannels.Add(userChannel);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

