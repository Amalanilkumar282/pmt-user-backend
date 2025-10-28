using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Users
{
    public class GetAllUsersQuery :IRequest<ApiResponse<List<UserDto>>>
    {
        public int UserId { get; set; }

   
}
}

