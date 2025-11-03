using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetCompletedIssueCountBySprintQuery : IRequest<ApiResponse<int>>
    {
        public Guid SprintId { get; set; }

        public GetCompletedIssueCountBySprintQuery(Guid sprintId)
        {
            SprintId = sprintId;
        }
    }
}