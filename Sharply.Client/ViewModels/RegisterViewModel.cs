using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Models;
using Sharply.Client.Services;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    #region Constructors

    public RegisterViewModel(INavigationService navigationService)
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
    private string? _confirmPassword;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        // Call API service to authenticate
        var apiService = new ApiService();

        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                throw new Exception("Username or password cannot be empty.");

            if (Password != ConfirmPassword)
                throw new Exception("Passwords must match.");

            // Attempt to register 
            var user = await apiService.RegisterAsync(Username, Password);

            if (user != null)
                OnRegisterSuccess(user);
            else
                OnRegisterFailed("Login failed. Please check your credentials.");
        }
        catch (HttpRequestException ex)
        {
            // Handle network or server-related errors
            OnRegisterFailed($"Unable to connect to the server. {ex}");
        }
        catch (Exception ex)
        {
            // Handle other errors (e.g., validation or unexpected exceptions)
            OnRegisterFailed(ex.Message);
        }

    }

    [RelayCommand]
    private void GoToLogin()
    {
        _navigationService.NavigateTo<LoginViewModel>();
    }

    private void OnRegisterSuccess(User user)
    {
        // Example: Navigate to the main view
        Debug.WriteLine($"Registration successful. Welcome, {user.Username}!");
        _navigationService.NavigateTo<MainViewModel>();
    }

    private void OnRegisterFailed(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Debug.WriteLine($"Registration failed: {errorMessage}");
    }

    #endregion
}
