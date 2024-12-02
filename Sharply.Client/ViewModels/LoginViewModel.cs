using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    #region Constructors

    public LoginViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    #endregion

    #region Properties

    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task LoginAsync()
    {
        // Call API service to authenticate
        var apiService = new ApiService();

        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                throw new Exception("Username or password cannot be empty.");

            // Attempt to log in
            var user = await apiService.LoginAsync(Username, Password);

            if (user != null)
                OnLoginSuccess(user);
            else
                OnLoginFailed("Login failed. Please check your credentials.");
        }
        catch (HttpRequestException ex)
        {
            // Handle network or server-related errors
            OnLoginFailed($"Unable to connect to the server. {ex}");
        }
        catch (Exception ex)
        {
            // Handle other errors (e.g., validation or unexpected exceptions)
            OnLoginFailed(ex.Message);
        }

    }

    private void OnLoginSuccess(User user)
    {
        // Example: Navigate to the main view
        Debug.WriteLine($"Login successful. Welcome, {user.Username}!");
        _navigationService.NavigateTo<MainViewModel>();
    }

    private void OnLoginFailed(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Debug.WriteLine($"Login failed: {errorMessage}");
    }

    #endregion
}
