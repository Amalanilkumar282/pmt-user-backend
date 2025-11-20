using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Dto.AI;
using BACKEND_CQRS.Application.Query.Sprints;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for SprintController
    /// Tests all API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class SprintControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly SprintController _controller;

        public SprintControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new SprintController(_mediatorMock.Object);
        }

        #region CreateSprint Tests

        [Fact]
        public async Task CreateSprint_WithValidCommand_ReturnsCreatedResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new CreateSprintCommand
            {
                ProjectId = projectId,
                SprintName = "Sprint 1",
                SprintGoal = "Complete user authentication",
                TeamAssigned = 1,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                Status = "Planned",
                StoryPoint = 25.5m
            };

            var expectedResponse = ApiResponse<CreateSprintDto>.Created(
                new CreateSprintDto
                {
                    ProjectId = projectId,
                    SprintName = "Sprint 1",
                    SprintGoal = "Complete user authentication",
                    TeamAssigned = 1,
                    Status = "Planned"
                },
                "Sprint created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSprintCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateSprint(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
            Assert.Equal("Sprint 1", result.Data.SprintName);
            Assert.Contains("created successfully", result.Message);
        }

        [Fact]
        public async Task CreateSprint_WithOptionalProjectId_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateSprintCommand
            {
                ProjectId = null, // Optional project ID
                SprintName = "Backlog Sprint",
                SprintGoal = "General backlog items",
                Status = "Planned"
            };

            var expectedResponse = ApiResponse<CreateSprintDto>.Created(
                new CreateSprintDto
                {
                    SprintName = "Backlog Sprint",
                    Status = "Planned"
                },
                "Sprint created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSprintCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateSprint(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        [Fact]
        public async Task CreateSprint_WithMinimalData_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateSprintCommand
            {
                SprintName = "Minimal Sprint",
                Status = "Planned"
            };

            var expectedResponse = ApiResponse<CreateSprintDto>.Created(
                new CreateSprintDto
                {
                    SprintName = "Minimal Sprint",
                    Status = "Planned"
                },
                "Sprint created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSprintCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateSprint(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        #endregion

        #region GetSprintsByProjectId Tests

        [Fact]
        public async Task GetSprintsByProjectId_WithValidProjectId_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var sprints = new List<SprintDto>
            {
                new SprintDto { Id = Guid.NewGuid(), Name = "Sprint 1", Status = "Active" },
                new SprintDto { Id = Guid.NewGuid(), Name = "Sprint 2", Status = "Planned" }
            };

            var response = ApiResponse<List<SprintDto>>.Success(sprints, "Sprints fetched successfully");
            response = new ApiResponse<List<SprintDto>>(200, sprints, "Sprints fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<GetSprintsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetSprintsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<SprintDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal(2, apiResponse.Data.Count);
        }

        [Fact]
        public async Task GetSprintsByProjectId_WithNoSprints_ReturnsOkWithEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var response = new ApiResponse<List<SprintDto>>(200, new List<SprintDto>(), "No sprints found");

            _mediatorMock.Setup(m => m.Send(It.Is<GetSprintsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetSprintsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<SprintDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Empty(apiResponse.Data);
        }

        [Fact]
        public async Task GetSprintsByProjectId_WhenProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var response = new ApiResponse<List<SprintDto>>(404, null, "Project not found");

            _mediatorMock.Setup(m => m.Send(It.Is<GetSprintsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetSprintsByProjectId(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<SprintDto>>>(notFoundResult.Value);
            Assert.Equal(404, apiResponse.Status);
        }

        [Fact]
        public async Task GetSprintsByProjectId_WithEmptyGuid_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.Empty;
            var response = new ApiResponse<List<SprintDto>>(400, null, "Invalid project ID");

            _mediatorMock.Setup(m => m.Send(It.Is<GetSprintsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetSprintsByProjectId(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region GetSprintsByTeamId Tests

        [Fact]
        public async Task GetSprintsByTeamId_WithValidTeamId_ReturnsOk()
        {
            // Arrange
            var teamId = 1;
            var sprints = new List<SprintDto>
            {
                new SprintDto { Id = Guid.NewGuid(), Name = "Team Sprint 1", TeamId = teamId }
            };

            var response = new ApiResponse<List<SprintDto>>(200, sprints, "Sprints fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<GetSprintsByTeamIdQuery>(q => q.TeamId == teamId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetSprintsByTeamId(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<SprintDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Single(apiResponse.Data);
        }

        [Fact]
        public async Task GetSprintsByTeamId_WhenTeamNotFound_ReturnsNotFound()
        {
            // Arrange
            var teamId = 999;
            var response = new ApiResponse<List<SprintDto>>(404, null, "Team not found");

            _mediatorMock.Setup(m => m.Send(It.Is<GetSprintsByTeamIdQuery>(q => q.TeamId == teamId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetSprintsByTeamId(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region UpdateSprint Tests

        [Fact]
        public async Task UpdateSprint_WithValidCommand_ReturnsOk()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var command = new UpdateSprintCommand
            {
                Id = sprintId,
                SprintName = "Updated Sprint",
                Status = "Active"
            };

            var response = new ApiResponse<SprintDto>(200, new SprintDto { Id = sprintId, Name = "Updated Sprint" }, "Sprint updated");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSprintCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateSprint(sprintId, command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<SprintDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
        }

        [Fact]
        public async Task UpdateSprint_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var command = new UpdateSprintCommand
            {
                Id = Guid.NewGuid(), // Different ID
                SprintName = "Updated Sprint"
            };

            // Act
            var result = await _controller.UpdateSprint(sprintId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<SprintDto>>(badRequestResult.Value);
            Assert.Contains("mismatch", apiResponse.Message);
        }

        [Fact]
        public async Task UpdateSprint_WhenSprintNotFound_ReturnsNotFound()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var command = new UpdateSprintCommand { Id = sprintId };
            var response = new ApiResponse<SprintDto>(404, null, "Sprint not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSprintCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateSprint(sprintId, command);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region DeleteSprint Tests

        [Fact]
        public async Task DeleteSprint_WithValidId_ReturnsOk()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var response = new ApiResponse<bool>(200, true, "Sprint deleted successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteSprintCommand>(c => c.Id == sprintId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteSprint(sprintId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.True(apiResponse.Data);
        }

        [Fact]
        public async Task DeleteSprint_WhenSprintNotFound_ReturnsNotFound()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var response = new ApiResponse<bool>(404, false, "Sprint not found");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteSprintCommand>(c => c.Id == sprintId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteSprint(sprintId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region CompleteSprint Tests

        [Fact]
        public async Task CompleteSprint_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var response = ApiResponse<bool>.Success(true, "Sprint completed successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<CompleteSprintCommand>(c => c.SprintId == sprintId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CompleteSprint(sprintId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task CompleteSprint_WhenSprintNotFound_ReturnsFail()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var response = ApiResponse<bool>.Fail("Sprint not found");

            _mediatorMock.Setup(m => m.Send(It.Is<CompleteSprintCommand>(c => c.SprintId == sprintId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CompleteSprint(sprintId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.False(result.Data);
        }

        #endregion

        #region PlanSprintWithAI Tests

        [Fact]
        public async Task PlanSprintWithAI_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = 1;
            var request = new PlanSprintRequestDto
            {
                SprintName = "AI Planned Sprint",
                SprintGoal = "Complete features",
                TeamId = 1,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                TargetStoryPoints = 50
            };

            var response = new ApiResponse<GeminiSprintPlanResponseDto>(
                200,
                new GeminiSprintPlanResponseDto { SprintPlan = new SprintPlanDto() },
                "Sprint planned successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<PlanSprintWithAICommand>(), default))
                .ReturnsAsync(response);

            // Setup User Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.PlanSprintWithAI(projectId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<GeminiSprintPlanResponseDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
        }

        [Fact]
        public async Task PlanSprintWithAI_WithoutUserIdInToken_ReturnsUnauthorized()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new PlanSprintRequestDto
            {
                SprintName = "AI Sprint",
                TeamId = 1
            };

            // Setup empty claims
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.PlanSprintWithAI(projectId, request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<GeminiSprintPlanResponseDto>>(unauthorizedResult.Value);
            Assert.Contains("User ID not found", apiResponse.Message);
        }

        [Fact]
        public async Task PlanSprintWithAI_WithInvalidUserId_ReturnsUnauthorized()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new PlanSprintRequestDto { SprintName = "AI Sprint" };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid_id")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.PlanSprintWithAI(projectId, request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult);
        }

        [Fact]
        public async Task PlanSprintWithAI_WhenMediatorReturns400_ReturnsBadRequest()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = 1;
            var request = new PlanSprintRequestDto { SprintName = "AI Sprint" };

            var response = new ApiResponse<GeminiSprintPlanResponseDto>(400, null, "Invalid data");

            _mediatorMock.Setup(m => m.Send(It.IsAny<PlanSprintWithAICommand>(), default))
                .ReturnsAsync(response);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Act
            var result = await _controller.PlanSprintWithAI(projectId, request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        #endregion
    }
}
