using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Permissions;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    /// <summary>
    /// Controller for managing user permissions in projects
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(IMediator mediator, ILogger<PermissionController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get user's permissions for a specific project
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="projectId">The project ID (GUID)</param>
        /// <returns>User's role and permissions for the specified project</returns>
        /// <response code="200">Returns the user's permissions and role for the project</response>
        /// <response code="400">If the user ID or project ID is invalid</response>
        /// <response code="404">If the user, project, or project membership is not found</response>
        /// <response code="500">If a server error occurs</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/permission/user/5/project/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// 
        /// This endpoint returns:
        /// - User's basic information (ID, name, email)
        /// - Project information (ID, name)
        /// - User's role in the project (role ID and name)
        /// - Whether the user is the project owner
        /// - Complete list of permissions granted to the user for this project
        /// - Quick permission flags for common operations (for frontend convenience)
        /// 
        /// **Use Cases:**
        /// - When a user clicks on a project, call this endpoint to determine their permissions
        /// - Use the returned permissions to show/hide UI elements based on user's access rights
        /// - Use permission flags for quick checks (e.g., CanCreateProject, CanManageUsers)
        /// 
        /// **Permission Names:**
        /// - project.create - Can create projects
        /// - project.read - Can read/view projects
        /// - project.update - Can update projects
        /// - project.delete - Can delete projects
        /// - team.manage - Can manage teams
        /// - user.manage - Can manage users
        /// 
        /// **Response includes:**
        /// - Full permission list with IDs, names, and descriptions
        /// - Permission flags (boolean) for quick checks
        /// - Role information
        /// - Project ownership status
        /// </remarks>
        [HttpGet("user/{userId}/project/{projectId}")]
        [ProducesResponseType(typeof(ApiResponse<UserProjectPermissionsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserProjectPermissionsDto>>> GetUserProjectPermissions(
            [FromRoute] int userId,
            [FromRoute] Guid projectId)
        {
            try
            {
                // Validate inputs
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID provided: {UserId}", userId);
                    return BadRequest(ApiResponse<object>.Fail(
                        "Invalid user ID. User ID must be greater than 0."));
                }

                if (projectId == Guid.Empty)
                {
                    _logger.LogWarning("Empty project ID provided");
                    return BadRequest(ApiResponse<object>.Fail(
                        "Invalid project ID. Project ID cannot be empty."));
                }

                _logger.LogInformation(
                    "API request received to get permissions for UserId: {UserId} in ProjectId: {ProjectId}",
                    userId, projectId);

                var query = new GetUserProjectPermissionsQuery(userId, projectId);
                var result = await _mediator.Send(query);

                if (result.Status == 200)
                {
                    return Ok(result);
                }

                // If status is 400 or other error status, return appropriate response
                if (result.Status == 404 || result.Message.Contains("not found") || result.Message.Contains("not a member"))
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred in PermissionController.GetUserProjectPermissions for UserId: {UserId}, ProjectId: {ProjectId}",
                    userId, projectId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        "An unexpected error occurred while retrieving user permissions. Please contact support if the issue persists."));
            }
        }

        /// <summary>
        /// Get current logged-in user's permissions for a specific project
        /// </summary>
        /// <param name="projectId">The project ID (GUID)</param>
        /// <returns>Current user's role and permissions for the specified project</returns>
        /// <response code="200">Returns the current user's permissions and role for the project</response>
        /// <response code="400">If the project ID is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the project or project membership is not found</response>
        /// <response code="500">If a server error occurs</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/permission/me/project/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// 
        /// This endpoint uses the JWT token to identify the current user and returns their permissions.
        /// More convenient than the user/{userId}/project/{projectId} endpoint when you don't need to specify userId.
        /// </remarks>
        [HttpGet("me/project/{projectId}")]
        [ProducesResponseType(typeof(ApiResponse<UserProjectPermissionsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserProjectPermissionsDto>>> GetMyProjectPermissions(
            [FromRoute] Guid projectId)
        {
            try
            {
                // Get user ID from JWT claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("User ID not found in JWT token or invalid");
                    return Unauthorized(ApiResponse<object>.Fail(
                        "User ID not found in token. Please log in again."));
                }

                if (projectId == Guid.Empty)
                {
                    _logger.LogWarning("Empty project ID provided");
                    return BadRequest(ApiResponse<object>.Fail(
                        "Invalid project ID. Project ID cannot be empty."));
                }

                _logger.LogInformation(
                    "API request received to get permissions for current user (UserId: {UserId}) in ProjectId: {ProjectId}",
                    userId, projectId);

                var query = new GetUserProjectPermissionsQuery(userId, projectId);
                var result = await _mediator.Send(query);

                if (result.Status == 200)
                {
                    return Ok(result);
                }

                if (result.Status == 404 || result.Message.Contains("not found") || result.Message.Contains("not a member"))
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred in PermissionController.GetMyProjectPermissions for ProjectId: {ProjectId}",
                    projectId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        "An unexpected error occurred while retrieving your permissions. Please contact support if the issue persists."));
            }
        }
    }
}
