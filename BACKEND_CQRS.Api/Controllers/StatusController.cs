using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Statuses;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IMediator mediator, ILogger<StatusController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all statuses in the system
        /// </summary>
        /// <returns>List of all statuses</returns>
        /// <response code="200">Returns the list of statuses (may be empty if no statuses exist)</response>
        /// <response code="500">If a server error occurs</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<StatusDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<StatusDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<StatusDto>>>> GetAllStatuses()
        {
            try
            {
                _logger.LogInformation("API request received to fetch all statuses");

                var result = await _mediator.Send(new GetAllStatusesQuery());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in StatusController.GetAllStatuses");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<StatusDto>>.Fail(
                        "An unexpected error occurred while fetching statuses. Please contact support if the issue persists."));
            }
        }
    }
}
