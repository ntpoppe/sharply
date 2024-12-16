using Moq;
using Moq.Protected;
using NUnit.Framework;
using Sharply.Client.Services;
using Sharply.Shared.Requests;
using System.Net;
using System.Net.Http.Json;

namespace Sharply.Client.Tests
{
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
            // Initialize the mock HttpMessageHandler
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Create HttpClient with the mocked handler
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost:8001/")
            };

            _tokenStorageService = new TokenStorageService();

            // Initialize ApiService with the mocked HttpClient
            // Modify ApiService to accept HttpClient instead of HttpClientHandler
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

            // Setup the mock to return a successful response with the expected content
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://localhost:8001/auth/register")),
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
                        req.RequestUri == new Uri("https://localhost:8001/auth/register")),
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
                        req.RequestUri == new Uri("https://localhost:8001/auth/login")),
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
                        req.RequestUri == new Uri("https://localhost:8001/auth/login")),
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

            Assert.That(ex.Message, Does.Contain("Login failed"));
            Assert.That(ex.Message, Does.Contain("credentials"));
        }
    }
}

