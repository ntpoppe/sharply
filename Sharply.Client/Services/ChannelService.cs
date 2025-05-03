using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sharply.Client.Interfaces;
using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;

namespace Sharply.Client.Services;

public class ChannelService : IChannelService
{
    private readonly IApiService _apiService;
    private readonly ITokenStorageService _tokenStorageService;

    public ChannelService(IApiService apiService, ITokenStorageService tokenStorageService)
    {
        _apiService = apiService;
        _tokenStorageService = tokenStorageService;
    }

    public async Task<List<MessageViewModel>> GetMessagesForChannel(int channelId)
    {
        var token = _tokenStorageService.TryLoadToken();
        if (token == null)
            throw new InvalidOperationException("token was null in GetMessagesForChannel");

        return await _apiService.GetMessagesForChannel(token, channelId);
    }
}
