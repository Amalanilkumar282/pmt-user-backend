using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query
{
    public class GetRecentProjectsQuery : IRequest<ApiResponse<List<ProjectDto>>>
    {
        public int UserId { get; }
        public int Take { get; }

        public GetRecentProjectsQuery(int userId, int take = 10)
        {
            UserId = userId;
            Take = take;
        }
    }
}
