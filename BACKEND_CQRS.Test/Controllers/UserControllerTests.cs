using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.User;
using BACKEND_CQRS.Application.Query.Users;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for UserController
    /// Tests all API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class UserControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new UserController(_mediatorMock.Object);
        }

        #region GetAllUsers Tests

        [Fact]
        public async Task GetAllUsers_WithUsers_ReturnsOk()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Name = "John Doe", Email = "john@test.com", IsActive = true },
                new UserDto { Id = 2, Name = "Jane Smith", Email = "jane@test.com", IsActive = true },
                new UserDto { Id = 3, Name = "Bob Johnson", Email = "bob@test.com", IsActive = true }
            };

            var response = ApiResponse<List<UserDto>>.Success(users, "Users fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<UserDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal(3, apiResponse.Data.Count);
        }

        [Fact]
        public async Task GetAllUsers_WithNoUsers_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<UserDto>();
            var response = ApiResponse<List<UserDto>>.Success(emptyList, "No users found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<UserDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Empty(apiResponse.Data);
        }

        [Fact]
        public async Task GetAllUsers_IncludesInactiveUsers_ReturnsAll()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Name = "Active User", IsActive = true },
                new UserDto { Id = 2, Name = "Inactive User", IsActive = false }
            };

            var response = ApiResponse<List<UserDto>>.Success(users, "Users fetched");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<UserDto>>>(okResult.Value);
            Assert.Equal(2, apiResponse.Data.Count);
            Assert.Contains(apiResponse.Data, u => u.IsActive == false);
        }

        [Fact]
        public async Task GetAllUsers_WhenExceptionInMediator_ReturnsFailure()
        {
            // Arrange
            var response = ApiResponse<List<UserDto>>.Fail("Database error occurred");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<UserDto>>>(okResult.Value);
            Assert.Equal(400, apiResponse.Status);
        }

        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_WithValidId_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var user = new UserDto
            {
                Id = userId,
                Name = "John Doe",
                Email = "john@test.com",
                IsActive = true
            };

            var response = ApiResponse<UserDto>.Success(user, "User fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal("John Doe", apiResponse.Data.Name);
        }

        [Fact]
        public async Task GetUserById_WhenUserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            var response = new ApiResponse<UserDto>(404, null, "User not found");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UserDto>>(notFoundResult.Value);
            Assert.Equal(404, apiResponse.Status);
        }

        [Fact]
        public async Task GetUserById_WithZeroId_CallsMediator()
        {
            // Arrange
            var userId = 0;
            var response = new ApiResponse<UserDto>(400, null, "Invalid user ID");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetUserById_WithNegativeId_CallsMediator()
        {
            // Arrange
            var userId = -1;
            var response = new ApiResponse<UserDto>(400, null, "Invalid user ID");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetUserById_WithInactiveUser_ReturnsUser()
        {
            // Arrange
            var userId = 1;
            var user = new UserDto
            {
                Id = userId,
                Name = "Inactive User",
                Email = "inactive@test.com",
                IsActive = false
            };

            var response = ApiResponse<UserDto>.Success(user, "User fetched");

            _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
            Assert.False(apiResponse.Data.IsActive);
        }

        #endregion

        #region GetUserActivities Tests

        [Fact]
        public async Task GetUserActivities_WithValidUserId_ReturnsActivities()
        {
            // Arrange
            var userId = 1;
            var take = 50;
            var activities = new List<ActivityLogDto>
            {
                new ActivityLogDto { Id = Guid.NewGuid(), UserId = userId, Action = "Created Issue", CreatedAt = DateTimeOffset.UtcNow },
                new ActivityLogDto { Id = Guid.NewGuid(), UserId = userId, Action = "Updated Sprint", CreatedAt = DateTimeOffset.UtcNow }
            };

            var response = ApiResponse<List<ActivityLogDto>>.Success(activities, "Activities fetched");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetUserActivitiesQuery>(q => q.UserId == userId && q.Take == take), 
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserActivities(userId, take);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetUserActivities_WithDefaultTake_Uses50()
        {
            // Arrange
            var userId = 1;
            var activities = new List<ActivityLogDto>();
            var response = ApiResponse<List<ActivityLogDto>>.Success(activities, "Fetched");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetUserActivitiesQuery>(q => q.UserId == userId && q.Take == 50), 
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserActivities(userId);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetUserActivitiesQuery>(q => q.Take == 50), 
                default), 
                Times.Once);
        }

        [Fact]
        public async Task GetUserActivities_WithCustomTake_UsesProvidedValue()
        {
            // Arrange
            var userId = 1;
            var take = 10;
            var activities = new List<ActivityLogDto>();
            var response = ApiResponse<List<ActivityLogDto>>.Success(activities, "Fetched");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetUserActivitiesQuery>(q => q.UserId == userId && q.Take == take), 
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserActivities(userId, take);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetUserActivitiesQuery>(q => q.Take == 10), 
                default), 
                Times.Once);
        }

        [Fact]
        public async Task GetUserActivities_WithNoActivities_ReturnsEmptyList()
        {
            // Arrange
            var userId = 1;
            var emptyList = new List<ActivityLogDto>();
            var response = ApiResponse<List<ActivityLogDto>>.Success(emptyList, "No activities found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserActivitiesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserActivities(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetUserActivities_WhenUserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = 999;
            var response = ApiResponse<List<ActivityLogDto>>.Fail("User not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserActivitiesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserActivities(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region GetUsersByProjectId Tests

        // Note: This endpoint is tested in ProjectControllerTests.GetUsersByProject
        // as it's part of the ProjectController, not UserController

        #endregion
    }
}
