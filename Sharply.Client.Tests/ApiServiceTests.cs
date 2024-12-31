using Moq;
using Moq.Protected;
using NUnit.Framework;
using Sharply.Client.Services;
using Sharply.Shared.Requests;
using Sharply.Shared.Models;
using Sharply.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Sharply.Client.Tests;

[TestFixture]
public class ApiServiceTests
{
	private Mock<HttpMessageHandler> _mockHttpMessageHandler;
	private TokenStorageService _tokenStorageService;
	private HttpClient _httpClient;
	private ApiService _apiService;

	[SetUp]
	public void SetUp()
	{
		_mockHttpMessageHandler = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_mockHttpMessageHandler.Object)
		{
			BaseAddress = new Uri("https://localhost:9999/")
		};
		_tokenStorageService = new TokenStorageService();
		_apiService = new ApiService(_httpClient, _tokenStorageService);
	}

	[Test]
	public async Task RegisterAsync_SuccessfulResponse_ReturnsUser()
	{
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

		_mockHttpMessageHandler.Protected()
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
		// Arrange
		var errorMessage = "User already exists";

		// Setup the mock to return a bad request response with the error message
		_mockHttpMessageHandler.Protected()
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
			await _apiService.RegisterAsync("existinguser", "password123");
		});

		Assert.That(ex.Message, Does.Contain("Registration failed"));
		Assert.That(ex.Message, Does.Contain(errorMessage));
	}

	[Test]
	public async Task LoginAsync_SuccessfulResponse_ReturnsUser()
	{
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
		_mockHttpMessageHandler.Protected()
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
		// Arrange
		var errorMessage = "Invalid credentials";

		// Setup the mock to return an unauthorized response with the error message
		_mockHttpMessageHandler.Protected()
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

		Assert.That(ex.Message, Does.Contain("Check your credentials."));
	}

	[Test]
	public async Task GetServersAsync_ValidResponse_ReturnsServerViewModels()
	{
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

		_mockHttpMessageHandler.Protected()
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
		// Arrange
		var mockResponse = new ApiResponse<List<MessageDto>>
		{
			Success = true,
			Data = new List<MessageDto>
			{
				new MessageDto { Id = 1, UserId = 1, Username = "TestUser", Content = "Hello, World!", ChannelId = 1, Timestamp = DateTime.Now }
			}
		};

		_mockHttpMessageHandler.Protected()
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
	public async Task DoesUserHaveAccessToChannel_UserHasAccess_ReturnsTrue()
	{
		// Arrange
		var mockResponse = new ApiResponse<bool>
		{
			Success = true,
			Data = true
		};

		_mockHttpMessageHandler.Protected()
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
		var result = await _apiService.DoesUserHaveAccessToChannel("dummy_token", 123, 456);

		// Assert
		Assert.That(result, Is.True);
	}

	[Test]
	public async Task GetCurrentUserData_ValidResponse_ReturnsUserDto()
	{
		// Arrange
		var mockResponse = new ApiResponse<UserDto>
		{
			Success = true,
			Data = new UserDto { Id = 1, Username = "TestUser" }
		};

		_mockHttpMessageHandler.Protected()
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
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Username, Is.EqualTo("TestUser"));
	}
}

