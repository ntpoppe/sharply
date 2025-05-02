using System;
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

    public async Task<ServerViewModel> CreateServer(int userId, string name)
    {
        var request = new CreateServerRequest()
        {
            OwnerId = userId,
            Name = name
        };

        var token = _tokenStorageService.LoadToken();
        if (token == null)
            throw new InvalidOperationException("token was null");

        var newServer = await _apiService.CreateServerAsync(token, request);
        return newServer;
    }
}
