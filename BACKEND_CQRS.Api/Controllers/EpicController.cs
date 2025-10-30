using BACKEND_CQRS.Application.Query.Epic;
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

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetEpicsByProjectId(Guid projectId)
        {
            var result = await _mediator.Send(new GetEpicsByProjectIdQuery(projectId));

            if (result == null || !result.Any())
                return NotFound($"No epics found for project {projectId}");

            return Ok(result);
        }
    }
}
