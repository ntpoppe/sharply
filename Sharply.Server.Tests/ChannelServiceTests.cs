using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Server.Services;
using Sharply.Shared.Models;

namespace Sharply.Server.Tests;

[TestFixture]
public class ChannelServiceTests
{
    private DbContextOptions<SharplyDbContext> _dbOptions;
    private Mock<ISharplyContextFactory<SharplyDbContext>> _contextFactoryMock;
    private Mock<IMapper> _mapperMock;
    private IChannelService _service;

    [SetUp]
    public void SetUp()
    {
        _dbOptions = new DbContextOptionsBuilder<SharplyDbContext>()
            .UseInMemoryDatabase(databaseName: $"SharplyTestDb_{Guid.NewGuid()}")
            .Options;

        // Mock the context factory so that each call to CreateSharplyContext()
        // returns a brand-new SharplyDbContext pointed at our unique in-memory DB.
        _contextFactoryMock = new Mock<ISharplyContextFactory<SharplyDbContext>>();
        _contextFactoryMock
            .Setup(factory => factory.CreateSharplyContext())
            .Returns(() => new SharplyDbContext(_dbOptions));

        // Mock the mapper for when we map from Message -> MessageDto, etc.
        _mapperMock = new Mock<IMapper>();
        _mapperMock
            .Setup(m => m.Map<List<MessageDto>>(It.IsAny<List<Message>>()))
            .Returns((List<Message> source) =>
            {
                // For simplicity, produce a minimal list of MessageDto objects.
                // You can adapt this to your real mapping logic as needed.
                return source.Select(msg => new MessageDto
                {
                    Id = msg.Id.Value,
                    Content = msg.Content,
                    UserId = msg.UserId,
                    Username = "TestUsername",
                    Timestamp = msg.Timestamp,
                    ChannelId = msg.ChannelId
                }).ToList();
            });

        _service = new ChannelService(_contextFactoryMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task GetMessagesForChannelAsync_ShouldReturnOnlyNonDeletedMessages()
    {
        // Arrange 
        await using (var seedContext = new SharplyDbContext(_dbOptions))
        {
            seedContext.Users.Add(
                new User { Id = 999, Username = "TestUser" }
            );

            seedContext.Messages.AddRange(
                new Message { Id = 1, ChannelId = 10, Content = "Msg1", UserId = 999, Timestamp = DateTime.UtcNow, IsDeleted = false },
                new Message { Id = 2, ChannelId = 10, Content = "Msg2", UserId = 999, Timestamp = DateTime.UtcNow, IsDeleted = true },  // Deleted
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
        // Arrange
        var userId = 100;
        var channelId = 100;

        // Act
        await _service.AddUserToChannelAsync(userId, channelId);

        // Assert
        await using var verifyContext = new SharplyDbContext(_dbOptions);

        var userChannel = await verifyContext.UserChannels
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChannelId == channelId);

        Assert.That(userChannel, Is.Not.Null);
        Assert.That(userChannel.IsActive, Is.True);
    }

    [Test]
    public async Task CheckUserChannelAccessAsync_ShouldReturnTrue_IfIsActive()
    {
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
        // Arrange
        var userId = 300;
        var channelId = 300;

        // Act
        var hasAccess = await _service.CheckUserChannelAccessAsync(channelId, userId);

        // Assert
        Assert.That(hasAccess, Is.False);
    }
}
