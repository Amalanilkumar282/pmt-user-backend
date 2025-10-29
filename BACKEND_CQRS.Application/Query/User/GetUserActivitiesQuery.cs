using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.User
{
    public class GetUserActivitiesQuery : IRequest<ApiResponse<List<ActivityLogDto>>>
    {
        public int UserId { get; set; }
        public int Take { get; set; }

        public GetUserActivitiesQuery(int userId, int take = 50)
        {
            UserId = userId;
            Take = take;
        }
    }
}
