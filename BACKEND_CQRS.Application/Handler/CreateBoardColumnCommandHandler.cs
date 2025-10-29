using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler
{
    public class CreateBoardColumnCommandHandler : IRequestHandler<CreateBoardColumnCommand, ApiResponse<CreateBoardColumnResponseDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly ILogger<CreateBoardColumnCommandHandler> _logger;

        public CreateBoardColumnCommandHandler(
            IBoardRepository boardRepository,
            IStatusRepository statusRepository,
            ILogger<CreateBoardColumnCommandHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _statusRepository = statusRepository ?? throw new ArgumentNullException(nameof(statusRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<CreateBoardColumnResponseDto>> Handle(
            CreateBoardColumnCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating board column for board {BoardId} with name '{ColumnName}' at position {Position}",
                    request.BoardId, request.BoardColumnName, request.Position);

                // Step 1: Validate that the board exists
                var boardExists = await _boardRepository.BoardExistsAsync(request.BoardId);
                if (!boardExists)
                {
                    _logger.LogWarning("Board {BoardId} does not exist or is inactive", request.BoardId);
                    return ApiResponse<CreateBoardColumnResponseDto>.Fail(
                        $"Board with ID {request.BoardId} does not exist or is inactive");
                }

                // Step 2: Get current columns to validate position
                var existingColumns = await _boardRepository.GetBoardColumnsAsync(request.BoardId);
                var maxPosition = existingColumns.Any() 
                    ? existingColumns.Max(c => c.Position ?? 0) 
                    : 0;

                // Validate that position is not greater than maxPosition + 1
                if (request.Position > maxPosition + 1)
                {
                    _logger.LogWarning("Invalid position {Position} for board {BoardId}. Max position is {MaxPosition}",
                        request.Position, request.BoardId, maxPosition);
                    return ApiResponse<CreateBoardColumnResponseDto>.Fail(
                        $"Invalid position {request.Position}. Must be between 1 and {maxPosition + 1}");
                }

                // Step 3: Check if status exists or create new one
                Status status;
                bool isNewStatus;

                var existingStatus = await _statusRepository.GetStatusByNameAsync(request.StatusName);
                
                if (existingStatus != null)
                {
                    // Reuse existing status
                    status = existingStatus;
                    isNewStatus = false;
                    _logger.LogInformation("Reusing existing status '{StatusName}' with ID {StatusId}",
                        status.StatusName, status.Id);
                }
                else
                {
                    // Create new status
                    status = await _statusRepository.CreateStatusAsync(request.StatusName);
                    isNewStatus = true;
                    _logger.LogInformation("Created new status '{StatusName}' with ID {StatusId}",
                        status.StatusName, status.Id);
                }

                // Step 4: Shift positions if necessary (before creating new column)
                int shiftedColumnsCount = 0;
                if (request.Position <= maxPosition)
                {
                    await _boardRepository.ShiftColumnPositionsAsync(request.BoardId, request.Position);
                    shiftedColumnsCount = existingColumns.Count(c => (c.Position ?? 0) >= request.Position);
                    _logger.LogInformation("Shifted {Count} columns to make space at position {Position}",
                        shiftedColumnsCount, request.Position);
                }

                // Step 5: Create the new board column
                var boardColumn = new BoardColumn
                {
                    Id = Guid.NewGuid(),
                    BoardColumnName = request.BoardColumnName.Trim(),
                    BoardColor = request.BoardColor.ToUpper(), // Normalize hex color
                    StatusId = status.Id,
                    Position = request.Position
                };

                var createdColumn = await _boardRepository.CreateBoardColumnAsync(request.BoardId, boardColumn);

                _logger.LogInformation("Successfully created board column {ColumnId} at position {Position} for board {BoardId}",
                    createdColumn.Id, createdColumn.Position, request.BoardId);

                // Step 6: Prepare response
                var response = new CreateBoardColumnResponseDto
                {
                    ColumnId = createdColumn.Id,
                    BoardId = request.BoardId,
                    BoardColumnName = createdColumn.BoardColumnName ?? string.Empty,
                    BoardColor = createdColumn.BoardColor ?? string.Empty,
                    Position = createdColumn.Position ?? 0,
                    StatusId = status.Id,
                    StatusName = status.StatusName,
                    IsNewStatus = isNewStatus,
                    ShiftedColumnsCount = shiftedColumnsCount
                };

                var message = isNewStatus
                    ? $"Board column '{request.BoardColumnName}' created successfully with new status '{status.StatusName}'"
                    : $"Board column '{request.BoardColumnName}' created successfully with existing status '{status.StatusName}'";

                if (shiftedColumnsCount > 0)
                {
                    message += $". {shiftedColumnsCount} column(s) shifted to accommodate new position";
                }

                return ApiResponse<CreateBoardColumnResponseDto>.Created(response, message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Board not found: {BoardId}", request.BoardId);
                return ApiResponse<CreateBoardColumnResponseDto>.Fail($"Board with ID {request.BoardId} not found");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error while creating board column for board {BoardId}", request.BoardId);
                return ApiResponse<CreateBoardColumnResponseDto>.Fail(
                    "A database error occurred while creating the board column. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating board column for board {BoardId}", request.BoardId);
                return ApiResponse<CreateBoardColumnResponseDto>.Fail(
                    "An unexpected error occurred while creating the board column. Please contact support if the issue persists.");
            }
        }
    }
}
