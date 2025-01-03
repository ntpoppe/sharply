using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Server.Services;
using Sharply.Server.Automapper;
using Sharply.Shared.Requests;

namespace Sharply.Server.Tests;

[TestFixture]
public class MessageServiceTests
{
	private DbContextOptions<SharplyDbContext> _dbOptions;
	private Mock<ISharplyContextFactory<SharplyDbContext>> _contextFactoryMock;
	private MessageService _service;
	private IMapper _mapper;

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
		_service = new MessageService(_contextFactoryMock.Object, _mapper);
	}

	[Test]
	public async Task CreateMessage_Exists()
	{
		// Arrange
		await using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(
				new User() { Username = "TestUser", Id = 1 }
			);

			seedContext.Servers.AddRange(
				new Models.Server() { Id = 1, OwnerId = 1, Name = "Test Server 1" }
			);
 
			seedContext.Channels.AddRange(
				new Channel() { Id = 1, ServerId = 1, Name = "Test Channel 1-1"}
			);

			await seedContext.SaveChangesAsync();
		}


		int channelId = 1;
		int userId = 1;
		string content = "Test message";

		// Act
		var dto = await _service.CreateMessage(channelId, userId, content);

		// Assert
		using var verifyContext = new SharplyDbContext(_dbOptions);

		var dbMessage = await verifyContext.Messages
			.Where(m => m.ChannelId == channelId)
			.Where(m => m.UserId == userId)
			.Where(m => m.Content == content)
			.FirstOrDefaultAsync();

		Assert.That(dbMessage, Is.Not.Null);
		Assert.That(dbMessage.Content, Is.EqualTo(content));
		Assert.That(dto, Is.Not.Null);
		Assert.That(dto.Content, Is.EqualTo(content));
	}

	[Test]
	public async Task CreateMessage_ThrowsException_NullChannel()
	{
		// Arrange
		await using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(
				new User() { Username = "TestUser", Id = 1 }
			);

			seedContext.Servers.AddRange(
				new Models.Server() { Id = 1, OwnerId = 1, Name = "Test Server 1" }
			);
 
			seedContext.Channels.AddRange(
				new Channel() { Id = 1, ServerId = 1, Name = "Test Channel 1-1"}
			);

			await seedContext.SaveChangesAsync();
		}

		int nonExistentChannelId = 2; // Non-existing channel
		int userId = 1;
		string content = "Test message";

		// Act & Assert
		var ex = Assert.ThrowsAsync<Exception>(async () =>
			await _service.CreateMessage(nonExistentChannelId, userId, content)
		);

		Assert.That(ex.Message, Is.EqualTo("Channel not found. (MessageService/CreateMessage)"));	}
}


