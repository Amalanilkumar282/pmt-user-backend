using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IssueController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IssueController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ApiResponse<CreateIssueDto>> CreateIssue([FromBody] CreateIssueCommand command)
        {
            var newIssue = await _mediator.Send(command);
            return newIssue;
        }

        [HttpPut("{id}")]
        public async Task<ApiResponse<Guid>> EditIssue([FromRoute] Guid id, [FromBody] EditIssueCommand command)
        {
            command.Id = id; // Set the ID from route parameter
            var result = await _mediator.Send(command);
            return result;
        }

        /// <summary>
        /// V2 endpoint that properly handles null values for SprintId.
        /// Use this to unassign issues from sprints by sending "sprintId": null.
        /// </summary>
        [HttpPut("{id}/v2")]
        public async Task<ApiResponse<Guid>> EditIssueV2([FromRoute] Guid id, [FromBody] EditIssueCommandV2 command)
        {
            command.Id = id; // Set the ID from route parameter
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpGet("project/{projectId}/issues")]
        public async Task<ApiResponse<List<IssueDto>>> GetIssuesByProject(
     [FromRoute] Guid projectId)
        {
            var query = new GetIssueBySprintProjectIdQuery(projectId, null);
            var issues = await _mediator.Send(query);
            return issues;
        }

        [HttpGet("project/{projectId}/sprint/{sprintId}/issues")]
        public async Task<ApiResponse<List<IssueDto>>> GetIssuesBySprint(
            [FromRoute] Guid projectId,
            [FromRoute] Guid sprintId)
        {
            var query = new GetIssueBySprintProjectIdQuery(projectId, sprintId);
            var issues = await _mediator.Send(query);
            return issues;
        }

        [HttpGet("project/{projectId}/user/{userId}/issues")]
        public async Task<ApiResponse<List<IssueDto>>> GetIssuesByProjectAndUser(
            [FromRoute] Guid projectId,
            [FromRoute] int userId)
        {
            var query = new GetIssuesByProjectAndUserQuery(projectId, userId);
            var issues = await _mediator.Send(query);
            return issues;
        }

        [HttpGet("project/{projectId}/type-count")]
        public async Task<ApiResponse<Dictionary<string, int>>> GetTypeCountByProject([FromRoute] Guid projectId)
        {
            var query = new GetIssueCountByTypeByProjectSprintQuery(projectId);
            return await _mediator.Send(query);
        }

        [HttpGet("project/{projectId}/sprint/{sprintId}/type-count")]
        public async Task<ApiResponse<Dictionary<string, int>>> GetTypeCountBySprint(
            [FromRoute] Guid projectId,
            [FromRoute] Guid sprintId)
        {
            var query = new GetIssueCountByTypeByProjectSprintQuery(projectId, sprintId);
            return await _mediator.Send(query);
        }

        [HttpGet("project/{projectId}/completed-count")]
        public async Task<ApiResponse<int>> GetCompletedIssueCountByProject([FromRoute] Guid projectId)
        {
            var query = new GetCompletedIssueCountByProjectQuery(projectId);
            return await _mediator.Send(query);
        }

        [HttpGet("sprint/{sprintId}/completed-count")]
        public async Task<ApiResponse<int>> GetCompletedIssueCountBySprint([FromRoute] Guid sprintId)
        {
            var query = new GetCompletedIssueCountBySprintQuery(sprintId);
            return await _mediator.Send(query);
        }

        [HttpGet("sprint/{sprintId}/status-count")]
        public async Task<ApiResponse<Dictionary<string, int>>> GetStatusCountBySprint([FromRoute] Guid sprintId)
        {
            var query = new GetIssueCountByStatusBySprintQuery(sprintId);
            return await _mediator.Send(query);
        }

        [HttpGet("user/{userId}")]
        public async Task<ApiResponse<List<IssueDto>>> GetIssuesByUser([FromRoute] int userId)
        {
            var query = new GetIssuesByUserIdQuery(userId);
            return await _mediator.Send(query);
        }

        [HttpGet("epic/{epicId}")]
        public async Task<ApiResponse<List<IssueDto>>> GetIssuesByEpic([FromRoute] Guid epicId)
        {
            var query = new GetIssuesByEpicIdQuery(epicId);
            return await _mediator.Send(query);
        }

        [HttpGet("project/{projectId}/status-count")]
        public async Task<ApiResponse<Dictionary<string, int>>> GetStatusCountByProject([FromRoute] Guid projectId)
        {
            var query = new GetIssueCountByStatusByProjectQuery(projectId);
            return await _mediator.Send(query);
        }
    }
}
