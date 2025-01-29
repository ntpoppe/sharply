using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Sharply.Server.AutoMapper;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Server.Services;
using Sharply.Shared.Requests;

namespace Sharply.Server.Tests;

// These tests utilize SQLite instead of EF's in-memory due to methods I used in the implementation that in-memory doesn't support.
[TestFixture]
public class ServerServiceTests
{
	private SqliteConnection? _connection;
	private DbContextOptions<SharplyDbContext>? _dbOptions;
	private IMapper? _mapper;
	private Mock<ISharplyContextFactory<SharplyDbContext>>? _contextFactoryMock;
	private ServerService? _service;

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		_connection = new SqliteConnection("Filename=:memory:");
		_connection.Open();

		_dbOptions = new DbContextOptionsBuilder<SharplyDbContext>()
			.UseSqlite(_connection)
			.Options;

		using var initContext = new SharplyDbContext(_dbOptions);
		initContext.Database.EnsureCreated();

		var mapperConfig = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = mapperConfig.CreateMapper();
	}

	[SetUp]
	public async Task SetUp()
	{
		if (_dbOptions == null || _mapper == null)
			throw new Exception("DbOptions or Mapper were null");

		using var context = new SharplyDbContext(_dbOptions);
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();

		_contextFactoryMock = new Mock<ISharplyContextFactory<SharplyDbContext>>();
		_contextFactoryMock
			.Setup(factory => factory.CreateSharplyContext())
			.Returns(() => new SharplyDbContext(_dbOptions));

		_service = new ServerService(_contextFactoryMock.Object, _mapper);

		// Remove seeded server
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			var globalServer = await seedContext.Servers.FirstOrDefaultAsync(s => s.Name == "Global");
			if (globalServer != null)
			{
				seedContext.Servers.Remove(globalServer);
				await seedContext.SaveChangesAsync();
			}
		}
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		_connection?.Close();
		_connection?.Dispose();
	}

	[Test]
	public async Task CreateServerAsync_ShouldCreateServerAndAssignOwner()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Arrange
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(new User { Id = 1, Username = "TestUser" });
			await seedContext.SaveChangesAsync();
		}

		var request = new CreateServerRequest
		{
			Name = "Test Server",
			OwnerId = 1
		};

		// Act
		var result = await _service.CreateServerAsync(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Name, Is.EqualTo("Test Server"));

		using var verificationContext = new SharplyDbContext(_dbOptions);

		var server = await verificationContext.Servers.FirstOrDefaultAsync();
		Assert.That(server, Is.Not.Null);
		Assert.That(server!.Name, Is.EqualTo("Test Server"));

		var userServer = await verificationContext.UserServers.FirstOrDefaultAsync();
		Assert.That(userServer, Is.Not.Null);
		Assert.That(userServer!.UserId, Is.EqualTo(1));
		Assert.That(userServer.ServerId, Is.EqualTo(server.Id));

		var channel = await verificationContext.Channels.FirstOrDefaultAsync();
		Assert.That(channel, Is.Not.Null);
		Assert.That(channel!.Name, Is.EqualTo("general"));
		Assert.That(channel.ServerId, Is.EqualTo(server.Id));
	}

	[Test]
	public async Task SoftDeleteServerAsync_Success()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Arrange
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(new User { Id = 1, Username = "SomeUser" });

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
		await _service.SoftDeleteServerAsync(1);

		// Assert
		using var verificationContext = new SharplyDbContext(_dbOptions);
		var server = await verificationContext.Servers.Where(s => s.Name == "Test Server").FirstOrDefaultAsync();

		Assert.That(server, Is.Not.Null);
		Assert.That(server!.IsDeleted, Is.True);
	}

	[Test]
	public void SoftDeleteServerAsync_ServerNotFound()
	{
		if (_service == null)
			throw new Exception("Service was null.");

		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await _service.SoftDeleteServerAsync(999)); // Non-existent

		Assert.That(ex!.Message, 
			Is.EqualTo("Attempted to delete server with id 999. Server does not exist."));
	}

	[Test]
	public async Task GetServersWithChannelsForUserAsync_ReturnCorrectly()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Seed
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(new User { Id = 1, Username = "TestUser" });

			seedContext.Servers.AddRange(
				new Models.Server { Id = 1, OwnerId = 1, Name = "Test Server 1", InviteCode = "12345678" },
				new Models.Server { Id = 2, OwnerId = 999, Name = "Test Server 2", InviteCode = "12345678" }
			);

			seedContext.Channels.AddRange(
				new Channel { Id = 1, ServerId = 1, Name = "Test Channel 1-1" },
				new Channel { Id = 2, ServerId = 1, Name = "Test Channel 1-2" },
				new Channel { Id = 3, ServerId = 2, Name = "Test Channel 2-1" }
			);

			seedContext.UserServers.Add(new UserServer { ServerId = 1, UserId = 1 });
			seedContext.UserChannels.Add(new UserChannel { ChannelId = 1, UserId = 1 });

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
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(new User { Id = 1, Username = "TestUser" });

			seedContext.Servers.AddRange(
				new Models.Server { Id = 1, OwnerId = 1, Name = "Test Server 1", InviteCode = "12345678" },
				new Models.Server { Id = 2, OwnerId = 1, Name = "Test Server 2", InviteCode = "12345678" }
			);

			seedContext.Channels.AddRange(
				new Channel { Id = 1, ServerId = 1, Name = "Test Channel 1-1" },
				new Channel { Id = 2, ServerId = 1, Name = "Test Channel 1-2", IsDeleted = true },
				new Channel { Id = 3, ServerId = 2, Name = "Test Channel 2-1" }
			);

			await seedContext.SaveChangesAsync();
		}

		var result = await _service.GetChannelsForServerAsync(serverId: 1);

		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result[0].Name, Is.EqualTo("Test Channel 1-1"));
	}

	[Test]
	public async Task AddUserToServerAsync_ShouldAddUser_WhenNotAlreadyInServer()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Arrange: Seed a server, user, and a default channel
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(new User { Id = 1, Username = "TestUser" });

			var server = new Models.Server
			{
				Id = 1,
				Name = "Test Server",
				OwnerId = 1,
				InviteCode = "12345678"
			};
			seedContext.Servers.Add(server);

			var defaultChannel = new Channel
			{
				Id = 1,
				ServerId = server.Id,
				Name = "general",
				IsDefault = true
			};
			seedContext.Channels.Add(defaultChannel);

			await seedContext.SaveChangesAsync();
		}

		// Act
		var result = await _service.AddUserToServerAsync(userId: 1, serverId: 1);

		// Assert
		Assert.That(result, Is.True);

		using var verificationContext = new SharplyDbContext(_dbOptions);

		var userServer = await verificationContext.UserServers
			.FirstOrDefaultAsync(us => us.UserId == 1 && us.ServerId == 1);
		Assert.That(userServer, Is.Not.Null);

		var userChannel = await verificationContext.UserChannels
			.FirstOrDefaultAsync(uc => uc.UserId == 1);
		Assert.That(userChannel, Is.Not.Null);
		Assert.That(userChannel!.ChannelId, Is.EqualTo(1)); // Default channel
	}

	[Test]
	public async Task AddUserToServerAsync_ShouldReturnFalse_WhenUserAlreadyInServer()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Arrange
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(new User { Id = 1, Username = "TestUser" });

			var server = new Models.Server
			{
				Id = 1,
				Name = "Test Server",
				OwnerId = 1,
				InviteCode = "12345678"
			};
			seedContext.Servers.Add(server);

			var defaultChannel = new Channel
			{
				Id = 1,
				ServerId = server.Id,
				Name = "general",
				IsDefault = true
			};
			seedContext.Channels.Add(defaultChannel);

			var existingUserServer = new UserServer { UserId = 1, ServerId = 1 };
			seedContext.UserServers.Add(existingUserServer);

			await seedContext.SaveChangesAsync();
		}

		// Act
		var result = await _service.AddUserToServerAsync(userId: 1, serverId: 1);

		// Assert
		Assert.That(result, Is.False);
	}

	[Test]
	public async Task AddUserToServerAsync_ShouldThrow_WhenNoDefaultChannel()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Arrange: Seed a server **without** a default channel
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Users.Add(new User { Id = 1, Username = "TestUser" });

			var server = new Models.Server
			{
				Id = 1,
				Name = "Test Server",
				OwnerId = 1,
				InviteCode = "12345678"
			};
			seedContext.Servers.Add(server);

			await seedContext.SaveChangesAsync();
		}

		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await _service.AddUserToServerAsync(userId: 1, serverId: 1));

		Assert.That(ex!.Message, Is.EqualTo("Server doesn't have a default channel."));
	}

	[Test]
	public async Task GetServerByInviteCodeAsync_ShouldReturnServer_WhenCodeIsValid()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Arrange: Seed a server with an invite code
		using (var seedContext = new SharplyDbContext(_dbOptions))
		{
			seedContext.Servers.Add(new Models.Server
			{
				Id = 1,
				Name = "Test Server",
				OwnerId = 1,
				InviteCode = "ABCDEFGH"
			});

			await seedContext.SaveChangesAsync();
		}

		// Act
		var result = await _service.GetServerByInviteCodeAsync("ABCDEFGH");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Name, Is.EqualTo("Test Server"));
	}

	[Test]
	public async Task GetServerByInviteCodeAsync_ShouldReturnNull_WhenCodeIsInvalid()
	{
		if (_service == null || _dbOptions == null)
			throw new Exception("Service or DbOptions was null.");

		// Act
		var result = await _service.GetServerByInviteCodeAsync("INVALIDCODE");

		// Assert
		Assert.That(result, Is.Null);
	}
}


