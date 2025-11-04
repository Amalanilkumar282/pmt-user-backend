using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class UpdateProjectMemberResponseDto
    {
        
            public int Id { get; set; }  // Member ID

            public int? RoleId { get; set; }
            public string Role { get; set; }  // New role value

             
        }
    
}
