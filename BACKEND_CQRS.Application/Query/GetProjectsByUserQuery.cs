using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query
{
    public class GetUserProjectsQuery : IRequest<ApiResponse<List<ProjectDto>>>
    {
        public int UserId { get; set; }

        public GetUserProjectsQuery(int userId)
        {
            UserId = userId;
        }
    }
}
