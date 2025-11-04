using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class TeamDetailsDto
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? LeadId { get; set; }  //new
        public LeadDto? Lead { get; set; }
        public List<TeamMemberDto> Members { get; set; } = new();

        public int MemberCount { get; set; }
        public int ActiveSprints { get; set; }
        public int CompletedSprints { get; set; }

        public class LeadDto
        {
            public int? UserId { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
        }

        public class TeamMemberDto
        {
            public int MemberId { get; set; } //new

            public int UserId { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
        }
    }
}
