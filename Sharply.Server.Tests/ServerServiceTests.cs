using System;
using System.Threading.Tasks;
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
public class ServerServiceTests
{
	private DbContextOptions<SharplyDbContext> _dbOptions;
	private Mock<ISharplyContextFactory<SharplyDbContext>> _contextFactoryMock;
	private ServerService _service;
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
		_service = new ServerService(_contextFactoryMock.Object, _mapper);
	}

	[Test]
	public async Task CreateServer_ShouldCreateServerAndAssignOwner()
	{
		// Arrange
		var request = new CreateServerRequest
		{
			Name = "Test Server",
			OwnerId = 1
		};

		// Act
		var result = await _service.CreateServer(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Name, Is.EqualTo("Test Server"));

		await using var verificationContext = new SharplyDbContext(_dbOptions);

		var server = await verificationContext.Servers.FirstOrDefaultAsync();
		Assert.That(server, Is.Not.Null);
		Assert.That(server.Name, Is.EqualTo("Test Server"));

		var userServer = await verificationContext.UserServers.FirstOrDefaultAsync();
		Assert.That(userServer, Is.Not.Null);
		Assert.That(userServer.UserId, Is.EqualTo(1));
		Assert.That(userServer.ServerId, Is.EqualTo(server.Id));

		var channel = await verificationContext.Channels.FirstOrDefaultAsync();
		Assert.That(channel, Is.Not.Null);
		Assert.That(channel.Name, Is.EqualTo("/general"));
		Assert.That(channel.ServerId, Is.EqualTo(server.Id));
	}

	[Test]
	public async Task GetServersWithChannelsForUserAsync_ReturnCorrectly()
	{
		// Arrange
		await using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(
				new User() { Username = "TestUser", Id = 1 }
			);

			seedContext.Servers.AddRange(
				new Models.Server() { Id = 1, OwnerId = 1, Name = "Test Server 1" },
				new Models.Server() { Id = 2, OwnerId = 999, Name = "Test Server 2" }
			);
 
			seedContext.Channels.AddRange(
				new Channel() { Id = 1, ServerId = 1, Name = "Test Channel 1-1"},
				new Channel() { Id = 2, ServerId = 1, Name = "Test Channel 1-2"},
				new Channel() { Id = 3, ServerId = 2, Name = "Test Channel 2-1"} 
			);

			seedContext.UserServers.Add(
				new UserServer() { ServerId = 1, UserId = 1 }
			);

			seedContext.UserChannels.Add(
				new UserChannel() { ChannelId = 1, UserId = 1 }
			);

			await seedContext.SaveChangesAsync();
		}

		// Act
		var result = await _service.GetServersWithChannelsForUserAsync(userId: 1);

		// Assert
		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result[0].Name, Is.EqualTo("Test Server 1"));
		Assert.That(result[0].Channels.Count, Is.EqualTo(1));
		Assert.That(result[0].Channels[0].Name, Is.EqualTo("Test Channel 1-1"));
	}

	[Test]
	public async Task GetChannelsForServerAsync_ReturnsNonDeletedChannels()
	{
		// Arrange
		await using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(
				new User() { Username = "TestUser", Id = 1 }
			);

			seedContext.Servers.AddRange(
				new Models.Server() { Id = 1, OwnerId = 1, Name = "Test Server 1" },
				new Models.Server() { Id = 2, OwnerId = 1, Name = "Test Server 2" }
			);

			seedContext.Channels.AddRange(
				new Channel() { Id = 1, ServerId = 1, Name = "Test Channel 1-1" },
				new Channel() { Id = 2, ServerId = 1, Name = "Test Channel 1-2", IsDeleted = true },
				new Channel() { Id = 3, ServerId = 2, Name = "Test Channel 2-1" }
			);

			await seedContext.SaveChangesAsync();
		}

		// Act
		var result = await _service.GetChannelsForServerAsync(serverId: 1);

		// Assert
		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result[0].Name, Is.EqualTo("Test Channel 1-1"));
	}
}


