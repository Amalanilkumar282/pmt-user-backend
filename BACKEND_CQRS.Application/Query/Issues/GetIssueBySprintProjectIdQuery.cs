using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssueBySprintProjectIdQuery : IRequest<ApiResponse<List<IssueDto>>>
    {
        public Guid ProjectId { get; set; }
        public Guid? SprintId { get; set; }

        public GetIssueBySprintProjectIdQuery(Guid projectId, Guid? sprintId = null)
        {
            ProjectId = projectId;
            SprintId = sprintId;
        }
    }
}
