using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Text.Json.Serialization;

namespace BACKEND_CQRS.Application.Command
{
    /// <summary>
    /// V2 version of EditIssueCommand that properly handles explicit null values
    /// for nullable fields like SprintId, allowing users to unassign issues from sprints.
    /// </summary>
    public class EditIssueCommandV2 : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore] // Don't expect this from request body
        public Guid Id { get; set; }

        public Guid? ProjectId { get; set; }
        public string? IssueType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public int? AssigneeId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }

        /// <summary>
        /// SprintId backing field - not directly serialized from JSON.
        /// Use SprintIdJson property for JSON deserialization.
        /// </summary>
        [JsonIgnore]
        public Guid? SprintId { get; set; }

        /// <summary>
        /// Set to true if SprintId should be updated (even if null).
        /// If false, SprintId won't be changed regardless of its value.
        /// </summary>
        [JsonIgnore]
        public bool UpdateSprintId { get; set; }

        public int? StoryPoints { get; set; }
        public Guid? EpicId { get; set; }
        public int? ReporterId { get; set; }
        public string? AttachmentUrl { get; set; }
        public int? StatusId { get; set; }
        public string? Labels { get; set; } // Stores labels as JSON string

        /// <summary>
        /// Called automatically when SprintId is deserialized from JSON.
        /// This ensures we know the field was explicitly provided in the request.
        /// </summary>
        [JsonPropertyName("sprintId")]
        public Guid? SprintIdJson
        {
            get => SprintId;
            set
            {
                SprintId = value;
                UpdateSprintId = true; // Mark that this field was explicitly set
            }
        }
    }
}
