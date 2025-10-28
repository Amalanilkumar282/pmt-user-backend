using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("issues")]
    public class Issue
    {
        [Key]
        [Column("id")]
        [Required]
        public Guid Id { get; set; }

        [Column("key")]
        public string? Key { get; set; }

        [Column("project_id")]
        [Required]
        public Guid ProjectId { get; set; }

        [Column("epic_id")]
        public Guid? EpicId { get; set; }

        [Column("sprint_id")]
        public Guid? SprintId { get; set; }

        [Column("parent_issue_id")]
        public Guid? ParentIssueId { get; set; }

        [Column("title")]
        [Required]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("type")]
        [Required]
        public string Type { get; set; }

        [Column("priority")]
        public string? Priority { get; set; }

        [Column("status")]
        public int? StatusId { get; set; } // Changed to int? and renamed to StatusId

        [Column("assignee_id")]
        public int? AssigneeId { get; set; }

        [Column("reporter_id")]
        [Required]
        public int ReporterId { get; set; }

        [Column("story_points")]
        public int? StoryPoints { get; set; }

        [Column("labels", TypeName = "jsonb")]
        public string? Labels { get; set; }

        [Column("start_date")]
        public DateTimeOffset? StartDate { get; set; }

        [Column("due_date")]
        public DateTimeOffset? DueDate { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [Column("attachment_url")]
        public string? AttachmentUrl { get; set; }
    }
}
