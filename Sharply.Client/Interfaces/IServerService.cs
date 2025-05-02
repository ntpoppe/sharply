using System.Collections.Generic;
using System.Threading.Tasks;
using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;

namespace Sharply.Client.Interfaces;

public interface IServerService
{
    Task<ServerViewModel> CreateServerAsync(int userId, string name);
    Task<List<ServerViewModel>> GetServersAsync();
    Task DeleteServerAsync(int serverId); 
    Task<(int? ServerId, string? Error)> JoinServerAsync(JoinServerRequest request);
}
