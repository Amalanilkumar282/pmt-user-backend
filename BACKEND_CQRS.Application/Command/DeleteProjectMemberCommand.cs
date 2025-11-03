using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Command
{
    public class DeleteProjectMemberCommand : IRequest<ApiResponse<string>>
    {
        public Guid ProjectId { get; set; }
        public int UserId { get; set; } // Member to remove
        public int DeletedBy { get; set; } // Admin or Project Manager performing deletion
    }
}
