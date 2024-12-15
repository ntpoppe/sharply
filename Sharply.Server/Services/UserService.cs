using Sharply.Server.Data;
using Sharply.Server.Models;

namespace Sharply.Server.Services;
public class UserService
{
    private readonly SharplyDbContext _context;

    public UserService(SharplyDbContext context)
        => _context = context;

    public async Task AddUserToServerAsync(string userId, string serverId)
    {
        var server = await _context.Servers.FindAsync(int.Parse(serverId));
        if (server == null) throw new Exception("Server not found");

        var userServer = new UserServer
        {
            UserId = int.Parse(userId),
            ServerId = server.Id
        };

        _context.UserServers.Add(userServer);
        await _context.SaveChangesAsync();
    }
}
