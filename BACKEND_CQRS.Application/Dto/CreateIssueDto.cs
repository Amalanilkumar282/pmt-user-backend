using System;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Dto
{
    public class CreateIssueDto
    {
        [Required]
        public Guid ProjectId { get; set; } // <-- Added ProjectId

        [Required]
        public string IssueType { get; set; } // maps to 'Type' in entity

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? Priority { get; set; }

        public int? AssigneeId { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public Guid? SprintId { get; set; }

        public int? StoryPoints { get; set; }

        public Guid? EpicId { get; set; }

        [Required]
        public int ReporterId { get; set; }

        public string? AttachmentUrl { get; set; }

        public int? StatusId { get; set; } // Added StatusId

        public string? Labels { get; set; } // Stores labels as JSON string
    }
}