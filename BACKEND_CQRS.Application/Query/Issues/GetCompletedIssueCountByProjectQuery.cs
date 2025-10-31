using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetCompletedIssueCountByProjectQuery : IRequest<ApiResponse<int>>
    {
        public Guid ProjectId { get; set; }

        public GetCompletedIssueCountByProjectQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}