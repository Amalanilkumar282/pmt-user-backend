using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssueActivitySummaryByProjectQuery : IRequest<ApiResponse<Dictionary<string, int>>>
    {
        public Guid ProjectId { get; set; }

        public GetIssueActivitySummaryByProjectQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
