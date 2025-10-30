using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class UpdateTeamDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? LeadId { get; set; }
        public List<int>? MemberIds { get; set; } = new();
        public List<string>? Label { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public int UpdatedBy { get; set; }
    }
}
