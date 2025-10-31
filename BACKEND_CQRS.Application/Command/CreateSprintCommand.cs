using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class CreateSprintCommand : IRequest<ApiResponse<CreateSprintDto>>
    {
        public Guid Id { get; set; } // Added Id
        public Guid? ProjectId { get; set; } // Added ProjectId as optional
        public string SprintName { get; set; }
        public string? SprintGoal { get; set; }
        public int? TeamAssigned { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Planned";
        public decimal? StoryPoint { get; set; }
    }
}
