using System;
using System.Threading.Tasks;
using Sharply.Client.Interfaces;
using Sharply.Client.Models;
using Sharply.Shared.Models;

namespace Sharply.Client.Services;

public class UserService(ITokenStorageService _tokenStorageService, IApiService _apiService) : IUserService
{
    public CurrentUser? CurrentUser { get; private set; } = null!;

    public async Task<CurrentUser> InitializeCurrentUserAsync()
    {
        var userData = await GetCurrentUserDataAsync();
        if (userData == null) throw new Exception("User data missing.");

        CurrentUser = CurrentUser.FromDto(userData);
        return CurrentUser;
    }

    public async Task<UserDto?> GetCurrentUserDataAsync()
    {
        var token = _tokenStorageService.TryLoadToken();

        var user = await _apiService.GetCurrentUserData(token);
        if (user == null)
            throw new InvalidOperationException("user was null");

        return user;
    }

    public async Task<bool> CheckUserChannelAccessAsync(int userId, int channelId)
    {
        var token = _tokenStorageService.TryLoadToken();
        return await _apiService.CheckUserChannelAccess(token, userId, channelId);
    }

    public void ClearCurrentUser() => CurrentUser = null!;
}
