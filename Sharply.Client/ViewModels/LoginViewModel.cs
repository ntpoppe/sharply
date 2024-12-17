﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly ApiService _apiService;
    private readonly INavigationService _navigationService;

    #region Constructors

    public LoginViewModel(ApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task LoginAsync()
    {
        var handler = new HttpClientHandler
        {
            // TODO: This needs to be disabled upon "production".
            ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true
        };

        // Call API service to authenticate
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                throw new Exception("Username or password cannot be empty.");

            // Attempt to log in
            var user = await _apiService.LoginAsync(Username, Password);

            if (user != null)
                await OnLoginSuccess(user);
            else
                OnLoginFailed("Login failed. Please check your credentials.");
        }
        catch (HttpRequestException ex)
        {
            OnLoginFailed($"Unable to connect to the server. {ex}");
        }
        catch (Exception ex)
        {
            OnLoginFailed(ex.Message);
        }
    }

    [RelayCommand]
    private void GoToRegister()
    {
        _navigationService.NavigateTo<RegisterViewModel>();
    }

    private async Task OnLoginSuccess(UserViewModel user)
    {
        var mainViewModel = _navigationService.NavigateTo<MainViewModel>();
        if (mainViewModel is MainViewModel vm)
        {
            await vm.LoadInitialData();
        }
    }

    private void OnLoginFailed(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    #endregion
}
