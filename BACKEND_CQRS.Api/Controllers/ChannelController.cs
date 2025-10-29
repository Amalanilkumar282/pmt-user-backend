using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Messages;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChannelController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
