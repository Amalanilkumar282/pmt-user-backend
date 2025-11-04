using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Dto
{
    public class CreateEpicDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int? AssigneeId { get; set; }

        [Required]
        public int ReporterId { get; set; }

        public List<string>? Labels { get; set; }
    }
}
