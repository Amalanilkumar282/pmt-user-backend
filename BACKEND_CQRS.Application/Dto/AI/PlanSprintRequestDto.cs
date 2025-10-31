using System;

namespace BACKEND_CQRS.Application.Dto.AI
{
    public class PlanSprintRequestDto
    {
        public string SprintName { get; set; } = string.Empty;
        public string? SprintGoal { get; set; }
        public int TeamId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TargetStoryPoints { get; set; }
    }
}
