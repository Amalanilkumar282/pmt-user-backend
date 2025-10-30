using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Project;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IMediator mediator, ILogger<ProjectController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Add a member to a project (requires project owner authorization)
        /// </summary>
        /// <param name="command">The member addition details</param>
        /// <returns>Details of the added member</returns>
        /// <response code="201">Member added successfully</response>
        /// <response code="400">If validation fails, member already exists, or unauthorized</response>
        /// <response code="500">If a server error occurs</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/project/member
        ///     {
        ///        "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///        "userId": 5,
        ///        "roleId": 2,
        ///        "addedBy": 1
        ///     }
        /// 
        /// **Authorization:**
        /// - The `addedBy` user MUST be a project owner (is_owner = true)
        /// - Only project owners can add members to the project
        /// - The `addedBy` user must be an existing member of the project
        /// 
        /// **Auto-detection:**
        /// - The system automatically determines if the new user is the project owner
        /// - If userId matches the project's projectManagerId, isOwner is set to true
        /// 
        /// **Validation:**
        /// - Project must exist
        /// - User to add must exist and be active
        /// - Role must exist
        /// - User must not already be a member
        /// - AddedBy user must be a project owner
        /// </remarks>
        [HttpPost("member")]
        [ProducesResponseType(typeof(ApiResponse<AddProjectMemberResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AddProjectMemberResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AddProjectMemberResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddProjectMemberResponseDto>>> AddProjectMember(
            [FromBody] AddProjectMemberCommand command)
        {
            try
            {
                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    _logger.LogWarning("Invalid model state for AddProjectMember: {Errors}", errors);
                    return BadRequest(ApiResponse<AddProjectMemberResponseDto>.Fail($"Validation failed: {errors}"));
                }

                // Additional validation: ProjectId cannot be empty
                if (command.ProjectId == Guid.Empty)
                {
                    _logger.LogWarning("Empty project ID provided in AddProjectMember request");
                    return BadRequest(ApiResponse<AddProjectMemberResponseDto>.Fail(
                        "Invalid project ID. Project ID cannot be empty."));
                }

                _logger.LogInformation(
                    "API request received to add user {UserId} to project {ProjectId} with role {RoleId} by user {AddedBy}",
                    command.UserId, command.ProjectId, command.RoleId, command.AddedBy);

                var result = await _mediator.Send(command);

                if (result.Status == 201)
                {
                    return CreatedAtAction(
                        nameof(GetUsersByProject),
                        new { projectId = result.Data?.ProjectId },
                        result);
                }

                // If status is 400, it's a business logic failure (including authorization)
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in ProjectController.AddProjectMember for project: {ProjectId}",
                    command?.ProjectId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<AddProjectMemberResponseDto>.Fail(
                        "An unexpected error occurred while adding the member. Please contact support if the issue persists."));
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ApiResponse<List<ProjectDto>>> GetProjectsByUser(int userId)
        {
            var query = new GetUserProjectsQuery(userId);
            var result = await _mediator.Send(query);
            return result;
        }



        [HttpGet("{projectId}/users")]
        public async Task<ApiResponse<List<ProjectUserDto>>> GetUsersByProject(Guid projectId)
        {
            var query = new GetUsersByProjectIdQuery(projectId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpGet("recent")]
        public async Task<ApiResponse<List<ProjectDto>>> GetRecentProjects([FromQuery] int take = 10)
        {
            var query = new GetRecentProjectsQuery(take);
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
