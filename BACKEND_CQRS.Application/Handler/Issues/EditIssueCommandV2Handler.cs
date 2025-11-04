using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    /// <summary>
    /// V2 handler that properly handles explicit null values for SprintId,
    /// allowing users to unassign issues from sprints by sending "sprintId": null.
    /// </summary>
    public class EditIssueCommandV2Handler : IRequestHandler<EditIssueCommandV2, ApiResponse<Guid>>
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IMapper _mapper;

        public EditIssueCommandV2Handler(IIssueRepository issueRepository, IMapper mapper)
        {
            _issueRepository = issueRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<Guid>> Handle(EditIssueCommandV2 request, CancellationToken cancellationToken)
        {
            var issues = await _issueRepository.FindAsync(i => i.Id == request.Id);
            var issue = issues.FirstOrDefault();

            if (issue == null)
                return ApiResponse<Guid>.Fail("Issue not found");

            // Update only the fields that are provided
            if (request.ProjectId.HasValue)
                issue.ProjectId = request.ProjectId.Value;

            if (!string.IsNullOrWhiteSpace(request.IssueType))
                issue.Type = request.IssueType;

            if (!string.IsNullOrWhiteSpace(request.Title))
                issue.Title = request.Title;

            if (request.Description != null)
                issue.Description = request.Description;

            if (request.Priority != null)
                issue.Priority = request.Priority;

            if (request.AssigneeId.HasValue)
                issue.AssigneeId = request.AssigneeId;

            if (request.StartDate.HasValue)
                issue.StartDate = request.StartDate;

            if (request.DueDate.HasValue)
                issue.DueDate = request.DueDate;

            // V2 FIX: Check UpdateSprintId flag instead of HasValue
            // This allows explicit null to unassign from sprint
            if (request.UpdateSprintId)
                issue.SprintId = request.SprintId; // Can be null

            if (request.StoryPoints.HasValue)
                issue.StoryPoints = request.StoryPoints;

            if (request.EpicId.HasValue)
                issue.EpicId = request.EpicId;

            if (request.ReporterId.HasValue)
                issue.ReporterId = request.ReporterId.Value;

            if (request.AttachmentUrl != null)
                issue.AttachmentUrl = request.AttachmentUrl;

            if (request.StatusId.HasValue)
                issue.StatusId = request.StatusId;

            if (request.Labels != null)
                issue.Labels = request.Labels;

            issue.UpdatedAt = DateTimeOffset.UtcNow;

            await _issueRepository.UpdateAsync(issue);
            return ApiResponse<Guid>.Success(issue.Id, "Issue updated successfully");
        }
    }
}
