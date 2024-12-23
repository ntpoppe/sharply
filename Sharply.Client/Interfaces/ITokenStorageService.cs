using System.Threading.Tasks;

namespace Sharply.Client.Interfaces;

public interface ITokenStorageService
{
    void SaveToken(string token);
    Task SaveTokenAsync(string token);
    string? LoadToken();
    void ClearToken();
}
