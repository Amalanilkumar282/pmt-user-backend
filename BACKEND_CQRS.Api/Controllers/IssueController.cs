using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Query.IssueComments;
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

        [HttpPut("{issueId}/dates")]
        public async Task<ApiResponse<Guid>> UpdateIssueDates([FromRoute] Guid issueId, [FromBody] UpdateIssueDatesCommand command)
        {
            command.IssueId = issueId; // Set the ID from route parameter
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(Guid id)
        {
            var command = new DeleteIssueCommand(id);
            var result = await _mediator.Send(command);

            if (result.Status != 200)
            {
                return NotFound(result);
            }

            return Ok(result);
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
        [HttpGet("project/{projectId}/recent")]
        public async Task<ActionResult<ApiResponse<object>>> GetRecentIssuesByProjectId(Guid projectId, [FromQuery] int count = 6)
        {
            var response = await _mediator.Send(new GetRecentIssuesQuery
            {
                ProjectId = projectId,
                Count = count
            });

            return Ok(response);
        }
        [HttpGet("project/{projectId}/statuses")]
        public async Task<ApiResponse<List<StatusDto>>> GetStatusesByProject([FromRoute] Guid projectId)
        {
            var query = new GetStatusesByProjectQuery(projectId);
            return await _mediator.Send(query);
        }

        #region Issue Comments

        /// <summary>
        /// Create a new comment for an issue
        /// </summary>
        [HttpPost("{issueId}/comments")]
        public async Task<ApiResponse<CreateIssueCommentDto>> CreateComment(
            [FromRoute] Guid issueId,
            [FromBody] CreateIssueCommentCommand command)
        {
            command.IssueId = issueId;
            var result = await _mediator.Send(command);
            return result;
        }

        /// <summary>
        /// Get all comments for a specific issue
        /// </summary>
        [HttpGet("{issueId}/comments")]
        public async Task<ApiResponse<List<IssueCommentDto>>> GetCommentsByIssueId([FromRoute] Guid issueId)
        {
            var query = new GetCommentsByIssueIdQuery(issueId);
            var result = await _mediator.Send(query);
            return result;
        }

        /// <summary>
        /// Get a specific comment by ID
        /// </summary>
        [HttpGet("comments/{commentId}")]
        public async Task<ApiResponse<IssueCommentDto>> GetCommentById([FromRoute] Guid commentId)
        {
            var query = new GetCommentByIdQuery(commentId);
            var result = await _mediator.Send(query);
            return result;
        }

        /// <summary>
        /// Update an existing comment
        /// </summary>
        [HttpPut("comments/{commentId}")]
        public async Task<ApiResponse<Guid>> UpdateComment(
            [FromRoute] Guid commentId,
            [FromBody] UpdateIssueCommentCommand command)
        {
            command.Id = commentId;
            var result = await _mediator.Send(command);
            return result;
        }

        /// <summary>
        /// Delete a comment
        /// </summary>
        [HttpDelete("comments/{commentId}")]
        public async Task<ApiResponse<Guid>> DeleteComment([FromRoute] Guid commentId)
        {
            var command = new DeleteIssueCommentCommand(commentId);
            var result = await _mediator.Send(command);
            return result;
        }


        [HttpGet("project/{projectId}/activity-summary")]
        public async Task<ApiResponse<Dictionary<string, int>>> GetIssueActivitySummaryByProjectId([FromRoute] Guid projectId)
        {
            var query = new GetIssueActivitySummaryByProjectQuery(projectId);
            return await _mediator.Send(query);
        }

        [HttpGet("project/{projectId}/sprint/{sprintId}/activity-summary")]
        public async Task<ApiResponse<Dictionary<string, int>>> GetIssueActivitySummaryBySprintId(
         [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
        {
            var query = new GetIssueActivitySummaryBysprintIdQuery(projectId, sprintId);
            return await _mediator.Send(query);
        }
    }
    #endregion

}

