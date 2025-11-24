using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Sprints;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BACKEND_CQRS.Test.Handler.CommandHandlers.Sprints
{
    /// <summary>
    /// Unit tests for Sprint Command Handlers (Create, Update, Delete, Complete)
    /// </summary>
    public class SprintCommandHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IMapper> _mapperMock;

        public SprintCommandHandlerTests()
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

        #region CreateSprintCommandHandler Tests

        [Fact]
        public async Task CreateSprint_WithValidData_CreatesSuccessfully()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new CreateSprintCommand
            {
                SprintName = "Sprint 1",
                ProjectId = projectId,
                TeamAssigned = 1,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                SprintGoal = "Complete user stories"
            };

            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = command.SprintName,
                ProjectId = projectId,
                TeamId = command.TeamAssigned,
                StartDate = DateTime.UtcNow, // FIXED: Changed to DateTime
                DueDate = DateTime.UtcNow.AddDays(14), // FIXED: Changed to DateTime
                SprintGoal = command.SprintGoal
            };

            var sprintDto = new CreateSprintDto
            {
                ProjectId = sprint.ProjectId,
                SprintGoal = sprint.SprintGoal
            };

            _mapperMock.Setup(m => m.Map<Sprint>(command)).Returns(sprint);
            _mapperMock.Setup(m => m.Map<CreateSprintDto>(It.IsAny<Sprint>())).Returns(sprintDto);

            var handler = new CreateSprintCommandHandler(_context, _mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
            Assert.Equal("Sprint created successfully", result.Message);
            Assert.NotNull(result.Data);

            var savedSprint = await _context.Sprints.FirstOrDefaultAsync();
            Assert.NotNull(savedSprint);
            Assert.Equal(command.SprintName, savedSprint.Name);
        }

        [Fact]
        public async Task CreateSprint_GeneratesUniqueId()
        {
            // Arrange
            var command = new CreateSprintCommand
            {
                SprintName = "Sprint Test",
                ProjectId = Guid.NewGuid(),
                TeamAssigned = 1
            };

            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = command.SprintName,
                ProjectId = command.ProjectId,
                TeamId = command.TeamAssigned
            };

            _mapperMock.Setup(m => m.Map<Sprint>(command)).Returns(sprint);
            _mapperMock.Setup(m => m.Map<CreateSprintDto>(It.IsAny<Sprint>()))
                .Returns(new CreateSprintDto { ProjectId = sprint.ProjectId });

            var handler = new CreateSprintCommandHandler(_context, _mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task CreateSprint_SetsCreatedAtTimestamp()
        {
            // Arrange
            var beforeCreate = DateTimeOffset.UtcNow;
            var command = new CreateSprintCommand
            {
                SprintName = "Sprint Timestamp",
                ProjectId = Guid.NewGuid(),
                TeamAssigned = 1
            };

            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = command.SprintName,
                ProjectId = command.ProjectId,
                TeamId = command.TeamAssigned,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _mapperMock.Setup(m => m.Map<Sprint>(command)).Returns(sprint);
            _mapperMock.Setup(m => m.Map<CreateSprintDto>(It.IsAny<Sprint>()))
                .Returns(new CreateSprintDto());

            var handler = new CreateSprintCommandHandler(_context, _mapperMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);
            var afterCreate = DateTimeOffset.UtcNow;

            // Assert
            var savedSprint = await _context.Sprints.FirstOrDefaultAsync();
            Assert.True(savedSprint.CreatedAt >= beforeCreate);
            Assert.True(savedSprint.CreatedAt <= afterCreate);
        }

        #endregion

        #region UpdateSprintCommandHandler Tests

        [Fact]
        public async Task UpdateSprint_WithValidData_UpdatesSuccessfully()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var sprint = new Sprint
            {
                Id = sprintId,
                Name = "Old Sprint Name",
                ProjectId = Guid.NewGuid(),
                TeamId = 1,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var command = new UpdateSprintCommand
            {
                Id = sprintId,
                SprintName = "Updated Sprint Name", // FIXED: Property name
                SprintGoal = "New Goal",
                Status = "COMPLETED"
            };

            var handler = new UpdateSprintCommandHandler(_context, _mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Contains("updated successfully", result.Message);

            var updatedSprint = await _context.Sprints.FindAsync(sprintId);
            Assert.Equal("Updated Sprint Name", updatedSprint.Name);
            Assert.Equal("New Goal", updatedSprint.SprintGoal);
        }

        [Fact]
        public async Task UpdateSprint_WithNonExistentId_ReturnsFail()
        {
            // Arrange
            var command = new UpdateSprintCommand
            {
                Id = Guid.NewGuid(),
                SprintName = "New Name" // FIXED: Property name
            };

            var handler = new UpdateSprintCommandHandler(_context, _mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task UpdateSprint_UpdatesOnlyProvidedFields()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var originalGoal = "Original Goal";
            var sprint = new Sprint
            {
                Id = sprintId,
                Name = "Sprint Name",
                ProjectId = Guid.NewGuid(),
                SprintGoal = originalGoal,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var command = new UpdateSprintCommand
            {
                Id = sprintId,
                SprintName = "Updated Name" // FIXED: Property name
                // Not updating SprintGoal
            };

            var handler = new UpdateSprintCommandHandler(_context, _mapperMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedSprint = await _context.Sprints.FindAsync(sprintId);
            Assert.Equal("Updated Name", updatedSprint.Name);
            Assert.Equal(originalGoal, updatedSprint.SprintGoal);
        }

        #endregion

        #region DeleteSprintCommandHandler Tests

        [Fact]
        public async Task DeleteSprint_WithValidId_DeletesSuccessfully()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var sprint = new Sprint
            {
                Id = sprintId,
                Name = "Sprint to Delete",
                ProjectId = Guid.NewGuid(),
                TeamId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var command = new DeleteSprintCommand(sprintId); // FIXED: Use constructor
            var handler = new DeleteSprintCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
            Assert.Contains("deleted successfully", result.Message);

            var deletedSprint = await _context.Sprints.FindAsync(sprintId);
            Assert.Null(deletedSprint);
        }

        [Fact]
        public async Task DeleteSprint_WithNonExistentId_ReturnsFail()
        {
            // Arrange
            var command = new DeleteSprintCommand(Guid.NewGuid()); // FIXED: Use constructor
            var handler = new DeleteSprintCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.False(result.Data);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task DeleteSprint_RemovesFromDatabase()
        {
            // Arrange
            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = "Sprint",
                ProjectId = Guid.NewGuid(),
                TeamId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var initialCount = await _context.Sprints.CountAsync();
            Assert.Equal(1, initialCount);

            var command = new DeleteSprintCommand(sprint.Id); // FIXED: Use constructor
            var handler = new DeleteSprintCommandHandler(_context);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var finalCount = await _context.Sprints.CountAsync();
            Assert.Equal(0, finalCount);
        }

        #endregion

        #region CompleteSprintCommandHandler Tests

        [Fact]
        public async Task CompleteSprint_WithValidId_MarksAsCompleted()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var sprint = new Sprint
            {
                Id = sprintId,
                Name = "Active Sprint",
                ProjectId = Guid.NewGuid(),
                Status = "ACTIVE",
                TeamId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var command = new CompleteSprintCommand(sprintId); // FIXED: Use constructor
            var handler = new CompleteSprintCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
            Assert.Contains("completed successfully", result.Message);

            var completedSprint = await _context.Sprints.FindAsync(sprintId);
            Assert.Equal("COMPLETED", completedSprint.Status);
        }

        [Fact]
        public async Task CompleteSprint_WithNonExistentId_ReturnsFail()
        {
            // Arrange
            var command = new CompleteSprintCommand(Guid.NewGuid()); // FIXED: Use constructor
            var handler = new CompleteSprintCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task CompleteSprint_AlreadyCompleted_ReturnsSuccessMessage()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var sprint = new Sprint
            {
                Id = sprintId,
                Name = "Already Completed Sprint",
                ProjectId = Guid.NewGuid(),
                Status = "COMPLETED",
                TeamId = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var command = new CompleteSprintCommand(sprintId); // FIXED: Use constructor
            var handler = new CompleteSprintCommandHandler(_context);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Contains("already completed", result.Message);
        }

        [Fact]
        public async Task CompleteSprint_UpdatesUpdatedAtTimestamp()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var sprint = new Sprint
            {
                Id = sprintId,
                Name = "Sprint",
                ProjectId = Guid.NewGuid(),
                Status = "ACTIVE",
                TeamId = 1,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var beforeComplete = DateTimeOffset.UtcNow;
            var command = new CompleteSprintCommand(sprintId); // FIXED: Use constructor
            var handler = new CompleteSprintCommandHandler(_context);

            // Act
            await handler.Handle(command, CancellationToken.None);
            var afterComplete = DateTimeOffset.UtcNow;

            // Assert
            var completedSprint = await _context.Sprints.FindAsync(sprintId);
            Assert.True(completedSprint.UpdatedAt >= beforeComplete);
            Assert.True(completedSprint.UpdatedAt <= afterComplete);
        }

        #endregion
    }
}
