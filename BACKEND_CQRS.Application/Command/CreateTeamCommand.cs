using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Command
{

    public class CreateTeamCommand : IRequest<int> // returns created team ID
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? LeadId { get; set; }
        public List<int>? MemberIds { get; set; } = new();
        public List<string>? Label { get; set; } = new();
        public int CreatedBy { get; set; }
    }

}
