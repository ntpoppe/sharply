using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Models;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;

/// <summary>
/// Handles operations related to servers, such as creating, retrieving, updating, or deleting servers and their related data (e.g., channels within servers).
/// </summary>
public class UserService
{
    private readonly SharplyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ServerService _serverService;

    public UserService(SharplyDbContext context, IMapper mapper, ServerService serverService)
    {
        _context = context;
        _mapper = mapper;
        _serverService = serverService;
    }

    /// <summary>
    /// Adds a user to a server.
    /// </summary>
    public async Task AddUserToServerAsync(int userId, int serverId, CancellationToken cancellationToken = default)
    {
        var serverExists = await _context.Servers.AnyAsync(s => s.Id == serverId, cancellationToken);
        if (!serverExists)
            throw new Exception("Server not found");

        var userServer = new UserServer
        {
            UserId = userId,
            ServerId = serverId
        };

        _context.UserServers.Add(userServer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all servers for a user.
    /// </summary>
    public async Task<List<ServerDto>> GetServersForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var servers = await _serverService.GetServersWithChannelsForUserAsync(userId, cancellationToken);
        return servers;
    }

    /// <summary>
    /// Retrieves all channels a user belongs to.
    /// </summary>
    public async Task<List<ChannelDto>> GetChannelsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var channels = await _context.UserChannels
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.Channel)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ChannelDto>>(channels);
    }
}
