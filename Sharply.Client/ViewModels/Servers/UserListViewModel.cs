using CommunityToolkit.Mvvm.ComponentModel;
using Sharply.Client.Interfaces;
using Sharply.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class UserListViewModel : ObservableObject
{
    private readonly IApiService _apiService;
	private readonly ITokenStorageService _tokenStorageService;

    public UserListViewModel(IApiService apiService, ITokenStorageService tokenStorageService)
    {
        _apiService = apiService;
		_tokenStorageService = tokenStorageService;
    }

	[ObservableProperty]
	private ObservableCollection<UserViewModel> _onlineUsers = new();
	
	private List<UserDto> _globalOnlineUsers = new(); // should change this to members in server

	public async Task OnOnlineUsersUpdatedAsync(List<UserDto> userDtos, ChannelViewModel selectedChannel)
    {
        _globalOnlineUsers = userDtos;
        await UpdateOnlineUsersForCurrentChannel(selectedChannel);
    }

    public async Task UpdateOnlineUsersForCurrentChannel(ChannelViewModel selectedChannel)
    {
        if (selectedChannel == null || selectedChannel.Id == null) return;

        var token = _tokenStorageService.LoadToken();
        if (token == null)
            throw new Exception("Token was null");

        var channelId = selectedChannel.Id.Value;
        var usersForChannel = new List<UserDto>();

        foreach (var user in _globalOnlineUsers)
        {
            bool hasAccess = await _apiService.DoesUserHaveAccessToChannel(token, user.Id, channelId);
            if (hasAccess)
            {
                usersForChannel.Add(user);
            }
        }

        OnlineUsers = new ObservableCollection<UserViewModel>(
            usersForChannel.Select(dto => new UserViewModel { Id = dto.Id, Username = dto.Username, Nickname = dto.Nickname })
        );
    }
}
