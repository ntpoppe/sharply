using Microsoft.EntityFrameworkCore;
using Sharply.Server.Data;
using Sharply.Server.Models;

namespace Sharply.Server.Services;
public class ServerService
{
    private readonly SharplyDbContext _context;

    public ServerService(SharplyDbContext context)
        => _context = context;

    /// <summary>
    /// Gets all of the channels for a server.
    /// </summary>
    /// <param name="serverId"></param>
    /// <returns></returns>
    public async Task<List<Channel>> GetChannelsForServerAsync(int serverId)
        => await _context.Channels
            .Where(c => c.ServerId == serverId)
            .ToListAsync();
}
