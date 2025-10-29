using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Boards
{
    /// <summary>
    /// Handler for deleting a board (soft delete)
    /// </summary>
    public class DeleteBoardCommandHandler : IRequestHandler<DeleteBoardCommand, ApiResponse<bool>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly ILogger<DeleteBoardCommandHandler> _logger;

        public DeleteBoardCommandHandler(
            IBoardRepository boardRepository,
            ILogger<DeleteBoardCommandHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<bool>> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing delete request for board {BoardId}", request.BoardId);

                // Step 1: Validate that the board exists
                var board = await _boardRepository.GetBoardByIdAsync(request.BoardId);
                
                if (board == null)
                {
                    _logger.LogWarning("Board {BoardId} does not exist", request.BoardId);
                    return ApiResponse<bool>.Fail($"Board with ID {request.BoardId} does not exist");
                }

                // Step 2: Check if board is already inactive
                if (!board.IsActive)
                {
                    _logger.LogWarning("Board {BoardId} is already deleted/inactive", request.BoardId);
                    return ApiResponse<bool>.Fail($"Board with ID {request.BoardId} is already deleted");
                }

                // Step 3: Perform soft delete
                var result = await _boardRepository.SoftDeleteBoardAsync(request.BoardId, request.DeletedBy);

                if (!result)
                {
                    _logger.LogError("Failed to delete board {BoardId}", request.BoardId);
                    return ApiResponse<bool>.Fail($"Failed to delete board with ID {request.BoardId}");
                }

                _logger.LogInformation("Successfully deleted board {BoardId}", request.BoardId);

                return ApiResponse<bool>.Success(
                    true,
                    $"Board '{board.Name}' (ID: {request.BoardId}) has been successfully deleted"
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error while deleting board {BoardId}", request.BoardId);
                return ApiResponse<bool>.Fail(
                    "A database error occurred while deleting the board. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting board {BoardId}", request.BoardId);
                return ApiResponse<bool>.Fail(
                    "An unexpected error occurred while deleting the board. Please contact support if the issue persists.");
            }
        }
    }
}
