using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Project;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for ProjectController
    /// Tests all API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class ProjectControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<ProjectController>> _loggerMock;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<ProjectController>>();
            _controller = new ProjectController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region AddProjectMember Tests

        [Fact]
        public async Task AddProjectMember_WithValidCommand_ReturnsCreated()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new AddProjectMemberCommand
            {
                ProjectId = projectId,
                UserId = 5,
                RoleId = 2,
                AddedBy = 1
            };

            var expectedResponse = ApiResponse<AddProjectMemberResponseDto>.Created(
                new AddProjectMemberResponseDto
                {
                    MemberId = 10,
                    ProjectId = projectId,
                    ProjectName = "Test Project",
                    UserId = 5,
                    UserName = "John Doe",
                    UserEmail = "john@test.com",
                    RoleId = 2,
                    RoleName = "Developer",
                    IsOwner = false,
                    AddedAt = DateTimeOffset.UtcNow,
                    AddedBy = 1,
                    AddedByName = "Admin User"
                },
                "John Doe successfully added to project Test Project as Member with role Developer");

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(createdResult.Value);
            Assert.Equal(201, response.Status);
            Assert.Equal(5, response.Data.UserId);
            Assert.Equal("John Doe", response.Data.UserName);
        }

        [Fact]
        public async Task AddProjectMember_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.Empty,
                UserId = 5,
                RoleId = 2,
                AddedBy = 1
            };

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid project ID", response.Message);
        }

        [Fact]
        public async Task AddProjectMember_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 0, // Invalid
                RoleId = 2,
                AddedBy = 1
            };

            _controller.ModelState.AddModelError("UserId", "User ID must be greater than 0");

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Validation failed", response.Message);
        }

        [Fact]
        public async Task AddProjectMember_WhenMediatorReturns400_ReturnsBadRequest()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 5,
                RoleId = 2,
                AddedBy = 1
            };

            var failureResponse = ApiResponse<AddProjectMemberResponseDto>.Fail("User is already a member");

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task AddProjectMember_WhenProjectNotFound_ReturnsBadRequest()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 5,
                RoleId = 2,
                AddedBy = 1
            };

            var failureResponse = ApiResponse<AddProjectMemberResponseDto>.Fail("Project does not exist");

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(badRequestResult.Value);
            Assert.Contains("does not exist", response.Message);
        }

        [Fact]
        public async Task AddProjectMember_WhenUserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 999,
                RoleId = 2,
                AddedBy = 1
            };

            var failureResponse = ApiResponse<AddProjectMemberResponseDto>.Fail("User with ID 999 does not exist");

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(badRequestResult.Value);
            Assert.Contains("does not exist", response.Message);
        }

        [Fact]
        public async Task AddProjectMember_WhenUserNotActive_ReturnsBadRequest()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 5,
                RoleId = 2,
                AddedBy = 1
            };

            var failureResponse = ApiResponse<AddProjectMemberResponseDto>.Fail("User is not active");

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(badRequestResult.Value);
            Assert.Contains("not active", response.Message);
        }

        [Fact]
        public async Task AddProjectMember_WhenRoleNotFound_ReturnsBadRequest()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 5,
                RoleId = 999,
                AddedBy = 1
            };

            var failureResponse = ApiResponse<AddProjectMemberResponseDto>.Fail("Role with ID 999 does not exist");

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(badRequestResult.Value);
            Assert.Contains("does not exist", response.Message);
        }

        [Fact]
        public async Task AddProjectMember_AsProjectOwner_SetsIsOwnerTrue()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new AddProjectMemberCommand
            {
                ProjectId = projectId,
                UserId = 1, // Project manager
                RoleId = 1,
                AddedBy = 1
            };

            var expectedResponse = ApiResponse<AddProjectMemberResponseDto>.Created(
                new AddProjectMemberResponseDto
                {
                    UserId = 1,
                    IsOwner = true,
                    RoleName = "Project Manager"
                },
                "Successfully added as Project Owner");

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(createdResult.Value);
            Assert.True(response.Data.IsOwner);
        }

        [Fact]
        public async Task AddProjectMember_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var command = new AddProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 5,
                RoleId = 2,
                AddedBy = 1
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProjectMemberCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.AddProjectMember(command);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<ApiResponse<AddProjectMemberResponseDto>>(statusCodeResult.Value);
            Assert.Contains("unexpected error occurred", response.Message);
        }

        #endregion

        #region GetProjectsByUser Tests

        [Fact]
        public async Task GetProjectsByUser_WithValidUserId_ReturnsProjects()
        {
            // Arrange
            var userId = 1;
            var projects = new List<ProjectDto>
            {
                new ProjectDto { Id = Guid.NewGuid(), Name = "Project 1" },
                new ProjectDto { Id = Guid.NewGuid(), Name = "Project 2" }
            };

            var response = ApiResponse<List<ProjectDto>>.Success(projects, "Projects fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserProjectsQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetProjectsByUser(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetProjectsByUser_WithNoProjects_ReturnsEmptyList()
        {
            // Arrange
            var userId = 1;
            var response = ApiResponse<List<ProjectDto>>.Success(new List<ProjectDto>(), "No projects found");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserProjectsQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetProjectsByUser(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProjectsByUser_WithInvalidUserId_ReturnsFailure()
        {
            // Arrange
            var userId = 0;
            var response = ApiResponse<List<ProjectDto>>.Fail("Invalid user ID");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserProjectsQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetProjectsByUser(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region GetUsersByProject Tests

        [Fact]
        public async Task GetUsersByProject_WithValidProjectId_ReturnsUsers()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var users = new List<ProjectUserDto>
            {
                new ProjectUserDto { Id = 1, Name = "User 1", RoleName = "Developer" },
                new ProjectUserDto { Id = 2, Name = "User 2", RoleName = "Tester" }
            };

            var response = ApiResponse<List<ProjectUserDto>>.Success(users, "Users fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUsersByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUsersByProject(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetUsersByProject_WithNoUsers_ReturnsEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var response = ApiResponse<List<ProjectUserDto>>.Success(new List<ProjectUserDto>(), "No users found");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUsersByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUsersByProject(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetUsersByProject_WithEmptyGuid_ReturnsFailure()
        {
            // Arrange
            var projectId = Guid.Empty;
            var response = ApiResponse<List<ProjectUserDto>>.Fail("Invalid project ID");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUsersByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUsersByProject(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region GetRecentProjects Tests

        [Fact]
        public async Task GetRecentProjects_WithValidUserId_ReturnsProjects()
        {
            // Arrange
            var userId = 1;
            var take = 10;
            var projects = new List<ProjectDto>
            {
                new ProjectDto { Id = Guid.NewGuid(), Name = "Recent Project 1" },
                new ProjectDto { Id = Guid.NewGuid(), Name = "Recent Project 2" }
            };

            var response = ApiResponse<List<ProjectDto>>.Success(projects, "Recent projects fetched");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetRecentProjectsQuery>(q => q.UserId == userId && q.Take == take), 
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetRecentProjects(userId, take);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetRecentProjects_WithDefaultTake_Uses10()
        {
            // Arrange
            var userId = 1;
            var projects = new List<ProjectDto>();
            var response = ApiResponse<List<ProjectDto>>.Success(projects, "Fetched");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetRecentProjectsQuery>(q => q.UserId == userId && q.Take == 10), 
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetRecentProjects(userId);

            // Assert
            Assert.NotNull(result);
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetRecentProjectsQuery>(q => q.Take == 10), 
                default), 
                Times.Once);
        }

        [Fact]
        public async Task GetRecentProjects_WithCustomTake_UsesProvidedValue()
        {
            // Arrange
            var userId = 1;
            var take = 5;
            var projects = new List<ProjectDto>();
            var response = ApiResponse<List<ProjectDto>>.Success(projects, "Fetched");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetRecentProjectsQuery>(q => q.UserId == userId && q.Take == take), 
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetRecentProjects(userId, take);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetRecentProjectsQuery>(q => q.Take == 5), 
                default), 
                Times.Once);
        }

        #endregion

        #region DeleteMember Tests

        [Fact]
        public async Task DeleteMember_WithValidCommand_ReturnsOk()
        {
            // Arrange
            var command = new DeleteProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 5
            };

            var response = ApiResponse<string>.Success("Member deleted successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProjectMemberCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteMember(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Contains("deleted successfully", apiResponse.Message);
        }

        [Fact]
        public async Task DeleteMember_WhenMemberNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                UserId = 999
            };

            var response = ApiResponse<string>.Fail("Member not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProjectMemberCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteMember(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.Equal(400, apiResponse.Status);
        }

        #endregion

        #region UpdateProjectMember Tests

        [Fact]
        public async Task UpdateProjectMember_WithValidCommand_ReturnsOk()
        {
            // Arrange
            var command = new UpdateProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                Id = 10,
                UserId = 5,
                RoleId = 3
            };

            var response = ApiResponse<UpdateProjectMemberResponseDto>.Success(
                new UpdateProjectMemberResponseDto
                {
                    Id = 10,
                    RoleId = 3,
                    Role = "Senior Developer"
                },
                "Member updated successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProjectMemberCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateProjectMember(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UpdateProjectMemberResponseDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal(3, apiResponse.Data.RoleId);
        }

        [Fact]
        public async Task UpdateProjectMember_WhenMemberNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateProjectMemberCommand
            {
                ProjectId = Guid.NewGuid(),
                Id = 999,
                UserId = 999,
                RoleId = 2
            };

            var response = ApiResponse<UpdateProjectMemberResponseDto>.Fail("Member not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProjectMemberCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateProjectMember(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UpdateProjectMemberResponseDto>>(okResult.Value);
            Assert.Equal(400, apiResponse.Status);
        }

        #endregion
    }
}
