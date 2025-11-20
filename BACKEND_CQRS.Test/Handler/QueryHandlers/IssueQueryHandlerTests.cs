using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Issues;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
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
    /// Unit tests for Issue Query Handlers
    /// Comprehensive tests for issue retrieval operations
    /// </summary>
    public class IssueQueryHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IIssueRepository> _issueRepositoryMock;
        private readonly Mock<IStatusRepository> _statusRepositoryMock;

        public IssueQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _issueRepositoryMock = new Mock<IIssueRepository>();
            _statusRepositoryMock = new Mock<IStatusRepository>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetIssuesByUserIdQueryHandler Tests

        [Fact]
        public async Task GetIssuesByUserIdQueryHandler_WithValidUserId_ReturnsUserIssues()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();

            var issues = new List<Issue>
            {
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Issue 1", 
                    AssigneeId = userId,
                    ProjectId = projectId,
                    Type = "Bug",
                    StatusId = 1
                },
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Issue 2", 
                    AssigneeId = userId,
                    ProjectId = projectId,
                    Type = "Task",
                    StatusId = 1
                }
            };

            _context.Issues.AddRange(issues);
            await _context.SaveChangesAsync();

            var issueDtos = new List<IssueDto>
            {
                new IssueDto { Id = issues[0].Id, Title = "Issue 1", IssueType = "Bug" },
                new IssueDto { Id = issues[1].Id, Title = "Issue 2", IssueType = "Task" }
            };

            _mapperMock
                .Setup(m => m.Map<List<IssueDto>>(It.IsAny<List<Issue>>()))
                .Returns(issueDtos);

            var handler = new GetIssuesByUserIdQueryHandler(_mapperMock.Object, _context);
            var query = new GetIssuesByUserIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Issue 1", result.Data[0].Title);

            _mapperMock.Verify(m => m.Map<List<IssueDto>>(It.IsAny<List<Issue>>()), Times.Once);
        }

        [Fact]
        public async Task GetIssuesByUserIdQueryHandler_WithNoIssues_ReturnsFail()
        {
            // Arrange
            var userId = 999;

            var handler = new GetIssuesByUserIdQueryHandler(_mapperMock.Object, _context);
            var query = new GetIssuesByUserIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("No issues found", result.Message);

            _mapperMock.Verify(m => m.Map<List<IssueDto>>(It.IsAny<List<Issue>>()), Times.Never);
        }

        [Fact]
        public async Task GetIssuesByUserIdQueryHandler_WithZeroUserId_ReturnsNoIssues()
        {
            // Arrange
            var userId = 0;

            var handler = new GetIssuesByUserIdQueryHandler(_mapperMock.Object, _context);
            var query = new GetIssuesByUserIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region GetIssueBySprintProjectIdQueryHandler Tests

        [Fact]
        public async Task GetIssueBySprintProjectIdQueryHandler_WithValidIds_ReturnsIssues()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();

            var issues = new List<Issue>
            {
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Sprint Issue 1", 
                    ProjectId = projectId,
                    SprintId = sprintId,
                    Type = "Story",
                    StatusId = 1
                },
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Sprint Issue 2", 
                    ProjectId = projectId,
                    SprintId = sprintId,
                    Type = "Bug",
                    StatusId = 1
                }
            };

            _context.Issues.AddRange(issues);
            await _context.SaveChangesAsync();

            var issueDtos = new List<IssueDto>
            {
                new IssueDto { Id = issues[0].Id, Title = "Sprint Issue 1" },
                new IssueDto { Id = issues[1].Id, Title = "Sprint Issue 2" }
            };

            _mapperMock
                .Setup(m => m.Map<List<IssueDto>>(It.IsAny<List<Issue>>()))
                .Returns(issueDtos);

            var handler = new GetIssueBySprintProjectIdQueryHandler(_mapperMock.Object, _issueRepositoryMock.Object, _context);
            var query = new GetIssueBySprintProjectIdQuery(projectId, sprintId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetIssueBySprintProjectIdQueryHandler_WithProjectIdOnly_ReturnsAllProjectIssues()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var issues = new List<Issue>
            {
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Project Issue 1", 
                    ProjectId = projectId,
                    SprintId = null,
                    Type = "Bug",
                    StatusId = 1
                },
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Project Issue 2", 
                    ProjectId = projectId,
                    SprintId = Guid.NewGuid(),
                    Type = "Story",
                    StatusId = 1
                }
            };

            _context.Issues.AddRange(issues);
            await _context.SaveChangesAsync();

            var issueDtos = new List<IssueDto>
            {
                new IssueDto { Id = issues[0].Id, Title = "Project Issue 1" },
                new IssueDto { Id = issues[1].Id, Title = "Project Issue 2" }
            };

            _mapperMock
                .Setup(m => m.Map<List<IssueDto>>(It.IsAny<List<Issue>>()))
                .Returns(issueDtos);

            var handler = new GetIssueBySprintProjectIdQueryHandler(_mapperMock.Object, _issueRepositoryMock.Object, _context);
            var query = new GetIssueBySprintProjectIdQuery(projectId, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetIssueBySprintProjectIdQueryHandler_WithNoIssues_ReturnsFail()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();

            var handler = new GetIssueBySprintProjectIdQueryHandler(_mapperMock.Object, _issueRepositoryMock.Object, _context);
            var query = new GetIssueBySprintProjectIdQuery(projectId, sprintId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("No issues found", result.Message);
        }

        #endregion

        #region GetRecentIssuesByProjectIdQueryHandler Tests

        [Fact]
        public async Task GetRecentIssuesQueryHandler_WithValidProjectId_ReturnsRecentIssues()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var issues = new List<Issue>
            {
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Recent 1", 
                    ProjectId = projectId,
                    Type = "Bug",
                    StatusId = 1,
                    UpdatedAt = DateTimeOffset.Now.AddDays(-1)
                },
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Recent 2", 
                    ProjectId = projectId,
                    Type = "Story",
                    StatusId = 1,
                    UpdatedAt = DateTimeOffset.Now.AddDays(-2)
                },
                new Issue 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Recent 3", 
                    ProjectId = projectId,
                    Type = "Task",
                    StatusId = 1,
                    UpdatedAt = DateTimeOffset.Now
                }
            };

            _context.Issues.AddRange(issues);
            await _context.SaveChangesAsync();

            var issueDtos = new List<IssueDto>
            {
                new IssueDto { Id = issues[2].Id, Title = "Recent 3" },
                new IssueDto { Id = issues[0].Id, Title = "Recent 1" }
            };

            _mapperMock
                .Setup(m => m.Map<List<IssueDto>>(It.IsAny<List<Issue>>()))
                .Returns(issueDtos);

            var handler = new GetRecentIssuesByProjectIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetRecentIssuesQuery { ProjectId = projectId, Count = 2 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetRecentIssuesQueryHandler_WithNoIssues_ReturnsFail()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var handler = new GetRecentIssuesByProjectIdQueryHandler(_context, _mapperMock.Object);
            var query = new GetRecentIssuesQuery { ProjectId = projectId, Count = 6 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("No recent issues found", result.Message);
        }

        #endregion

        #region GetCompletedIssueCountByProjectQueryHandler Tests

        [Fact]
        public async Task GetCompletedIssueCountByProjectQueryHandler_WithCompletedIssues_ReturnsCount()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var doneStatus = new Status { Id = 1, StatusName = "DONE" };

            var completedIssues = new List<Issue>
            {
                new Issue { Id = Guid.NewGuid(), ProjectId = projectId, StatusId = 1, Title = "Done 1", Type = "Bug" },
                new Issue { Id = Guid.NewGuid(), ProjectId = projectId, StatusId = 1, Title = "Done 2", Type = "Story" },
                new Issue { Id = Guid.NewGuid(), ProjectId = projectId, StatusId = 1, Title = "Done 3", Type = "Task" }
            };

            _statusRepositoryMock
                .Setup(repo => repo.GetStatusByNameAsync("DONE"))
                .ReturnsAsync(doneStatus);

            _issueRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Issue, bool>>>()))
                .ReturnsAsync(completedIssues);

            var handler = new GetCompletedIssueCountByProjectQueryHandler(_issueRepositoryMock.Object, _statusRepositoryMock.Object);
            var query = new GetCompletedIssueCountByProjectQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(3, result.Data);
            Assert.Contains("3 completed issue(s)", result.Message);

            _statusRepositoryMock.Verify(repo => repo.GetStatusByNameAsync("DONE"), Times.Once);
            _issueRepositoryMock.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Issue, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetCompletedIssueCountByProjectQueryHandler_WithNoCompletedIssues_ReturnsZero()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var doneStatus = new Status { Id = 1, StatusName = "DONE" };

            _statusRepositoryMock
                .Setup(repo => repo.GetStatusByNameAsync("DONE"))
                .ReturnsAsync(doneStatus);

            _issueRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Issue, bool>>>()))
                .ReturnsAsync(new List<Issue>());

            var handler = new GetCompletedIssueCountByProjectQueryHandler(_issueRepositoryMock.Object, _statusRepositoryMock.Object);
            var query = new GetCompletedIssueCountByProjectQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(0, result.Data);
        }

        [Fact]
        public async Task GetCompletedIssueCountByProjectQueryHandler_WhenDoneStatusNotFound_ReturnsFail()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _statusRepositoryMock
                .Setup(repo => repo.GetStatusByNameAsync("DONE"))
                .ReturnsAsync((Status)null!);

            var handler = new GetCompletedIssueCountByProjectQueryHandler(_issueRepositoryMock.Object, _statusRepositoryMock.Object);
            var query = new GetCompletedIssueCountByProjectQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("DONE status not found", result.Message);

            _issueRepositoryMock.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Issue, bool>>>()), Times.Never);
        }

        #endregion

        #region GetIssueCountByTypeBySprintProjectIdQueryHandler Tests

        [Fact]
        public async Task GetIssueCountByTypeQueryHandler_WithValidIds_ReturnsCounts()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();

            var issues = new List<Issue>
            {
                new Issue { Id = Guid.NewGuid(), ProjectId = projectId, SprintId = sprintId, Type = "Bug", Title = "Bug 1", StatusId = 1 },
                new Issue { Id = Guid.NewGuid(), ProjectId = projectId, SprintId = sprintId, Type = "Bug", Title = "Bug 2", StatusId = 1 },
                new Issue { Id = Guid.NewGuid(), ProjectId = projectId, SprintId = sprintId, Type = "Story", Title = "Story 1", StatusId = 1 },
                new Issue { Id = Guid.NewGuid(), ProjectId = projectId, SprintId = sprintId, Type = "Task", Title = "Task 1", StatusId = 1 }
            };

            _issueRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Issue, bool>>>()))
                .ReturnsAsync(issues);

            var handler = new GetIssueCountByTypeBySprintProjectIdQueryHandler(_issueRepositoryMock.Object);
            var query = new GetIssueCountByTypeByProjectSprintQuery(projectId, sprintId); // FIXED: Use constructor

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data["Bug"]);
            Assert.Equal(1, result.Data["Story"]);
            Assert.Equal(1, result.Data["Task"]);
        }

        [Fact]
        public async Task GetIssueCountByTypeQueryHandler_WithNoIssues_ReturnsFail()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _issueRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Issue, bool>>>()))
                .ReturnsAsync(new List<Issue>());

            var handler = new GetIssueCountByTypeBySprintProjectIdQueryHandler(_issueRepositoryMock.Object);
            var query = new GetIssueCountByTypeByProjectSprintQuery(projectId, null); // FIXED: Use constructor

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("No issues found", result.Message);
        }

        #endregion
    }
}
