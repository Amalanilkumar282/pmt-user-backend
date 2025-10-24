using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
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
    }
}
