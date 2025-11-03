using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class GetIssueCountByStatusByProjectQueryHandler 
        : IRequestHandler<GetIssueCountByStatusByProjectQuery, ApiResponse<Dictionary<string, int>>>
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IStatusRepository _statusRepository;

        public GetIssueCountByStatusByProjectQueryHandler(
            IIssueRepository issueRepository,
            IStatusRepository statusRepository)
        {
            _issueRepository = issueRepository;
            _statusRepository = statusRepository;
        }

        public async Task<ApiResponse<Dictionary<string, int>>> Handle(
            GetIssueCountByStatusByProjectQuery request, 
            CancellationToken cancellationToken)
        {
            // Fetch all issues for the project
            var issues = await _issueRepository.FindAsync(i => i.ProjectId == request.ProjectId);

            if (issues == null || !issues.Any())
            {
                return ApiResponse<Dictionary<string, int>>.Success(
                    new Dictionary<string, int>
                    {
                        { "To Do", 0 },
                        { "In Progress", 0 },
                        { "Done", 0 }
                    },
                    "No issues found for the specified project.");
            }

            // Get status names for "To Do" and "Done"
            var toDoStatus = await _statusRepository.GetStatusByNameAsync("To Do");
            var doneStatus = await _statusRepository.GetStatusByNameAsync("Done");

            var toDoCount = 0;
            var inProgressCount = 0;
            var doneCount = 0;

            foreach (var issue in issues)
            {
                if (issue.StatusId == null)
                {
                    // If no status is assigned, consider it as "To Do"
                    toDoCount++;
                }
                else if (toDoStatus != null && issue.StatusId == toDoStatus.Id)
                {
                    toDoCount++;
                }
                else if (doneStatus != null && issue.StatusId == doneStatus.Id)
                {
                    doneCount++;
                }
                else
                {
                    // All other statuses are considered "In Progress"
                    inProgressCount++;
                }
            }

            var result = new Dictionary<string, int>
            {
                { "To Do", toDoCount },
                { "In Progress", inProgressCount },
                { "Done", doneCount }
            };

            return ApiResponse<Dictionary<string, int>>.Success(
                result,
                $"Successfully retrieved issue count by status for project.");
        }
    }
}