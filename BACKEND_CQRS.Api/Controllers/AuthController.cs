using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Access token and refresh token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ApiResponse<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login endpoint called for email: {Email}", request.Email);

            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);
            return result;
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ApiResponse<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            _logger.LogInformation("Refresh token endpoint called");

            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            };

            var result = await _mediator.Send(command);
            return result;
        }

        /// <summary>
        /// Logout and revoke all refresh tokens
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ApiResponse<bool>> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("Logout failed: Invalid user ID in token");
                return ApiResponse<bool>.Fail("Invalid user session");
            }

            _logger.LogInformation("Logout endpoint called for user: {UserId}", userId);

            var command = new LogoutCommand
            {
                UserId = userId
            };

            var result = await _mediator.Send(command);
            return result;
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user details</returns>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("name")?.Value;
            var isSuperAdmin = User.FindFirst("is_super_admin")?.Value;
            var isActive = User.FindFirst("is_active")?.Value;

            var userInfo = new
            {
                userId = userId,
                email = email,
                name = name,
                isSuperAdmin = bool.Parse(isSuperAdmin ?? "false"),
                isActive = bool.Parse(isActive ?? "false"),
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };

            return Ok(ApiResponse<object>.Success(userInfo, "User information retrieved successfully"));
        }
    }
}
