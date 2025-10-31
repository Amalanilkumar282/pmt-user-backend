using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Boards;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Boards
{
    /// <summary>
    /// Handler for retrieving a single board by ID with all related data
    /// </summary>
    public class GetBoardByIdQueryHandler : IRequestHandler<GetBoardByIdQuery, BoardWithColumnsDto?>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBoardByIdQueryHandler> _logger;

        public GetBoardByIdQueryHandler(
            IBoardRepository boardRepository,
            IMapper mapper,
            ILogger<GetBoardByIdQueryHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BoardWithColumnsDto?> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing query to fetch board: {BoardId}. IncludeInactive: {IncludeInactive}", 
                    request.BoardId, request.IncludeInactive);

                // Fetch board with columns and related data from repository
                var board = await _boardRepository.GetBoardByIdWithColumnsAsync(
                    request.BoardId, 
                    request.IncludeInactive);

                if (board == null)
                {
                    _logger.LogWarning("Board {BoardId} not found or is inactive", request.BoardId);
                    return null;
                }

                // Map entity to DTO
                var boardDto = _mapper.Map<BoardWithColumnsDto>(board);

                _logger.LogInformation(
                    "Successfully fetched board {BoardId} ('{BoardName}') with {ColumnCount} column(s)", 
                    board.Id, board.Name, boardDto.Columns.Count);

                return boardDto;
            }
            catch (InvalidOperationException ex)
            {
                // Database operation failed - log and re-throw
                _logger.LogError(ex, "Database error while fetching board: {BoardId}", request.BoardId);
                throw;
            }
            catch (Exception ex)
            {
                // Unexpected error - log and re-throw
                _logger.LogError(ex, "Unexpected error occurred while processing board query for board: {BoardId}", 
                    request.BoardId);
                throw;
            }
        }
    }
}
