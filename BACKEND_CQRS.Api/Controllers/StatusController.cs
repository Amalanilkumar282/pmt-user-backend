using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Statuses;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatusController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IMediator mediator, ILogger<StatusController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all statuses in the system
        /// </summary>
        /// <returns>List of all statuses</returns>
        /// <response code="200">Returns the list of statuses (may be empty if no statuses exist)</response>
        /// <response code="500">If a server error occurs</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<StatusDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<StatusDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<StatusDto>>>> GetAllStatuses()
        {
            try
            {
                _logger.LogInformation("API request received to fetch all statuses");

                var result = await _mediator.Send(new GetAllStatusesQuery());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in StatusController.GetAllStatuses");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<StatusDto>>.Fail(
                        "An unexpected error occurred while fetching statuses. Please contact support if the issue persists."));
            }
        }

        /// <summary>
        /// Get a single status by its ID
        /// </summary>
        /// <param name="statusId">The status ID</param>
        /// <returns>Status details</returns>
        /// <response code="200">Returns the status details</response>
        /// <response code="400">If the status ID is invalid</response>
        /// <response code="404">If the status does not exist</response>
        /// <response code="500">If a server error occurs</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/status/1
        /// 
        /// This endpoint returns complete information about a specific status.
        /// </remarks>
        [HttpGet("{statusId:int}")]
        [ProducesResponseType(typeof(ApiResponse<StatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<StatusDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<StatusDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<StatusDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<StatusDto>>> GetStatusById(int statusId)
        {
            try
            {
                // Validate input
                if (statusId <= 0)
                {
                    _logger.LogWarning("Invalid status ID provided: {StatusId}", statusId);
                    return BadRequest(ApiResponse<StatusDto>.Fail("Invalid status ID. Status ID must be greater than 0."));
                }

                _logger.LogInformation("API request received to fetch status: {StatusId}", statusId);

                var result = await _mediator.Send(new GetStatusByIdQuery(statusId));

                if (result.Status == 400)
                {
                    return BadRequest(result);
                }

                if (result.Data == null)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in StatusController.GetStatusById for status: {StatusId}", statusId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<StatusDto>.Fail(
                        "An unexpected error occurred while fetching the status. Please contact support if the issue persists."));
            }
        }

        /// <summary>
        /// Get all statuses used in a specific project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <returns>List of statuses used in the project's boards</returns>
        /// <response code="200">Returns the list of statuses (may be empty if no statuses are configured)</response>
        /// <response code="400">If the project ID is invalid</response>
        /// <response code="500">If a server error occurs</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/status/by-project/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// 
        /// This endpoint returns all distinct statuses that are configured in the project's board columns.
        /// Statuses are retrieved from the board columns associated with the project's active boards.
        /// </remarks>
        [HttpGet("by-project/{projectId}")]
        [ProducesResponseType(typeof(ApiResponse<List<StatusDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<StatusDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<List<StatusDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<StatusDto>>>> GetStatusesByProjectId(Guid projectId)
        {
            try
            {
                // Validate input
                if (projectId == Guid.Empty)
                {
                    _logger.LogWarning("Empty project ID provided");
                    return BadRequest(ApiResponse<List<StatusDto>>.Fail("Invalid project ID. Project ID cannot be empty."));
                }

                _logger.LogInformation("API request received to fetch statuses for project: {ProjectId}", projectId);

                var result = await _mediator.Send(new GetStatusesByProjectIdQuery(projectId));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in StatusController.GetStatusesByProjectId for project: {ProjectId}", projectId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<StatusDto>>.Fail(
                        "An unexpected error occurred while fetching statuses for the project. Please contact support if the issue persists."));
            }
        }
    }
}
