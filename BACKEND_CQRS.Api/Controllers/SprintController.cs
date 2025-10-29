using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Sprints;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/sprints")]
    [ApiController]
    public class SprintController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public SprintController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ApiResponse<CreateSprintDto>> CreateSprint([FromBody] CreateSprintCommand command)
        {
            var newSprint = await _mediator.Send(command);
            return newSprint;
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetSprintsByProjectId(Guid projectId)
        {
            var query = new GetSprintsByProjectIdQuery(projectId);
            var result = await _mediator.Send(query);

            if (result.Status != 200)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
