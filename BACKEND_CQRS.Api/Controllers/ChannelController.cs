using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Messages;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChannelController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ChannelController> _logger;

        public ChannelController(IMediator mediator, ILogger<ChannelController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("team/{teamId:int}")]
        public async Task<ApiResponse<List<ChannelDto>>> GetChannelsByTeamId(int teamId)
        {
            var query = new GetChannelsByTeamIdQuery(teamId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpGet("{channelId:guid}/messages")]
        public async Task<ApiResponse<List<MessageDto>>> GetMessagesByChannelId(
            [FromRoute] Guid channelId,
            [FromQuery] int take = 100)
        {
            var query = new GetMessagesByChannelIdQuery(channelId, take);
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<ApiResponse<ChannelDto>> CreateChannel([FromBody] CreateChannelCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }

        /// <summary>
        /// Delete a channel and all its messages
        /// </summary>
        /// <param name="channelId">The ID of the channel to delete</param>
        /// <param name="deletedBy">Optional: User ID performing the deletion</param>
        /// <returns>Success status</returns>
        /// <response code="200">Channel deleted successfully</response>
        /// <response code="400">If channel doesn't exist or deletion fails</response>
        [HttpDelete("{channelId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteChannel(
            [FromRoute] Guid channelId,
            [FromQuery] int? deletedBy = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    _logger.LogWarning("Invalid model state for DeleteChannel: {Errors}", errors);
                    return BadRequest(ApiResponse<bool>.Fail($"Validation failed: {errors}"));
                }

                _logger.LogInformation("API request to delete channel {ChannelId} by user {DeletedBy}", channelId, deletedBy);

                var command = new DeleteChannelCommand(channelId, deletedBy);
                var result = await _mediator.Send(command);

                if (result != null && result.Status >= 200 && result.Status < 300)
                {
                    return Ok(result);
                }

                return BadRequest(result ?? ApiResponse<bool>.Fail("Failed to delete channel."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ChannelController.DeleteChannel for channel {ChannelId}", channelId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<bool>.Fail("An unexpected error occurred while deleting the channel."));
            }
        }
    }
}
