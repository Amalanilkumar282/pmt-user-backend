using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Sprints;
using BACKEND_CQRS.Application.Query.Sprints;
using BACKEND_CQRS.Application.Wrapper;
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
    /// Unit tests for Sprint Query Handlers
    /// Tests sprint retrieval by project and team
    /// </summary>
    public class SprintQueryHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IMapper> _mapperMock;

        public SprintQueryHandlerTests()
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

        #region GetSprintsByProjectIdQueryHandler Tests

        [Fact]
        public async Task GetSprintsByProjectIdQueryHandler_WithValidProjectId_ReturnsSprints()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var sprints = new List<Sprint>
            {
                new Sprint 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Sprint 1", 
                    ProjectId = projectId,
                    Status = "ACTIVE",
                    CreatedAt = DateTimeOffset.Now.AddDays(-3)
                },
                new Sprint 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Sprint 2", 
                    ProjectId = projectId,
                    Status = "PLANNED",
                    CreatedAt = DateTimeOffset.Now.AddDays(-1)
                }
            };

            _context.Sprints.AddRange(sprints);
            await _context.SaveChangesAsync();

            var sprintDtos = new List<SprintDto>
            {
                new SprintDto { Id = sprints[1].Id, Name = "Sprint 2", Status = "PLANNED" },
                new SprintDto { Id = sprints[0].Id, Name = "Sprint 1", Status = "ACTIVE" }
            };

            _mapperMock
                .Setup(m => m.Map<List<SprintDto>>(It.IsAny<List<Sprint>>()))
                .Returns(sprintDtos);

            var handler = new GetSprintsByProjectIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByProjectIdQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);

            _mapperMock.Verify(m => m.Map<List<SprintDto>>(It.IsAny<List<Sprint>>()), Times.Once);
        }

        [Fact]
        public async Task GetSprintsByProjectIdQueryHandler_WithNoSprints_ReturnsFail()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var handler = new GetSprintsByProjectIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByProjectIdQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("No sprints found", result.Message);

            _mapperMock.Verify(m => m.Map<List<SprintDto>>(It.IsAny<List<Sprint>>()), Times.Never);
        }

        [Fact]
        public async Task GetSprintsByProjectIdQueryHandler_OrdersByCreatedAtDescending()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var sprints = new List<Sprint>
            {
                new Sprint 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Oldest", 
                    ProjectId = projectId,
                    CreatedAt = DateTimeOffset.Now.AddDays(-5)
                },
                new Sprint 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Newest", 
                    ProjectId = projectId,
                    CreatedAt = DateTimeOffset.Now
                },
                new Sprint 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Middle", 
                    ProjectId = projectId,
                    CreatedAt = DateTimeOffset.Now.AddDays(-2)
                }
            };

            _context.Sprints.AddRange(sprints);
            await _context.SaveChangesAsync();

            var handler = new GetSprintsByProjectIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByProjectIdQuery(projectId);

            // Setup mapper to pass through
            _mapperMock
                .Setup(m => m.Map<List<SprintDto>>(It.IsAny<List<Sprint>>()))
                .Returns<List<Sprint>>(s => s.Select(sprint => new SprintDto 
                { 
                    Id = sprint.Id, 
                    Name = sprint.Name 
                }).ToList());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal("Newest", result.Data[0].Name);
            Assert.Equal("Middle", result.Data[1].Name);
            Assert.Equal("Oldest", result.Data[2].Name);
        }

        [Fact]
        public async Task GetSprintsByProjectIdQueryHandler_WithEmptyGuid_ReturnsNoSprints()
        {
            // Arrange
            var projectId = Guid.Empty;

            var handler = new GetSprintsByProjectIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByProjectIdQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region GetSprintsByTeamIdQueryHandler Tests

        [Fact]
        public async Task GetSprintsByTeamIdQueryHandler_WithValidTeamId_ReturnsSprints()
        {
            // Arrange
            var teamId = 1;
            var projectId = Guid.NewGuid();

            var sprints = new List<Sprint>
            {
                new Sprint 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Team Sprint 1", 
                    TeamId = teamId,
                    ProjectId = projectId,
                    Status = "ACTIVE",
                    CreatedAt = DateTimeOffset.Now.AddDays(-2)
                },
                new Sprint 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Team Sprint 2", 
                    TeamId = teamId,
                    ProjectId = projectId,
                    Status = "COMPLETED",
                    CreatedAt = DateTimeOffset.Now
                }
            };

            _context.Sprints.AddRange(sprints);
            await _context.SaveChangesAsync();

            var sprintDtos = new List<SprintDto>
            {
                new SprintDto { Id = sprints[1].Id, Name = "Team Sprint 2" },
                new SprintDto { Id = sprints[0].Id, Name = "Team Sprint 1" }
            };

            _mapperMock
                .Setup(m => m.Map<List<SprintDto>>(It.IsAny<List<Sprint>>()))
                .Returns(sprintDtos);

            var handler = new GetSprintsByTeamIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByTeamIdQuery(teamId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetSprintsByTeamIdQueryHandler_WithNoSprints_ReturnsFail()
        {
            // Arrange
            var teamId = 999;

            var handler = new GetSprintsByTeamIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByTeamIdQuery(teamId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("No sprints found", result.Message);
        }

        [Fact]
        public async Task GetSprintsByTeamIdQueryHandler_WithZeroTeamId_ReturnsNoSprints()
        {
            // Arrange
            var teamId = 0;

            var handler = new GetSprintsByTeamIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByTeamIdQuery(teamId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        [Fact]
        public async Task GetSprintsByTeamIdQueryHandler_FiltersCorrectly()
        {
            // Arrange
            var teamId1 = 1;
            var teamId2 = 2;
            var projectId = Guid.NewGuid();

            var sprints = new List<Sprint>
            {
                new Sprint { Id = Guid.NewGuid(), Name = "Team 1 Sprint", TeamId = teamId1, ProjectId = projectId, CreatedAt = DateTimeOffset.Now },
                new Sprint { Id = Guid.NewGuid(), Name = "Team 2 Sprint", TeamId = teamId2, ProjectId = projectId, CreatedAt = DateTimeOffset.Now }
            };

            _context.Sprints.AddRange(sprints);
            await _context.SaveChangesAsync();

            var sprintDtos = new List<SprintDto>
            {
                new SprintDto { Id = sprints[0].Id, Name = "Team 1 Sprint" }
            };

            _mapperMock
                .Setup(m => m.Map<List<SprintDto>>(It.IsAny<List<Sprint>>()))
                .Returns(sprintDtos);

            var handler = new GetSprintsByTeamIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetSprintsByTeamIdQuery(teamId1);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("Team 1 Sprint", result.Data[0].Name);
        }

        #endregion
    }
}
