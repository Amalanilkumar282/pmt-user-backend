using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Epic;
using BACKEND_CQRS.Application.Query.Epic;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
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
    /// Unit tests for Epic Query Handlers
    /// Tests epic retrieval by project and by ID
    /// </summary>
    public class EpicQueryHandlerTests
    {
        private readonly Mock<IEpicRepository> _epicRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public EpicQueryHandlerTests()
        {
            _epicRepositoryMock = new Mock<IEpicRepository>();
            _mapperMock = new Mock<IMapper>();
        }

        #region GetEpicsByProjectIdQueryHandler Tests

        [Fact]
        public async Task GetEpicsByProjectIdQueryHandler_WithValidProjectId_ReturnsEpicList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var epics = new List<Domain.Entities.Epic>
            {
                new Domain.Entities.Epic { Id = Guid.NewGuid(), Title = "Epic 1", ProjectId = projectId },
                new Domain.Entities.Epic { Id = Guid.NewGuid(), Title = "Epic 2", ProjectId = projectId },
                new Domain.Entities.Epic { Id = Guid.NewGuid(), Title = "Epic 3", ProjectId = projectId }
            };

            var epicDtos = new List<EpicDto>
            {
                new EpicDto { Id = epics[0].Id, Title = "Epic 1" },
                new EpicDto { Id = epics[1].Id, Title = "Epic 2" },
                new EpicDto { Id = epics[2].Id, Title = "Epic 3" }
            };

            _epicRepositoryMock
                .Setup(repo => repo.GetEpicsByProjectIdAsync(projectId))
                .ReturnsAsync(epics);

            _mapperMock
                .Setup(m => m.Map<List<EpicDto>>(epics))
                .Returns(epicDtos);

            var handler = new GetEpicsByProjectIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicsByProjectIdQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Epic 1", result[0].Title);
            Assert.Equal("Epic 2", result[1].Title);

            _epicRepositoryMock.Verify(repo => repo.GetEpicsByProjectIdAsync(projectId), Times.Once);
            _mapperMock.Verify(m => m.Map<List<EpicDto>>(epics), Times.Once);
        }

        [Fact]
        public async Task GetEpicsByProjectIdQueryHandler_WithNoEpics_ReturnsEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var epics = new List<Domain.Entities.Epic>();
            var epicDtos = new List<EpicDto>();

            _epicRepositoryMock
                .Setup(repo => repo.GetEpicsByProjectIdAsync(projectId))
                .ReturnsAsync(epics);

            _mapperMock
                .Setup(m => m.Map<List<EpicDto>>(epics))
                .Returns(epicDtos);

            var handler = new GetEpicsByProjectIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicsByProjectIdQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _epicRepositoryMock.Verify(repo => repo.GetEpicsByProjectIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetEpicsByProjectIdQueryHandler_WithSingleEpic_ReturnsOneEpic()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var epics = new List<Domain.Entities.Epic>
            {
                new Domain.Entities.Epic 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Single Epic",
                    Description = "Epic description",
                    ProjectId = projectId 
                }
            };

            var epicDtos = new List<EpicDto>
            {
                new EpicDto 
                { 
                    Id = epics[0].Id, 
                    Title = "Single Epic",
                    Description = "Epic description"
                }
            };

            _epicRepositoryMock
                .Setup(repo => repo.GetEpicsByProjectIdAsync(projectId))
                .ReturnsAsync(epics);

            _mapperMock
                .Setup(m => m.Map<List<EpicDto>>(epics))
                .Returns(epicDtos);

            var handler = new GetEpicsByProjectIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicsByProjectIdQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Single Epic", result[0].Title);
            Assert.Equal("Epic description", result[0].Description);
        }

        [Fact]
        public async Task GetEpicsByProjectIdQueryHandler_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _epicRepositoryMock
                .Setup(repo => repo.GetEpicsByProjectIdAsync(projectId))
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new GetEpicsByProjectIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicsByProjectIdQuery(projectId);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task GetEpicsByProjectIdQueryHandler_WithEmptyProjectId_CallsRepository()
        {
            // Arrange
            var projectId = Guid.Empty;
            var epics = new List<Domain.Entities.Epic>();
            var epicDtos = new List<EpicDto>();

            _epicRepositoryMock
                .Setup(repo => repo.GetEpicsByProjectIdAsync(projectId))
                .ReturnsAsync(epics);

            _mapperMock
                .Setup(m => m.Map<List<EpicDto>>(epics))
                .Returns(epicDtos);

            var handler = new GetEpicsByProjectIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicsByProjectIdQuery(projectId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetEpicByIdQueryHandler Tests

        [Fact]
        public async Task GetEpicByIdQueryHandler_WithValidEpicId_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var epic = new Domain.Entities.Epic
            {
                Id = epicId,
                Title = "Test Epic",
                Description = "Epic description",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var epicDto = new EpicDto
            {
                Id = epicId,
                Title = "Test Epic",
                Description = "Epic description",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _epicRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Epic, bool>>>()))
                .ReturnsAsync(new List<Domain.Entities.Epic> { epic });

            _mapperMock
                .Setup(m => m.Map<EpicDto>(epic))
                .Returns(epicDto);

            var handler = new GetEpicByIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicByIdQuery(epicId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(epicId, result.Data.Id);
            Assert.Equal("Test Epic", result.Data.Title);

            _epicRepositoryMock.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Epic, bool>>>()), Times.Once);
            _mapperMock.Verify(m => m.Map<EpicDto>(epic), Times.Once);
        }

        [Fact]
        public async Task GetEpicByIdQueryHandler_WithInvalidEpicId_ReturnsFail()
        {
            // Arrange
            var epicId = Guid.NewGuid();

            _epicRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Epic, bool>>>()))
                .ReturnsAsync(new List<Domain.Entities.Epic>());

            var handler = new GetEpicByIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicByIdQuery(epicId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Equal("Epic not found", result.Message);
            Assert.Null(result.Data);

            _mapperMock.Verify(m => m.Map<EpicDto>(It.IsAny<Domain.Entities.Epic>()), Times.Never);
        }

        [Fact]
        public async Task GetEpicByIdQueryHandler_WithNullResult_ReturnsFail()
        {
            // Arrange
            var epicId = Guid.NewGuid();

            _epicRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Epic, bool>>>()))
                .ReturnsAsync((List<Domain.Entities.Epic>)null!);

            var handler = new GetEpicByIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicByIdQuery(epicId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Equal("Epic not found", result.Message);
        }

        [Fact]
        public async Task GetEpicByIdQueryHandler_WithEmptyGuid_ReturnsFail()
        {
            // Arrange
            var epicId = Guid.Empty;

            _epicRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Epic, bool>>>()))
                .ReturnsAsync(new List<Domain.Entities.Epic>());

            var handler = new GetEpicByIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicByIdQuery(epicId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Equal("Epic not found", result.Message);
        }

        [Fact]
        public async Task GetEpicByIdQueryHandler_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var epicId = Guid.NewGuid();

            _epicRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Epic, bool>>>()))
                .ThrowsAsync(new Exception("Database error"));

            var handler = new GetEpicByIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicByIdQuery(epicId);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task GetEpicByIdQueryHandler_MapsAllProperties_Correctly()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var startDate = DateTime.Now;
            var dueDate = DateTime.Now.AddDays(30);

            var epic = new Domain.Entities.Epic
            {
                Id = epicId,
                Title = "Complete Epic",
                Description = "Full description",
                ProjectId = projectId,
                StartDate = startDate,
                DueDate = dueDate
            };

            var epicDto = new EpicDto
            {
                Id = epicId,
                Title = "Complete Epic",
                Description = "Full description",
                ProjectId = projectId,
                StartDate = startDate,
                DueDate = dueDate
            };

            _epicRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Epic, bool>>>()))
                .ReturnsAsync(new List<Domain.Entities.Epic> { epic });

            _mapperMock
                .Setup(m => m.Map<EpicDto>(epic))
                .Returns(epicDto);

            var handler = new GetEpicByIdQueryHandler(_epicRepositoryMock.Object, _mapperMock.Object);
            var query = new GetEpicByIdQuery(epicId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal("Complete Epic", result.Data.Title);
            Assert.Equal("Full description", result.Data.Description);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.NotNull(result.Data.StartDate);
            Assert.NotNull(result.Data.DueDate);
            Assert.Equal(startDate, result.Data.StartDate); // FIXED
            Assert.Equal(dueDate, result.Data.DueDate); // FIXED
        }

        #endregion
    }
}
