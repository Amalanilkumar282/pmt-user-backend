using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Issues
{
    public class GetStatusesByProjectQuery : IRequest<ApiResponse<List<StatusDto>>>
    {
        public Guid ProjectId { get; set; }

        public GetStatusesByProjectQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}