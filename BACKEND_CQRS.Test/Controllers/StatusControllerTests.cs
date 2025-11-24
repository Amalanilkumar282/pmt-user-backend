using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Statuses;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for StatusController
    /// Tests all API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class StatusControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<StatusController>> _loggerMock;
        private readonly StatusController _controller;

        public StatusControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<StatusController>>();
            _controller = new StatusController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region GetAllStatuses Tests

        [Fact]
        public async Task GetAllStatuses_WithStatuses_ReturnsOkWithList()
        {
            // Arrange
            var expectedStatuses = new List<StatusDto>
            {
                new StatusDto { Id = 1, StatusName = "To Do" },
                new StatusDto { Id = 2, StatusName = "In Progress" },
                new StatusDto { Id = 3, StatusName = "Done" }
            };

            var response = ApiResponse<List<StatusDto>>.Success(
                expectedStatuses,
                $"Successfully fetched {expectedStatuses.Count} status(es)");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllStatusesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllStatuses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<StatusDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal(3, apiResponse.Data.Count);
            Assert.Contains("Successfully fetched 3 status(es)", apiResponse.Message);
        }

        [Fact]
        public async Task GetAllStatuses_WithNoStatuses_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<StatusDto>();
            var response = ApiResponse<List<StatusDto>>.Success(emptyList, "No statuses found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllStatusesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllStatuses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<StatusDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Empty(apiResponse.Data);
            Assert.Contains("No statuses found", apiResponse.Message);
        }

        [Fact]
        public async Task GetAllStatuses_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllStatusesQuery>(), default))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.GetAllStatuses();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse<List<StatusDto>>>(statusCodeResult.Value);
            Assert.Contains("unexpected error occurred", apiResponse.Message);
        }

        [Fact]
        public async Task GetAllStatuses_WhenDatabaseError_ReturnsInternalServerError()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllStatusesQuery>(), default))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _controller.GetAllStatuses();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region GetStatusById Tests

        [Fact]
        public async Task GetStatusById_WithValidId_ReturnsOk()
        {
            // Arrange
            var statusId = 1;
            var expectedStatus = new StatusDto 
            { 
                Id = statusId, 
                StatusName = "To Do"
            };

            var response = ApiResponse<StatusDto>.Success(
                expectedStatus, 
                $"Successfully fetched status '{expectedStatus.StatusName}'");

            _mediatorMock.Setup(m => m.Send(It.Is<GetStatusByIdQuery>(q => q.StatusId == statusId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StatusDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal("To Do", apiResponse.Data.StatusName);
            Assert.Contains("Successfully fetched status", apiResponse.Message);
        }

        [Fact]
        public async Task GetStatusById_WithZeroId_ReturnsBadRequest()
        {
            // Arrange
            var statusId = 0;

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StatusDto>>(badRequestResult.Value);
            Assert.Equal(400, apiResponse.Status);
            Assert.Contains("Invalid status ID", apiResponse.Message);
        }

        [Fact]
        public async Task GetStatusById_WithNegativeId_ReturnsBadRequest()
        {
            // Arrange
            var statusId = -1;

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StatusDto>>(badRequestResult.Value);
            Assert.Equal(400, apiResponse.Status);
            Assert.Contains("Invalid status ID", apiResponse.Message);
        }

        [Fact]
        public async Task GetStatusById_WhenStatusNotFound_ReturnsNotFound()
        {
            // Arrange
            var statusId = 999;
            var response = ApiResponse<StatusDto>.Fail($"Status with ID {statusId} does not exist");

            _mediatorMock.Setup(m => m.Send(It.Is<GetStatusByIdQuery>(q => q.StatusId == statusId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StatusDto>>(notFoundResult.Value);
            Assert.Null(apiResponse.Data);
            Assert.Contains("does not exist", apiResponse.Message);
        }

        [Fact]
        public async Task GetStatusById_WhenMediatorReturns400_ReturnsBadRequest()
        {
            // Arrange
            var statusId = 1;
            var response = new ApiResponse<StatusDto>(400, null, "Invalid status");

            _mediatorMock.Setup(m => m.Send(It.Is<GetStatusByIdQuery>(q => q.StatusId == statusId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StatusDto>>(badRequestResult.Value);
            Assert.Equal(400, apiResponse.Status);
        }

        [Fact]
        public async Task GetStatusById_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var statusId = 1;
            _mediatorMock.Setup(m => m.Send(It.Is<GetStatusByIdQuery>(q => q.StatusId == statusId), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse<StatusDto>>(statusCodeResult.Value);
            Assert.Contains("unexpected error occurred", apiResponse.Message);
        }

        [Fact]
        public async Task GetStatusById_WhenDatabaseError_ReturnsInternalServerError()
        {
            // Arrange
            var statusId = 1;
            _mediatorMock.Setup(m => m.Send(It.Is<GetStatusByIdQuery>(q => q.StatusId == statusId), default))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetStatusById_WithValidLargeId_ReturnsOk()
        {
            // Arrange
            var statusId = int.MaxValue;
            var expectedStatus = new StatusDto 
            { 
                Id = statusId, 
                StatusName = "Custom Status"
            };

            var response = ApiResponse<StatusDto>.Success(expectedStatus, "Successfully fetched status");

            _mediatorMock.Setup(m => m.Send(It.Is<GetStatusByIdQuery>(q => q.StatusId == statusId), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetStatusById(statusId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StatusDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal(statusId, apiResponse.Data.Id);
        }

        #endregion
    }
}
