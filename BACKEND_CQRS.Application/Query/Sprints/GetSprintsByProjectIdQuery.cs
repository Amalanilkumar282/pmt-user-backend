using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Sprints
{
    public class GetSprintsByProjectIdQuery : IRequest<ApiResponse<List<SprintDto>>>
    {
        public Guid ProjectId { get; set; }

        public GetSprintsByProjectIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}