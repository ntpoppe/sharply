using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Sharply.Server.AutoMapper;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Server.Services;
using Sharply.Shared.Models;

namespace Sharply.Server.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private DbContextOptions<SharplyDbContext>? _dbOptions;
        private Mock<ISharplyContextFactory<SharplyDbContext>>? _contextFactoryMock;
        private Mock<IServerService>? _serverServiceMock;
        private IMapper? _realMapper;
        private UserService? _service;

        [SetUp]
        public void Setup()
        {
            _dbOptions = new DbContextOptionsBuilder<SharplyDbContext>()
                .UseInMemoryDatabase($"SharplyTestDb_{Guid.NewGuid()}")
                .Options;

            _contextFactoryMock = new Mock<ISharplyContextFactory<SharplyDbContext>>();
            _contextFactoryMock
                .Setup(factory => factory.CreateSharplyContext())
                .Returns(() => new SharplyDbContext(_dbOptions));

            _serverServiceMock = new Mock<IServerService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _realMapper = config.CreateMapper();

            _service = new UserService(
                _contextFactoryMock.Object,
                _realMapper,
                _serverServiceMock.Object
            );
        }

        [Test]
        public void GetUserDto_ThrowsIfNotFound()
        {
            if (_service == null)
                throw new Exception("_service were null");

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetUserDto(999)
            );

            if (ex == null) throw new Exception("ex was null");
            Assert.That(ex.Message, Does.Contain("was not found from GetUserDto"));
        }

        [Test]
        public async Task GetUserDto_ReturnsUserDtoIfFound()
        {
            if (_dbOptions == null || _service == null)
                throw new Exception("_dbOptions and _service were null");

            // Arrange
            await using (var seedContext = new SharplyDbContext(_dbOptions))
            {
                seedContext.Users.Add(new User
                {
                    Id = 1,
                    Username = "TestUser",
                    IsDeleted = false
                });
                await seedContext.SaveChangesAsync();
            }

            // Act
            var userDto = await _service.GetUserDto(1);

            // Assert
            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(1));
            Assert.That(userDto.Username, Is.EqualTo("TestUser"));
        }

        [Test]
        public void AddUserToServerAsync_ThrowsIfServerNotFound()
        {
            if (_service == null)
                throw new Exception("_service were null");

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _service.AddUserToServerAsync(userId: 1, serverId: 999)
            );

            if (ex == null) throw new Exception("ex was null");
            Assert.That(ex.Message, Is.EqualTo("Server not found"));
        }

        [Test]
        public async Task AddUserToServerAsync_AddsIfServerFound()
        {
            if (_dbOptions == null || _service == null)
                throw new Exception("_dbOptions and _service were null");

            // Arrange
            await using (var seedContext = new SharplyDbContext(_dbOptions))
            {
                seedContext.Servers.Add(new Models.Server
                {
                    Id = 1,
                    OwnerId = 1,
                    Name = "Test Server",
                    InviteCode = "12345678",
                    IsDeleted = false
                });
                await seedContext.SaveChangesAsync();
            }

            // Act
            await _service.AddUserToServerAsync(userId: 1, serverId: 1);

            // Assert
            await using var verifyContext = new SharplyDbContext(_dbOptions);
            var userServer = await verifyContext.UserServers
                .FirstOrDefaultAsync(us => us.UserId == 1 && us.ServerId == 1);
            Assert.That(userServer, Is.Not.Null);
        }

        [Test]
        public async Task GetServersForUserAsync_CallsServerServiceAndReturnsResult()
        {
            if (_serverServiceMock == null || _service == null)
                throw new Exception("_serverServiceMock and _service were null");

            // Arrange
            var fakeServers = new System.Collections.Generic.List<ServerDto>
            {
                new ServerDto { Id = 1, OwnerId = 1, Name = "MockServer1", InviteCode = "123456789" }
            };
            _serverServiceMock
                .Setup(s => s.GetServersWithChannelsForUserAsync(It.IsAny<int>(), default))
                .ReturnsAsync(fakeServers);

            // Act
            var result = await _service.GetServersForUserAsync(userId: 1);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("MockServer1"));
            _serverServiceMock.Verify(s => s.GetServersWithChannelsForUserAsync(1, default), Times.Once);
        }

        [Test]
        public async Task GetChannelsForUserAsync_ReturnsUserChannels()
        {
            if (_dbOptions == null || _service == null)
                throw new Exception("_dbOptions and _service were null");

            // Arrange
            await using (var seedContext = new SharplyDbContext(_dbOptions))
            {
                seedContext.Users.Add(new User { Id = 1, Username = "Test User" });
                seedContext.Servers.Add(new Models.Server { Id = 1, OwnerId = 1, Name = "Test Server", InviteCode = "12345678" });
                seedContext.Channels.Add(new Channel { Id = 1, ServerId = 1, Name = "Test Channel" });

                seedContext.UserChannels.Add(new UserChannel
                {
                    UserId = 1,
                    ChannelId = 1,
                    IsActive = true // needs to be set, in-memory doesnt apply default values for some reason
                });

                await seedContext.SaveChangesAsync();
            }

            // Act
            var channels = await _service.GetChannelsForUserAsync(userId: 1);

            // Assert
            Assert.That(channels.Count, Is.EqualTo(1));
            Assert.That(channels[0].Id, Is.EqualTo(1));
            Assert.That(channels[0].Name, Is.EqualTo("Test Channel"));
        }

        [Test]
        public void GetUsernameFromId_ThrowsIfNotFound()
        {
            if (_service == null)
                throw new Exception("_service were null");

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetUsernameFromId(userId: 999)
            );

            if (ex == null) throw new Exception("ex was null");
            Assert.That(ex.Message, Does.Contain("No user found with ID 999."));
        }

        [Test]
        public async Task GetUsernameFromId_ReturnsUsernameIfFound()
        {
            if (_dbOptions == null || _service == null)
                throw new Exception("_dbOptions and _service were null");

            // Arrange
            await using (var seedContext = new SharplyDbContext(_dbOptions))
            {
                seedContext.Users.Add(new User
                {
                    Id = 1,
                    Username = "Test User",
                    IsDeleted = false
                });
                await seedContext.SaveChangesAsync();
            }

            // Act
            var username = await _service.GetUsernameFromId(userId: 1);

            // Assert
            Assert.That(username, Is.EqualTo("Test User"));
        }
    }
}

