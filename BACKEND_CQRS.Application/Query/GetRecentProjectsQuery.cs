using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query
{
    public class GetRecentProjectsQuery : IRequest<ApiResponse<List<ProjectDto>>>
    {
        public int Take { get; }

        public GetRecentProjectsQuery(int take = 10)
        {
            Take = take;
        }
    }
}
