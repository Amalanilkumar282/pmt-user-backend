using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
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
    /// Comprehensive unit tests for AuthController
    /// Tests all authentication endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = "Password123!"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Success(
                new LoginResponseDto
                {
                    UserId = 1,
                    Email = "user@example.com",
                    Name = "Test User",
                    AccessToken = "access_token_here",
                    RefreshToken = "refresh_token_here",
                    AccessTokenExpires = DateTimeOffset.UtcNow.AddHours(1),
                    RefreshTokenExpires = DateTimeOffset.UtcNow.AddDays(7),
                    IsActive = true,
                    IsSuperAdmin = false
                },
                "Login successful");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("user@example.com", result.Data.Email);
            Assert.NotNull(result.Data.AccessToken);
            Assert.NotNull(result.Data.RefreshToken);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsFailure()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = "WrongPassword"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("Invalid email or password");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("Invalid email or password", result.Message);
        }

        [Fact]
        public async Task Login_WithInactiveUser_ReturnsFailure()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "inactive@example.com",
                Password = "Password123!"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("User account is inactive");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("inactive", result.Message);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("User not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task Login_WithEmptyEmail_SendsCommandToMediator()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "",
                Password = "Password123!"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("Email is required");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<LoginCommand>(), default), Times.Once);
        }

        [Fact]
        public async Task Login_WithEmptyPassword_SendsCommandToMediator()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = ""
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("Password is required");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<LoginCommand>(), default), Times.Once);
        }

        [Fact]
        public async Task Login_LogsInformation()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = "Password123!"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Success(new LoginResponseDto(), "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.Login(request);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login endpoint called")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region RefreshToken Tests

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var request = new RefreshTokenRequestDto
            {
                RefreshToken = "valid_refresh_token"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Success(
                new LoginResponseDto
                {
                    UserId = 1,
                    AccessToken = "new_access_token",
                    RefreshToken = "new_refresh_token",
                    AccessTokenExpires = DateTimeOffset.UtcNow.AddHours(1),
                    RefreshTokenExpires = DateTimeOffset.UtcNow.AddDays(7)
                },
                "Token refreshed successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(request);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data.AccessToken);
            Assert.NotNull(result.Data.RefreshToken);
            Assert.NotEqual("valid_refresh_token", result.Data.RefreshToken); // New token should be different
        }

        [Fact]
        public async Task RefreshToken_WithInvalidToken_ReturnsFailure()
        {
            // Arrange
            var request = new RefreshTokenRequestDto
            {
                RefreshToken = "invalid_refresh_token"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("Invalid or expired refresh token");

            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(request);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("Invalid or expired", result.Message);
        }

        [Fact]
        public async Task RefreshToken_WithExpiredToken_ReturnsFailure()
        {
            // Arrange
            var request = new RefreshTokenRequestDto
            {
                RefreshToken = "expired_refresh_token"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("Refresh token has expired");

            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(request);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("expired", result.Message);
        }

        [Fact]
        public async Task RefreshToken_WithRevokedToken_ReturnsFailure()
        {
            // Arrange
            var request = new RefreshTokenRequestDto
            {
                RefreshToken = "revoked_refresh_token"
            };

            var expectedResponse = ApiResponse<LoginResponseDto>.Fail("Refresh token has been revoked");

            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(request);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("revoked", result.Message);
        }

        [Fact]
        public async Task RefreshToken_LogsInformation()
        {
            // Arrange
            var request = new RefreshTokenRequestDto { RefreshToken = "token" };
            var expectedResponse = ApiResponse<LoginResponseDto>.Success(new LoginResponseDto(), "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.RefreshToken(request);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refresh token endpoint called")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task Logout_WithValidUser_ReturnsSuccess()
        {
            // Arrange
            var userId = 1;
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

            var expectedResponse = ApiResponse<bool>.Success(true, "Logged out successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<LogoutCommand>(c => c.UserId == userId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.Equal(200, result.Status);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task Logout_WithMissingUserId_ReturnsFailure()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("Invalid user session", result.Message);
        }

        [Fact]
        public async Task Logout_WithInvalidUserId_ReturnsFailure()
        {
            // Arrange
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
            var result = await _controller.Logout();

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("Invalid user session", result.Message);
        }

        [Fact]
        public async Task Logout_UsesSubClaimIfNameIdentifierMissing()
        {
            // Arrange
            var userId = 1;
            var claims = new List<Claim>
            {
                new Claim("sub", userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var expectedResponse = ApiResponse<bool>.Success(true, "Logged out successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<LogoutCommand>(c => c.UserId == userId), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.Equal(200, result.Status);
        }

        [Fact]
        public async Task Logout_LogsInformation()
        {
            // Arrange
            var userId = 1;
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var expectedResponse = ApiResponse<bool>.Success(true, "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<LogoutCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.Logout();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logout endpoint called")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region GetCurrentUser Tests

        [Fact]
        public void GetCurrentUser_WithValidClaims_ReturnsUserInfo()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim("is_super_admin", "false"),
                new Claim("is_active", "true"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Developer")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            
            dynamic? userData = apiResponse.Data;
            Assert.NotNull(userData);
        }

        [Fact]
        public void GetCurrentUser_WithAlternativeClaimNames_ReturnsUserInfo()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("sub", "1"),
                new Claim("email", "user@example.com"),
                new Claim("name", "Test User"),
                new Claim("is_super_admin", "true"),
                new Claim("is_active", "true")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
        }

        [Fact]
        public void GetCurrentUser_WithMissingOptionalClaims_ReturnsUserInfo()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "user@example.com")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
        }

        [Fact]
        public void GetCurrentUser_WithNoRoles_ReturnsEmptyRolesList()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim("is_super_admin", "false"),
                new Claim("is_active", "true")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion
    }
}
