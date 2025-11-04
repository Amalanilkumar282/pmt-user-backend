using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssueActivitySummaryBysprintIdQuery : IRequest<ApiResponse<Dictionary<string, int>>>
    {
        public Guid ProjectId { get; set; }
        public Guid? SprintId { get; set; }   // Optional — if not provided, calculate for the entire project

        public GetIssueActivitySummaryBysprintIdQuery(Guid projectId, Guid? sprintId = null)
        {
            ProjectId = projectId;
            SprintId = sprintId;
        }
    }
}
