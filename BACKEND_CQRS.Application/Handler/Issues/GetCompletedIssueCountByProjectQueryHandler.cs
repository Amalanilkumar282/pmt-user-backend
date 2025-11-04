using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class GetCompletedIssueCountByProjectQueryHandler
        : IRequestHandler<GetCompletedIssueCountByProjectQuery, ApiResponse<int>>
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IStatusRepository _statusRepository;

        public GetCompletedIssueCountByProjectQueryHandler(
            IIssueRepository issueRepository,
            IStatusRepository statusRepository)
        {
            _issueRepository = issueRepository;
            _statusRepository = statusRepository;
        }

        public async Task<ApiResponse<int>> Handle(GetCompletedIssueCountByProjectQuery request, CancellationToken cancellationToken)
        {
            // Get the "DONE" status 
            var completedStatus = await _statusRepository.GetStatusByNameAsync("DONE");
           
            if (completedStatus == null)
            {
                return ApiResponse<int>.Fail("DONE status not found in the system.");
            }

            // Fetch issues matching the project and completed status
            var completedIssues = await _issueRepository.FindAsync(i => 
                i.ProjectId == request.ProjectId && 
                i.StatusId == completedStatus.Id);

            var count = completedIssues?.Count ?? 0;

            return ApiResponse<int>.Success(count, $"Found {count} completed issue(s) for the project.");
        }
    }
}