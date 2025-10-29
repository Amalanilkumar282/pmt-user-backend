using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BoardController> _logger;

        public BoardController(IMediator mediator, ILogger<BoardController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all active boards by project ID with their columns
        /// </summary>
        /// <param name="projectId">The project ID (GUID)</param>
        /// <returns>List of boards with columns</returns>
        /// <response code="200">Returns the list of boards (may be empty if no boards exist)</response>
        /// <response code="400">If the project ID is invalid</response>
        /// <response code="404">If the project does not exist</response>
        /// <response code="500">If a server error occurs</response>
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<BoardWithColumnsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<BoardWithColumnsDto>>>> GetBoardsByProjectId(Guid projectId)
        {
            try
            {
                // Validate input
                if (projectId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid project ID provided: {ProjectId}", projectId);
                    return BadRequest(ApiResponse<object>.Fail("Invalid project ID. Project ID cannot be empty."));
                }

                _logger.LogInformation("API request received to fetch boards for project: {ProjectId}", projectId);

                var result = await _mediator.Send(new GetBoardsByProjectIdQuery(projectId));

                var message = result.Count > 0
                    ? $"Successfully fetched {result.Count} board(s) for project {projectId}"
                    : $"No active boards found for project {projectId}";

                return Ok(ApiResponse<List<BoardWithColumnsDto>>.Success(result, message));
            }
            catch (KeyNotFoundException ex)
            {
                // Project doesn't exist
                _logger.LogWarning(ex, "Project not found: {ProjectId}", projectId);
                return NotFound(ApiResponse<object>.Fail($"Project with ID {projectId} does not exist"));
            }
            catch (InvalidOperationException ex)
            {
                // Database operation failed
                _logger.LogError(ex, "Database operation failed for project: {ProjectId}", projectId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("A database error occurred while fetching boards. Please try again later."));
            }
            catch (Exception ex)
            {
                // Unexpected error
                _logger.LogError(ex, "Unexpected error occurred in BoardController.GetBoardsByProjectId for project: {ProjectId}", projectId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An unexpected error occurred while fetching boards. Please contact support if the issue persists."));
            }
        }

        /// <summary>
        /// Create a new board column
        /// </summary>
        /// <param name="command">The board column creation details</param>
        /// <returns>Created board column details</returns>
        /// <response code="201">Board column created successfully</response>
        /// <response code="400">If the input is invalid or board doesn't exist</response>
        /// <response code="500">If a server error occurs</response>
        [HttpPost("column")]
        [ProducesResponseType(typeof(ApiResponse<CreateBoardColumnResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CreateBoardColumnResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CreateBoardColumnResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CreateBoardColumnResponseDto>>> CreateBoardColumn(
            [FromBody] CreateBoardColumnCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    
                    _logger.LogWarning("Invalid model state for CreateBoardColumn: {Errors}", errors);
                    return BadRequest(ApiResponse<CreateBoardColumnResponseDto>.Fail($"Validation failed: {errors}"));
                }

                _logger.LogInformation("API request received to create board column for board: {BoardId}", command.BoardId);

                var result = await _mediator.Send(command);

                if (result.Status == 201)
                {
                    return CreatedAtAction(
                        nameof(CreateBoardColumn),
                        new { id = result.Data?.ColumnId },
                        result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in BoardController.CreateBoardColumn for board: {BoardId}", 
                    command?.BoardId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<CreateBoardColumnResponseDto>.Fail(
                        "An unexpected error occurred while creating board column. Please contact support if the issue persists."));
            }
        }
    }
}
