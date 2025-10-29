using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class EditIssueCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public string IssueType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public int? AssigneeId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public Guid? SprintId { get; set; }
        public int? StoryPoints { get; set; }
        public Guid? EpicId { get; set; }
        public int? ReporterId { get; set; }
        public string AttachmentUrl { get; set; }
        public int? StatusId { get; set; }
    }
}
