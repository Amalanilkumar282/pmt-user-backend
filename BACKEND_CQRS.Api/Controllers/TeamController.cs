using BACKEND_CQRS.Application.Command;
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

        //POST: api/team
        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data");

            var command = new CreateTeamCommand
            {
                ProjectId = dto.ProjectId,
                Name = dto.Name,
                Description = dto.Description,
                LeadId = dto.LeadId,
                MemberIds = dto.MemberIds,
                Label = dto.Label,
                CreatedBy = dto.CreatedBy
            };

            var teamId = await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                message = "Team created successfully",
                team_id = teamId
            });
        }
    

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
    [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            var result = await _mediator.Send(new DeleteTeamCommand(teamId));

            if (!result)
                return NotFound(new { Message = "Team not found" });

            return Ok(new { Message = "Team deleted successfully" });
        }

        [HttpGet("count/{projectId}")]
        public async Task<IActionResult> GetTeamCountByProjectId(Guid projectId)
        {
            var count = await _mediator.Send(new GetTeamCountByProjectIdQuery(projectId));
            return Ok(new { ProjectId = projectId, TeamCount = count });
        }

    }
}

