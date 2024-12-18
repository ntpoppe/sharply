using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;

/// <summary>
/// Handles operations related to servers, such as creating, retrieving, updating, or deleting servers and their related data (e.g., channels within servers).
/// </summary>
public class UserService : IUserService
{
    private readonly ISharplyContextFactory<SharplyDbContext> _contextFactory;
    private readonly IMapper _mapper;
    private readonly IServerService _serverService;

    public UserService(ISharplyContextFactory<SharplyDbContext> contextFactory, IMapper mapper, IServerService serverService)
    {
        _contextFactory = contextFactory;
        _mapper = mapper;
        _serverService = serverService;
    }

    /// <summary>
    /// Adds a user to a server.
    /// </summary>
    public async Task AddUserToServerAsync(int userId, int serverId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();
        var serverExists = await context.Servers.AnyAsync(s => s.Id == serverId && s.IsDeleted == false, cancellationToken);
        if (!serverExists)
            throw new Exception("Server not found");

        var userServer = new UserServer
        {
            UserId = userId,
            ServerId = serverId
        };

        context.UserServers.Add(userServer);
        await context.SaveChangesAsync(cancellationToken);
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
        using var context = _contextFactory.CreateSharplyContext();
        var channels = await context.UserChannels
            .Where(uc => uc.UserId == userId)
            .Where(uc => uc.IsActive == true)
            .Select(uc => uc.Channel)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ChannelDto>>(channels);
    }

    public async Task<string?> GetUsernameFromId(int userId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        return await context.Users
            .Where(u => u.Id == userId)
            .Where(u => u.IsDeleted == false)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
