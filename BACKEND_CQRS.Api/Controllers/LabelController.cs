using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LabelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LabelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ApiResponse<int>> CreateLabel([FromBody] CreateLabelCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpGet]
        public async Task<ApiResponse<List<LabelDto>>> GetAllLabels()
        {
            var result = await _mediator.Send(new GetAllLabelsQuery());
            return result;
        }

        [HttpPut]
        public async Task<ApiResponse<int>> EditLabel([FromBody] EditLabelCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }
    }
}
