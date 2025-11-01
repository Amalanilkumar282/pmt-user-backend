using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Epic;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BACKEND_CQRS.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EpicController : Controller
    {

        private readonly IMediator _mediator;

        public EpicController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ApiResponse<CreateEpicDto>> CreateEpic([FromBody] CreateEpicCommand command)
        {
            var newEpic = await _mediator.Send(command);
            return newEpic;
        }

        [HttpPut]
        public async Task<ApiResponse<Guid>> UpdateEpic([FromBody] UpdateEpicCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpDelete("{epicId}")]
        public async Task<ApiResponse<Guid>> DeleteEpic(Guid epicId)
        {
            var command = new DeleteEpicByIdCommand(epicId);
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetEpicsByProjectId(Guid projectId)
        {
            var result = await _mediator.Send(new GetEpicsByProjectIdQuery(projectId));

            if (result == null || !result.Any())
                return NotFound($"No epics found for project {projectId}");

            return Ok(result);
        }

        [HttpGet("{epicId}")]
        public async Task<ApiResponse<EpicDto>> GetEpicById(Guid epicId)
        {
            var query = new GetEpicByIdQuery(epicId);
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
