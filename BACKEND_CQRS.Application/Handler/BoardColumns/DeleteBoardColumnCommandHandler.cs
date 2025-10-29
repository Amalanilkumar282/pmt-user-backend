using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.BoardColumns
{
    /// <summary>
    /// Handler for deleting a board column
    /// </summary>
    public class DeleteBoardColumnCommandHandler : IRequestHandler<DeleteBoardColumnCommand, ApiResponse<DeleteBoardColumnResponseDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly ILogger<DeleteBoardColumnCommandHandler> _logger;

        public DeleteBoardColumnCommandHandler(
            IBoardRepository boardRepository,
            ILogger<DeleteBoardColumnCommandHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<DeleteBoardColumnResponseDto>> Handle(
            DeleteBoardColumnCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing delete request for board column {ColumnId} from board {BoardId}", 
                    request.ColumnId, request.BoardId);

                // Step 1: Validate that the board exists
                var boardExists = await _boardRepository.BoardExistsAsync(request.BoardId);
                if (!boardExists)
                {
                    _logger.LogWarning("Board {BoardId} does not exist or is inactive", request.BoardId);
                    return ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                        $"Board with ID {request.BoardId} does not exist or is inactive");
                }

                // Step 2: Get the board column to delete
                var column = await _boardRepository.GetBoardColumnByIdAsync(request.ColumnId);
                if (column == null)
                {
                    _logger.LogWarning("Board column {ColumnId} does not exist", request.ColumnId);
                    return ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                        $"Board column with ID {request.ColumnId} does not exist");
                }

                var columnName = column.BoardColumnName ?? "Unnamed Column";
                var columnPosition = column.Position ?? 0;

                // Step 3: Get all columns for the board to determine reorder count
                var allColumns = await _boardRepository.GetBoardColumnsAsync(request.BoardId);
                var columnsToReorder = allColumns.Count(c => c.Position > columnPosition);

                _logger.LogInformation("Deleting column '{ColumnName}' at position {Position}. Will reorder {Count} columns", 
                    columnName, columnPosition, columnsToReorder);

                // Step 4: Delete the column (this will also reorder remaining columns)
                var result = await _boardRepository.DeleteBoardColumnAsync(request.ColumnId, request.BoardId);

                if (!result)
                {
                    _logger.LogError("Failed to delete board column {ColumnId}", request.ColumnId);
                    return ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                        $"Failed to delete board column with ID {request.ColumnId}");
                }

                _logger.LogInformation("Successfully deleted board column {ColumnId} from board {BoardId}", 
                    request.ColumnId, request.BoardId);

                // Step 5: Prepare response
                var response = new DeleteBoardColumnResponseDto
                {
                    ColumnId = request.ColumnId,
                    BoardId = request.BoardId,
                    BoardColumnName = columnName,
                    Position = columnPosition,
                    ReorderedColumnsCount = columnsToReorder,
                    WasDeleted = true
                };

                var message = $"Board column '{columnName}' (position {columnPosition}) has been successfully deleted";
                if (columnsToReorder > 0)
                {
                    message += $". {columnsToReorder} column(s) have been automatically reordered";
                }

                return ApiResponse<DeleteBoardColumnResponseDto>.Success(response, message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error while deleting board column {ColumnId}", request.ColumnId);
                return ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                    "A database error occurred while deleting the board column. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting board column {ColumnId}", request.ColumnId);
                return ApiResponse<DeleteBoardColumnResponseDto>.Fail(
                    "An unexpected error occurred while deleting the board column. Please contact support if the issue persists.");
            }
        }
    }
}
