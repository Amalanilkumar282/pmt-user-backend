using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class UpdateSprintCommand : IRequest<ApiResponse<SprintDto>>
    {
        public Guid Id { get; set; } // Sprint ID to update
        public string SprintName { get; set; }
        public string? SprintGoal { get; set; }
        public int? TeamAssigned { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public decimal? StoryPoint { get; set; }
        public Guid? ProjectId { get; set; }
    }
}