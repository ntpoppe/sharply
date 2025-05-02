using System.Collections.Generic;
using System.Threading.Tasks;
using Sharply.Client.Interfaces;
using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;

namespace Sharply.Client.Services;

public class ServerService : IServerService
{
    private readonly IApiService _apiService;
    private readonly ITokenStorageService _tokenStorageService;

    public ServerService(IApiService apiService, ITokenStorageService tokenStorageService)
    {
        _apiService = apiService;
        _tokenStorageService = tokenStorageService;
    }

    public async Task<List<ServerViewModel>> GetServersAsync()
    {
        var token = _tokenStorageService.TryLoadToken();
        return await _apiService.GetServersAsync(token);
    }

    public async Task<ServerViewModel> CreateServerAsync(int userId, string name)
    {
        var request = new CreateServerRequest()
        {
            OwnerId = userId,
            Name = name
        };

        var token = _tokenStorageService.TryLoadToken();

        var newServer = await _apiService.CreateServerAsync(token, request);
        return newServer;
    }

    public async Task DeleteServerAsync(int serverId)
    {
        var token = _tokenStorageService.TryLoadToken();
        await _apiService.SoftDeleteServerAsync(token, serverId);
    }

    public async Task<(int? ServerId, string? Error)> JoinServerAsync(JoinServerRequest request)
    {
        var token = _tokenStorageService.TryLoadToken();
        var response = await _apiService.JoinServerAsync(token, request);

        if (!response.Success || response.Data == null)
            return (null, response.Error ?? "An unknown error occurred.");

        return (response.Data.Id, null);
    }
}
