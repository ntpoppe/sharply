using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Sharply.Client.Interfaces;
using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;

namespace Sharply.Client.Services;

public class ServerService : IServerService
{
    private readonly IApiService _apiService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IMapper _mapper;

    public ServerService(IApiService apiService, ITokenStorageService tokenStorageService, IMapper mapper)
    {
        _apiService = apiService;
        _tokenStorageService = tokenStorageService;
        _mapper = mapper;
    }

    public async Task<List<ServerViewModel>> GetServersAsync()
    {
        var token = _tokenStorageService.TryLoadToken();
        var serverDtos = await _apiService.GetServersAsync(token);
        return _mapper.Map<List<ServerViewModel>>(serverDtos);
    }

    public async Task<ServerViewModel> CreateServerAsync(int userId, string name)
    {
        var request = new CreateServerRequest()
        {
            OwnerId = userId,
            Name = name
        };

        var token = _tokenStorageService.TryLoadToken();
        var newServerDto = await _apiService.CreateServerAsync(token, request);
        return _mapper.Map<ServerViewModel>(newServerDto);
    }

    public async Task DeleteServerAsync(int serverId)
    {
        var token = _tokenStorageService.TryLoadToken();
        await _apiService.SoftDeleteServerAsync(token, serverId);
    }

    public async Task<(int? ServerId, string? Error)> JoinServerAsync(string inviteCode)
    {
        var token = _tokenStorageService.TryLoadToken();
        var request = new JoinServerRequest { InviteCode = inviteCode };

        try
        {
            var serverDto = await _apiService.JoinServerAsync(token, request);
            return (serverDto.Id, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<bool> LeaveServerAsync(int serverId)
    {
        var token = _tokenStorageService.TryLoadToken();
        var request = new LeaveServerRequest { ServerId = serverId };
        return await _apiService.LeaveServerAsync(token, request);
    }
}
