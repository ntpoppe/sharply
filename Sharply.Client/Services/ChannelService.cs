using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Sharply.Client.Interfaces;
using Sharply.Client.ViewModels;

namespace Sharply.Client.Services;

public class ChannelService : IChannelService
{
    private readonly IApiService _apiService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IMapper _mapper;

    public ChannelService(IApiService apiService, ITokenStorageService tokenStorageService, IMapper mapper)
    {
        _apiService = apiService;
        _tokenStorageService = tokenStorageService;
        _mapper = mapper;
    }

    public async Task<List<MessageViewModel>> GetMessagesForChannel(int channelId)
    {
        var token = _tokenStorageService.TryLoadToken();
        var messages = await _apiService.GetMessagesForChannel(token, channelId);
        return _mapper.Map<List<MessageViewModel>>(messages);
    }
}
