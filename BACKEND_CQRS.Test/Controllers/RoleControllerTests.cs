using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for RoleController
    /// Tests role retrieval endpoints with success, empty list, and error scenarios
    /// </summary>
    public class RoleControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly RoleController _controller;

        public RoleControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new RoleController(_mediatorMock.Object);
        }

        #region GetAllRoles Tests

        [Fact]
        public async Task GetAllRoles_WithRoles_ReturnsOkWithList()
        {
            // Arrange
            var expectedRoles = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin" },
                new RoleDto { Id = 2, Name = "Developer" },
                new RoleDto { Id = 3, Name = "Tester" },
                new RoleDto { Id = 4, Name = "Project Manager" }
            };

            var response = ApiResponse<List<RoleDto>>.Success(
                expectedRoles,
                $"Successfully fetched {expectedRoles.Count} role(s)");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<RoleDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal(4, apiResponse.Data.Count);
            Assert.Contains("Successfully fetched 4 role(s)", apiResponse.Message);
        }

        [Fact]
        public async Task GetAllRoles_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var expectedRoles = new List<RoleDto>();

            var response = ApiResponse<List<RoleDto>>.Success(
                expectedRoles,
                "No roles found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<RoleDto>>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Empty(apiResponse.Data);
        }

        [Fact]
        public async Task GetAllRoles_CallsMediatorOnce()
        {
            // Arrange
            var response = ApiResponse<List<RoleDto>>.Success(new List<RoleDto>(), "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.GetAllRoles();

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllRolesQuery>(), default), Times.Once);
        }

        [Fact]
        public async Task GetAllRoles_WithSingleRole_ReturnsOkWithSingleItem()
        {
            // Arrange
            var expectedRoles = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin" }
            };

            var response = ApiResponse<List<RoleDto>>.Success(expectedRoles, "Role fetched");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<RoleDto>>>(okResult.Value);
            Assert.Single(apiResponse.Data);
            Assert.Equal("Admin", apiResponse.Data[0].Name);
        }

        [Fact]
        public async Task GetAllRoles_VerifiesRoleProperties()
        {
            // Arrange
            var expectedRoles = new List<RoleDto>
            {
                new RoleDto 
                { 
                    Id = 1, 
                    Name = "Admin"
                }
            };

            var response = ApiResponse<List<RoleDto>>.Success(expectedRoles, "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<RoleDto>>>(okResult.Value);
            var role = apiResponse.Data[0];
            Assert.Equal(1, role.Id);
            Assert.Equal("Admin", role.Name);
        }

        [Fact]
        public async Task GetAllRoles_WithMultipleRoles_VerifiesOrder()
        {
            // Arrange
            var expectedRoles = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin" },
                new RoleDto { Id = 2, Name = "Developer" },
                new RoleDto { Id = 3, Name = "Tester" }
            };

            var response = ApiResponse<List<RoleDto>>.Success(expectedRoles, "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<RoleDto>>>(okResult.Value);
            Assert.Equal("Admin", apiResponse.Data[0].Name);
            Assert.Equal("Developer", apiResponse.Data[1].Name);
            Assert.Equal("Tester", apiResponse.Data[2].Name);
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOkActionResult()
        {
            // Arrange
            var response = ApiResponse<List<RoleDto>>.Success(new List<RoleDto>(), "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllRoles_WithFailureResponse_ReturnsResponse()
        {
            // Arrange
            var response = ApiResponse<List<RoleDto>>.Fail("Database error");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<List<RoleDto>>>(okResult.Value);
            Assert.Equal(400, apiResponse.Status);
            Assert.Contains("Database error", apiResponse.Message);
        }

        #endregion
    }
}
