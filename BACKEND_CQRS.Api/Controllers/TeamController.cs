using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 🔹 GET: api/team/project/{projectId}
        [HttpGet("project/{projectId:guid}")]
        public async Task<ActionResult<List<TeamDto>>> GetTeamsByProjectId(Guid projectId)
        {
            var result = await _mediator.Send(new GetTeamsByProjectIdQuery(projectId));
            return Ok(result);
        }

        // 🔹 GET: api/team/user/{userId}
        //[HttpGet("user/{userId:int}")]
        //public async Task<ActionResult<List<TeamDto>>> GetTeamsByUserId(int userId)
        //{
        //    var result = await _mediator.Send(new GetTeamsByUserIdQuery(userId));
        //    return Ok(result);
        //}

        // 🔹 POST: api/team
        //[HttpPost]
        //public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamCommand command)
        //{
        //    var result = await _mediator.Send(command);
        //    return CreatedAtAction(nameof(GetTeamById), new { id = result.Id }, result);
        //}

        // 🔹 GET: api/team/{id}
        //[HttpGet("{id:int}")]
        //public async Task<ActionResult<TeamDto>> GetTeamById(int id)
        //{
        //    var result = await _mediator.Send(new GetTeamByIdQuery(id));
        //    return result is not null ? Ok(result) : NotFound();
        //}

        // 🔹 PUT: api/team/{id}
        //[HttpPut("{id:int}")]
        //public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamCommand command)
        //{
        //    if (id != command.Id)
        //        return BadRequest("Team ID mismatch.");

        //    var result = await _mediator.Send(command);
        //    return result ? NoContent() : NotFound();
        //}

        // 🔹 DELETE: api/team/{id}
        //[HttpDelete("{id:int}")]
        //public async Task<IActionResult> DeleteTeam(int id)
        //{
        //    var result = await _mediator.Send(new DeleteTeamCommand(id));
        //    return result ? NoContent() : NotFound();
        //}
    }
}
