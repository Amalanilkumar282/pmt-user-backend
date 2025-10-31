using System;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Dto
{
    public class IssueDto
    {
        public Guid Id { get; set; }

        public string? Key { get; set; }
        [Required]
        public Guid ProjectId { get; set; } // <-- Added ProjectId

        [Required]
        public string IssueType { get; set; } // maps to 'Type' in entity

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public int? AssigneeId { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? DueDate { get; set; }
        public int? StatusId { get; set; }

        public Guid? SprintId { get; set; }
        public Guid? ParentIssueId { get; set; }

        public int? StoryPoints { get; set; }

        public Guid? EpicId { get; set; }

        [Required]
        public int ReporterId { get; set; }
        public string? Labels { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}