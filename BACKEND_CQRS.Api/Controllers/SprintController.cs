using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Sprints;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/sprints")]
    [ApiController]
    [Authorize]
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

        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetSprintsByTeamId(int teamId)
        {
            var query = new GetSprintsByTeamIdQuery(teamId);
            var result = await _mediator.Send(query);

            if (result.Status != 200)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSprint(Guid id, [FromBody] UpdateSprintCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<SprintDto>.Fail("Sprint ID mismatch."));
            }

            var result = await _mediator.Send(command);

            if (result.Status != 200)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSprint(Guid id)
        {
            var command = new DeleteSprintCommand(id);
            var result = await _mediator.Send(command);

            if (result.Status != 200)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPatch("{id}/complete")]
        public async Task<ApiResponse<bool>> CompleteSprint([FromRoute] Guid id)
        {
            var command = new CompleteSprintCommand(id);
            var result = await _mediator.Send(command);
            return result;
        }
    }
}
