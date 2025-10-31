using BACKEND_CQRS.Domain.Dto.AI;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class PlanSprintWithAICommand : IRequest<ApiResponse<GeminiSprintPlanResponseDto>>
    {
        public Guid ProjectId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public string? SprintGoal { get; set; }
        public int TeamId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TargetStoryPoints { get; set; }
        public int UserId { get; set; }
    }
}
