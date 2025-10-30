using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        /// Create a new board in a project
        /// </summary>
        /// <param name="command">The board creation details</param>
        /// <returns>Created board details</returns>
        /// <response code="201">Board created successfully</response>
        /// <response code="400">If the input is invalid, project doesn't exist, team doesn't exist, or validation fails</response>
        /// <response code="500">If a server error occurs</response>
        /// <remarks>
        /// Sample request for team-based board:
        /// 
        ///     POST /api/board
        ///     {
        ///        "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///        "name": "Development Team Board",
        ///        "description": "Board for development team sprint planning",
        ///        "type": "team",
        ///        "teamId": 1,
        ///        "createdBy": 5
        ///     }
        ///     
        /// Sample request for custom board:
        /// 
        ///     POST /api/board
        ///     {
        ///        "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///        "name": "Custom Workflow Board",
        ///        "description": "Custom board for specific workflow",
        ///        "type": "custom",
        ///        "createdBy": 5
        ///     }
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateBoardResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CreateBoardResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CreateBoardResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CreateBoardResponseDto>>> CreateBoard(
            [FromBody] CreateBoardCommand command)
        {
            try
            {
                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    
                    _logger.LogWarning("Invalid model state for CreateBoard: {Errors}", errors);
                    return BadRequest(ApiResponse<CreateBoardResponseDto>.Fail($"Validation failed: {errors}"));
                }

                // Additional validation: ProjectId cannot be empty
                if (command.ProjectId == Guid.Empty)
                {
                    _logger.LogWarning("Empty project ID provided in CreateBoard request");
                    return BadRequest(ApiResponse<CreateBoardResponseDto>.Fail(
                        "Invalid project ID. Project ID cannot be empty."));
                }

                _logger.LogInformation(
                    "API request received to create board '{Name}' in project {ProjectId}, Type: {Type}, TeamId: {TeamId}",
                    command.Name, command.ProjectId, command.Type, command.TeamId);

                var result = await _mediator.Send(command);

                if (result.Status == 201)
                {
                    return CreatedAtAction(
                        nameof(GetBoardsByProjectId),
                        new { projectId = result.Data?.ProjectId },
                        result);
                }

                // If status is 400, it's a business logic failure
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in BoardController.CreateBoard for project: {ProjectId}", 
                    command?.ProjectId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<CreateBoardResponseDto>.Fail(
                        "An unexpected error occurred while creating the board. Please contact support if the issue persists."));
            }
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

        /// <summary>
        /// Delete a board column
        /// </summary>
        /// <param name="columnId">The board column ID to delete</param>
        /// <param name="boardId">The board ID that contains the column</param>
        /// <param name="deletedBy">Optional: User ID who is deleting the column (for audit purposes)</param>
        /// <returns>Deletion result with details about reordered columns</returns>
        /// <response code="200">Board column deleted successfully</response>
        /// <response code="400">If the column ID or board ID is invalid, or column/board doesn't exist</response>
        /// <response code="500">If a server error occurs</response>
        [HttpDelete("column/{columnId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DeleteBoardColumnResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DeleteBoardColumnResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<DeleteBoardColumnResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DeleteBoardColumnResponseDto>>> DeleteBoardColumn(
            [FromRoute] Guid columnId,
            [FromQuery] int boardId,
            [FromQuery] int? deletedBy = null)
        {
            try
            {
                // Validate input
                if (columnId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid column ID provided: {ColumnId}", columnId);
                    return BadRequest(ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                        "Invalid column ID. Column ID cannot be empty."));
                }

                if (boardId <= 0)
                {
                    _logger.LogWarning("Invalid board ID provided: {BoardId}", boardId);
                    return BadRequest(ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                        "Invalid board ID. Board ID must be greater than 0."));
                }

                _logger.LogInformation("API request received to delete board column {ColumnId} from board {BoardId}", 
                    columnId, boardId);

                var command = new DeleteBoardColumnCommand(columnId, boardId, deletedBy);
                var result = await _mediator.Send(command);

                if (result.Status == 200)
                {
                    return Ok(result);
                }

                // If status is 400, it's a business logic failure (board/column doesn't exist)
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in BoardController.DeleteBoardColumn for column: {ColumnId}", 
                    columnId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                        "An unexpected error occurred while deleting the board column. Please contact support if the issue persists."));
            }
        }

        /// <summary>
        /// Delete a board (soft delete - sets IsActive to false)
        /// </summary>
        /// <param name="boardId">The board ID to delete</param>
        /// <param name="deletedBy">Optional: User ID who is deleting the board</param>
        /// <returns>Success status</returns>
        /// <response code="200">Board deleted successfully</response>
        /// <response code="400">If the board doesn't exist or is already deleted</response>
        /// <response code="500">If a server error occurs</response>
        [HttpDelete("{boardId:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBoard(
            [FromRoute] int boardId,
            [FromQuery] int? deletedBy = null)
        {
            try
            {
                if (boardId <= 0)
                {
                    _logger.LogWarning("Invalid board ID provided: {BoardId}", boardId);
                    return BadRequest(ApiResponse<bool>.Fail("Invalid board ID. Board ID must be greater than 0."));
                }

                _logger.LogInformation("API request received to delete board: {BoardId}", boardId);

                var command = new DeleteBoardCommand(boardId, deletedBy);
                var result = await _mediator.Send(command);

                if (result.Status == 200)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in BoardController.DeleteBoard for board: {BoardId}", boardId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<bool>.Fail(
                        "An unexpected error occurred while deleting the board. Please contact support if the issue persists."));
            }
        }
    }
}
