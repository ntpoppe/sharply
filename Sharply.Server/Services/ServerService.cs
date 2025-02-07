using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Shared.Models;
using Sharply.Shared.Requests;

namespace Sharply.Server.Services;

/// <summary>
/// Focuses on user-centric operations, such as managing user-server relationships, 
/// fetching user-specific data (e.g., their channels or servers), and updating user information.
/// </summary>
public class ServerService : IServerService
{
    private readonly ISharplyContextFactory<SharplyDbContext> _contextFactory;
    private readonly IMapper _mapper;

    public ServerService(ISharplyContextFactory<SharplyDbContext> contextFactory, IMapper mapper)
    {
        _contextFactory = contextFactory;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new server.
    /// </summary>
    public async Task<ServerDto> CreateServerAsync(CreateServerRequest request, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var newServer = new Models.Server
        {
            OwnerId = request.OwnerId,
            Name = request.Name,
            InviteCode = Guid.NewGuid().ToString().Substring(0, 8)
        };

        context.Servers.Add(newServer);
        await context.SaveChangesAsync(cancellationToken);

        var defaultChannel = new Channel
        {
            ServerId = newServer.Id,
            Name = "general",
            IsDefault = true
        };

        context.Channels.Add(defaultChannel);
        await context.SaveChangesAsync(cancellationToken);

        var newUserServer = new UserServer
        {
            UserId = request.OwnerId,
            ServerId = newServer.Id
        };

        var newUserChannel = new UserChannel
        {
            ChannelId = defaultChannel.Id,
            UserId = request.OwnerId,
            IsActive = true
        };

        context.UserServers.Add(newUserServer);
        context.UserChannels.Add(newUserChannel);
        await context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ServerDto>(newServer);
    }

    /// <summary>
    /// Soft deletes a server.
    /// </summary>
    public async Task SoftDeleteServerAsync(int serverId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var server = await context.Servers.FirstOrDefaultAsync(s => s.Id == serverId, cancellationToken);
        if (server == null)
            throw new InvalidOperationException($"Attempted to delete server with id {serverId}. Server does not exist.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Soft delete all related channels
            await context.Channels
                .Where(c => c.ServerId == serverId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDeleted, true), cancellationToken);

            // Soft delete user-server relationships
            await context.UserServers
                .Where(us => us.ServerId == serverId)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.IsActive, false), cancellationToken);

            // Soft delete user-channel relationships
            await context.UserChannels
                .Where(uc => context.Channels.Any(c => c.ServerId == serverId && c.Id == uc.ChannelId))
                .ExecuteUpdateAsync(s => s.SetProperty(uc => uc.IsActive, false), cancellationToken);

            // Soft delete the server
            server.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Adds a user to a server.
    /// </summary>
    public async Task<bool> AddUserToServerAsync(int userId, int serverId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var exists = await context.UserServers.AnyAsync(us => us.UserId == userId && us.ServerId == serverId, cancellationToken);
        if (exists)
            return false;

        var newUserServer = new UserServer { UserId = userId, ServerId = serverId };
        context.UserServers.Add(newUserServer);

        var defaultChannel = context.Channels
            .Where(c => c.ServerId == serverId)
            .FirstOrDefault(c => c.IsDefault == true);

        if (defaultChannel == null)
            throw new InvalidOperationException("Server doesn't have a default channel.");

        var newUserChannel = new UserChannel { UserId = userId, ChannelId = defaultChannel.Id };
        context.UserChannels.Add(newUserChannel);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveUserFromServerAsync(int userId, int serverId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var isUserInServer = await context.UserServers.AnyAsync(us => us.UserId == userId && us.ServerId == serverId, cancellationToken);
        if (!isUserInServer)
            return false;

        var channelIds = await context.Servers
              .Where(s => s.Id == serverId)
              .SelectMany(s => s.Channels)
              .Select(c => c.Id)
              .ToListAsync(cancellationToken);

        var userChannels = await context.UserChannels
            .Where(uc => uc.UserId == userId && channelIds.Contains(uc.ChannelId))
            .ToListAsync(cancellationToken);

        context.UserChannels.RemoveRange(userChannels);

        var userServer = await context.UserServers
        .FirstOrDefaultAsync(us => us.UserId == userId && us.ServerId == serverId, cancellationToken);

        if (userServer != null)
            context.UserServers.Remove(userServer);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Retrieves servers with associated channels for a user.
    /// </summary>
    public async Task<List<ServerDto>> GetServersWithChannelsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        // Retrieve all servers and their channels/messages the user has access to
        var servers = await context.Servers
            .Include(s => s.Channels)
                .ThenInclude(c => c.Messages)
                    .ThenInclude(m => m.User) // fetch the user who sent the message
            .Where(s => s.IsDeleted == false)
            .Where(s => s.UserServers.Any(us => us.UserId == userId))
            .ToListAsync(cancellationToken);

        // Filter channels based on whether the user has access to them.
        foreach (var server in servers)
        {
            server.Channels = server.Channels
                .Where(c => c.IsDeleted == false)
                .Where(c => context.UserChannels.Any(uc => uc.UserId == userId && uc.ChannelId == c.Id && uc.IsActive == true))
                .ToList();
        }

        return _mapper.Map<List<ServerDto>>(servers);
    }

    /// <summary>
    /// Retrieves a server via an invite code.
    /// </summary>
    public async Task<ServerDto?> GetServerByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var server = await context.Servers.FirstOrDefaultAsync(s => s.InviteCode == inviteCode, cancellationToken);
        return _mapper.Map<ServerDto>(server);
    }

    /// <summary>
    /// Retrieves all channels for a given server.
    /// </summary>
    public async Task<List<ChannelDto>> GetChannelsForServerAsync(int serverId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateSharplyContext();

        var channels = await context.Channels
            .Where(c => c.ServerId == serverId)
            .Where(c => c.IsDeleted == false)
            .Include(c => c.Messages)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ChannelDto>>(channels);
    }
}

