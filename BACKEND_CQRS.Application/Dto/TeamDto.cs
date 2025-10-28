using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class TeamDto
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty; // optional
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? LeadId { get; set; }
        public string? LeadName { get; set; } // optional
        public bool? IsActive { get; set; }
        public List<string>? Label { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int MemberCount { get; set; }

        public int ActiveSprintCount { get; set; }

    }
}
