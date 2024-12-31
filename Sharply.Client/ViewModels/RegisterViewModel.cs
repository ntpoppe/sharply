using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class RegisterViewModel : ViewModelBase, INavigable
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    #region Constructors

    public RegisterViewModel(IApiService apiService, INavigationService navigationService)
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
    private string? _confirmPassword;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task RegisterAsync()
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
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                throw new Exception("Username or password cannot be empty.");

            if (Password != ConfirmPassword)
                throw new Exception("Passwords must match.");

            // Attempt to register 
            var user = await _apiService.RegisterAsync(Username, Password);

            if (user != null)
                await OnRegisterSuccess(user);
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

	public void OnNavigatedTo(object? parameter)
	{
		// Do nothing, may be useful in the future
	}

    private async Task OnRegisterSuccess(UserViewModel user)
    {
        // Example: Navigate to the main view
        var mainViewModel = _navigationService.NavigateTo<MainViewModel>();
        if (mainViewModel is MainViewModel vm)
        {
            await vm.LoadInitialData();
        }
    }

    private void OnRegisterFailed(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Debug.WriteLine($"Registration failed: {errorMessage}");
    }

    #endregion
}
