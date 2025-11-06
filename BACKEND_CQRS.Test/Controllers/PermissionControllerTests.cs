using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Permissions;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for PermissionController
    /// Tests permission management endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class PermissionControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<PermissionController>> _loggerMock;
        private readonly PermissionController _controller;

        public PermissionControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<PermissionController>>();
            _controller = new PermissionController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region GetUserProjectPermissions Tests

        [Fact]
        public async Task GetUserProjectPermissions_WithValidIds_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var permissions = new UserProjectPermissionsDto
            {
                UserId = userId,
                ProjectId = projectId,
                RoleName = "Developer",
                IsOwner = false,
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Name = "project.read" },
                    new PermissionDto { Name = "project.update" }
                }
            };

            var response = ApiResponse<UserProjectPermissionsDto>.Success(permissions, "Permissions retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetUserProjectPermissionsQuery>(q => q.UserId == userId && q.ProjectId == projectId),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UserProjectPermissionsDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal(2, apiResponse.Data.Permissions.Count);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WithZeroUserId_ReturnsBadRequest()
        {
            // Arrange
            var userId = 0;
            var projectId = Guid.NewGuid();

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.Contains("Invalid user ID", apiResponse.Message);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WithNegativeUserId_ReturnsBadRequest()
        {
            // Arrange
            var userId = -1;
            var projectId = Guid.NewGuid();

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.Empty;

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.Contains("Invalid project ID", apiResponse.Message);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WhenUserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            var projectId = Guid.NewGuid();
            var response = new ApiResponse<UserProjectPermissionsDto>(404, null!, "User not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WhenProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var response = new ApiResponse<UserProjectPermissionsDto>(404, null!, "Project not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WhenUserNotMember_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var response = new ApiResponse<UserProjectPermissionsDto>(404, null!, "User is not a member of this project");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetUserProjectPermissions_LogsInformation()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var response = ApiResponse<UserProjectPermissionsDto>.Success(new UserProjectPermissionsDto(), "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API request received to get permissions")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUserProjectPermissions_WithProjectOwner_ReturnsAllPermissions()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var permissions = new UserProjectPermissionsDto
            {
                UserId = userId,
                ProjectId = projectId,
                RoleName = "Owner",
                IsOwner = true,
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Name = "project.create" },
                    new PermissionDto { Name = "project.read" },
                    new PermissionDto { Name = "project.update" },
                    new PermissionDto { Name = "project.delete" },
                    new PermissionDto { Name = "user.manage" },
                    new PermissionDto { Name = "team.manage" }
                }
            };

            var response = ApiResponse<UserProjectPermissionsDto>.Success(permissions, "Permissions retrieved");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserProjectPermissions(userId, projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UserProjectPermissionsDto>>(okResult.Value);
            Assert.True(apiResponse.Data.IsOwner);
            Assert.Equal(6, apiResponse.Data.Permissions.Count);
        }

        #endregion

        #region GetMyProjectPermissions Tests

        [Fact]
        public async Task GetMyProjectPermissions_WithValidToken_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
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

            var permissions = new UserProjectPermissionsDto
            {
                UserId = userId,
                ProjectId = projectId,
                RoleName = "Developer"
            };

            var response = ApiResponse<UserProjectPermissionsDto>.Success(permissions, "Permissions retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetUserProjectPermissionsQuery>(q => q.UserId == userId && q.ProjectId == projectId),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyProjectPermissions(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UserProjectPermissionsDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
        }

        [Fact]
        public async Task GetMyProjectPermissions_WithoutUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetMyProjectPermissions(projectId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
            Assert.Contains("User ID not found in token", apiResponse.Message);
        }

        [Fact]
        public async Task GetMyProjectPermissions_WithInvalidUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            var projectId = Guid.NewGuid();
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
            var result = await _controller.GetMyProjectPermissions(projectId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.NotNull(unauthorizedResult);
        }

        [Fact]
        public async Task GetMyProjectPermissions_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.Empty;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Act
            var result = await _controller.GetMyProjectPermissions(projectId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.Contains("Invalid project ID", apiResponse.Message);
        }

        [Fact]
        public async Task GetMyProjectPermissions_WhenProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var response = new ApiResponse<UserProjectPermissionsDto>(404, null!, "Project not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyProjectPermissions(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetMyProjectPermissions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetMyProjectPermissions(projectId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMyProjectPermissions_LogsInformation()
        {
            // Arrange
            var userId = 1;
            var projectId = Guid.NewGuid();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var response = ApiResponse<UserProjectPermissionsDto>.Success(new UserProjectPermissionsDto(), "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProjectPermissionsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.GetMyProjectPermissions(projectId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API request received to get permissions for current user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
