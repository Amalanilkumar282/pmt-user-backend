using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler
{
    public class GetBoardsByProjectIdQueryHandler : IRequestHandler<GetBoardsByProjectIdQuery, List<BoardWithColumnsDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBoardsByProjectIdQueryHandler> _logger;

        public GetBoardsByProjectIdQueryHandler(
            IBoardRepository boardRepository, 
            IMapper mapper,
            ILogger<GetBoardsByProjectIdQueryHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<BoardWithColumnsDto>> Handle(GetBoardsByProjectIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing query for boards in project: {ProjectId}", request.ProjectId);

                // Fetch boards with columns from repository
                // This will throw KeyNotFoundException if project doesn't exist
                var boards = await _boardRepository.GetBoardsByProjectIdWithColumnsAsync(request.ProjectId);

                if (boards == null || !boards.Any())
                {
                    _logger.LogInformation("No active boards found for project: {ProjectId}", request.ProjectId);
                    return new List<BoardWithColumnsDto>();
                }

                // Map to DTOs
                var boardDtos = _mapper.Map<List<BoardWithColumnsDto>>(boards);

                _logger.LogInformation("Successfully mapped {Count} board(s) to DTOs for project: {ProjectId}", 
                    boardDtos.Count, request.ProjectId);

                return boardDtos;
            }
            catch (KeyNotFoundException ex)
            {
                // Project doesn't exist - log and re-throw to be handled by controller
                _logger.LogWarning(ex, "Project {ProjectId} not found", request.ProjectId);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                // Database operation failed - log and re-throw
                _logger.LogError(ex, "Database error while fetching boards for project: {ProjectId}", request.ProjectId);
                throw;
            }
            catch (Exception ex)
            {
                // Unexpected error - log and re-throw
                _logger.LogError(ex, "Unexpected error occurred while processing boards query for project: {ProjectId}", 
                    request.ProjectId);
                throw;
            }
        }
    }
}
