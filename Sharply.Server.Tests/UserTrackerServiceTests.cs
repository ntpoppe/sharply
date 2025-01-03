using Moq;
using NUnit.Framework;
using Sharply.Server.Interfaces;
using Sharply.Server.Services;
using Sharply.Shared.Models;

namespace Sharply.Server.Tests
{
    [TestFixture]
    public class UserTrackerServiceTests
    {
        private Mock<IUserService>? _userServiceMock;
        private UserTrackerService? _service;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
            _service = new UserTrackerService(_userServiceMock.Object);
        }

        [Test]
        public async Task AddUser_ShouldStoreUserInConnectionMap_AndTrackChannels()
        {
			if (_userServiceMock == null || _service == null)
				throw new Exception("_userServiceMock and _service were null");

            // Arrange
            var connectionId = "conn-1";
            var userId = 1;
            var fakeChannels = new List<ChannelDto>
            {
                new ChannelDto { Id = 1, ServerId = 1, Name = "General" },
                new ChannelDto { Id = 2, ServerId = 1, Name = "Random" }
            };

            _userServiceMock
                .Setup(u => u.GetChannelsForUserAsync(userId, default))
                .ReturnsAsync(fakeChannels);

            // Act
            await _service.AddUser(connectionId, userId);

            // Assert
            var resultUserId = _service.GetUserIdFromConnectionId(connectionId);
            Assert.That(resultUserId, Is.EqualTo(userId));

            var trackedChannels = _service.GetTrackedUserChannels(userId);
            Assert.That(trackedChannels.Count, Is.EqualTo(2));
            Assert.That(trackedChannels, Is.EquivalentTo(new[] { 1, 2 }));

            _userServiceMock.Verify(u => u.GetChannelsForUserAsync(userId, default), Times.Once);
        }

        [Test]
        public async Task RemoveUser_ShouldRemoveFromConnectionMap_AndChannelAccess()
        {
			if (_userServiceMock == null || _service == null)
				throw new Exception("_userServiceMock and _service were null");

            // Arrange
            var connectionId = "conn-1";
            var userId = 1;

            _userServiceMock
                .Setup(u => u.GetChannelsForUserAsync(userId, default))
                .ReturnsAsync(new List<ChannelDto> { new ChannelDto { Id = 1, ServerId = 1, Name = "Test Channel" } });

            await _service.AddUser(connectionId, userId);

            // Confirm they were added
            Assert.That(_service.GetUserIdFromConnectionId(connectionId), Is.EqualTo(userId));
            Assert.That(_service.GetTrackedUserChannels(userId).Count, Is.EqualTo(1));

            // Act
            _service.RemoveUser(connectionId);

            // Assert
            Assert.That(_service.GetUserIdFromConnectionId(connectionId), Is.Null);
            Assert.That(_service.GetTrackedUserChannels(userId).Count, Is.EqualTo(0));
        }

        [Test]
        public void GetUserIdFromConnectionId_ReturnsNull_IfNotFound()
        {
			if (_service == null)
				throw new Exception("_service were null");

            // Arrange
            var connectionId = "nonexistent-conn";

            // Act
            var resultUserId = _service.GetUserIdFromConnectionId(connectionId);

            // Assert
            Assert.That(resultUserId, Is.Null);
        }

        [Test]
        public async Task GetTrackedUserChannels_ReturnsEmpty_IfUserNotTracked()
        {
			if (_service == null)
				throw new Exception("_service were null");

            // Arrange
            var userId = 1;

            // Act
            var channels = _service.GetTrackedUserChannels(userId);

            // Assert
            Assert.That(channels, Is.Empty);

            await _service.AddUser("some-other-conn", userId: 2);
            var stillEmpty = _service.GetTrackedUserChannels(userId);
            Assert.That(stillEmpty, Is.Empty);
        }

        [Test]
        public async Task GetAllTrackedUsers_ReturnsUserDtos_SortedByUsername()
        {
			if (_userServiceMock == null || _service == null)
				throw new Exception("_userServiceMock and _service were null");

            // Arrange
            var connectionId1 = "conn-A";
            var userId1 = 1;
            var connectionId2 = "conn-B";
            var userId2 = 2;

            _userServiceMock
                .Setup(u => u.GetChannelsForUserAsync(It.IsAny<int>(), default))
                .ReturnsAsync(new List<ChannelDto>());

            _userServiceMock
                .Setup(u => u.GetUserDto(1, default))
                .ReturnsAsync(new UserDto { Id = 1, Username = "Alice" });

            _userServiceMock
                .Setup(u => u.GetUserDto(2, default))
                .ReturnsAsync(new UserDto { Id = 2, Username = "Bob" });

            await _service.AddUser(connectionId1, userId1);
            await _service.AddUser(connectionId2, userId2);

            // Act
            var trackedUsers = await _service.GetAllTrackedUsers();

            // Assert
            Assert.That(trackedUsers.Count, Is.EqualTo(2));
            Assert.That(trackedUsers[0].Username, Is.EqualTo("Alice"));
            Assert.That(trackedUsers[1].Username, Is.EqualTo("Bob"));

            _userServiceMock.Verify(u => u.GetUserDto(1, default), Times.Once);
            _userServiceMock.Verify(u => u.GetUserDto(2, default), Times.Once);
        }
    }
}

