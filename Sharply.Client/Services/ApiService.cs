﻿using AutoMapper;
using Sharply.Client.Interfaces;
using Sharply.Client.ViewModels;
using Sharply.Shared;
using Sharply.Shared.Models;
using Sharply.Shared.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Sharply.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _client;
    private readonly ITokenStorageService _tokenStorageService;
	private readonly IMapper _mapper;

    public ApiService(HttpClient client, ITokenStorageService tokenStorageService, IMapper mapper)
    {
        _client = client;
        _tokenStorageService = tokenStorageService;
		_mapper = mapper;
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

        throw new Exception("Check your credentials.");
    }

	public async Task<ServerViewModel> CreateServerAsync(string tokenString, CreateServerRequest request)
	{
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
		var response = await _client.PostAsJsonAsync("api/servers/create-server", request);

		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<ApiResponse<ServerDto>>();

			if (result != null && result.Success)
			{
				var newServer = _mapper.Map<ServerViewModel>(result.Data);
				return newServer;
			}
			else
            {
                throw new Exception(result?.Error ?? "Unknown error occurred.");
            }
		}

		throw new Exception($"Server returned {response.StatusCode} in CreateServerAsync()");
	}

    public async Task<List<ServerViewModel>> GetServersAsync(string tokenString)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        var response = await _client.GetAsync("api/servers/get-user-servers");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ServerDto>>>();

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

    public async Task<List<MessageViewModel>> GetMessagesForChannel(string tokenString, int channelId)
    {
        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
            var response = await _client.GetAsync($"api/channels/{channelId}/get-messages");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<MessageDto>>>();

                if (result != null && result.Success)
                {
                    var viewModels = result.Data?.Select(messageDto => new MessageViewModel
                    {
                        Username = messageDto.Username,
                        Content = messageDto.Content,
                        Timestamp = messageDto.Timestamp
                    }).ToList();

                    return viewModels ?? new List<MessageViewModel>();
                }
            }

            throw new Exception($"Server returned {response.StatusCode} in GetServersAsync()");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured in GetMessagesForChannel(): " + ex);
            return new List<MessageViewModel>();
        }
    }

    public async Task<bool> CheckUserChannelAccess(string tokenString, int userId, int channelId)
    {
        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
            var response = await _client.GetAsync($"api/channels/{channelId}/{userId}/is-user-accessible");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                if (result != null && result.Success)
                {
                    return result.Data;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occured in DoesUserHaveAccessToChannel()" + ex.Message);
            return false;
        }
    }

    public async Task<UserDto?> GetCurrentUserData(string tokenString)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        var response = await _client.GetAsync("api/users/get-user-data");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
            if (result != null && result.Success)
                return result.Data;
        }

        return null;
    }

	private ServerViewModel MapToServerViewModel(ServerDto serverDto)
	{
		return new ServerViewModel
		{
			Id = serverDto.Id,
			Name = serverDto.Name,
			Channels = serverDto.Channels.Select(MapToChannelViewModel).ToList()
		};
	}

	private ChannelViewModel MapToChannelViewModel(ChannelDto channelDto)
	{
		return new ChannelViewModel
		{
			Id = channelDto.Id,
			Name = channelDto.Name,
			ServerId = channelDto.ServerId,
			Messages = channelDto.Messages.Select(MapToMessageViewModel).ToList()
		};
	}

	private MessageViewModel MapToMessageViewModel(MessageDto messageDto)
	{
		return new MessageViewModel
		{
			Username = messageDto.Username,
			Content = messageDto.Content,
			Timestamp = messageDto.Timestamp
		};
	}

}
