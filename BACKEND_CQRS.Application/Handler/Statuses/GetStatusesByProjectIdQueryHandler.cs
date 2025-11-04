using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Statuses;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Statuses
{
    /// <summary>
    /// Handler to retrieve all statuses used in a specific project
    /// </summary>
    public class GetStatusesByProjectIdQueryHandler 
        : IRequestHandler<GetStatusesByProjectIdQuery, ApiResponse<List<StatusDto>>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStatusesByProjectIdQueryHandler>? _logger;

        public GetStatusesByProjectIdQueryHandler(
            AppDbContext context, 
            IMapper mapper,
            ILogger<GetStatusesByProjectIdQueryHandler>? logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
        }

        public async Task<ApiResponse<List<StatusDto>>> Handle(
            GetStatusesByProjectIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate project ID
                if (request.ProjectId == Guid.Empty)
                {
                    return ApiResponse<List<StatusDto>>.Fail("Invalid project ID. Project ID cannot be empty.");
                }

                _logger?.LogInformation("Fetching statuses for project: {ProjectId}", request.ProjectId);

                // Query to get distinct statuses used in the project's boards
                var statuses = await _context.Boards
                    .Where(b => b.ProjectId == request.ProjectId && b.IsActive)
                    .Join(_context.BoardBoardColumnMaps,
                        board => board.Id,
                        map => map.BoardId,
                        (board, map) => map.BoardColumnId)
                    .Join(_context.BoardColumns,
                        columnId => columnId,
                        column => column.Id,
                        (columnId, column) => column.StatusId)
                    .Where(statusId => statusId.HasValue)
                    .Distinct()
                    .Join(_context.Statuses,
                        statusId => statusId.Value,
                        status => status.Id,
                        (statusId, status) => status)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                if (statuses == null || !statuses.Any())
                {
                    _logger?.LogInformation("No statuses found for project: {ProjectId}", request.ProjectId);
                    return ApiResponse<List<StatusDto>>.Success(
                        new List<StatusDto>(), 
                        "No statuses found for this project.");
                }

                // Map to DTOs
                var statusDtos = _mapper.Map<List<StatusDto>>(statuses);

                _logger?.LogInformation("Found {Count} status(es) for project: {ProjectId}", 
                    statusDtos.Count, request.ProjectId);

                return ApiResponse<List<StatusDto>>.Success(
                    statusDtos, 
                    $"Successfully retrieved {statusDtos.Count} status(es) for the project.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching statuses for project: {ProjectId}", request.ProjectId);
                return ApiResponse<List<StatusDto>>.Fail(
                    "An error occurred while fetching statuses for the project.");
            }
        }
    }
}
