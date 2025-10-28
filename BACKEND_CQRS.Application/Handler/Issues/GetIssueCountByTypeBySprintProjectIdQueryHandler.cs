using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class GetIssueCountByTypeBySprintProjectIdQueryHandler
        : IRequestHandler<GetIssueCountByTypeByProjectSprintQuery, ApiResponse<Dictionary<string, int>>>
    {
        private readonly IIssueRepository _issueRepository;

        public GetIssueCountByTypeBySprintProjectIdQueryHandler(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<ApiResponse<Dictionary<string, int>>> Handle(GetIssueCountByTypeByProjectSprintQuery request, CancellationToken cancellationToken)
        {
            List<Issue> issues;

            if (request.SprintId.HasValue)
            {
                // Fetch issues matching both project and sprint
                issues = await _issueRepository.FindAsync(i => i.ProjectId == request.ProjectId && i.SprintId == request.SprintId);
            }
            else
            {
                // Fetch all issues under the project (no sprint filter)
                issues = await _issueRepository.FindAsync(i => i.ProjectId == request.ProjectId);
            }

            if (issues == null || !issues.Any())
            {
                return ApiResponse<Dictionary<string, int>>.Fail("No issues found for the specified project/sprint.");
            }

            // Group by Type and get counts
            var typeCounts = issues
                .GroupBy(i => i.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            return ApiResponse<Dictionary<string, int>>.Success(typeCounts);
        }
    }
}
