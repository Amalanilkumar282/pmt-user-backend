using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssueCountByStatusBySprintQuery : IRequest<ApiResponse<Dictionary<string, int>>>
    {
        public Guid SprintId { get; set; }

        public GetIssueCountByStatusBySprintQuery(Guid sprintId)
        {
            SprintId = sprintId;
        }
    }
}