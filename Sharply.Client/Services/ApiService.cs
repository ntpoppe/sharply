﻿using Sharply.Client.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class ApiService
{
    private readonly HttpClient _client;

    public ApiService()
    {

		// TODO: This needs to be disabled upon "production".
		var handler = new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true
		};

        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:8001/")
        };
    }

    public async Task<User?> RegisterAsync(string username, string password)
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
                return new User
                {
                    Username = registerResponse.Username,
                    Token = registerResponse.Token
                };
            }
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        throw new Exception($"Registration failed. Please try again. {errorMessage}");
    }

    public async Task<User?> LoginAsync(string username, string password)
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
                return new User
                {
                    Username = loginResponse.Username,
                    Token = loginResponse.Token
                };
            }
        }

        throw new Exception("Login failed. Please check your credientials.");
    }
}
