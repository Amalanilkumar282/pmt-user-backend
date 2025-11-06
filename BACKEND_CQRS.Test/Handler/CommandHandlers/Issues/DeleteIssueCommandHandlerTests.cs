using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Handler.Issues;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BACKEND_CQRS.Test.Handler.CommandHandlers.Issues
{
    /// <summary>
    /// Unit tests for DeleteIssueCommandHandler
    /// </summary>
    public class DeleteIssueCommandHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;

        public DeleteIssueCommandHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Handle_WithValidIssueId_DeletesIssueSuccessfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var issue = new Issue
            {
                Id = issueId,
                Title = "Issue to Delete",
                Description = "This issue will be deleted",
                Type = "Bug",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
            Assert.Equal("Issue deleted successfully", result.Message);

            // Verify issue was removed from database
            var deletedIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueId);
            Assert.Null(deletedIssue);
        }

        [Fact]
        public async Task Handle_WithNonExistentIssueId_ReturnsFail()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var command = new DeleteIssueCommand(nonExistentId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.False(result.Data);
            Assert.Contains("not found", result.Message);
            Assert.Contains(nonExistentId.ToString(), result.Message);
        }

        [Fact]
        public async Task Handle_DeletesBugIssue_Successfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var bugIssue = new Issue
            {
                Id = issueId,
                Title = "Bug to Delete",
                Type = "Bug",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                Priority = "High",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(bugIssue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);

            var deletedIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueId);
            Assert.Null(deletedIssue);
        }

        [Fact]
        public async Task Handle_DeletesStoryIssue_Successfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var storyIssue = new Issue
            {
                Id = issueId,
                Title = "Story to Delete",
                Type = "Story",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                StoryPoints = 5,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(storyIssue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task Handle_DeletesTaskIssue_Successfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var taskIssue = new Issue
            {
                Id = issueId,
                Title = "Task to Delete",
                Type = "Task",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(taskIssue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task Handle_DeletesIssueWithAssignee_Successfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var issue = new Issue
            {
                Id = issueId,
                Title = "Assigned Issue",
                Type = "Bug",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                AssigneeId = 5,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);

            var deletedIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueId);
            Assert.Null(deletedIssue);
        }

        [Fact]
        public async Task Handle_DeletesIssueInSprint_Successfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();
            var issue = new Issue
            {
                Id = issueId,
                Title = "Sprint Issue",
                Type = "Story",
                ProjectId = Guid.NewGuid(),
                SprintId = sprintId,
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task Handle_DeletesIssueInEpic_Successfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var epicId = Guid.NewGuid();
            var issue = new Issue
            {
                Id = issueId,
                Title = "Epic Issue",
                Type = "Story",
                ProjectId = Guid.NewGuid(),
                EpicId = epicId,
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task Handle_DeletesOneIssue_LeavesOthersUntouched()
        {
            // Arrange
            var issueToDelete = new Issue
            {
                Id = Guid.NewGuid(),
                Title = "Issue to Delete",
                Type = "Bug",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var issueToKeep = new Issue
            {
                Id = Guid.NewGuid(),
                Title = "Issue to Keep",
                Type = "Story",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.AddRange(issueToDelete, issueToKeep);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueToDelete.Id); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);

            var deletedIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueToDelete.Id);
            Assert.Null(deletedIssue);

            var keptIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueToKeep.Id);
            Assert.NotNull(keptIssue);
            Assert.Equal("Issue to Keep", keptIssue.Title);
        }

        [Fact]
        public async Task Handle_DeletesMultipleIssues_Sequentially()
        {
            // Arrange
            var issue1 = new Issue
            {
                Id = Guid.NewGuid(),
                Title = "Issue 1",
                Type = "Bug",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var issue2 = new Issue
            {
                Id = Guid.NewGuid(),
                Title = "Issue 2",
                Type = "Story",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.AddRange(issue1, issue2);
            await _context.SaveChangesAsync();

            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result1 = await handler.Handle(new DeleteIssueCommand(issue1.Id), CancellationToken.None); // FIXED
            var result2 = await handler.Handle(new DeleteIssueCommand(issue2.Id), CancellationToken.None); // FIXED

            // Assert
            Assert.Equal(200, result1.Status);
            Assert.Equal(200, result2.Status);

            var remainingIssues = await _context.Issues.ToListAsync();
            Assert.Empty(remainingIssues);
        }

        [Fact]
        public async Task Handle_DeletesIssueWithComplexData_Successfully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var issue = new Issue
            {
                Id = issueId,
                Title = "Complex Issue",
                Description = "Detailed description",
                Type = "Story",
                ProjectId = Guid.NewGuid(),
                SprintId = Guid.NewGuid(),
                EpicId = Guid.NewGuid(),
                StatusId = 2,
                Priority = "Critical",
                AssigneeId = 3,
                ReporterId = 1,
                StoryPoints = 8,
                StartDate = DateTimeOffset.UtcNow,
                DueDate = DateTimeOffset.UtcNow.AddDays(7),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);

            var deletedIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueId);
            Assert.Null(deletedIssue);
        }

        [Fact]
        public async Task Handle_WithCancellationToken_HandlesGracefully()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var issue = new Issue
            {
                Id = issueId,
                Title = "Issue for Cancellation Test",
                Type = "Bug",
                ProjectId = Guid.NewGuid(),
                StatusId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var command = new DeleteIssueCommand(issueId); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);
            var cts = new CancellationTokenSource();

            // Act
            var result = await handler.Handle(command, cts.Token);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task Handle_VerifiesIssueCount_AfterDeletion()
        {
            // Arrange
            var issues = new[]
            {
                new Issue { Id = Guid.NewGuid(), Title = "Issue 1", Type = "Bug", ProjectId = Guid.NewGuid(), StatusId = 1, CreatedAt = DateTimeOffset.UtcNow },
                new Issue { Id = Guid.NewGuid(), Title = "Issue 2", Type = "Story", ProjectId = Guid.NewGuid(), StatusId = 1, CreatedAt = DateTimeOffset.UtcNow },
                new Issue { Id = Guid.NewGuid(), Title = "Issue 3", Type = "Task", ProjectId = Guid.NewGuid(), StatusId = 1, CreatedAt = DateTimeOffset.UtcNow }
            };

            _context.Issues.AddRange(issues);
            await _context.SaveChangesAsync();

            var initialCount = await _context.Issues.CountAsync();
            Assert.Equal(3, initialCount);

            var command = new DeleteIssueCommand(issues[1].Id); // FIXED: Use constructor
            var handler = new DeleteIssueCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(200, result.Status);

            var finalCount = await _context.Issues.CountAsync();
            Assert.Equal(2, finalCount);
        }
    }
}
