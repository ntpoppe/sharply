using Moq;
using Moq.Protected;
using AutoMapper;
using NUnit.Framework;
using Sharply.Client.Services;
using Sharply.Client.AutoMapper;
using Sharply.Shared.Requests;
using Sharply.Shared.Models;
using Sharply.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Sharply.Client.Tests;

[TestFixture]
public class ApiServiceTests
{
	private Mock<HttpMessageHandler>? _mockHttpMessageHandler;
	private TokenStorageService? _tokenStorageService;
	private HttpClient? _httpClient;
	private ApiService? _apiService;
	private IMapper? _mapper;

	[SetUp]
	public void SetUp()
	{
		_mockHttpMessageHandler = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_mockHttpMessageHandler.Object)
		{
			BaseAddress = new Uri("https://localhost:9999/")
		};

		var mapperConfig = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>(); 
		});
		_mapper = mapperConfig.CreateMapper();

		_tokenStorageService = new TokenStorageService();
		_apiService = new ApiService(_httpClient, _tokenStorageService, _mapper);
	}

	[Test]
	public async Task RegisterAsync_SuccessfulResponse_ReturnsUser()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var expectedId = 1;
		var expectedUsername = "testuser";
		var expectedToken = "fake-jwt-token";

		var registerResponse = new RegisterResponse
		{
			Id = expectedId,
			Username = expectedUsername,
			Token = expectedToken
		};

		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("https://localhost:9999/api/auth/register")),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(registerResponse)
			});

		// Act
		var result = await _apiService.RegisterAsync("testuser", "password123");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(expectedUsername, Is.EqualTo(result.Username));
	}

	[Test]
	public void RegisterAsync_FailedResponse_ThrowsException()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var errorMessage = "User already exists";

		// Setup the mock to return a bad request response with the error message
		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("https://localhost:9999/api/auth/register")),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(errorMessage)
			});

		// Act & Assert
		var ex = Assert.ThrowsAsync<Exception>(async () =>
		{
			if (_apiService == null) return;
			await _apiService.RegisterAsync("existinguser", "password123");
		});

		if (ex == null) return;
		Assert.That(ex.Message, Does.Contain("Registration failed"));
		Assert.That(ex.Message, Does.Contain(errorMessage));
	}

	[Test]
	public async Task LoginAsync_SuccessfulResponse_ReturnsUser()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var expectedId = 1;
		var expectedUsername = "testuser";
		var expectedToken = "fake-jwt-token";

		var loginResponse = new LoginResponse
		{
			Id = expectedId,
			Username = expectedUsername,
			Token = expectedToken
		};

		// Setup the mock to return a successful response with the expected content
		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("https://localhost:9999/api/auth/login")),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(loginResponse)
			});

		// Act
		var result = await _apiService.LoginAsync("testuser", "password123");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(expectedUsername, Is.EqualTo(result.Username));
	}

	[Test]
	public void LoginAsync_FailedResponse_ThrowsException()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var errorMessage = "Invalid credentials";

		// Setup the mock to return an unauthorized response with the error message
		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("https://localhost:9999/api/auth/login")),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.Unauthorized,
				Content = new StringContent(errorMessage)
			});

		// Act & Assert
		var ex = Assert.ThrowsAsync<Exception>(async () =>
		{
			await _apiService.LoginAsync("nonexistentuser", "wrongpassword");
		});

		if (ex == null) return;
		Assert.That(ex.Message, Does.Contain("Check your credentials."));
	}

	[Test]
	public async Task GetServersAsync_ValidResponse_ReturnsServerViewModels()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var mockResponse = new ApiResponse<List<ServerDto>>
		{
			Success = true,
			Data = new List<ServerDto>
			{
				new ServerDto
				{
					Id = 1,
					Name = "Test Server",
					OwnerId = 1,
					Channels = new List<ChannelDto>
					{
						new ChannelDto
						{
							Id = 101,
							Name = "General",
							ServerId = 1,
							Messages = new List<MessageDto>
							{
								new MessageDto { Id = 1, UserId = 1, Username = "TestUser", Content = "Hello, World!", ChannelId = 1, Timestamp = DateTime.Now }
							}
						}
					}
				}
			}
		};

		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(mockResponse)
			});

		// Act
		var result = await _apiService.GetServersAsync("dummy_token");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result[0].Name, Is.EqualTo("Test Server"));
		Assert.That(result[0].Channels[0].Name, Is.EqualTo("General"));
	}
	
	[Test]
	public async Task GetMessagesForChannel_ValidResponse_ReturnsMessages()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var mockResponse = new ApiResponse<List<MessageDto>>
		{
			Success = true,
			Data = new List<MessageDto>
			{
				new MessageDto { Id = 1, UserId = 1, Username = "TestUser", Content = "Hello, World!", ChannelId = 1, Timestamp = DateTime.Now }
			}
		};

		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Get &&
					req.RequestUri == new Uri("https://localhost:9999/api/channels/1/get-messages")
				),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(mockResponse)
			});

		// Act
		var result = await _apiService.GetMessagesForChannel("dummy_token", 1);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result[0].Content, Is.EqualTo("Hello, World!"));
	}

	[Test]
	public async Task CheckUserChannelAccess_UserHasAccess_ReturnsTrue()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var mockResponse = new ApiResponse<bool>
		{
			Success = true,
			Data = true
		};

		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Get &&
					req.RequestUri == new Uri("https://localhost:9999/api/channels/456/123/is-user-accessible")
				),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(mockResponse)
			});

		// Act
		var result = await _apiService.CheckUserChannelAccess("dummy_token", 123, 456);

		// Assert
		Assert.That(result, Is.True);
	}

	[Test]
	public async Task GetCurrentUserData_ValidResponse_ReturnsUserDto()
	{
		if (_apiService == null) throw new Exception("_apiService was null");	

		// Arrange
		var mockResponse = new ApiResponse<UserDto>
		{
			Success = true,
			Data = new UserDto { Id = 1, Username = "TestUser" }
		};

		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Get &&
					req.RequestUri == new Uri("https://localhost:9999/api/users/get-user-data")
				),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(mockResponse)
			});

		// Act
		var result = await _apiService.GetCurrentUserData("dummy_token");

		// Assert
		if (result == null) throw new Exception("result was null");
		Assert.That(result.Username, Is.EqualTo("TestUser"));
	}

	[Test]
	public async Task CreateServerAsync_SuccessfulResponse_ReturnsServerViewModel()
	{
		if (_apiService == null)
			throw new Exception("_apiService was null");

		// Arrange
		var tokenString = "dummy_token";
		var createServerRequest = new CreateServerRequest
		{
			Name = "Test Server",
			OwnerId = 1
		};

		var apiResponse = new ApiResponse<ServerDto>
		{
			Success = true,
			Data = new ServerDto
			{
				Id = 1,
				OwnerId = 1,
				Name = "Test Server",
				Channels = new List<ChannelDto>() // possibly empty for this test
			}
		};

		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("https://localhost:9999/api/servers/create-server")),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(apiResponse)
			});

		// Act
		var result = await _apiService.CreateServerAsync(tokenString, createServerRequest);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Id, Is.EqualTo(1));
		Assert.That(result.Name, Is.EqualTo("Test Server"));
	}

	[Test]
	public void CreateServerAsync_NonSuccessStatusCode_ThrowsException()
	{
		if (_apiService == null)
			throw new Exception("_apiService was null");

		// Arrange
		var tokenString = "dummy_token";
		var createServerRequest = new CreateServerRequest
		{
			Name = "Failing Server",
			OwnerId = 1
		};

		// Mock an HTTP 500 Internal Server Error
		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("https://localhost:9999/api/servers/create-server")),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Content = new StringContent("Something went wrong on the server.")
			});

		// Act & Assert
		var ex = Assert.ThrowsAsync<Exception>(async () =>
			await _apiService.CreateServerAsync(tokenString, createServerRequest)
		);

		Assert.That(ex, Is.Not.Null);
		Assert.That(ex?.Message, Does.Contain("Server returned InternalServerError"));
	}

	[Test]
	public void CreateServerAsync_ApiResponseFailure_ThrowsException()
	{
		if (_apiService == null)
			throw new Exception("_apiService was null");

		// Arrange
		var tokenString = "dummy_token";
		var createServerRequest = new CreateServerRequest
		{
			Name = "Failing Server",
			OwnerId = 1
		};

		// Server returns 200 OK, but the "Success" property is false.
		var failureApiResponse = new ApiResponse<ServerDto>
		{
			Success = false,
			Error = "Uncreative error."
		};

		_mockHttpMessageHandler?.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("https://localhost:9999/api/servers/create-server")),
				ItExpr.IsAny<CancellationToken>()
			)
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = JsonContent.Create(failureApiResponse)
			});

		// Act & Assert
		var ex = Assert.ThrowsAsync<Exception>(async () =>
			await _apiService.CreateServerAsync(tokenString, createServerRequest)
		);

		Assert.That(ex, Is.Not.Null);
		Assert.That(ex!.Message, Is.EqualTo("Uncreative error."));
	}
}

