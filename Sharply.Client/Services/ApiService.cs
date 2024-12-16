using Sharply.Client.ViewModels;
using Sharply.Shared;
using Sharply.Shared.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);

        if (response.IsSuccessStatusCode)
        {
            var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            if (registerResponse != null)
            {

                await _tokenStorageService.SaveTokenAsync(registerResponse.Token);

                return new UserViewModel
                {
                    Id = registerResponse.Id,
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

        var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse != null)
            {
                await _tokenStorageService.SaveTokenAsync(loginResponse.Token);

                return new UserViewModel
                {
                    Id = loginResponse.Id,
                    Username = loginResponse.Username,
                };
            }
        }

        throw new Exception("Login failed. Please check your credentials.");
    }

    public async Task<List<ServerViewModel>> GetServersAsync(string tokenString)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        var response = await _client.GetAsync("api/servers/get-user-servers");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ServerViewModel>>>();

            if (result != null && result.Success)
            {
                var viewModels = result.Data?.Select(serverDto => new ServerViewModel
                {
                    Id = serverDto.Id,
                    Name = serverDto.Name,
                    Channels = serverDto.Channels.Select(channelDto => new ChannelViewModel
                    {
                        Id = channelDto.Id,
                        Name = channelDto.Name,
                        ServerId = channelDto.ServerId,
                        Messages = channelDto.Messages.Select(messageDto => new MessageViewModel
                        {
                            Username = messageDto.Username,
                            Content = messageDto.Content,
                            Timestamp = messageDto.Timestamp,
                        }).ToList()
                    }).ToList()
                }).ToList();

                return viewModels ?? new List<ServerViewModel>();
            }
            else
            {
                throw new Exception(result?.Error ?? "Unknown error occurred.");
            }
        }

        throw new Exception($"Server returned {response.StatusCode} in GetServersAsync()");
    }
}
