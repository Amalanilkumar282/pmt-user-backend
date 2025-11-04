using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMediator mediator, ILogger<MessageController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new message in a channel
        /// </summary>
        /// <param name="command">The message creation details</param>
        /// <returns>The created message</returns>
        /// <response code="200">Message created successfully</response>
        /// <response code="400">If validation fails or referenced entities don't exist</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<MessageDto>>> CreateMessage([FromBody] CreateMessageCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    _logger.LogWarning("Invalid model state for CreateMessage: {Errors}", errors);
                    return BadRequest(ApiResponse<MessageDto>.Fail($"Validation failed: {errors}"));
                }

                var result = await _mediator.Send(command);

                // ApiResponse<T> does not expose 'Succeeded'.  Use Status (or Data) to determine success.
                if (result != null && result.Status >= 200 && result.Status < 300)
                {
                    return Ok(result);
                }

                return BadRequest(result ?? ApiResponse<MessageDto>.Fail("Failed to create message."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in MessageController.CreateMessage");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<MessageDto>.Fail("An unexpected error occurred while creating the message."));
            }
        }
    }
}