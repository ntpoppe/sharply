using Sharply.Client.Interfaces;
using System.Threading.Tasks;

public class TokenStorageService : ITokenStorageService
{
    private string? _token;

    public void SaveToken(string token)
    {
        if (token == null)
            SecureStorageService.ClearToken();
        else
            SecureStorageService.SaveToken(token);
    }
    public async Task SaveTokenAsync(string token)
    {
        if (token == null)
            SecureStorageService.ClearToken();
        else
            await SecureStorageService.SaveTokenAsync(token);
    }

    public string? LoadToken() => _token ??= SecureStorageService.LoadToken();
}

