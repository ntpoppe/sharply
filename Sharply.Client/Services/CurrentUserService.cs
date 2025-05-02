using System;
using System.Threading.Tasks;
using Sharply.Client.Interfaces;
using Sharply.Client.Models;

namespace Sharply.Client.Services;

public class CurrentUserService(IApiService apiService) : ICurrentUserService
{
    public CurrentUser? CurrentUser { get; private set; } = null!;

    public async Task<CurrentUser> InitializeUser(string token)
    {
        var userData = await apiService.GetCurrentUserData(token);
        if (userData == null) throw new Exception("User data missing.");

        CurrentUser = CurrentUser.FromDto(userData);
        return CurrentUser;
    }

    public void ClearUser() => CurrentUser = null!;
}
