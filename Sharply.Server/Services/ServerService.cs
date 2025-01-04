using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Models;
using Sharply.Server.Interfaces;
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
	public async Task<ServerDto> CreateServer(CreateServerRequest request, CancellationToken cancellationToken = default)
	{
		using var context = _contextFactory.CreateSharplyContext();

		var newServer = new Models.Server
		{
			OwnerId = request.OwnerId,
			Name = request.Name
		};

		context.Servers.Add(newServer);
		await context.SaveChangesAsync(cancellationToken);

		var defaultChannel = new Channel
		{
			ServerId = newServer.Id,
			Name = "general"
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
			UserId = request.OwnerId
		};

		context.UserServers.Add(newUserServer);
		context.UserChannels.Add(newUserChannel);
		await context.SaveChangesAsync(cancellationToken);

		return _mapper.Map<ServerDto>(newServer);
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
            .Where(s => s.UserServers.Any(us => us.UserId == userId))
            .ToListAsync(cancellationToken);

        // Filter channels based on whether the user has access to them.
        foreach (var server in servers)
        {
            server.Channels = server.Channels
                .Where(c => context.UserChannels.Any(uc => uc.UserId == userId && uc.ChannelId == c.Id))
                .ToList();
        }

        return _mapper.Map<List<ServerDto>>(servers);
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

