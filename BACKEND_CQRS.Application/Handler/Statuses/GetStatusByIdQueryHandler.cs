using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Statuses;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Statuses
{
    /// <summary>
    /// Handler for retrieving a single status by ID
    /// </summary>
    public class GetStatusByIdQueryHandler : IRequestHandler<GetStatusByIdQuery, ApiResponse<StatusDto>>
    {
        private readonly IStatusRepository _statusRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStatusByIdQueryHandler> _logger;

        public GetStatusByIdQueryHandler(
            IStatusRepository statusRepository,
            IMapper mapper,
            ILogger<GetStatusByIdQueryHandler> logger)
        {
            _statusRepository = statusRepository ?? throw new ArgumentNullException(nameof(statusRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<StatusDto>> Handle(GetStatusByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing query to fetch status: {StatusId}", request.StatusId);

                // Validate input
                if (request.StatusId <= 0)
                {
                    _logger.LogWarning("Invalid status ID: {StatusId}", request.StatusId);
                    return ApiResponse<StatusDto>.Fail("Invalid status ID. Status ID must be greater than 0.");
                }

                // Fetch status from repository
                var status = await _statusRepository.GetStatusByIdAsync(request.StatusId);

                if (status == null)
                {
                    _logger.LogWarning("Status {StatusId} not found", request.StatusId);
                    return ApiResponse<StatusDto>.Fail($"Status with ID {request.StatusId} does not exist");
                }

                // Map entity to DTO
                var statusDto = _mapper.Map<StatusDto>(status);

                _logger.LogInformation("Successfully fetched status {StatusId} ('{StatusName}')", 
                    status.Id, status.StatusName);

                return ApiResponse<StatusDto>.Success(
                    statusDto, 
                    $"Successfully fetched status '{status.StatusName}'");
            }
            catch (InvalidOperationException ex)
            {
                // Database operation failed
                _logger.LogError(ex, "Database error while fetching status: {StatusId}", request.StatusId);
                return ApiResponse<StatusDto>.Fail(
                    "A database error occurred while fetching the status. Please try again later.");
            }
            catch (Exception ex)
            {
                // Unexpected error
                _logger.LogError(ex, "Unexpected error occurred while processing status query for status: {StatusId}", 
                    request.StatusId);
                return ApiResponse<StatusDto>.Fail(
                    "An unexpected error occurred while fetching the status. Please contact support if the issue persists.");
            }
        }
    }
}
