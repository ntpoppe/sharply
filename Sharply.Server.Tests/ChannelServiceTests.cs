using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Sharply.Server.Data;
using Sharply.Server.Automapper;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Server.Services;

namespace Sharply.Server.Tests;

[TestFixture]
public class ChannelServiceTests
{
    private DbContextOptions<SharplyDbContext>? _dbOptions;
    private Mock<ISharplyContextFactory<SharplyDbContext>>? _contextFactoryMock;
	private IMapper? _mapper;
    private IChannelService? _service;

    [SetUp]
    public void SetUp()
    {
        _dbOptions = new DbContextOptionsBuilder<SharplyDbContext>()
            .UseInMemoryDatabase(databaseName: $"SharplyTestDb_{Guid.NewGuid()}")
            .Options;

        _contextFactoryMock = new Mock<ISharplyContextFactory<SharplyDbContext>>();
        _contextFactoryMock
            .Setup(factory => factory.CreateSharplyContext())
            .Returns(() => new SharplyDbContext(_dbOptions));

		var mapperConfig = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>(); 
		});

		_mapper = mapperConfig.CreateMapper();
        _service = new ChannelService(_contextFactoryMock.Object, _mapper);
    }

    [Test]
    public async Task GetMessagesForChannelAsync_ShouldReturnOnlyNonDeletedMessages()
    {
		if (_dbOptions == null || _service == null)
			throw new Exception("_dbOptions and _service were null");

        // Arrange 
        await using (var seedContext = new SharplyDbContext(_dbOptions))
        {
            seedContext.Users.Add(
                new User { Id = 999, Username = "TestUser" }
            );

            seedContext.Messages.AddRange(
                new Message { Id = 1, ChannelId = 10, Content = "Msg1", UserId = 999, Timestamp = DateTime.UtcNow, IsDeleted = false },
                new Message { Id = 2, ChannelId = 10, Content = "Msg2", UserId = 999, Timestamp = DateTime.UtcNow, IsDeleted = true }, 
                new Message { Id = 3, ChannelId = 20, Content = "Msg3", UserId = 999, Timestamp = DateTime.UtcNow, IsDeleted = false }
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        var result = await _service.GetMessagesForChannelAsync(channelId: 10);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Content, Is.EqualTo("Msg1"));
    }

    [Test]
    public async Task AddUserToChannelAsync_ShouldCreateUserChannelRecord()
    {
		if (_dbOptions == null || _service == null)
			throw new Exception("_dbOptions and _service were null");

        // Arrange
        var userId = 100;
        var channelId = 100;

        // Act
        await _service.AddUserToChannelAsync(userId, channelId);

        // Assert
        await using var verifyContext = new SharplyDbContext(_dbOptions);

        var userChannel = await verifyContext.UserChannels
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChannelId == channelId);

		if (userChannel == null) throw new Exception("userChannel was null");
        Assert.That(userChannel.IsActive, Is.True);
    }

    [Test]
    public async Task CheckUserChannelAccessAsync_ShouldReturnTrue_IfIsActive()
    {
		if (_dbOptions == null || _service == null)
			throw new Exception("_dbOptions and _service were null");

        // Arrange
        var userId = 200;
        var channelId = 200;

        // Seed a UserChannel with IsActive = true
        await using (var seedContext = new SharplyDbContext(_dbOptions))
        {
            seedContext.UserChannels.Add(new UserChannel
            {
                UserId = userId,
                ChannelId = channelId,
                IsActive = true
            });
            await seedContext.SaveChangesAsync();
        }

        // Act
        var hasAccess = await _service.CheckUserChannelAccessAsync(channelId, userId);

        // Assert
        Assert.That(hasAccess, Is.True);
    }

    [Test]
    public async Task CheckUserChannelAccessAsync_ShouldReturnFalse_IfNotActiveOrDoesNotExist()
    {
		if (_service == null) throw new Exception("_service were null");

        // Arrange
        var userId = 300;
        var channelId = 300;

        // Act
        var hasAccess = await _service.CheckUserChannelAccessAsync(channelId, userId);

        // Assert
        Assert.That(hasAccess, Is.False);
    }
}
