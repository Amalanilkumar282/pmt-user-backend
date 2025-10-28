using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Project;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("user/{userId}")]
        public async Task<ApiResponse<List<ProjectDto>>> GetProjectsByUser(int userId)
        {
            var query = new GetUserProjectsQuery(userId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpGet("{projectId}/users")]
        public async Task<ApiResponse<List<UserDto>>> GetUsersByProject(Guid projectId)
        {
            var query = new GetUsersByProjectIdQuery(projectId);
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
