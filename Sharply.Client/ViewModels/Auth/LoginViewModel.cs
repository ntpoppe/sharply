﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;

namespace Sharply.Client.ViewModels;

public partial class LoginViewModel : ViewModelBase, INavigable
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    #region Constructors

    public LoginViewModel(IApiService apiService, INavigationService navigationService)
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

        try
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                throw new Exception("Username or password cannot be empty.");

            var user = await _apiService.LoginAsync(Username, Password);

            if (user != null)
                await OnLoginSuccess(user);
            else
                OnLoginFailed("Check your credentials.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex);
            OnLoginFailed($"Unable to connect to the server.");
        }
        catch (Exception ex)
        {
            OnLoginFailed($"Login failed. {ex.Message} Please try again.");
        }
    }

    [RelayCommand]
    private void GoToRegister()
        => _navigationService.NavigateTo<RegisterViewModel>();

    public void OnNavigatedTo(object? parameter)
    {
        // Do nothing, may be useful in the future
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
        => ErrorMessage = errorMessage;

    #endregion
}
