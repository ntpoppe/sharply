using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Models;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;
public class UserService
{
    private readonly SharplyDbContext _context;

    public UserService(SharplyDbContext context)
        => _context = context;

    public async Task AddUserToServerAsync(int userId, int serverId)
    {
        var server = await _context.Servers.FindAsync(serverId);
        if (server == null) throw new Exception("Server not found");

        var userServer = new UserServer
        {
            UserId = userId,
            ServerId = server.Id
        };

        _context.UserServers.Add(userServer);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ServerDto>> GetServersForUserAsync(int userId)
    {
        var servers = await _context.Servers
             .Include(s => s.Channels)
             .ThenInclude(c => c.UserChannels)
             .Where(s => s.UserServers.Any(us => us.UserId == userId))
             .ToListAsync();

        var serverDtos = servers.Select(server => new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            Channels = server.Channels?
                .Where(c => c.UserChannels.Any(uc => uc.UserId == userId))
                .Select(channel => new ChannelDto
                {
                    Id = channel.Id,
                    Name = channel.Name,
                    ServerId = server.Id
                }).ToList() ?? new List<ChannelDto>()
        }).ToList();

        return serverDtos;
    }

    public async Task<List<Channel>> GetDBChannelsForUserAsync(int userId)
        => await _context.UserChannels
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.Channel)
            .ToListAsync();
}
