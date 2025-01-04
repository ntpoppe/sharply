using System.Threading.Tasks;

namespace Sharply.Client.Interfaces;

public interface IServerService
{
	Task CreateServer(int userId, string name);
}
