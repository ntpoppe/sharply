using System.Threading.Tasks;
using Sharply.Client.ViewModels;

namespace Sharply.Client.Interfaces;

public interface IServerService
{
    Task<ServerViewModel> CreateServer(int userId, string name);
}
