using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Statuses;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Statuses
{
    public class GetAllStatusesQueryHandler : IRequestHandler<GetAllStatusesQuery, ApiResponse<List<StatusDto>>>
    {
        private readonly IStatusRepository _statusRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllStatusesQueryHandler> _logger;

        public GetAllStatusesQueryHandler(
            IStatusRepository statusRepository, 
            IMapper mapper,
            ILogger<GetAllStatusesQueryHandler> logger)
        {
            _statusRepository = statusRepository ?? throw new ArgumentNullException(nameof(statusRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<List<StatusDto>>> Handle(GetAllStatusesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching all statuses");

                var statuses = await _statusRepository.GetAllAsync();

                if (statuses == null || !statuses.Any())
                {
                    _logger.LogInformation("No statuses found in the system");
                    return ApiResponse<List<StatusDto>>.Success(
                        new List<StatusDto>(), 
                        "No statuses found");
                }

                var statusDtos = _mapper.Map<List<StatusDto>>(statuses);

                _logger.LogInformation("Successfully fetched {Count} statuses", statusDtos.Count);

                return ApiResponse<List<StatusDto>>.Success(
                    statusDtos, 
                    $"Successfully fetched {statusDtos.Count} status(es)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all statuses");
                return ApiResponse<List<StatusDto>>.Fail(
                    "An error occurred while fetching statuses. Please try again later.");
            }
        }
    }
}
