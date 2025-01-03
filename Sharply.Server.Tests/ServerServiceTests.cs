using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Services;
using Sharply.Shared.Models;
using Sharply.Shared.Requests;

namespace Sharply.Server.Tests
{
    [TestFixture]
    public class ServerServiceTests
    {
        private DbContextOptions<SharplyDbContext> _dbOptions;
        private Mock<ISharplyContextFactory<SharplyDbContext>> _contextFactoryMock;
        private Mock<IMapper> _mapperMock;
        private ServerService _service;

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

            // Mock the mapper
            _mapperMock = new Mock<IMapper>();
            _mapperMock
                .Setup(m => m.Map<ServerDto>(It.IsAny<Models.Server>()))
                .Returns((Models.Server server) =>
                    new ServerDto
                    {
                        Id = 1, // dummy for testing
                        OwnerId = server.OwnerId,
                        Name = server.Name
                    });

            _service = new ServerService(_contextFactoryMock.Object, _mapperMock.Object);
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

            // Use a *new* SharplyDbContext for verification 
            // (i.e., do not reuse the one that was just disposed in the service)
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
    }
}

