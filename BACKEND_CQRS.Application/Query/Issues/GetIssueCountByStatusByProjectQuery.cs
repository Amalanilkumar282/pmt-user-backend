using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssueCountByStatusByProjectQuery : IRequest<ApiResponse<Dictionary<string, int>>>
    {
        public Guid ProjectId { get; set; }

        public GetIssueCountByStatusByProjectQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}