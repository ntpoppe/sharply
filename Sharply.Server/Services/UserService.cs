using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Models;

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

    public async Task<List<Channel>> GetChannelsForUserAsync(int userId)
        => await _context.UserChannels
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.Channel)
            .ToListAsync();
}
