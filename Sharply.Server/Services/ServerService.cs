using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;

/// <summary>
/// Focuses on user-centric operations, such as managing user-server relationships, 
/// fetching user-specific data (e.g., their channels or servers), and updating user information.
/// </summary>
public class ServerService
{
    private readonly SharplyDbContext _context;
    private readonly IMapper _mapper;

    public ServerService(SharplyDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves servers with associated channels for a user.
    /// </summary>
    public async Task<List<ServerDto>> GetServersWithChannelsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Retrieve all servers and their channels/messages the user has access to
        var servers = await _context.Servers
            .Include(s => s.Channels)
                .ThenInclude(c => c.Messages)
                    .ThenInclude(m => m.User) // fetch the user who sent the message
            .Where(s => s.UserServers.Any(us => us.UserId == userId))
            .ToListAsync(cancellationToken);

        // Filter channels based on whether the user has access to them.
        foreach (var server in servers)
        {
            server.Channels = server.Channels
                .Where(c => _context.UserChannels.Any(uc => uc.UserId == userId && uc.ChannelId == c.Id))
                .ToList();
        }

        return _mapper.Map<List<ServerDto>>(servers);
    }

    /// <summary>
    /// Retrieves all channels for a given server.
    /// </summary>
    public async Task<List<ChannelDto>> GetChannelsForServerAsync(int serverId, CancellationToken cancellationToken = default)
    {
        var channels = await _context.Channels
            .Where(c => c.ServerId == serverId)
            .Include(c => c.Messages)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ChannelDto>>(channels);
    }
}

