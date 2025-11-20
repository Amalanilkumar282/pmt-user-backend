using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Channels; // ADDED
using BACKEND_CQRS.Application.Handler.Messages;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Messages;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BACKEND_CQRS.Test.Handler.QueryHandlers
{
    /// <summary>
    /// Unit tests for Message and Channel Query Handlers
    /// </summary>
    public class CommentMessageChannelQueryHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IMapper> _mapperMock;

        public CommentMessageChannelQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _mapperMock = new Mock<IMapper>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetMessagesByChannelIdQueryHandler Tests

        [Fact]
        public async Task GetMessagesByChannelIdQueryHandler_WithMessages_ReturnsSuccessWithData()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var messages = new List<Message>
            {
                new Message
                {
                    Id = Guid.NewGuid(),
                    ChannelId = channelId,
                    Body = "Hello World",
                    CreatedBy = 1,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ChannelId = channelId,
                    Body = "Second Message",
                    CreatedBy = 2,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            var query = new GetMessagesByChannelIdQuery(channelId, 10);
            var handler = new GetMessagesByChannelIdQueryHandler(_context);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetMessagesByChannelIdQueryHandler_WithNoMessages_ReturnsFail()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var query = new GetMessagesByChannelIdQuery(channelId, 10);
            var handler = new GetMessagesByChannelIdQueryHandler(_context);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        [Fact]
        public async Task GetMessagesByChannelIdQueryHandler_LimitsResultsByTake()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var messages = Enumerable.Range(1, 10).Select(i => new Message
            {
                Id = Guid.NewGuid(),
                ChannelId = channelId,
                Body = $"Message {i}",
                CreatedBy = 1,
                CreatedAt = DateTimeOffset.UtcNow.AddSeconds(i)
            }).ToList();

            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            var query = new GetMessagesByChannelIdQuery(channelId, 5);
            var handler = new GetMessagesByChannelIdQueryHandler(_context);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMessagesByChannelIdQueryHandler_ReturnsMessagesInDescendingOrder()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var oldMessage = new Message
            {
                Id = Guid.NewGuid(),
                ChannelId = channelId,
                Body = "Old Message",
                CreatedBy = 1,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2)
            };

            var newMessage = new Message
            {
                Id = Guid.NewGuid(),
                ChannelId = channelId,
                Body = "New Message",
                CreatedBy = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Messages.AddRange(oldMessage, newMessage);
            await _context.SaveChangesAsync();

            var query = new GetMessagesByChannelIdQuery(channelId, 10);
            var handler = new GetMessagesByChannelIdQueryHandler(_context);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal("New Message", result.Data[0].Body);
            Assert.Equal("Old Message", result.Data[1].Body);
        }

        #endregion

        #region GetChannelsByTeamIdQueryHandler Tests

        [Fact]
        public async Task GetChannelsByTeamIdQueryHandler_WithChannels_ReturnsSuccessWithData()
        {
            // Arrange
            var teamId = 1;
            var channels = new List<Channel>
            {
                new Channel { Id = Guid.NewGuid(), Name = "General", TeamId = teamId },
                new Channel { Id = Guid.NewGuid(), Name = "Random", TeamId = teamId },
                new Channel { Id = Guid.NewGuid(), Name = "Dev", TeamId = teamId }
            };

            _context.Channels.AddRange(channels);
            await _context.SaveChangesAsync();

            var query = new GetChannelsByTeamIdQuery(teamId);
            var handler = new GetChannelsByTeamIdQueryHandler(_context); // FIXED: Remove _mapperMock

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task GetChannelsByTeamIdQueryHandler_WithNoChannels_ReturnsFail()
        {
            // Arrange
            var teamId = 999;
            var query = new GetChannelsByTeamIdQuery(teamId);
            var handler = new GetChannelsByTeamIdQueryHandler(_context); // FIXED: Remove _mapperMock

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("No channels found", result.Message);
        }

        [Fact]
        public async Task GetChannelsByTeamIdQueryHandler_FiltersChannelsByTeam()
        {
            // Arrange
            var teamId1 = 1;
            var teamId2 = 2;

            var channels = new List<Channel>
            {
                new Channel { Id = Guid.NewGuid(), Name = "Team 1 Channel", TeamId = teamId1 },
                new Channel { Id = Guid.NewGuid(), Name = "Team 2 Channel", TeamId = teamId2 }
            };

            _context.Channels.AddRange(channels);
            await _context.SaveChangesAsync();

            var query = new GetChannelsByTeamIdQuery(teamId1);
            var handler = new GetChannelsByTeamIdQueryHandler(_context); // FIXED: Remove _mapperMock

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Single(result.Data);
            Assert.Equal("Team 1 Channel", result.Data[0].Name);
        }

        #endregion
    }
}
