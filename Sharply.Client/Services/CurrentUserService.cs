using Sharply.Client.Interfaces;
using Sharply.Client.Models;
using System;
using System.Threading.Tasks;

namespace Sharply.Client.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IApiService _apiService;

    public CurrentUserService(ITokenStorageService tokenStorageService, IApiService apiService)
    {
        _tokenStorageService = tokenStorageService;
        _apiService = apiService;
    }

    public CurrentUser CurrentUser { get; private set; } = null!;

    public async Task<CurrentUser> InitializeUser(string token)
    {
        var userData = await _apiService.GetCurrentUserData(token);
        if (userData == null) throw new Exception("User data missing.");

        CurrentUser = CurrentUser.FromDto(userData);
        return CurrentUser;
    }

    public void ClearUser() => CurrentUser = null!;
}
