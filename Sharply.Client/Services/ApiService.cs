using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace Sharply.Client.Services;

public class ApiService
{
    private readonly HttpClient _client;
    private readonly TokenStorageService _tokenStorageService;

    public ApiService(HttpClient client, TokenStorageService tokenStorageService)
    {
        _client = client;
        _tokenStorageService = tokenStorageService;
    }

    public async Task<UserViewModel> RegisterAsync(string username, string password)
    {
        var registerRequest = new RegisterRequest
        {
            Username = username,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("auth/register", registerRequest);

        if (response.IsSuccessStatusCode)
        {
            var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            if (registerResponse != null)
            {

                await _tokenStorageService.SaveTokenAsync(registerResponse.Token);

                return new UserViewModel
                {
                    Username = registerResponse.Username,
                };
            }
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        throw new Exception($"Registration failed. Please try again. {errorMessage}");
    }

    public async Task<UserViewModel> LoginAsync(string username, string password)
    {
        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse != null)
            {
                await _tokenStorageService.SaveTokenAsync(loginResponse.Token);

                return new UserViewModel
                {
                    Username = loginResponse.Username,
                };
            }
        }

        throw new Exception("Login failed. Please check your credentials.");
    }
}
