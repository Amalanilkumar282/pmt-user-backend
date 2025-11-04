using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class UpdateIssueDatesCommandHandler : IRequestHandler<UpdateIssueDatesCommand, ApiResponse<Guid>>
    {
        private readonly IIssueRepository _issueRepository;

        public UpdateIssueDatesCommandHandler(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateIssueDatesCommand request, CancellationToken cancellationToken)
        {
            var issues = await _issueRepository.FindAsync(i => i.Id == request.IssueId);
            var issue = issues.FirstOrDefault();

            if (issue == null)
                return ApiResponse<Guid>.Fail("Issue not found");

            // Update dates
            if (request.StartDate.HasValue)
                issue.StartDate = request.StartDate;

            if (request.DueDate.HasValue)
                issue.DueDate = request.DueDate;

            // Validate that StartDate is before DueDate if both are provided
            if (issue.StartDate.HasValue && issue.DueDate.HasValue && issue.StartDate > issue.DueDate)
                return ApiResponse<Guid>.Fail("Start date cannot be after due date");

            issue.UpdatedAt = DateTimeOffset.UtcNow;

            await _issueRepository.UpdateAsync(issue);
            return ApiResponse<Guid>.Success(issue.Id, "Issue dates updated successfully");
        }
    }
}