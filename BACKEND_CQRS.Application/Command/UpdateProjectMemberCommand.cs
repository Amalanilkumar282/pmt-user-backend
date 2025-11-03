using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Command
{
    public class UpdateProjectMemberCommand : IRequest<ApiResponse<UpdateProjectMemberResponseDto>>
    {
        public Guid ProjectId { get; set; }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; } // New Role
       
       
    }
}
