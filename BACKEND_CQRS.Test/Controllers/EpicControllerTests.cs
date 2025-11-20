using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Epic;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for EpicController
    /// Tests all API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class EpicControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly EpicController _controller;

        public EpicControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new EpicController(_mediatorMock.Object);
        }

        #region CreateEpic Tests

        [Fact]
        public async Task CreateEpic_WithValidCommand_ReturnsCreatedResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new CreateEpicCommand
            {
                ProjectId = projectId,
                Title = "User Authentication Epic",
                Description = "Implement complete user authentication system",
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                AssigneeId = 1,
                ReporterId = 2,
                Labels = new List<string> { "Security", "High Priority" }
            };

            var expectedResponse = ApiResponse<CreateEpicDto>.Created(
                new CreateEpicDto
                {
                    ProjectId = projectId,
                    Title = "User Authentication Epic",
                    ReporterId = 2
                },
                "Epic created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
            Assert.Equal("User Authentication Epic", result.Data.Title);
            Assert.Contains("created successfully", result.Message);
        }

        [Fact]
        public async Task CreateEpic_WithMinimalData_ReturnsSuccess()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new CreateEpicCommand
            {
                ProjectId = projectId,
                Title = "Minimal Epic",
                ReporterId = 1
            };

            var expectedResponse = ApiResponse<CreateEpicDto>.Created(
                new CreateEpicDto
                {
                    ProjectId = projectId,
                    Title = "Minimal Epic",
                    ReporterId = 1
                },
                "Epic created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        [Fact]
        public async Task CreateEpic_WithNullLabels_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateEpicCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "No Labels Epic",
                ReporterId = 1,
                Labels = null
            };

            var expectedResponse = ApiResponse<CreateEpicDto>.Created(
                new CreateEpicDto { ProjectId = command.ProjectId, Title = "No Labels Epic", ReporterId = 1 },
                "Epic created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        [Fact]
        public async Task CreateEpic_WithEmptyDescription_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateEpicCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "Epic without Description",
                ReporterId = 1,
                Description = null
            };

            var expectedResponse = ApiResponse<CreateEpicDto>.Created(
                new CreateEpicDto { ProjectId = command.ProjectId, Title = "Epic without Description", ReporterId = 1 },
                "Epic created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        #endregion

        #region UpdateEpic Tests

        [Fact]
        public async Task UpdateEpic_WithValidCommand_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var command = new UpdateEpicCommand
            {
                Id = epicId,
                Title = "Updated Epic Title",
                Description = "Updated description",
                AssigneeId = 5
            };

            var expectedResponse = ApiResponse<Guid>.Success(epicId, "Epic updated successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(epicId, result.Data);
            Assert.Contains("updated successfully", result.Message);
        }

        [Fact]
        public async Task UpdateEpic_WithPartialUpdate_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var command = new UpdateEpicCommand
            {
                Id = epicId,
                Title = "Only Title Updated"
                // Other fields are null/not provided
            };

            var expectedResponse = ApiResponse<Guid>.Success(epicId, "Epic updated successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(epicId, result.Data);
        }

        [Fact]
        public async Task UpdateEpic_WhenEpicNotFound_ReturnsFail()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var command = new UpdateEpicCommand { Id = epicId, Title = "Test" };

            var expectedResponse = ApiResponse<Guid>.Fail("Epic not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task UpdateEpic_WithNullTitle_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var command = new UpdateEpicCommand
            {
                Id = epicId,
                Title = null,
                Description = "Only update description"
            };

            var expectedResponse = ApiResponse<Guid>.Success(epicId, "Epic updated successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateEpicCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateEpic(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
        }

        #endregion

        #region UpdateEpicDates Tests

        [Fact]
        public async Task UpdateEpicDates_WithValidDates_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var command = new UpdateEpicDatesCommand
            {
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30)
            };

            var expectedResponse = ApiResponse<Guid>.Success(epicId, "Epic dates updated successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<UpdateEpicDatesCommand>(c => c.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateEpicDates(epicId, command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(epicId, result.Data);
        }

        [Fact]
        public async Task UpdateEpicDates_SetsEpicIdFromRoute_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var command = new UpdateEpicDatesCommand
            {
                EpicId = Guid.Empty, // Will be overridden
                StartDate = DateTime.UtcNow
            };

            var expectedResponse = ApiResponse<Guid>.Success(epicId, "Epic dates updated successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<UpdateEpicDatesCommand>(c => c.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateEpicDates(epicId, command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            _mediatorMock.Verify(m => m.Send(It.Is<UpdateEpicDatesCommand>(c => c.EpicId == epicId), default), Times.Once);
        }

        [Fact]
        public async Task UpdateEpicDates_WhenEpicNotFound_ReturnsFail()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var command = new UpdateEpicDatesCommand { StartDate = DateTime.UtcNow };

            var expectedResponse = ApiResponse<Guid>.Fail("Epic not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateEpicDatesCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateEpicDates(epicId, command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region DeleteEpic Tests

        [Fact]
        public async Task DeleteEpic_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var expectedResponse = ApiResponse<Guid>.Success(epicId, "Epic deleted successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteEpicByIdCommand>(c => c.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteEpic(epicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(epicId, result.Data);
            Assert.Contains("deleted successfully", result.Message);
        }

        [Fact]
        public async Task DeleteEpic_WhenEpicNotFound_ReturnsFail()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var expectedResponse = ApiResponse<Guid>.Fail("Epic not found");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteEpicByIdCommand>(c => c.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteEpic(epicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task DeleteEpic_WithEmptyGuid_CallsMediator()
        {
            // Arrange
            var epicId = Guid.Empty;
            var expectedResponse = ApiResponse<Guid>.Fail("Invalid epic ID");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteEpicByIdCommand>(c => c.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteEpic(epicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region GetEpicsByProjectId Tests

        [Fact]
        public async Task GetEpicsByProjectId_WithValidProjectId_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var epics = new List<EpicDto>
            {
                new EpicDto { Id = Guid.NewGuid(), Title = "Epic 1", ProjectId = projectId },
                new EpicDto { Id = Guid.NewGuid(), Title = "Epic 2", ProjectId = projectId }
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(epics);

            // Act
            var result = await _controller.GetEpicsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEpics = Assert.IsAssignableFrom<IEnumerable<EpicDto>>(okResult.Value);
            Assert.Equal(2, returnedEpics.Count());
        }

        [Fact]
        public async Task GetEpicsByProjectId_WithNoEpics_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var emptyList = new List<EpicDto>();

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetEpicsByProjectId(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains($"No epics found for project {projectId}", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task GetEpicsByProjectId_WhenNullReturned_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync((List<EpicDto>)null);

            // Act
            var result = await _controller.GetEpicsByProjectId(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetEpicsByProjectId_WithEmptyGuid_CallsMediator()
        {
            // Arrange
            var projectId = Guid.Empty;
            var epics = new List<EpicDto>();

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(epics);

            // Act
            var result = await _controller.GetEpicsByProjectId(projectId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region GetEpicById Tests

        [Fact]
        public async Task GetEpicById_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var expectedEpic = new EpicDto
            {
                Id = epicId,
                Title = "Test Epic",
                Description = "Test Description"
            };

            var expectedResponse = ApiResponse<EpicDto>.Success(expectedEpic, "Epic fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicByIdQuery>(q => q.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetEpicById(epicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("Test Epic", result.Data.Title);
        }

        [Fact]
        public async Task GetEpicById_WhenEpicNotFound_ReturnsFail()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var expectedResponse = ApiResponse<EpicDto>.Fail("Epic not found");

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicByIdQuery>(q => q.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetEpicById(epicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetEpicById_WithEmptyGuid_CallsMediator()
        {
            // Arrange
            var epicId = Guid.Empty;
            var expectedResponse = ApiResponse<EpicDto>.Fail("Invalid epic ID");

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicByIdQuery>(q => q.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetEpicById(epicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        [Fact]
        public async Task GetEpicById_WithCompleteData_ReturnsAllFields()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var expectedEpic = new EpicDto
            {
                Id = epicId,
                Title = "Complete Epic",
                Description = "Full description",
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                AssigneeName = "John Doe",
                ReporterName = "Jane Smith",
                Labels = new List<string> { "Backend", "Frontend" }
            };

            var expectedResponse = ApiResponse<EpicDto>.Success(expectedEpic, "Epic fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<GetEpicByIdQuery>(q => q.EpicId == epicId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetEpicById(epicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("John Doe", result.Data.AssigneeName);
            Assert.Equal("Jane Smith", result.Data.ReporterName);
            Assert.Equal(2, result.Data.Labels.Count);
        }

        #endregion
    }
}
