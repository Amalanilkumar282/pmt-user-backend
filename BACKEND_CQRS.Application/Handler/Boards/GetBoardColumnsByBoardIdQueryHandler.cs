using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Boards;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Boards
{
    public class GetBoardColumnsByBoardIdQueryHandler : IRequestHandler<GetBoardColumnsByBoardIdQuery, List<BoardColumnDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBoardColumnsByBoardIdQueryHandler> _logger;

        public GetBoardColumnsByBoardIdQueryHandler(
            IBoardRepository boardRepository,
            IMapper mapper,
            ILogger<GetBoardColumnsByBoardIdQueryHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<BoardColumnDto>> Handle(GetBoardColumnsByBoardIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing query for board columns in board: {BoardId}", request.BoardId);

                // Validate that the board exists
                var boardExists = await _boardRepository.BoardExistsAsync(request.BoardId);
                if (!boardExists)
                {
                    _logger.LogWarning("Board {BoardId} does not exist or is inactive", request.BoardId);
                    throw new KeyNotFoundException($"Board with ID {request.BoardId} does not exist or is inactive");
                }

                // Fetch board columns from repository (ordered by position)
                var boardColumns = await _boardRepository.GetBoardColumnsAsync(request.BoardId);

                if (boardColumns == null || !boardColumns.Any())
                {
                    _logger.LogInformation("No columns found for board: {BoardId}", request.BoardId);
                    return new List<BoardColumnDto>();
                }

                // Map to DTOs
                var columnDtos = _mapper.Map<List<BoardColumnDto>>(boardColumns);

                _logger.LogInformation("Successfully fetched {Count} column(s) for board: {BoardId}",
                    columnDtos.Count, request.BoardId);

                return columnDtos;
            }
            catch (KeyNotFoundException ex)
            {
                // Board doesn't exist - log and re-throw to be handled by controller
                _logger.LogWarning(ex, "Board {BoardId} not found", request.BoardId);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                // Database operation failed - log and re-throw
                _logger.LogError(ex, "Database error while fetching board columns for board: {BoardId}", request.BoardId);
                throw;
            }
            catch (Exception ex)
            {
                // Unexpected error - log and re-throw
                _logger.LogError(ex, "Unexpected error occurred while processing board columns query for board: {BoardId}",
                    request.BoardId);
                throw;
            }
        }
    }
}
