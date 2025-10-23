using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

namespace BACKEND_CQRS.Domain.Entities
{
    public class Issues
    {
        [Key]
        [Column("id")]
        [Required]
        public Guid Id { get; set; } // uuid, required

        [Column("key")]
        public string Key { get; set; } // text

        [Column("project_id")]
        [Required]
        public Guid ProjectId { get; set; } // uuid, required

        [Column("epic_id")]
        public Guid? EpicId { get; set; } // uuid

        [Column("sprint_id")]
        public Guid? SprintId { get; set; } // uuid

        [Column("parent_issue_id")]
        public Guid? ParentIssueId { get; set; } // uuid

        [Column("title")]
        [Required]
        public string Title { get; set; } // text, required

        [Column("description")]
        public string Description { get; set; } // text

        [Column("type")]
        [Required]
        public string Type { get; set; } // varchar, required

        [Column("priority")]
        public string Priority { get; set; } // varchar

        [Column("status")]
        public string Status { get; set; } // varchar

        [Column("assignee_id")]
        public int? AssigneeId { get; set; } // int4

        [Column("reporter_id")]
        [Required]
        public int ReporterId { get; set; } // int4, required

        [Column("story_points")]
        public int? StoryPoints { get; set; } // int4

        [Column("labels")]
        public JsonNode Labels { get; set; } // jsonb

        [Column("start_date")]
        public DateTimeOffset? StartDate { get; set; } // timestamptz

        [Column("due_date")]
        public DateTimeOffset? DueDate { get; set; } // timestamptz

        [Column("created_by")]
        public int? CreatedBy { get; set; } // int4

        [Column("updated_by")]
        public int? UpdatedBy { get; set; } // int4

        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; set; } // timestamptz

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; } // timestamptz

        [Column("attachment_url")]
        public string AttachmentUrl { get; set; } // text
    }
}