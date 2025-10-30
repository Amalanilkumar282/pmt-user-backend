using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class ProjectMemberCountDto
    {
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int UnassignedMembers { get; set; }
    }
}
