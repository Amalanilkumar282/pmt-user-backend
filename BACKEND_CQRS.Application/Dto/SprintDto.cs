using System;

namespace BACKEND_CQRS.Application.Dto
{
    public class SprintDto
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public string Name { get; set; }
        public string? SprintGoal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public decimal? StoryPoint { get; set; }
        public int? TeamId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}