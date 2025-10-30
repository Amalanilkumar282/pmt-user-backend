using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.BoardColumns
{
    /// <summary>
    /// Handler for updating board column properties with position management
    /// </summary>
    public class UpdateBoardColumnCommandHandler 
        : IRequestHandler<UpdateBoardColumnCommand, ApiResponse<UpdateBoardColumnResponseDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly ILogger<UpdateBoardColumnCommandHandler> _logger;

        public UpdateBoardColumnCommandHandler(
            IBoardRepository boardRepository,
            IStatusRepository statusRepository,
            ILogger<UpdateBoardColumnCommandHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _statusRepository = statusRepository ?? throw new ArgumentNullException(nameof(statusRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<UpdateBoardColumnResponseDto>> Handle(
            UpdateBoardColumnCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Updating board column {ColumnId} for board {BoardId}",
                    request.ColumnId, request.BoardId);

                // Step 1: Validate that at least one field is being updated
                if (string.IsNullOrWhiteSpace(request.BoardColumnName) &&
                    string.IsNullOrWhiteSpace(request.BoardColor) &&
                    !request.Position.HasValue &&
                    string.IsNullOrWhiteSpace(request.StatusName))
                {
                    _logger.LogWarning("No fields to update for board column {ColumnId}", request.ColumnId);
                    return ApiResponse<UpdateBoardColumnResponseDto>.Fail(
                        "At least one field must be provided for update (BoardColumnName, BoardColor, Position, or StatusName)");
                }

                // Step 2: Validate that the board exists and is active
                var boardExists = await _boardRepository.BoardExistsAsync(request.BoardId);
                if (!boardExists)
                {
                    _logger.LogWarning("Board {BoardId} does not exist or is inactive", request.BoardId);
                    return ApiResponse<UpdateBoardColumnResponseDto>.Fail(
                        $"Board with ID {request.BoardId} does not exist or is inactive");
                }

                // Step 3: Get the existing board column
                var existingColumn = await _boardRepository.GetBoardColumnByIdAsync(request.ColumnId);
                if (existingColumn == null)
                {
                    _logger.LogWarning("Board column {ColumnId} not found", request.ColumnId);
                    return ApiResponse<UpdateBoardColumnResponseDto>.Fail(
                        $"Board column with ID {request.ColumnId} does not exist");
                }

                // Step 4: Verify the column belongs to the specified board
                var boardColumns = await _boardRepository.GetBoardColumnsAsync(request.BoardId);
                if (!boardColumns.Any(c => c.Id == request.ColumnId))
                {
                    _logger.LogWarning(
                        "Board column {ColumnId} does not belong to board {BoardId}",
                        request.ColumnId, request.BoardId);
                    return ApiResponse<UpdateBoardColumnResponseDto>.Fail(
                        $"Board column with ID {request.ColumnId} does not belong to board {request.BoardId}");
                }

                var previousPosition = existingColumn.Position ?? 0;
                var updatedFields = new List<string>();
                int shiftedColumnsCount = 0;

                // Step 5: Handle position change if requested
                if (request.Position.HasValue && request.Position.Value != previousPosition)
                {
                    var maxPosition = boardColumns.Count;

                    // Validate new position
                    if (request.Position.Value < 1 || request.Position.Value > maxPosition)
                    {
                        _logger.LogWarning(
                            "Invalid position {Position} for board column {ColumnId}. Valid range is 1-{MaxPosition}",
                            request.Position.Value, request.ColumnId, maxPosition);
                        return ApiResponse<UpdateBoardColumnResponseDto>.Fail(
                            $"Invalid position {request.Position.Value}. Must be between 1 and {maxPosition}");
                    }

                    _logger.LogInformation(
                        "Moving board column {ColumnId} from position {OldPosition} to {NewPosition}",
                        request.ColumnId, previousPosition, request.Position.Value);

                    // Shift other columns to make space
                    await _boardRepository.ShiftColumnPositionsForMoveAsync(
                        request.BoardId,
                        previousPosition,
                        request.Position.Value);

                    // Calculate how many columns were affected
                    if (previousPosition < request.Position.Value)
                    {
                        shiftedColumnsCount = boardColumns.Count(c => 
                            c.Position > previousPosition && c.Position <= request.Position.Value);
                    }
                    else
                    {
                        shiftedColumnsCount = boardColumns.Count(c => 
                            c.Position >= request.Position.Value && c.Position < previousPosition);
                    }

                    updatedFields.Add($"Position (from {previousPosition} to {request.Position.Value})");
                }

                // Step 6: Handle status change if requested
                Status? newStatus = null;
                bool isNewStatus = false;

                if (!string.IsNullOrWhiteSpace(request.StatusName))
                {
                    var existingStatus = await _statusRepository.GetStatusByNameAsync(request.StatusName);

                    if (existingStatus != null)
                    {
                        // Reuse existing status
                        newStatus = existingStatus;
                        isNewStatus = false;
                        _logger.LogInformation(
                            "Reusing existing status '{StatusName}' with ID {StatusId}",
                            newStatus.StatusName, newStatus.Id);
                    }
                    else
                    {
                        // Create new status
                        newStatus = await _statusRepository.CreateStatusAsync(request.StatusName);
                        isNewStatus = true;
                        _logger.LogInformation(
                            "Created new status '{StatusName}' with ID {StatusId}",
                            newStatus.StatusName, newStatus.Id);
                    }

                    updatedFields.Add($"Status (to '{newStatus.StatusName}')");
                }

                // Step 7: Build the update object
                var updatedColumn = new BoardColumn
                {
                    Id = request.ColumnId,
                    BoardColumnName = request.BoardColumnName,
                    BoardColor = request.BoardColor,
                    Position = request.Position,
                    StatusId = newStatus?.Id
                };

                // Track other field updates
                if (!string.IsNullOrWhiteSpace(request.BoardColumnName) && 
                    request.BoardColumnName != existingColumn.BoardColumnName)
                {
                    updatedFields.Add($"Name (to '{request.BoardColumnName}')");
                }

                if (!string.IsNullOrWhiteSpace(request.BoardColor) && 
                    request.BoardColor.ToUpper() != existingColumn.BoardColor?.ToUpper())
                {
                    updatedFields.Add($"Color (to '{request.BoardColor}')");
                }

                // Step 8: Update the column
                var result = await _boardRepository.UpdateBoardColumnAsync(request.ColumnId, updatedColumn);

                _logger.LogInformation(
                    "Successfully updated board column {ColumnId}. Updated fields: {UpdatedFields}",
                    request.ColumnId, string.Join(", ", updatedFields));

                // Step 9: Prepare response
                var response = new UpdateBoardColumnResponseDto
                {
                    ColumnId = result.Id,
                    BoardId = request.BoardId,
                    BoardColumnName = result.BoardColumnName ?? string.Empty,
                    BoardColor = result.BoardColor ?? string.Empty,
                    Position = result.Position ?? 0,
                    StatusId = result.StatusId ?? 0,
                    StatusName = result.Status?.StatusName ?? string.Empty,
                    IsNewStatus = isNewStatus,
                    PreviousPosition = previousPosition != result.Position ? previousPosition : null,
                    ShiftedColumnsCount = shiftedColumnsCount,
                    UpdatedFields = updatedFields
                };

                var message = $"Board column '{result.BoardColumnName}' updated successfully";
                if (updatedFields.Count > 0)
                {
                    message += $". Updated: {string.Join(", ", updatedFields)}";
                }
                if (shiftedColumnsCount > 0)
                {
                    message += $". {shiftedColumnsCount} column(s) repositioned";
                }

                return ApiResponse<UpdateBoardColumnResponseDto>.Success(response, message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while updating board column {ColumnId}", request.ColumnId);
                return ApiResponse<UpdateBoardColumnResponseDto>.Fail(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error while updating board column {ColumnId}", request.ColumnId);
                return ApiResponse<UpdateBoardColumnResponseDto>.Fail(
                    "A database error occurred while updating the board column. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating board column {ColumnId}", request.ColumnId);
                return ApiResponse<UpdateBoardColumnResponseDto>.Fail(
                    "An unexpected error occurred while updating the board column. Please contact support if the issue persists.");
            }
        }
    }
}
