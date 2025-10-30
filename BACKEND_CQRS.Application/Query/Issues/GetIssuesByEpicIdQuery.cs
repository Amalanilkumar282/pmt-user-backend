using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetIssuesByEpicIdQuery : IRequest<ApiResponse<List<IssueDto>>>
    {
        public Guid EpicId { get; set; }

        public GetIssuesByEpicIdQuery(Guid epicId)
        {
            EpicId = epicId;
        }
    }
}
