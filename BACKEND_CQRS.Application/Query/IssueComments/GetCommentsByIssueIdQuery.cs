using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.IssueComments
{
    public class GetCommentsByIssueIdQuery : IRequest<ApiResponse<List<IssueCommentDto>>>
    {
        public Guid IssueId { get; set; }

        public GetCommentsByIssueIdQuery(Guid issueId)
        {
            IssueId = issueId;
        }
    }
}
