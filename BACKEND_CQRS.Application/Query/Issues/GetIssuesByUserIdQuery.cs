using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssuesByUserIdQuery : IRequest<ApiResponse<List<IssueDto>>>
    {
        public int UserId { get; set; }

        public GetIssuesByUserIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}
