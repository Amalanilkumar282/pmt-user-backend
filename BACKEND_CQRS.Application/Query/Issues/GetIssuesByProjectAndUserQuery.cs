using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssuesByProjectAndUserQuery : IRequest<ApiResponse<List<IssueDto>>>
    {
        public Guid ProjectId { get; set; }
        public int UserId { get; set; }

        public GetIssuesByProjectAndUserQuery(Guid projectId, int userId)
        {
            ProjectId = projectId;
            UserId = userId;
        }
    }
}
