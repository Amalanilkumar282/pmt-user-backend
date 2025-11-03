using System;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Dto
{
    public class CreateSprintDto
    {
        public Guid Id { get; set; } // Added Id
        public Guid? ProjectId { get; set; } // Optional

        [Required]
        public string SprintName { get; set; } = string.Empty; // Required
        public string? SprintGoal { get; set; } // Optional
        public int? TeamAssigned { get; set; } // Optional
        public DateTime? StartDate { get; set; } // Optional
        public DateTime? DueDate { get; set; } // Optional
        public string? Status { get; set; } = "PLANNED"; // Optional, defaults to PLANNED
        public decimal? StoryPoint { get; set; } // Optional
    }
}
